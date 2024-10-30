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
      };
    }

    //
    public static void PlayParticles(ParticleType particleType)
    {
      s_Singleton._particles[particleType].Play();
    }
    public static void PlayParticles(ParticleType particleType, int count)
    {
      s_Singleton._particles[particleType].Emit(count);
    }

  }

}