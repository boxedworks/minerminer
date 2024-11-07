using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;
using Unity.VisualScripting;

namespace Controllers
{

  //
  public class RockController : IHasInfoables
  {

    //
    public static RockController s_Singleton;

    //
    bool _hasRock;
    public static bool s_HasRock { get { return s_Singleton._hasRock; } }
    Button _addRockButton, _toggleMineButton, _stopMineButton;

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
    }
    public class RockInfo : IInfoable
    {
      public RockType _RockType;
      public string _Title;

      public float _Health, _HealthMax, _XpGain;
      public (InventoryController.ItemType, float)[] _DropTable;

      public GameObject _MenuEntry;

      //
      public string _Info
      {
        get
        {

          var breakProductString = "";
          foreach (var drop in _DropTable)
          {
            breakProductString += $"- {InventoryController.GetItemName(drop.Item1)} ({drop.Item2}%)\n";
          }

          //
          var desc = "";
          if (breakProductString.Length > 0)
          {
            desc += $"<b>Drop table</b>\n\n{breakProductString}";
          }

          return InfoController.GetInfoString($"{_Title}", @$"Health: {_HealthMax}
Xp:     {_XpGain}

{desc}");
        }
      }
      public RectTransform _Transform { get { return _MenuEntry.transform as RectTransform; } }
    }

    //
    List<IInfoable> _otherInfoables;
    IInfoable _rockInfo;
    public InfoData _InfoData
    {
      get
      {
        var returnData = new InfoData();

        var returnList = new List<IInfoable>();
        if (_rockMenu.gameObject.activeSelf)
        {
          foreach (var rockInfo in _rocks)
            if (rockInfo.Value._MenuEntry != null)
              returnList.Add(rockInfo.Value);
        }
        else
          foreach (var otherInfo in _otherInfoables)
            returnList.Add(otherInfo);

        //
        if (_rockInfo != null)
          returnList.Add(_rockInfo);

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

      SetHealth(0f);

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
        _rockMenu.gameObject.SetActive(true);

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

        AudioController.PlayAudio("MenuSelect");
      });
      _otherInfoables.Add(new SimpleInfoable()
      {
        _GameObject = _toggleMineButton.gameObject,
        _Description = InfoController.GetInfoString("Auto replace rock", "On or off.")
      });

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
      SetRockType(RockType.STONE);
      ToggleRock(false);
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

    }

    //
    public void Hit(float damage)
    {

      // Vibrate rock
      var amount = 0.15f;
      _vibrateAmount = Mathf.Clamp(_vibrateAmount + amount, amount, 1f);

      // Take damage
      SetHealth(_health - damage);
      DamageTextController.s_Singleton.AddText(damage);

      // Break
      if (_health <= 0f)
      {
        StatsController.s_Singleton.AddXp(_rocks[_currentRock]._XpGain);

        for (var i = 0; i < 5; i++)
          AddItemAmount(GetRockDrop(_currentRock), 1);

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
        for (var i = 0; i < 1; i++)
          AddItemAmount(GetRockDrop(_currentRock), 1);
      }

      // Pickaxe Fx
      ParticleController.PlayParticles(ParticleController.ParticleType.ROCK_HIT);
      if (MineBoxMenuController.s_Singleton.IsVisible(MineBoxMenuController.MenuType.MINE))
        AudioController.PlayAudio("PickaxeHit");
    }

    //
    void SetRockType(RockType rockType)
    {
      _currentRock = rockType;
      _rockModel.GetComponent<SpriteRenderer>().sprite = GetRockSprite(rockType);

      var rockInfo = _rocks[_currentRock];
      SetHealth(rockInfo._HealthMax);

      SetRockHoverInfos();
    }

    //
    void SetRockHoverInfos()
    {
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
        _rockInfo = null;

        SetHealth(0f);
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
        if (randomPercent <= percentThreshhold)
          return drop.Item1;

        percentAdd += percentThreshhold;
      }

      return dropTable[^1].Item1;
    }

    //
    void SetRockSelection(int buttonIndex, RockInfo rockInfo)
    {

      var button = (buttonIndex < 5 ? _rockMenu.GetChild(0).GetChild(buttonIndex) : _rockMenu.GetChild(1).GetChild(buttonIndex - 5))
        .GetChild(0).GetComponent<Button>();
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

    public static void SetRockDropTable(RockType rockType, (InventoryController.ItemType, float)[] dropTable)
    {
      s_Singleton._rocks[rockType]._DropTable = dropTable;

      if (rockType == s_Singleton._currentRock)
        s_Singleton.SetRockHoverInfos();
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

            _DropTable = new (InventoryController.ItemType, float)[] { (InventoryController.ItemType.STONE, 100f) },
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

            _DropTable = new (InventoryController.ItemType, float)[] { (InventoryController.ItemType.COPPER, 10f), (InventoryController.ItemType.STONE, 90f) },
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

            _DropTable = new (InventoryController.ItemType, float)[] { (InventoryController.ItemType.TIN, 10f), (InventoryController.ItemType.STONE, 90f) },
          });
          s_Singleton.SetRockSelection(2, s_Singleton._rocks[rockType]);

          break;

      }

    }

  }

}