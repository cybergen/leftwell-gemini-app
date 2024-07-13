namespace BriLib
{
    public struct ThreeDimensionalPoint<K>
    {
        public float X;
        public float Y;
        public float Z;
        public K StoredObject;

        public ThreeDimensionalPoint(float x, float y, float z, K obj)
        {
            X = x;
            Y = y;
            Z = z;
            StoredObject = obj;
        }
    }
}
