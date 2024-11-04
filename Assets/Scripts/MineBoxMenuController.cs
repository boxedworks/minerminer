using UnityEngine;

namespace Controllers
{

  //
  public class MineBoxMenuController : BoxMenuController, IHasInfoables
  {

    //
    public static MineBoxMenuController s_Singleton;

    MenuType _currentMenu { get { return (MenuType)_currentMenuIndex; } }
    public enum MenuType
    {

      NONE,

      MINE,
      FORGE,

    }

    //
    public InfoData _InfoData
    {
      get
      {
        InfoData returnData;

        switch (_currentMenu)
        {
          case MenuType.MINE:
            returnData = RockController.s_Singleton._InfoData;
            break;
          case MenuType.FORGE:
            returnData = ForgeController.s_Singleton._InfoData;
            break;

          //
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
    public MineBoxMenuController()
    {
      s_Singleton = this;

      //
      SetMenuDependancy((int)MenuType.MINE, GameObject.Find("MineDependancies"));
      SetMenuDependancy((int)MenuType.FORGE, GameObject.Find("ForgeDependancies"));

      //
      var menuOrderString = new string[]{
        MenuType.NONE.ToString(),

        MenuType.MINE.ToString(),
        MenuType.FORGE.ToString(),
      };
      SetUpMenus(GameObject.Find("BoxMineButtons").transform.GetChild(0), menuOrderString);

      //
      SetMenuType((int)MenuType.MINE);
      SetMenuActive(MenuType.MINE, true);
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

        case "MineButton":
          return InfoController.GetInfoString("Mine", "Mine rocks to get resources.");
        case "ForgeButton":
          return InfoController.GetInfoString("Forge", "Use resources to create more resources.");

        default:
          return "NOT_IMPL";
      }
    }

  }
}