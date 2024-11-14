using UnityEngine;
using System.Collections.Generic;

namespace Controllers
{

  //
  public abstract class BoxMenuController
  {

    //
    List<GameObject> _buttons;
    Transform _buttonSelectorUi;
    protected int _currentMenuIndex;

    protected Dictionary<int, GameObject> _menus;

    protected Dictionary<int, List<GameObject>> _menuDependancies;

    public BoxMenuController()
    {
      _menuDependancies = new();
    }

    protected List<IInfoable> _dependencyInfos;

    public void SetUpMenus(List<GameObject> buttons, string[] enumOrder)
    {

      //
      _buttons = buttons;
      _menus = new();
      _dependencyInfos = new();
      for (var i = 0; i < _buttons.Count; i++)
      {
        var button = _buttons[i].GetComponent<UnityEngine.UI.Button>();
        if (i == 0)
          _buttonSelectorUi = button.transform.GetChild(0);

        var menuType = button.gameObject.name[0..(button.gameObject.name.Length - 6)];
        var menuIndex = System.Array.IndexOf(enumOrder, menuType.ToUpper());

        SetMenuActive(menuIndex, false, false);

        button.onClick.AddListener(() =>
        {
          SetMenuType(menuIndex);

          AudioController.PlayAudio("MenuSelect");
        });

        //
        var menu = GameObject.Find($"{enumOrder[menuIndex].ToLower()}Menu");
        _menus.Add(menuIndex, menu);
        ToggleMenu(menuIndex, false);

        //
        _dependencyInfos.Add(new SimpleInfoable()
        {
          _GameObject = button.gameObject,

          _Description = GetButtonDescription(button.gameObject.name)
        });
      }
    }

    //
    public GameObject GetMenu(int ofMenuIndex)
    {
      return _menus[ofMenuIndex];
    }
    protected GameObject GetMenuButton(int ofMenuIndex)
    {
      return _buttons[ofMenuIndex - 1];
    }

    //
    protected void SetMenuType(int toMenuIndex)
    {
      var saveMenuIndex = _currentMenuIndex;
      if (_currentMenuIndex != 0)
        ToggleMenu(_currentMenuIndex, false);
      _currentMenuIndex = toMenuIndex;
      ToggleMenu(_currentMenuIndex, true);

      _buttonSelectorUi.SetParent(GetMenuButton(toMenuIndex).transform, false);
      _buttonSelectorUi.SetAsFirstSibling();

      AfterMenuSwitch(saveMenuIndex, _currentMenuIndex);
    }

    //
    void ToggleMenu(int menuIndex, bool toggle)
    {
      GetMenu(menuIndex).SetActive(toggle);
      if (_menuDependancies.ContainsKey(menuIndex))
        foreach (var dependancy in _menuDependancies[menuIndex])
          dependancy.SetActive(toggle);
    }

    //
    protected bool IsVisible(int ofMenuIndex)
    {
      return GetMenu(ofMenuIndex).gameObject.activeSelf;
    }

    protected void SetMenuActive(int ofMenuIndex, bool toggle, bool useNotify, string notifyText = null)
    {
      // Check loading game + notify
      if (useNotify && SaveController.s_IsLoading)
        useNotify = false;

      //
      var button = GetMenuButton(ofMenuIndex);
      if (toggle)
      {
        if (useNotify)
          UnlockController.NotifyUnlockButton(button, notifyText);
        else
          button.SetActive(toggle);
      }
      else
        button.SetActive(toggle);
    }
    protected bool IsMenuActive(int ofMenuIndex)
    {
      return GetMenuButton(ofMenuIndex).activeSelf;
    }

    //
    protected void SetMenuDependancy(int ofMenuIndex, GameObject forDependancy)
    {
      if (!_menuDependancies.ContainsKey(ofMenuIndex))
        _menuDependancies.Add(ofMenuIndex, new());
      _menuDependancies[ofMenuIndex].Add(forDependancy);
    }

    //
    protected virtual void AfterMenuSwitch(int fromMenuIndex, int toMenuIndex) { }

    //
    protected abstract string GetButtonDescription(string buttonName);


    //
    protected static List<GameObject> GetChildrenAsList(Transform parent)
    {
      var returnList = new List<GameObject>();
      for (var i = 0; i < parent.childCount; i++)
        returnList.Add(parent.GetChild(i).gameObject);
      return returnList;
    }
  }

}