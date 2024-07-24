using System;
using System.Collections.Generic;
using UnityEngine;
using BriLib;

public class PortalManager : Singleton<PortalManager>
{
  [SerializeField] private GameObject _smallMarkerPrefab;
  [SerializeField] private GameObject _bigMarkerPrefab;
  [SerializeField] private float _bigPortalSpawnHeight;
  [SerializeField] private float _forwardSpawnDistance;

  private List<ItemCaptureMarker> _captureMarkers = new List<ItemCaptureMarker>();
  private ItemCaptureMarker _bigPortal;

  public int SpawnCaptureMarker()
  {
    var pose = CameraManager.Instance.GetCameraPose();
    var pos = pose.Item1 + (pose.Item2 * Vector3.forward) * _forwardSpawnDistance;
    var marker = Instantiate(_smallMarkerPrefab, pos, pose.Item2);
    var captureMarker = marker.GetComponent<ItemCaptureMarker>();
    captureMarker.Spawn();
    var index = _captureMarkers.Count;
    _captureMarkers.Add(captureMarker);
    return index;
  }

  public void SpawnBigPortal()
  {
    var camPose = CameraManager.Instance.GetCameraPose();
    var spawnPoint = camPose.Item1 + (camPose.Item2 * Vector3.forward) * _forwardSpawnDistance;
    spawnPoint.y = PlaneManager.Instance.GroundHeight + _bigPortalSpawnHeight;
    var forwardDir = Vector3.ProjectOnPlane(spawnPoint - camPose.Item1, Vector3.up).normalized;
    var marker = Instantiate(_bigMarkerPrefab, spawnPoint, Quaternion.LookRotation(forwardDir));
    var captureMarker = marker.GetComponent<ItemCaptureMarker>();
    _bigPortal = captureMarker;
  }

  public void SetMarkerLoading(int markerIndex)
  {
    if (!(_captureMarkers.Count > markerIndex) || markerIndex < 0)
    {
      Debug.LogError($"Attempted to activate invalid capture marker index {markerIndex}");
      return;
    }
    _captureMarkers[markerIndex].SetLoadingState();
  }

  public void SetBigPortalLoading()
  {
    if (_bigPortal == null)
    {
      Debug.LogError("Did not have big portal to set to loading");
      return;
    }
    _bigPortal.SetLoadingState();
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

  public void SetBigPortalActivatable(Texture2D portalImage, Action onActivated)
  {
    if (_bigPortal == null)
    {
      Debug.LogError("Did not have big portal to mark activatable");
      return;
    }
    _bigPortal.MarkActivatable(portalImage, onActivated);
  }

  public void ActivateMarker(int index)
  {
    _captureMarkers[index].Activate();
  }

  public void ActivatePortal()
  {
    _bigPortal.Activate();
  }

  public bool GetAllMarkersActivatable()
  {
    foreach (var marker in _captureMarkers)
    {
      if (!marker.Activatable) return false;
    }
    return true;
  }

  public Vector3 GetBigPortalPosition()
  {
    if (_bigPortal == null)
    {
      Debug.LogError("Did not have big portal to request position of");
      return Vector3.zero;
    }
    return _bigPortal.transform.position;
  }
}