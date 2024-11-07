using UnityEngine;
using System.Collections.Generic;

namespace Controllers
{

  //
  public abstract class BoxMenuController
  {

    //
    Transform _buttonsContainer, _buttonSelectorUi;
    protected int _currentMenuIndex;

    protected Dictionary<int, GameObject> _menus;
    Dictionary<int, int> _menuIndexes;

    protected Dictionary<int, List<GameObject>> _menuDependancies;

    public BoxMenuController()
    {
      _menuDependancies = new();
    }

    protected List<IInfoable> _dependencyInfos;

    public void SetUpMenus(Transform buttonsContainer, string[] enumOrder)
    {

      //
      _buttonsContainer = buttonsContainer;
      _menus = new();
      _menuIndexes = new();
      _dependencyInfos = new();
      for (var i = 0; i < _buttonsContainer.childCount; i++)
      {
        var button = _buttonsContainer.GetChild(i).GetComponent<UnityEngine.UI.Button>();
        if (i == 0)
          _buttonSelectorUi = button.transform.GetChild(0);

        var menuType = button.gameObject.name[0..(button.gameObject.name.Length - 6)];
        var menuIndex = System.Array.IndexOf(enumOrder, menuType.ToUpper());

        _menuIndexes.Add(menuIndex, i);
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
      return _buttonsContainer.GetChild(_menuIndexes[ofMenuIndex]).gameObject;
    }

    //
    protected void SetMenuType(int toMenuIndex)
    {
      if (_currentMenuIndex != 0)
        ToggleMenu(_currentMenuIndex, false);
      _currentMenuIndex = toMenuIndex;
      ToggleMenu(_currentMenuIndex, true);

      _buttonSelectorUi.SetParent(GetMenuButton(toMenuIndex).transform, false);
      _buttonSelectorUi.SetAsFirstSibling();
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
      var button = GetMenuButton(ofMenuIndex);
      if (toggle)
      {
        if (useNotify)
          UpgradeController.NotifyUnlockButton(button, notifyText);
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
    protected abstract string GetButtonDescription(string buttonName);

  }

}