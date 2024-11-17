using UnityEngine;
using System.Collections.Generic;

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

    //
    PickaxeStats _pickaxeStats;
    public static PickaxeStats s_PickaxeStats { get { return s_Singleton._pickaxeStats; } }
    public class PickaxeStats : IInfoable
    {

      public float Damage { get { return SkillController.GetMaths(SkillController.SkillType.DAMAGE); } }
      public float Speed { get { return SkillController.GetMaths(SkillController.SkillType.SPEED); } }

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

Drop on hit:   {AmountDroppedOnHit}
Drop on break: {AmountDroppedOnBreak}");
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
    }

    //
    public void Update()
    {

      //
      _swingTimerVisual += (_swingTimer - _swingTimerVisual) * Time.deltaTime * (_swingTimer < _swingTimerVisual ? 50f : 10f);
      _pickaxeModel.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-50f, 65f, Easings.EaseInOutElastic(_swingTimerVisual)));
      _pickaxeModel.localPosition = Vector3.Lerp(new Vector3(-0.7f, 0.2f, 0f), new Vector3(-2f, 1f, 0f), Easings.EaseInOutElastic(_swingTimerVisual));

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

            LogController.AppendLog("Power!");
          }

          powerModifier -= 1f;
        }

        damage *= multiplier;
        RockController.s_Singleton.Hit(damage);
      }
    }

  }

}