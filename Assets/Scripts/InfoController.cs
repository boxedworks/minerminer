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
    LineRenderer _mouseLineRenderer;

    //
    public InfoController()
    {
      s_Singleton = this;

      //
      _infoText = InfoBoxMenuController.s_Singleton.GetMenu(InfoBoxMenuController.MenuType.INFO).transform.Find("InfoText").GetComponent<TMPro.TextMeshProUGUI>();
      _mouseLineRenderer = GameObject.Find("MouseInfo").GetComponent<LineRenderer>();
    }

    //
    IInfoable _defaultInfo, _lastInfo;
    public void Update()
    {
      var hooveredObj = GatherDescriptionObjectName(Input.mousePosition);
      if (hooveredObj == null)
      {
        if (_lastInfo != null && _lastInfo._Transform != null && _lastInfo._Transform.gameObject != null && _lastInfo._Transform.gameObject.activeInHierarchy)
        {
          SetInfoText(_lastInfo._Info);
          SetInfoPointer(_lastInfo);
        }
        else if (_defaultInfo != null && _defaultInfo._Transform != null && _defaultInfo._Transform.gameObject != null && _defaultInfo._Transform.gameObject.activeInHierarchy)
        {
          SetInfoText(_defaultInfo._Info);
          SetInfoPointer(_defaultInfo);
        }
        else
        {
          SetInfoText("");
          SetInfoPointer(null);
        }
      }
      else
      {
        _lastInfo = hooveredObj;

        SetInfoText(hooveredObj._Info);
        SetInfoPointer(hooveredObj);
      }
    }

    //
    void SetInfoPointer(IInfoable ofInfoable)
    {
      if (ofInfoable == null || ofInfoable._Transform == null)
      {
        _mouseLineRenderer.SetPositions(new Vector3[] { new Vector3(1000f, 0f, 0f), new Vector3(1000f, 0f, 1f) });
        return;
      }

      var infoButtonPosition = InfoBoxMenuController.GetInfoButton().transform.position;

      var hoverLocalPos = ofInfoable._Transform.rect.center;
      if (infoButtonPosition.x > ofInfoable._Transform.position.x)
        hoverLocalPos.x += ofInfoable._Transform.rect.width * 0.5f;
      else
        hoverLocalPos.x -= ofInfoable._Transform.rect.width * 0.5f;
      if (infoButtonPosition.y > ofInfoable._Transform.position.y)
        hoverLocalPos.y += ofInfoable._Transform.rect.height * 0.5f;
      else
        hoverLocalPos.y -= ofInfoable._Transform.rect.height * 0.5f;

      var hoverPosition = ofInfoable._Transform.TransformPoint(hoverLocalPos);

      infoButtonPosition.z = 0f;
      hoverPosition.z = 0f;
      _mouseLineRenderer.SetPositions(new Vector3[] { infoButtonPosition, hoverPosition });
    }

    //
    public IInfoable GatherDescriptionObjectName(Vector2 mousePos)
    {

      var checkInfos = new List<IHasInfoables>()
      {
        MainBoxMenuController.s_Singleton,
        MineBoxMenuController.s_Singleton,

        InfoBoxMenuController.s_Singleton,
      };
      var defaultInfo = MainBoxMenuController.s_Singleton._InfoData._DefaultInfo;

      //
      if (defaultInfo != null)
        _defaultInfo = defaultInfo;

      //
      foreach (var checkInfo in checkInfos)
        foreach (var info in checkInfo._InfoData._Infos)
          if (info._Transform != null && info._Transform.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(info._Transform, mousePos, Camera.main))
            return info;

      return null;
    }

    //
    public static void SetInfoText(string text)
    {
      s_Singleton._infoText.text = text;
    }

    //
    public static string GetInfoString(string title, string desc)
    {
      return $@"<b><size=45>{title}
======</size></b>

{desc}
      ";
    }
  }

}