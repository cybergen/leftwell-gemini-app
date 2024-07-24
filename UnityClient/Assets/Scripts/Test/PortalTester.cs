using System;
using UnityEngine;
using BriLib;
using JetBrains.Annotations;

public class PortalTester : MonoBehaviour
{
  [SerializeField] private PortalActivater _activater;
  [SerializeField] private Texture2D _markerImage;
  [SerializeField] private Texture2D _portalImage;
  private int _markers = 0;
  private bool _hasBigPortal;

  public void OnSpawnMarker()
  {
    _markers++;
    PortalManager.Instance.SpawnCaptureMarker();
  }

  public void OnSetMarkersActivatable()
  {
    if (_markers == 0) return;
    for (int i = 0; i < _markers; i++)
    {
      PortalManager.Instance.SetMarkerActivatable(i, _markerImage, null);
    }
  }

  public void OnSpawnPortal()
  {
    if (_hasBigPortal) return;
    _hasBigPortal = true;
    PortalManager.Instance.SpawnBigPortal();
  }

  public void OnSetPortalLoading()
  {
    if (!_hasBigPortal) return;
    PortalManager.Instance.SetBigPortalLoading();
  }

  public void OnSetPortalActivatable()
  {
    if (!_hasBigPortal) return;
    PortalManager.Instance.SetBigPortalActivatable(_portalImage, null);
  }

  private void Awake()
  {
    _activater.SetShowable(true);
    
  }
}