using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers
{

  public class ForgeController
  {

    //
    public static ForgeController s_Singleton;

    //
    Transform _menu, _menuUi, _dependencies, _recipeMenu;

    float _forgeProgress, _forgeSpeed;
    Slider _forgeProgressSlider;

    TextMeshProUGUI _input0Text, _output0Text;
    Image _input0Image, _output0Image;
    Button _setRecipeButton, _startButton, _output0Button;

    bool _cooking;
    int _currentOutputAmount;

    //
    Recipe _currentRecipe;
    class Recipe
    {

      public InventoryController.ItemType _InputType, _OutputType;
      public int _InputAmount, _OutputAmount;

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

      _input0Text = _dependencies.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
      _input0Image = _dependencies.GetChild(0).GetChild(1).GetComponent<Image>();
      _output0Text = _dependencies.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
      _output0Image = _dependencies.GetChild(1).GetChild(1).GetChild(0).GetComponent<Image>();
      _output0Button = _dependencies.GetChild(1).GetChild(1).GetComponent<Button>();

      _forgeProgress = 0f;
      _forgeSpeed = 1f;

      // Buttons
      _setRecipeButton = _dependencies.GetChild(2).GetChild(0).GetComponent<Button>();
      _setRecipeButton.onClick.AddListener(() =>
      {
        _recipeMenu.gameObject.SetActive(true);
      });

      _startButton = _dependencies.GetChild(2).GetChild(1).GetComponent<Button>();
      _startButton.gameObject.SetActive(false);

      //
      SetInput(InventoryController.ItemType.NONE, 0, false);
      SetOutput(InventoryController.ItemType.NONE, 0);
      SetRecipe(0, new Recipe()
      {
        _InputType = InventoryController.ItemType.COPPER,
        _InputAmount = 5,

        _OutputType = InventoryController.ItemType.COPPER_INGOT,
        _OutputAmount = 1
      });
      for (var i = 1; i < 10; i++)
        SetRecipe(i, null);
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

        _currentOutputAmount += _currentRecipe._OutputAmount;
        SetOutput(_currentRecipe._OutputType, _currentOutputAmount);
      }

      _forgeProgressSlider.value = _forgeProgress;
    }

    //
    void SetInput(InventoryController.ItemType inputType, int amount, bool transparent)
    {

      if (inputType == InventoryController.ItemType.NONE)
      {
        _input0Text.text = "";
        _input0Image.enabled = false;
        return;
      }

      _input0Text.text = $"x{amount}";
      _input0Image.sprite = InventoryController.GetItemSprite(inputType);
      var spriteColor = _input0Image.color;
      spriteColor.a = transparent ? 0.4f : 1f;
      _input0Image.color = spriteColor;
      _input0Image.enabled = true;
    }
    void SetOutput(InventoryController.ItemType inputType, int amount)
    {

      _output0Button.onClick.RemoveAllListeners();

      if (inputType == InventoryController.ItemType.NONE)
      {
        _output0Text.text = "";
        _output0Image.enabled = false;

        return;
      }

      _output0Text.text = $"x{amount}";
      _output0Image.sprite = InventoryController.GetItemSprite(inputType);
      _output0Image.enabled = true;

      _output0Button.onClick.AddListener(() =>
      {
        SetOutput(InventoryController.ItemType.NONE, 0);
        _currentOutputAmount = 0;

        InventoryController.s_Singleton.AddItemAmount(inputType, amount);

        if (!_cooking)
          _setRecipeButton.gameObject.SetActive(true);
      });
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
          SetInput(InventoryController.ItemType.NONE, 0, false);

          _recipeMenu.gameObject.SetActive(false);
          _startButton.gameObject.SetActive(false);
        });
      }
      else
      {
        img.sprite = InventoryController.GetItemSprite(recipe._OutputType);
        img.enabled = true;

        button.onClick.AddListener(() =>
        {
          _currentRecipe = recipe;
          SetInput(recipe._InputType, recipe._InputAmount, true);
          //SetOutput(recipe._OutputType, recipe._OutputAmount);

          _recipeMenu.gameObject.SetActive(false);

          _startButton.gameObject.SetActive(true);
          _startButton.onClick.RemoveAllListeners();
          _startButton.onClick.AddListener(() =>
          {
            if (InventoryController.s_Singleton.GetItemAmount(_currentRecipe._InputType) < _currentRecipe._InputAmount)
              return;

            _startButton.gameObject.SetActive(false);
            _setRecipeButton.gameObject.SetActive(false);

            SetInput(_currentRecipe._InputType, _currentRecipe._InputAmount, false);

            InventoryController.s_Singleton.RemoveItemAmount(_currentRecipe._InputType, _currentRecipe._InputAmount);

            _cooking = true;
          });
        });
      }
    }

  }

}