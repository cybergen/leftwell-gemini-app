using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BriLib;

public class ProceduralAnimator : MonoBehaviour, IProceduralAnimator
{
  public bool Animating { get; private set; } = false;
  public bool Cancelling { get; private set; } = false;
  [SerializeField] private int _sineMotionRepeatsBeforeRandom = 3;
  [SerializeField] private CameraTracker _cameraTracker;
  private List<IProceduralAnimator> _proceduralAnimators;
  private SineMotionAnimator _sineMotion;
  private IProceduralAnimator _currentAnimator;
  private int _currentSineMotionCount = 0;

  public void Play()
  {
    if (Animating) return;

    Animating = true;
    Cancelling = false;
    _currentSineMotionCount = 1;
    _currentAnimator = _sineMotion;
    _currentAnimator.Play();
  }

  public void Stop()
  {
    Animating = false;
    Cancelling = true;
    if (_currentAnimator != null) { _currentAnimator.Stop(); }
    _cameraTracker.StopSlerping();
  }

  private void Update()
  {
    if (Cancelling && (_currentAnimator == null || !_currentAnimator.Cancelling))
    {
      Cancelling = false;
      return;
    }

    if (_currentAnimator == null || (Animating && _currentAnimator.Animating)) { return; }

    if (Animating && !_currentAnimator.Animating)
    {
      if (_currentSineMotionCount < _sineMotionRepeatsBeforeRandom)
      {
        _cameraTracker.StartSlerping();
        _currentSineMotionCount++;
        _currentAnimator = _sineMotion;
      }
      else
      {
        _cameraTracker.StopSlerping();
        _currentSineMotionCount = 0;
        _currentAnimator = MathHelpers.SelectFromRange(_proceduralAnimators, new System.Random());
      }
      _currentAnimator.Play();
    }
  }

  private void Awake()
  {
    _proceduralAnimators = gameObject.GetComponents<IProceduralAnimator>().ToList();
    _sineMotion = gameObject.GetComponent<SineMotionAnimator>();
    _proceduralAnimators.Remove(this);
  }
}