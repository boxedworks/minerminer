using UnityEngine;
using System.Collections.Generic;

namespace Controllers
{

  //
  public class InfoBoxMenuController : BoxMenuController, IHasInfoables
  {

    //
    public static InfoBoxMenuController s_Singleton;

    public enum MenuType
    {

      NONE,

      INFO,
      LOG
    }

    //
    public InfoData _InfoData
    {
      get
      {
        InfoData returnData = new();
        returnData._Infos = new();

        //
        foreach (var dependency in _dependencyInfos)
          returnData._Infos.Add(dependency);

        //
        return returnData;
      }
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
        MenuType.LOG.ToString(),
      };
      SetUpMenus(GameObject.Find("BoxInfoButtons").transform.GetChild(0), menuOrderString);

      //
      SetMenuType((int)MenuType.INFO);
      SetMenuActive(MenuType.INFO, true, false);
      SetMenuActive(MenuType.LOG, true, false);
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
    public void SetMenuActive(MenuType menuType, bool toggle, bool useNotify, string notifyText = null)
    {
      SetMenuActive((int)menuType, toggle, useNotify, notifyText);
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

        case "LogButton":
          return InfoController.GetInfoString("Log", "Details past events.");
        case "InfoButton":
          return InfoController.GetInfoString("Info", "Shows info on game objects.");

        default:
          return "NOT_IMPL";
      }
    }

    public static GameObject GetInfoButton()
    {
      return s_Singleton.GetMenuButton((int)MenuType.INFO);
    }
  }
}