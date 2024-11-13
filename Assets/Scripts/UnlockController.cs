using UnityEngine;
using System.Collections.Generic;
using System;

namespace Controllers
{

  public class UnlockController
  {

    //
    public static UnlockController s_Singleton;

    RectTransform _notificationUi, _unlockTarget;
    TMPro.TextMeshProUGUI _notifyText;
    float _notificationTimer, _notificationWaitTimer;
    bool _notifying, _notifiyToggle, _particlesNotifying;
    ParticleSystem _unlockParticles, _unlockExplosionParticles;
    Vector3 _unlockParticlesPositionTo;
    Action _unlockAction;

    //
    List<UnlockType> _unlocks;
    public enum UnlockType
    {
      NONE,

      SHOP,
      SKILLS,

      FORGE,
    }

    //
    public UnlockController()
    {
      s_Singleton = this;

      //
      _notificationUi = GameObject.Find("UnlockNotification").transform as RectTransform;
      _notifyText = _notificationUi.Find("NotifyText").GetComponent<TMPro.TextMeshProUGUI>();

      _unlockParticles = GameObject.Find("UnlockCursor").GetComponent<ParticleSystem>();
      _unlockExplosionParticles = GameObject.Find("UnlockExplosion").GetComponent<ParticleSystem>();

      //
      _unlocks = new();
    }

    //
    public void Update()
    {

      if (_notifying)
      {
        if (_notifiyToggle)
          _notificationTimer += (1f - _notificationTimer) * Time.deltaTime * 5f;
        else
          _notificationTimer += (0f - _notificationTimer) * Time.deltaTime * 5f;

        //
        if (_notifiyToggle && _notificationTimer > 0.95f)
        {
          if (_notificationWaitTimer == 5f)
          {
            ParticlesNotifyTo(_unlockTarget.position);
          }

          //
          _notificationWaitTimer -= Time.deltaTime;
          if (_notificationWaitTimer <= 0f && !_particlesNotifying)
          {
            _notifiyToggle = false;
            _notificationWaitTimer = 1f;
          }
        }
        else if (!_notifiyToggle && _notificationTimer < 0.05f)
        {
          _notifying = false;
        }
      }

      //
      _notificationUi.anchoredPosition = new Vector3(2082.4f, Mathf.Lerp(116f, 17.3f, _notificationTimer), 0f);

      //
      if (_particlesNotifying)
      {

        var positionDistance = _unlockParticlesPositionTo - _unlockParticles.transform.position;
        _unlockParticles.transform.position += positionDistance * Time.deltaTime * 5f;
        if (positionDistance.magnitude < 0.25f)
        {
          _particlesNotifying = false;
          _unlockParticles.Stop();
          _unlockExplosionParticles.Play();
        }

      }
    }

    //
    void ParticlesNotifyTo(Vector3 position)
    {
      position.z = 0f;

      _particlesNotifying = true;
      _unlockParticlesPositionTo = position;
      _unlockExplosionParticles.transform.position = position;

      _unlockParticles.transform.position = new Vector3(6.5f, 5f, 0f);
      _unlockParticles.Play();
    }
    public static void NotifyUnlockButton(GameObject button, string notificationText)
    {

      //
      s_Singleton._unlockTarget = button.transform as RectTransform;
      s_Singleton._unlockAction = () =>
      {
        button.SetActive(true);
      };
      s_Singleton._unlockAction?.Invoke();

      s_Singleton._notifyText.text = notificationText;

      s_Singleton._notifying = true;
      s_Singleton._notifiyToggle = true;
      s_Singleton._notificationWaitTimer = 5f;
    }

    //
    public static void Unlock(UnlockType unlock)
    {
      //
      if (s_Singleton._unlocks.Contains(unlock)) return;

      //
      s_Singleton._unlocks.Add(unlock);

      //
      switch (unlock)
      {

        //
        case UnlockType.SHOP:
          MainBoxMenuController.s_Singleton.SetMenuActive(MainBoxMenuController.MenuType.SHOP, true, true, "Shop unlocked!");
          break;
        case UnlockType.SKILLS:
          MainBoxMenuController.s_Singleton.SetMenuActive(MainBoxMenuController.MenuType.SKILLS, true, true, "Skills unlocked!");
          break;

        case UnlockType.FORGE:
          MineBoxMenuController.s_Singleton.SetMenuActive(MineBoxMenuController.MenuType.FORGE, true, true, "Forge unlocked!");
          break;

      }
    }
    public static bool HasUnlock(UnlockType unlock)
    {
      return s_Singleton._unlocks.Contains(unlock);
    }

    //
    public static List<string> GetUnlocks()
    {

      var returnList = new List<string>();
      foreach (var unlock in s_Singleton._unlocks)
        returnList.Add(unlock.ToString());

      return returnList;
    }
    public static void SetUnlocks(List<string> unlocks)
    {
      foreach (var unlockString in unlocks)
      {
        if (Enum.TryParse(unlockString, true, out UnlockType unlockType))
          Unlock(unlockType);
      }
    }
  }

}