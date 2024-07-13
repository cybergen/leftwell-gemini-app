using System;

namespace BriLib
{
    public class ObservableProperty<T> : IObservable<T>
    {
        public Action<IObservable> OnChanged { get; set; }

        private T _value;
        public T Value 
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                OnChanged.Execute(this);
            }
        }

        object IObservable.Value
        {
            get
            {
                return Value;
            }

            set
            {
               Value = (T)value;
            }
        }

        public ObservableProperty(T value)
        {
            Value = value;
        }

        public ObservableProperty() { }
    }
}
