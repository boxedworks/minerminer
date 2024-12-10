using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Net.Security;

namespace Controllers
{

  public class HammerController : IHasInfoables
  {

    //
    public static HammerController s_Singleton;

    MachineController _hammerController;
    RecipeType _currentRecipe { get { return (RecipeType)_hammerController._CurrentRecipe; } }
    public enum RecipeType
    {
      NONE,

      STONE_DUST,

      GEM_DUST_0,

      MIX_0,
      MIX_1,
    }

    //
    Transform _menu, _menuUi, _dependencies;

    Transform _hammerModel;
    RectTransform _hammerUi;

    float _swingTimer, _swingTimerVisual;

    //
    public InfoData _InfoData
    {
      get
      {
        return _hammerController._InfoData;
      }
    }

    //
    public HammerController()
    {
      s_Singleton = this;

      _menu = MineBoxMenuController.s_Singleton.GetMenu(MineBoxMenuController.MenuType.HAMMER).transform;
      _menuUi = GameObject.Find("BoxMineMenu").transform;
      _dependencies = _menuUi.GetChild(0).GetChild(2);

      // Create machine
      var startButton = _dependencies.GetChild(3).GetChild(1).GetComponent<Button>();
      var setRecipeButton = _dependencies.GetChild(3).GetChild(0).GetComponent<Button>();
      var startLoopButton = _dependencies.GetChild(3).GetChild(2).GetComponent<Button>();
      var stopLoopButton = _dependencies.GetChild(3).GetChild(3).GetComponent<Button>();
      var setRecipesButton = _dependencies.GetChild(3).GetChild(4).GetComponent<Button>();
      var progressSlider = _dependencies.Find("HammerHealth").GetComponent<Slider>();
      var recipeMenu = _dependencies.Find("RecipeMenu").gameObject;

      List<Button> recipeButtons = new();
      var buttonContainer = recipeMenu.transform.GetChild(0);
      for (var i = 0; i < 10; i++)
      {
        var button = (i < 5 ? buttonContainer.GetChild(0).GetChild(i) : buttonContainer.GetChild(1).GetChild(i - 5))
          .GetChild(0).GetComponent<Button>();
        recipeButtons.Add(button);
      }

      var inputNodes = new MachineController.CraftingNode[]{
        new(_dependencies.GetChild(1).GetChild(1).GetComponent<Image>(), _dependencies.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>()),
        new(_dependencies.GetChild(0).GetChild(1).GetComponent<Image>(), _dependencies.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>()),
      };
      var outputNodes = new MachineController.CraftingNode[]{
        new(_dependencies.GetChild(2).GetChild(1).GetChild(0).GetComponent<Image>(), _dependencies.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>()),
      };
      outputNodes[0].RegisterButton(_dependencies.GetChild(2).GetChild(1).GetComponent<Button>());

      _hammerController = new(
        recipeButtons,
        startButton,
        setRecipeButton,
        setRecipesButton,
        recipeMenu,
        progressSlider,

        inputNodes,
        outputNodes
      );
      _hammerController._CookSpeedAction = () => { return 1f; };
      _hammerController.RegisterLoopButtons(startLoopButton, stopLoopButton);
      _hammerController.EnableLoopButtons();
      _hammerController._CookOverTime = false;

      UnlockRecipe(RecipeType.STONE_DUST);
      UnlockRecipe(RecipeType.GEM_DUST_0);

      //
      _hammerModel = _menu.Find("Hammer").transform;
      //_hammerUi = GameObject.Find("PicBox").transform as RectTransform;
    }

    //
    public void Update()
    {

      _hammerController.Update();

      //
      _swingTimerVisual += (_swingTimer - _swingTimerVisual) * Time.deltaTime * (_swingTimer < _swingTimerVisual ? 50f : 10f);
      _hammerModel.localPosition = Vector3.Lerp(new Vector3(0f, 0.5f, 100f), new Vector3(0f, 1.5f, 100f), _swingTimerVisual);

      //
      _swingTimer += Time.deltaTime * 1f * 0.25f * GameController.s_GameSpeedMod;
      if (_swingTimer >= 1f && !_hammerController._IsCooking) { _swingTimer = 1f; return; }
      while (_swingTimer >= 1f)
      {
        _swingTimer -= 1f;

        //
        _hammerController.InstantCook();

        ParticleController.PlayParticles(ParticleController.ParticleType.HAMMER_SPARKS);

        if (MineBoxMenuController.s_Singleton.IsVisible(MineBoxMenuController.MenuType.HAMMER))
          AudioController.PlayAudio("HammerHit");
      }

    }

    //
    public static void UnlockRecipe(RecipeType recipeType)
    {

      switch (recipeType)
      {

        //
        case RecipeType.STONE_DUST:

          s_Singleton._hammerController.UnlockRecipe(
            (int)recipeType,
            "Stone Dust",
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.STONE, 15)
            },
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.STONE_DUST, 5)
            }
          );

          break;

        //
        case RecipeType.GEM_DUST_0:

          s_Singleton._hammerController.UnlockRecipe(
            (int)recipeType,
            "Citrine Dust",
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.CITRINE, 2),
              (InventoryController.ItemType.DIAMOND, 1),
            },
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.GEM_DUST_0, 2)
            }
          );

          break;

        //
        case RecipeType.MIX_0:

          s_Singleton._hammerController.UnlockRecipe(
            (int)recipeType,
            "Orange Mix",
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.GEM_DUST_0, 1),
              (InventoryController.ItemType.STONE_CHUNK, 10),
            },
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.MIX_0, 1)
            }
          );

          break;

        //
        case RecipeType.MIX_1:

          s_Singleton._hammerController.UnlockRecipe(
            (int)recipeType,
            "Dual Mix",
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.COBALT, 10),
              (InventoryController.ItemType.CINNABAR, 10),
            },
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.MIX_1, 1)
            }
          );

          break;

      }

    }

    //
    public static MachineController.SaveInfo GetSaveInfo()
    {
      return s_Singleton._hammerController.GetSaveInfo();
    }
    public static void SetSaveInfo(MachineController.SaveInfo saveInfo)
    {
      s_Singleton._hammerController.SetSaveInfo(saveInfo);
    }

  }

}