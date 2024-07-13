using System;
using System.Collections.Generic;

namespace BriLib
{
    public class ParallelQueueableExecutor : IQueueable
    {
        public Action<IQueueable> OnBegan { get; set; }
        public Action<IQueueable> OnEnded { get; set; }
        public Action<IQueueable> OnKilled { get; set; }

        private List<IQueueable> _notInProgress = new List<IQueueable>();
        private List<IQueueable> _inProgress = new List<IQueueable>();

        public ParallelQueueableExecutor(IEnumerable<IQueueable> list)
        {
            list.ForEach(entry => AddQueueable(entry));
        }

        public ParallelQueueableExecutor(IQueueable[] list)
        {
            list.ForEach(entry => AddQueueable(entry));
        }

        public ParallelQueueableExecutor(IQueueable single)
        {
            AddQueueable(single);
        }

        public ParallelQueueableExecutor() { }

        public void Begin()
        {
            _notInProgress.ForEach((entry) =>
            {
                entry.OnEnded += OnEntryEnded;
                entry.OnKilled += OnEntryKilled;
                entry.Begin();
                _inProgress.Add(entry);
            });
            _notInProgress.Clear();
            OnBegan.Execute(this);
        }

        public void Kill()
        {
            _inProgress.ForEach((entry) =>
            {
                entry.OnEnded -= OnEntryEnded;
                entry.OnKilled -= OnEntryKilled;
                entry.Kill();
            });
            _inProgress.Clear();
            OnKilled.Execute(this);
        }

        public void AddQueueable(IQueueable queueable)
        {
            _notInProgress.Add(queueable);
        }

        private void OnEntryEnded(IQueueable obj)
        {
            obj.OnEnded -= OnEntryEnded;
            obj.OnKilled -= OnEntryKilled;
            _inProgress.Remove(obj);
            if (_inProgress.Count == 0) { OnEnded.Execute(this); }
        }

        private void OnEntryKilled(IQueueable obj)
        {
            obj.OnEnded -= OnEntryEnded;
            obj.OnKilled -= OnEntryKilled;
            _inProgress.Remove(obj);
            Kill();
        }
    }
}
