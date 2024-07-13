using System;
using System.Collections.Generic;

namespace BriLib
{
  public class StateManager : Singleton<StateManager>
  {
    private Dictionary<string, StateMachine> _machineMap = new Dictionary<string, StateMachine>();
    private Queue<StateMachine> _uninitializedStateMachines = new Queue<StateMachine>();

    public void AddStateMachine(string name)
    {
      var machine = new StateMachine();
      _machineMap.Add(name, machine);
      _uninitializedStateMachines.Enqueue(machine);
    }

    public void RemoveStateMachine(string name)
    {
      var machine = _machineMap[name];
      machine.End();
      _machineMap.Remove(name);
    }

    public void AddState(string machineName, string stateName, Action onStart, Action onTick, Action onEnd, bool isInitialState)
    {
      var machine = _machineMap[machineName];
      machine.AddState(stateName, onStart, onTick, onEnd, isInitialState);
    }

    public void AddTransition(string machineName, string fromState, Func<bool> condition, string toState)
    {
      var machine = _machineMap[machineName];
      machine.AddTransition(fromState, condition, toState);
    }

    private void Update()
    {
      while (_uninitializedStateMachines.Count > 0)
      {
        _uninitializedStateMachines.Dequeue().Begin();
      }

      for (var enumerator = _machineMap.GetEnumerator(); enumerator.MoveNext();)
      {
        enumerator.Current.Value.TickState();
      }
    }
  }
}
