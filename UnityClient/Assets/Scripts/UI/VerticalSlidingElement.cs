using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using BriLib;

public class VerticalSlidingElement : MonoBehaviour
{
  public bool Animating { get; private set; } = false;
  private RectTransform _button;
  private int _animationMillis;
  private float _finalY;
  private int _milliStep = 16;
  private CancellationTokenSource _cancellationTokenSource;

  public async void Show(int animationDuration)
  {
    Animating = true;
    _animationMillis = animationDuration;
    gameObject.SetActive(true);

    // Cancel the previous animation if any
    _cancellationTokenSource?.Cancel();
    _cancellationTokenSource = new CancellationTokenSource();

    await Animate(true, _cancellationTokenSource.Token);
  }

  public async void Hide()
  {
    Animating = true;
    // Cancel the previous animation if any
    _cancellationTokenSource?.Cancel();
    _cancellationTokenSource = new CancellationTokenSource();

    await Animate(false, _cancellationTokenSource.Token);
    gameObject.SetActive(false);
  }

  private async Task Animate(bool show, CancellationToken cancellationToken)
  {
    var startY = _button.anchoredPosition.y;
    var endY = show ? _finalY : -_finalY;
    var elapsedMillis = 0;

    while (elapsedMillis < _animationMillis)
    {
      if (cancellationToken.IsCancellationRequested)
      {
        return; // Exit if animation is cancelled
      }

      var progress = Easing.ExpoEaseOut((float)elapsedMillis / _animationMillis);
      var targetY = Mathf.Lerp(startY, endY, progress);
      var buttonPosition = _button.anchoredPosition;
      buttonPosition.y = targetY;
      _button.anchoredPosition = buttonPosition;

      await Task.Delay(_milliStep, cancellationToken);
      elapsedMillis += _milliStep;
    }

    // Ensure final position is set
    var finalPosition = _button.anchoredPosition;
    finalPosition.y = endY;
    _button.anchoredPosition = finalPosition;

    Animating = false;
  }

  private void Awake()
  {
    _button = GetComponent<RectTransform>();
    _finalY = _button.anchoredPosition.y;
    var pos = _button.anchoredPosition;
    pos.y = -_finalY;
    _button.anchoredPosition = pos;
    gameObject.SetActive(false);
  }
}
