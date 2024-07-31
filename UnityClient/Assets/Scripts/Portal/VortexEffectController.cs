using UnityEngine;
using BriLib;

public class VortexEffectController : MonoBehaviour
{
  [SerializeField] private Material _vortexMaterial;
  [Header("Phase Effect Config")]
  [SerializeField] private float _shownOpacity;
  [SerializeField] private float _opacityAmplitude;
  [SerializeField] private float _phaseTime;
  [Header("Show/Hide Config")]
  [SerializeField] private float _showAnimationDuration;
  private float _actualOpacity;
  private float _elapsedSeconds;
  private float _startOpacity;
  private float _endOpacity;
  private bool _animating;

  public void Show()
  {
    gameObject.SetActive(true);
    StartAnimation(true);
  }

  public void Hide()
  {
    StartAnimation(false);
  }

  private void StartAnimation(bool shown)
  {
    _animating = true;
    _elapsedSeconds = 0f;
    _startOpacity = _actualOpacity;
    _endOpacity = shown ? _shownOpacity : 0f;
  }

  private void Update()
  {
    if (_animating)
    {
      _elapsedSeconds += Time.deltaTime;
      float progress = Mathf.Clamp01(_elapsedSeconds / _showAnimationDuration);
      float easedProgress = Easing.ExpoEaseOut(progress);
      _actualOpacity = Mathf.Lerp(_startOpacity, _endOpacity, easedProgress);

      if (_elapsedSeconds >= _showAnimationDuration)
      {
        _animating = false;
        _actualOpacity = _endOpacity;

        if (_endOpacity == 0f)
        {
          gameObject.SetActive(false);
        }
      }
    }

    var sineTime = Mathf.Sin(((Time.time % _phaseTime) / _phaseTime * 360f) * Mathf.Deg2Rad);
    var alpha = sineTime * _opacityAmplitude * _actualOpacity; //Multiply by opacity to allow to fully fade
    _vortexMaterial.SetFloat("_Opacity", _actualOpacity + alpha);
  }

  private void Awake()
  {
    _vortexMaterial.SetFloat("_Opacity", 0f);
    _actualOpacity = 0f;
  }
}
