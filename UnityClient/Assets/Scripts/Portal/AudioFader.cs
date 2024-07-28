using UnityEngine;
using BriLib;

public class AudioFader : MonoBehaviour
{
  [SerializeField] private AudioSource _source;
  [SerializeField] private float _fadeDuration;
  private bool _fading;
  private float _finalVolume;
  private float _startVolume;
  private float _endVolume;
  private float _elapsedTime;

  public void FadeIn()
  {
    _fading = true;
    _elapsedTime = 0f;
    _startVolume = _source.volume;
    _endVolume = _finalVolume;
  }

  public void FadeOut()
  {
    _fading = true;
    _elapsedTime = 0f;
    _startVolume = _source.volume;
    _endVolume = 0f;
  }

  private void Update()
  {
    if (!_fading) { return; }
    
    var progress = Mathf.Clamp01(Easing.ExpoEaseOut(_elapsedTime / _fadeDuration));
    var current = (_endVolume - _startVolume) * progress + _startVolume;
    _source.volume = current;
    _elapsedTime += Time.deltaTime;

    if (progress >= 1f) { _fading = false; }
  }

  private void Awake()
  {
    _fading = false;
    _finalVolume = _source.volume;
    _source.volume = 0;
  }
}