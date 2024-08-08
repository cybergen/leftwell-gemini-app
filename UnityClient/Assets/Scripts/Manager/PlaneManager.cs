using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using BriLib;

public class PlaneManager : Singleton<PlaneManager>
{
  public float GroundHeight { get; private set; } = float.MinValue;
  public bool Ready { get; private set; } = false;
  [SerializeField] private ARPlaneManager _planeManager;
  [SerializeField] private Transform _cameraTransform;
  [SerializeField] private GameObject _shadowReceiverPlanePrefab;
  [SerializeField] private float boundarySize = 3f;
  private List<ARPlane> _knownPlanes = new List<ARPlane>();
  private Transform _shadowReceiverPlane;

  public override void Begin()
  {
    base.Begin();
    _shadowReceiverPlane = Instantiate(_shadowReceiverPlanePrefab).transform;
    _planeManager.planesChanged += OnPlanesChanged;
  }

  public override void End()
  {
    base.End();
    _planeManager.planesChanged -= OnPlanesChanged;
    Destroy(_shadowReceiverPlane.gameObject);
  }

  private void Update()
  {
    var camPos = _cameraTransform.position;
    var halfExtents = new Vector3(boundarySize, boundarySize, boundarySize) / 2f;

    //Find all colliders within the boundary box around the camera
    var collidersInBoundary = Physics.OverlapBox(camPos, halfExtents, Quaternion.identity, LayerMask.GetMask("ARPlane"));

    //Filter colliders to find those belonging to upward-facing planes and below height of camera
    var upwardPlanes = collidersInBoundary
      .Select(collider => collider.GetComponent<ARPlane>())
      .Where(plane => plane != null && Vector3.Dot(Vector3.up, plane.transform.up) > 0.8f && plane.transform.position.y < camPos.y);

    if (!upwardPlanes.Any()) return;

    //Get the highest known plane as ground and move shadow receiver there
    GroundHeight = upwardPlanes.Max(p => p.transform.position.y);
    _shadowReceiverPlane.position = new Vector3(camPos.x, GroundHeight, camPos.z);
  }

  private void OnPlanesChanged(ARPlanesChangedEventArgs args)
  {
    foreach (var plane in args.removed)
    {
      if (_knownPlanes.Contains(plane))
      {
        _knownPlanes.Remove(plane);
      }
    }
    _knownPlanes.AddRange(args.added);

    Ready = _knownPlanes.Count > 0;
  }
}