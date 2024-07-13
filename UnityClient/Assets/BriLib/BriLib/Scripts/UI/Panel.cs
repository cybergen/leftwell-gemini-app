using System;
using System.Threading.Tasks;
using UnityEngine;

namespace BriLib.UI
{
  /// <summary>
  /// Base class for UI Panels, where T is a data type that the panel expects to receive from
  /// an external source and applies it when being Shown. In an MVC (model, view, controller) design pattern,
  /// the Data object of type T should be the model, and the Panel roughly conforms to the V and C parts.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class Panel<T> : Panel, IUIPanel
  {
    /// <summary>
    /// The data model supplied to this panel
    /// </summary>
    /// <value></value>
    public T Data { get; private set; }

    /// <summary>
    /// Instruct our Panel to show, supplying it a data model of type T. Includes optional
    /// callback to signal when the Show animation has resolved.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="onShowAnimationFinish"></param>
    public virtual void Show(T data, Action onShowAnimationFinish = null)
    {
      Data = data;
      Show(onShowAnimationFinish);
    }
  }

  /// <summary>
  /// Version of Panel that does not expect data to be passed in on Show
  /// </summary>
  public class Panel : MonoBehaviour, IUIPanel
  {
    protected Action _onShowAnimationFinish;
    protected Action _onHideAnimationFinish;
    protected Func<GameObject, Task> _showAnimation;
    protected Func<GameObject, Task> _hideAnimation;
    protected bool _interactable;
    protected bool _setupComplete;

    /// <summary>
    /// Show our panel with nothing but a callback to signal show animation completion.
    /// </summary>
    /// <param name="onShowAnimationFinish"></param>
    public virtual async void Show(Action onShowAnimationFinish = null)
    {
      _onShowAnimationFinish = onShowAnimationFinish;
      gameObject.SetActive(true);
      if (_showAnimation != null) await _showAnimation(gameObject);
      _onShowAnimationFinish.Execute();
    }

    /// <summary>
    /// Instruct our panel to hide itself with optional callback to signal animation completion.
    /// </summary>
    /// <param name="onHideAnimationFinish"></param>
    public virtual async void Hide(Action onHideAnimationFinish)
    {
      _onHideAnimationFinish = onHideAnimationFinish;
      if (_hideAnimation != null) await _hideAnimation(gameObject);
      gameObject.SetActive(false);
      _onHideAnimationFinish.Execute();
    }
  }

  /// <summary>
  /// Basic interface for all Panel's to simplify accumulation of Panels into lists
  /// and standardize interactions with them.
  /// </summary>
  public interface IUIPanel
  {
    void Show(Action onShowAnimationFinish);
    void Hide(Action onHideAnimationFinish);
  }
}