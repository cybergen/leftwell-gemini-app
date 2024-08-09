using UnityEngine;
using BriLib;

public class VerticalSlidingElement : MonoBehaviour
{
  public bool Animating { get; private set; } = false;
  public bool Shown { get; private set; } = false;
  private RectTransform _self;
  private float _finalY;
  private float _animationDuration;
  private float _elapsedSeconds;
  private float _startPositionY;
  private float _targetPositionY;

  public void Show(float animationDuration)
  {
    Shown = true;
    _animationDuration = animationDuration;
    gameObject.SetActive(true);
    StartAnimation(true);
  }

  public void Hide(float animationDuration)
  {
    Shown = false;
    _animationDuration = animationDuration;
    StartAnimation(false);
  }

  private void StartAnimation(bool show)
  {
    Animating = true;
    _elapsedSeconds = 0f;
    _startPositionY = _self.anchoredPosition.y;
    _targetPositionY = show ? _finalY : -_finalY;
  }

  private void Update()
  {
    if (Animating)
    {
      _elapsedSeconds += Time.deltaTime;

      float progress = Mathf.Clamp01(_elapsedSeconds / _animationDuration);
      float easedProgress = Easing.ExpoEaseOut(progress);
      float targetY = Mathf.Lerp(_startPositionY, _targetPositionY, easedProgress);

      var selfPos = _self.anchoredPosition;
      selfPos.y = targetY;
      _self.anchoredPosition = selfPos;

      if (_elapsedSeconds >= _animationDuration)
      {
        Animating = false;
        var finalPosition = _self.anchoredPosition;
        finalPosition.y = _targetPositionY;
        _self.anchoredPosition = finalPosition;

        if (_targetPositionY == -_finalY)
        {
          gameObject.SetActive(false);
        }
      }
    }
  }

  private void Awake()
  {
    _self = GetComponent<RectTransform>();
    _finalY = _self.anchoredPosition.y;
    var pos = _self.anchoredPosition;
    pos.y = -_finalY;
    _self.anchoredPosition = pos;
    gameObject.SetActive(false);
  }
}
