using System;
using System.Collections.Generic;
using UnityEngine;

namespace BriLib
{
  /// <summary>
  /// Basic manager for input events. Queryable touch events, as well as stack-like lists of listeners for key code
  /// input events (such as the back button). When added as a listener, the listener can return true to indicate that
  /// it has consumed the input, in order to prevent further propagation. This allows behavior such as a modal
  /// to register for the back key, act on it, and prevent the other UI that registered on the back button
  /// previously from also acting on it. Alternatively, if a new key listener were added that did not want to
  /// prevent prior listeners from acting on the input, it could return false and allow propagation.
  /// </summary>
  public class InputManager : Singleton<InputManager>
  {
    #region Touch Control Queries
    public bool TouchBegan
    {
      get
      {
#if UNITY_EDITOR
        return Input.GetMouseButtonDown(0);
#else
      return Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began;
#endif
      }
    }

    public bool Touching
    {
      get
      {
#if UNITY_EDITOR
        return Input.GetMouseButton(0);
#else
      return Input.touchCount > 0;
#endif
      }
    }

    public bool TouchEnding
    {
      get
      {
#if UNITY_EDITOR
        return Input.GetMouseButtonUp(0);
#else
      return Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended;
#endif
      }
    }

    public Vector2 Position
    {
      get
      {
#if UNITY_EDITOR
        return Input.mousePosition;
#else
      return Input.touchCount > 0 ? Input.touches[0].position : Vector2.zero;
#endif
      }
    }
    #endregion

    private Dictionary<KeyCode, List<Func<bool>>> _inputStacks = new Dictionary<KeyCode, List<Func<bool>>>();

    /// <summary>
    /// Insert a keycode consumer into a list at the beginning of list. The function should return true
    /// if it will consume the key input event and prevent propagation of the event to prior-inserted
    /// listeners
    /// </summary>
    /// <param name="code"></param>
    /// <param name="listenerFunction"></param>
    public void AddKeyListener(KeyCode code, Func<bool> listenerFunction)
    {
      if (listenerFunction == null)
      {
        LogManager.Error("Cannot insert null function into listener list");
        return;
      }
      if (!_inputStacks.ContainsKey(code)) _inputStacks.Add(code, new List<Func<bool>>());
      _inputStacks[code].Insert(0, listenerFunction);
    }

    /// <summary>
    /// Remove the input listener from the list for the specified key code
    /// </summary>
    /// <param name="code"></param>
    /// <param name="listenerFunction"></param>
    public void RemoveKeyListener(KeyCode code, Func<bool> listenerFunction)
    {
      if (!_inputStacks.ContainsKey(code))
      {
        LogManager.Error("No entries in list for keycode: " + code);
        return;
      }
      _inputStacks[code].Remove(listenerFunction);
    }

    private void Update()
    {
      for (var enumerator = _inputStacks.GetEnumerator(); enumerator.MoveNext();)
      {
        if (Input.GetKeyDown(enumerator.Current.Key))
        {
          for (int i = 0; i < enumerator.Current.Value.Count; i++) if (enumerator.Current.Value[i]()) break;
        }
      }
    }
  }
}