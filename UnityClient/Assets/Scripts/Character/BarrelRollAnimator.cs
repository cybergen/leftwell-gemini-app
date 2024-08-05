using UnityEngine;
using BriLib;

public class BarrelRollAnimator : MonoBehaviour
{
  public bool Animating { get; private set; } = false;
  public bool Cancelling { get; private set; } = false;
  private float _duration;
  private float _elapsedTime;
  private Quaternion _startLocalRotation;
  private Transform _self;

  public void Play(float duration)
  {
    _duration = duration;
    _elapsedTime = 0f;
    _startLocalRotation = transform.localRotation;
    Animating = true;
    Cancelling = false;
  }

  public void Stop()
  {
    Animating = false;
    Cancelling = true;
  }

  private void Update()
  {
    if (Animating)
    {
      _elapsedTime += Time.deltaTime;
      float progress = Mathf.Clamp01(_elapsedTime / _duration);
      float easedProgress = Easing.ElasticEaseOut(progress);
      float angle = easedProgress * 360;

      _self.localRotation = _startLocalRotation * Quaternion.Euler(0, 0, angle);

      if (progress >= 1f)
      {
        Animating = false;
        Cancelling = true;
      }
    }
    else if (Cancelling)
    {
      float rotationThreshold = 1f;
      var amount = Time.deltaTime * 360 / _duration;

      _self.localRotation = Quaternion.RotateTowards(_self.localRotation, _startLocalRotation, amount);

      if (Quaternion.Angle(_self.localRotation, _startLocalRotation) < rotationThreshold)
      {
        Cancelling = false;
      }
    }
  }

  private void Awake()
  {
    _self = transform;
  }
}
