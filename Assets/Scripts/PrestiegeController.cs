using UnityEngine;

using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Controllers
{

  public class PrestiegeController : IHasInfoables
  {

    int _prestiegeLevel, _maxLevelRequirement;
    bool _justPrestieged;

    Button _prestiegeButton;
    TMPro.TextMeshProUGUI _requiredTotalLevelText, _currentPrestiegeText;
    bool _prestiegeMet;

    //
    public InfoData _InfoData
    {
      get
      {
        var returnData = new InfoData();

        var returnList = new List<IInfoable>();
        returnData._Infos = returnList;

        return returnData;
      }
    }

    //
    public static PrestiegeController s_Singleton;
    public PrestiegeController()
    {
      s_Singleton = this;

      _currentPrestiegeText = GameObject.Find($"PrestiegeLevel").transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();

      var prestiegeNextContainer = GameObject.Find($"PrestiegeNext");
      _requiredTotalLevelText = prestiegeNextContainer.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();

      // Prestiege
      _prestiegeButton = prestiegeNextContainer.transform.Find($"NextPrestiegeButton").GetComponent<Button>();
      _prestiegeButton.onClick.AddListener(() =>
      {
        AudioController.PlayAudio("MenuSelect");

        if (_prestiegeMet)
        {

          // Save prestiege level
          _prestiegeLevel++;
          _justPrestieged = true;

          // Reload scene
          SaveController.Save();
          SceneManager.LoadScene(0);
        }
        else
        {
          LogController.AppendLog($"<color=red>Total level not high enough to prestiege!</color>");
        }
      });
    }

    //
    static void OnPrestiege(int prestiegeLevel)
    {
      s_Singleton._prestiegeLevel = prestiegeLevel;
      s_Singleton._currentPrestiegeText.text = $"Prestiege level: {s_Singleton._prestiegeLevel}";

      var menu = MainBoxMenuController.s_Singleton.GetMenu(MainBoxMenuController.MenuType.PRESTIEGE).transform;
      var maxLevelRequirement = 250;
      for (var i = 1; i < prestiegeLevel + 1; i++)
      {

        // Set prestiege attributes
        switch (i)
        {

          // Prestiege 1; triple XP rewards
          case 1:

            SkillController.s_XpMultiplier += 2f;

            break;

          // Prestiege 2; double rock drops
          case 2:

            RockController.s_DropMultiplier += 1f;

            break;

        }

        //
        maxLevelRequirement += 50;

        // Show prestiege enabled
        var menuEntry = menu.GetChild(1 + i);
        var entryImage = menuEntry.GetChild(2).GetComponent<Image>();
        var buttonText = entryImage.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();

        entryImage.color = StringController.s_ColorGreen;
        buttonText.text = "Active";
      }

      //
      s_Singleton._maxLevelRequirement = maxLevelRequirement;

      //
      if (prestiegeLevel > 0)
        MainBoxMenuController.s_Singleton.SetMenuActive(MainBoxMenuController.MenuType.PRESTIEGE, true, false);
    }

    //
    public static void OnTotalLevelUpdate(int toTotalLevel)
    {
      var prestiegeMet = toTotalLevel >= s_Singleton._maxLevelRequirement;
      if (prestiegeMet && s_Singleton._prestiegeLevel > 1) prestiegeMet = false; // Max prestiege (for now)
      s_Singleton._prestiegeMet = prestiegeMet;

      var prestiegeColor = prestiegeMet ? "green" : "red";
      s_Singleton._requiredTotalLevelText.text = $"Total level requirement - <color={prestiegeColor}>{toTotalLevel} / {s_Singleton._maxLevelRequirement}</color>";
      s_Singleton._prestiegeButton.image.color = prestiegeMet ? StringController.s_ColorGreen : StringController.s_ColorRed;
    }

    //
    [System.Serializable]
    public class SaveInfo
    {
      public int PrestiegeLevel;
      public bool FreshPrestiege;
    }
    public static SaveInfo GetSaveInfo()
    {
      var saveInfo = new SaveInfo();

      saveInfo.PrestiegeLevel = s_Singleton._prestiegeLevel;
      saveInfo.FreshPrestiege = s_Singleton._justPrestieged;

      return saveInfo;
    }
    public static void SetSaveInfo(SaveInfo saveInfo)
    {
      OnPrestiege(saveInfo.PrestiegeLevel);

      if (saveInfo.FreshPrestiege)
        MainBoxMenuController.s_Singleton.SetMenuType(MainBoxMenuController.MenuType.PRESTIEGE);
    }
  }

}