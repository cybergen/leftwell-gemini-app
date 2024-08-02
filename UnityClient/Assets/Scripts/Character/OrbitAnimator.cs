using System.Threading.Tasks;
using UnityEngine;

public class OrbitAnimator : MonoBehaviour
{
  public bool Animating { get; private set; } = false;
  [SerializeField] private int _milliStep = 16;
  private Transform _self;

  public async Task Play(Vector3 targetPoint, float speed, float rotationSpeed, int delayMillis = 0)
  {
    Animating = true;
    await Task.Delay(delayMillis);

    Vector3 startPosition = _self.position;
    float radius = Vector3.Distance(startPosition, targetPoint);
    float circumference = 2 * Mathf.PI * radius;
    float duration = circumference / speed * 1000f; // Convert seconds to milliseconds

    //Calculate the initial rotation to face the tangent of the starting orbit position
    Vector3 initialOffset = (startPosition - targetPoint).normalized * radius;
    Quaternion targetRotation = Quaternion.LookRotation(Vector3.Cross(initialOffset, Vector3.up).normalized);

    //Slerp to the starting rotation
    float rotationElapsedMillis = 0f;
    while (rotationElapsedMillis < duration / rotationSpeed)
    {
      rotationElapsedMillis += _milliStep;
      _self.rotation = Quaternion.Slerp(_self.rotation, targetRotation, Mathf.Clamp01(rotationElapsedMillis / (duration / rotationSpeed)));
      await Task.Delay(_milliStep);
    }

    //Orbit around the target point
    float startAngle = Mathf.Atan2(startPosition.z - targetPoint.z, startPosition.x - targetPoint.x);
    float elapsedMillis = 0f;
    while (elapsedMillis < duration)
    {
      elapsedMillis += _milliStep;
      float angle = startAngle + Mathf.Clamp01(elapsedMillis / duration) * 2 * Mathf.PI;
      Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, startPosition.y - targetPoint.y, Mathf.Sin(angle) * radius);
      _self.position = targetPoint + offset;
      _self.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.up, -offset).normalized);

      await Task.Delay(_milliStep);
    }

    _self.position = startPosition;
    Animating = false;
  }

  private void Awake()
  {
    _self = transform;
  }
}
