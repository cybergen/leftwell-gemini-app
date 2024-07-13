using UnityEngine;

namespace BriLib
{
  public class LogManager : Singleton<LogManager>
  {
    public enum LogLevel
    {
      Info = 0,
      Warning = 1,
      Error = 2,
      Silent = 3
    }

    private LogLevel _currentLevel;

    public void SetLogLevel(LogLevel level)
    {
      _currentLevel = level;
    }

    public void Log(string msg, LogLevel level)
    {
      MainThreadQueue.Instance.QueueAction(() =>
      {
        if (level >= _currentLevel)
        {
          switch (level)
          {
            case LogLevel.Info:
              Debug.Log(msg);
              break;
            case LogLevel.Warning:
              Debug.LogWarning(msg);
              break;
            case LogLevel.Error:
              Debug.LogError(msg);
              break;
          }
        }
      });
    }

    public static void Info(string msg)
    {
      Instance.Log(msg, LogLevel.Info);
    }

    public static void Warn(string msg)
    {
      Instance.Log(msg, LogLevel.Warning);
    }

    public static void Error(string msg)
    {
      Instance.Log(msg, LogLevel.Error);
    }
  }
}

