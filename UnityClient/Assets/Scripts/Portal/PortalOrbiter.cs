using UnityEngine;

public class PortalOrbiter : MonoBehaviour
{
  [SerializeField] private GameObject _targetPlane;
  [SerializeField] private float _speed;
  [SerializeField] private float _cornerProximity;
  [SerializeField] private int startCorner;

  private Vector3[] corners;
  private int currentCornerIndex;
  private Vector3 nextCorner;

  private void Start()
  {
    corners = new Vector3[4];
    UpdateCorners();
    currentCornerIndex = startCorner;
    nextCorner = corners[currentCornerIndex];
  }

  private void Update()
  {
    // Update the corners of the plane if it has changed
    UpdateCorners();

    // Move the object towards the next corner
    transform.position = Vector3.MoveTowards(transform.position, nextCorner, _speed * Time.deltaTime);

    // If the object has reached the next corner, move to the next corner
    if (Vector3.Distance(transform.position, nextCorner) < _cornerProximity)
    {
      currentCornerIndex = (currentCornerIndex + 1) % corners.Length;
      nextCorner = corners[currentCornerIndex];
    }
  }

  private void UpdateCorners()
  {
    if (_targetPlane == null) return;

    // Get the plane's transform and scale
    Transform planeTransform = _targetPlane.transform;
    Vector3 planeScale = planeTransform.localScale;

    // Default plane dimensions in Unity are 10x10 units
    float halfWidth = 5f * planeScale.x * (1f / planeScale.x);
    float halfHeight = 5f * planeScale.z * (1f / planeScale.z);

    // Calculate the corners based on the plane's local position and scale, then transform to world space
    corners[0] = planeTransform.TransformPoint(new Vector3(-halfWidth, 0, -halfHeight));
    corners[1] = planeTransform.TransformPoint(new Vector3(halfWidth, 0, -halfHeight));
    corners[2] = planeTransform.TransformPoint(new Vector3(halfWidth, 0, halfHeight));
    corners[3] = planeTransform.TransformPoint(new Vector3(-halfWidth, 0, halfHeight));
  }
}
