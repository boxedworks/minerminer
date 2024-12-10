using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

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

    int _inventoryPage;
    TMPro.TextMeshProUGUI _pageText;
    int _totalInventoryPages
    {
      get
      {
        return s_Singleton._itemInfos
          .Where((i) => { return i.Value._InInventory; })
          .Count() / (10 + 1);
      }
    }

    public enum ItemType
    {
      NONE,

      STONE,
      STONE_CHUNK,

      COPPER,
      COPPER_INGOT,

      TIN,
      BRONZE_INGOT,

      IRON,
      IRON_INGOT,

      COAL,
      STEEL_INGOT,

      EMERALD,
      SAPPHIRE,
      RUBY,
      CITRINE,
      DIAMOND,

      STONE_DUST,
      GEM_DUST_0,

      MIX_0,
      GEM_INGOT_0,

      COBALT, CINNABAR,

    }
    class ItemInfo : IInfoable
    {
      public ItemType _ItemType;
      public string _Title;

      public int _AmountHeld;
      public bool _InInventory { get { return _AmountHeld > 0; } }

      public int _SellValue;

      public GameObject _MenuEntry;
      public TMPro.TextMeshProUGUI _Text;
      public ParticleController.ParticleType _ParticleType;

      public string _Info
      {
        get
        {
          return InfoController.GetInfoString(_Title, @$"Amount:      {_AmountHeld}
Sell Price: ${_SellValue}

Sum Value:  ${GetItemValue(_ItemType, _AmountHeld)}");
        }
      }
      public RectTransform _Transform { get { return _MenuEntry == null ? null : _MenuEntry.transform as RectTransform; } }
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

      _selectedItem = ItemType.NONE;

      _itemInfos = new();
      void AddInventoryItemInfo(ItemType itemType, ItemInfo itemInfo)
      {
        itemInfo._ItemType = itemType;
        _itemInfos.Add(itemType, itemInfo);
      }

      AddInventoryItemInfo(ItemType.STONE, new ItemInfo()
      {
        _Title = "Stone",
        _SellValue = 1,

        _ParticleType = ParticleController.ParticleType.ORE_0,
      });
      AddInventoryItemInfo(ItemType.STONE_CHUNK, new ItemInfo()
      {
        _Title = "Stone Chunk",
        _SellValue = 40,

        _ParticleType = ParticleController.ParticleType.ORE_5,
      });
      AddInventoryItemInfo(ItemType.STONE_DUST, new ItemInfo()
      {
        _Title = "Stone Dust",
        _SellValue = 4
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
        _SellValue = 65
      });

      AddInventoryItemInfo(ItemType.TIN, new ItemInfo()
      {
        _Title = "Tin",
        _SellValue = 10,

        _ParticleType = ParticleController.ParticleType.ORE_2,
      });
      AddInventoryItemInfo(ItemType.BRONZE_INGOT, new ItemInfo()
      {
        _Title = "Bronze Ingot",
        _SellValue = 200
      });

      AddInventoryItemInfo(ItemType.IRON, new ItemInfo()
      {
        _Title = "Iron",
        _SellValue = 15,

        _ParticleType = ParticleController.ParticleType.ORE_3,
      });
      AddInventoryItemInfo(ItemType.IRON_INGOT, new ItemInfo()
      {
        _Title = "Iron Ingot",
        _SellValue = 750
      });

      AddInventoryItemInfo(ItemType.COAL, new ItemInfo()
      {
        _Title = "Coal",
        _SellValue = 35,

        _ParticleType = ParticleController.ParticleType.ORE_4,
      });
      AddInventoryItemInfo(ItemType.STEEL_INGOT, new ItemInfo()
      {
        _Title = "Steel Ingot",
        _SellValue = 4500
      });

      AddInventoryItemInfo(ItemType.EMERALD, new ItemInfo()
      {
        _Title = "Emerald",
        _SellValue = 15,

        _ParticleType = ParticleController.ParticleType.GEM_0
      });
      AddInventoryItemInfo(ItemType.SAPPHIRE, new ItemInfo()
      {
        _Title = "Sapphire",
        _SellValue = 35,

        _ParticleType = ParticleController.ParticleType.GEM_1

      });
      AddInventoryItemInfo(ItemType.RUBY, new ItemInfo()
      {
        _Title = "Ruby",
        _SellValue = 85,

        _ParticleType = ParticleController.ParticleType.GEM_2

      });
      AddInventoryItemInfo(ItemType.CITRINE, new ItemInfo()
      {
        _Title = "Citrine",
        _SellValue = 150,

        _ParticleType = ParticleController.ParticleType.GEM_3

      });
      AddInventoryItemInfo(ItemType.DIAMOND, new ItemInfo()
      {
        _Title = "Diamond",
        _SellValue = 250,

        _ParticleType = ParticleController.ParticleType.GEM_4

      });

      AddInventoryItemInfo(ItemType.GEM_DUST_0, new ItemInfo()
      {
        _Title = "Citrine Dust",
        _SellValue = 450,
      });

      AddInventoryItemInfo(ItemType.MIX_0, new ItemInfo()
      {
        _Title = "Orange Blend",
        _SellValue = 1500,
      });
      AddInventoryItemInfo(ItemType.GEM_INGOT_0, new ItemInfo()
      {
        _Title = "Citrine Ingot",
        _SellValue = 8500,
      });

      AddInventoryItemInfo(ItemType.COBALT, new ItemInfo()
      {
        _Title = "Cobalt",
        _SellValue = 6,

        _ParticleType = ParticleController.ParticleType.ORE_6,
      });
      AddInventoryItemInfo(ItemType.CINNABAR, new ItemInfo()
      {
        _Title = "Cinnabar",
        _SellValue = 3,

        _ParticleType = ParticleController.ParticleType.ORE_7,
      });

      // Sell interface
      _otherInfoables = new();
      var buttonSell1 = _menu.GetChild(1).GetChild(1).GetChild(0).GetComponent<Button>();
      buttonSell1.onClick.AddListener(() =>
      {
        AudioController.PlayAudio("MenuSelect");

        SellItem(1);
      });
      _otherInfoables.Add(new SimpleInfoable()
      {
        _GameObject = buttonSell1.gameObject,
        _Description = InfoController.GetInfoString("Sell 1", "Sell 1 of the selected item.")
      });

      var buttonSell5 = _menu.GetChild(1).GetChild(1).GetChild(1).GetComponent<Button>();
      buttonSell5.onClick.AddListener(() =>
      {
        AudioController.PlayAudio("MenuSelect");

        SellItem(5);
      });
      _otherInfoables.Add(new SimpleInfoable()
      {
        _GameObject = buttonSell5.gameObject,
        _Description = InfoController.GetInfoString("Sell 5", "Sell 5 of the selected item.")
      });

      var buttonSellAll = _menu.GetChild(1).GetChild(1).GetChild(2).GetComponent<Button>();
      buttonSellAll.onClick.AddListener(() =>
      {
        AudioController.PlayAudio("MenuSelect");

        SellItem(10000000);
      });
      _otherInfoables.Add(new SimpleInfoable()
      {
        _GameObject = buttonSellAll.gameObject,
        _Description = InfoController.GetInfoString("Sell All", "Sell all of the selected item.\n\nYou can also use middle mouse on an inventory item to do this.")
      });

      // Page buttons
      _pageText = _menu.GetChild(1).GetChild(2).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
      var buttonPageLeft = _menu.GetChild(1).GetChild(2).GetChild(1).GetComponent<Button>();
      buttonPageLeft.onClick.AddListener(() =>
      {
        AudioController.PlayAudio("MenuSelect");

        PageLeft();
      });
      _otherInfoables.Add(new SimpleInfoable()
      {
        _GameObject = buttonPageLeft.gameObject,
        _Description = InfoController.GetInfoString("Page left [<]", "")
      });

      var buttonPageRight = _menu.GetChild(1).GetChild(2).GetChild(2).GetComponent<Button>();
      buttonPageRight.onClick.AddListener(() =>
      {
        AudioController.PlayAudio("MenuSelect");

        PageRight();
      });
      _otherInfoables.Add(new SimpleInfoable()
      {
        _GameObject = buttonPageRight.gameObject,
        _Description = InfoController.GetInfoString("Page right [>]", "")
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
          if (inventoryItem.Value._MenuEntry == null) continue;

          if (Input.GetMouseButtonUp(0))
            if (RectTransformUtility.RectangleContainsScreenPoint(inventoryItem.Value._MenuEntry.transform as RectTransform, mousePos, Camera.main))
            {
              SelectItem(inventoryItem.Key);

              AudioController.PlayAudio("MenuSelect");
              break;
            }

          if (Input.GetMouseButtonUp(2))
            if (RectTransformUtility.RectangleContainsScreenPoint(inventoryItem.Value._MenuEntry.transform as RectTransform, mousePos, Camera.main))
            {
              SelectItem(inventoryItem.Key);
              SellItem(_itemInfos[inventoryItem.Key]._AmountHeld);

              AudioController.PlayAudio("MenuSelect");
              break;
            }
        }
      }

    }

    //
    void SellItem(int tryAmount)
    {
      AudioController.PlayAudio("MenuSelect");

      if (_selectedItem == ItemType.NONE)
      {
        LogController.AppendLog($"<color=red>No item selected to sell!</color>");

        return;
      }

      var itemInfo = _itemInfos[_selectedItem];
      var sellAmount = Mathf.Clamp(tryAmount, 0, itemInfo._AmountHeld);
      if (sellAmount == 0) return;

      var sellValueTotal = sellAmount * itemInfo._SellValue;

      LogController.AppendLog($"Sold {sellAmount} {GetItemName(_selectedItem)} for ${sellValueTotal}.");

      SkillController.s_Singleton._Gold += sellValueTotal;
      AudioController.PlayAudio("SellItems");

      RemoveItemAmount(_selectedItem, sellAmount);

      //
      if (!UnlockController.HasUnlock(UnlockController.UnlockType.SHOP))
      {
        UnlockController.Unlock(UnlockController.UnlockType.SHOP);

        LogController.AppendLog("Unlocked the Shop menu.");
      }
    }

    //
    void SelectItem(ItemType inventoryItemType)
    {
      var itemInfo = _itemInfos[inventoryItemType];
      if (itemInfo._MenuEntry == null) return;

      _selectedItem = inventoryItemType;

      _itemSelectorUi.SetParent(itemInfo._MenuEntry.transform, false);
      _itemSelectorUi.SetAsFirstSibling();
    }

    //
    public static int GetItemAmount(ItemType itemType)
    {
      return s_Singleton._itemInfos[itemType]._AmountHeld;
    }

    //
    public void AddItemAmount(ItemType itemType, int amount)
    {
      if (amount < 1)
      {
        Debug.LogWarning($"Trying to add {amount} {itemType} to inventory");
        return;
      }

      var itemInfo = _itemInfos[itemType];

      var addUi = itemInfo._AmountHeld == 0;
      itemInfo._AmountHeld += amount;

      if (addUi)
      {
        UpdateInventoryDisplay();

        if (_selectedItem == ItemType.NONE)
          SelectItem(itemType);
      }
      else
        UpdateItemDisplay(itemType);

      //
      ShopController.UpdatePurchasesUi();
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

        UpdateInventoryDisplay();
      }
      else
        UpdateItemDisplay(itemType);

      //
      ShopController.UpdatePurchasesUi();
    }

    //
    void UpdateInventoryDisplay()
    {

      var itemsPerPage = 10;

      // Clean up old
      foreach (var itemInfo in _itemInfos)
        itemInfo.Value._MenuEntry = null;
      var prefabContainer = _prefab.transform.parent;
      for (var i = prefabContainer.childCount - 1; i > 1; i--)
        prefabContainer.GetChild(i).gameObject.SetActive(false);

      // Update current page
      var selectedItemFound = false;
      var itemIndex = 0;
      var menuIndex = 0;
      foreach (var itemInfo in _itemInfos)
      {
        if (!itemInfo.Value._InInventory) continue;
        if (itemIndex++ < itemsPerPage * _inventoryPage) continue;

        GameObject menuEntry;
        if (menuIndex < prefabContainer.childCount - 2)
          menuEntry = prefabContainer.GetChild(menuIndex + 2).gameObject;
        else
        {
          menuEntry = GameObject.Instantiate(_prefab, prefabContainer);
          if (_selectedItem == ItemType.NONE)
            GameObject.DestroyImmediate(menuEntry.transform.GetChild(0).gameObject);
        }

        if (_selectedItem == itemInfo.Key)
        {
          selectedItemFound = true;

          _itemSelectorUi.SetParent(menuEntry.transform, false);
          _itemSelectorUi.SetAsFirstSibling();
        }

        var offset = 0;
        if (menuEntry.transform.GetChild(0).gameObject == _itemSelectorUi.gameObject)
          offset = 1;

        itemInfo.Value._MenuEntry = menuEntry;
        itemInfo.Value._Text = menuEntry.transform.GetChild(offset + 1).GetComponent<TMPro.TextMeshProUGUI>();

        var image = menuEntry.transform.GetChild(offset + 2).GetComponent<Image>();
        image.sprite = GetItemSprite(itemInfo.Key);

        UpdateItemDisplay(itemInfo.Key);

        menuEntry.SetActive(true);

        if (++menuIndex == itemsPerPage) break;
      }

      // Make sure not on empty page
      if (menuIndex == 0 && _inventoryPage > 0)
        PageLeft();

      // Check selected item not on this page
      if (!selectedItemFound)
      {
        _itemSelectorUi.SetParent(_prefab.transform, false);
        _itemSelectorUi.SetAsFirstSibling();
      }

      //
      UpdatePageText();
    }

    void UpdateItemDisplay(ItemType inventoryItemType)
    {
      var itemInfo = _itemInfos[inventoryItemType];
      if (itemInfo._MenuEntry == null) return;
      itemInfo._Text.text = string.Format("   {0,-34}{1,-10}{2,8}", $"{itemInfo._Title}", $"{itemInfo._AmountHeld}", $"${itemInfo._SellValue}");
    }

    //
    public static void PageLeft()
    {
      if (s_Singleton._inventoryPage <= 0)
        return;

      s_Singleton._inventoryPage--;
      s_Singleton.UpdateInventoryDisplay();

      s_Singleton.UpdatePageText();
    }
    public static void PageRight()
    {
      if (s_Singleton._inventoryPage >= s_Singleton._totalInventoryPages)
        return;

      s_Singleton._inventoryPage++;
      s_Singleton.UpdateInventoryDisplay();

      s_Singleton.UpdatePageText();
    }
    void UpdatePageText()
    {
      _pageText.text = $"{_inventoryPage + 1}/{_totalInventoryPages + 1}";
    }

    //
    public static string GetItemName(ItemType itemType)
    {
      return s_Singleton._itemInfos[itemType]._Title;
    }
    public static int GetItemValue(ItemType itemType, int amount)
    {
      var itemInfo = s_Singleton._itemInfos[itemType];
      return itemInfo._SellValue * amount;
    }

    public static Sprite GetItemSprite(ItemType itemType)
    {
      return Resources.Load<ItemResourceInfo>($"Items/{itemType.ToString().ToLower()}").Sprite;
    }
    public static ParticleController.ParticleType GetItemParticles(ItemType itemType)
    {
      var itemInfo = s_Singleton._itemInfos[itemType];
      return itemInfo._ParticleType;
    }

    //
    [System.Serializable]
    public class SaveInfo
    {
      public List<string> ItemList;
      public List<int> AmountList;
    }
    public static SaveInfo GetSaveInfo()
    {
      var saveInfo = new SaveInfo();

      saveInfo.ItemList = new();
      saveInfo.AmountList = new();
      foreach (var item in s_Singleton._itemInfos)
      {
        if (item.Value._InInventory)
        {
          saveInfo.ItemList.Add(item.Key.ToString());
          saveInfo.AmountList.Add(item.Value._AmountHeld);
        }
      }

      return saveInfo;
    }
    public static void SetSaveInfo(SaveInfo saveInfo)
    {

      var index = -1;
      foreach (var item in saveInfo.ItemList)
      {
        index++;

        if (System.Enum.TryParse(item, true, out ItemType itemType))
          s_Singleton.AddItemAmount(itemType, saveInfo.AmountList[index]);
      }

      //
      s_Singleton.UpdatePageText();
    }


  }

}