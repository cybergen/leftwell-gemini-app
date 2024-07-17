using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PlanePlacementTester : MonoBehaviour
{
  [SerializeField] private ARPlaneManager _planeManager;
  [SerializeField] private GameObject _dragonPrefab;
  [SerializeField] private Transform _cameraTransform;
  private GameObject _activeDragon;

  private void Start()
  {
    _planeManager.planesChanged += OnPlanesChanged;
  }

  private void OnDestroy()
  {
    _planeManager.planesChanged -= OnPlanesChanged;
  }

  private void OnPlanesChanged(ARPlanesChangedEventArgs args)
  {
    if (args.added == null || args.added.Count == 0) return;

    if (_activeDragon == null) { _activeDragon =  Instantiate(_dragonPrefab); }
    _activeDragon.transform.position = args.added[0].center;
    _activeDragon.transform.LookAt(_cameraTransform);
  }
}