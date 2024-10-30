using UnityEngine;
using System.Collections.Generic;

namespace Controllers
{

  //
  public class InfoBoxMenuController : BoxMenuController
  {

    //
    public static InfoBoxMenuController s_Singleton;

    public enum MenuType
    {

      NONE,

      INFO
    }

    //
    public InfoBoxMenuController()
    {
      s_Singleton = this;

      //

      //
      var menuOrderString = new string[]{
        MenuType.NONE.ToString(),

        MenuType.INFO.ToString(),
      };
      SetUpMenus(GameObject.Find("BoxInfoButtons").transform.GetChild(0), menuOrderString);

      //
      SetMenuType((int)MenuType.INFO);
      SetMenuActive(MenuType.INFO, true);
    }

    //
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