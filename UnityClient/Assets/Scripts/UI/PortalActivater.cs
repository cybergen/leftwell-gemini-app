using UnityEngine;
using TMPro;

public class PortalActivater : MonoBehaviour
{
  [SerializeField] private VerticalSlidingElement _screenUI;
  [SerializeField] private VortexEffectController _vortex;
  [SerializeField] private TMP_Text _text;
  [SerializeField] private float _animSeconds = 0.45f;
  [SerializeField] private GameObject _portalIcon;
  [SerializeField] private GameObject _shareIcon;
  private Transform _cameraTransform;
  private bool _showable;
  private bool _wasActive;
  private IActivatable _currentActivatable;

  public void SetShowable(bool showable, Transform camera)
  {
    _showable = showable;
    _cameraTransform = camera;
    if (!_showable) _currentActivatable = null;
  }

  public void OnPress()
  {
    if (_currentActivatable == null) return;
    _currentActivatable.Activate();
  }

  private void Update()
  {
    var active = _showable && _currentActivatable != null && _currentActivatable.Activatable;
    if (active && !_wasActive)
    {
      _text.text = _currentActivatable.ActivationText;
      _screenUI.Show(_animSeconds);
      if (!_currentActivatable.ShareMode) 
      {
        _portalIcon.SetActive(true);
        _shareIcon.SetActive(false);
        _vortex.Show();
      }
      else
      {
        _portalIcon.SetActive(false);
        _shareIcon.SetActive(true);
      }
    }
    else if (!active && _wasActive)
    {
      _screenUI.Hide(_animSeconds);
      _vortex.Hide();
      _text.text = string.Empty;
    }
    _wasActive = active;
  }

  private void FixedUpdate()
  {
    if (!_showable || _cameraTransform == null) return;

    var ray = new Ray(_cameraTransform.position, _cameraTransform.forward);
    var mask = LayerMask.GetMask("Portal");
    RaycastHit hit;
    if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
    {
      _currentActivatable = hit.collider.gameObject.GetComponent<IActivatable>();
    }
    else
    {
      _currentActivatable = null;
    }
  }

  private void Awake()
  {
    _text.text = string.Empty;
  }
}