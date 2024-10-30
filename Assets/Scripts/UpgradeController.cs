using UnityEngine;
using System.Collections.Generic;

namespace Controllers
{

  public class UpgradeController
  {

    //
    public static UpgradeController s_Singleton;

    //
    List<UpgradeType> _unlockedUpgrades;
    public enum UpgradeType
    {
      NONE,

      SHOP,
      SKILLS,

      FORGE,
    }

    //
    public UpgradeController()
    {
      s_Singleton = this;

      //
      _unlockedUpgrades = new();
    }

    //
    public static void UnlockUpgrade(UpgradeType unlock)
    {
      s_Singleton._unlockedUpgrades.Add(unlock);

      //
      switch (unlock)
      {

        //
        case UpgradeType.SHOP:
          MainBoxMenuController.s_Singleton.SetMenuActive(MainBoxMenuController.MenuType.SHOP, true);
          break;
        case UpgradeType.SKILLS:
          MainBoxMenuController.s_Singleton.SetMenuActive(MainBoxMenuController.MenuType.SKILLS, true);
          break;

        case UpgradeType.FORGE:
          MineBoxMenuController.s_Singleton.SetMenuActive(MineBoxMenuController.MenuType.FORGE, true);
          break;

      }
    }
    public static bool HasUpgrade(UpgradeType upgrade)
    {
      return s_Singleton._unlockedUpgrades.Contains(upgrade);
    }
  }

}