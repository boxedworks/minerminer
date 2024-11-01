using UnityEngine;
using System.Collections.Generic;

namespace Controllers
{

  //
  public class InventoryController : IHasInfoables
  {

    //
    public static InventoryController s_Singleton;

    //
    Transform _menu, _itemSelectorUi;
    GameObject _prefab;
    ItemType _selectedItem;
    bool _menuIsVisible { get { return MainBoxMenuController.s_Singleton.IsVisible(MainBoxMenuController.MenuType.INVENTORY); } }

    TMPro.TextMeshProUGUI _sellText;
    int _sellAmount;

    public enum ItemType
    {
      NONE,

      STONE,
      COPPER,
      COPPER_INGOT,
    }
    class ItemInfo : IInfoable
    {
      public string _Title;

      public int _AmountHeld;
      public bool _InInventory { get { return _AmountHeld > 0; } }

      public int _SellValue;

      public GameObject _MenuEntry;
      public TMPro.TextMeshProUGUI _Text;
      public ParticleController.ParticleType _ParticleType;

      public string _Info { get { return @$"<b>{_Title}</b>

Amount:     {_AmountHeld}
Sell Price: ${_SellValue}
      "; } }
      public RectTransform _Transform { get { return _MenuEntry.transform as RectTransform; } }
    }
    Dictionary<ItemType, ItemInfo> _itemInfos;

    //
    List<IInfoable> _otherInfoables;
    public InfoData _InfoData
    {
      get
      {
        var returnData = new InfoData();

        var returnList = new List<IInfoable>();
        foreach (var item in _itemInfos)
          if (item.Value._MenuEntry != null)
            returnList.Add(item.Value);
        foreach (var otherInfo in _otherInfoables)
          returnList.Add(otherInfo);
        returnData._Infos = returnList;

        if (_selectedItem != ItemType.NONE)
          returnData._DefaultInfo = _itemInfos[_selectedItem];

        return returnData;
      }
    }

    //
    public InventoryController()
    {
      s_Singleton = this;

      //
      _menu = MainBoxMenuController.s_Singleton.GetMenu(MainBoxMenuController.MenuType.INVENTORY).transform;
      _prefab = _menu.GetChild(0).GetChild(0).gameObject;
      _itemSelectorUi = _menu.GetChild(0).GetChild(0).GetChild(0);
      _sellText = _menu.GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
      UpdateSellText();

      _selectedItem = ItemType.NONE;

      _itemInfos = new();
      void AddInventoryItemInfo(ItemType inventoryItem, ItemInfo inventoryItemInfo)
      {
        _itemInfos.Add(inventoryItem, inventoryItemInfo);
      }

      AddInventoryItemInfo(ItemType.STONE, new ItemInfo()
      {
        _Title = "Stone",
        _SellValue = 1,

        _ParticleType = ParticleController.ParticleType.ORE_0,
      });

      AddInventoryItemInfo(ItemType.COPPER, new ItemInfo()
      {
        _Title = "Copper",
        _SellValue = 5,

        _ParticleType = ParticleController.ParticleType.ORE_1,
      });
      AddInventoryItemInfo(ItemType.COPPER_INGOT, new ItemInfo()
      {
        _Title = "Copper Ingot",
        _SellValue = 50
      });

      // Sell interface
      _otherInfoables = new();
      var buttonSell1 = _menu.GetChild(1).GetChild(1).GetChild(0).GetComponent<UnityEngine.UI.Button>();
      buttonSell1.onClick.AddListener(() =>
      {
        AudioController.PlayAudio("MenuSelect");

        if (_selectedItem == ItemType.NONE) return;

        SetSaleAmount(_sellAmount + 1);
        UpdateSellText();
      });
      _otherInfoables.Add(new SimpleInfoable()
      {
        _GameObject = buttonSell1.gameObject,
        _Description = @"<b>Sell 1</b>

Add 1 item to sell."
      });

      var buttonSell5 = _menu.GetChild(1).GetChild(1).GetChild(1).GetComponent<UnityEngine.UI.Button>();
      buttonSell5.onClick.AddListener(() =>
      {
        AudioController.PlayAudio("MenuSelect");

        if (_selectedItem == ItemType.NONE) return;

        SetSaleAmount(_sellAmount + 5);
        UpdateSellText();
      });
      _otherInfoables.Add(new SimpleInfoable()
      {
        _GameObject = buttonSell5.gameObject,
        _Description = @"<b>Sell 5</b>

Add 5 items to sell."
      });

      var buttonSell10 = _menu.GetChild(1).GetChild(1).GetChild(2).GetComponent<UnityEngine.UI.Button>();
      buttonSell10.onClick.AddListener(() =>
      {
        AudioController.PlayAudio("MenuSelect");

        if (_selectedItem == ItemType.NONE) return;

        SetSaleAmount(_sellAmount + 10);
        UpdateSellText();
      });
      _otherInfoables.Add(new SimpleInfoable()
      {
        _GameObject = buttonSell10.gameObject,
        _Description = @"<b>Sell 10</b>

Add 10 items to sell."
      });

      var buttonSell25 = _menu.GetChild(1).GetChild(1).GetChild(3).GetComponent<UnityEngine.UI.Button>();
      buttonSell25.onClick.AddListener(() =>
      {
        AudioController.PlayAudio("MenuSelect");

        if (_selectedItem == ItemType.NONE) return;

        SetSaleAmount(_sellAmount + 25);
        UpdateSellText();
      });
      _otherInfoables.Add(new SimpleInfoable()
      {
        _GameObject = buttonSell25.gameObject,
        _Description = @"<b>Sell 25<b>

Add 25 item to sell."
      });

      var buttonClear = _menu.GetChild(1).GetChild(1).GetChild(4).GetComponent<UnityEngine.UI.Button>();
      buttonClear.onClick.AddListener(() =>
      {
        AudioController.PlayAudio("MenuSelect");

        if (_selectedItem == ItemType.NONE) return;

        SetSaleAmount(0);
        UpdateSellText();
      });
      _otherInfoables.Add(new SimpleInfoable()
      {
        _GameObject = buttonClear.gameObject,
        _Description = @"<b>Clear</b>

Set amount of items to sell to: 0."
      });

      var buttonSell = _menu.GetChild(1).GetChild(1).GetChild(5).GetComponent<UnityEngine.UI.Button>();
      buttonSell.onClick.AddListener(() =>
      {
        AudioController.PlayAudio("MenuSelect");

        if (_selectedItem == ItemType.NONE) return;
        if (_sellAmount == 0) return;

        var itemInfo = _itemInfos[_selectedItem];
        var sellValueTotal = _sellAmount * itemInfo._SellValue;

        StatsController.s_Singleton._Gold += sellValueTotal;

        RemoveItemAmount(_selectedItem, _sellAmount);

        _sellAmount = Mathf.Clamp(_sellAmount, 0, itemInfo._AmountHeld);
        UpdateSellText();

        //
        if (!UpgradeController.HasUpgrade(UpgradeController.UpgradeType.SHOP))
          UpgradeController.UnlockUpgrade(UpgradeController.UpgradeType.SHOP);
      });
      _otherInfoables.Add(new SimpleInfoable()
      {
        _GameObject = buttonSell.gameObject,
        _Description = @"<b>Sell</b>

Sell items."
      });


    }

    //
    public void Update()
    {

      //
      if (_menuIsVisible)
      {
        var mousePos = Input.mousePosition;
        foreach (var inventoryItem in _itemInfos)
        {
          if (!inventoryItem.Value._InInventory) continue;

          if (Input.GetMouseButtonDown(0))
            if (RectTransformUtility.RectangleContainsScreenPoint(inventoryItem.Value._MenuEntry.transform as RectTransform, mousePos))
            {
              SelectItem(inventoryItem.Key);

              AudioController.PlayAudio("MenuSelect");
              break;
            }
        }
      }

    }

    //
    void SelectItem(ItemType inventoryItemType)
    {
      _selectedItem = inventoryItemType;

      var itemInfo = _itemInfos[inventoryItemType];
      _itemSelectorUi.SetParent(itemInfo._MenuEntry.transform, false);
      _itemSelectorUi.SetAsFirstSibling();

      // Set text
      _sellAmount = 0;
      UpdateSellText();
    }

    //
    void UpdateSellText()
    {

      if (_selectedItem == ItemType.NONE)
      {
        _sellText.text = "Sell:";
        return;
      }

      var itemInfo = _itemInfos[_selectedItem];

      var sellAmount = _sellAmount;
      var totalAmount = itemInfo._AmountHeld;

      var sellValue = itemInfo._SellValue;
      var totalSellPrice = sellAmount * sellValue;

      _sellText.text = string.Format("Sell:{0,50}", $"{sellAmount} / {totalAmount} = ${totalSellPrice}");
    }
    void SetSaleAmount(int toAmount)
    {
      if (_selectedItem == ItemType.NONE)
        return;

      var itemInfo = _itemInfos[_selectedItem];
      _sellAmount = Mathf.Clamp(toAmount, 0, itemInfo._AmountHeld);
    }

    //
    public int GetItemAmount(ItemType itemType)
    {
      return _itemInfos[itemType]._AmountHeld;
    }

    //
    public void AddItemAmount(ItemType itemType, int amount)
    {
      var itemInfo = _itemInfos[itemType];

      var addUi = itemInfo._AmountHeld == 0;
      itemInfo._AmountHeld += amount;

      if (addUi)
      {
        AddItemToDisplay(itemType);

        if (_selectedItem == ItemType.NONE)
          SelectItem(itemType);
      }
      else
      {
        UpdateItemDisplay(itemType);
      }
    }
    public void RemoveItemAmount(ItemType itemType, int amount)
    {

      var itemInfo = _itemInfos[itemType];
      itemInfo._AmountHeld -= amount;

      var isCurrentItem = _selectedItem == itemType;

      if (itemInfo._AmountHeld == 0)
      {
        if (isCurrentItem)
        {
          _selectedItem = ItemType.NONE;

          _itemSelectorUi.SetParent(_prefab.transform, false);
          _itemSelectorUi.SetAsFirstSibling();
        }

        GameObject.DestroyImmediate(itemInfo._MenuEntry);
      }
      else
        UpdateItemDisplay(itemType);

      if (isCurrentItem)
        UpdateSellText();
    }

    //
    void AddItemToDisplay(ItemType itemType)
    {

      var newMenuEntry = GameObject.Instantiate(_prefab, _prefab.transform.parent);
      if (_selectedItem == ItemType.NONE)
        GameObject.DestroyImmediate(newMenuEntry.transform.GetChild(0).gameObject);
      _itemInfos[itemType]._MenuEntry = newMenuEntry;
      _itemInfos[itemType]._Text = newMenuEntry.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();

      UpdateItemDisplay(itemType);

      newMenuEntry.gameObject.SetActive(true);
    }
    void UpdateItemDisplay(ItemType inventoryItemType)
    {
      var itemInfo = _itemInfos[inventoryItemType];
      itemInfo._Text.text = string.Format("{0,-18}{1,-10}{2,6}", $"{itemInfo._Title}", $"{itemInfo._AmountHeld}", $"${itemInfo._SellValue}");

      if (_selectedItem == inventoryItemType)
        UpdateSellText();
    }

    //
    public static Sprite GetItemSprite(ItemType itemType)
    {
      return Resources.Load<ItemResourceInfo>($"Items/{itemType.ToString().ToLower()}").Sprite;
    }
    public static ParticleController.ParticleType GetItemParticles(ItemType itemType)
    {
      var itemInfo = s_Singleton._itemInfos[itemType];
      return itemInfo._ParticleType;
    }

  }

}