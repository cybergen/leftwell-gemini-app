using UnityEngine;

public class PortalActivater : MonoBehaviour
{
  [SerializeField] FullScreenTapButton _screenUI;
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
      _screenUI.Show("Activate portal!", OnPress);
    }
    else if (!active && _wasActive)
    {
      _screenUI.Hide();
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
}