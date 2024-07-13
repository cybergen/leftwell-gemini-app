using System;
using System.Collections.Generic;

namespace BriLib
{
  /// <summary>
  /// Simple queue for adding actions to the main thread (for use by things that might resolve/trigger actions off of main
  /// thread)
  /// </summary>
  public class MainThreadQueue : Singleton<MainThreadQueue>
  {
    private List<Action> _queuedActions = new List<Action>();

    public void QueueAction(Action action)
    {
      _queuedActions.Add(action);
    }

    private void Update()
    {
      for (int i = 0; i < _queuedActions.Count; i++)
      {
        _queuedActions[i].Execute();
      }
      _queuedActions.Clear();
    }
  }
}
