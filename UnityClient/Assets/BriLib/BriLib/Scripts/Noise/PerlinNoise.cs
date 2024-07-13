using System;

namespace BriLib
{
    public static class PerlinNoise
    {
        public static float Seed = 0.2f;

        public static float Random()
        {
            var time = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            return NoiseOneDimensional((float)time * Seed + Seed);
        }

        public static float Random01()
        {
            return Random().MapRange(-1, 1, 0, 1);
        }

        public static float Random(float input)
        {
            return NoiseOneDimensional(input * Seed + Seed);
        }

        public static float Random01(float input)
        {
            return Random(input).MapRange(-1, 1, 0, 1);
        }

        public static float ScaledRandom(float upperRange)
        {
            return upperRange * Random01();
        }

        public static float ScaledRandom(float upperRange, float input)
        {
            return upperRange * Random01(input);
        }

        public static float RangedRandom(float lowerRange, float upperRange, float input)
        {
            return Random(input).MapRange(-1, 1, lowerRange, upperRange);
        }

        public static float NoiseOneDimensional(float x)
        {
            int intX = x.GetHashCode();
            intX = intX << 13 ^ intX;
            return 1f - ((intX * (intX * intX * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824f;
        }
    }
}
