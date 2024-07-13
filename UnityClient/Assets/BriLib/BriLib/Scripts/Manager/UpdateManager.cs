using System;
using System.Collections.Generic;
using UnityEngine;

namespace BriLib
{
  /// <summary>
  /// System by which a non-monobehaviour object can easily register to get ticked
  /// within the normal Unity update loop. Allows to operate over many frames without
  /// requiring being added to a game object in a scene.
  /// </summary>
  public class UpdateManager : Singleton<UpdateManager>
  {
    private List<Action<float>> _updaters = new List<Action<float>>();
    private List<Action<float>> _addList = new List<Action<float>>();
    private List<Action<float>> _removeList = new List<Action<float>>();

    public void AddUpdater(Action<float> onUpdate)
    {
      _addList.Add(onUpdate);
    }

    public void RemoveUpdater(Action<float> onUpdate)
    {
      _removeList.Add(onUpdate);
    }

    private void Update()
    {
      _updaters.ForEach(u => u.Execute(Time.deltaTime));
      _removeList.ForEach(u => _updaters.Remove(u));
      _addList.ForEach(u => _updaters.Add(u));
      _removeList.Clear();
      _addList.Clear();
    }
  }
}