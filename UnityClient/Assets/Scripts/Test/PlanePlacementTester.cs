using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PlanePlacementTester : MonoBehaviour
{
  [SerializeField] private ARPlaneManager _planeManager;
  [SerializeField] private GameObject _dragonPrefab;
  [SerializeField] private Transform _cameraTransform;
  [SerializeField] private float _distanceFromCamera;
  [SerializeField] private float _heightAboveGround;
  [SerializeField] private float _angleFromPlayerForwardToSeek;
  private GameObject _activeDragon;

  private void Start()
  {
    _planeManager.planesChanged += OnPlanesChanged;
  }

  private void OnDestroy()
  {
    _planeManager.planesChanged -= OnPlanesChanged;
  }

  private void Update()
  {
    if (_activeDragon == null) return;

    var forwardVector = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up);
    forwardVector.Normalize();
    forwardVector = Quaternion.AngleAxis(_angleFromPlayerForwardToSeek, Vector3.up) * forwardVector;
    var newPosition = _cameraTransform.position + forwardVector * _distanceFromCamera;
    newPosition.y = PlaneManager.Instance.GroundHeight + _heightAboveGround;

    _activeDragon.transform.position = newPosition;
    _activeDragon.transform.LookAt(_cameraTransform);
  }

  private void OnPlanesChanged(ARPlanesChangedEventArgs args)
  {
    if (args.added == null || args.added.Count == 0) return;

    if (_activeDragon == null) { _activeDragon =  Instantiate(_dragonPrefab); }
  }
}