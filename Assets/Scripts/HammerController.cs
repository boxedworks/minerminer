using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Net.Security;
using UnityEngine.EventSystems;

namespace Controllers
{

  public class HammerController : IHasInfoables
  {

    //
    public static HammerController s_Singleton;

    public MachineController _Machine;
    ClickManager _clickManager;

    float _hammerSpeed, _hammerSpeedMax, _hammerSpeedGrant;

    int _swingId;

    RecipeType _currentRecipe { get { return (RecipeType)_Machine._CurrentRecipe; } }
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
    Slider _hammerSpeedSlider;

    float _swingTimer, _swingTimerVisual;

    //
    public InfoData _InfoData
    {
      get
      {
        return _Machine._InfoData;
      }
    }

    //
    public HammerController()
    {
      s_Singleton = this;

      _menu = MineBoxMenuController.s_Singleton.GetMenu(MineBoxMenuController.MenuType.HAMMER).transform;
      _menuUi = GameObject.Find("BoxMineMenu").transform;
      _dependencies = _menuUi.GetChild(0).GetChild(2);

      _hammerSpeed = 1f;
      _hammerSpeedMax = 5f;
      _hammerSpeedSlider = _dependencies.Find("HammerSpeed").GetComponent<Slider>();
      _hammerSpeedGrant = 0.3f;

      // Create machine
      var buttons = _dependencies.Find("HammerButtons");
      var startButton = buttons.GetChild(1).GetComponent<Button>();
      var setRecipeButton = buttons.GetChild(0).GetComponent<Button>();
      var startLoopButton = buttons.GetChild(2).GetComponent<Button>();
      var stopLoopButton = buttons.GetChild(3).GetComponent<Button>();
      var setRecipesButton = buttons.GetChild(4).GetComponent<Button>();
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

      _Machine = new(
        recipeButtons,
        startButton,
        setRecipeButton,
        setRecipesButton,
        recipeMenu,
        progressSlider,

        inputNodes,
        outputNodes
      );
      _Machine._CookSpeedAction = () => { return 1f; };
      _Machine.RegisterLoopButtons(startLoopButton, stopLoopButton);
      _Machine.EnableLoopButtons();
      _Machine._CookOverTime = false;

      _clickManager = new();

      UnlockRecipe(RecipeType.STONE_DUST);
      UnlockRecipe(RecipeType.GEM_DUST_0);

      //
      _hammerModel = _menu.Find("Hammer").transform;
      //_hammerUi = GameObject.Find("PicBox").transform as RectTransform;

      // Set onclick
      var eventTrigger = _dependencies.Find("Clicks").GetComponent<EventTrigger>();
      var entry = new EventTrigger.Entry();
      entry.eventID = EventTriggerType.PointerDown;
      entry.callback.AddListener((data) =>
      {
        _hammerSpeed = Mathf.Clamp(_hammerSpeed - 0.5f, 1f, _hammerSpeedMax);

        AudioController.PlayAudio("HammerClick", Random.Range(0.2f, 0.3f));
        ParticleController.PlayParticles(ParticleController.ParticleType.HAMMER_SPARKS);
      });
      eventTrigger.triggers.Add(entry);
    }

