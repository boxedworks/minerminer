using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;

namespace Controllers
{

  //
  public class RockController : IHasInfoables
  {

    //
    public static RockController s_Singleton;

    //
    public static float s_GemPercent = 0.5f;

    //
    bool _hasRock;
    public static bool s_HasRock { get { return s_Singleton._hasRock; } }
    Button _addRockButton, _toggleMineButton, _stopMineButton;

    Transform _tutorialArrow;

    //
    Transform _menu, _menuUi, _dependencies, _rockModel, _rockMenu;
    public static Transform transform { get { return s_Singleton._rockModel; } }
    Vector3 _rockPosition;
    float _vibrateAmount;

    Slider _healthSlider;
    float _health { get { return _rocks[_currentRock]._Health; } set { _rocks[_currentRock]._Health = value; } }

    bool _autoReplaceRock;

    Dictionary<RockType, RockInfo> _rocks;
    RockType _currentRock;
    public enum RockType
    {
      NONE,

      STONE,
      COPPER,
      TIN,
      IRON,
      COAL,
    }
    public class RockInfo : IInfoable
    {
      public RockType _RockType;
      public string _Title;

      public float _Health, _HealthMax, _XpGain;
      public (InventoryController.ItemType ItemType, float DropChance)[] _DropTable;

      public GameObject _MenuEntry;

      //
      public string _Info
      {
        get
        {

          var breakProductString = "";
          foreach (var drop in _DropTable)
          {
            breakProductString += string.Format("- {0,-10} - {1,4}%\n", InventoryController.GetItemName(drop.Item1), drop.Item2);
          }

          //
          var desc = "";
          if (breakProductString.Length > 0)
          {
            desc += $"<b>Drop table</b>\n\n{breakProductString}";
          }

          //
          var healthText = s_Singleton._rockMenu.gameObject.activeSelf ? $"Health: {_HealthMax}" : $"Health: {s_Singleton._health} / {_HealthMax}";

          return InfoController.GetInfoString($"{_Title}", @$"{healthText}
Xp:     {_XpGain}

{desc}");
        }
      }
      public RectTransform _Transform { get { return _MenuEntry.transform as RectTransform; } }
    }

    //
    List<IInfoable> _otherInfoables;
    IInfoable _rockInfo, _autoRockInfo;
    public InfoData _InfoData
    {
      get
      {
        var returnData = new InfoData();

        var returnList = new List<IInfoable>();

        // Rock menu visible
        if (_rockMenu.gameObject.activeSelf)
        {
          foreach (var rockInfo in _rocks)
            if (rockInfo.Value._MenuEntry != null)
              returnList.Add(rockInfo.Value);
        }

        // Rock menu hidden
        else
        {
          foreach (var otherInfo in _otherInfoables)
            returnList.Add(otherInfo);
          returnList.Add(_autoRockInfo);

          //
          returnList.Add(PickaxeController.s_PickaxeStats);
          if (_rockInfo != null)
          {
            returnList.Add(_rockInfo);
          }
        }

        //
        returnData._Infos = returnList;

        return returnData;
      }
    }

