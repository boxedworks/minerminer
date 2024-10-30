using UnityEngine;
using System.Collections.Generic;

namespace Controllers
{

  //
  public class DamageTextController
  {

    //
    public static DamageTextController s_Singleton;

    //
    List<(TMPro.TextMeshProUGUI, float)> _texts;
    GameObject _prefab;

    //
    public DamageTextController()
    {
      s_Singleton = this;

      _texts = new();
      _prefab = GameObject.Find("DmgTexts").transform.GetChild(0).gameObject;
    }

    //
    public void Update()
    {
      for (var i = _texts.Count - 1; i >= 0; i--)
      {

        var textData = _texts[i];
        var text = textData.Item1;
        var textAliveTime = Time.time - textData.Item2;

        var textColor = text.color;
        textColor.a = 1f - textAliveTime;
        text.color = textColor;

        (text.transform as RectTransform).position += new Vector3(0f, 50f, 0f) * Time.deltaTime;

        if (textAliveTime > 1f)
        {
          GameObject.Destroy(text.gameObject);
          _texts.RemoveAt(i);
        }
      }
    }

    //
    public void AddText(float damage)
    {
      var newText = GameObject.Instantiate(_prefab, _prefab.transform.parent).GetComponent<TMPro.TextMeshProUGUI>();
      newText.text = $"{damage}";
      //(newText.transform as RectTransform).position = RockController.transform.position;

      _texts.Add((newText, Time.time));

      newText.gameObject.SetActive(true);
    }
  }

}