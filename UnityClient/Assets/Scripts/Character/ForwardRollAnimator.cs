using UnityEngine;
using BriLib;

public class ForwardRollAnimator : MonoBehaviour
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
    _startLocalRotation = _self.localRotation;
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
      float easedProgress = Easing.EaseInOutBack(progress);
      float angleX = easedProgress * 360; // Forward roll
      float angleZ = easedProgress * 360; // Roll

      _self.localRotation = _startLocalRotation * Quaternion.Euler(angleX, 0, angleZ);

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
