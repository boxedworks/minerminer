using UnityEngine;
using UnityEngine.UI;

namespace Controllers
{

  //
  public class RockController
  {

    //
    public static RockController s_Singleton;

    //
    bool _hasRock;
    public static bool s_HasRock { get { return s_Singleton._hasRock; } }
    Button _addRockButton;

    //
    Transform _menu, _menuUi, _rockModel;
    public static Transform transform { get { return s_Singleton._rockModel; } }
    Vector3 _rockPosition;
    float _vibrateAmount;

    float _health, _healthMax;
    Slider _healthSlider;

    public enum RockType
    {
      NONE,

      COPPER,
    }
    public class RockInfo
    {
      public float _Health;
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
      _healthSlider = _menuUi.GetChild(0).GetChild(0).Find("RockHealth").GetComponent<Slider>();
      _healthMax = 5f;
      SetHealth(0f);

      //
      _addRockButton = GameObject.Find("AddRockButton").GetComponent<Button>();
      _addRockButton.onClick.AddListener(() =>
      {
        ToggleRock(true);

        AudioController.PlayAudio("MenuSelect");
      });

      //
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
        StatsController.s_Singleton.AddXp(8f);

        AddItemAmount(InventoryController.ItemType.STONE, 5);
        AddItemAmount(InventoryController.ItemType.COPPER, 1);

        ToggleRock(false);

        // Rock Fx
        ParticleController.PlayParticles(ParticleController.ParticleType.ROCK_DESTROY);
        if (MineBoxMenuController.s_Singleton.IsVisible(MineBoxMenuController.MenuType.MINE))
          AudioController.PlayAudio("RockBreak");
      }

      // Normal damage
      else
      {
        AddItemAmount(InventoryController.ItemType.STONE, 1);
      }

      // Pickaxe Fx
      ParticleController.PlayParticles(ParticleController.ParticleType.ROCK_HIT);
      if (MineBoxMenuController.s_Singleton.IsVisible(MineBoxMenuController.MenuType.MINE))
        AudioController.PlayAudio("PickaxeHit");
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
      _health = health;
      _healthSlider.value = _health / _healthMax;
    }

    //
    void ToggleRock(bool toggle)
    {
      _rockModel.gameObject.SetActive(toggle);
      _addRockButton.gameObject.SetActive(!toggle);
      _hasRock = toggle;

      if (toggle)
      {
        SetHealth(_healthMax);
      }
    }

  }

}