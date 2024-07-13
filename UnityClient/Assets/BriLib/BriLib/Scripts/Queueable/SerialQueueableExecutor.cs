using System;
using System.Collections.Generic;

namespace BriLib
{
    public class SerialQueueableExecutor : IQueueable
    {
        public Action<IQueueable> OnBegan { get; set; }
        public Action<IQueueable> OnEnded { get; set; }
        public Action<IQueueable> OnKilled { get; set; }

        private Queue<IQueueable> _queue = new Queue<IQueueable>();

        public SerialQueueableExecutor(IEnumerable<IQueueable> queue)
        {
            foreach (var entry in queue) { Queue(entry); }
        }

        public SerialQueueableExecutor(IQueueable[] queue)
        {
            foreach (var entry in queue) { Queue(entry); }
        }

        public SerialQueueableExecutor(IQueueable entry)
        {
            Queue(entry);
        }

        public SerialQueueableExecutor() { }

        public void Queue(IQueueable queueable)
        {
            _queue.Enqueue(queueable);
        }

        private IQueueable _current;

        public void Begin()
        {
            if (_queue.Count > 0)
            {
                _current = _queue.Dequeue();
                _current.OnEnded += OnCurrentEnded;
                _current.OnKilled += OnCurrentKilled;
                _current.Begin();
            }
            OnBegan.Execute(this);
        }

        public void Kill()
        {
            if (_current != null)
            {
                _current.OnEnded -= OnCurrentEnded;
                _current.OnKilled -= OnCurrentKilled;
                _current.Kill();
                _current = null;
            }
            OnKilled.Execute(this);
        }

        private void OnCurrentEnded(IQueueable obj)
        {
            _current.OnKilled -= OnCurrentKilled;
            _current.OnEnded -= OnCurrentEnded;

            if (_queue.Count > 0)
            {
                _current = _queue.Dequeue();
                _current.OnEnded += OnCurrentEnded;
                _current.OnKilled += OnCurrentKilled;
                _current.Begin();
            }
            else
            {
                OnEnded.Execute(this);
            }
        }

        private void OnCurrentKilled(IQueueable obj)
        {
            _current.OnKilled -= OnCurrentKilled;
            _current.OnEnded -= OnCurrentEnded;
            OnKilled.Execute(this);
        }
    }
}
