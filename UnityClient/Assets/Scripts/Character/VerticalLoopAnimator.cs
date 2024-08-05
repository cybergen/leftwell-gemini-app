using UnityEngine;

public class VerticalLoopAnimator : MonoBehaviour
{
  public bool Animating { get; private set; } = false;
  public bool Cancelling { get; private set; } = false;
  private float _radius;
  private float _duration;
  private float _elapsedTime;
  private Vector3 _startLocalPosition;
  private Quaternion _startLocalRotation;
  private Transform _self;

  public void Play(float radius, float duration)
  {
    this._radius = radius;
    this._duration = duration;
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
      float progress = Mathf.Clamp01(_elapsedTime / _duration);
      float angle = progress * 2 * Mathf.PI;
      float y = Mathf.Sin(angle) * _radius;
      float z = Mathf.Cos(angle) * _radius;

      _self.localPosition = new Vector3(_startLocalPosition.x, _startLocalPosition.y + y, _startLocalPosition.z + z);
      _self.localRotation = Quaternion.LookRotation(Vector3.Cross(Vector3.right, new Vector3(0, y, z)).normalized);

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
      var posAmount = Time.deltaTime * _radius / _duration;
      var rotAmount = Time.deltaTime * 360 / _duration;

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
