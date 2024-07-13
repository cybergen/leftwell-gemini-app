using UnityEngine;

namespace BriLib.UI
{
  /// <summary>
  /// Primary entry point for all UI. Used to show and hide panels placed at different
  /// onscreen regions.
  /// </summary>
  public class UIManager : Singleton<UIManager>
  {
    public static Region Header { get { return Instance._header; } }
    public static Region Footer { get { return Instance._footer; } }
    public static Region Body { get { return Instance._body; } }
    public static Region Overlay { get { return Instance._overlay; } }

    [SerializeField] private Region _header;
    [SerializeField] private Region _footer;
    [SerializeField] private Region _body;
    [SerializeField] private Region _overlay;
    [SerializeField] private RectTransform _regionContainer;
    [SerializeField] private GameObject _interactionBlocker;

    private Vector2 _currentScreenSize;
    private Rect _safeArea;

    /// <summary>
    /// Controls an interaction blocker that will intercept touch events
    /// </summary>
    /// <param name="interactable"></param>
    public static void SetInteractable(bool interactable)
    {
      Instance._interactionBlocker.SetActive(!interactable);
    }

    /// <summary>
    /// Tell all regions to hide their current screens
    /// </summary>
    public static void HideAll()
    {
      Header.HidePanel();
      Footer.HidePanel();
      Body.HidePanel();
      Overlay.HidePanel();
    }

    /// <summary>
    /// On initialization, UIManager will update positions/sizes of main canvas based on safeArea for notched devices
    /// target device
    /// </summary>
    public override void Begin()
    {
      base.Begin();

      _currentScreenSize = new Vector2(Screen.width, Screen.height);
      _safeArea = Screen.safeArea;

      SetSafeArea();
    }

    private void SetSafeArea()
    {
      //https://forum.unity.com/threads/canvashelper-resizes-a-recttransform-to-iphone-xs-safe-area.521107/
      var safeArea = Screen.safeArea;
      var screenSize = new Vector2(Screen.width, Screen.height);
      var anchorMin = safeArea.position;
      var anchorMax = safeArea.position + safeArea.size;
      anchorMin.x /= screenSize.x;
      anchorMin.y /= screenSize.y;
      anchorMax.x /= screenSize.x;
      anchorMax.y /= screenSize.y;

      LogManager.Info("SAFE AREA: " + safeArea);
      LogManager.Info("ORIGINAL SCREEN SIZE: " + screenSize);

      _regionContainer.anchorMin = anchorMin;
      _regionContainer.anchorMax = anchorMax;
    }

    private void Update()
    {
      var newSize = new Vector2(Screen.width, Screen.height);
      if (_currentScreenSize != newSize)
      {
        LogManager.Info("DEVICE ORIENTATION CHANGED, RECALCULATING SCREEN SIZE");
        _currentScreenSize = newSize;
        _safeArea = Screen.safeArea;
        SetSafeArea();
      }
    }
  }
}