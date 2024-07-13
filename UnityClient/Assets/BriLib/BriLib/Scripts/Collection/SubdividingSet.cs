using System.Linq;
using System.Collections.Generic;

namespace BriLib
{
    public class SubdividingSet<T>
    {
        public int Count { get { return Entries.Count; } }
        public SubdividingSet<T> Next = null;
        public List<T> Entries = new List<T>();

        private int _maxCount;

        public SubdividingSet(IEnumerable<T> points, int maxCount)
        {
            Entries = points.ToList();
            _maxCount = maxCount;
            Subdivide();
        }

        private void Subdivide()
        {
            if (Count <= _maxCount) return;

            var limit = Count / 2;
            var leftList = Entries.GetRange(0, limit);

            var next = new SubdividingSet<T>(Entries.GetRange(limit, Count - limit), _maxCount);
            if (Next != null)
            {
                var oldNext = Next;
                Next = next;
                Next.Next = oldNext;
            }
            else
            {
                Next = next;
            }
            Entries = leftList;

            Subdivide();
        }
    }
}
