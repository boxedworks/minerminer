using UnityEngine;

namespace Controllers
{

  //
  public class PickaxeController
  {

    //
    public static PickaxeController s_Singleton;

    //
    Transform _pickaxeModel;
    public static Transform transform { get { return s_Singleton._pickaxeModel; } }
    float _swingTimer, _swingTimerVisual;

    //
    public PickaxeController()
    {
      s_Singleton = this;

      //
      _pickaxeModel = GameObject.Find("Pickaxe").transform;
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
      _swingTimer += Time.deltaTime * StatsController.s_Singleton._PickaxeSpeed;
      if (_swingTimer >= 1f && !RockController.s_HasRock) { _swingTimer = 1f; return; }
      while (_swingTimer >= 1f)
      {
        _swingTimer -= 1f;

        RockController.s_Singleton.Hit(StatsController.s_Singleton._PickaxeDamage);
      }
    }

  }

}