    //
    public RockController()
    {
      s_Singleton = this;

      //
      _menu = MineBoxMenuController.s_Singleton.GetMenu(MineBoxMenuController.MenuType.MINE).transform;
      _rockModel = _menu.Find("Rock").transform;
      _rockPosition = _rockModel.localPosition;

      _menuUi = GameObject.Find("BoxMineMenu").transform;
      _dependencies = _menuUi.GetChild(0).GetChild(0);

      _healthSlider = _dependencies.Find("RockHealth").GetComponent<Slider>();
      _rockMenu = _dependencies.Find("RockMenu");
      _rockMenu.gameObject.SetActive(false);

      //
      for (var i = 1; i < 10; i++)
        SetRockSelection(i, null);

      _rocks = new();
      UnlockRock(RockType.STONE);

      //
      _otherInfoables = new();

      _addRockButton = GameObject.Find("AddRockButton").GetComponent<Button>();
      _addRockButton.onClick.AddListener(() =>
      {
        //ToggleRock(true);
        if (!_rocks.ContainsKey(RockType.COPPER))
          GetRockMenuButton(0).onClick?.Invoke();
        else
          _rockMenu.gameObject.SetActive(true);

        if (_tutorialArrow != null)
          GameObject.Destroy(_tutorialArrow.gameObject);

        AudioController.PlayAudio("MenuSelect");
      });
      _otherInfoables.Add(new SimpleInfoable()
      {
        _GameObject = _addRockButton.gameObject,
        _Description = InfoController.GetInfoString("Add Rock", "Place a rock for you to mine.")
      });

      //
      _toggleMineButton = GameObject.Find("PlayMineButton").GetComponent<Button>();
      _toggleMineButton.onClick.AddListener(() =>
      {
        _autoReplaceRock = !_autoReplaceRock;
        var toggle = _autoReplaceRock;
        _toggleMineButton.transform.GetChild(0).gameObject.SetActive(!toggle);
        _toggleMineButton.transform.GetChild(1).gameObject.SetActive(toggle);

        var toggleString = toggle ? "On" : "Off";
        _autoRockInfo = new SimpleInfoable()
        {
          _GameObject = _toggleMineButton.gameObject,
          _Description = InfoController.GetInfoString("Auto replace rock", @$"Click to toggle.

Mode: {toggleString}")
        };

        AudioController.PlayAudio("MenuSelect");
      });
      _autoRockInfo = new SimpleInfoable()
      {
        _GameObject = _toggleMineButton.gameObject,
        _Description = InfoController.GetInfoString("Auto replace rock", @$"Click to toggle.

Mode: Off")
      };
      ToggleAutoRock(false);

      //
      _stopMineButton = GameObject.Find("StopMineButton").GetComponent<Button>();
      _stopMineButton.onClick.AddListener(() =>
      {
        ToggleRock(false);

        AudioController.PlayAudio("MenuSelect");
      });
      _otherInfoables.Add(new SimpleInfoable()
      {
        _GameObject = _stopMineButton.gameObject,
        _Description = InfoController.GetInfoString("Clear rock", "Remove current rock to select another.")
      });

      //
      SetHealth(0f);

      //
      SetRockType(RockType.STONE);
      ToggleRock(false);


      //
      _tutorialArrow = _dependencies.Find("TutArrow");
    }

    //
    public void Update()
    {

      _vibrateAmount -= Time.deltaTime;
      if (_vibrateAmount > 0f)
      {
        _rockModel.localPosition = _rockPosition + Random.onUnitSphere * _vibrateAmount;
      }
      else
        _rockModel.localPosition = _rockPosition;

      if (_tutorialArrow != null)
      {
        var pos = _tutorialArrow.localPosition;
        pos.y = Mathf.Lerp(400f, 450f, (Mathf.Sin(Time.time * 4f) + 1f) / 2f);
        _tutorialArrow.localPosition = pos;
      }
    }

    //
    public void Hit(float damage)
    {

      // Vibrate rock
      var amount = 0.15f;
      _vibrateAmount = Mathf.Clamp(_vibrateAmount + amount, amount, 1f);

      // Take damage
      SetHealth(_health - damage);
      DamageTextController.s_Singleton.ReportDamage(damage);

      //
      var baseDropAmount = 0;
      var luckModifier = SkillController.GetMaths(SkillController.SkillType.LUCK);
      while (luckModifier > 0f)
      {
        var randomNumber = Random.Range(0f, 1f);
        if (randomNumber <= luckModifier)
        {
          baseDropAmount++;

          LogController.AppendLog("Luck!");
        }

        luckModifier -= 1f;
      }

      // Break
      if (_health <= 0f)
      {
        SkillController.s_Singleton.AddXp(_rocks[_currentRock]._XpGain);

        DropFromDropTable(baseDropAmount + PickaxeController.s_PickaxeStats.AmountDroppedOnBreak);

        ToggleRock(false);
        if (_autoReplaceRock)
        {
          _addRockButton.gameObject.SetActive(false);
          ToggleRock(true);
        }

        // Rock Fx
        ParticleController.PlayParticles(ParticleController.ParticleType.ROCK_DESTROY);
        if (MineBoxMenuController.s_Singleton.IsVisible(MineBoxMenuController.MenuType.MINE))
          AudioController.PlayAudio("RockBreak");
      }

      // Normal damage
      else
      {
        DropFromDropTable(baseDropAmount + PickaxeController.s_PickaxeStats.AmountDroppedOnHit);
      }

      // Pickaxe Fx
      ParticleController.PlayParticles(ParticleController.ParticleType.ROCK_HIT);
      if (MineBoxMenuController.s_Singleton.IsVisible(MineBoxMenuController.MenuType.MINE))
        AudioController.PlayAudio("PickaxeHit");
    }

    //
    void DropFromDropTable(int amount)
    {

      Dictionary<InventoryController.ItemType, int> itemDrops = new();

      for (var i = 0; i < amount; i++)
      {
        var drop = GetRockDrop(_currentRock);
        if (!itemDrops.ContainsKey(drop))
          itemDrops.Add(drop, 1);
        else
          itemDrops[drop]++;
      }

      var logString = "Collected ";
      var logIter = 0;
      foreach (var drop in itemDrops)
      {
        if (logIter++ != 0)
          logString += ", ";

        AddItemAmount(drop.Key, drop.Value);
        logString += $"{drop.Value} {InventoryController.GetItemName(drop.Key)}";
      }
      logString += ".";
      LogController.AppendLog(logString);
    }

    //
    void SetRockType(RockType rockType)
    {
      _currentRock = rockType;
      _rockModel.GetComponent<SpriteRenderer>().sprite = GetRockSprite(rockType);

      var rockInfo = _rocks[_currentRock];
      SetHealth(rockInfo._HealthMax);
    }

    //
    void SetRockHoverInfos()
    {
      if (_currentRock == RockType.NONE)
      {
        _rockInfo = null;
        return;
      }

      var rockInfo = _rocks[_currentRock];
      _rockInfo = new SimpleInfoable()
      {
        _GameObject = GameObject.Find("RockBox"),
        _Description = rockInfo._Info
      };
    }

    //
    void AddItemAmount(InventoryController.ItemType ofItem, int amount)
    {
      InventoryController.s_Singleton.AddItemAmount(ofItem, amount);

      //
      var particleType = InventoryController.GetItemParticles(ofItem);
      ParticleController.PlayParticles(particleType, amount);
    }

    //
    void SetHealth(float health)
    {
      if (_currentRock != RockType.NONE)
        _health = health;
      if (health == 0f)
        _healthSlider.value = 0f;
      else
        _healthSlider.value = _health / _rocks[_currentRock]._HealthMax;

      SetRockHoverInfos();
    }

    //
    void ToggleRock(bool toggle)
    {
      _rockModel.gameObject.SetActive(toggle);
      _addRockButton.gameObject.SetActive(!toggle);
      _stopMineButton.gameObject.SetActive(toggle);
      _hasRock = toggle;

      if (toggle)
      {
        SetRockType(_currentRock);
      }
      else
      {
        SetHealth(0f);

        _rockInfo = null;
      }
    }

    //
    InventoryController.ItemType GetRockDrop(RockType rockType)
    {
      var rockInfo = _rocks[rockType];
      var dropTable = rockInfo._DropTable;

      var randomPercent = Random.Range(0, 100f);
      var percentAdd = 0f;
      foreach (var drop in dropTable)
      {
        var percentThreshhold = drop.Item2;
        percentAdd += percentThreshhold;
        if (randomPercent <= percentAdd)
          return drop.Item1;

      }

      return dropTable[^1].Item1;
    }

    //
    Button GetRockMenuButton(int index)
    {
      return (index < 5 ? _rockMenu.GetChild(0).GetChild(index) : _rockMenu.GetChild(1).GetChild(index - 5))
        .GetChild(0).GetComponent<Button>();
    }
    void SetRockSelection(int buttonIndex, RockInfo rockInfo)
    {

      var button = GetRockMenuButton(buttonIndex);
      var img = button.transform.GetChild(1).GetComponent<Image>();

      button.onClick.RemoveAllListeners();

      if (rockInfo == null)
      {
        img.enabled = false;

        button.onClick.AddListener(() =>
        {
          _currentRock = RockType.NONE;

          _rockMenu.gameObject.SetActive(false);
          _addRockButton.gameObject.SetActive(true);

          ToggleRock(false);

          AudioController.PlayAudio("MenuSelect");
        });
      }
      else
      {
        img.sprite = GetRockSprite(rockInfo._RockType);
        img.enabled = true;

        button.onClick.AddListener(() =>
        {
          _currentRock = rockInfo._RockType;

          _rockMenu.gameObject.SetActive(false);

          ToggleRock(true);

          AudioController.PlayAudio("MenuSelect");
        });

        _rocks[rockInfo._RockType]._MenuEntry = button.transform.parent.gameObject;
      }
    }

    //
    public static Sprite GetRockSprite(RockType rockType)
    {
      return Resources.Load<ItemResourceInfo>($"Rocks/{rockType.ToString().ToLower()}").Sprite;
    }

    public static void SetRockDropTable(RockType rockType, (InventoryController.ItemType ItemType, float DropChance)[] dropTable)
    {
      s_Singleton._rocks[rockType]._DropTable = dropTable;

      if (rockType == s_Singleton._currentRock)
        s_Singleton.SetRockHoverInfos();
    }

    public static (InventoryController.ItemType ItemType, float DropChance)[] GenerateRockDropTable((InventoryController.ItemType ItemType, float DropChance)[] requiredDrops, InventoryController.ItemType baseItem)
    {
      var dropsAppended = new List<(InventoryController.ItemType ItemType, float DropChance)>(requiredDrops);
      var percentRemaining = 100f;
      foreach (var drop in dropsAppended)
        percentRemaining -= drop.DropChance;
      dropsAppended.Add((ItemType: baseItem, DropChance: percentRemaining));

      return dropsAppended.ToArray();
    }

    static void AddRockType(RockType rockType, RockInfo rockInfo)
    {
      rockInfo._RockType = rockType;
      s_Singleton._rocks.Add(rockType, rockInfo);
    }
    public static void UnlockRock(RockType rockType)
    {

      switch (rockType)
      {

        //
        case RockType.STONE:

          AddRockType(rockType, new RockInfo()
          {
            _Title = "Stone",
            _HealthMax = 5f,
            _XpGain = 5f,

            _DropTable = GenerateRockDropTable(new[] {
              (InventoryController.ItemType.EMERALD, s_GemPercent),
            }, InventoryController.ItemType.STONE),
          });
          s_Singleton.SetRockSelection(0, s_Singleton._rocks[rockType]);

          break;

        //
        case RockType.COPPER:

          AddRockType(rockType, new RockInfo()
          {
            _Title = "Copper",
            _HealthMax = 15f,
            _XpGain = 10f,

            _DropTable = GenerateRockDropTable(new[] {
              (InventoryController.ItemType.COPPER, 10f),
              (InventoryController.ItemType.SAPPHIRE, s_GemPercent),
            }, InventoryController.ItemType.STONE),
          });
          s_Singleton.SetRockSelection(1, s_Singleton._rocks[rockType]);

          break;

        //
        case RockType.TIN:

          AddRockType(rockType, new RockInfo()
          {
            _Title = "Tin",
            _HealthMax = 50f,
            _XpGain = 25f,

            _DropTable = GenerateRockDropTable(new[] {
              (InventoryController.ItemType.TIN, 10f),
              (InventoryController.ItemType.RUBY, s_GemPercent),
            }, InventoryController.ItemType.STONE),
          });
          s_Singleton.SetRockSelection(2, s_Singleton._rocks[rockType]);

          break;

        //
        case RockType.IRON:

          AddRockType(rockType, new RockInfo()
          {
            _Title = "Iron",
            _HealthMax = 125f,
            _XpGain = 65f,

            _DropTable = GenerateRockDropTable(new[] {
              (InventoryController.ItemType.IRON, 10f),
              (InventoryController.ItemType.CITRINE, s_GemPercent),
            }, InventoryController.ItemType.STONE),
          });
          s_Singleton.SetRockSelection(3, s_Singleton._rocks[rockType]);

          break;

        //
        case RockType.COAL:

          AddRockType(rockType, new RockInfo()
          {
            _Title = "Coal",
            _HealthMax = 300f,
            _XpGain = 150f,

            _DropTable = GenerateRockDropTable(new[] {
              (InventoryController.ItemType.COAL, 10f),
              (InventoryController.ItemType.DIAMOND, s_GemPercent),
            }, InventoryController.ItemType.STONE),
          });
          s_Singleton.SetRockSelection(4, s_Singleton._rocks[rockType]);

          break;
      }

    }

    //
    void ToggleAutoRock(bool toggle)
    {
      _toggleMineButton.gameObject.SetActive(toggle);
    }
    public static void UnlockAutoRock()
    {
      //s_Singleton.ToggleAutoRock(true);
      s_Singleton._autoReplaceRock = true;
    }

    //
    [System.Serializable]
    public class SaveInfo
    {
      public RockType RockType;
      public float CurrentHealth;

      public bool RockVisible;

      public bool TutorialComplete;
    }
    public static SaveInfo GetSaveInfo()
    {
      var saveInfo = new SaveInfo();

      saveInfo.RockType = s_Singleton._currentRock;
      saveInfo.CurrentHealth = s_Singleton._health;
      saveInfo.RockVisible = s_HasRock;

      saveInfo.TutorialComplete = s_Singleton._tutorialArrow == null;

      return saveInfo;
    }
    public static void SetSaveInfo(SaveInfo saveInfo)
    {
      if (saveInfo.RockVisible)
      {
        s_Singleton.SetRockType(saveInfo.RockType);
        s_Singleton.ToggleRock(true);
        s_Singleton.SetHealth(saveInfo.CurrentHealth);
      }
      else
        s_Singleton._currentRock = saveInfo.RockType;

      if (saveInfo.TutorialComplete)
        GameObject.Destroy(s_Singleton._tutorialArrow.gameObject);
    }

  }

}