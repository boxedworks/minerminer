using UnityEngine;

namespace Controllers
{

  //
  public class MineBoxMenuController : BoxMenuController
  {

    //
    public static MineBoxMenuController s_Singleton;

    public enum MenuType
    {

      NONE,

      MINE,
      FORGE,

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
      return "NOT_IMPL";
    }

  }
}