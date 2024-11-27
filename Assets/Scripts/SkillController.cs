using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework.Constraints;

namespace Controllers
{

  //
  public class SkillController : IHasInfoables
  {

    //
    public static SkillController s_Singleton;

    //
    public static float s_XpMultiplier { get { return s_Singleton._xpMultiplier; } set { s_Singleton._xpMultiplier = value; } }
    float _xpMultiplier;

    //
    Transform _menu, _prefab;
    bool _menuIsVisible { get { return MainBoxMenuController.s_Singleton.IsVisible(MainBoxMenuController.MenuType.SKILLS); } }

    //
    int _gold, _goldVisual;
    public int _Gold
    {
      get { return _gold; }
      set
      {
        _gold = value;

        ShopController.UpdatePurchasesUi();
      }
    }

    int _totalLevel
    {
      get
      {
        var total = 0;
        foreach (var skill in _skills)
          total += skill._Level;
        return total;
      }
    }

    //
    public enum SkillType
    {
      NONE,

      DAMAGE,
      SPEED,
      LUCK,
      POWER,

      HEAT
    }

    //
    List<Skill> _skills;
    int _currentSkill;
    Transform _skillSelectorUi;

    TMPro.TextMeshProUGUI _totalLevelText;

    //
    public InfoData _InfoData
    {
      get
      {
        var returnData = new InfoData();

        var returnList = new List<IInfoable>();
        foreach (var skill in _skills)
          returnList.Add(skill);
        returnData._Infos = returnList;

        returnData._DefaultInfo = _skills[_currentSkill];

        return returnData;
      }
    }

