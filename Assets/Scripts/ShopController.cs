using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Controllers
{

  //
  public class ShopController
  {

    //
    public static ShopController s_Singleton;

    //
    List<PurchaseType> _purchases;
    Dictionary<PurchaseType, PurchaseInfo> _purchaseInfos;
    public enum PurchaseType
    {
      NONE,

      SKILLS,
      FORGE,

      AUTO_ROCK,

      PICKAXE_UPGRADE_0,

      PICKAXE_BREAK_UPGRADE_0,
      PICKAXE_BREAK_UPGRADE_1,
      PICKAXE_BREAK_UPGRADE_2,
      PICKAXE_BREAK_UPGRADE_3,

      ROCK_0_UPGRADE_0,
      ROCK_1_UPGRADE_0, ROCK_1_UPGRADE_1, ROCK_1_UPGRADE_2,
      ROCK_2_UPGRADE_0, ROCK_2_UPGRADE_1, ROCK_2_UPGRADE_2,
      ROCK_3_UPGRADE_0, ROCK_3_UPGRADE_1, ROCK_3_UPGRADE_2,

      ROCK_BUY_0,
      ROCK_BUY_1,
      ROCK_BUY_2,

      SKILL_LUCK,
      SKILL_POWER,
    }
    class PurchaseInfo : IInfoable
    {
      public int _Cost;

      public string _Title, _Description, _PurchaseString;

      public bool _IsPurchased;

      public GameObject _MenuEntry;

      //
      public string _Info { get { return InfoController.GetInfoString(_Title, _Description); } }
      public RectTransform _Transform { get { return _MenuEntry == null ? null : _MenuEntry.transform as RectTransform; } }
    }

    Transform _menu;
    bool _menuIsVisible { get { return MainBoxMenuController.s_Singleton.IsVisible(MainBoxMenuController.MenuType.SHOP); } }

    //
    public InfoData _InfoData
    {
      get
      {
        var returnData = new InfoData();

        var returnList = new List<IInfoable>();
        foreach (var item in _purchaseInfos)
          if (item.Value._MenuEntry != null)
            returnList.Add(item.Value);
        returnData._Infos = returnList;

        return returnData;
      }
    }

    //
    public ShopController()
    {
      s_Singleton = this;

      //
      _purchases = new();
      _purchaseInfos = new();
      void AddPurchase(PurchaseType purchaseType, PurchaseInfo purchaseInfo)
      {
        _purchaseInfos.Add(purchaseType, purchaseInfo);
      }

      AddPurchase(PurchaseType.SKILLS, new PurchaseInfo()
      {
        _Cost = 10,

        _Title = "Skills",
        _Description = "Unlock the Skills tab.",

        _PurchaseString = "Unlocked the Skills menu."
      });
      AddPurchase(PurchaseType.FORGE, new PurchaseInfo()
      {
        _Cost = 75,

        _Title = "Forge",
        _Description = "Unlock the Forge for smelting.",

        _PurchaseString = "Unlocked the Forge menu."
      });

      AddPurchase(PurchaseType.AUTO_ROCK, new PurchaseInfo()
      {
        _Cost = 250,

        _Title = "Auto Rock",
        _Description = "Automatically replace rocks when they are destroyed.",

        _PurchaseString = "Purchased the Auto Rock upgrade."
      });

      AddPurchase(PurchaseType.PICKAXE_UPGRADE_0, new PurchaseInfo()
      {
        _Cost = 1000,

        _Title = "Iron Pickaxe",
        _Description = "Base pickaxe damage: 1 -> 2."
      });
      AddPurchase(PurchaseType.PICKAXE_BREAK_UPGRADE_0, new PurchaseInfo()
      {
        _Cost = 35,

        _Title = "Sharpen Pickaxe 1",
        _Description = "Increases items dropped on rock break: 4 -> 5.",

        _PurchaseString = "Purchased the Sharpen Pickaxe 1 upgrade."
      });
      AddPurchase(PurchaseType.PICKAXE_BREAK_UPGRADE_1, new PurchaseInfo()
      {
        _Cost = 150,

        _Title = "Sharpen Pickaxe 2",
        _Description = "Increases items dropped on rock break: 5 -> 6.",

        _PurchaseString = "Purchased the Sharpen Pickaxe 2 upgrade."
      });
      AddPurchase(PurchaseType.PICKAXE_BREAK_UPGRADE_2, new PurchaseInfo()
      {
        _Cost = 500,

        _Title = "Sharpen Pickaxe 3",
        _Description = "Increases items dropped on rock break: 6 -> 7.",

        _PurchaseString = "Purchased the Sharpen Pickaxe 3 upgrade."
      });
      AddPurchase(PurchaseType.PICKAXE_BREAK_UPGRADE_3, new PurchaseInfo()
      {
        _Cost = 1250,

        _Title = "Sharpen Pickaxe 4",
        _Description = "Increases items dropped on rock break: 7 -> 8.",

        _PurchaseString = "Purchased the Sharpen Pickaxe 4 upgrade."
      });

      AddPurchase(PurchaseType.ROCK_0_UPGRADE_0, new PurchaseInfo()
      {
        _Cost = 25,

        _Title = "Stone Upgrade 1",
        _Description = "Double the drops of the Stone rock."
      });

      AddPurchase(PurchaseType.ROCK_1_UPGRADE_0, new PurchaseInfo()
      {
        _Cost = 50,

        _Title = "Copper Upgrade 1",
        _Description = "Extra 5% chance to find Copper in Copper rocks.",

        _PurchaseString = "Purchased the Copper Rock Upgrade 1."
      });
      AddPurchase(PurchaseType.ROCK_1_UPGRADE_1, new PurchaseInfo()
      {
        _Cost = 125,

        _Title = "Copper Upgrade 2",
        _Description = "Extra 5% chance to find Copper in Copper rocks.",

        _PurchaseString = "Purchased the Copper Rock Upgrade 2."
      });
      AddPurchase(PurchaseType.ROCK_1_UPGRADE_2, new PurchaseInfo()
      {
        _Cost = 200,

        _Title = "Copper Upgrade 3",
        _Description = "Extra 5% chance to find Copper in Copper rocks.",

        _PurchaseString = "Purchased the Copper Rock Upgrade 3."
      });

      AddPurchase(PurchaseType.ROCK_2_UPGRADE_0, new PurchaseInfo()
      {
        _Cost = 250,

        _Title = "Tin Upgrade 1",
        _Description = "Extra 5% chance to find Tin in Tin rocks.",

        _PurchaseString = "Purchased the Tin Rock Upgrade 1."
      });
      AddPurchase(PurchaseType.ROCK_2_UPGRADE_1, new PurchaseInfo()
      {
        _Cost = 350,

        _Title = "Tin Upgrade 2",
        _Description = "Extra 5% chance to find Tin in Tin rocks.",

        _PurchaseString = "Purchased the Tin Rock Upgrade 2."
      });
      AddPurchase(PurchaseType.ROCK_2_UPGRADE_2, new PurchaseInfo()
      {
        _Cost = 500,

        _Title = "Tin Upgrade 3",
        _Description = "Extra 5% chance to find Tin in Tin rocks.",

        _PurchaseString = "Purchased the Tin Rock Upgrade 3."
      });

      AddPurchase(PurchaseType.ROCK_3_UPGRADE_0, new PurchaseInfo()
      {
        _Cost = 1250,

        _Title = "Iron Upgrade 1",
        _Description = "Extra 5% chance to find Iron in Iron rocks.",

        _PurchaseString = "Purchased the Iron Rock Upgrade 1."
      });
      AddPurchase(PurchaseType.ROCK_3_UPGRADE_1, new PurchaseInfo()
      {
        _Cost = 1500,

        _Title = "Iron Upgrade 2",
        _Description = "Extra 5% chance to find Iron in Iron rocks.",

        _PurchaseString = "Purchased the Iron Rock Upgrade 2."
      });
      AddPurchase(PurchaseType.ROCK_3_UPGRADE_2, new PurchaseInfo()
      {
        _Cost = 1850,

        _Title = "Iron Upgrade 3",
        _Description = "Extra 5% chance to find Iron in Iron rocks.",

        _PurchaseString = "Purchased the Iron Rock Upgrade 3."
      });

      AddPurchase(PurchaseType.ROCK_BUY_0, new PurchaseInfo()
      {
        _Cost = 20,

        _Title = "Copper Rock",
        _Description = "Unlock the Copper rock.",

        _PurchaseString = "Unlocked the Copper Rock."
      });
      AddPurchase(PurchaseType.ROCK_BUY_1, new PurchaseInfo()
      {
        _Cost = 250,

        _Title = "Tin Rock",
        _Description = "Unlock the Tin rock.",

        _PurchaseString = "Unlocked the Tin Rock."
      });
      AddPurchase(PurchaseType.ROCK_BUY_2, new PurchaseInfo()
      {
        _Cost = 1000,

        _Title = "Iron Rock",
        _Description = "Unlock the Iron rock.",

        _PurchaseString = "Unlocked the Iron Rock."
      });

      AddPurchase(PurchaseType.SKILL_LUCK, new PurchaseInfo()
      {
        _Cost = 250,

        _Title = "Skill - Luck",
        _Description = @"Unlock the Luck skill.

Luck - A chance to get extra items!",

        _PurchaseString = "Unlocked skill: Luck."
      });
      AddPurchase(PurchaseType.SKILL_POWER, new PurchaseInfo()
      {
        _Cost = 250,

        _Title = "Skill - Power",
        _Description = @"Unlock the Power skill.

Power - A chance to do more damage!",

        _PurchaseString = "Unlocked skill: Power."
      });

      //
      _menu = MainBoxMenuController.s_Singleton.GetMenu(MainBoxMenuController.MenuType.SHOP).transform;

      //
      AddPurchaseToDisplay(PurchaseType.SKILLS);
      //AddPurchaseToDisplay(PurchaseType.ROCK_0_UPGRADE_0);
      AddPurchaseToDisplay(PurchaseType.ROCK_BUY_0);
      AddPurchaseToDisplay(PurchaseType.AUTO_ROCK);
      AddPurchaseToDisplay(PurchaseType.PICKAXE_BREAK_UPGRADE_0);
      //AddPurchaseToDisplay(PurchaseType.FORGE);

      UpdatePurchasesUi();
    }

    //
    public static void UpdatePurchasesUi()
    {
      if (s_Singleton == null) return;
      var purchaseInfos = s_Singleton._purchaseInfos;

      foreach (var purchaseInfo in purchaseInfos)
      {
        if (purchaseInfo.Value._MenuEntry == null) continue;

        var cost = purchaseInfo.Value._Cost;
        var buttonImg = purchaseInfo.Value._MenuEntry.transform.GetChild(2).GetComponent<Image>();
        buttonImg.color = cost > SkillController.s_Singleton._Gold ? new Color(0.945098f, 0.3239185f, 0.3176471f) : new Color(0.3267443f, 0.9433962f, 0.3159488f);
      }

    }

    //
    void AddPurchaseToDisplay(PurchaseType purchaseType)
    {
      var prefab = _menu.GetChild(0);

      var newMenuEntry = GameObject.Instantiate(prefab.gameObject, prefab.parent);
      _purchaseInfos[purchaseType]._MenuEntry = newMenuEntry;

      var text = newMenuEntry.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();

      var purchaseInfo = _purchaseInfos[purchaseType];
      text.text = string.Format("{0,-42} {1,10}", $"{purchaseInfo._Title}", $"${purchaseInfo._Cost}");

      newMenuEntry.gameObject.SetActive(true);

      // Set button
      var button = newMenuEntry.transform.GetChild(2).GetComponent<Button>();
      button.onClick.AddListener(() =>
      {

        AudioController.PlayAudio("MenuSelect");

        //
        if (purchaseInfo._Cost > SkillController.s_Singleton._Gold)
        {

          LogController.AppendLog($"<color=red>Insufficient gold to purchase </color>{purchaseInfo._Title}<color=red>!</color>");
          LogController.ForceOpen();

          return;
        }

        SkillController.s_Singleton._Gold -= purchaseInfo._Cost;
        AudioController.PlayAudio("ShopBuy");

        Purchase(purchaseType);
      });
    }

    //
    static void Purchase(PurchaseType purchaseType)
    {
      var purchaseInfo = s_Singleton._purchaseInfos[purchaseType];
      if (purchaseInfo._IsPurchased)
        return;

      //
      s_Singleton._purchases.Add(purchaseType);
      purchaseInfo._IsPurchased = true;
      if (purchaseInfo._MenuEntry != null)
        GameObject.DestroyImmediate(purchaseInfo._MenuEntry.gameObject);

      //
      switch (purchaseType)
      {

        //
        case PurchaseType.SKILLS:
          UnlockController.Unlock(UnlockController.UnlockType.SKILLS);
          s_Singleton.AddPurchaseToDisplay(PurchaseType.SKILL_LUCK);
          s_Singleton.AddPurchaseToDisplay(PurchaseType.SKILL_POWER);

          break;
        case PurchaseType.FORGE:
          UnlockController.Unlock(UnlockController.UnlockType.FORGE);

          break;

        //
        case PurchaseType.SKILL_LUCK:
          SkillController.ToggleSkill(SkillController.SkillType.LUCK, true);

          break;
        case PurchaseType.SKILL_POWER:
          SkillController.ToggleSkill(SkillController.SkillType.POWER, true);

          break;

        //
        case PurchaseType.PICKAXE_BREAK_UPGRADE_0:
          PickaxeController.s_PickaxeStats.SetDropOnBreak(5);
          s_Singleton.AddPurchaseToDisplay(PurchaseType.PICKAXE_BREAK_UPGRADE_1);

          break;
        case PurchaseType.PICKAXE_BREAK_UPGRADE_1:
          PickaxeController.s_PickaxeStats.SetDropOnBreak(6);
          s_Singleton.AddPurchaseToDisplay(PurchaseType.PICKAXE_BREAK_UPGRADE_2);

          break;
        case PurchaseType.PICKAXE_BREAK_UPGRADE_2:
          PickaxeController.s_PickaxeStats.SetDropOnBreak(7);
          s_Singleton.AddPurchaseToDisplay(PurchaseType.PICKAXE_BREAK_UPGRADE_3);

          break;
        case PurchaseType.PICKAXE_BREAK_UPGRADE_3:
          PickaxeController.s_PickaxeStats.SetDropOnBreak(8);

          break;

        //
        case PurchaseType.ROCK_0_UPGRADE_0:
          //RockController.UpgradeRock(RockController.RockType.STONE);

          break;
        case PurchaseType.ROCK_1_UPGRADE_0:
          s_Singleton.AddPurchaseToDisplay(PurchaseType.ROCK_1_UPGRADE_1);
          RockController.SetRockDropTable(RockController.RockType.COPPER, RockController.GenerateRockDropTable(new[]{
            (InventoryController.ItemType.COPPER, 15f),
            (InventoryController.ItemType.SAPPHIRE, RockController.s_GemPercent),
          }, InventoryController.ItemType.STONE));

          break;
        case PurchaseType.ROCK_1_UPGRADE_1:
          s_Singleton.AddPurchaseToDisplay(PurchaseType.ROCK_1_UPGRADE_2);
          RockController.SetRockDropTable(RockController.RockType.COPPER, RockController.GenerateRockDropTable(new[]{
            (InventoryController.ItemType.COPPER, 20f),
            (InventoryController.ItemType.SAPPHIRE, RockController.s_GemPercent),
          }, InventoryController.ItemType.STONE));

          break;
        case PurchaseType.ROCK_1_UPGRADE_2:
          RockController.SetRockDropTable(RockController.RockType.COPPER, RockController.GenerateRockDropTable(new[]{
            (InventoryController.ItemType.COPPER, 25f),
            (InventoryController.ItemType.SAPPHIRE, RockController.s_GemPercent),
          }, InventoryController.ItemType.STONE));

          break;

        case PurchaseType.ROCK_2_UPGRADE_0:
          s_Singleton.AddPurchaseToDisplay(PurchaseType.ROCK_2_UPGRADE_1);
          RockController.SetRockDropTable(RockController.RockType.TIN, RockController.GenerateRockDropTable(new[]{
            (InventoryController.ItemType.TIN, 15f),
            (InventoryController.ItemType.RUBY, RockController.s_GemPercent),
          }, InventoryController.ItemType.STONE));

          break;
        case PurchaseType.ROCK_2_UPGRADE_1:
          s_Singleton.AddPurchaseToDisplay(PurchaseType.ROCK_2_UPGRADE_2);
          RockController.SetRockDropTable(RockController.RockType.TIN, RockController.GenerateRockDropTable(new[]{
            (InventoryController.ItemType.TIN, 20f),
            (InventoryController.ItemType.RUBY, RockController.s_GemPercent),
          }, InventoryController.ItemType.STONE));

          break;
        case PurchaseType.ROCK_2_UPGRADE_2:
          RockController.SetRockDropTable(RockController.RockType.TIN, RockController.GenerateRockDropTable(new[]{
            (InventoryController.ItemType.TIN, 25f),
            (InventoryController.ItemType.RUBY, RockController.s_GemPercent),
          }, InventoryController.ItemType.STONE));

          break;

        case PurchaseType.ROCK_3_UPGRADE_0:
          s_Singleton.AddPurchaseToDisplay(PurchaseType.ROCK_3_UPGRADE_1);
          RockController.SetRockDropTable(RockController.RockType.IRON, RockController.GenerateRockDropTable(new[]{
            (InventoryController.ItemType.IRON, 15f),
            (InventoryController.ItemType.CITRINE, RockController.s_GemPercent),
          }, InventoryController.ItemType.STONE));

          break;
        case PurchaseType.ROCK_3_UPGRADE_1:
          s_Singleton.AddPurchaseToDisplay(PurchaseType.ROCK_3_UPGRADE_2);
          RockController.SetRockDropTable(RockController.RockType.IRON, RockController.GenerateRockDropTable(new[]{
            (InventoryController.ItemType.IRON, 20f),
            (InventoryController.ItemType.CITRINE, RockController.s_GemPercent),
          }, InventoryController.ItemType.STONE));

          break;
        case PurchaseType.ROCK_3_UPGRADE_2:
          RockController.SetRockDropTable(RockController.RockType.IRON, RockController.GenerateRockDropTable(new[]{
            (InventoryController.ItemType.IRON, 25f),
            (InventoryController.ItemType.CITRINE, RockController.s_GemPercent),
          }, InventoryController.ItemType.STONE));

          break;

        case PurchaseType.ROCK_BUY_0:
          RockController.UnlockRock(RockController.RockType.COPPER);
          s_Singleton.AddPurchaseToDisplay(PurchaseType.FORGE);
          s_Singleton.AddPurchaseToDisplay(PurchaseType.ROCK_1_UPGRADE_0);
          s_Singleton.AddPurchaseToDisplay(PurchaseType.ROCK_BUY_1);

          break;
        case PurchaseType.ROCK_BUY_1:
          RockController.UnlockRock(RockController.RockType.TIN);
          s_Singleton.AddPurchaseToDisplay(PurchaseType.ROCK_2_UPGRADE_0);
          s_Singleton.AddPurchaseToDisplay(PurchaseType.ROCK_BUY_2);
          ForgeController.UnlockRecipe(ForgeController.RecipeType.BRONZE_INGOT);

          break;
        case PurchaseType.ROCK_BUY_2:
          RockController.UnlockRock(RockController.RockType.IRON);
          s_Singleton.AddPurchaseToDisplay(PurchaseType.ROCK_3_UPGRADE_0);
          ForgeController.UnlockRecipe(ForgeController.RecipeType.IRON_INGOT);

          break;

        //
        case PurchaseType.AUTO_ROCK:
          RockController.UnlockAutoRock();

          break;
      }
      UpdatePurchasesUi();

      //
      var purchaseString = s_Singleton._purchaseInfos[purchaseType]._PurchaseString;
      if (purchaseString != null)
        LogController.AppendLog(purchaseString);

    }

    //
    public static List<string> GetPurchases()
    {

      var returnList = new List<string>();
      foreach (var purchase in s_Singleton._purchases)
        returnList.Add(purchase.ToString());

      return returnList;
    }
    public static void SetPurchases(List<string> purchases)
    {
      foreach (var purchaseString in purchases)
      {
        if (System.Enum.TryParse(purchaseString, true, out PurchaseType purchaseType))
          Purchase(purchaseType);
      }
    }

  }


}