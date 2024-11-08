using System.Collections.Generic;
using Controllers;
using UnityEngine;
using System.IO;

namespace Packages
{

  public static class SaveController
  {

    //
    [System.Serializable]
    class SaveData
    {

      //
      public SaveData()
      {

        // Gold
        Gold = StatsController.s_Singleton._Gold;

        // Purchases
        Purchases = ShopController.GetPurchases();

        // Unlocks
        Unlocks = UpgradeController.GetUnlocks();

        // Skills
        Skills = StatsController.GetSkills();

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
      public List<StatsController.SkillSaveInfo> Skills;

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
      File.WriteAllText("save.json", json);
    }

    public static void Load()
    {

      // Check for save
      if (!File.Exists("save.json"))
        return;

      // Load save
      var json = File.ReadAllText("save.json");
      var saveData = JsonUtility.FromJson<SaveData>(json);

      // Load data
      {

        // Gold
        StatsController.s_Singleton._Gold = saveData.Gold;

        // Purchases
        ShopController.SetPurchases(saveData.Purchases);

        // Unlocks
        UpgradeController.SetUnlocks(saveData.Unlocks);

        // Skills
        StatsController.SetSkills(saveData.Skills);

        // Pic / Rock
        PickaxeController.PickaxeStats.SetSaveInfo(saveData.PickaxeInfo);
        RockController.SetSaveInfo(saveData.RockInfo);

        // Forge
        ForgeController.SetSaveInfo(saveData.ForgeInfo);

        // Inventory
        InventoryController.SetSaveInfo(saveData.InventoryInfo);
      }
    }


  }

}