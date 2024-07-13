using System;

namespace BriLib
{
    public class FilteredCollection<T> : ObservableCollection<T>
    {
        private Func<T, bool> _filter;
        private ObservableCollection<T> _observed;

        public FilteredCollection(ObservableCollection<T> coll, Func<T, bool> filter)
        {
            _filter = filter;
            _observed = coll;
            _observed.ForEach((entry) => { if (_filter(entry)) { Add(entry); } });

            _observed.OnAdded += OnObserverAdded;
            _observed.OnRemoved += OnObserverRemoved;
            _observed.OnCleared += OnObserverCleared;
            _observed.OnReplaced += OnObserverReplaced;
        }

        public override void Dispose()
        {
            base.Dispose();

            _observed.OnAdded -= OnObserverAdded;
            _observed.OnRemoved -= OnObserverRemoved;
            _observed.OnCleared -= OnObserverCleared;
            _observed.OnReplaced -= OnObserverReplaced;

            _filter = null;
            _observed = null;
        }

        private void OnObserverAdded(int arg1, T arg2)
        {            
            if (_filter(arg2)) { Insert(GetFilteredIndex(arg1, arg2), arg2); }
        }

        private void OnObserverRemoved(int arg1, T arg2)
        {
            if (Contains(arg2)) { Remove(arg2); }
        }

        private void OnObserverCleared()
        {
            Clear();
        }

        private void OnObserverReplaced(int arg1, T arg2, T arg3)
        {
            var contained = Contains(arg2);
            var keepNew = _filter(arg3);

            if (contained && keepNew)
            {
                var index = IndexOf(arg2);
                this[index] = arg3;
            }
            else if (contained)
            {
                Remove(arg2);
            }
            else if (keepNew)
            {                
                Insert(GetFilteredIndex(arg1, arg3), arg3);
            }
        }

        private int GetFilteredIndex(int underlyingIndex, T obj)
        {
            var newIndex = 0;
            for (int i = 0; i < underlyingIndex; i++)
            {
                if (Contains(_observed[i])) { newIndex++; }
            }
            return newIndex;
        }
    }
}
