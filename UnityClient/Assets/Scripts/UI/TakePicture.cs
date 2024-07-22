using System;
using System.Threading.Tasks;
using UnityEngine;
using BriLib;

public class TakePicture : MonoBehaviour
{
  [SerializeField] private int _animationMillis;
  [SerializeField] private CanvasGroup _canvas;
  private int _milliStep = 10;
  private bool _animating;
  private bool _shown;
  private Action _onPress;

  public void OnPress()
  {
    _onPress?.Invoke();
  }

  public async void Show(Action onPress)
  {
    _onPress = onPress;
    if (_shown || _animating) return;
    await Animate(true);
    _canvas.interactable = true;
    _shown = true;
  }

  public async void Hide()
  {
    if (!_shown || _animating) return;
    await Animate(false);
    _canvas.interactable = false;
    _shown = false;
  }

  private async Task Animate(bool show)
  {
    _animating = true;
    _canvas.interactable = false;
    var startAlpha = show ? 0f : 1f;
    var endAlpha = show ? 1f : 0f;
    var elapsedMillis = 0;

    while (elapsedMillis < _animationMillis)
    {
      var progress = Easing.ExpoEaseOut((float)elapsedMillis / _animationMillis);
      var alpha = Mathf.Lerp(startAlpha, endAlpha, progress);
      _canvas.alpha = alpha;
      await Task.Delay(_milliStep);
      elapsedMillis += _milliStep;
    }

    _canvas.alpha = endAlpha;
    _animating = false;
  }

  private void Awake()
  {
    _shown = false;
    _animating = false;
    _canvas.alpha = 0f;
  }
}