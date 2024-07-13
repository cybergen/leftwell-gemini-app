namespace BriLib
{
    public struct TwoDimensionalPoint<K>
    {
        public K StoredObject;
        public float X;
        public float Y;

        public TwoDimensionalPoint(float x, float y, K obj)
        {
            X = x;
            Y = y;
            StoredObject = obj;
        }
    }
}
