using UnityEngine;

using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Controllers
{

  public class PrestigeController : IHasInfoables
  {

    int _prestigeLevel, _maxLevelRequirement;
    bool _justPrestiged;

    Button _prestigeButton;
    TMPro.TextMeshProUGUI _requiredTotalLevelText, _currentPrestigeText;
    bool _prestigeMet;

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
    public static PrestigeController s_Singleton;
    public PrestigeController()
    {
      s_Singleton = this;

      _currentPrestigeText = GameObject.Find($"PrestigeLevel").transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();

      var prestigeNextContainer = GameObject.Find($"PrestigeNext");
      _requiredTotalLevelText = prestigeNextContainer.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();

      // Prestige
      _prestigeButton = prestigeNextContainer.transform.Find($"NextPrestigeButton").GetComponent<Button>();
      _prestigeButton.onClick.AddListener(() =>
      {
        AudioController.PlayAudio("MenuSelect");

        if (_prestigeMet)
        {

          // Save prestige level
          _prestigeLevel++;
          _justPrestiged = true;

          // Reload scene
          SaveController.Save();
          SceneManager.LoadScene(0);
        }
        else
        {
          LogController.AppendLog($"<color=red>Total level not high enough to prestige!</color>");
        }
      });
    }

    //
    static void OnPrestige(int prestigeLevel)
    {
      s_Singleton._prestigeLevel = prestigeLevel;
      s_Singleton._currentPrestigeText.text = $"Prestige level: {s_Singleton._prestigeLevel}";

      var menu = MainBoxMenuController.s_Singleton.GetMenu(MainBoxMenuController.MenuType.PRESTIGE).transform;
      var maxLevelRequirement = 250;
      for (var i = 1; i < prestigeLevel + 1; i++)
      {

        // Set prestige attributes
        switch (i)
        {

          // Prestige 1; triple XP rewards
          case 1:

            SkillController.s_XpMultiplier += 2f;

            break;

          // Prestige 2; double rock drops
          case 2:

            RockController.s_DropMultiplier += 1f;

            break;

        }

        //
        maxLevelRequirement += 50;

        // Show prestige enabled
        var menuEntry = menu.GetChild(1 + i);
        var entryImage = menuEntry.GetChild(2).GetComponent<Image>();
        var buttonText = entryImage.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();

        entryImage.color = StringController.s_ColorGreen;
        buttonText.text = "Active";
      }

      //
      s_Singleton._maxLevelRequirement = maxLevelRequirement;

      //
      if (prestigeLevel > 0)
        MainBoxMenuController.s_Singleton.SetMenuActive(MainBoxMenuController.MenuType.PRESTIGE, true, false);
    }

    //
    public static void OnTotalLevelUpdate(int toTotalLevel)
    {
      var prestigeMet = toTotalLevel >= s_Singleton._maxLevelRequirement;
      if (prestigeMet && s_Singleton._prestigeLevel > 1) prestigeMet = false; // Max prestige (for now)
      s_Singleton._prestigeMet = prestigeMet;

      var prestigeColor = prestigeMet ? "green" : "red";
      s_Singleton._requiredTotalLevelText.text = $"Total level requirement - <color={prestigeColor}>{toTotalLevel} / {s_Singleton._maxLevelRequirement}</color>";
      s_Singleton._prestigeButton.image.color = prestigeMet ? StringController.s_ColorGreen : StringController.s_ColorRed;
    }

    //
    [System.Serializable]
    public class SaveInfo
    {
      public int PrestigeLevel;
      public bool FreshPrestige;
    }
    public static SaveInfo GetSaveInfo()
    {
      var saveInfo = new SaveInfo();

      saveInfo.PrestigeLevel = s_Singleton._prestigeLevel;
      saveInfo.FreshPrestige = s_Singleton._justPrestiged;

      return saveInfo;
    }
    public static void SetSaveInfo(SaveInfo saveInfo)
    {
      OnPrestige(saveInfo.PrestigeLevel);

      if (saveInfo.FreshPrestige)
        MainBoxMenuController.s_Singleton.SetMenuType(MainBoxMenuController.MenuType.PRESTIGE);
    }
  }

}