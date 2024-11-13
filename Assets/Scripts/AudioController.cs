using UnityEngine;
using System.Collections.Generic;

namespace Controllers
{

  //
  public class AudioController
  {

    //
    public static AudioController s_Singleton;

    //
    Dictionary<string, AudioObject> _audioObjects;

    Transform _audioContainer;
    List<(AudioSource AudioSource, float Volume)> _audios;

    //
    public AudioController()
    {
      s_Singleton = this;

      //
      _audioObjects = new();
      var audioObjectsResources = Resources.Load<AudioObjects>("AudioObjects");
      foreach (var audioObject in audioObjectsResources.AudioObjects_)
        _audioObjects.Add(audioObject.name, audioObject);

      _audioContainer = GameObject.Find("Audio").transform;
      _audios = new();
    }

    //
    public void Update()
    {
      for (var i = _audios.Count - 1; i >= 0; i--)
      {
        var audioInfo = _audios[i];
        if (!audioInfo.AudioSource.isPlaying)
        {
          _audios.RemoveAt(i);

          GameObject.Destroy(audioInfo.AudioSource.gameObject);
        }else{
          audioInfo.AudioSource.volume = audioInfo.Volume * (OptionsController.s_Volume / 5f);
        }
      }
    }

    //
    public static AudioSource PlayAudio(string name)
    {
      var audio = new GameObject().AddComponent<AudioSource>();
      audio.transform.position = Vector3.zero;
      audio.transform.SetParent(s_Singleton._audioContainer);

      var audioObject = s_Singleton._audioObjects[name];
      audio.clip = audioObject.AudioClip;
      audio.volume = audioObject.Volume;
      audio.pitch = Random.Range(audioObject.PitchMin, audioObject.PitchMax);

      s_Singleton._audios.Add((AudioSource: audio, Volume: audioObject.Volume));
      audio.Play();

      return audio;
    }

  }
}