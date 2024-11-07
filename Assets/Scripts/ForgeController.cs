using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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

    TextMeshProUGUI _input0Text, _input1Text, _output0Text;
    Image _input0Image, _input1Image, _output0Image;
    Button _setRecipeButton, _startButton, _output0Button;

    bool _cooking;
    int _currentOutputAmount;

    //
    public enum RecipeType
    {
      NONE,

      COPPER_INGOT,
      BRONZE_INGOT,
    }

    //
    List<Recipe> _recipes;
    Recipe _currentRecipe;
    class Recipe : IInfoable
    {
      public string _Title;
      public RecipeType _RecipeType;

      public (InventoryController.ItemType InputType, int Amount)[] _Inputs, _Outputs;

      public GameObject _MenuEntry;

      //
      public bool _IsCraftable
      {

        get
        {

          if (InventoryController.s_Singleton.GetItemAmount(_Inputs[0].InputType) < _Inputs[0].Amount)
            return false;
          if (_Inputs.Length > 1 && InventoryController.s_Singleton.GetItemAmount(_Inputs[1].InputType) < _Inputs[1].Amount)
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
            var itemName = InventoryController.GetItemName(item.InputType);

            var itemValue = InventoryController.GetItemValue(item.InputType, item.Amount);
            inputValue += itemValue;

            inputString += $"- {itemName} x{item.Amount}\n";
          }

          var outputString = "";
          var outputValue = 0;
          foreach (var item in _Outputs)
          {
            var itemName = InventoryController.GetItemName(item.InputType);

            var itemValue = InventoryController.GetItemValue(item.InputType, item.Amount);
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
    SimpleInfoable _input0Info, _input1Info, _outputInfo;
    public InfoData _InfoData
    {
      get
      {
        var returnData = new InfoData();

        var returnList = new List<IInfoable>();
        if (_recipeMenu.gameObject.activeSelf)
        {
          foreach (var recipe in _recipes)
            if (recipe._MenuEntry != null)
              returnList.Add(recipe);
        }
        else
        {
          foreach (var otherInfo in _otherInfoables)
            returnList.Add(otherInfo);
          if (_input0Info != null)
            returnList.Add(_input0Info);
          if (_input1Info != null)
            returnList.Add(_input1Info);
          if (_outputInfo != null)
            returnList.Add(_outputInfo);
        }
        returnData._Infos = returnList;

        return returnData;
      }
    }

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

      _input0Text = _dependencies.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
      _input0Image = _dependencies.GetChild(1).GetChild(1).GetComponent<Image>();

      _input1Text = _dependencies.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
      _input1Image = _dependencies.GetChild(0).GetChild(1).GetComponent<Image>();

      _output0Text = _dependencies.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>();
      _output0Image = _dependencies.GetChild(2).GetChild(1).GetChild(0).GetComponent<Image>();
      _output0Button = _dependencies.GetChild(2).GetChild(1).GetComponent<Button>();

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
      SetInput(0, InventoryController.ItemType.NONE, 0, false);
      SetInput(1, InventoryController.ItemType.NONE, 0, false);
      SetOutput(InventoryController.ItemType.NONE, 0);
      for (var i = 0; i < 10; i++)
        SetRecipe(i, null);

      UnlockRecipe(RecipeType.COPPER_INGOT);
    }

    //
    public void Update()
    {

      //
      if (_cooking)
        _forgeProgress += Time.deltaTime * _forgeSpeed * 0.05f;
      while (_forgeProgress >= 1f)
      {
        _forgeProgress -= 1f;

        //
        _cooking = false;
        _startButton.gameObject.SetActive(true);

        _currentOutputAmount += _currentRecipe._Outputs[0].Amount;

        var recipeInput0 = _currentRecipe._Inputs[0];
        SetInput(0, recipeInput0.InputType, recipeInput0.Amount, true);
        if (_currentRecipe._Inputs.Length > 1)
        {
          var recipeInput1 = _currentRecipe._Inputs[0];
          SetInput(1, recipeInput1.InputType, recipeInput1.Amount, true);
        }

        SetOutput(_currentRecipe._Outputs[0].InputType, _currentOutputAmount);
      }

      _forgeProgressSlider.value = _forgeProgress;
    }

    //
    void SetInput(int index, InventoryController.ItemType inputType, int amount, bool transparent)
    {

      var inputText = index == 0 ? _input0Text : _input1Text;
      var inputImage = index == 0 ? _input0Image : _input1Image;

      if (inputType == InventoryController.ItemType.NONE)
      {
        inputText.text = "";
        inputImage.enabled = false;

        if (index == 0)
          _input0Info = null;
        else
          _input1Info = null;
        return;
      }

      inputText.text = $"x{amount}";
      inputImage.sprite = InventoryController.GetItemSprite(inputType);
      var spriteColor = inputImage.color;
      spriteColor.a = transparent ? 0.4f : 1f;
      inputImage.color = spriteColor;
      inputImage.enabled = true;

      var newInfo = new SimpleInfoable()
      {
        _Description = InfoController.GetInfoString("Input", $"x{amount} {InventoryController.GetItemName(inputType)}"),
        _GameObject = inputText.transform.parent.gameObject
      };
      if (index == 0)
        _input0Info = newInfo;
      else
        _input1Info = newInfo;
    }
    void SetOutput(InventoryController.ItemType outputType, int amount)
    {

      _output0Button.onClick.RemoveAllListeners();

      if (outputType == InventoryController.ItemType.NONE)
      {
        _output0Text.text = "";
        _output0Image.enabled = false;

        _outputInfo = null;

        AudioController.PlayAudio("MenuSelect");
        return;
      }

      _output0Text.text = $"x{amount}";
      _output0Image.sprite = InventoryController.GetItemSprite(outputType);
      _output0Image.enabled = true;

      _output0Button.onClick.AddListener(() =>
      {
        SetOutput(InventoryController.ItemType.NONE, 0);
        _currentOutputAmount = 0;

        InventoryController.s_Singleton.AddItemAmount(outputType, amount);

        if (!_cooking)
          _setRecipeButton.gameObject.SetActive(true);

        AudioController.PlayAudio("MenuSelect");
      });

      _outputInfo = new()
      {
        _Description = InfoController.GetInfoString("Collect Output", $"x{amount} {InventoryController.GetItemName(outputType)}"),
        _GameObject = _output0Text.transform.parent.gameObject
      };
    }

    //
    void SetRecipe(int buttonIndex, Recipe recipe)
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
          _currentRecipe = null;
          SetInput(0, InventoryController.ItemType.NONE, 0, false);
          SetInput(1, InventoryController.ItemType.NONE, 0, false);

          _recipeMenu.gameObject.SetActive(false);
          _startButton.gameObject.SetActive(false);

          AudioController.PlayAudio("MenuSelect");
        });
      }
      else
      {
        img.sprite = InventoryController.GetItemSprite(recipe._Outputs[0].InputType);
        img.enabled = true;

        button.onClick.AddListener(() =>
        {
          _currentRecipe = recipe;

          var recipeInput0 = recipe._Inputs[0];
          SetInput(0, recipeInput0.InputType, recipeInput0.Amount, true);
          if (recipe._Inputs.Length > 1)
          {
            var recipeInput1 = recipe._Inputs[1];
            SetInput(1, recipeInput1.InputType, recipeInput1.Amount, true);
          }
          else
            SetInput(1, InventoryController.ItemType.NONE, 0, false);

          _recipeMenu.gameObject.SetActive(false);

          _startButton.gameObject.SetActive(true);
          _startButton.onClick.RemoveAllListeners();
          _startButton.onClick.AddListener(() =>
          {
            AudioController.PlayAudio("MenuSelect");


            if (!_currentRecipe._IsCraftable)
              return;

            _startButton.gameObject.SetActive(false);
            _setRecipeButton.gameObject.SetActive(false);

            var recipeInput0 = _currentRecipe._Inputs[0];
            SetInput(0, recipeInput0.InputType, recipeInput0.Amount, false);
            InventoryController.s_Singleton.RemoveItemAmount(recipeInput0.InputType, recipeInput0.Amount);

            if (_currentRecipe._Inputs.Length > 1)
            {
              var recipeInput1 = _currentRecipe._Inputs[1];
              SetInput(1, recipeInput1.InputType, recipeInput1.Amount, false);
              InventoryController.s_Singleton.RemoveItemAmount(recipeInput1.InputType, recipeInput1.Amount);
            }
            else
              SetInput(1, InventoryController.ItemType.NONE, 0, false);


            _cooking = true;
          });
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
            _RecipeType = RecipeType.COPPER_INGOT,

            _Inputs = new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.COPPER, 5)
            },
            _Outputs = new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.COPPER_INGOT, 1)
            },
          };
          s_Singleton._recipes.Add(newRecipe);
          s_Singleton.SetRecipe(0, newRecipe);

          break;

        //
        case RecipeType.BRONZE_INGOT:

          newRecipe = new Recipe()
          {
            _Title = "Bronze Ingot",
            _RecipeType = RecipeType.BRONZE_INGOT,

            _Inputs = new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.COPPER, 10),
              (InventoryController.ItemType.TIN, 5),
            },
            _Outputs = new (InventoryController.ItemType, int)[]{
              (InventoryController.ItemType.BRONZE_INGOT, 1)
            },
          };
          s_Singleton._recipes.Add(newRecipe);
          s_Singleton.SetRecipe(1, newRecipe);

          break;

      }

    }

  }

}