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

  public void SpawnCaptureMarker(Texture2D texture)
  {
    var pose = CameraManager.Instance.GetCameraPose();
    var pos = pose.Item1 + (pose.Item2 * Vector3.forward) * _forwardSpawnDistance;
    var marker = Instantiate(_smallMarkerPrefab, pos, pose.Item2);
    var captureMarker = marker.GetComponent<ItemCaptureMarker>();
    captureMarker.Spawn(texture);
    _captureMarkers.Add(captureMarker);
  }

  public void SpawnBigPortal()
  {
    var spawnPose = CameraManager.Instance.GetCameraPose();
    var spawnPos = new Vector3(spawnPose.Item1.x, PlaneManager.Instance.GroundHeight + _bigPortalSpawnHeight, spawnPose.Item1.z);
    var forwardDir = (CameraManager.Instance.GetCameraPose().Item1 - spawnPos).normalized;
    var marker = Instantiate(_bigPortal, spawnPos, Quaternion.LookRotation(forwardDir));
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

  public Vector3 GetBigPortalPosition()
  {
    if (_bigPortal == null)
    {
      Debug.LogError("Did not have big portal to request position of");
      return Vector3.zero;
    }
    return _bigPortal.transform.position;
  }

  public void SupplyTransformedImage(int markerIndex, Texture2D finalImage, Action onActivated)
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
}