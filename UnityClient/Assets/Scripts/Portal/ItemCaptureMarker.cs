using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ItemCaptureMarker : MonoBehaviour, IActivatable
{
  public bool Activatable { get; private set; }
  [SerializeField] private MeshRenderer _renderer;
  [SerializeField] private GameObject _imagePlane;
  [SerializeField] private List<GameObject> _frameSparks;
  [SerializeField] private int _animationMillis;
  [SerializeField] private float _targetHeightScale;
  private int _elapsedAnimationMillis;
  private Texture2D _finalImage;
  private Action _onActivated;

  public void Spawn()
  {
    
  }

  public void SetLoadingState()
  {

  }

  public void MarkActivatable(Texture2D finalImage, Action onActivated)
  {
    _finalImage = finalImage;
    _onActivated = onActivated;

    var dimensionRatio = ((float)_finalImage.width / _finalImage.height) * _targetHeightScale;
    var scale = _imagePlane.transform.localScale;
    scale.x *= dimensionRatio;
    _imagePlane.transform.localScale = scale;

    _imagePlane.SetActive(true);
    foreach (var frame in _frameSparks)
    {
      frame.SetActive(true);
    }
    Activatable = true;
  }

  public async void Activate()
  {
    _renderer.material.mainTexture = _finalImage;
    _onActivated?.Invoke();
    Activatable = false;
    await AnimateOpen();
  }

  private async Task AnimateOpen()
  {
    var scale = _imagePlane.transform.localScale;
    while (_elapsedAnimationMillis < _animationMillis)
    {
      var progress = BriLib.Easing.ExpoEaseOut((_elapsedAnimationMillis / (float)_animationMillis));
      var scaleZ = Mathf.Lerp(0f, _targetHeightScale, progress);
      scale.z = scaleZ;
      _imagePlane.transform.localScale = scale;
      _elapsedAnimationMillis += 16;
      await Task.Delay(16);
    }
    scale.z = _targetHeightScale;
    _imagePlane.transform.localScale = scale;
  }
}