using System;
using System.Collections.ObjectModel;

namespace BriLib
{
    public class ObservableCollection<T> : Collection<T>, IObservableCollection<T>
    {
        public Action<int, T> OnAdded { get; set; }
        public Action<int, T> OnRemoved { get; set; }
        public Action<int, T, T> OnReplaced { get; set; }
        public Action OnCleared { get; set; }
        public Action OnChanged { get; set; }

        public Action<int, object> OnAddedNonGeneric { get; set; }
        public Action<int, object> OnRemovedNonGeneric { get; set; }
        public Action<int, object, object> OnReplacedNonGeneric { get; set; }        

        public IObservableCollection FilterNonGeneric(Func<object, bool> filter)
        {
            return new FilteredCollection<T>(this, (entry) => { return filter(entry); });
        }

        public IObservableCollection<T> Filter(Func<T, bool> filter)
        {
            return new FilteredCollection<T>(this, filter);
        }

        public IObservableCollection Map(Func<object, object> mapper)
        {
            return new MappedCollection<object, T>(this, (genericEntry) => { return mapper(genericEntry); });
        }

        public IObservableCollection<L> Map<L>(Func<T, L> mapper)
        {
            return new MappedCollection<L, T>(this, mapper);
        }

        public T1 ReduceNonGeneric<T1>(T1 seed, Func<object, T1, T1> reducer)
        {
            this.ForEach((entry) => { seed = reducer(entry, seed); });
            return seed;
        }

        public K Reduce<K>(K seed, Func<T, K, K> reducer)
        {
            this.ForEach((entry) => { seed = reducer(entry, seed); });
            return seed;
        }

        public IObservableCollection SortNonGeneric(Func<object, object, int> comparer)
        {
            return new SortedCollection<T>(this, (entry, entry2) => { return comparer(entry, entry2); });
        }

        public IObservableCollection<T> Sort(Func<T, T, int> comparer)
        {
            return new SortedCollection<T>(this, comparer);
        }

        public IObservableCollection Union(IObservableCollection other)
        {
            if (!(other is IObservableCollection<T>))
            {
                var msg = "Expected union with object of type IObservableCollection<" + typeof(T) + ">";
                throw new InvalidCastException(msg);
            }
            return new UnionCollection<T>(this, other as IObservableCollection<T>);
        }

        public IObservableCollection<T> Union(IObservableCollection<T> other)
        {
            return new UnionCollection<T>(this, other);
        }

        public virtual void Dispose()
        {
            OnAdded = null;
            OnRemoved = null;
            OnCleared = null;
            OnReplaced = null;
            OnChanged = null;
            Clear();
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            if (OnAdded != null) { OnAdded(index, item); }
            if (OnAddedNonGeneric != null) { OnAddedNonGeneric(index, item); }
            if (OnChanged != null) { OnChanged(); }
        }

        protected override void RemoveItem(int index)
        {
            var removedItem = this[index];
            base.RemoveItem(index);
            if (OnRemoved != null) { OnRemoved(index, removedItem); }
            if (OnRemovedNonGeneric != null) { OnRemovedNonGeneric(index, removedItem); }
            if (OnChanged != null) { OnChanged(); }
        }

        protected override void SetItem(int index, T item)
        {
            var oldItem = this[index];
            base.SetItem(index, item);
            if (OnReplaced != null) { OnReplaced(index, oldItem, item); }
            if (OnReplacedNonGeneric != null) { OnReplacedNonGeneric(index, oldItem, item); }
            if (OnChanged != null) { OnChanged(); }
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            OnCleared.Execute();
            if (OnChanged != null) { OnChanged(); }
        }
    }
}
