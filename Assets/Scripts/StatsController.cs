using UnityEngine;
using System.Collections.Generic;
using Packages;
using System.Runtime.InteropServices;

namespace Controllers
{

  //
  public class StatsController : IHasInfoables
  {

    //
    public static StatsController s_Singleton;

    //
    Transform _menu, _prefab;
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
      SPEED,
      LUCK,
      POWER,

      HEAT
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
      _prefab = _menu.GetChild(0);

      _skillStatSelectorUi = _menu.GetChild(0).GetChild(0);

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
            return 1f + level * 0.05f;
          },
          (Skill skill, int level, float xp, float xpMax) => {
            return InfoController.GetInfoString("Speed", @$"Increase your pickaxe's speed.

Level: {level} / 99
Xp:    {getSafeFloat(xp)} / {getSafeFloat(xpMax)}
Speed: {getSafeFloat(skill._OnMaths * 100)}%");
        }),

        new Skill(StatType.LUCK, "Luck",
          (int level) => {
            return level / 99f * 5f;
          },
          (Skill skill, int level, float xp, float xpMax) => {
            return InfoController.GetInfoString("Luck", @$"Percent chance to get 1 extra item when damaging a rock.

Level:  {level} / 99
Xp:     {getSafeFloat(xp)} / {getSafeFloat(xpMax)}
Chance: {getSafeFloat(skill._OnMaths * 100)}%");
        }),

        new Skill(StatType.POWER, "Power",
          (int level) => {
            return level / 99f;
          },
          (Skill skill, int level, float xp, float xpMax) => {
            return InfoController.GetInfoString("Power", @$"Percent chance to do 2x damage on hit.

Level:  {level} / 99
Xp:     {getSafeFloat(xp)} / {getSafeFloat(xpMax)}
Chance: {getSafeFloat(skill._OnMaths * 100)}%");
        }),

      };

      SetSkillStatSelector(0);
      ToggleStat(StatType.DAMAGE, true);
      ToggleStat(StatType.SPEED, true);
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
    public static void ToggleStat(StatType statType, bool toggle)
    {
      s_Singleton._skillStats[((int)statType) - 1].ToggleVisibility(toggle);
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

        _uiComponents = GameObject.Instantiate(s_Singleton._prefab.gameObject, s_Singleton._prefab.parent).transform;

        GameObject.DestroyImmediate(_uiComponents.GetChild(0).gameObject);

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

            LogController.AppendLog($"Skill leveled: {_name} ({_level - 1} -> {_level})");
          }

          UpdateUi();
        }
      }

      //
      public void ToggleVisibility(bool toggle)
      {
        _uiComponents.gameObject.SetActive(toggle);
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

      //
      public SkillSaveInfo GetSkillInfo()
      {
        var info = new SkillSaveInfo();

        info.SkillType = _statType;
        info.Level = _level;
        info.Xp = _xpVisual;
        info.XpLeft = _xp;
        info.XpMax = _xpMax;

        return info;
      }
      public void SetSkillInfo(SkillSaveInfo skillInfo)
      {
        _level = skillInfo.Level;
        _xpVisual = skillInfo.Xp;
        _xpMax = skillInfo.XpLeft;
        _xpMax = skillInfo.XpMax;

        UpdateUi();
      }
    }

    //
    [System.Serializable]
    public class SkillSaveInfo
    {
      public StatType SkillType;
      public int Level;
      public float Xp, XpLeft, XpMax;
    }
    public static List<SkillSaveInfo> GetSkills()
    {
      var returnList = new List<SkillSaveInfo>();
      foreach (var skill in s_Singleton._skillStats)
        returnList.Add(skill.GetSkillInfo());
      return returnList;
    }
    public static void SetSkills(List<SkillSaveInfo> skillInfos)
    {
      foreach (var skillInfo in skillInfos)
        s_Singleton._skillStats[((int)skillInfo.SkillType) - 1].SetSkillInfo(skillInfo);
    }

  }

}