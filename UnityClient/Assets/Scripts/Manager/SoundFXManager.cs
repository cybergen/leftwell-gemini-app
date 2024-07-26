using UnityEngine;
using BriLib;

public class SoundFXManager : Singleton<SoundFXManager>
{
  [SerializeField] private AudioSource _uiAudioSource;
  [SerializeField] private AudioClip _intro;
  [SerializeField] private AudioClip _chime;
  [SerializeField] private AudioClip _select;
  [SerializeField] private AudioClip _cameraShutter;

  public void PlaySound(Sound sound)
  {
    AudioClip clip = null;
    switch (sound)
    {
      case Sound.IntroFlourish: clip = _intro; break;
      case Sound.Chime: clip = _chime; break;
      case Sound.Select: clip = _select; break;
      case Sound.Camera: clip = _cameraShutter; break;
    }
    _uiAudioSource.PlayOneShot(clip);
  }

  public static void TriggerCameraShutter()
  {
    Instance.PlaySound(Sound.Camera);
  }
}

public enum Sound
{
  IntroFlourish,
  Chime,
  Select,
  Camera,
}