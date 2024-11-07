using UnityEngine;

namespace Controllers
{

  public class GameController : MonoBehaviour
  {
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      new EventController();
      new AudioController();
      new ParticleController();
      new MainBoxMenuController();
      new MineBoxMenuController();
      new InfoBoxMenuController();
      new InfoController();
      new StatsController();
      new InventoryController();
      new ShopController();
      new PickaxeController();
      new ForgeController();
      new RockController();
      new DamageTextController();
      new UpgradeController();
    }

    // Update is called once per frame
    void Update()
    {
      StatsController.s_Singleton.Update();
      PickaxeController.s_Singleton.Update();
      RockController.s_Singleton.Update();
      DamageTextController.s_Singleton.Update();
      InventoryController.s_Singleton.Update();
      AudioController.s_Singleton.Update();
      ForgeController.s_Singleton.Update();
      InfoController.s_Singleton.Update();
      UpgradeController.s_Singleton.Update();

      // Cheats
      if (Input.GetKeyDown(KeyCode.Space))
        StatsController.s_Singleton._Gold += 50;
    }
  }

}