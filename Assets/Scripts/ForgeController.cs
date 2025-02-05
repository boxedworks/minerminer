using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Net.Security;

namespace Controllers
{

  public class ForgeController : IHasInfoables
  {

    //
    public static ForgeController s_Singleton;

    //
    Transform _menu, _menuUi, _dependencies;
    bool _menuIsVisible { get { return MineBoxMenuController.s_Singleton.IsVisible(MineBoxMenuController.MenuType.FORGE); } }

    //
    MachineController _forgeController;
    RecipeType _currentRecipe { get { return (RecipeType)_forgeController._CurrentRecipe; } }
    public enum RecipeType
    {
      NONE,

      BRICK,

      COPPER_INGOT,
      BRONZE_INGOT,
      IRON_INGOT,
      STEEL_INGOT,
      CITRINE_INGOT,
      DUAL_INGOT,
    }

    //
    AudioSource _forgeLoopAudio;
    float _menuVisibleTime;

    //
    public InfoData _InfoData
    {
      get
      {
        return _forgeController._InfoData;
      }
    }

    //
    public ForgeController()
    {
      s_Singleton = this;

      _menu = MineBoxMenuController.s_Singleton.GetMenu(MineBoxMenuController.MenuType.FORGE).transform;
      _menuUi = GameObject.Find("BoxMineMenu").transform;
      _dependencies = _menuUi.GetChild(0).GetChild(1);

      // Create machine
      var startButton = _dependencies.GetChild(3).GetChild(1).GetComponent<Button>();
      var viewRecipesButton = _dependencies.GetChild(3).GetChild(2).GetComponent<Button>();
      var setRecipeButton = _dependencies.GetChild(3).GetChild(0).GetComponent<Button>();
      var startLoopButton = _dependencies.GetChild(3).GetChild(3).GetComponent<Button>();
      var stopLoopButton = _dependencies.GetChild(3).GetChild(4).GetComponent<Button>();
      var progressSlider = _dependencies.Find("ForgeHealth").GetComponent<Slider>();
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

      _forgeController = new(
        recipeButtons,
        startButton,
        setRecipeButton,
        viewRecipesButton,
        recipeMenu,
        progressSlider,

        inputNodes,
        outputNodes
      );
      _forgeController._CookSpeedAction = () => { return SkillController.GetMaths(SkillController.SkillType.HEAT); };
      _forgeController.RegisterLoopButtons(startLoopButton, stopLoopButton);

      UnlockRecipe(RecipeType.BRICK);
    }

    //
    public void Update()
    {

      _forgeController.Update();

      // FX
      {
        var menuVisible = MineBoxMenuController.s_Singleton.IsVisible(MineBoxMenuController.MenuType.FORGE);

        // Make it so forge particles appear correctly when changing menus
        {
          if (!menuVisible)
            _menuVisibleTime = Time.time;
          var particlesValueDesired = menuVisible ? false : true;
          if (ParticleController.GetParticles(ParticleController.ParticleType.FORGE_SMOKE).main.prewarm != particlesValueDesired)
          {
            if (particlesValueDesired == false && Time.time - _menuVisibleTime < 0.01f)
              ;
            else
            {
              var main = ParticleController.GetParticles(ParticleController.ParticleType.FORGE_SMOKE).main;
              main.prewarm = particlesValueDesired;

              main = ParticleController.GetParticles(ParticleController.ParticleType.FORGE_EMBERS).main;
              main.prewarm = particlesValueDesired;
            }
          }
        }

        // Start / Stop audio and particle FX
        if (_forgeController._IsCooking)
        {
          if (_forgeLoopAudio == null)
          {
            if (_menuIsVisible)
            {
              _forgeLoopAudio = AudioController.PlayAudio("ForgeSmelt");
              _forgeLoopAudio.loop = true;
            }
          }
          else
          {

            var pitchDesired = menuVisible ? 1f : 0f;
            if (_forgeLoopAudio.pitch != pitchDesired)
              _forgeLoopAudio.pitch = pitchDesired;
          }

          if (!ParticleController.IsParticlesPlaying(ParticleController.ParticleType.FORGE_SMOKE))
          {
            ParticleController.PlayParticles(ParticleController.ParticleType.FORGE_SMOKE);
            ParticleController.PlayParticles(ParticleController.ParticleType.FORGE_EMBERS);
          }
        }
        else
        {
          if (_forgeLoopAudio != null)
          {
            _forgeLoopAudio.Stop();
            _forgeLoopAudio = null;
          }

          if (ParticleController.IsParticlesPlaying(ParticleController.ParticleType.FORGE_SMOKE))
          {
            ParticleController.StopParticles(ParticleController.ParticleType.FORGE_SMOKE);
            ParticleController.StopParticles(ParticleController.ParticleType.FORGE_EMBERS);
          }
        }
      }

    }

    //
    public static void UnlockRecipe(RecipeType recipeType)
    {

      switch (recipeType)
      {

        //
        case RecipeType.BRICK:

          s_Singleton._forgeController.UnlockRecipe(
            (int)recipeType,
            "Brick",
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.STONE_DUST, 10)
            },
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.BRICK, 1)
            }
          );

          break;

        //
        case RecipeType.COPPER_INGOT:

          s_Singleton._forgeController.UnlockRecipe(
            (int)recipeType,
            "Copper Ingot",
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.COPPER, 5)
            },
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.COPPER_INGOT, 1)
            }
          );

          break;

        //
        case RecipeType.BRONZE_INGOT:

          s_Singleton._forgeController.UnlockRecipe(
            (int)recipeType,
            "Bronze Ingot",
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.COPPER, 10),
              (InventoryController.ItemType.TIN, 5),
            },
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.BRONZE_INGOT, 1)
            }
          );

          break;

        //
        case RecipeType.IRON_INGOT:

          s_Singleton._forgeController.UnlockRecipe(
            (int)recipeType,
            "Iron Ingot",
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.STONE_DUST, 25),
              (InventoryController.ItemType.IRON, 15),
            },
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.IRON_INGOT, 1)
            }
          );

          break;

        //
        case RecipeType.STEEL_INGOT:

          s_Singleton._forgeController.UnlockRecipe(
            (int)recipeType,
            "Steel Plate",
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.IRON_INGOT, 2),
              (InventoryController.ItemType.COAL, 25),
            },
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.STEEL_INGOT, 1)
            }
          );

          break;

        //
        case RecipeType.CITRINE_INGOT:

          s_Singleton._forgeController.UnlockRecipe(
            (int)recipeType,
            "Citrine Plate",
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.IRON_INGOT, 4),
              (InventoryController.ItemType.MIX_0, 1),
            },
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.GEM_INGOT_0, 1)
            }
          );

          break;

        //
        case RecipeType.DUAL_INGOT:

          s_Singleton._forgeController.UnlockRecipe(
            (int)recipeType,
            "Dual Plate",
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.MIX_1, 25),
              (InventoryController.ItemType.GEM_DUST_1, 1),
            },
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.DUAL_INGOT, 1)
            }
          );

          break;

      }

    }

    //
    public static void UnlockAutoForge()
    {
      s_Singleton._forgeController.EnableLoopButtons();
    }

    //
    public static MachineController.SaveInfo GetMachineSaveInfo()
    {
      return s_Singleton._forgeController.GetSaveInfo();
    }
    public static void SetMachineSaveInfo(MachineController.SaveInfo saveInfo)
    {
      s_Singleton._forgeController.SetSaveInfo(saveInfo);
    }

  }

}