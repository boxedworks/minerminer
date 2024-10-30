using UnityEngine;
using System.Collections.Generic;

namespace Controllers
{

  public class InfoController
  {

    //
    public static InfoController s_Singleton;

    //
    TMPro.TextMeshProUGUI _infoText;

    //
    public InfoController()
    {
      s_Singleton = this;

      //
      _infoText = InfoBoxMenuController.s_Singleton.GetMenu(InfoBoxMenuController.MenuType.INFO).transform.Find("InfoText").GetComponent<TMPro.TextMeshProUGUI>();
    }

        //
    IInfoable _defaultInfo;
    public void Update()
    {
      var hooveredObj = GatherDescriptionObjectName(Input.mousePosition);
      if (hooveredObj == null)
      {
        if (_defaultInfo != null)
          SetInfoText(_defaultInfo._Info);
        else
          SetInfoText("");
      }
      else
      {
        _defaultInfo = hooveredObj;
        SetInfoText(hooveredObj._Info);
      }
    }

    //
    public IInfoable GatherDescriptionObjectName(Vector2 mousePos)
    {

      List<IHasInfoables> checkInfos = new();
      checkInfos.Add(MainBoxMenuController.s_Singleton);

      var defaultInfo = MainBoxMenuController.s_Singleton._InfoData._DefaultInfo;

      //
      if (defaultInfo != null)
        _defaultInfo = defaultInfo;

      //
      foreach (var checkInfo in checkInfos)
        foreach (var info in checkInfo._InfoData._Infos)
          if (RectTransformUtility.RectangleContainsScreenPoint(info._Transform, mousePos))
            return info;

      return null;
    }

    //
    public static void SetInfoText(string text)
    {
      s_Singleton._infoText.text = text;
    }
  }

}