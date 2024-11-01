using System.Collections.Generic;
using UnityEngine;

namespace Controllers
{

  //
  public class MainBoxMenuController : BoxMenuController, IHasInfoables
  {

    //
    public static MainBoxMenuController s_Singleton;

    MenuType _currentMenu { get { return (MenuType)_currentMenuIndex; } }
    public enum MenuType
    {

      NONE,

      SHOP,
      SKILLS,
      INVENTORY,

    }

    //
    public InfoData _InfoData
    {
      get
      {
        InfoData returnData;

        switch (_currentMenu)
        {
          case MenuType.SHOP:
            returnData = StatsController.s_Singleton._InfoData;
            break;

          case MenuType.INVENTORY:
            returnData = InventoryController.s_Singleton._InfoData;
            break;

          default:
            returnData = StatsController.s_Singleton._InfoData;
            break;
        }

        //
        foreach (var dependency in _dependencyInfos)
          returnData._Infos.Add(dependency);

        //
        return returnData;
      }
    }

    //
    public MainBoxMenuController()
    {
      s_Singleton = this;

      //
      var menuOrderString = new string[]{
        MenuType.NONE.ToString(),

        MenuType.SHOP.ToString(),
        MenuType.SKILLS.ToString(),
        MenuType.INVENTORY.ToString(),
      };
      SetUpMenus(GameObject.Find("BoxStatsButtons").transform.GetChild(0), menuOrderString);

      //
      SetMenuType((int)MenuType.INVENTORY);
      SetMenuActive(MenuType.INVENTORY, true);
    }

    public GameObject GetMenu(MenuType ofMenu)
    {
      return _menus[(int)ofMenu];
    }

    public bool IsVisible(MenuType ofMenu)
    {
      return IsVisible((int)ofMenu);
    }
    public void SetMenuActive(MenuType menuType, bool toggle)
    {
      SetMenuActive((int)menuType, toggle);
    }
    public bool IsMenuActive(MenuType menuType)
    {
      return IsMenuActive((int)menuType);
    }

    //
    protected override string GetButtonDescription(string buttonName)
    {
      switch (buttonName)
      {

        case "InventoryButton":
          return @"<b>Inventory</b>

Browse and sell your items.
          ";
        case "ShopButton":
          return @"<b>Shop</b>

Purchase upgrades with your money.
          ";
        case "SkillsButton":
          return @"<b>Skills</b>

Check out your skills, what they do, and select 1 to level up.

Gain experience by breaking rocks.
          ";

        default:
          return "NOT_IMPL";
      }
    }

  }

}