using System;
using System.Collections.Generic;
using UnityEngine;
using BriLib;

public class PortalManager : Singleton<PortalManager>
{
  public float MarkerSpawnDistance;
  [SerializeField] private GameObject _smallMarkerPrefab;
  [SerializeField] private GameObject _heroPortalPrefab;
  [SerializeField] private float _heroPortalSpawnHeight;
  [SerializeField] private float _heroPortalSpawnDistance;

  private List<ItemCaptureMarker> _captureMarkers = new List<ItemCaptureMarker>();
  private HeroPortal _heroPortal;

  public int SpawnCaptureMarker()
  {
    var pose = CameraManager.Instance.GetCameraPose();
    var pos = pose.Item1 + (pose.Item2 * Vector3.forward) * MarkerSpawnDistance;
    var marker = Instantiate(_smallMarkerPrefab, pos, pose.Item2);
    var captureMarker = marker.GetComponent<ItemCaptureMarker>();
    var index = _captureMarkers.Count;
    _captureMarkers.Add(captureMarker);
    return index;
  }

  public void SpawnHeroPortal()
  {
    var camPose = CameraManager.Instance.GetCameraPose();
    var spawnPoint = camPose.Item1 + (camPose.Item2 * Vector3.forward) * _heroPortalSpawnDistance;
    spawnPoint.y = PlaneManager.Instance.GroundHeight + _heroPortalSpawnHeight;
    var forwardDir = Vector3.ProjectOnPlane(camPose.Item1 - spawnPoint, Vector3.up).normalized;
    var marker = Instantiate(_heroPortalPrefab, spawnPoint, Quaternion.LookRotation(forwardDir));
    _heroPortal = marker.GetComponent<HeroPortal>();
  }

  public void SetMarkerActivatable(int markerIndex, Texture2D finalImage, Action onActivated)
  {
    if (!(_captureMarkers.Count > markerIndex) || markerIndex < 0)
    {
      Debug.LogError($"Attempted to activate invalid capture marker index {markerIndex}");
      return;
    }
    _captureMarkers[markerIndex].MarkActivatable(finalImage, onActivated);
  }

  public void SetMarkerSharable(int markerIndex, string shareText, Action onShared)
  {
    Debug.LogWarning($"In SetMarkerSharable for cap index {markerIndex} and share text {shareText}");
    if (!(_captureMarkers.Count > markerIndex) || markerIndex < 0)
    {
      Debug.LogError($"Attempted to set sharability for invalid capture marker index {markerIndex}");
      return;
    }
    _captureMarkers[markerIndex].MarkSharable(shareText, onShared);
  }

  public void SetHeroPortalActivatable(Action onActivated)
  {
    if (_heroPortal == null)
    {
      Debug.LogError("Did not have big portal to mark activatable");
      return;
    }
    _heroPortal.SetActivatable(onActivated);
  }

  public void SetHeroPortalClosable(Action onClosed)
  {
    if (_heroPortal == null)
    {
      Debug.LogError("Did not have big portal to mark closable");
      return;
    }
    _heroPortal.SetClosable(onClosed);
  }

  public void ActivateMarker(int index)
  {
    _captureMarkers[index].Activate();
  }

  public void ActivatePortal()
  {
    _heroPortal.Activate();
  }

  public bool GetAllMarkersActivatable()
  {
    foreach (var marker in _captureMarkers)
    {
      if (!marker.Activatable) return false;
    }
    return true;
  }

  public void DestroyEverything()
  {
    foreach (var marker in _captureMarkers) { Destroy(marker.gameObject); }
    _captureMarkers.Clear();
    if (_heroPortal != null) { Destroy(_heroPortal.gameObject); }
    _heroPortal = null;
  }

  public Vector3 GetHeroPortalPosition()
  {
    if (_heroPortal == null)
    {
      Debug.LogError("Did not have big portal to request position of");
      return Vector3.zero;
    }
    return _heroPortal.transform.position;
  }
}