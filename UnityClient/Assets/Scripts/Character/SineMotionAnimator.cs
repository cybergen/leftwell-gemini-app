using UnityEngine;

public class SineMotionAnimator : MonoBehaviour
{
  [SerializeField] private float _amplitude;
  [Tooltip("Waves per second")][SerializeField] private float _frequency;
  private Transform _self;
  private Vector3 _localPosition; //This position should be expected to otherwise be static
  private bool _active;
  private float _startTime;

  public void Stop()
  {
    _active = false;
  }

  public void Resume()
  {
    _active = true;
    _startTime = Time.time;
  }

  private void Update()
  {
    if (!_active) { return; }
    var time = (Time.time - _startTime) % _frequency;
    var rad = (time / _frequency) * 360f * Mathf.Deg2Rad;
    var displacement = Mathf.Sin(rad) * _amplitude;
    var newLocal = new Vector3(_localPosition.x, displacement, _localPosition.z);
    _self.localPosition = newLocal;
  }

  private void Awake()
  {
    _self = transform;
    _localPosition = transform.localPosition;
    Resume();
  }
}