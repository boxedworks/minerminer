using UnityEngine;
using UnityEngine.SceneManagement;

using System.IO;
using System.Collections.Generic;

namespace Controllers
{

  public class SaveController
  {

    //
    public static SaveController s_Singleton;

    bool _isLoading;
    public static bool s_IsLoading { get { return s_Singleton._isLoading; } }

    //
    public SaveController()
    {
      s_Singleton = this;
    }

    //
    [System.Serializable]
    class SaveData
    {

      //
      public SaveData()
      {

        // Gold
        Gold = SkillController.s_Singleton._Gold;

        // Purchases
        Purchases = ShopController.GetPurchases();

        // Unlocks
        Unlocks = UnlockController.GetUnlocks();

        // Skills
        Skills = SkillController.GetSkills();
        SkillInfo = SkillController.GetSaveInfo();

        // Pic / Rock
        PickaxeInfo = PickaxeController.PickaxeStats.GetSaveInfo();
        RockInfo = RockController.GetSaveInfo();

        // Forge
        ForgeInfo = ForgeController.GetSaveInfo();

        // Hammer
        HammerInfo = HammerController.GetSaveInfo();

        // Inventory
        InventoryInfo = InventoryController.GetSaveInfo();

        // Options
        OptionsInfo = OptionsController.GetSaveInfo();
      }

      //
      public int Gold;

      //
      public List<string> Purchases, Unlocks;

      //
      public List<SkillController.SkillSaveInfo> Skills;
      public SkillController.SaveInfo SkillInfo;

      //
      public PickaxeController.PickaxeStats.SaveInfo PickaxeInfo;
      public RockController.SaveInfo RockInfo;

      //
      public MachineController.SaveInfo ForgeInfo, HammerInfo;

      //
      public InventoryController.SaveInfo InventoryInfo;

      //
      public OptionsController.SaveInfo OptionsInfo;

    }

    static string s_saveDirPath
    {
      get
      {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
          return "/idbfs/minerminer";

        return $"{Application.persistentDataPath}";
      }
    }
    static string s_saveFilePath
    {
      get
      {
        return $"{s_saveDirPath}/save.json";
      }
    }

    //
    public static void Save()
    {

      //
      var save = new SaveData();

      //
      var json = JsonUtility.ToJson(save, true);

      //
      if (Application.platform == RuntimePlatform.WebGLPlayer)
        if (!Directory.Exists(s_saveDirPath))
          Directory.CreateDirectory(s_saveDirPath);
      File.WriteAllText(s_saveFilePath, json);

      LogController.AppendLog("<color=purple>Game saved.</color>");
    }

    public static void Load()
    {

      var json = "";
      if (File.Exists(s_saveFilePath))
      {
        json = File.ReadAllText(s_saveFilePath);
        LogController.AppendLog("<color=purple>Game loaded from save.</color>");
      }

      // Check for save
      if (json == null || json.Trim().Length == 0)
        return;

      // Load save
      var saveData = JsonUtility.FromJson<SaveData>(json);

      // Load data
      s_Singleton._isLoading = true;
      {

        // Gold
        SkillController.s_Singleton._Gold = saveData.Gold;

        // Purchases
        ShopController.SetPurchases(saveData.Purchases);

        // Unlocks
        UnlockController.SetUnlocks(saveData.Unlocks);

        // Skills
        SkillController.SetSkills(saveData.Skills);
        SkillController.SetSaveInfo(saveData.SkillInfo);

        // Pic / Rock
        PickaxeController.PickaxeStats.SetSaveInfo(saveData.PickaxeInfo);
        RockController.SetSaveInfo(saveData.RockInfo);

        // Forge
        ForgeController.SetSaveInfo(saveData.ForgeInfo);

        // Hammer
        HammerController.SetSaveInfo(saveData.HammerInfo);

        // Inventory
        InventoryController.SetSaveInfo(saveData.InventoryInfo);

        // Options
        OptionsController.SetSaveInfo(saveData.OptionsInfo);
      }
      s_Singleton._isLoading = false;
    }

    //
    public static void Delete()
    {

      // Keep options
      //var options = OptionsController.GetSaveInfo();

      //
      if (File.Exists(s_saveFilePath))
        File.Delete(s_saveFilePath);

      //
      //OptionsController.SetSaveInfo(options);
      //Save();

      //
      SceneManager.LoadScene(0);
    }
  }

}