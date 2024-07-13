using System;
using System.Threading.Tasks;

namespace BriLib
{
  public static class AsyncMethods
  {
    public static async Task DoAfterTime(float seconds, Action queuedAction)
    {
      await DoAfterTime((int)(seconds * 1000), queuedAction);
    }

    public static async Task DoAfterTime(int milliseconds, Action queuedAction)
    {
      await Task.Delay(milliseconds);
      queuedAction.Execute();
    }
  }
}