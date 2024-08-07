using UnityEngine;

public class CameraTracker : MonoBehaviour
{
  [SerializeField] private float _maxAngleThreshold = 15f;
  [SerializeField] private float _rotationRate = 1f;

  private Quaternion _startRotation;
  private bool _isAnimating = false;
  private Transform _self;

  private void Awake()
  {
    _startRotation = transform.localRotation;
    _self = transform;
  }

  private void Update()
  {
    if (_isAnimating)
    {
      var targetPos = CameraManager.Instance.GetCameraPose().Item1;
      var lookRotation = Quaternion.LookRotation(targetPos - _self.position);
      //Convert limitedRotation to local space
      lookRotation = Quaternion.Inverse(_self.parent.rotation) * lookRotation;
      var limitedRotation = LimitRotation(_startRotation, lookRotation);
      

      //Slerp from the last rotation towards the limited target rotation
      var oldRot = _self.localRotation;
      var newRot = Quaternion.Slerp(oldRot, limitedRotation, Time.deltaTime * _rotationRate);
      _self.localRotation = newRot;
    }
    else if (Quaternion.Angle(_self.localRotation, _startRotation) > Mathf.Epsilon)
    {
      _self.localRotation = Quaternion.RotateTowards(_self.localRotation, _startRotation, Time.deltaTime * _rotationRate);
    }
  }

  public void StartSlerping()
  {
    Debug.Log("Camera tracker active");
    _isAnimating = true;
  }

  public void StopSlerping()
  {
    Debug.Log("Camera tracker disabled");
    _isAnimating = false;
  }

  private Quaternion LimitRotation(Quaternion startRotation, Quaternion targetRotation)
  {
    //Calculate the angle between the initial rotation and the target rotation
    var angle = Quaternion.Angle(startRotation, targetRotation);

    //If the angle exceeds the threshold, clamp it to the threshold
    if (angle > _maxAngleThreshold)
    {
      var t = _maxAngleThreshold / angle;
      return Quaternion.Slerp(startRotation, targetRotation, t);
    }
    else
    {
      return targetRotation;
    }
  }
}
