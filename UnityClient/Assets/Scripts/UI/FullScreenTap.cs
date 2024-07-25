using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using BriLib;

public class FullScreenTapButton : MonoBehaviour
{
  [SerializeField] private int _animationMillis;
  [SerializeField] private CanvasGroup _canvas;
  [SerializeField] private RectTransform _button;
  [SerializeField] private TMP_Text _text;
  [SerializeField] private FadeOutImage _flashImage;
  private float _buttonTargetY;
  private int _milliStep = 10;
  private bool _shown;
  private Action _onPress;
  private CancellationTokenSource _cancellationTokenSource;

  public void OnPress()
  {
    _onPress?.Invoke();
    _flashImage.TriggerAnimate();
  }

  public async void Show(string text, Action onPress)
  {
    _onPress = onPress;
    _text.text = text;

    if (_shown) return;

    _cancellationTokenSource?.Cancel();
    _cancellationTokenSource = new CancellationTokenSource();

    gameObject.SetActive(true);
    _shown = true;
    await Animate(true, _cancellationTokenSource.Token);
    if (_cancellationTokenSource.Token.IsCancellationRequested) return;
    _canvas.interactable = true;
  }

  public async void Hide()
  {
    if (!_shown) return;

    _cancellationTokenSource?.Cancel();
    _cancellationTokenSource = new CancellationTokenSource();

    _shown = false;
    _canvas.interactable = false;
    await Animate(false, _cancellationTokenSource.Token);
    if (_cancellationTokenSource.Token.IsCancellationRequested) return;
    gameObject.SetActive(false);
  }

  private async Task Animate(bool show, CancellationToken token)
  {
    _canvas.interactable = false;

    var startAlpha = _canvas.alpha;
    var endAlpha = show ? 1f : 0f;
    var startY = _button.anchoredPosition.y;
    var endY = show ? _buttonTargetY : -_buttonTargetY;
    var elapsedMillis = 0;

    while (elapsedMillis < _animationMillis)
    {
      if (token.IsCancellationRequested) break;

      var progress = Easing.ExpoEaseOut((float)elapsedMillis / _animationMillis);
      
      var alpha = Mathf.Lerp(startAlpha, endAlpha, progress);
      _canvas.alpha = alpha;

      var targetY = Mathf.Lerp(startY, endY, progress);
      var buttonPosition = _button.anchoredPosition;
      buttonPosition.y = targetY;
      _button.anchoredPosition = buttonPosition;

      await Task.Delay(_milliStep, token);
      elapsedMillis += _milliStep;
    }

    if (!token.IsCancellationRequested)
    {
      _canvas.alpha = endAlpha;
    }
  }

  private void Awake()
  {
    _shown = false;
    _canvas.alpha = 0f;
    _buttonTargetY = _button.anchoredPosition.y;
    var pos = _button.anchoredPosition;
    pos.y = -_buttonTargetY;
    _button.anchoredPosition = pos; 
    gameObject.SetActive(false);
    _cancellationTokenSource = new CancellationTokenSource();
  }

  private void OnDestroy()
  {
    _cancellationTokenSource.Cancel();
  }
}
