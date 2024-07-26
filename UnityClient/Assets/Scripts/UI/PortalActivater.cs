using UnityEngine;
using TMPro;

public class PortalActivater : MonoBehaviour
{
  [SerializeField] private VerticalSlidingElement _screenUI;
  [SerializeField] private VortexEffectController _vortex;
  [SerializeField] private TMP_Text _text;
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
      _text.text = "Open portal!";
      _screenUI.Show(450);
      _vortex.Show();
    }
    else if (!active && _wasActive)
    {
      _screenUI.Hide();
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