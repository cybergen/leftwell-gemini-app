using System;

namespace BriLib
{
  internal class Edge
  {
    public State NextState { get; private set; }

    private Func<bool> _transitionCondition;

    public Edge(State nextState, Func<bool> transitionCondition)
    {
      NextState = nextState;
      _transitionCondition = transitionCondition;
    }

    public bool CheckCondition()
    {
      return _transitionCondition != null && _transitionCondition();
    }

    public void Destroy()
    {
      NextState = null;
      _transitionCondition = null;
    }
  }
}
