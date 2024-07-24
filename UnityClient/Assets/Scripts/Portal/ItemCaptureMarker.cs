using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ItemCaptureMarker : MonoBehaviour, IActivatable
{
  public bool Activatable { get; private set; }
  [SerializeField] private MeshRenderer _renderer;
  [SerializeField] private GameObject _imagePlane;
  [SerializeField] private GameObject _explosionParticles;
  [SerializeField] private GameObject _initialSparks;
  [SerializeField] private List<GameObject> _frameSparks;
  [SerializeField] private int _animationMillis;
  private float _targetZScale;
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
    _initialSparks.SetActive(false);
    _finalImage = finalImage;
    _onActivated = onActivated;

    var scale = _imagePlane.transform.localScale;
    _targetZScale = scale.x;
    var width = ((float)_finalImage.width / _finalImage.height) * _targetZScale;
    scale.x = width;
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
    _explosionParticles.SetActive(true);
    await AnimateOpen();
  }

  private async Task AnimateOpen()
  {
    var scale = _imagePlane.transform.localScale;
    while (_elapsedAnimationMillis < _animationMillis)
    {
      var progress = BriLib.Easing.ExpoEaseOut((_elapsedAnimationMillis / (float)_animationMillis));
      var scaleZ = Mathf.Lerp(0f, _targetZScale, progress);
      scale.z = scaleZ;
      _imagePlane.transform.localScale = scale;
      _elapsedAnimationMillis += 16;
      await Task.Delay(16);
    }
    scale.z = _targetZScale;
    _imagePlane.transform.localScale = scale;
  }
}