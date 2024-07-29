using System;
using UnityEngine;
using BriLib;

public class HeroPortal : MonoBehaviour, IActivatable
{
  public bool Activatable { get; private set; }
  public string ActivationText { get
    {
      return _isOpen ? "Close Portal" : "Open Portal";
    }
  }
  [SerializeField] private Animator _animator;
  [SerializeField] private AudioFader _loopingAudio;
  [SerializeField] private GameObject _openSoundOne;
  [SerializeField] private GameObject _openSoundTwo;
  [SerializeField] private float _pauseBeforeSecondOpenSound;
  private const string LOADING_TRIGGER = "IsLoading";
  private const string OPEN_TRIGGER = "IsOpen";
  private Action _onActivate;
  private bool _isOpen = false;

  public void Activate()
  {
    Activatable = false;
    if (!_isOpen)
    {
      _openSoundOne.SetActive(false);
      _openSoundOne.SetActive(true);
      _ = AsyncMethods.DoAfterTime(_pauseBeforeSecondOpenSound, () => 
      {
        _openSoundTwo.SetActive(false);
        _openSoundTwo.SetActive(true);
      });
      _animator.SetBool(OPEN_TRIGGER, true);
    }
    else
    {
      _loopingAudio.FadeOut();
      _animator.SetBool(OPEN_TRIGGER, false);
      _animator.SetBool(LOADING_TRIGGER, false);
    }
    _isOpen = !_isOpen;
    _onActivate?.Invoke();
  }

  public void SetClosable(Action onActivate)
  {
    Activatable = true;
    _onActivate = onActivate;
  }

  public void SetActivatable(Action onActivate)
  {
    _loopingAudio.FadeIn();
    _onActivate = onActivate;
    Activatable = true;
    _animator.SetBool(LOADING_TRIGGER, true);
  }
}