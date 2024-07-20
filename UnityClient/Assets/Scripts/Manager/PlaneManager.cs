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
  [SerializeField] private Transform _shadowReceiverPlane;
  private List<ARPlane> _knownPlanes = new List<ARPlane>();

  public override void Begin()
  {
    base.Begin();
    _planeManager.planesChanged += OnPlanesChanged;
  }

  public override void End()
  {
    base.End();
    _planeManager.planesChanged -= OnPlanesChanged;
  }

  private void Update()
  {
    //Go through set of planes and find planes with mostly vertical orientation
    var camPos = _cameraTransform.position;
    var upwardPlanes = _knownPlanes.Where((plane) =>
    {
      var t = plane.transform;
      return Vector3.Dot(Vector3.up, t.up) > 0.8f && t.position.y < camPos.y;
    });

    if (upwardPlanes.Count() == 0) return;

    //Get highest known plane as ground and move shadow receiver there
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