using UnityEngine;

namespace Controllers
{

  public class OptionsController
  {

    //
    SaveInfo _SaveInfo;

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
    public static OptionsController s_Singleton;
    public OptionsController()
    {
      s_Singleton = this;

      _SaveInfo = new();

      //
      _volumeText = GameObject.Find($"SfxOption").transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
      s_Volume = 3;

      // Set up buttons
      {

        // Volume
        for (var i = 0; i < 6; i++)
        {
          var volumeAmount = i;
          GameObject.Find($"AudioButton{i}").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
          {
            AudioController.PlayAudio("MenuSelect");

            s_Volume = volumeAmount;
          });
        }

        // Delete save
        GameObject.Find("DeleteSaveButton").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
        {
          AudioController.PlayAudio("MenuSelect");

          SaveController.Delete();
        });

        // Steam
        GameObject.Find("SteamButton").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
        {
          AudioController.PlayAudio("MenuSelect");

          Application.OpenURL("https://store.steampowered.com/search/?developer=boxedworks");
        });

      }
    }

    //
    [System.Serializable]
    public class SaveInfo
    {
      public int Volume;
    }
    public static SaveInfo GetSaveInfo()
    {
      return s_Singleton._SaveInfo;
    }
    public static void SetSaveInfo(SaveInfo saveInfo)
    {
      s_Volume = saveInfo.Volume;
    }
  }

}