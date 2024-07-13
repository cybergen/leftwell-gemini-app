using System;
using System.Collections.Generic;
using UnityEngine;

namespace BriLib.UI
{
  /// <summary>
  /// An area on the screen into which panels can be added. At launch, Region
  /// will look for all Panels are children of it, and add them to a manifest for
  /// later retrieval.
  /// </summary>
  public class Region : MonoBehaviour
  {
    public RectTransform Rect { get; private set; }

    //TODO $BS - Add stack behavior
    private IUIPanel _activePanel;
    private Dictionary<Type, IUIPanel> _panelMap = new Dictionary<Type, IUIPanel>();

    /// <summary>
    /// Show a panel of type T, that expects data of type K to be supplied to it. This will begin
    /// by hiding the active panel (if there is one). On completion of the old panel's close animation,
    /// the Region will then Show the new panel, piping in the new data, and await completion of the
    /// open animation. On animation completion, the UIManager is told to allow interaction again
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="K"></typeparam>
    public void ShowPanel<T, K>(K data) where T : Panel<K>
    {
      var _newPanel = GetPanel<T, K>();
      
      if (_activePanel != null && !ReferenceEquals(_newPanel, _activePanel))
      {
        UIManager.SetInteractable(false);
        _activePanel.Hide(() =>
        {
          _activePanel = null;
          ShowPanel<T, K>(data);
        });
      }
      else
      {
        var panel = GetPanel<T, K>();
        _activePanel = panel;
        UIManager.SetInteractable(false);
        panel.transform.SetAsLastSibling();
        panel.Show(data, () => UIManager.SetInteractable(true));
      }
    }

    /// <summary>
    /// Shows a panel of type T, that does not require any additional data piped in. As in the case of
    /// the data receiving ShowPanel method above, this hides the prior active panel before opening the
    /// new one.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void ShowPanel<T>() where T : Panel
    {
      if (_activePanel != null)
      {
        UIManager.SetInteractable(false);
        _activePanel.Hide(() =>
        {
          _activePanel = null;
          ShowPanel<T>();
        });
      }
      else
      {
        var panel = GetPanel<T>();
        _activePanel = panel;
        UIManager.SetInteractable(false);
        panel.Show(() => UIManager.SetInteractable(true));
      }
    }

    /// <summary>
    /// Hides the current panel (if there is one)
    /// </summary>
    public void HidePanel()
    {
      if (_activePanel != null) _activePanel.Hide(null);
    }

    /// <summary>
    /// Retrieve a panel of type T that expects data of type K from our panel map, if it exists
    /// </summary>
    /// <typeparam name="K"></typeparam>
    private Panel<K> GetPanel<T, K>() where T : Panel<K>
    {
      var type = typeof(T);
      if (!_panelMap.ContainsKey(type))
      {
        LogManager.Error("Failed to retrieve panel with type " + type + " that was not in UI heirarchy of parent " + name);
        return null;
      }
      var panel = _panelMap[type];
      if (!(panel is Panel<K>))
      {
        LogManager.Error("Data did not take expected data type " + typeof(K));
        return null;
      }
      return _panelMap[type] as Panel<K>;
    }

    /// <summary>
    /// Retrieve a panel of type T from our map, with no expectation of special data passed in
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private Panel GetPanel<T>() where T : Panel
    {
      var type = typeof(T);
      if (!_panelMap.ContainsKey(typeof(T)))
      {
        LogManager.Error("Failed to retrieve panel with type " + type + " that was not in UI heirarchy of parent " + name);
        return null;
      }
      return _panelMap[type] as T;
    }

    /// <summary>
    /// On awake, find all Panels in our children and add to our map
    /// </summary>
    private void Awake()
    {
      Rect = GetComponent<RectTransform>();
      var go = gameObject;
      var childPanels = go.GetComponentsInChildren<Panel>(includeInactive: true);
      for (int i = 0; i < childPanels.Length; i++)
      {
        var type = childPanels[i].GetType();
        if (_panelMap.ContainsKey(type))
        {
          LogManager.Error("Tried to add more than one panel with type " + type);
          continue;
        }
        _panelMap.Add(type, childPanels[i]);
      }
    }
  }
}