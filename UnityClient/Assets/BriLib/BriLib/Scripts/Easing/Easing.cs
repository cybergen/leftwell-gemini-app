using System;
using UnityEngine;

namespace BriLib
{
  public static class Easing
  {
    public enum Method
    {
      None = 0,
      ExpoOut = 1,
      ExpoIn = 2,
      ExpoInOut = 3,
      ExpoOutIn = 4,
      QuadOut = 5,
      QuadIn = 6,
      QuadInOut = 7,
      QuadOutIn = 8,
      SineOut = 9,
      SineIn = 10,
      SineInOut = 11,
      SineOutIn = 12,
      ElasticOut = 13,
      ElasticIn = 14,
      ElasticInOut = 15,
      ElasticOutIn = 16,
      BounceOut = 17,
      BounceIn = 18,
      BounceInOut = 19,
      BounceOutIn = 20,
      Linear = 21
    }

    public static float Ease(float startVal, float endVal, float startTime, float endTime, float currentTime, Method type)
    {
      var duration = endTime - startTime;
      var time = (currentTime - startTime) / duration;
      float mult = 0f;
      switch (type)
      {
        case Method.ExpoOut: mult = ExpoEaseOut(time); break;
        case Method.ExpoIn: mult = ExpoEaseIn(time); break;
        case Method.ExpoInOut: mult = TwoPartEquation(time, ExpoEaseIn, ExpoEaseOut); break;
        case Method.ExpoOutIn: mult = TwoPartEquation(time, ExpoEaseOut, ExpoEaseIn); break;
        case Method.QuadOut: mult = QuadEaseOut(time); break;
        case Method.QuadIn: mult = QuadEaseIn(time); break;
        case Method.QuadInOut: mult = TwoPartEquation(time, QuadEaseIn, QuadEaseOut); break;
        case Method.QuadOutIn: mult = TwoPartEquation(time, QuadEaseOut, QuadEaseIn); break;
        case Method.SineOut: mult = SineEaseOut(time); break;
        case Method.SineIn: mult = SineEaseIn(time); break;
        case Method.SineInOut: mult = TwoPartEquation(time, SineEaseIn, SineEaseOut); break;
        case Method.SineOutIn: mult = TwoPartEquation(time, SineEaseOut, SineEaseIn); break;
        case Method.ElasticOut: mult = ElasticEaseOut(time); break;
        case Method.ElasticIn: mult = ElasticEaseIn(time); break;
        case Method.ElasticInOut: mult = TwoPartEquation(time, ElasticEaseIn, ElasticEaseOut); break;
        case Method.ElasticOutIn: mult = TwoPartEquation(time, ElasticEaseOut, ElasticEaseIn); break;
        case Method.BounceOut: mult = BounceEaseOut(time); break;
        case Method.BounceIn: mult = BounceEaseIn(time); break;
        case Method.BounceInOut: mult = TwoPartEquation(time, BounceEaseIn, BounceEaseOut); break;
        case Method.BounceOutIn: mult = TwoPartEquation(time, BounceEaseOut, BounceEaseIn); break;
        case Method.Linear: mult = time; break;
      }
      if (currentTime >= endTime) return endVal;
      return (endVal - startVal) * mult + startVal;
    }

    public static Vector2 Ease(Vector2 start, Vector2 end, float startTime, float endTime, float current, Method type)
    {
      var x = Ease(start.x, end.x, startTime, endTime, current, type);
      var y = Ease(start.y, end.y, startTime, endTime, current, type);
      return new Vector2(x, y);
    }

    public static Vector3 Ease(Vector3 start, Vector3 end, float startTime, float endTime, float current, Method type)
    {
      var x = Ease(start.x, end.x, startTime, endTime, current, type);
      var y = Ease(start.y, end.y, startTime, endTime, current, type);
      var z = Ease(start.z, end.z, startTime, endTime, current, type);
      return new Vector3(x, y, z);
    }

    public static Vector4 Ease(Vector4 start, Vector4 end, float startTime, float endTime, float current, Method type)
    {
      var x = Ease(start.x, end.x, startTime, endTime, current, type);
      var y = Ease(start.y, end.y, startTime, endTime, current, type);
      var z = Ease(start.z, end.z, startTime, endTime, current, type);
      var w = Ease(start.w, end.w, startTime, endTime, current, type);
      return new Vector4(x, y, z, w);
    }

