using UnityEngine;

public class PortalActivater : MonoBehaviour
{
  [SerializeField] private float _targetAlpha;
  [SerializeField] private float _alphaAmplitude;
  [SerializeField] private float _phaseTime;
  [SerializeField] private CanvasGroup _canvasGroup;
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
    var currentAlpha = _canvasGroup.alpha;

    _canvasGroup.interactable = active;
    _canvasGroup.blocksRaycasts = !active;

    var sineTime = Mathf.Sin(((Time.time % _phaseTime) / _phaseTime * 360f) * Mathf.Deg2Rad);
    var bonusAlpha = active ? sineTime * _alphaAmplitude : 0f;

    _canvasGroup.alpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime) + bonusAlpha;
  }

  private void FixedUpdate()
  {
    if (!_showable) return;

    RaycastHit hit;
    if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out hit))
    {
      _activatable = hit.collider.gameObject.GetComponent<IActivatable>();
    }
    _activatable = null;
  }

  private void Awake()
  {
    _cameraTransform = Camera.main.transform;
  }
}