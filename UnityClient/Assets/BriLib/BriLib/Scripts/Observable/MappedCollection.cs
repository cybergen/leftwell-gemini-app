using System;

namespace BriLib
{
    public class MappedCollection<T, K> : ObservableCollection<T>
    {
        private Func<K, T> _mapper;
        private ObservableCollection<K> _observed;

        public MappedCollection(ObservableCollection<K> observed, Func<K, T> mapper)
        {
            _mapper = mapper;
            _observed = observed;
            _observed.ForEach((entry) => { Add(_mapper(entry)); });

            _observed.OnAdded += OnObservedAdded;
            _observed.OnRemoved += OnObserverRemoved;
            _observed.OnReplaced += OnObserverReplaced;
            _observed.OnCleared += OnObserverCleared;
        }

        public override void Dispose()
        {
            base.Dispose();

            _observed.OnAdded -= OnObservedAdded;
            _observed.OnRemoved -= OnObserverRemoved;
            _observed.OnReplaced -= OnObserverReplaced;
            _observed.OnCleared -= OnObserverCleared;

            _mapper = null;
            _observed = null;
        }

        private void OnObservedAdded(int arg1, K arg2)
        {
            Insert(arg1, _mapper(arg2));
        }

        private void OnObserverRemoved(int arg1, K arg2)
        {
            RemoveAt(arg1);
        }

        private void OnObserverReplaced(int arg1, K arg2, K arg3)
        {
            this[arg1] = _mapper(arg3);
        }

        private void OnObserverCleared()
        {
            Clear();
        }
    }
}
