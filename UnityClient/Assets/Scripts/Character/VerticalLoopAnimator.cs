using UnityEngine;
using BriLib;

public class VerticalLoopAnimator : MonoBehaviour, IProceduralAnimator
{
  public bool Animating { get; private set; } = false;
  public bool Cancelling { get; private set; } = false;
  [SerializeField] private float _radius;
  [SerializeField] private float _duration = 3f;
  [SerializeField] private float _cancelMultiplier = 3f;
  private float _elapsedTime;
  private Vector3 _startLocalPosition;
  private Quaternion _startLocalRotation;
  private Transform _self;

  public void Play()
  {
    _elapsedTime = 0f;
    _startLocalPosition = _self.localPosition;
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
      var progress = Easing.Ease(0, 1, 0, 1, Mathf.Clamp01(_elapsedTime / _duration), Easing.Method.ExpoInOut);
      float angle = progress * 360f;
      float angleY = (angle - 180f) * Mathf.Deg2Rad; //Shift the curve by pi radians
      float angleZ = angle * Mathf.Deg2Rad;
      float y = Mathf.Cos(angleY) * _radius + _radius; //Should go from position 0 to 1 to 0
      float z = Mathf.Sin(angleZ) * _radius; //Should go from 0 to +0.5 to 0 to -0.5 to 0

      var newPosition = new Vector3(_startLocalPosition.x, _startLocalPosition.y + y, _startLocalPosition.z + z);
      _self.localPosition = newPosition;
      var rotationDelta = Quaternion.Euler(-angle, 0, 0); //Rotate around the x-axis by current traversed degrees
      _self.localRotation = _startLocalRotation * rotationDelta;

      if (progress >= 1f)
      {
        Animating = false;
        Cancelling = true;
      }
    }
    else if (Cancelling)
    {
      float positionThreshold = 0.01f;
      float rotationThreshold = 1f;
      var posAmount = Time.deltaTime * _cancelMultiplier * _radius / _duration;
      var rotAmount = Time.deltaTime * _cancelMultiplier * 360 / _duration;

      _self.localPosition = Vector3.MoveTowards(_self.localPosition, _startLocalPosition, posAmount);
      _self.localRotation = Quaternion.RotateTowards(_self.localRotation, _startLocalRotation, rotAmount);

      if (Vector3.Distance(_self.localPosition, _startLocalPosition) < positionThreshold && Quaternion.Angle(_self.localRotation, _startLocalRotation) < rotationThreshold)
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