    //
    public void Update()
    {

      _Machine.Update();
      _clickManager.Update();

      //
      _hammerSpeed = Mathf.Clamp(_hammerSpeed - Time.deltaTime * (_Machine._IsCooking ? 0.01f : 0.1f) * _hammerSpeed * 0.6f, 1f, _hammerSpeedMax);
      _hammerSpeedSlider.value += ((_hammerSpeed - 1f) / (_hammerSpeedMax - 1f) - _hammerSpeedSlider.value) * Time.deltaTime * 5f;

      //
      _swingTimerVisual = Mathf.Clamp(_swingTimerVisual + (_swingTimer - _swingTimerVisual) * Time.deltaTime * (_swingTimer < _swingTimerVisual ? 50f : 10f), 0f, 1f);
      _hammerModel.localPosition = Vector3.Lerp(new Vector3(0f, 0.6f, 100f), new Vector3(0f, 1.5f, 100f), _swingTimerVisual);

      //
      _swingTimer += Time.deltaTime * 1f * 0.25f * GameController.s_GameSpeedMod * _hammerSpeed;
      if (_swingTimer >= 1f && !_Machine._IsCooking) { _swingTimer = 1f; return; }
      while (_swingTimer >= 1f)
      {
        _swingTimer -= 1f;
        _swingId++;

        //
        _Machine.TakeRecipeInputs();
        _Machine.InstantCook();

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

          s_Singleton._Machine.UnlockRecipe(
            (int)recipeType,
            "Stone Dust",
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.STONE, 10)
            },
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.STONE_DUST, 3)
            }
          );

          break;

        //
        case RecipeType.GEM_DUST_0:

          s_Singleton._Machine.UnlockRecipe(
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

          s_Singleton._Machine.UnlockRecipe(
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

          s_Singleton._Machine.UnlockRecipe(
            (int)recipeType,
            "Dual Mix",
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.COBALT, 5),
              (InventoryController.ItemType.CINNABAR, 5),
            },
            new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.MIX_1, 1)
            }
          );

          break;

      }

    }

    //
    class ClickManager
    {

      Transform _container;
      Image _containerImage;

      bool _clickVisible;

      int _upgradeLevel, _lastSwingId;

      public ClickManager()
      {
        _container = s_Singleton._dependencies.Find("Clicks").transform;
        _containerImage = _container.GetComponent<Image>();
      }

      //
      public void Update()
      {

        // Remove if not cooking
        if (_clickVisible)
        {

          if (!s_Singleton._Machine._IsCooking || s_Singleton._swingTimer > 0.7f)
          {
            for (var i = _container.childCount - 1; i > 0; i--)
              GameObject.DestroyImmediate(_container.GetChild(i).gameObject);
            _clickVisible = false;
            // _containerImage.enabled = false;
          }
        }

        // Spawn new click
        if (!_clickVisible && s_Singleton._swingTimer > 0.3f && s_Singleton._swingTimer < 0.7f && _lastSwingId != s_Singleton._swingId)
        {
          if (!s_Singleton._Machine._IsCooking)
          {
            return;
          }

          _clickVisible = true;
          _lastSwingId = s_Singleton._swingId;

          var prefab = _container.GetChild(0).gameObject;
          var newFab = GameObject.Instantiate(prefab, _container);

          newFab.transform.localPosition += new Vector3(Random.Range(-1f, 1f) * 70f, Random.Range(-1f, 1f) * 70f, 0f);

          var button = newFab.GetComponent<EventTrigger>();
          var entry = new EventTrigger.Entry();
          entry.eventID = EventTriggerType.PointerDown;
          entry.callback.AddListener((data) =>
          {
            _clickVisible = false;

            GameObject.Destroy(newFab);
            // _containerImage.enabled = false;

            s_Singleton._hammerSpeed = Mathf.Clamp(s_Singleton._hammerSpeed + s_Singleton._hammerSpeedGrant, 1f, s_Singleton._hammerSpeedMax);

            // FX
            AudioController.PlayAudio("HammerClick", s_Singleton._hammerSpeed * 0.6f);
            ParticleController.PlayParticles(ParticleController.ParticleType.HAMMER_CLICK);
          });
          button.triggers.Add(entry);

          // _containerImage.enabled = true;
          newFab.SetActive(true);
        }

      }
    }

    //
    public static MachineController.SaveInfo GetMachineSaveInfo()
    {
      return s_Singleton._Machine.GetSaveInfo();
    }
    public static void SetMachineSaveInfo(MachineController.SaveInfo saveInfo)
    {
      s_Singleton._Machine.SetSaveInfo(saveInfo);
    }

    //
    [System.Serializable]
    public class SaveInfo
    {
      public float HammerSpeed;
    }
    public static SaveInfo GetSaveInfo()
    {
      var saveInfo = new SaveInfo();

      saveInfo.HammerSpeed = s_Singleton._hammerSpeed;

      return saveInfo;
    }
    public static void SetSaveInfo(SaveInfo saveInfo)
    {
      s_Singleton._hammerSpeed = Mathf.Clamp(saveInfo.HammerSpeed, 1f, s_Singleton._hammerSpeedMax);
    }

  }
}