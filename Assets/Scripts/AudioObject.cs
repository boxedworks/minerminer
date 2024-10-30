using UnityEngine;

[CreateAssetMenu(fileName = "AudioObjects", menuName = "Scriptable Objects/AudioObject")]
public class AudioObject : ScriptableObject
{
  public AudioClip AudioClip;
  public float PitchMin = 0.9f, PitchMax = 1.1f, Volume = 1;
}
