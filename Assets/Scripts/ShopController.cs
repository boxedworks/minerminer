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

      PIAKCAXE_UPGRADE_0
    }
    class PurchaseInfo
    {
      public int _Cost;

      public string _Title, _Description;

      public GameObject _MenuEntry;
    }

    Transform _menu;
    bool _menuIsVisible { get { return MainBoxMenuController.s_Singleton.IsVisible(MainBoxMenuController.MenuType.SHOP); } }

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
        _Cost = 100,

        _Title = "Skills",
        _Description = "."
      });
      AddPurchase(PurchaseType.FORGE, new PurchaseInfo()
      {
        _Cost = 25,

        _Title = "Forge",
        _Description = "."
      });

      AddPurchase(PurchaseType.AUTO_ROCK, new PurchaseInfo()
      {
        _Cost = 500,

        _Title = "Auto Rock",
        _Description = "Automatically replace rocks when they are destroyed."
      });
      AddPurchase(PurchaseType.PIAKCAXE_UPGRADE_0, new PurchaseInfo()
      {
        _Cost = 1000,

        _Title = "Iron Pickaxe",
        _Description = "Base pickaxe damage: 1 -> 2."
      });

      //
      _menu = MainBoxMenuController.s_Singleton.GetMenu(MainBoxMenuController.MenuType.SHOP).transform;

      //
      AddPurchaseToDisplay(PurchaseType.SKILLS);
      AddPurchaseToDisplay(PurchaseType.FORGE);
      AddPurchaseToDisplay(PurchaseType.AUTO_ROCK);
      AddPurchaseToDisplay(PurchaseType.PIAKCAXE_UPGRADE_0);

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
      }

    }

  }


}