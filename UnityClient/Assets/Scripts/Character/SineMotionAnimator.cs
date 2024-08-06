using UnityEngine;

public class SineMotionAnimator : MonoBehaviour, IProceduralAnimator
{
  public bool Animating { get; private set; } = false;
  public bool Cancelling { get; private set; } = false;
  [SerializeField] private float _amplitude = 0.15f;
  [SerializeField] private float _duration = 2f;
  private float _elapsedTime;
  private Vector3 _startLocalPosition;
  private Transform _self;

  public void Play()
  {
    _elapsedTime = 0f;
    _startLocalPosition = _self.localPosition;
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
      var rad = progress * 360f * Mathf.Deg2Rad;
      var displacement = Mathf.Sin(rad) * _amplitude;
      var newLocal = new Vector3(_startLocalPosition.x, displacement, _startLocalPosition.z);
      _self.localPosition = newLocal;

      if (progress >= 1f)
      {
        Animating = false;
        Cancelling = true;
      }
    }
    else if (Cancelling)
    {
      float positionThreshold = 0.01f;
      var posAmount = Time.deltaTime * 4f * _amplitude;

      _self.localPosition = Vector3.MoveTowards(_self.localPosition, _startLocalPosition, posAmount);

      if (Vector3.Distance(_self.localPosition, _startLocalPosition) < positionThreshold)
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
