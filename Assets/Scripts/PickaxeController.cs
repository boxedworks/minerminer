using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Controllers
{

  //
  public class PickaxeController
  {

    //
    public static PickaxeController s_Singleton;

    //
    Transform _pickaxeModel;
    RectTransform _pickaxeUi;

    float _swingTimer, _swingTimerVisual;

    ClickManager _clickManager;

    //
    PickaxeStats _pickaxeStats;
    public static PickaxeStats s_PickaxeStats { get { return s_Singleton._pickaxeStats; } }
    public class PickaxeStats : IInfoable
    {

      public float Damage { get { return SkillController.GetMaths(SkillController.SkillType.DAMAGE); } }
      public float Speed { get { return SkillController.GetMaths(SkillController.SkillType.SPEED) * GameController.s_GameSpeedMod; } }

      public int AmountDroppedOnHit { get { return 1; } }
      int _amountDroppedOnBreak;
      public int AmountDroppedOnBreak { get { return _amountDroppedOnBreak; } }

      //
      public string _Info
      {
        get
        {
          return InfoController.GetInfoString("Pickaxe", @$"Damage: {Damage}
Speed:  {Speed * 100f}%

Drop on hit:   {AmountDroppedOnHit * RockController.s_DropMultiplier}
Drop on break: {AmountDroppedOnBreak * RockController.s_DropMultiplier}");
        }
      }
      public RectTransform _Transform { get { return s_Singleton._pickaxeUi; } }

      //
      public PickaxeStats()
      {
        _amountDroppedOnBreak = 4;
      }

      //
      public void SetDropOnBreak(int amount)
      {
        _amountDroppedOnBreak = amount;
      }

      //
      [System.Serializable]
      public class SaveInfo
      {
        public int DropOnHit, DropOnBreak;
      }
      public static SaveInfo GetSaveInfo()
      {
        var saveInfo = new SaveInfo();

        // saveInfo.DropOnHit = s_PickaxeStats.am;
        saveInfo.DropOnBreak = s_PickaxeStats._amountDroppedOnBreak;

        return saveInfo;
      }
      public static void SetSaveInfo(SaveInfo saveInfo)
      {
        s_PickaxeStats.SetDropOnBreak(saveInfo.DropOnBreak);
      }
    }

    //
    public PickaxeController()
    {
      s_Singleton = this;
      _pickaxeStats = new();

      //
      _pickaxeModel = GameObject.Find("Pickaxe").transform;
      _pickaxeUi = GameObject.Find("PicBox").transform as RectTransform;

      _swingTimer = 0f;

      //
      _clickManager = new();
    }

    //
    public void Update()
    {

      _clickManager.Update();

      //
      _swingTimerVisual = Mathf.Clamp(_swingTimerVisual + (_swingTimer - _swingTimerVisual) * Time.deltaTime * (_swingTimer < _swingTimerVisual ? 50f : 10f), 0f, 1f);
      _pickaxeModel.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-50f, 65f, Easings.EaseInOutElastic(_swingTimerVisual)));
      _pickaxeModel.localPosition = Vector3.Lerp(new Vector3(-890f, 320f, 0f), new Vector3(-1030f, 420f, 0f), Easings.EaseInOutElastic(_swingTimerVisual));

      //
      _swingTimer += Time.deltaTime * s_PickaxeStats.Speed * 0.25f;
      if (_swingTimer >= 1f && !RockController.s_HasRock) { _swingTimer = 1f; return; }
      while (_swingTimer >= 1f)
      {
        _swingTimer -= 1f;

        //
        var damage = s_PickaxeStats.Damage;
        var multiplier = 1;
        var powerModifier = SkillController.GetMaths(SkillController.SkillType.POWER);
        while (powerModifier > 0f)
        {
          var randomNumber = Random.Range(0f, 1f);
          if (powerModifier >= 1 || randomNumber <= powerModifier)
          {
            multiplier++;
          }

          powerModifier -= 1f;
        }

        damage *= multiplier;
        RockController.s_Singleton.Hit(damage);
        DamageTextController.s_Singleton.ReportDamage(damage, multiplier - 1);
      }
    }

    //
    public static void UpgradeClicker()
    {
      s_Singleton._clickManager.Upgrade();
    }

    //
    class ClickManager
    {

      Transform _container;

      float _lastSpawnTime, _clickTimeToReach, _damageMultiplier;
      int _numClicks, _maxClicks;

      int _upgradeLevel;

      public ClickManager()
      {
        _container = GameObject.Find("Clicks").transform;

        _clickTimeToReach = 5f;
        _maxClicks = 1;

        _damageMultiplier = 1f;
      }

      //
      public void Update()
      {

        // Check unlocked
        if (!ShopController.IsPurchased(ShopController.PurchaseType.ROCK_CLICKER))
          return;

        // Remove if rock breaks
        if (_numClicks > 0)
        {

          if (!RockController.s_HasRock)
          {
            for (var i = _container.childCount - 1; i > 0; i--)
              GameObject.DestroyImmediate(_container.GetChild(i).gameObject);
            _numClicks = 0;
          }
        }

        // Spawn new click
        if (_numClicks < _maxClicks && Time.time - _lastSpawnTime > _clickTimeToReach)
        {
          if (!RockController.s_HasRock)
          {
            _lastSpawnTime = Time.time + Random.value;
            return;
          }

          _numClicks++;
          _lastSpawnTime = Time.time + Random.value;

          var prefab = _container.GetChild(0).gameObject;
          var newFab = GameObject.Instantiate(prefab, _container);

          newFab.transform.localPosition += new Vector3(Random.Range(-1f, 1f) * 70f, Random.Range(-1f, 1f) * 70f, 0f);

          var button = newFab.GetComponent<Button>();
          button.onClick.AddListener(() =>
          {

            _numClicks--;

            GameObject.Destroy(newFab);

            var damage = s_PickaxeStats.Damage / 2f * _damageMultiplier;
            RockController.s_Singleton.Hit(damage);
            DamageTextController.s_Singleton.ReportDamage(damage, 0);

            if (_numClicks == _maxClicks - 1)
              _lastSpawnTime = Time.time + Random.value;
          });
          newFab.SetActive(true);
        }

      }

      //
      public void Upgrade()
      {
        switch (_upgradeLevel++)
        {

          case 0:
            _maxClicks++;
            break;
          case 1:
            _damageMultiplier *= 2f;
            break;
          case 2:
            _clickTimeToReach -= 1f;
            break;
          case 3:
            _maxClicks++;
            break;
          case 4:
            _damageMultiplier *= 2f;
            break;
          case 5:
            _clickTimeToReach -= 1f;
            break;
        }

      }

    }

  }

}