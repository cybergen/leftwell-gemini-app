using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BriLib
{
    public class SortedCollection<T> : ObservableCollection<T>
    {
        private Func<T, T, int> _comparer;
        private ObservableCollection<T> _observed;

        public SortedCollection(ObservableCollection<T> observed, Func<T, T, int> comparer)
        {
            _observed = observed;
            _comparer = comparer;

            _observed.ForEach((entry) =>
            {
                Insert(GetIndexForObject(entry), entry);
            });

            _observed.OnAdded += OnObserverAdded;
            _observed.OnRemoved += OnObserverRemoved;
            _observed.OnReplaced += OnObserverReplaced;
            _observed.OnCleared += OnObserverCleared;
        }

        public void ReSort()
        {
            var list = new List<T>();
            for (int i = Count - 1; i >= 0; i--)
            {
                list.Add(this[i]);
                RemoveAt(i);
            }
            list.ForEach((entry) => {
                Insert(GetIndexForObject(entry), entry);
            });
        }

        public override void Dispose()
        {
            base.Dispose();

            _observed.OnAdded -= OnObserverAdded;
            _observed.OnRemoved -= OnObserverRemoved;
            _observed.OnCleared -= OnObserverCleared;
            _observed.OnReplaced -= OnObserverReplaced;

            _comparer = null;
            _observed = null;
        }

        private void OnObserverAdded(int arg1, T arg2)
        {
            Insert(GetIndexForObject(arg2), arg2);
        }

        private void OnObserverRemoved(int arg1, T arg2)
        {
            Remove(arg2);
        }

        private void OnObserverCleared()
        {
            Clear();
        }

        private void OnObserverReplaced(int arg1, T arg2, T arg3)
        {
            Remove(arg2);
            Insert(GetIndexForObject(arg3), arg3);
        }

        private int GetIndexForObject(T obj)
        {
            if (Count == 0) { return 0; }

            for (int i = 0; i < Count; i++)
            {
                if (_comparer(obj, this[i]) < 0) { return i; }
            }

            return Count;
        }
    }
}
