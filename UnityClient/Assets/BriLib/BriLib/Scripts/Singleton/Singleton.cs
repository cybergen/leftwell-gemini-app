using System;
using UnityEngine;

namespace BriLib
{
  /// <summary>
  /// A form of singleton where we expect some prefab or game object to exist in world. If the game object does not yet exist,
  /// we automatically make it. If the game object DOES exist on create, we bail out,
  /// as we should not attempt to create more than one of a singleton
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public abstract class Singleton<T> : MonoBehaviour, ISingleton where T : Component, ISingleton
  {
    public static T Instance => _instance.Value;

    private static readonly Lazy<T> _instance = new Lazy<T>(CreateSingleton);

    private static T CreateSingleton()
    {
      var existingObj = (T)FindObjectOfType(typeof(T));
      if (existingObj != null)
      {
        existingObj.OnCreate();
        existingObj.Begin();
        return existingObj;
      }

      var typeString = typeof(T).ToString();
      var obj = new GameObject(typeString);
      obj.AddComponent<T>();
      obj.name = typeof(T).ToString();

      var instance = obj.GetComponent<T>();
      instance.OnCreate();
      instance.Begin();

      return instance;
    }

    private void Awake()
    {
      if (_instance.IsValueCreated)
      {
        Debug.LogWarning("Attempted to initialize already initialized singleton " + typeof(T));
        //throw new DuplicateInstanceException();
        DestroyImmediate(gameObject);
        return;
      }
      OnCreate();
    }

    private void Start()
    {
      //Ping our instance at least once to ensure that it has initialized
      var temp = Instance;
    }

    public virtual void OnCreate()
    {
      if (Application.isPlaying) DontDestroyOnLoad(gameObject);
    }

    public virtual void Begin() { }

    public virtual void End()
    {
      Destroy(gameObject);
    }
  }
}
