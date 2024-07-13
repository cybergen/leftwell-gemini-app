using System.Collections.Generic;

namespace BriLib.Delaunay
{
  public static class DelaunayExtensions
  {
    public static bool ContainsAny<T>(this IList<T> startList, IEnumerable<T> other)
    {
      for (var enumerator = other.GetEnumerator(); enumerator.MoveNext();)
      {
        if (startList.Contains(enumerator.Current)) return true;
      }
      return false;
    }

    public static bool AddIfNotContains<T>(this IList<T> startList, T obj)
    {
      if (startList.Contains(obj)) return false;
      startList.Add(obj);
      return true;
    }

    public static void RemoveAll<T>(this IList<T> list, IList<T> removeList)
    {
      for (var enumerator = removeList.GetEnumerator(); enumerator.MoveNext();)
      {
        if (list.Contains(enumerator.Current)) list.Remove(enumerator.Current);
      }
    }

    public static void AddAll<T>(this IList<T> list, IList<T> addList)
    {
      for (var enumerator = addList.GetEnumerator(); enumerator.MoveNext();)
      {
        list.AddIfNotContains(enumerator.Current);
      }
    }

    public static bool ListsEqual<T>(this IList<T> list, IList<T> otherList)
    {
      if (list.Count != otherList.Count) return false;
      for (int i = 0; i < list.Count; i++)
      {
        if (!otherList.Contains(list[i])) return false;
      }
      return true;
    }

    public static void AddUniques<T>(this IList<T> list, IList<T> others)
    {
      for (int i = 0; i < others.Count; i++)
      {
        list.AddIfNotContains(others[i]);
      }
    }
  }
}
