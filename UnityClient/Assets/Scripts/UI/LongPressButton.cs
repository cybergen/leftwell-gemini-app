using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using BriLib;

public class LongPressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
  [Header("Asset Refs")]
  [SerializeField] private Button _button;
  [SerializeField] private Image _radialProgress;
  [SerializeField] private RectTransform _bgCircle;

  [Header("Animation Config")]
  [SerializeField] private float _slideUpAnimSeconds;
  [SerializeField] private float _circleExpandAnimSeconds;
  [SerializeField] private float _progressFillDurationSeconds;

  //Vertical animation fields
  private bool _shown;
  private RectTransform _rect;
  private Vector3 _shownAnchoredPosition;
  private float _expandedHeight;
  private float _animStartHeight;
  private float _animTargetHeight;
  private float _verticalAnimElapsed;

  //Pressed animation fields
  private Color _radialBarColor;
  private bool _pressed;
  private float _circleAnimElapsed;
  private float _circleTargetScale;
  private float _circleStartScale;
  private float _heldDuration;
  private bool _animatingVertically;
  private bool _animatingCircle;

  //Callbacks
  private Action _onPress;
  private Action _onRelease;

  public void Show(Action onPress, Action onRelease)
  {
    _onPress = onPress;
    _onRelease = onRelease;
    gameObject.SetActive(true);

    if (_shown) return;

    _shown = true;
    _animatingVertically = true;
    _verticalAnimElapsed = 0f;
    _animTargetHeight = _expandedHeight;
    _animStartHeight = _rect.anchoredPosition.y;
  }

  public void Hide()
  {
    if (!_shown) return;

    _shown = false;
    _pressed = false;
    _button.interactable = false;
    _button.enabled = false;

    _verticalAnimElapsed = 0f;
    _animatingVertically = true;
    _animTargetHeight = -_expandedHeight;
    _animStartHeight = _rect.anchoredPosition.y;
  }

  private void Update()
  {
    //Resolve circle showing/hiding before animating vertically
    if (_animatingCircle)
    {
      _circleAnimElapsed += Time.deltaTime;
      var progress = Mathf.Clamp01(Easing.ExpoEaseOut(_circleAnimElapsed / _circleExpandAnimSeconds));
      var scale = (_circleTargetScale - _circleStartScale) * progress + _circleStartScale;
      _bgCircle.localScale = new Vector3(scale, scale, scale);
      _radialProgress.color = new Color(_radialBarColor.r, _radialBarColor.g, _radialBarColor.b, scale);

      if (_circleAnimElapsed >= _circleExpandAnimSeconds) { _animatingCircle = false; }
    }
    else if (_animatingVertically)
    {
      //Continue animation sequence
      _verticalAnimElapsed += Time.deltaTime;
      var progress = Mathf.Clamp01(Easing.ExpoEaseOut(_verticalAnimElapsed / _slideUpAnimSeconds));
      var height = (_animTargetHeight - _animStartHeight) * progress + _animStartHeight;
      _rect.anchoredPosition = new Vector3(_shownAnchoredPosition.x, height, _shownAnchoredPosition.z);

      //After completion, do any animation resetting/cleanup as well as interactable setup
      if (_verticalAnimElapsed >= _slideUpAnimSeconds)
      {
        _animatingVertically = false;
        _button.interactable = _shown;
        _button.enabled = _shown;
        if (!_shown) { ResetState(); }
      }
    }

    if (_pressed)
    {
      _heldDuration += Time.deltaTime;
      _radialProgress.fillAmount = _heldDuration / _progressFillDurationSeconds;
    }
  }

  public void OnPointerDown(PointerEventData eventData)
  {
    _onPress?.Invoke();
    _heldDuration = 0f;
    _circleAnimElapsed = 0f;
    _circleTargetScale = 1f;
    _circleStartScale = _bgCircle.localScale.x;

    _animatingCircle = true;
    _pressed = true;
  }

  public void OnPointerUp(PointerEventData eventData)
  {
    _onRelease?.Invoke();
    _pressed = false;
    _animatingCircle = true;
    _circleAnimElapsed = 0f;
    _circleTargetScale = 0f;
    _circleStartScale = _bgCircle.localScale.x;
  }

  private void Awake()
  {
    _rect = gameObject.GetComponent<RectTransform>();
    _shownAnchoredPosition = _rect.anchoredPosition;
    _expandedHeight = _rect.rect.height;
    _radialBarColor = _radialProgress.color;
    ResetState();
  }

  private void ResetState()
  {
    _shown = false;
    _animatingVertically = false;
    _pressed = false;
    _heldDuration = 0f;
    _circleAnimElapsed = 0f;

    _rect.anchoredPosition = new Vector3(_shownAnchoredPosition.x, -_expandedHeight, _shownAnchoredPosition.z);
    _bgCircle.localScale = Vector3.zero;
    _radialProgress.fillAmount = 0f;
    _button.interactable = false;
    _button.enabled = false;
    gameObject.SetActive(false);
  }
}