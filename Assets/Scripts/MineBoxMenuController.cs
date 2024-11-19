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
      HAMMER,

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
          case MenuType.HAMMER:
            returnData = HammerController.s_Singleton._InfoData;
            break;

          //
          default:
            returnData = SkillController.s_Singleton._InfoData;
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
      SetMenuDependancy((int)MenuType.HAMMER, GameObject.Find("HammerDependancies"));

      //
      var menuOrderString = new string[]{
        MenuType.NONE.ToString(),

        MenuType.MINE.ToString(),
        MenuType.FORGE.ToString(),
        MenuType.HAMMER.ToString(),
      };
      var buttons = GetChildrenAsList(GameObject.Find("BoxMineButtons").transform.GetChild(0));
      SetUpMenus(buttons, menuOrderString);

      //
      SetMenuType(MenuType.MINE);
      SetMenuActive(MenuType.MINE, true, false);
    }

    public GameObject GetMenu(MenuType ofMenu)
    {
      return _menus[(int)ofMenu];
    }

    public bool IsVisible(MenuType ofMenu)
    {
      return IsVisible((int)ofMenu);
    }
    public void SetMenuActive(MenuType menuType, bool toggle, bool useNotify, string notifyText = null)
    {
      SetMenuActive((int)menuType, toggle, useNotify, notifyText);
    }
    public bool IsMenuActive(MenuType menuType)
    {
      return IsMenuActive((int)menuType);
    }

    public void SetMenuType(MenuType menuType)
    {
      SetMenuType((int)menuType);
    }

    //
    protected override string GetButtonDescription(string buttonName)
    {
      switch (buttonName)
      {

        case "MineButton":
          return InfoController.GetInfoString("Mine [M]", "Mine rocks to get resources.");
        case "ForgeButton":
          return InfoController.GetInfoString("Forge [F]", "Use resources to create more resources.");
        case "HammerButton":
          return InfoController.GetInfoString("Hammer [H]", "Use resources to create more resources.");

        default:
          return "NOT_IMPL";
      }
    }

  }
}