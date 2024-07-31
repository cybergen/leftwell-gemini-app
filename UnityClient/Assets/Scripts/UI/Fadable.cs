using UnityEngine;
using BriLib;

public class Fadable : MonoBehaviour
{
  public bool Animating { get; private set; } = false;

  [SerializeField] private CanvasGroup _canvas;
  private float _animationDuration;
  private float _elapsedSeconds;
  private float _startAlpha;
  private float _endAlpha;
  private bool _shown;

  public void Show(float animationDuration)
  {
    if (_shown) return;

    _animationDuration = animationDuration;
    Animating = true;
    gameObject.SetActive(true);
    _shown = true;
    _canvas.interactable = false;
    StartAnimation(true);
  }

  public void Hide(float animationDuration)
  {
    if (!_shown) return;

    _animationDuration = animationDuration;
    Animating = true;
    _shown = false;
    _canvas.interactable = false;
    StartAnimation(false);
  }

  private void StartAnimation(bool show)
  {
    _elapsedSeconds = 0f;
    _startAlpha = _canvas.alpha;
    _endAlpha = show ? 1f : 0f;
  }

  private void Update()
  {
    if (Animating)
    {
      _elapsedSeconds += Time.deltaTime;
      float progress = Mathf.Clamp01(_elapsedSeconds / _animationDuration);
      float easedProgress = Easing.ExpoEaseOut(progress);
      _canvas.alpha = Mathf.Lerp(_startAlpha, _endAlpha, easedProgress);

      if (_elapsedSeconds >= _animationDuration)
      {
        Animating = false;
        _canvas.alpha = _endAlpha;

        if (_endAlpha == 0f)
        {
          gameObject.SetActive(false);
        }
        else
        {
          _canvas.interactable = true;
        }
      }
    }
  }

  private void Awake()
  {
    _shown = false;
    _canvas.alpha = 0f;
    gameObject.SetActive(false);
  }
}