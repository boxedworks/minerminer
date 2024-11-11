using Packages;
using UnityEngine;

namespace Controllers
{

  public class GameController : MonoBehaviour
  {
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      new SaveController();
      new EventController();
      new AudioController();
      new ParticleController();
      new MainBoxMenuController();
      new MineBoxMenuController();
      new InfoBoxMenuController();
      new InfoController();
      new LogController();
      new SkillController();
      new InventoryController();
      new ShopController();
      new PickaxeController();
      new ForgeController();
      new RockController();
      new DamageTextController();
      new UnlockController();

      //
      SaveController.Load();
    }

    // Update is called once per frame
    void Update()
    {
      SkillController.s_Singleton.Update();
      PickaxeController.s_Singleton.Update();
      RockController.s_Singleton.Update();
      DamageTextController.s_Singleton.Update();
      InventoryController.s_Singleton.Update();
      AudioController.s_Singleton.Update();
      ForgeController.s_Singleton.Update();
      InfoController.s_Singleton.Update();
      UnlockController.s_Singleton.Update();

      // Cheats
      {
        if (Input.GetKeyDown(KeyCode.Space))
          SkillController.s_Singleton._Gold += 50;
      }

      //
      if (Input.GetKeyDown(KeyCode.S))
      {
        SaveController.Save();
        LogController.AppendLog("Saved.");
      }
      if (Input.GetKeyDown(KeyCode.L))
      {
        SaveController.Load();
        LogController.AppendLog("Loaded.");
      }
    }

    //
    public void OnApplicationQuit()
    {
      SaveController.Save();
    }


  }

}