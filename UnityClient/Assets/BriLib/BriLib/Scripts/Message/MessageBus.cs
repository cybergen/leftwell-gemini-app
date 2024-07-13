using System;
using System.Collections.Generic;

namespace BriLib
{
    public class MessageBus
    {
        private Dictionary<Type, IActionStore> _actionStores = new Dictionary<Type, IActionStore>();

        public void Subscribe<T>(Action<T> subscriber) where T : Message
        {
            var type = typeof(T);
            if (!_actionStores.ContainsKey(type))
            {
                _actionStores.Add(type, new ActionStore<T>());
            }
            (_actionStores[type] as ActionStore<T>).Actions += subscriber;
        }

        public void Unsubscribe<T>(Action<T> subscriber) where T : Message
        {
            var type = typeof(T);
            if (!_actionStores.ContainsKey(type)) { return; }
            (_actionStores[type] as ActionStore<T>).Actions -= subscriber;
        }

        public void Broadcast<T>(T message) where T : Message
        {
            var type = typeof(T);
            if (!_actionStores.ContainsKey(type)) { return; }
            (_actionStores[type] as ActionStore<T>).Actions.Execute<T>(message);
        }
    }

    public class ActionStore<T> : IActionStore where T : Message
    {
        public Action<T> Actions;
    }

    public interface IActionStore { }

    public class Message { }
}
