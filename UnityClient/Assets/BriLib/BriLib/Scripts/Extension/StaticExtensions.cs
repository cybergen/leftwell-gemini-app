using System;
using System.Collections;
using System.Collections.Generic;

namespace BriLib
{
  public static class StaticExtensions
  {
    public static void ForEach(this IEnumerable list, Action<object> action)
    {
      for (var enumerator = list.GetEnumerator(); enumerator.MoveNext();)
      {
        action.Execute(enumerator.Current);
      }
    }

    public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
    {
      for (var enumerator = list.GetEnumerator(); enumerator.MoveNext();)
      {
        action.Execute(enumerator.Current);
      }
    }

    public static void Execute(this Action action)
    {
      if (action != null) { action(); }
    }

    public static void Execute<T>(this Action<T> action, T obj)
    {
      if (action != null) { action(obj); }
    }

    public static void Execute<T, K>(this Action<T, K> action, T objOne, K objTwo)
    {
      if (action != null) { action(objOne, objTwo); }
    }

    public static void Execute<T, K, L>(this Action<T, K, L> action, T objOne, K objTwo, L objThree)
    {
      if (action != null) { action(objOne, objTwo, objThree); }
    }

    public static void Execute<T, K, L, M>(this Action<T, K, L, M> action, T objOne, K objTwo, L objThree, M objFour)
    {
      if (action != null) { action(objOne, objTwo, objThree, objFour); }
    }

    public static float Sq(this float value)
    {
      return value * value;
    }

    public static double Sq(this double value)
    {
      return value * value;
    }

    public static float Sqrt(this float value)
    {
      return (float)Math.Sqrt(value);
    }

    public static double Sqrt(this double value)
    {
      return Math.Sqrt(value);
    }

    public static float MapRange(this float currValue, float fromStart, float toStart, float fromEnd, float toEnd)
    {
      return (currValue - fromStart) / (toStart - fromStart) * (toEnd - fromEnd) + fromEnd;
    }

    public static K GetIfNotNull<T, K>(this Dictionary<T, K> dict, T key) where K : class
    {
      if (dict.ContainsKey(key)) return dict[key];
      return null;
    }

    public static bool IsValid(this UnityEngine.Vector3 vector)
    {
        return !float.IsNaN(vector.x) && !float.IsNaN(vector.y) && !float.IsNaN(vector.z) &&
          !float.IsInfinity(vector.x) && !float.IsInfinity(vector.y) && !float.IsInfinity(vector.z);
    }
  }
}