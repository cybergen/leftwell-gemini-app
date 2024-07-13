using System.Collections.Generic;
using UnityEngine;

namespace BriLib
{
  public class TemplateDuplicator : MonoBehaviour
  {
#pragma warning disable CS0649
    [SerializeField] private GameObject _listEntryTemplate;
    [SerializeField] private Transform _listParent;
#pragma warning restore CS0649
    private Dictionary<object, IView> _viewMap = new Dictionary<object, IView>();
    private IObservableCollection _collection;
    private bool _bound;

    public void BindOnCollection(IObservableCollection collection)
    {
      if (_bound)
      {
        Debug.LogWarning("Attempting to bind on collection when already bound");
        return;
      }

      _collection = collection;
      SpawnEntries();
      _collection.OnAddedNonGeneric += OnAdded;
      _collection.OnRemovedNonGeneric += OnRemoved;
      _collection.OnReplacedNonGeneric += OnReplaced;
      _bound = true;
    }

    public void UnbindOnCollection()
    {
      if (!_bound) return;

      _collection.OnAddedNonGeneric -= OnAdded;
      _collection.OnRemovedNonGeneric -= OnRemoved;
      _collection.OnReplacedNonGeneric -= OnReplaced;
      CleanupEntries();
      _bound = false;
    }

    public IView GetViewForObject(object obj)
    {
      return _viewMap[obj];
    }

    private void SpawnEntries()
    {
      for (int i = 0; i < _collection.Count; i++)
      {
        SpawnView(_collection[i]);
      }
    }

    private void CleanupEntries()
    {
      for (var enumerator = _viewMap.GetEnumerator(); enumerator.MoveNext();)
      {
        GameObject.Destroy(enumerator.Current.Value.GameObject);
      }
      _viewMap.Clear();
    }

    private IView SpawnView(object data)
    {
      var go = GameObject.Instantiate(_listEntryTemplate, _listParent);
      var view = go.GetComponent<IView>();
      view.ApplyData(data);
      _viewMap.Add(data, view);
      return view;
    }

    private void OnAdded(int arg1, object arg2)
    {
      var view = SpawnView(arg2);
      view.GameObject.transform.SetSiblingIndex(arg1);
    }

    private void OnRemoved(int arg1, object arg2)
    {
      var view = _viewMap[arg2];
      var data = view.Data;
      GameObject.Destroy(view.GameObject);
      _viewMap.Remove(data);
    }

    private void OnReplaced(int arg1, object oldItem, object newItem)
    {
      var view = _viewMap[oldItem];
      view.ApplyData(newItem);
      _viewMap.Remove(oldItem);
      _viewMap.Add(newItem, view);
    }
  }
}