    //
    public SkillController()
    {
      s_Singleton = this;

      //
      _xpMultiplier = 1f;

      //
      _Gold = 0;

      //
      _menu = MainBoxMenuController.s_Singleton.GetMenu(MainBoxMenuController.MenuType.SKILLS).transform;
      _prefab = _menu.GetChild(0);

      _totalLevelText = _menu.GetChild(1).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();

      _skillSelectorUi = _menu.GetChild(0).GetChild(0);

      string getSafeFloat(float inFloat)
      {
        return inFloat.ToString("0.00");
      }

      _skills = new() {
        new Skill(SkillType.DAMAGE, "Strength",
          (int level) => {
            return 1f + level * 0.5f;
          },
          (Skill skill, int level, float xp, float xpMax) => {
            return InfoController.GetInfoString("Strength", @$"Increases your pickaxe's damage.

Level:  {level} / 99
Xp:     {getSafeFloat(xp)} / {getSafeFloat(xpMax)}
Damage: {getSafeFloat(skill._OnMaths)}");
        }),

        new Skill(SkillType.SPEED, "Speed",
          (int level) => {
            return 1f + level * 0.05f;
          },
          (Skill skill, int level, float xp, float xpMax) => {
            return InfoController.GetInfoString("Speed", @$"Increases your pickaxe's speed.

Level: {level} / 99
Xp:    {getSafeFloat(xp)} / {getSafeFloat(xpMax)}
Speed: {getSafeFloat(skill._OnMaths * 100)}%");
        }),

        new Skill(SkillType.LUCK, "Luck",
          (int level) => {
            return level / 99f * 5f;
          },
          (Skill skill, int level, float xp, float xpMax) => {
            return InfoController.GetInfoString("Luck", @$"Percent chance to get 1 extra item when damaging a rock.

Level:  {level} / 99
Xp:     {getSafeFloat(xp)} / {getSafeFloat(xpMax)}
Chance: {getSafeFloat(skill._OnMaths * 100)}%");
        }),

        new Skill(SkillType.POWER, "Power",
          (int level) => {
            return level / 99f * 5f;
          },
          (Skill skill, int level, float xp, float xpMax) => {
            return InfoController.GetInfoString("Power", @$"Percent chance to do 2x damage on hit.

Level:  {level} / 99
Xp:     {getSafeFloat(xp)} / {getSafeFloat(xpMax)}
Chance: {getSafeFloat(skill._OnMaths * 100)}%");
        }),

        new Skill(SkillType.HEAT, "Heat",
          (int level) => {
            return 1f + level * 0.05f;
          },
          (Skill skill, int level, float xp, float xpMax) => {
            return InfoController.GetInfoString("Heat", @$"Increases forge speed.

Level: {level} / 99
Xp:    {getSafeFloat(xp)} / {getSafeFloat(xpMax)}
Speed: {getSafeFloat(skill._OnMaths * 100)}%");
        }),

      };

      SetSkillSelector(0);
      ToggleSkill(SkillType.DAMAGE, true);
      ToggleSkill(SkillType.SPEED, true);
    }

    public void AddXp(float xpAmount)
    {
      if (!MainBoxMenuController.s_Singleton.IsMenuActive(MainBoxMenuController.MenuType.SKILLS))
        return;
      _skills[_currentSkill].AddXp(xpAmount);
    }
    void SetSkillSelector(int index)
    {
      _currentSkill = index;

      _skillSelectorUi.SetParent(_skills[index]._MenuEntry, false);
      _skillSelectorUi.SetAsFirstSibling();
    }

    //
    public void Update()
    {

      // Check new skill selection
      if (_menuIsVisible)
      {
        var skillIndex = -1;
        var mousePos = Input.mousePosition;
        foreach (var skill in _skills)
        {
          skillIndex++;

          if (Input.GetMouseButtonDown(0))
            if (RectTransformUtility.RectangleContainsScreenPoint(skill._MenuEntry as RectTransform, mousePos, Camera.main))
            {
              SetSkillSelector(skillIndex);

              AudioController.PlayAudio("MenuSelect");
              break;
            }

        }

      }

      //
      foreach (var skill in _skills)
      {
        skill.Update();
      }

      // Gold
      if (_goldVisual != _gold)
      {
        _goldVisual += _goldVisual > _gold ? Mathf.FloorToInt((Mathf.Abs(_gold) - Mathf.Abs(_goldVisual)) * 0.02f) : Mathf.CeilToInt((Mathf.Abs(_gold) - Mathf.Abs(_goldVisual)) * 0.02f);// * (_goldVisual < _gold ? 1 : -1);
        GameObject.Find("Money").transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = $"${_goldVisual}";
      }
    }

    void UpdateTotalLevel()
    {
      var totalLevel = _totalLevel;
      _totalLevelText.text = $"Total level: {totalLevel}";

      PrestiegeController.OnTotalLevelUpdate(totalLevel);
    }

    //
    public static float GetMaths(SkillType skillType)
    {
      return s_Singleton._skills[((int)skillType) - 1]._OnMaths;
    }

    //
    public static void ToggleSkill(SkillType skillType, bool toggle)
    {
      s_Singleton._skills[((int)skillType) - 1].ToggleVisibility(toggle);
    }

    //
    public class Skill : IInfoable
    {

      SkillType _skillType;
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

      public Skill(SkillType skillType, string name, System.Func<int, float> onMaths, System.Func<Skill, int, float, float, string> onInfo)
      {
        _skillType = skillType;
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
        if (_level >= 99 && _xp > 0)
        {
          _xp = 0f;
          _xpVisual = _xpMax;
          _level = 99;

          UpdateUi();
        }

        else if (_xp > 0f)
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

            s_Singleton.UpdateTotalLevel();
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

        info.SkillName = _skillType.ToString();
        info.Level = _level;
        info.Xp = _xpVisual;
        info.XpLeft = _xp;
        info.XpMax = _xpMax;

        return info;
      }
      public void SetSkillInfo(SkillSaveInfo skillInfo)
      {
        _level = Mathf.Clamp(skillInfo.Level, 0, 99);
        if (_level != 99)
        {
          _xpVisual = skillInfo.Xp;
          _xp = skillInfo.XpLeft;
          _xpMax = skillInfo.XpMax;
        }
        else
          _xpVisual = _xpMax;

        UpdateUi();
      }
    }

    //
    [System.Serializable]
    public class SkillSaveInfo
    {
      public string SkillName;
      public int Level;
      public float Xp, XpLeft, XpMax;
    }
    public static List<SkillSaveInfo> GetSkills()
    {
      var returnList = new List<SkillSaveInfo>();
      foreach (var skill in s_Singleton._skills)
        returnList.Add(skill.GetSkillInfo());
      return returnList;
    }
    public static void SetSkills(List<SkillSaveInfo> skillInfos)
    {
      foreach (var skillInfo in skillInfos)
      {
        if (System.Enum.TryParse(skillInfo.SkillName, true, out SkillType skillType))
          s_Singleton._skills[((int)skillType) - 1].SetSkillInfo(skillInfo);
      }

      s_Singleton.UpdateTotalLevel();
    }

    //
    [System.Serializable]
    public class SaveInfo
    {
      public string ActiveSkill;
    }
    public static SaveInfo GetSaveInfo()
    {
      var saveInfo = new SaveInfo();

      saveInfo.ActiveSkill = ((SkillType)s_Singleton._currentSkill + 1).ToString();

      return saveInfo;
    }
    public static void SetSaveInfo(SaveInfo saveInfo)
    {
      if (System.Enum.TryParse(saveInfo.ActiveSkill, true, out SkillType skillType))
      {
        s_Singleton.SetSkillSelector(((int)skillType) - 1);
      }
    }

  }

}