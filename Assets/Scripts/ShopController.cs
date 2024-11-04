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
    Dictionary<PurchaseType, PurchaseInfo> _purchaseInfos;
    public enum PurchaseType
    {
      NONE,

      SKILLS,
      FORGE,

      AUTO_ROCK,

      PICKAXE_UPGRADE_0,

      ROCK_0_UPGRADE_0,
      ROCK_1_UPGRADE_0,

      ROCK_BUY_0,
    }
    class PurchaseInfo : IInfoable
    {
      public int _Cost;

      public string _Title, _Description;

      public GameObject _MenuEntry;

      //
      public string _Info { get { return InfoController.GetInfoString(_Title, _Description); } }
      public RectTransform _Transform { get { return _MenuEntry.transform as RectTransform; } }
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
      _purchaseInfos = new();
      void AddPurchase(PurchaseType purchaseType, PurchaseInfo purchaseInfo)
      {
        _purchaseInfos.Add(purchaseType, purchaseInfo);
      }

      AddPurchase(PurchaseType.SKILLS, new PurchaseInfo()
      {
        _Cost = 15,

        _Title = "Skills",
        _Description = "Unlock the Skills tab."
      });
      AddPurchase(PurchaseType.FORGE, new PurchaseInfo()
      {
        _Cost = 250,

        _Title = "Forge",
        _Description = "Unlock the Forge for smelting."
      });

      AddPurchase(PurchaseType.AUTO_ROCK, new PurchaseInfo()
      {
        _Cost = 500,

        _Title = "Auto Rock",
        _Description = "Automatically replace rocks when they are destroyed."
      });

      AddPurchase(PurchaseType.PICKAXE_UPGRADE_0, new PurchaseInfo()
      {
        _Cost = 1000,

        _Title = "Iron Pickaxe",
        _Description = "Base pickaxe damage: 1 -> 2."
      });

      AddPurchase(PurchaseType.ROCK_0_UPGRADE_0, new PurchaseInfo()
      {
        _Cost = 50,

        _Title = "Stone Upgrade",
        _Description = "Double the drops of the Stone rock."
      });
      AddPurchase(PurchaseType.ROCK_1_UPGRADE_0, new PurchaseInfo()
      {
        _Cost = 50,

        _Title = "Copper Upgrade",
        _Description = "Double the drops of the Copper rock."
      });

      AddPurchase(PurchaseType.ROCK_BUY_0, new PurchaseInfo()
      {
        _Cost = 25,

        _Title = "Copper Rock",
        _Description = "Unlock the Copper rock."
      });

      //
      _menu = MainBoxMenuController.s_Singleton.GetMenu(MainBoxMenuController.MenuType.SHOP).transform;

      //
      AddPurchaseToDisplay(PurchaseType.SKILLS);
      AddPurchaseToDisplay(PurchaseType.ROCK_0_UPGRADE_0);
      AddPurchaseToDisplay(PurchaseType.ROCK_BUY_0);
      AddPurchaseToDisplay(PurchaseType.AUTO_ROCK);
      AddPurchaseToDisplay(PurchaseType.FORGE);

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
        buttonImg.color = cost > StatsController.s_Singleton._Gold ? new Color(0.945098f, 0.3239185f, 0.3176471f) : new Color(0.3267443f, 0.9433962f, 0.3159488f);
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
      text.text = string.Format("{0,-24} {1,5}", $"{purchaseInfo._Title}", $"${purchaseInfo._Cost}");

      newMenuEntry.gameObject.SetActive(true);

      // Set button
      var button = newMenuEntry.transform.GetChild(2).GetComponent<Button>();
      button.onClick.AddListener(() =>
      {

        //
        if (purchaseInfo._Cost > StatsController.s_Singleton._Gold)
          return;

        StatsController.s_Singleton._Gold -= purchaseInfo._Cost;
        OnPurchased(purchaseType);

        //
        GameObject.DestroyImmediate(newMenuEntry);

      });
    }

    //
    static void OnPurchased(PurchaseType purchaseType)
    {

      switch (purchaseType)
      {


        //
        case PurchaseType.SKILLS:
          UpgradeController.UnlockUpgrade(UpgradeController.UpgradeType.SKILLS);
          break;
        case PurchaseType.FORGE:
          UpgradeController.UnlockUpgrade(UpgradeController.UpgradeType.FORGE);
          break;

        //
        case PurchaseType.ROCK_0_UPGRADE_0:
          RockController.UpgradeRock(RockController.RockType.STONE);
          break;
        case PurchaseType.ROCK_1_UPGRADE_0:
          RockController.UpgradeRock(RockController.RockType.COPPER);
          break;

        case PurchaseType.ROCK_BUY_0:
          RockController.UnlockRock(RockController.RockType.COPPER);
          s_Singleton.AddPurchaseToDisplay(PurchaseType.ROCK_1_UPGRADE_0);

          break;
      }

    }

  }


}