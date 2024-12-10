using UnityEngine;
using System.Collections.Generic;

namespace Controllers
{


  //
  public class ParticleController
  {

    //
    public static ParticleController s_Singleton;

    //
    public enum ParticleType
    {
      NONE,

      ROCK_HIT,
      ROCK_DESTROY,

      ORE_0,
      ORE_1,
      ORE_2,
      ORE_3,
      ORE_4,
      ORE_5,
      ORE_6,
      ORE_7,

      GEM_0,
      GEM_1,
      GEM_2,
      GEM_3,
      GEM_4,

      FORGE_SMOKE,
      FORGE_EMBERS,

      HAMMER_SPARKS,

    }
    Dictionary<ParticleType, ParticleSystem> _particles;

    //
    public ParticleController()
    {
      s_Singleton = this;

      //
      var mineParticles = MineBoxMenuController.s_Singleton.GetMenu(MineBoxMenuController.MenuType.MINE).transform.Find("Particles");
      var forgeParticles = MineBoxMenuController.s_Singleton.GetMenu(MineBoxMenuController.MenuType.FORGE).transform.Find("Particles");
      var hammerParticles = MineBoxMenuController.s_Singleton.GetMenu(MineBoxMenuController.MenuType.HAMMER).transform.Find("Particles");
      _particles = new()
      {
        { ParticleType.ROCK_HIT, mineParticles.Find("RockHit").GetComponent<ParticleSystem>() },
        { ParticleType.ROCK_DESTROY, mineParticles.Find("RockDestroy").GetComponent<ParticleSystem>() },

        { ParticleType.ORE_0, mineParticles.Find("Ores/Ore0").GetComponent<ParticleSystem>() },
        { ParticleType.ORE_1, mineParticles.Find("Ores/Ore1").GetComponent<ParticleSystem>() },
        { ParticleType.ORE_2, mineParticles.Find("Ores/Ore2").GetComponent<ParticleSystem>() },
        { ParticleType.ORE_3, mineParticles.Find("Ores/Ore3").GetComponent<ParticleSystem>() },
        { ParticleType.ORE_4, mineParticles.Find("Ores/Ore4").GetComponent<ParticleSystem>() },
        { ParticleType.ORE_5, mineParticles.Find("Ores/Ore5").GetComponent<ParticleSystem>() },
        { ParticleType.ORE_6, mineParticles.Find("Ores/Ore6").GetComponent<ParticleSystem>() },
        { ParticleType.ORE_7, mineParticles.Find("Ores/Ore7").GetComponent<ParticleSystem>() },

        { ParticleType.GEM_0, mineParticles.Find("Ores/Gem0").GetComponent<ParticleSystem>() },
        { ParticleType.GEM_1, mineParticles.Find("Ores/Gem1").GetComponent<ParticleSystem>() },
        { ParticleType.GEM_2, mineParticles.Find("Ores/Gem2").GetComponent<ParticleSystem>() },
        { ParticleType.GEM_3, mineParticles.Find("Ores/Gem3").GetComponent<ParticleSystem>() },
        { ParticleType.GEM_4, mineParticles.Find("Ores/Gem4").GetComponent<ParticleSystem>() },

        { ParticleType.FORGE_SMOKE, forgeParticles.Find("ForgeSmoke").GetComponent<ParticleSystem>() },
        { ParticleType.FORGE_EMBERS, forgeParticles.Find("ForgeEmbers").GetComponent<ParticleSystem>() },

        { ParticleType.HAMMER_SPARKS, hammerParticles.Find("HammerSparks").GetComponent<ParticleSystem>() },
      };
    }

    //
    public static ParticleSystem GetParticles(ParticleType particleType)
    {
      return s_Singleton._particles[particleType];
    }

    //
    public static void PlayParticles(ParticleType particleType)
    {
      GetParticles(particleType).Play();
    }
    public static void PlayParticles(ParticleType particleType, int count)
    {
      GetParticles(particleType).Emit(count);
    }

    public static void StopParticles(ParticleType particleType)
    {
      GetParticles(particleType).Stop();
    }
    public static bool IsParticlesPlaying(ParticleType particleType)
    {
      return GetParticles(particleType).isPlaying;
    }
  }

}