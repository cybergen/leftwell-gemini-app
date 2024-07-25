using System.Threading.Tasks;
using UnityEngine;
using BriLib;

public class VerticalSlidingElement : MonoBehaviour
{  
  private RectTransform _button;
  private int _animationMillis;
  private float _finalY;
  private int _milliStep = 16;

  public async void Show(int animationDuration)
  {
    _animationMillis = animationDuration;
    gameObject.SetActive(true);
    await Animate(true);
  }

  public async void Hide()
  {
    await Animate(false);
    gameObject.SetActive(false);
  }

  private async Task Animate(bool show)
  {
    var startY = _button.anchoredPosition.y;
    var endY = show ? _finalY : -_finalY;
    var elapsedMillis = 0;

    while (elapsedMillis < _animationMillis)
    {
      var progress = Easing.ExpoEaseOut((float)elapsedMillis / _animationMillis);

      var targetY = Mathf.Lerp(startY, endY, progress);
      var buttonPosition = _button.anchoredPosition;
      buttonPosition.y = targetY;
      _button.anchoredPosition = buttonPosition;

      await Task.Delay(_milliStep);
      elapsedMillis += _milliStep;
    }
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