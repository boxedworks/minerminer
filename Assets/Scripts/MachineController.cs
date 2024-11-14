
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers
{

  public class MachineController : IHasInfoables
  {

    bool _isCooking;
    public bool _IsCooking { get { return _isCooking; } }
    float _cookProgress, _forgeSpeed;

    //
    Dictionary<int, Recipe> _recipes;
    Recipe _currentRecipe;
    public int _CurrentRecipe { get { return _currentRecipe == null ? 0 : _currentRecipe._RecipeType; } }
    int _recipePage;

    CraftingNode[] _inputNodes, _outputNodes;

    //
    List<Button> _recipeButtons;
    Button _startButton, _setRecipeButton;
    GameObject _recipeMenu;
    Slider _cookProgressSlider;

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
          foreach (var node in _inputNodes)
            if (node._Info != null)
              returnList.Add(node._Info);
          foreach (var node in _outputNodes)
            if (node._Info != null)
              returnList.Add(node._Info);
        }
        returnData._Infos = returnList;

        return returnData;
      }
    }

    public MachineController(
      List<Button> recipeButtons,
      Button startButton,
      Button setRecipeButton,
      GameObject recipeMenu,
      Slider progressSlider,

      CraftingNode[] inputNodes,
      CraftingNode[] outputNodes
      )
    {
      _recipeButtons = recipeButtons;
      _startButton = startButton;
      _setRecipeButton = setRecipeButton;
      _recipeMenu = recipeMenu;
      _cookProgressSlider = progressSlider;

      _inputNodes = inputNodes;
      _outputNodes = outputNodes;

      _recipeMenu.SetActive(false);

      //
      _recipes = new();

      _cookProgress = 0f;
      _forgeSpeed = 1f;

      // Buttons
      _otherInfoables = new();
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

      _startButton.gameObject.SetActive(false);
      _otherInfoables.Add(new SimpleInfoable()
      {
        _GameObject = _startButton.gameObject,
        _Description = InfoController.GetInfoString("Start Forge", "Start crafting the recipe.")
      });

      //
      foreach (var node in _inputNodes)
      {
        node.RegisterMachine(this);
        node.SetItem(InventoryController.ItemType.NONE, 0, false);
      }
      foreach (var node in _outputNodes)
      {
        node.RegisterMachine(this);
        node.SetItem(InventoryController.ItemType.NONE, 0, false);
      }
      UpdateRecipesUi();
    }

    //
    public void Update()
    {
      //
      if (_isCooking)
        _cookProgress += Time.deltaTime * _forgeSpeed * 0.05f;
      while (_cookProgress >= 1f)
      {
        _cookProgress -= 1f;

        //
        _isCooking = false;
        _startButton.gameObject.SetActive(true);

        //
        for (var i = 0; i < _outputNodes.Length; i++)
        {
          var node = _outputNodes[i];

          if (i < _currentRecipe._Outputs.Length)
            node._Amount += _currentRecipe._Outputs[i].Amount;
        }
        SetRecipeInputs(_currentRecipe, true);
        SetRecipeOutputs(_currentRecipe);
      }

      //
      _cookProgressSlider.value = _cookProgress;
    }

    //
    public void UnlockRecipe(
      int recipeType,
      string title,
      (InventoryController.ItemType ItemType, int ItemAmount)[] inputs,
      (InventoryController.ItemType ItemType, int ItemAmount)[] outputs
      )
    {
      _recipes.Add(recipeType, new Recipe()
      {
        _RecipeType = recipeType,
        _Title = title,

        _Inputs = inputs,
        _Outputs = outputs
      });
      UpdateRecipesUi();
    }

    //
    void UpdateRecipesUi()
    {

      var recipeKeys = new List<int>(_recipes.Keys);
      for (var i = 0; i < _recipeButtons.Count; i++)
      {

        var recipeType = i < recipeKeys.Count ? recipeKeys[i] : -1;

        var button = _recipeButtons[i];
        var img = button.transform.GetChild(1).GetComponent<Image>();

        button.onClick.RemoveAllListeners();

        if (recipeType == -1)
        {
          img.enabled = false;
          button.onClick.AddListener(() =>
          {
            SetRecipe(null);

            AudioController.PlayAudio("MenuSelect");
          });
        }
        else
        {
          var recipe = _recipes[recipeType];

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
    }

    //
    public void SelectRecipe(int recipeType)
    {
      SetRecipe(recipeType < 1 ? null : _recipes[recipeType]);
    }
    void SetRecipe(Recipe recipe)
    {
      _currentRecipe = recipe;

      if (recipe == null)
      {
        foreach (var input in _inputNodes)
          input.SetItem(InventoryController.ItemType.NONE, 0, false);

        _recipeMenu.gameObject.SetActive(false);
        _startButton.gameObject.SetActive(false);

        return;
      }

      //
      for (var i = 0; i < _inputNodes.Length; i++)
      {
        var input = _inputNodes[i];

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
        _cookProgress = 0f;

        LogController.AppendLog($"Started forge with {recipe._Title} recipe.");
      });

    }

    //
    void SetRecipeInputs(Recipe recipe, bool transparent)
    {
      //
      if (recipe == null)
      {
        foreach (var node in _inputNodes)
          node.SetItem(InventoryController.ItemType.NONE, 0, false);

        return;
      }

      //
      for (var i = 0; i < _inputNodes.Length; i++)
      {
        var node = _inputNodes[i];

        if (i >= recipe._Inputs.Length)
          node.SetItem(InventoryController.ItemType.NONE, 0, false);
        else
          node.SetItem(recipe._Inputs[i].ItemType, recipe._Inputs[i].Amount, transparent);
      }
    }
    void SetRecipeOutputs(Recipe recipe)
    {
      //
      if (recipe == null)
      {
        foreach (var node in _outputNodes)
          node.SetItem(InventoryController.ItemType.NONE, 0, false);

        return;
      }

      //
      for (var i = 0; i < _outputNodes.Length; i++)
      {
        var node = _outputNodes[i];

        if (i >= recipe._Outputs.Length)
          node.SetItem(InventoryController.ItemType.NONE, 0, false);
        else
          node.SetItem(recipe._Outputs[i].ItemType, node._Amount, node._Amount == 0);
      }
    }

    //
    public class Recipe : IInfoable
    {
      public string _Title;
      public int _RecipeType;

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

    // Represents an input or output in a crafting system
    public class CraftingNode
    {

      MachineController _machineController;

      public int _Amount;

      Image _itemImage;
      TMPro.TextMeshProUGUI _amountText;
      Button _itemButton;

      public SimpleInfoable _Info;

      public CraftingNode(Image image, TMPro.TextMeshProUGUI text)
      {
        _itemImage = image;
        _amountText = text;
      }

      //
      public void RegisterMachine(MachineController machineController)
      {
        _machineController = machineController;
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
        {
          _itemButton.onClick.RemoveAllListeners();
          _itemButton.onClick.AddListener(() =>
          {
            AudioController.PlayAudio("MenuSelect");
          });
        }

        if (amount == 0)
          itemType = InventoryController.ItemType.NONE;

        if (itemType == InventoryController.ItemType.NONE)
        {
          _amountText.text = "";
          _itemImage.enabled = false;
          _Amount = 0;

          _Info = null;
          return;
        }

        _Amount = amount;

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
            InventoryController.s_Singleton.AddItemAmount(itemType, _Amount);
            SetItem(InventoryController.ItemType.NONE, 0, false);

            if (!_machineController._isCooking)
              _machineController._setRecipeButton.gameObject.SetActive(true);
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

    //
    [System.Serializable]
    public class SaveInfo
    {
      public string CurrentRecipeTitle;

      public bool IsCooking;
      public float CookProgress;
      public List<int> OutputAmounts;
    }
    public SaveInfo GetSaveInfo()
    {
      var saveInfo = new SaveInfo();

      saveInfo.IsCooking = _isCooking;
      if (_currentRecipe != null)
      {
        saveInfo.CurrentRecipeTitle = _currentRecipe._Title;

        saveInfo.OutputAmounts = new();
        foreach (var node in _outputNodes)
          saveInfo.OutputAmounts.Add(node._Amount);
      }
      saveInfo.CookProgress = _cookProgress;

      return saveInfo;
    }
    public void SetSaveInfo(SaveInfo saveInfo)
    {

      var recipeTitle = saveInfo.CurrentRecipeTitle;
      if (recipeTitle != null)
        foreach (var recipe in _recipes)
        {
          if (recipe.Value._Title == recipeTitle)
          {
            SetRecipe(recipe.Value);
            break;
          }
        }

      _isCooking = saveInfo.IsCooking;
      if (_isCooking)
      {
        _startButton.gameObject.SetActive(false);
        _setRecipeButton.gameObject.SetActive(false);
      }
      _cookProgress = saveInfo.CookProgress;

      if (_currentRecipe != null)
      {

        var outputAmounts = saveInfo.OutputAmounts;
        if (outputAmounts != null)
          for (var i = 0; i < _outputNodes.Length && i < outputAmounts.Count; i++)
          {
            var node = _outputNodes[i];
            node._Amount = outputAmounts[i];
          }

        SetRecipeInputs(_currentRecipe, !_isCooking);
        SetRecipeOutputs(_currentRecipe);
      }
    }

  }

}