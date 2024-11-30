using UnityEngine;
using UnityEngine.UI;

namespace Controllers
{

  //
  public class MainBoxMenuController : BoxMenuController, IHasInfoables
  {

    //
    public static MainBoxMenuController s_Singleton;

    MenuType _currentMenu { get { return (MenuType)_currentMenuIndex; } }
    public enum MenuType
    {

      NONE,

      INVENTORY,
      SHOP,
      SKILLS,

      PRESTIGE,

      OPTIONS,
    }

    //
    public InfoData _InfoData
    {
      get
      {
        InfoData returnData;

        switch (_currentMenu)
        {
          case MenuType.SHOP:
            returnData = ShopController.s_Singleton._InfoData;
            break;
          case MenuType.SKILLS:
            returnData = SkillController.s_Singleton._InfoData;
            break;
          case MenuType.INVENTORY:
            returnData = InventoryController.s_Singleton._InfoData;
            break;

          case MenuType.PRESTIGE:
            returnData = PrestigeController.s_Singleton._InfoData;
            break;

          case MenuType.OPTIONS:
            returnData = InventoryController.s_Singleton._InfoData;
            break;

          //
          default:
            returnData = SkillController.s_Singleton._InfoData;
            break;
        }

        //
        foreach (var dependency in _dependencyInfos)
          returnData._Infos.Add(dependency);

        //
        return returnData;
      }
    }

    //
    public MainBoxMenuController()
    {
      s_Singleton = this;

      //
      var menuOrderString = new string[]{
        MenuType.NONE.ToString(),

        MenuType.INVENTORY.ToString(),
        MenuType.SHOP.ToString(),
        MenuType.SKILLS.ToString(),

        MenuType.PRESTIGE.ToString(),

        MenuType.OPTIONS.ToString()
      };
      var buttons = GetChildrenAsList(GameObject.Find("BoxStatsButtons").transform.GetChild(0));
      var optionsButton = GameObject.Find("OptionsButton");
      buttons.Add(optionsButton);
      SetUpMenus(buttons, menuOrderString);

      //
      var discordButton = GameObject.Find("DiscordButton").GetComponent<Button>();
      discordButton.onClick.AddListener(() =>
      {
        Application.OpenURL("https://discord.gg/dzjj5gTyZc");

        AudioController.PlayAudio("MenuSelect");
      });
      _dependencyInfos.Add(new SimpleInfoable()
      {
        _GameObject = discordButton.gameObject,
        _Description = InfoController.GetInfoString("Open Discord", $"Join the discord channel for {StringController.s_GAME_NAME}!")
      });

      //
      var saveButton = GameObject.Find("SaveButton").GetComponent<Button>();
      saveButton.onClick.AddListener(() =>
      {
        SaveController.Save();

        AudioController.PlayAudio("MenuSelect");
      });
      _dependencyInfos.Add(new SimpleInfoable()
      {
        _GameObject = saveButton.gameObject,
        _Description = InfoController.GetInfoString("Save [Insert]", $"Save your game progress.")
      });

      //
      var optionsButton_ = optionsButton.GetComponent<Button>();
      optionsButton_.onClick.RemoveAllListeners();
      optionsButton_.onClick.AddListener(() =>
      {
        if (IsVisible(MenuType.OPTIONS))
          OptionsController.PressMenuButton();
        else
          SetMenuType(MenuType.OPTIONS);
        AudioController.PlayAudio("MenuSelect");
      });

      //
      SetMenuActive(MenuType.INVENTORY, true, false);
      SetMenuActive(MenuType.OPTIONS, true, false);
      SetMenuType(MenuType.INVENTORY);
    }

    public GameObject GetMenu(MenuType ofMenu)
    {
      return _menus[(int)ofMenu];
    }

    public bool IsVisible(MenuType ofMenu)
    {
      return IsVisible((int)ofMenu);
    }
    public void SetMenuActive(MenuType menuType, bool toggle, bool useNotify, string notifyText = null)
    {
      SetMenuActive((int)menuType, toggle, useNotify, notifyText);
    }
    public bool IsMenuActive(MenuType menuType)
    {
      return IsMenuActive((int)menuType);
    }

    public void SetMenuType(MenuType menuType)
    {
      SetMenuType((int)menuType);
    }

    //
    protected override void AfterMenuSwitch(int fromMenuIndex, int toMenuIndex) { AfterMenuSwitch((MenuType)fromMenuIndex, (MenuType)toMenuIndex); }
    void AfterMenuSwitch(MenuType fromMenu, MenuType toMenu)
    {
      switch (toMenu)
      {
        case MenuType.OPTIONS:

          OptionsController.OnSwitch(fromMenu);
          break;
      }
    }

    //
    protected override string GetButtonDescription(string buttonName)
    {
      switch (buttonName)
      {

        case "InventoryButton":
          return InfoController.GetInfoString("Inventory [I]", "Browse and sell your items.");
        case "ShopButton":
          return InfoController.GetInfoString("Upgrades [U]", "Purchase upgrades with money and items.");
        case "SkillsButton":
          return InfoController.GetInfoString("Skills [S]", @"Check out your skills, what they do, and select 1 to level up.

Gain experience by breaking rocks.");

        case "PrestigeButton":
          return InfoController.GetInfoString("Prestige [P]", "Gain prestige levels by restarting the game with modifiers!");

        case "OptionsButton":
          return InfoController.GetInfoString("Options [Esc]", "View game options.");


        default:
          return "NOT_IMPL";
      }
    }

  }

}