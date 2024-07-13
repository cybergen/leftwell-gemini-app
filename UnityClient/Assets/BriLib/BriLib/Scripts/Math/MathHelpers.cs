using System.Collections.Generic;

namespace BriLib
{
    public static class MathHelpers
    {
        /// <summary>
        /// Get the distance between two vectors
        /// </summary>
        /// <param name="xA"></param>
        /// <param name="yA"></param>
        /// <param name="zA"></param>
        /// <param name="xB"></param>
        /// <param name="yB"></param>
        /// <param name="zB"></param>
        /// <returns></returns>
        public static float Distance(float xA, float yA, float xB, float yB)
        {
            return ((xA - xB).Sq() + (yA - yB).Sq()).Sqrt();
        }

        /// <summary>
        /// Get the distance between two vectors
        /// </summary>
        /// <param name="xA"></param>
        /// <param name="yA"></param>
        /// <param name="zA"></param>
        /// <param name="xB"></param>
        /// <param name="yB"></param>
        /// <param name="zB"></param>
        /// <returns></returns>
        public static float Distance(float xA, float yA, float zA, float xB, float yB, float zB)
        {
            return ((xA - xB).Sq() + (yA - yB).Sq() + (zA - zB).Sq()).Sqrt();
        }

        /// <summary>
        /// Generate a random float value
        /// </summary>
        /// <param name="max"></param>
        /// <param name="rand"></param>
        /// <returns></returns>
        public static float GetRandom(float max, System.Random rand)
        {
            return (float)rand.Next(1000) / 1000f * max;
        }

        /// <summary>
        /// Return a float value from within a given range
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="rand"></param>
        /// <returns></returns>
        public static float GetRandomFromRange(float min, float max, System.Random rand)
        {
            return GetRandom(1f, rand) * (max - min) + min;
        }

        /// <summary>
        /// Generate a random positive/negative value
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
        public static int GetPosNeg(System.Random rand)
        {
            return rand.Next(2) > 0 ? 1 : -1;
        }

        /// <summary>
        /// Return absolute value of a float
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float AbsFloat(float value)
        {
            return value < 0 ? value * -1f : value;
        }

        /// <summary>
        /// Epsilon compare for floats to get equality where precision may be lost
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool FloatCompare(float a, float b)
        {
            return AbsFloat(a - b) < 1E-05F;
        }

        /// <summary>
        /// Less than equals comparison for floats, where precision may impact == evaluation
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool LessThanEqual(float a, float b)
        {
            return a < b || FloatCompare(a, b);
        }

        /// <summary>
        /// Greater than equals comparison for floats, where precision may impact == evaluation
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool GreaterThanEqual(float a, float b)
        {
            return a > b || FloatCompare(a, b);
        }

        /// <summary>
        /// Select one random entry from a list
        /// </summary>
        /// <param name="list"></param>
        /// <param name="rand"></param>
        /// <returns></returns>
        public static T SelectFromRange<T>(List<T> list, System.Random rand)
        {
            return list[rand.Next(list.Count)];
        }
    }
}
