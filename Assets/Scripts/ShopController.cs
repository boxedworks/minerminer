using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace Controllers
{

  //
  public class ShopController
  {

    //
    public static ShopController s_Singleton;

    float _lastPurchaseTime;

    //
    List<PurchaseType> _purchases;
    Dictionary<PurchaseType, PurchaseInfo> _purchaseInfos;
    public enum PurchaseType
    {
      NONE,

      SKILLS,
      FORGE,
      HAMMER,

      PRESTIGE,

      AUTO_ROCK,
      AUTO_FORGE,

      ROCK_CLICKER,
      ROCK_CLICKER_UPGRADE_0,
      ROCK_CLICKER_UPGRADE_1,
      ROCK_CLICKER_UPGRADE_2,
      ROCK_CLICKER_UPGRADE_3,
      ROCK_CLICKER_UPGRADE_4,

      PICKAXE_BREAK_UPGRADE_0,
      PICKAXE_BREAK_UPGRADE_1,
      PICKAXE_BREAK_UPGRADE_2,
      PICKAXE_BREAK_UPGRADE_3,
      PICKAXE_BREAK_UPGRADE_4,
      PICKAXE_BREAK_UPGRADE_5,

      ROCK_1_UPGRADE_0, ROCK_1_UPGRADE_1, ROCK_1_UPGRADE_2,
      ROCK_2_UPGRADE_0, ROCK_2_UPGRADE_1, ROCK_2_UPGRADE_2,
      ROCK_3_UPGRADE_0, ROCK_3_UPGRADE_1, ROCK_3_UPGRADE_2,
      ROCK_4_UPGRADE_0, ROCK_4_UPGRADE_1, ROCK_4_UPGRADE_2,

      ROCK_BUY_0,
      ROCK_BUY_1,
      ROCK_BUY_2,
      ROCK_BUY_3,

      ROCK_BUY_4,
      ROCK_BUY_5,

      SKILL_LUCK,
      SKILL_POWER,
      SKILL_HEAT,
    }
    class PurchaseInfo : IInfoable
    {
      public (InventoryController.ItemType ItemType, int Amount)[] _Costs;
      public bool _CanAffordPurchase
      {
        get
        {
          foreach (var costInfo in _Costs)
          {

            // Check gold
            if (costInfo.ItemType == InventoryController.ItemType.NONE)
            {
              if (SkillController.s_Singleton._Gold < costInfo.Amount)
                return false;

              continue;
            }

            // Check item amount
            if (InventoryController.GetItemAmount(costInfo.ItemType) < costInfo.Amount)
              return false;
          }

          return true;
        }
      }

      public string _Title, _Description, _PurchaseString;

      public bool _CanPurchase, _IsPurchased;

      public GameObject _MenuEntry;

      //
      public string _Info
      {
        get
        {

          var costString = "";
          foreach (var costInfo in _Costs)
          {

            if (costInfo.ItemType == InventoryController.ItemType.NONE)
            {
              costString += $"- ${costInfo.Amount}\n";
              continue;
            }
            costString += $"- {costInfo.Amount} {InventoryController.GetItemName(costInfo.ItemType)}\n";
          }

          return InfoController.GetInfoString(_Title, $"{_Description}\n\n\n<b>= Cost:</b>\n{costString}");
        }
      }
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
        _Costs = GetSimpleCost(10),

        _Title = "Skills",
        _Description = "Unlock the Skills tab.",

        _PurchaseString = "Unlocked the Skills menu."
      });
      AddPurchase(PurchaseType.FORGE, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(45),

        _Title = "Forge",
        _Description = "Unlock the Forge for smelting.",

        _PurchaseString = "Unlocked the Forge menu."
      });
      AddPurchase(PurchaseType.HAMMER, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(350),

        _Title = "Hammer",
        _Description = "Unlock the Hammer to create new items.",

        _PurchaseString = "Unlocked the Hammer menu."
      });

      AddPurchase(PurchaseType.PRESTIGE, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(25000),

        _Title = "Prestige",
        _Description = "Unlock the Prestige menu to... prestige!",

        _PurchaseString = "Unlocked the Prestige menu."
      });

      AddPurchase(PurchaseType.AUTO_ROCK, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(150),

        _Title = "Auto Rock",
        _Description = "Automatically replace rocks when they are destroyed.",

        _PurchaseString = "Purchased the Auto Rock upgrade."
      });
      AddPurchase(PurchaseType.AUTO_FORGE, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(250),

        _Title = "Auto Forge",
        _Description = "Automatically continue forging if you have enough resources.",

        _PurchaseString = "Purchased the Auto Forge upgrade."
      });

      AddPurchase(PurchaseType.ROCK_CLICKER, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(15),

        _Title = "Rock Clicker",
        _Description = "Periodically spawn weak points on rocks that you can click to do 1/2 your pickaxe damage.",

        _PurchaseString = "Purchased the Rock Clicker upgrade."
      });
      AddPurchase(PurchaseType.ROCK_CLICKER_UPGRADE_0, new PurchaseInfo()
      {
        _Costs = new[]{
          (InventoryController.ItemType.EMERALD, 1)
        },

        _Title = "Rock Clicker Upgrade 1",
        _Description = "Increases the max number of weak spots that can appear: 1 -> 2.",

        _PurchaseString = "Purchased Rock Clicker Upgrade 1."
      });
      AddPurchase(PurchaseType.ROCK_CLICKER_UPGRADE_1, new PurchaseInfo()
      {
        _Costs = new[]{
          (InventoryController.ItemType.EMERALD, 1),
          (InventoryController.ItemType.SAPPHIRE, 1)
        },

        _Title = "Rock Clicker Upgrade 2",
        _Description = "Weak spot damage 1x -> 2x.",

        _PurchaseString = "Purchased Rock Clicker Upgrade 2."
      });
      AddPurchase(PurchaseType.ROCK_CLICKER_UPGRADE_2, new PurchaseInfo()
      {
        _Costs = new[]{
          (InventoryController.ItemType.SAPPHIRE, 1),
          (InventoryController.ItemType.RUBY, 1)
        },

        _Title = "Rock Clicker Upgrade 3",
        _Description = "Slightly increase the speed that weak spots appear.",

        _PurchaseString = "Purchased Rock Clicker Upgrade 4."
      });
      AddPurchase(PurchaseType.ROCK_CLICKER_UPGRADE_3, new PurchaseInfo()
      {
        _Costs = new[]{
          (InventoryController.ItemType.RUBY, 1),
          (InventoryController.ItemType.CITRINE, 1)
        },

        _Title = "Rock Clicker Upgrade 4",
        _Description = "Increases the max number of weak spots that can appear: 2 -> 3.",

        _PurchaseString = "Purchased Rock Clicker Upgrade 4."
      });
      AddPurchase(PurchaseType.ROCK_CLICKER_UPGRADE_4, new PurchaseInfo()
      {
        _Costs = new[]{
          (InventoryController.ItemType.CITRINE, 1),
          (InventoryController.ItemType.DIAMOND, 1)
        },

        _Title = "Rock Clicker Upgrade 5",
        _Description = "Weak spot damage 2x -> 4x.",

        _PurchaseString = "Purchased Rock Clicker Upgrade 5."
      });

      AddPurchase(PurchaseType.PICKAXE_BREAK_UPGRADE_0, new PurchaseInfo()
      {
        _Costs = new[]{
          (InventoryController.ItemType.NONE, 10),
          (InventoryController.ItemType.STONE, 5)
        },

        _Title = "Sharpen Pickaxe 1",
        _Description = "Increases items dropped on rock break: 4 -> 5.",

        _PurchaseString = "Purchased the Sharpen Pickaxe 1 upgrade."
      });
      AddPurchase(PurchaseType.PICKAXE_BREAK_UPGRADE_1, new PurchaseInfo()
      {
        _Costs = new[]{
          (InventoryController.ItemType.NONE, 85),
          (InventoryController.ItemType.COPPER, 5)
        },

        _Title = "Sharpen Pickaxe 2",
        _Description = "Increases items dropped on rock break: 5 -> 6.",

        _PurchaseString = "Purchased the Sharpen Pickaxe 2 upgrade."
      });
      AddPurchase(PurchaseType.PICKAXE_BREAK_UPGRADE_2, new PurchaseInfo()
      {
        _Costs = new[]{
          (InventoryController.ItemType.NONE, 275),
          (InventoryController.ItemType.TIN, 10)
        },

        _Title = "Sharpen Pickaxe 3",
        _Description = "Increases items dropped on rock break: 6 -> 7.",

        _PurchaseString = "Purchased the Sharpen Pickaxe 3 upgrade."
      });
      AddPurchase(PurchaseType.PICKAXE_BREAK_UPGRADE_3, new PurchaseInfo()
      {
        _Costs = new[]{
          (InventoryController.ItemType.NONE, 1100),
          (InventoryController.ItemType.IRON, 20)
        },

        _Title = "Sharpen Pickaxe 4",
        _Description = "Increases items dropped on rock break: 7 -> 8.",

        _PurchaseString = "Purchased the Sharpen Pickaxe 4 upgrade."
      });
      AddPurchase(PurchaseType.PICKAXE_BREAK_UPGRADE_4, new PurchaseInfo()
      {
        _Costs = new[]{
          (InventoryController.ItemType.NONE, 4500),
          (InventoryController.ItemType.COAL, 25)
        },

        _Title = "Sharpen Pickaxe 5",
        _Description = "Increases items dropped on rock break: 8 -> 9.",

        _PurchaseString = "Purchased the Sharpen Pickaxe 5 upgrade."
      });
      AddPurchase(PurchaseType.PICKAXE_BREAK_UPGRADE_5, new PurchaseInfo()
      {
        _Costs = new[]{
          (InventoryController.ItemType.NONE, 17500),
          (InventoryController.ItemType.STONE_CHUNK, 30)
        },

        _Title = "Sharpen Pickaxe 6",
        _Description = "Increases items dropped on rock break: 9 -> 10.",

        _PurchaseString = "Purchased the Sharpen Pickaxe 6 upgrade."
      });

      AddPurchase(PurchaseType.ROCK_1_UPGRADE_0, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(65),

        _Title = "Copper Upgrade 1",
        _Description = "Extra 5% chance to find Copper in Copper rocks.",

        _PurchaseString = "Purchased the Copper Rock Upgrade 1."
      });
      AddPurchase(PurchaseType.ROCK_1_UPGRADE_1, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(175),

        _Title = "Copper Upgrade 2",
        _Description = "Extra 5% chance to find Copper in Copper rocks.",

        _PurchaseString = "Purchased the Copper Rock Upgrade 2."
      });
      AddPurchase(PurchaseType.ROCK_1_UPGRADE_2, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(300),

        _Title = "Copper Upgrade 3",
        _Description = "Extra 5% chance to find Copper in Copper rocks.",

        _PurchaseString = "Purchased the Copper Rock Upgrade 3."
      });

      AddPurchase(PurchaseType.ROCK_2_UPGRADE_0, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(200),

        _Title = "Tin Upgrade 1",
        _Description = "Extra 5% chance to find Tin in Tin rocks.",

        _PurchaseString = "Purchased the Tin Rock Upgrade 1."
      });
      AddPurchase(PurchaseType.ROCK_2_UPGRADE_1, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(325),

        _Title = "Tin Upgrade 2",
        _Description = "Extra 5% chance to find Tin in Tin rocks.",

        _PurchaseString = "Purchased the Tin Rock Upgrade 2."
      });
      AddPurchase(PurchaseType.ROCK_2_UPGRADE_2, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(475),

        _Title = "Tin Upgrade 3",
        _Description = "Extra 5% chance to find Tin in Tin rocks.",

        _PurchaseString = "Purchased the Tin Rock Upgrade 3."
      });

      AddPurchase(PurchaseType.ROCK_3_UPGRADE_0, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(1250),

        _Title = "Iron Upgrade 1",
        _Description = "Extra 5% chance to find Iron in Iron rocks.",

        _PurchaseString = "Purchased the Iron Rock Upgrade 1."
      });
      AddPurchase(PurchaseType.ROCK_3_UPGRADE_1, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(1850),

        _Title = "Iron Upgrade 2",
        _Description = "Extra 5% chance to find Iron in Iron rocks.",

        _PurchaseString = "Purchased the Iron Rock Upgrade 2."
      });
      AddPurchase(PurchaseType.ROCK_3_UPGRADE_2, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(3350),

        _Title = "Iron Upgrade 3",
        _Description = "Extra 5% chance to find Iron in Iron rocks.",

        _PurchaseString = "Purchased the Iron Rock Upgrade 3."
      });

      AddPurchase(PurchaseType.ROCK_4_UPGRADE_0, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(6500),

        _Title = "Coal Upgrade 1",
        _Description = "Extra 5% chance to find Coal in Coal rocks.",

        _PurchaseString = "Purchased the Coal Rock Upgrade 1."
      });
      AddPurchase(PurchaseType.ROCK_4_UPGRADE_1, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(12500),

        _Title = "Coal Upgrade 2",
        _Description = "Extra 5% chance to find Coal in Coal rocks.",

        _PurchaseString = "Purchased the Coal Rock Upgrade 2."
      });
      AddPurchase(PurchaseType.ROCK_4_UPGRADE_2, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(30000),

        _Title = "Coal Upgrade 3",
        _Description = "Extra 5% chance to find Coal in Coal rocks.",

        _PurchaseString = "Purchased the Coal Rock Upgrade 3."
      });

      AddPurchase(PurchaseType.ROCK_BUY_0, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(25),

        _Title = "Copper Rock",
        _Description = "Unlock the Copper rock.",

        _PurchaseString = "Unlocked the Copper Rock."
      });
      AddPurchase(PurchaseType.ROCK_BUY_1, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(150),

        _Title = "Tin Rock",
        _Description = "Unlock the Tin rock.",

        _PurchaseString = "Unlocked the Tin Rock."
      });
      AddPurchase(PurchaseType.ROCK_BUY_2, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(1000),

        _Title = "Iron Rock",
        _Description = "Unlock the Iron rock.",

        _PurchaseString = "Unlocked the Iron Rock."
      });
      AddPurchase(PurchaseType.ROCK_BUY_3, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(5000),

        _Title = "Coal Rock",
        _Description = "Unlock the Coal rock.",

        _PurchaseString = "Unlocked the Coal Rock."
      });
      AddPurchase(PurchaseType.ROCK_BUY_4, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(15000),

        _Title = "Boulder",
        _Description = "Replace the Stone rock with the Boulder rock.",

        _PurchaseString = "Unlocked the Boulder Rock."
      });
      AddPurchase(PurchaseType.ROCK_BUY_5, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(25000),

        _Title = "Cobalt",
        _Description = "Unlock the Cobalt rock.",

        _PurchaseString = "Unlocked the Cobalt Rock."
      });

      AddPurchase(PurchaseType.SKILL_LUCK, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(250),

        _Title = "Skill - Luck",
        _Description = @"Unlock the Luck skill.

Luck - A chance to get extra items!",

        _PurchaseString = "Unlocked skill: Luck."
      });
      AddPurchase(PurchaseType.SKILL_POWER, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(250),

        _Title = "Skill - Power",
        _Description = @"Unlock the Power skill.

Power - A chance to do more damage!",

        _PurchaseString = "Unlocked skill: Power."
      });
      AddPurchase(PurchaseType.SKILL_HEAT, new PurchaseInfo()
      {
        _Costs = GetSimpleCost(500),

        _Title = "Skill - Heat",
        _Description = @"Unlock the Heat skill.

Heat - Increases forge speed!",

        _PurchaseString = "Unlocked skill: Heat."
      });

      //
      _menu = MainBoxMenuController.s_Singleton.GetMenu(MainBoxMenuController.MenuType.SHOP).transform;

      //
      UnlockPurchase(PurchaseType.SKILLS);
      UnlockPurchase(PurchaseType.ROCK_BUY_0);
      UnlockPurchase(PurchaseType.ROCK_CLICKER);
      UnlockPurchase(PurchaseType.AUTO_ROCK);
      UnlockPurchase(PurchaseType.PICKAXE_BREAK_UPGRADE_0);

      UpdatePurchases();
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

        var buttonImg = purchaseInfo.Value._MenuEntry.transform.GetChild(2).GetComponent<Image>();
        buttonImg.color = purchaseInfo.Value._CanAffordPurchase ? StringController.s_ColorGreen : StringController.s_ColorRed;

        // Check cost labels
        var costContainer = purchaseInfo.Value._MenuEntry.transform.GetChild(3);
        var costIndex = -1;
        foreach (var costInfo in purchaseInfo.Value._Costs)
        {
          costIndex++;

          var hasItem = false;
          Transform costEntry = null;

          // $$
          if (costInfo.ItemType == InventoryController.ItemType.NONE)
          {
            hasItem = SkillController.s_Singleton._Gold >= costInfo.Amount;

            if (costIndex == 0)
              costEntry = costContainer.GetChild(0);
          }
          else
          {
            if (costIndex < 1)
              costIndex++;

            hasItem = InventoryController.GetItemAmount(costInfo.ItemType) >= costInfo.Amount;
            costEntry = costContainer.GetChild(costIndex + 1);
          }

          costEntry.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().color = !hasItem ? StringController.s_ColorRed : StringController.s_ColorGreen;
        }
      }

    }

    //
    void UnlockPurchase(PurchaseType purchaseType)
    {
      var purchaseInfo = _purchaseInfos[purchaseType];
      purchaseInfo._CanPurchase = true;
    }

    void UpdatePurchases()
    {
      var prefab = _menu.GetChild(0);

      var entryIndex = 0;
      foreach (PurchaseType purchaseType in Enum.GetValues(typeof(PurchaseType)))
      {

        //
        if (purchaseType == PurchaseType.NONE) continue;

        //
        var purchaseInfo = _purchaseInfos[purchaseType];
        purchaseInfo._MenuEntry = null;
        if (purchaseInfo._IsPurchased || !purchaseInfo._CanPurchase) continue;

        Transform menuEntry = null;
        if (entryIndex >= _menu.childCount - 1)
          menuEntry = GameObject.Instantiate(prefab.gameObject, prefab.parent).transform;
        else
          menuEntry = _menu.GetChild(entryIndex + 1);
        entryIndex++;

        purchaseInfo._MenuEntry = menuEntry.gameObject;

        var text = menuEntry.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
        text.text = string.Format("{0,-42}", $"{purchaseInfo._Title}");

        // Create cost labels
        var costContainer = menuEntry.GetChild(3);
        for (var i = costContainer.childCount - 1; i > 1; i--)
          GameObject.DestroyImmediate(costContainer.GetChild(i).gameObject);
        var goldCost = costContainer.GetChild(0);
        var costPrefab = costContainer.GetChild(1).gameObject;
        var hasGoldCost = false;
        foreach (var costInfo in purchaseInfo._Costs)
        {

          if (costInfo.ItemType == InventoryController.ItemType.NONE)
          {
            hasGoldCost = true;

            var goldText = goldCost.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
            goldText.text = $"${costInfo.Amount}";

            goldText.transform.parent.gameObject.SetActive(true);

            continue;
          }

          //
          var newCost = GameObject.Instantiate(costPrefab, costPrefab.transform.parent);

          var labelText = newCost.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
          labelText.text = $"{costInfo.Amount}";
          newCost.transform.GetChild(1).GetComponent<Image>().sprite = InventoryController.GetItemSprite(costInfo.ItemType);

          if (costInfo.Amount > 9)
          {

            var rect = (labelText.transform as RectTransform).sizeDelta = new Vector2(61.2f, 54.5f);
            (newCost.transform as RectTransform).sizeDelta = new Vector2(119.5f, 54.5f);
          }

          newCost.gameObject.SetActive(true);

        }
        goldCost.gameObject.SetActive(hasGoldCost);

        // Set button
        var button = menuEntry.GetChild(2).GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {

          AudioController.PlayAudio("MenuSelect");

          //
          if (Time.time - _lastPurchaseTime < 0.05f) return;

          //
          if (!purchaseInfo._CanAffordPurchase)
          {

            LogController.AppendLog($"<color=red>Cannot purchase </color>{purchaseInfo._Title}<color=red>!</color>");

            return;
          }

          foreach (var costInfo in purchaseInfo._Costs)
          {

            //
            if (costInfo.ItemType == InventoryController.ItemType.NONE)
            {
              SkillController.s_Singleton._Gold -= costInfo.Amount;

              continue;
            }

            //
            InventoryController.s_Singleton.RemoveItemAmount(costInfo.ItemType, costInfo.Amount);
          }

          AudioController.PlayAudio("ShopBuy");

          _lastPurchaseTime = Time.time;
          Purchase(purchaseType);
        });

        //
        menuEntry.gameObject.SetActive(true);
      }
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
          s_Singleton.UnlockPurchase(PurchaseType.SKILL_LUCK);
          s_Singleton.UnlockPurchase(PurchaseType.SKILL_POWER);

          break;
        case PurchaseType.FORGE:
          UnlockController.Unlock(UnlockController.UnlockType.FORGE);
          s_Singleton.UnlockPurchase(PurchaseType.HAMMER);
          s_Singleton.UnlockPurchase(PurchaseType.SKILL_HEAT);

          break;
        case PurchaseType.HAMMER:
          UnlockController.Unlock(UnlockController.UnlockType.HAMMER);

          break;

        case PurchaseType.PRESTIGE:
          UnlockController.Unlock(UnlockController.UnlockType.PRESTIGE);

          break;

        //
        case PurchaseType.SKILL_LUCK:
          SkillController.ToggleSkill(SkillController.SkillType.LUCK, true);

          break;
        case PurchaseType.SKILL_POWER:
          SkillController.ToggleSkill(SkillController.SkillType.POWER, true);

          break;
        case PurchaseType.SKILL_HEAT:
          SkillController.ToggleSkill(SkillController.SkillType.HEAT, true);

          break;

        //
        case PurchaseType.PICKAXE_BREAK_UPGRADE_0:
          PickaxeController.s_PickaxeStats.SetDropOnBreak(5);
          s_Singleton.UnlockPurchase(PurchaseType.PICKAXE_BREAK_UPGRADE_1);

          break;
        case PurchaseType.PICKAXE_BREAK_UPGRADE_1:
          PickaxeController.s_PickaxeStats.SetDropOnBreak(6);
          s_Singleton.UnlockPurchase(PurchaseType.PICKAXE_BREAK_UPGRADE_2);

          break;
        case PurchaseType.PICKAXE_BREAK_UPGRADE_2:
          PickaxeController.s_PickaxeStats.SetDropOnBreak(7);
          s_Singleton.UnlockPurchase(PurchaseType.PICKAXE_BREAK_UPGRADE_3);

          break;
        case PurchaseType.PICKAXE_BREAK_UPGRADE_3:
          PickaxeController.s_PickaxeStats.SetDropOnBreak(8);
          s_Singleton.UnlockPurchase(PurchaseType.PICKAXE_BREAK_UPGRADE_4);

          break;
        case PurchaseType.PICKAXE_BREAK_UPGRADE_4:
          PickaxeController.s_PickaxeStats.SetDropOnBreak(9);
          s_Singleton.UnlockPurchase(PurchaseType.PICKAXE_BREAK_UPGRADE_5);

          break;
        case PurchaseType.PICKAXE_BREAK_UPGRADE_5:
          PickaxeController.s_PickaxeStats.SetDropOnBreak(10);

          break;

        //
        case PurchaseType.ROCK_1_UPGRADE_0:
          s_Singleton.UnlockPurchase(PurchaseType.ROCK_1_UPGRADE_1);
          RockController.SetRockDropTable(RockController.RockType.COPPER, RockController.GenerateRockDropTable(new[]{
            (InventoryController.ItemType.COPPER, 15f),
            (InventoryController.ItemType.SAPPHIRE, RockController.s_GemPercent),
          }, InventoryController.ItemType.STONE));

          break;
        case PurchaseType.ROCK_1_UPGRADE_1:
          s_Singleton.UnlockPurchase(PurchaseType.ROCK_1_UPGRADE_2);
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
          s_Singleton.UnlockPurchase(PurchaseType.ROCK_2_UPGRADE_1);
          RockController.SetRockDropTable(RockController.RockType.TIN, RockController.GenerateRockDropTable(new[]{
            (InventoryController.ItemType.TIN, 15f),
            (InventoryController.ItemType.RUBY, RockController.s_GemPercent),
          }, InventoryController.ItemType.STONE));

          break;
        case PurchaseType.ROCK_2_UPGRADE_1:
          s_Singleton.UnlockPurchase(PurchaseType.ROCK_2_UPGRADE_2);
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
          s_Singleton.UnlockPurchase(PurchaseType.ROCK_3_UPGRADE_1);
          RockController.SetRockDropTable(RockController.RockType.IRON, RockController.GenerateRockDropTable(new[]{
            (InventoryController.ItemType.IRON, 15f),
            (InventoryController.ItemType.CITRINE, RockController.s_GemPercent),
          }, InventoryController.ItemType.STONE));

          break;
        case PurchaseType.ROCK_3_UPGRADE_1:
          s_Singleton.UnlockPurchase(PurchaseType.ROCK_3_UPGRADE_2);
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

        case PurchaseType.ROCK_4_UPGRADE_0:
          s_Singleton.UnlockPurchase(PurchaseType.ROCK_4_UPGRADE_1);
          RockController.SetRockDropTable(RockController.RockType.COAL, RockController.GenerateRockDropTable(new[]{
            (InventoryController.ItemType.COAL, 15f),
            (InventoryController.ItemType.DIAMOND, RockController.s_GemPercent),
          }, InventoryController.ItemType.STONE));

          break;
        case PurchaseType.ROCK_4_UPGRADE_1:
          s_Singleton.UnlockPurchase(PurchaseType.ROCK_4_UPGRADE_2);
          RockController.SetRockDropTable(RockController.RockType.COAL, RockController.GenerateRockDropTable(new[]{
            (InventoryController.ItemType.COAL, 20f),
            (InventoryController.ItemType.DIAMOND, RockController.s_GemPercent),
          }, InventoryController.ItemType.STONE));

          break;
        case PurchaseType.ROCK_4_UPGRADE_2:
          RockController.SetRockDropTable(RockController.RockType.COAL, RockController.GenerateRockDropTable(new[]{
            (InventoryController.ItemType.COAL, 25f),
            (InventoryController.ItemType.DIAMOND, RockController.s_GemPercent),
          }, InventoryController.ItemType.STONE));

          break;

        case PurchaseType.ROCK_BUY_0:
          RockController.UnlockRock(RockController.RockType.COPPER);
          s_Singleton.UnlockPurchase(PurchaseType.FORGE);
          s_Singleton.UnlockPurchase(PurchaseType.ROCK_1_UPGRADE_0);
          s_Singleton.UnlockPurchase(PurchaseType.ROCK_BUY_1);

          break;
        case PurchaseType.ROCK_BUY_1:
          RockController.UnlockRock(RockController.RockType.TIN);
          s_Singleton.UnlockPurchase(PurchaseType.ROCK_2_UPGRADE_0);
          s_Singleton.UnlockPurchase(PurchaseType.ROCK_BUY_2);
          s_Singleton.UnlockPurchase(PurchaseType.AUTO_FORGE);

          ForgeController.UnlockRecipe(ForgeController.RecipeType.IRON_INGOT);

          break;
        case PurchaseType.ROCK_BUY_2:
          RockController.UnlockRock(RockController.RockType.IRON);
          s_Singleton.UnlockPurchase(PurchaseType.ROCK_3_UPGRADE_0);
          s_Singleton.UnlockPurchase(PurchaseType.ROCK_BUY_3);

          ForgeController.UnlockRecipe(ForgeController.RecipeType.STEEL_INGOT);

          break;
        case PurchaseType.ROCK_BUY_3:
          RockController.UnlockRock(RockController.RockType.COAL);
          s_Singleton.UnlockPurchase(PurchaseType.ROCK_4_UPGRADE_0);
          s_Singleton.UnlockPurchase(PurchaseType.ROCK_BUY_4);

          HammerController.UnlockRecipe(HammerController.RecipeType.MIX_0);
          ForgeController.UnlockRecipe(ForgeController.RecipeType.CITRINE_INGOT);

          break;

        case PurchaseType.ROCK_BUY_4:
          RockController.UnlockRock(RockController.RockType.BOULDER);
          s_Singleton.UnlockPurchase(PurchaseType.ROCK_BUY_5);

          if (!MainBoxMenuController.s_Singleton.IsMenuActive(MainBoxMenuController.MenuType.PRESTIGE))
            s_Singleton.UnlockPurchase(PurchaseType.PRESTIGE);

          break;
        case PurchaseType.ROCK_BUY_5:
          RockController.UnlockRock(RockController.RockType.COBALT);

          break;

        //
        case PurchaseType.AUTO_ROCK:
          RockController.UnlockAutoRock();

          break;
        case PurchaseType.AUTO_FORGE:
          ForgeController.UnlockAutoForge();

          break;

        case PurchaseType.ROCK_CLICKER:
          s_Singleton.UnlockPurchase(PurchaseType.ROCK_CLICKER_UPGRADE_0);

          break;
        case PurchaseType.ROCK_CLICKER_UPGRADE_0:
          PickaxeController.UpgradeClicker();
          s_Singleton.UnlockPurchase(PurchaseType.ROCK_CLICKER_UPGRADE_1);

          break;
        case PurchaseType.ROCK_CLICKER_UPGRADE_1:
          PickaxeController.UpgradeClicker();
          s_Singleton.UnlockPurchase(PurchaseType.ROCK_CLICKER_UPGRADE_2);

          break;
        case PurchaseType.ROCK_CLICKER_UPGRADE_2:
          PickaxeController.UpgradeClicker();
          s_Singleton.UnlockPurchase(PurchaseType.ROCK_CLICKER_UPGRADE_3);

          break;
        case PurchaseType.ROCK_CLICKER_UPGRADE_3:
          PickaxeController.UpgradeClicker();
          s_Singleton.UnlockPurchase(PurchaseType.ROCK_CLICKER_UPGRADE_4);

          break;
        case PurchaseType.ROCK_CLICKER_UPGRADE_4:
          PickaxeController.UpgradeClicker();
          //s_Singleton.UnlockPurchase(PurchaseType.ROCK_CLICKER_UPGRADE_5);

          break;
      }
      s_Singleton.UpdatePurchases();
      UpdatePurchasesUi();

      //
      var purchaseString = s_Singleton._purchaseInfos[purchaseType]._PurchaseString;
      if (purchaseString != null)
        LogController.AppendLog(purchaseString);
    }

    //
    public static bool IsPurchased(PurchaseType purchaseType)
    {
      return s_Singleton._purchaseInfos[purchaseType]._IsPurchased;
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

    static (InventoryController.ItemType, int)[] GetSimpleCost(int goldAmount)
    {
      return new[] { (InventoryController.ItemType.NONE, goldAmount) };
    }

  }


}