using UnityEngine;

public class PortalActivater : MonoBehaviour
{
  [SerializeField] private float _targetAlpha;
  [SerializeField] private float _alphaAmplitude;
  [SerializeField] private float _phaseTime;
  [SerializeField] private CanvasGroup _canvasGroup;
  [SerializeField] private UnityEngine.UI.Image _image;
  private float _realAlpha;
  private Transform _cameraTransform;
  private bool _showable;
  private IActivatable _activatable;

  public void SetShowable(bool showable)
  {
    _showable = showable;
    if (!_showable) _activatable = null;
  }

  public void OnPress()
  {
    if (_activatable == null) return;
    _activatable.Activate();
  }

  private void Update()
  {
    var active = _showable && _activatable != null && _activatable.Activatable;
    var targetAlpha = active ? _targetAlpha : 0f;
    var currentAlpha = _realAlpha;

    _canvasGroup.interactable = active;
    _canvasGroup.blocksRaycasts = active;
    _image.raycastTarget = active;

    var sineTime = Mathf.Sin(((Time.time % _phaseTime) / _phaseTime * 360f) * Mathf.Deg2Rad);
    var bonusAlpha = active ? sineTime * _alphaAmplitude : 0f;

    _realAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime);
    _canvasGroup.alpha = _realAlpha + bonusAlpha;
  }

  private void FixedUpdate()
  {
    if (!_showable) return;

    var ray = new Ray(_cameraTransform.position, _cameraTransform.forward);
    var mask = LayerMask.GetMask("Portal");
    RaycastHit hit;
    if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
    {
      _activatable = hit.collider.gameObject.GetComponent<IActivatable>();
    }
    else
    {
      _activatable = null;
    }
  }

  private void Awake()
  {
    _cameraTransform = Camera.main.transform;
    _canvasGroup.interactable = false;
    _canvasGroup.blocksRaycasts = false;
    _image.raycastTarget = false;
    _canvasGroup.alpha = 0f;
    _realAlpha = 0f;
  }
}