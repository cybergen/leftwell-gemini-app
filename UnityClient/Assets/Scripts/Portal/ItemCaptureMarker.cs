using System;
using UnityEngine;

public class ItemCaptureMarker : MonoBehaviour
{
  public CaptureMarkerState CaptureMarkerState { get; private set; } = CaptureMarkerState.Spawned;
  [SerializeField] private MeshRenderer _renderer;
  private Texture2D _finalImage;
  private Action _onActivated;

  public void Spawn(Texture2D initialImage)
  {
    _renderer.material.mainTexture = initialImage;
    CaptureMarkerState = CaptureMarkerState.Spawned;
  }

  public void SetLoadingState()
  {
    CaptureMarkerState = CaptureMarkerState.Loading;
  }

  public void MarkActivatable(Texture2D finalImage, Action onActivated)
  {
    _finalImage = finalImage;
    _onActivated = onActivated;
    CaptureMarkerState = CaptureMarkerState.Activatable;
  }

  public void Activate()
  {
    _renderer.material.mainTexture = _finalImage;
    CaptureMarkerState = CaptureMarkerState.Activated;
    _onActivated?.Invoke();
  }
}

public enum CaptureMarkerState
{
  None,
  Spawned,
  Loading,
  Activatable,
  Activated
}