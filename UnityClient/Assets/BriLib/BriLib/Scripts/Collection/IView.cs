using UnityEngine;

namespace BriLib
{
  public interface IView
  {
    GameObject GameObject { get; }
    object Data { get; }
    void ApplyData(object data);
  }
}