    public static Matrix4x4 Ease(Matrix4x4 start, Matrix4x4 end, float startTime, float endTime, float current, Method type)
    {
      var m00 = Ease(start.m00, end.m00, startTime, endTime, current, type);
      var m01 = Ease(start.m01, end.m01, startTime, endTime, current, type);
      var m02 = Ease(start.m02, end.m02, startTime, endTime, current, type);
      var m03 = Ease(start.m03, end.m03, startTime, endTime, current, type);
      var m10 = Ease(start.m10, end.m10, startTime, endTime, current, type);
      var m11 = Ease(start.m11, end.m11, startTime, endTime, current, type);
      var m12 = Ease(start.m12, end.m12, startTime, endTime, current, type);
      var m13 = Ease(start.m13, end.m13, startTime, endTime, current, type);
      var m20 = Ease(start.m20, end.m20, startTime, endTime, current, type);
      var m21 = Ease(start.m21, end.m21, startTime, endTime, current, type);
      var m22 = Ease(start.m22, end.m22, startTime, endTime, current, type);
      var m23 = Ease(start.m23, end.m23, startTime, endTime, current, type);
      var m30 = Ease(start.m30, end.m30, startTime, endTime, current, type);
      var m31 = Ease(start.m31, end.m31, startTime, endTime, current, type);
      var m32 = Ease(start.m32, end.m32, startTime, endTime, current, type);
      var m33 = Ease(start.m33, end.m33, startTime, endTime, current, type);

      var matrix = new Matrix4x4();
      matrix.m00 = m00;
      matrix.m01 = m01;
      matrix.m02 = m02;
      matrix.m03 = m03;
      matrix.m10 = m10;
      matrix.m11 = m11;
      matrix.m12 = m12;
      matrix.m13 = m13;
      matrix.m20 = m20;
      matrix.m21 = m21;
      matrix.m22 = m22;
      matrix.m23 = m23;
      matrix.m30 = m30;
      matrix.m31 = m31;
      matrix.m32 = m32;
      matrix.m33 = m33;
      return matrix;
    }

    public static float ExpoEaseOut(float time)
    {
      return 1f - (float)Math.Pow(2, -10 * time);
    }

    public static float ExpoEaseIn(float time)
    {
      return (float)Math.Pow(2, 10 * (time - 1));
    }

    public static float QuadEaseOut(float time)
    {
      return -time * (time - 2f);
    }

    public static float QuadEaseIn(float time)
    {
      return time.Sq();
    }

    public static float SineEaseOut(float time)
    {
      return (float)Math.Sin(time * (Math.PI / 2));
    }

    public static float SineEaseIn(float time)
    {
      return 1 - (float)Math.Cos(time * (Math.PI / 2));
    }

    public static float ElasticEaseOut(float time)
    {
      float p = .3f;
      float s = p / 4f;
      return 1f + ((float)Math.Pow(2, -10 * time) * (float)Math.Sin((time - s) * (2 * Math.PI) / p));
    }

    public static float ElasticEaseIn(float time)
    {
      float p = .3f;
      float s = p / 4f;
      return -(float)(Math.Pow(2, 10 * (time -= 1)) * Math.Sin((time - s) * (2 * Math.PI) / p));
    }

    public static float BounceEaseOut(float time)
    {
      if (time < (1 / 2.75)) return (7.5625f * time.Sq());
      else if (time < (2 / 2.75)) return 7.5625f * (time -= (1.5f / 2.75f)) * time + .75f;
      else if (time < (2.5 / 2.75)) return 7.5625f * (time -= (2.25f / 2.75f)) * time + .9375f;
      else return 7.5625f * (time -= (2.625f / 2.75f)) * time + .984375f;
    }

    public static float BounceEaseIn(float time)
    {
      return 1 - BounceEaseOut(1f - time);
    }

    public static float EaseInOutBack(float time)
    {
      float c1 = 1.70158f;
      float c2 = c1 * 1.525f;

      return time < 0.5
        ? (Mathf.Pow(2 * time, 2) * ((c2 + 1) * 2 * time - c2)) / 2
        : (Mathf.Pow(2 * time - 2, 2) * ((c2 + 1) * (time * 2 - 2) + c2) + 2) / 2;
    }

    private static float TwoPartEquation(float time, Func<float, float> partOne, Func<float, float> partTwo)
    {
      if (time > 0.5f) return (partOne(1f) + partTwo((time - 0.5f) / 0.5f)) / 2f;
      return partOne(time / 0.5f) / 2f;
    }
  }
}
