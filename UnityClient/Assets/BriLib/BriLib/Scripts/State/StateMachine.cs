using System;
using System.Collections.Generic;

namespace BriLib
{
  public class StateMachine
  {
    public string CurrentState { get; private set; }

    private Dictionary<string, State> _stateMap = new Dictionary<string, State>();
    private Dictionary<State, List<Edge>> _edgeMap = new Dictionary<State, List<Edge>>();

    private State _currentState;
    private State _initialState;

    public void AddState(string name, Action doOnStart, Action doOnTick, Action doOnEnd, bool firstState = false)
    {
      var state = new State(doOnStart, doOnTick, doOnEnd);
      _stateMap.Add(name, state);
      _edgeMap.Add(state, new List<Edge>());
      if (firstState) _initialState = state;
    }

    public void AddTransition(string stateName, Func<bool> transitionCondition, string targetState)
    {
      var edge = new Edge(_stateMap[targetState], transitionCondition);
      _edgeMap[_stateMap[stateName]].Add(edge);
    }

    public void TickState()
    {
      if (_currentState != null)
      {
        _currentState.OnUpdate();
      }

      var edges = _edgeMap[_currentState];
      for (int i = 0; i < edges.Count; i++)
      {
        if (edges[i].CheckCondition())
        {
          SetState(edges[i].NextState);
          return;
        }
      }
    }

    public void Begin()
    {
      if (_initialState == null)
      {
        throw new MissingMemberException("No initial state provided to state machine");
      }

      SetState(_initialState);
    }

    public void End()
    {
      for (var enumerator = _stateMap.GetEnumerator(); enumerator.MoveNext();)
      {
        var state = enumerator.Current.Value;
        var edges = _edgeMap[state];
        for (int i = 0; i < edges.Count; i++)
        {
          edges[i].Destroy();
        }
        _edgeMap[state].Clear();
        state.Destroy();
      }
      _edgeMap.Clear();
      _stateMap.Clear();
    }

    private void SetState(State state)
    {
      if (_currentState != null) _currentState.End();
      _currentState = state;
      if (_currentState != null) _currentState.Start();
    }
  }
}
