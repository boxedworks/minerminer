using UnityEngine;
using System.Collections.Generic;
using Packages;
using System.Runtime.InteropServices;

namespace Controllers
{

  //
  public class SkillController : IHasInfoables
  {

    //
    public static SkillController s_Singleton;

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
      _Gold = 0;

      //
      _menu = MainBoxMenuController.s_Singleton.GetMenu(MainBoxMenuController.MenuType.SKILLS).transform;
      _prefab = _menu.GetChild(0);

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
            return InfoController.GetInfoString("Strength", @$"Increase your pickaxe's damage.

Level:  {level} / 99
Xp:     {getSafeFloat(xp)} / {getSafeFloat(xpMax)}
Damage: {getSafeFloat(skill._OnMaths)}");
        }),

        new Skill(SkillType.SPEED, "Speed",
          (int level) => {
            return 1f + level * 0.05f;
          },
          (Skill skill, int level, float xp, float xpMax) => {
            return InfoController.GetInfoString("Speed", @$"Increase your pickaxe's speed.

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
            return level / 99f;
          },
          (Skill skill, int level, float xp, float xpMax) => {
            return InfoController.GetInfoString("Power", @$"Percent chance to do 2x damage on hit.

Level:  {level} / 99
Xp:     {getSafeFloat(xp)} / {getSafeFloat(xpMax)}
Chance: {getSafeFloat(skill._OnMaths * 100)}%");
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

        info.SkillType = _skillType;
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
        _xp = skillInfo.XpLeft;
        _xpMax = skillInfo.XpMax;

        UpdateUi();
      }
    }

    //
    [System.Serializable]
    public class SkillSaveInfo
    {
      public SkillType SkillType;
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
        s_Singleton._skills[((int)skillInfo.SkillType) - 1].SetSkillInfo(skillInfo);
    }

  }

}