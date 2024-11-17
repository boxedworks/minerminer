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
      var progressSlider = _dependencies.Find("HammerHealth").GetComponent<Slider>();
      var recipeMenu = _dependencies.Find("RecipeMenu").gameObject;

      List<Button> recipeButtons = new();
      var buttonContainer = recipeMenu.transform;
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
        recipeMenu,
        progressSlider,

        inputNodes,
        outputNodes
      );
      _hammerController.RegisterLoopButtons(startLoopButton, stopLoopButton);
      _hammerController._CookOverTime = false;
      UnlockRecipe(RecipeType.STONE_DUST);

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
      _hammerModel.localPosition = Vector3.Lerp(new Vector3(0f, 0.5f, 0f), new Vector3(0f, 1.5f, 0f), _swingTimerVisual);

      //
      _swingTimer += Time.deltaTime * 1 * 0.25f;
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
              (InventoryController.ItemType.STONE, 5)
            },
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.STONE_DUST, 10)
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