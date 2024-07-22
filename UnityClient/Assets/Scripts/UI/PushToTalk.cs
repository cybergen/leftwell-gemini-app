using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using BriLib;

public class PushToTalkButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
  [SerializeField] private Button _button;
  [SerializeField] private int _animationMillis;
  private RectTransform _rect;
  private Vector3 _shownAnchoredPosition;
  private float _height;
  private int _milliStep = 10;
  private bool _animating;
  private bool _shown;
  private bool _pressed;
  private Action _onPress;
  private Action _onRelease;

  public async void Show(Action onPress, Action onRelease)
  {
    _onPress = onPress;
    _onRelease = onRelease;
    if (_shown || _animating) return;
    await Animate(true);
    _button.enabled = true;
    _shown = true;
  }

  public async void Hide()
  {
    if (!_shown || _animating) return;
    await Animate(false);
    _button.enabled = false;
    _shown = false;
  }

  private async Task Animate(bool show)
  {
    _animating = true;
    _button.enabled = false;
    var startHeight = show ? -_height : _shownAnchoredPosition.y;
    var endHeight = show ? _shownAnchoredPosition.y : -_height;
    var elapsedMillis = 0;

    while (elapsedMillis < _animationMillis)
    {
      var progress = Easing.ExpoEaseOut((float)elapsedMillis / _animationMillis);
      var height = Mathf.Lerp(startHeight, endHeight, progress);
      _rect.anchoredPosition = new Vector3(_shownAnchoredPosition.x, height, _shownAnchoredPosition.z);
      await Task.Delay(_milliStep);
      elapsedMillis += _milliStep;
    }

    _rect.anchoredPosition = new Vector3(_shownAnchoredPosition.x, endHeight, _shownAnchoredPosition.z);
    _animating = false;
  }

  private void Awake()
  {
    _rect = gameObject.GetComponent<RectTransform>();
    _shownAnchoredPosition = _rect.anchoredPosition;
    _height = _rect.rect.height;
    _shown = false;
    _animating = false;
    _rect.anchoredPosition = new Vector3(_shownAnchoredPosition.x, -_height, _shownAnchoredPosition.z);
  }

  public void OnPointerDown(PointerEventData eventData)
  {
    _pressed = true;
    _onPress?.Invoke();
  }

  public void OnPointerUp(PointerEventData eventData)
  {
    _pressed = false;
    _onRelease?.Invoke();
  }
}