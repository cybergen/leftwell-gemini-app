using System;

namespace BriLib
{
    public interface IObservable
    {
        Action<IObservable> OnChanged { get; set; }
        object Value { get; set; }
    }

    public interface IObservable<T> : IObservable
    {
        new T Value { get; set; }
    }
}
