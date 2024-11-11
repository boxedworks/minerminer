using System.Collections.Generic;
using Controllers;
using UnityEngine;
using System.IO;

namespace Packages
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

        // Pic / Rock
        PickaxeInfo = PickaxeController.PickaxeStats.GetSaveInfo();
        RockInfo = RockController.GetSaveInfo();

        // Forge
        ForgeInfo = ForgeController.GetSaveInfo();

        // Inventory
        InventoryInfo = InventoryController.GetSaveInfo();
      }

      //
      public int Gold;

      //
      public List<string> Purchases, Unlocks;

      //
      public List<SkillController.SkillSaveInfo> Skills;

      //
      public PickaxeController.PickaxeStats.PickaxeSaveInfo PickaxeInfo;
      public RockController.RockSaveInfo RockInfo;

      //
      public ForgeController.ForgeSaveInfo ForgeInfo;

      //
      public InventoryController.InventoryInfo InventoryInfo;

    }

    //
    public static void Save()
    {

      //
      var save = new SaveData();

      //
      var json = JsonUtility.ToJson(save);

      //
      if (Application.platform == RuntimePlatform.WebGLPlayer)
        PlayerPrefs.SetString("json", json);

      else
        File.WriteAllText("save.json", json);
    }

    public static void Load()
    {

      var json = "";
      if (Application.platform == RuntimePlatform.WebGLPlayer)
        json = PlayerPrefs.GetString("json", "");

      else
      {
        if (File.Exists("save.json"))
          json = File.ReadAllText("save.json");
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

        // Pic / Rock
        PickaxeController.PickaxeStats.SetSaveInfo(saveData.PickaxeInfo);
        RockController.SetSaveInfo(saveData.RockInfo);

        // Forge
        ForgeController.SetSaveInfo(saveData.ForgeInfo);

        // Inventory
        InventoryController.SetSaveInfo(saveData.InventoryInfo);
      }
      s_Singleton._isLoading = false;
    }


  }

}