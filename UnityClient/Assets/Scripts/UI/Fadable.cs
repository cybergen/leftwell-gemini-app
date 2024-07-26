using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using BriLib;

public class Fadable : MonoBehaviour
{
  public bool Animating { get; private set; } = false;
  [SerializeField] private CanvasGroup _canvas;
  private int _animationMillis;
  private int _milliStep = 16;
  private CancellationTokenSource _cancellationTokenSource;
  private bool _shown;

  public async void Show(int animationMillis)
  {
    if (_shown) return;

    _animationMillis = animationMillis;
    Animating = true;

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

    Animating = true;

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
    var elapsedMillis = 0;

    while (elapsedMillis < _animationMillis)
    {
      if (token.IsCancellationRequested) break;

      var progress = Easing.ExpoEaseOut((float)elapsedMillis / _animationMillis);

      var alpha = Mathf.Lerp(startAlpha, endAlpha, progress);
      _canvas.alpha = alpha;

      await Task.Delay(_milliStep, token);
      elapsedMillis += _milliStep;
    }

    if (!token.IsCancellationRequested)
    {
      _canvas.alpha = endAlpha;
    }

    Animating = false;
  }

  private void Awake()
  {
    _shown = false;
    _canvas.alpha = 0f;
    gameObject.SetActive(false);
    _cancellationTokenSource = new CancellationTokenSource();
  }

  private void OnDestroy()
  {
    _cancellationTokenSource.Cancel();
  }
}