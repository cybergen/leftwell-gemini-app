using System;

namespace BriLib
{
  internal class State
  {
    private Action _doOnTick;
    private Action _doOnStart;
    private Action _doOnEnd;

    public State(Action onStart, Action onTick, Action onEnd)
    {
      _doOnStart = onStart;
      _doOnTick = onTick;
      _doOnEnd = onEnd;
    }

    public void Start()
    {
      _doOnStart.Execute();
    }

    public void End()
    {
      _doOnEnd.Execute();
    }

    public void OnUpdate()
    {
      _doOnTick.Execute();
    }

    public void Destroy()
    {
      _doOnTick = null;
      _doOnStart = null;
      _doOnEnd = null;
    }
  }
}
