using UnityEngine;

namespace Controllers
{

  public class OptionsController
  {

    Transform _menu;
    bool _menuIsVisible { get { return MainBoxMenuController.s_Singleton.IsVisible(MainBoxMenuController.MenuType.OPTIONS); } }

    //
    SaveInfo _SaveInfo;
    MainBoxMenuController.MenuType _lastMenu;

    TMPro.TextMeshProUGUI _volumeText;
    public static int s_Volume
    {
      get
      {
        return s_Singleton._SaveInfo.Volume;
      }
      set
      {
        s_Singleton._SaveInfo.Volume = value;
        s_Singleton._volumeText.text = $"Volume: {value}";
      }
    }

    //
    TMPro.TextMeshProUGUI _gameVersionText;
    public static void SetGameVersion()
    {
      s_Singleton._gameVersionText.text = $"Game version: {GameController.s_GameVersion}";
    }

    //
    public static OptionsController s_Singleton;
    public OptionsController()
    {
      s_Singleton = this;

      //
      _menu = MainBoxMenuController.s_Singleton.GetMenu(MainBoxMenuController.MenuType.OPTIONS).transform;

      //
      _SaveInfo = new();

      //
      _volumeText = _menu.Find($"SfxOption").GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
      s_Volume = 3;

      //
      _gameVersionText = _menu.Find("GameVersion").GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();

      // Set up buttons
      {

        // Volume
        for (var i = 0; i < 6; i++)
        {
          var volumeAmount = i;
          _menu.Find($"SfxOption/Buttons/AudioButton{i}").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
          {
            AudioController.PlayAudio("MenuSelect");

            s_Volume = volumeAmount;
          });
        }

        // Delete save
        _menu.Find("DeleteOption/DeleteSaveButton").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
        {
          AudioController.PlayAudio("MenuSelect");

          SaveController.Delete();
        });

        // Steam
        _menu.Find("SteamLink/SteamButton").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
        {
          AudioController.PlayAudio("MenuSelect");

          Application.OpenURL("https://store.steampowered.com/search/?developer=boxedworks");
        });

      }
    }

    //
    public static void OnSwitch(MainBoxMenuController.MenuType menuType)
    {
      if (menuType == MainBoxMenuController.MenuType.OPTIONS)
      {
        MainBoxMenuController.s_Singleton.SetMenuType(s_Singleton._lastMenu);
        return;
      }
      s_Singleton._lastMenu = menuType;
    }

    public static void PressMenuButton()
    {
      OnSwitch(MainBoxMenuController.MenuType.OPTIONS);
    }

    //
    [System.Serializable]
    public class SaveInfo
    {
      public int Volume;

      public string Version;
    }
    public static SaveInfo GetSaveInfo()
    {
      s_Singleton._SaveInfo.Version = GameController.s_GameVersion;

      return s_Singleton._SaveInfo;
    }
    public static void SetSaveInfo(SaveInfo saveInfo)
    {
      s_Volume = saveInfo.Volume;
    }
  }

}