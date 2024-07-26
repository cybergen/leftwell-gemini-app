using System;
using UnityEngine;

public class HeroPortal : MonoBehaviour, IActivatable
{
  public bool Activatable { get; private set; }
  public string ActivationText { get
    {
      return _isOpen ? "Close Portal" : "Open Portal";
    }
  }
  [SerializeField] private Animator _animator;
  private const string LOADING_TRIGGER = "IsLoading";
  private const string OPEN_TRIGGER = "IsOpen";
  private Action _onActivate;
  private bool _isOpen = false;

  public void Activate()
  {
    Activatable = false;
    if (!_isOpen)
    {
      _animator.SetBool(OPEN_TRIGGER, true);
    }
    else
    {
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
    _onActivate = onActivate;
    Activatable = true;
    _animator.SetBool(LOADING_TRIGGER, true);
  }
}