using UnityEngine;

//
public static class Easings
{


  public static float EaseInOutElastic(float x)
  {
    var c5 = 2f * Mathf.PI / 4.5f;
    return x == 0f
      ? 0f
      : x == 1f
      ? 1f
      : x < 0.5f
      ? -(Mathf.Pow(2f, 20f * x - 10f) * Mathf.Sin((20f * x - 11.125f) * c5)) / 2f
      : (Mathf.Pow(2f, -20f * x + 10f) * Mathf.Sin((20f * x - 11.125f) * c5)) / 2f + 1f;
  }

}