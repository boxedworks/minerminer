using UnityEngine;

namespace Controllers
{

  public class GameController : MonoBehaviour
  {

    public static string s_GameVersion = "0.0.12";

    public static float s_GameSpeedMod = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

      //
      new MainBoxMenuController();
      new MineBoxMenuController();
      new InfoController();
      new LogController();

      //
      new OptionsController();
      OptionsController.SetGameVersion();

      new SaveController();
      new EventController();
      new AudioController();
      new ParticleController();
      new PrestigeController();
      new SkillController();
      new InventoryController();
      new ShopController();
      new PickaxeController();
      new ForgeController();
      new HammerController();
      new RockController();
      new DamageTextController();
      new UnlockController();

      //
      SaveController.Load();

      //
      LogController.AppendLog($"Welcome to {StringController.s_GAME_NAME} :).");
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
      HammerController.s_Singleton.Update();
      InfoController.s_Singleton.Update();
      UnlockController.s_Singleton.Update();

      // Cheats
#if UNITY_EDITOR
      {
        if (Input.GetKey(KeyCode.Space))
          SkillController.s_Singleton._Gold += 30;

        if (Input.GetKey(KeyCode.M))
          SkillController.s_Singleton.AddXp(30);

        if (Input.GetKey(KeyCode.N))
          InventoryController.s_Singleton.AddItemAmount(1);

        //s_GameSpeedMod = 3f;
      }
#endif

      // Menu hotkeys
      if (Input.GetKeyDown(KeyCode.U))
      {
        MainBoxMenuController.s_Singleton.SetMenuType(MainBoxMenuController.MenuType.SHOP);
        AudioController.PlayAudio("MenuSelect");
      }
      else if (Input.GetKeyDown(KeyCode.I))
      {
        MainBoxMenuController.s_Singleton.SetMenuType(MainBoxMenuController.MenuType.INVENTORY);
        AudioController.PlayAudio("MenuSelect");
      }
      else if (Input.GetKeyDown(KeyCode.S))
      {
        MainBoxMenuController.s_Singleton.SetMenuType(MainBoxMenuController.MenuType.SKILLS);
        AudioController.PlayAudio("MenuSelect");
      }
      else if (Input.GetKeyDown(KeyCode.P))
      {
        MainBoxMenuController.s_Singleton.SetMenuType(MainBoxMenuController.MenuType.PRESTIGE);
        AudioController.PlayAudio("MenuSelect");
      }

      else if (Input.GetKeyDown(KeyCode.Escape))
      {
        if (MainBoxMenuController.s_Singleton.IsVisible(MainBoxMenuController.MenuType.OPTIONS))
          OptionsController.PressMenuButton();
        else
          MainBoxMenuController.s_Singleton.SetMenuType(MainBoxMenuController.MenuType.OPTIONS);
        AudioController.PlayAudio("MenuSelect");
      }
      else if (Input.GetKeyDown(KeyCode.Insert))
      {
        SaveController.Save();
      }

      else if (Input.GetKeyDown(KeyCode.M))
      {
        MineBoxMenuController.s_Singleton.SetMenuType(MineBoxMenuController.MenuType.MINE);
        AudioController.PlayAudio("MenuSelect");
      }
      else if (Input.GetKeyDown(KeyCode.F))
      {
        MineBoxMenuController.s_Singleton.SetMenuType(MineBoxMenuController.MenuType.FORGE);
        AudioController.PlayAudio("MenuSelect");
      }
      else if (Input.GetKeyDown(KeyCode.H))
      {
        MineBoxMenuController.s_Singleton.SetMenuType(MineBoxMenuController.MenuType.HAMMER);
        AudioController.PlayAudio("MenuSelect");
      }

      else if (Input.GetKeyDown(KeyCode.Comma))
      {
        InventoryController.PageLeft();
        AudioController.PlayAudio("MenuSelect");
      }
      else if (Input.GetKeyDown(KeyCode.Period))
      {
        InventoryController.PageRight();
        AudioController.PlayAudio("MenuSelect");
      }
    }

    //
    public void OnApplicationQuit()
    {
      SaveController.Save();
    }


  }

}