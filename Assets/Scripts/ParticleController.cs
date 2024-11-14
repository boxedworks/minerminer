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

      GEM_0,
      GEM_1,
      GEM_2,
      GEM_3,

      FORGE_SMOKE,
      FORGE_EMBERS,

    }
    Dictionary<ParticleType, ParticleSystem> _particles;

    //
    public ParticleController()
    {
      s_Singleton = this;

      //
      _particles = new()
      {
        { ParticleType.ROCK_HIT, GameObject.Find("RockHit").GetComponent<ParticleSystem>() },
        { ParticleType.ROCK_DESTROY, GameObject.Find("RockDestroy").GetComponent<ParticleSystem>() },

        { ParticleType.ORE_0, GameObject.Find("Ore0").GetComponent<ParticleSystem>() },
        { ParticleType.ORE_1, GameObject.Find("Ore1").GetComponent<ParticleSystem>() },
        { ParticleType.ORE_2, GameObject.Find("Ore2").GetComponent<ParticleSystem>() },

        { ParticleType.GEM_0, GameObject.Find("Gem0").GetComponent<ParticleSystem>() },
        { ParticleType.GEM_1, GameObject.Find("Gem1").GetComponent<ParticleSystem>() },
        { ParticleType.GEM_2, GameObject.Find("Gem2").GetComponent<ParticleSystem>() },
        { ParticleType.GEM_3, GameObject.Find("Gem3").GetComponent<ParticleSystem>() },

        { ParticleType.FORGE_SMOKE, GameObject.Find("ForgeSmoke").GetComponent<ParticleSystem>() },
        { ParticleType.FORGE_EMBERS, GameObject.Find("ForgeEmbers").GetComponent<ParticleSystem>() },
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