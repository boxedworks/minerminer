using UnityEngine;
using System.Collections.Generic;
using Packages;

namespace Controllers
{

  public class LogController
  {

    public static LogController s_Singleton;

    Transform _menu;
    TMPro.TextMeshProUGUI _logText;

    List<string> _log;

    public LogController()
    {
      s_Singleton = this;

      _menu = InfoBoxMenuController.s_Singleton.GetMenu(InfoBoxMenuController.MenuType.LOG).transform;
      _logText = _menu.Find("logText").GetComponent<TMPro.TextMeshProUGUI>();

      _log = new();
      AppendLog("Welcome to minerminer :).");
    }


    //
    public static void AppendLog(string text)
    {

      //
      if (SaveController.s_IsLoading)
        return;

      //
      var log = s_Singleton._log;
      log.Add(text);

      if (log.Count > 17)
        log.RemoveAt(0);

      var logString = string.Join("\n", log);
      s_Singleton._logText.text = InfoController.GetInfoString("Log", logString);
    }

  }

}