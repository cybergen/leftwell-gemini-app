using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Threading.Tasks;
using UnityEngine;
using static NativeShare;

public class ItemCaptureMarker : MonoBehaviour, IActivatable
{
  public bool Activatable { get { return _state == MarkerState.Activatable || _state == MarkerState.Sharable; } }
  public string ActivationText { get { return _activationText; } }
  public bool ShareMode { get { return _state == MarkerState.Sharable; } }
  [SerializeField] private MeshRenderer _renderer;
  [SerializeField] private BoxCollider _collider;
  [SerializeField] private AudioFader _loopingSound;
  [SerializeField] private AudioSource _poofSound;
  [SerializeField] private AudioSource _chimeSound;
  [SerializeField] private GameObject _imagePlane;
  [SerializeField] private GameObject _explosionParticles;
  [SerializeField] private GameObject _initialSparks;
  [SerializeField] private GameObject _activatableTrail;
  [SerializeField] private List<GameObject> _frameSparks;
  [SerializeField] private int _animationMillis;
  private float _targetZScale;
  private int _elapsedAnimationMillis;
  private Texture2D _finalImage;
  private Action _onActivated;
  private MarkerState _state;
  private string _activationText;
  private Action _onShare;
  
  private enum MarkerState
  {
    None,
    Loading,
    Activatable,
    Activated,
    Sharable
  }

  public void MarkActivatable(Texture2D finalImage, Action onActivated)
  {
    _state = MarkerState.Activatable;
    _collider.enabled = true;
    _initialSparks.SetActive(false);
    _finalImage = finalImage;
    _onActivated = onActivated;

    var scale = _imagePlane.transform.localScale;
    _targetZScale = scale.x;
    var width = ((float)_finalImage.width / _finalImage.height) * _targetZScale;
    scale.x = width;
    _imagePlane.transform.localScale = scale;

    _imagePlane.SetActive(true);
    _activatableTrail.SetActive(true);

    _chimeSound.enabled = true;
    _activationText = "Activate Item of Power";
  }

  public void MarkSharable(string shareText, Action onShare)
  {
    _state = MarkerState.Sharable;
    _collider.enabled = true;
    _activationText = shareText;
    _onShare = onShare;
  }

  public async void Activate()
  {
    if (_state == MarkerState.Activatable)
    {
      _state = MarkerState.Activated;

      _poofSound.enabled = true;
      _renderer.enabled = true;
      _renderer.material.mainTexture = _finalImage;
      _onActivated?.Invoke();
      _activatableTrail.SetActive(false);
      _explosionParticles.SetActive(true);
      _collider.enabled = false;

      foreach (var frame in _frameSparks)
      {
        frame.SetActive(true);
      }
      await AnimateOpen();
    }
    else if (_state == MarkerState.Sharable)
    {
      var share = new NativeShare();
      share.AddFile(_finalImage);
      share.SetTitle("Teleportation Turmoil item transformation!");
      share.SetCallback(OnShareResult);
      share.Share();
    }
  }

  private void OnShareResult(ShareResult result, string shareTarget)
  {
    Debug.Log($"Got share result {result}");
    _onShare?.Invoke();
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

  private void Start()
  {
    _state = MarkerState.Loading;
    _loopingSound.FadeIn();
    _renderer.enabled = false;
  }
}