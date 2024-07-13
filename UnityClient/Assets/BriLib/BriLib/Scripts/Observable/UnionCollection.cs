namespace BriLib
{
    public class UnionCollection<T> : ObservableCollection<T>
    {
        private ObservableCollection<T> _left;
        private ObservableCollection<T> _right;

        public UnionCollection(IObservableCollection<T> left, IObservableCollection<T> right)
        {
            _left = left as ObservableCollection<T>;
            _right = right as ObservableCollection<T>;

            _left.ForEach((entry) => { Add(entry); });
            _right.ForEach((entry) => { Add(entry); });

            _left.OnAdded += OnLeftAdded;
            _left.OnRemoved += OnLeftRemoved;
            _left.OnReplaced += OnLeftReplaced;
            _left.OnCleared += OnLeftCleared;

            _right.OnAdded += OnRightAdded;
            _right.OnRemoved += OnRightRemoved;
            _right.OnReplaced += OnRightReplace;
            _right.OnCleared += OnRightCleared;
        }

        public override void Dispose()
        {
            base.Dispose();

            _left.OnAdded -= OnLeftAdded;
            _left.OnRemoved -= OnLeftRemoved;
            _left.OnReplaced -= OnLeftReplaced;
            _left.OnCleared -= OnLeftCleared;

            _right.OnAdded -= OnRightAdded;
            _right.OnRemoved -= OnRightRemoved;
            _right.OnReplaced -= OnRightReplace;
            _right.OnCleared -= OnRightCleared;

            _left = null;
            _right = null;
        }

        private void OnLeftAdded(int arg1, T arg2)
        {
            Insert(arg1, arg2);
        }

        private void OnLeftRemoved(int arg1, T arg2)
        {
            RemoveAt(arg1);
        }

        private void OnLeftReplaced(int arg1, T arg2, T arg3)
        {
            this[arg1] = arg3;
        }

        private void OnLeftCleared()
        {
            Clear();
            _right.ForEach((entry) => { Add(entry); });
        }

        private void OnRightAdded(int arg1, T arg2)
        {
            Insert(arg1 + _left.Count, arg2);
        }

        private void OnRightRemoved(int arg1, T arg2)
        {
            RemoveAt(arg1 + _left.Count);
        }

        private void OnRightReplace(int arg1, T arg2, T arg3)
        {
            this[arg1 + _left.Count] = arg3;
        }

        private void OnRightCleared()
        {
            Clear();
            _left.ForEach((entry) => { Add(entry); });
        }
    }
}
