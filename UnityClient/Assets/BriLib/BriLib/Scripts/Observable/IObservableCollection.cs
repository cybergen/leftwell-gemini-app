using System;
using System.Collections;
using System.Collections.Generic;

namespace BriLib
{
    public interface IObservableCollection : IList, IDisposable
    {
        Action<int, object> OnAddedNonGeneric { get; set; }
        Action<int, object> OnRemovedNonGeneric { get; set; }
        Action<int, object, object> OnReplacedNonGeneric { get; set; }
        Action OnCleared { get; set; }
        Action OnChanged { get; set; }

        IObservableCollection Map(Func<object, object> mapper);
        IObservableCollection FilterNonGeneric(Func<object, bool> filter);
        T ReduceNonGeneric<T>(T seed, Func<object, T, T> reducer);
        IObservableCollection Union(IObservableCollection other);
        IObservableCollection SortNonGeneric(Func<object, object, int> comparer);
    }

    public interface IObservableCollection<T> : IList<T>, IObservableCollection
    {
        Action<int, T> OnAdded { get; set; }
        Action<int, T> OnRemoved { get; set; }
        Action<int, T, T> OnReplaced { get; set; }

        IObservableCollection<L> Map<L>(Func<T, L> mapper);
        IObservableCollection<T> Filter(Func<T, bool> filter);
        K Reduce<K>(K seed, Func<T, K, K> reducer);
        IObservableCollection<T> Union(IObservableCollection<T> other);
        IObservableCollection<T> Sort(Func<T, T, int> comparer);
    }
}
