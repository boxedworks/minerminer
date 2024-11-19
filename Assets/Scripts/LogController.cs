using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Controllers
{

  public class LogController
  {

    public static LogController s_Singleton;

    Transform _menu;
    TMPro.TextMeshProUGUI _logText;

    Queue<string> _log;

    public LogController()
    {
      s_Singleton = this;

      _menu = GameObject.Find("logMenu").transform;//InfoBoxMenuController.s_Singleton.GetMenu(InfoBoxMenuController.MenuType.LOG).transform;
      _logText = _menu.Find("logText").GetComponent<TMPro.TextMeshProUGUI>();

      _log = new();
      AppendLog($"Welcome to {StringController.s_GAME_NAME} :).");
    }


    //
    public static void AppendLog(string text)
    {

      //
      if (SaveController.s_IsLoading)
        return;

      //
      var log = s_Singleton._log;
      log.Enqueue(text);

      if (log.Count > 21)
        log.Dequeue();

      var logList = log.ToList();
      logList.Reverse();
      var logString = string.Join("\n", logList);
      s_Singleton._logText.text = logString;
    }

  }

}