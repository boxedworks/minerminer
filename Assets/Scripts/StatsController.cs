using UnityEngine;
using System.Collections.Generic;

namespace Controllers
{

  //
  public class StatsController : IHasInfoables
  {

    //
    public static StatsController s_Singleton;

    //
    Transform _menu;
    bool _menuIsVisible { get { return MainBoxMenuController.s_Singleton.IsVisible(MainBoxMenuController.MenuType.SKILLS); } }

    //
    int _gold;
    public int _Gold
    {
      get { return _gold; }
      set
      {
        _gold = value;
        GameObject.Find("Money").transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = $"${_gold}";

        ShopController.UpdatePurchasesUi();
      }
    }

    //
    public enum StatType
    {
      NONE,

      DAMAGE,
      SPEED
    }

    //
    List<Skill> _skillStats;
    int _currentSkillStat;
    Transform _skillStatSelectorUi;

    //
    public InfoData _InfoData
    {
      get
      {
        var returnData = new InfoData();

        var returnList = new List<IInfoable>();
        foreach (var skill in _skillStats)
          returnList.Add(skill);
        returnData._Infos = returnList;

        returnData._DefaultInfo = _skillStats[_currentSkillStat];

        return returnData;
      }
    }

    //
    public StatsController()
    {
      s_Singleton = this;

      //
      _Gold = 0;

      //
      _menu = MainBoxMenuController.s_Singleton.GetMenu(MainBoxMenuController.MenuType.SKILLS).transform;
      _skillStatSelectorUi = _menu.GetChild(0).GetChild(0);
      _skillStatSelectorUi.SetAsLastSibling();

      string getSafeFloat(float inFloat)
      {
        return inFloat.ToString("0.00");
      }

      _skillStats = new() {
        new Skill(StatType.DAMAGE, "Strength",
          (int level) => {
            return 1f + level * 0.5f;
          },
          (Skill skill, int level, float xp, float xpMax) => {
            return InfoController.GetInfoString("Strength", @$"Increase your pickaxe's damage.

Level:  {level} / 99
Xp:     {getSafeFloat(xp)} / {getSafeFloat(xpMax)}
Damage: {getSafeFloat(skill._OnMaths)}");
        }),

        new Skill(StatType.SPEED, "Speed",
          (int level) => {
            return (1f + level * 0.05f) * 0.25f;
          },
          (Skill skill, int level, float xp, float xpMax) => {
            return InfoController.GetInfoString("Speed", @$"Increase your pickaxe's speed.

Level: {level} / 99
Xp:    {getSafeFloat(xp)} / {getSafeFloat(xpMax)}
Speed: {getSafeFloat(skill._OnMaths * 100)}%");
        }),

      };
      SetSkillStatSelector(0);
    }

    public void AddXp(float xpAmount)
    {
      if (!MainBoxMenuController.s_Singleton.IsMenuActive(MainBoxMenuController.MenuType.SKILLS))
        return;
      _skillStats[_currentSkillStat].AddXp(xpAmount);
    }
    void SetSkillStatSelector(int index)
    {
      _currentSkillStat = index;

      _skillStatSelectorUi.SetParent(_skillStats[index]._MenuEntry, false);
      _skillStatSelectorUi.SetAsFirstSibling();
    }

    //
    public void Update()
    {

      // Check new skill selection
      if (_menuIsVisible)
      {
        var skillIndex = -1;
        var mousePos = Input.mousePosition;
        foreach (var skillstat in _skillStats)
        {
          skillIndex++;

          if (Input.GetMouseButtonDown(0))
            if (RectTransformUtility.RectangleContainsScreenPoint(skillstat._MenuEntry as RectTransform, mousePos, Camera.main))
            {
              SetSkillStatSelector(skillIndex);

              AudioController.PlayAudio("MenuSelect");
              break;
            }

        }

      }

      //
      foreach (var skillstat in _skillStats)
      {
        skillstat.Update();
      }
    }

    //
    public static float GetMaths(StatType statType)
    {
      return s_Singleton._skillStats[((int)statType) - 1]._OnMaths;
    }

    //
    public class Skill : IInfoable
    {

      StatType _statType;
      string _name;

      float _xp, _xpVisual, _xpMax;
      int _level;
      public int _Level { get { return _level; } }

      Transform _uiComponents;
      public Transform _MenuEntry { get { return _uiComponents; } }
      TMPro.TextMeshProUGUI _uiText;
      UnityEngine.UI.Slider _uiSlider;

      System.Func<Skill, int, float, float, string> _onInfo;
      public string _Info { get { return _onInfo.Invoke(this, _level, _xpVisual, _xpMax); } }
      public RectTransform _Transform { get { return _MenuEntry as RectTransform; } }

      System.Func<int, float> _onMaths;
      public float _OnMaths { get { return _onMaths.Invoke(_level); } }

      public Skill(StatType statType, string name, System.Func<int, float> onMaths, System.Func<Skill, int, float, float, string> onInfo)
      {
        _statType = statType;
        _name = name;

        _onMaths = onMaths;
        _onInfo = onInfo;

        _xpMax = 10;

        _uiComponents = MainBoxMenuController.s_Singleton.GetMenu(MainBoxMenuController.MenuType.SKILLS).transform.Find(_name);
        _uiText = _uiComponents.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
        _uiSlider = _uiComponents.GetChild(2).GetComponent<UnityEngine.UI.Slider>();

        UpdateUi();
      }

      //
      public void Update()
      {
        if (_xp > 0f)
        {

          float xpAmount;
          if (_xp < _xpMax * 0.005f)
            xpAmount = _xp;
          else
            xpAmount = Mathf.Clamp(_xp * 0.75f * Time.deltaTime, 0f, _xp);

          _xp -= xpAmount;
          _xpVisual += xpAmount;

          if (System.Math.Round(_xpVisual, 2) >= System.Math.Round(_xpMax, 2))
          {
            _xpVisual -= _xpMax;

            _level++;
            _xpMax += _level;
          }

          UpdateUi();
        }
      }

      //
      public void AddXp(float xpAmount)
      {
        _xp += xpAmount;
      }

      //
      void UpdateUi()
      {
        _uiText.text = $"{_name} - {_level}/99";
        _uiSlider.value = _xpVisual / _xpMax;
      }
    }

  }

}