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
    Transform _menu, _menuUi, _dependencies, _recipeMenu;

    float _forgeProgress, _forgeSpeed;
    Slider _forgeProgressSlider;

    Button _setRecipeButton, _startButton;

    bool _isCooking;
    int _currentOutputAmount;

    //
    AudioSource _forgeLoopAudio;
    float _menuVisibleTime;

    //
    public enum RecipeType
    {
      NONE,

      COPPER_INGOT,
      BRONZE_INGOT,
      IRON_INGOT,
    }

    //
    Dictionary<RecipeType, Recipe> _recipes;
    Recipe _currentRecipe;
    class Recipe : IInfoable
    {
      public string _Title;
      public RecipeType _RecipeType;

      public (InventoryController.ItemType ItemType, int Amount)[] _Inputs, _Outputs;

      public GameObject _MenuEntry;

      //
      public bool _IsCraftable
      {

        get
        {

          if (InventoryController.s_Singleton.GetItemAmount(_Inputs[0].ItemType) < _Inputs[0].Amount)
            return false;
          if (_Inputs.Length > 1 && InventoryController.s_Singleton.GetItemAmount(_Inputs[1].ItemType) < _Inputs[1].Amount)
            return false;

          return true;

        }

      }

      //
      public string _Info
      {
        get
        {

          var inputString = "";
          var inputValue = 0;
          foreach (var item in _Inputs)
          {
            var itemName = InventoryController.GetItemName(item.ItemType);

            var itemValue = InventoryController.GetItemValue(item.ItemType, item.Amount);
            inputValue += itemValue;

            inputString += $"- {itemName} x{item.Amount}\n";
          }

          var outputString = "";
          var outputValue = 0;
          foreach (var item in _Outputs)
          {
            var itemName = InventoryController.GetItemName(item.ItemType);

            var itemValue = InventoryController.GetItemValue(item.ItemType, item.Amount);
            outputValue += itemValue;

            outputString += $"- {itemName} x{item.Amount}\n";
          }

          return InfoController.GetInfoString($"{_Title}", @$"<b>Inputs</b>

{inputString.Trim()}
  (${inputValue})

<b>Outputs</b>

{outputString.Trim()}
  (${outputValue})
");
        }
      }
      public RectTransform _Transform { get { return _MenuEntry.transform as RectTransform; } }
    }

    //
    List<IInfoable> _otherInfoables;
    public InfoData _InfoData
    {
      get
      {
        var returnData = new InfoData();

        var returnList = new List<IInfoable>();
        if (_recipeMenu.gameObject.activeSelf)
        {
          foreach (var recipe in _recipes)
            if (recipe.Value._MenuEntry != null)
              returnList.Add(recipe.Value);
        }
        else
        {
          foreach (var otherInfo in _otherInfoables)
            returnList.Add(otherInfo);
          foreach (var input in _inputs)
            if (input._Info != null)
              returnList.Add(input._Info);
          if (_output._Info != null)
            returnList.Add(_output._Info);
        }
        returnData._Infos = returnList;

        return returnData;
      }
    }

    // Represents an input or output in a crafting system
    class CraftingNode
    {

      Image _itemImage;
      TextMeshProUGUI _amountText;
      Button _itemButton;

      public SimpleInfoable _Info;

      public CraftingNode(Image image, TextMeshProUGUI text)
      {
        _itemImage = image;
        _amountText = text;
      }

      //
      public void RegisterButton(Button button)
      {
        _itemButton = button;
      }
      public void SetButtonAction(UnityEngine.Events.UnityAction buttonAction)
      {
        _itemButton.onClick.RemoveAllListeners();
        _itemButton.onClick.AddListener(buttonAction);
      }

      //
      public void SetItem(InventoryController.ItemType itemType, int amount, bool transparent)
      {

        if (_itemButton != null)
          _itemButton.onClick.RemoveAllListeners();

        if (itemType == InventoryController.ItemType.NONE)
        {
          _amountText.text = "";
          _itemImage.enabled = false;

          _Info = null;
          return;
        }

        _amountText.text = $"x{amount}";
        _itemImage.sprite = InventoryController.GetItemSprite(itemType);
        var spriteColor = _itemImage.color;
        spriteColor.a = transparent ? 0.4f : 1f;
        _itemImage.color = spriteColor;
        _itemImage.enabled = true;

        // Output
        if (_itemButton != null)
        {
          _itemButton.onClick.AddListener(() =>
          {
            s_Singleton._output.SetItem(InventoryController.ItemType.NONE, 0, false);
            s_Singleton._currentOutputAmount = 0;

            InventoryController.s_Singleton.AddItemAmount(itemType, amount);

            if (!s_Singleton._isCooking)
              s_Singleton._setRecipeButton.gameObject.SetActive(true);

            AudioController.PlayAudio("MenuSelect");
          });

          _Info = new()
          {
            _Description = InfoController.GetInfoString("Collect Output", $"x{amount} {InventoryController.GetItemName(itemType)}"),
            _GameObject = _amountText.transform.parent.gameObject
          };
        }

        // Input
        else
        {

          _Info = new()
          {
            _Description = InfoController.GetInfoString("Input", $"x{amount} {InventoryController.GetItemName(itemType)}"),
            _GameObject = _amountText.transform.parent.gameObject
          };
        }
      }

    }
    CraftingNode[] _inputs;
    CraftingNode _output;

    //
    public ForgeController()
    {
      s_Singleton = this;

      //
      _menu = MineBoxMenuController.s_Singleton.GetMenu(MineBoxMenuController.MenuType.FORGE).transform;
      _menuUi = GameObject.Find("BoxMineMenu").transform;
      _dependencies = _menuUi.GetChild(0).GetChild(1);

      _recipeMenu = _dependencies.Find("RecipeMenu");
      _recipeMenu.gameObject.SetActive(false);

      _forgeProgressSlider = _dependencies.Find("ForgeHealth").GetComponent<Slider>();

      //
      _inputs = new CraftingNode[]{
        new(_dependencies.GetChild(1).GetChild(1).GetComponent<Image>(), _dependencies.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>()),
        new(_dependencies.GetChild(0).GetChild(1).GetComponent<Image>(), _dependencies.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>()),
      };
      _output = new(_dependencies.GetChild(2).GetChild(1).GetChild(0).GetComponent<Image>(), _dependencies.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>());
      _output.RegisterButton(_dependencies.GetChild(2).GetChild(1).GetComponent<Button>());

      _forgeProgress = 0f;
      _forgeSpeed = 1f;

      //
      _recipes = new()
      {

      };

      // Buttons
      _otherInfoables = new();
      _setRecipeButton = _dependencies.GetChild(3).GetChild(0).GetComponent<Button>();
      _setRecipeButton.onClick.AddListener(() =>
      {
        _recipeMenu.gameObject.SetActive(true);

        AudioController.PlayAudio("MenuSelect");
      });
      _otherInfoables.Add(new SimpleInfoable()
      {
        _GameObject = _setRecipeButton.gameObject,
        _Description = InfoController.GetInfoString("Select Recipe", "Select the recipe for the forge.")
      });

      _startButton = _dependencies.GetChild(3).GetChild(1).GetComponent<Button>();
      _startButton.gameObject.SetActive(false);
      _otherInfoables.Add(new SimpleInfoable()
      {
        _GameObject = _startButton.gameObject,
        _Description = InfoController.GetInfoString("Start Forge", "Start crafting the recipe.")
      });

      //
      foreach (var input in _inputs)
        input.SetItem(InventoryController.ItemType.NONE, 0, false);
      _output.SetItem(InventoryController.ItemType.NONE, 0, false);
      for (var i = 0; i < 10; i++)
        SetRecipeUI(i, null);

      UnlockRecipe(RecipeType.COPPER_INGOT);
    }

    //
    public void Update()
    {

      //
      if (_isCooking)
        _forgeProgress += Time.deltaTime * _forgeSpeed * 0.05f;
      while (_forgeProgress >= 1f)
      {
        _forgeProgress -= 1f;

        //
        _isCooking = false;
        _startButton.gameObject.SetActive(true);

        _currentOutputAmount += _currentRecipe._Outputs[0].Amount;

        SetRecipeInputs(_currentRecipe, true);

        _output.SetItem(_currentRecipe._Outputs[0].ItemType, _currentOutputAmount, false);
      }

      //
      _forgeProgressSlider.value = _forgeProgress;

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
        if (_isCooking)
        {
          if (_forgeLoopAudio == null)
          {
            _forgeLoopAudio = AudioController.PlayAudio("ForgeSmelt");
            _forgeLoopAudio.loop = true;
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
    void SetRecipe(RecipeType recipeType)
    {
      SetRecipe(recipeType == RecipeType.NONE ? null : _recipes[recipeType]);
    }
    void SetRecipe(Recipe recipe)
    {
      _currentRecipe = recipe;

      if (recipe == null)
      {
        foreach (var input in _inputs)
          input.SetItem(InventoryController.ItemType.NONE, 0, false);

        _recipeMenu.gameObject.SetActive(false);
        _startButton.gameObject.SetActive(false);

        return;
      }

      //
      for (var i = 0; i < _inputs.Length; i++)
      {
        var input = _inputs[i];

        if (i >= recipe._Inputs.Length)
          input.SetItem(InventoryController.ItemType.NONE, 0, false);
        else
          input.SetItem(recipe._Inputs[i].ItemType, recipe._Inputs[i].Amount, true);
      }

      _recipeMenu.gameObject.SetActive(false);

      _startButton.gameObject.SetActive(true);
      _startButton.onClick.RemoveAllListeners();
      _startButton.onClick.AddListener(() =>
      {
        AudioController.PlayAudio("MenuSelect");

        if (!_currentRecipe._IsCraftable)
        {
          LogController.AppendLog($"<color=red>Insufficient materials to forge </color>{recipe._Title}<color=red> recipe!</color>");
          LogController.ForceOpen();
          return;
        }

        _startButton.gameObject.SetActive(false);
        _setRecipeButton.gameObject.SetActive(false);

        var recipeInput0 = _currentRecipe._Inputs[0];
        InventoryController.s_Singleton.RemoveItemAmount(recipeInput0.ItemType, recipeInput0.Amount);
        if (_currentRecipe._Inputs.Length > 1)
        {
          var recipeInput1 = _currentRecipe._Inputs[1];
          InventoryController.s_Singleton.RemoveItemAmount(recipeInput1.ItemType, recipeInput1.Amount);
        }

        SetRecipeInputs(_currentRecipe, false);

        _isCooking = true;

        LogController.AppendLog($"Started forge with {recipe._Title} recipe.");
      });

    }

    //
    void SetRecipeInputs(Recipe recipe, bool transparent)
    {
      //
      if (recipe == null)
      {
        foreach (var input in _inputs)
          input.SetItem(InventoryController.ItemType.NONE, 0, false);

        return;
      }

      //
      for (var i = 0; i < _inputs.Length; i++)
      {
        var input = _inputs[i];

        if (i >= recipe._Inputs.Length)
          input.SetItem(InventoryController.ItemType.NONE, 0, transparent);
        else
          input.SetItem(recipe._Inputs[i].ItemType, recipe._Inputs[i].Amount, transparent);
      }
    }

    //
    void SetRecipeUI(int buttonIndex, Recipe recipe)
    {

      var button = (buttonIndex < 5 ? _recipeMenu.GetChild(0).GetChild(buttonIndex) : _recipeMenu.GetChild(1).GetChild(buttonIndex - 5))
        .GetChild(0).GetComponent<Button>();
      var img = button.transform.GetChild(1).GetComponent<Image>();

      button.onClick.RemoveAllListeners();

      if (recipe == null)
      {
        img.enabled = false;

        button.onClick.AddListener(() =>
        {
          SetRecipe(recipe);

          AudioController.PlayAudio("MenuSelect");
        });
      }
      else
      {
        img.sprite = InventoryController.GetItemSprite(recipe._Outputs[0].ItemType);
        img.enabled = true;

        button.onClick.AddListener(() =>
        {
          SetRecipe(recipe);

          AudioController.PlayAudio("MenuSelect");
        });

        recipe._MenuEntry = button.transform.parent.gameObject;
      }
    }

    //
    public static void UnlockRecipe(RecipeType recipeType)
    {

      switch (recipeType)
      {

        //
        case RecipeType.COPPER_INGOT:

          var newRecipe = new Recipe()
          {
            _Title = "Copper Ingot",
            _RecipeType = recipeType,

            _Inputs = new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.COPPER, 5)
            },
            _Outputs = new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.COPPER_INGOT, 1)
            },
          };
          s_Singleton._recipes.Add(recipeType, newRecipe);
          s_Singleton.SetRecipeUI(0, newRecipe);

          break;

        //
        case RecipeType.BRONZE_INGOT:

          newRecipe = new Recipe()
          {
            _Title = "Bronze Ingot",
            _RecipeType = recipeType,

            _Inputs = new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.COPPER, 10),
              (InventoryController.ItemType.TIN, 5),
            },
            _Outputs = new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.BRONZE_INGOT, 1)
            },
          };
          s_Singleton._recipes.Add(recipeType, newRecipe);
          s_Singleton.SetRecipeUI(1, newRecipe);

          break;

        //
        case RecipeType.IRON_INGOT:

          newRecipe = new Recipe()
          {
            _Title = "Iron Ingot",
            _RecipeType = recipeType,

            _Inputs = new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.STONE, 50),
              (InventoryController.ItemType.IRON, 10),
            },
            _Outputs = new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.IRON_INGOT, 1)
            },
          };
          s_Singleton._recipes.Add(recipeType, newRecipe);
          s_Singleton.SetRecipeUI(2, newRecipe);

          break;

      }

    }

    //
    [System.Serializable]
    public class SaveInfo
    {
      public string CurrentRecipe;

      public bool IsCooking;
      public int OutputAmount;
      public float ForgeProgress;
    }
    public static SaveInfo GetSaveInfo()
    {
      var saveInfo = new SaveInfo();

      saveInfo.CurrentRecipe = s_Singleton._currentRecipe == null ? null : s_Singleton._currentRecipe._RecipeType.ToString();
      saveInfo.IsCooking = s_Singleton._isCooking;
      saveInfo.OutputAmount = s_Singleton._currentOutputAmount;
      saveInfo.ForgeProgress = s_Singleton._forgeProgress;

      return saveInfo;
    }
    public static void SetSaveInfo(SaveInfo saveInfo)
    {
      if (System.Enum.TryParse(saveInfo.CurrentRecipe, true, out RecipeType currentRecipeType))
        s_Singleton.SetRecipe(currentRecipeType);

      s_Singleton._isCooking = saveInfo.IsCooking;
      if (s_Singleton._isCooking)
      {
        s_Singleton._startButton.gameObject.SetActive(false);
        s_Singleton._setRecipeButton.gameObject.SetActive(false);
      }

      s_Singleton._currentOutputAmount = saveInfo.OutputAmount;
      s_Singleton.SetRecipeInputs(s_Singleton._currentRecipe, !s_Singleton._isCooking);
      if (s_Singleton._currentOutputAmount > 0)
      {
        s_Singleton._output.SetItem(s_Singleton._currentRecipe._Outputs[0].ItemType, s_Singleton._currentOutputAmount, false);
      }

      s_Singleton._forgeProgress = saveInfo.ForgeProgress;
    }

  }

}