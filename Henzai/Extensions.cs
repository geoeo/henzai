using System;
using System.Numerics;
using System.Collections.Generic;

namespace Henzai.Extensions
{
    public static class MathExtensions
    {
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>{
            return (val.CompareTo(min) < 0) ? min : ((val.CompareTo(max) > 0) ? max : val);
        }

        public static Matrix4x4 Invert(this Matrix4x4 val){
            Matrix4x4 inverted;
            bool success =  Matrix4x4.Invert(val,out inverted);
            return success? inverted : Matrix4x4.Identity;
        }

        public static Matrix4x4 Transpose(this Matrix4x4 val){
            return Matrix4x4.Transpose(val);
        }

        public static Vector4 ToHomogeneous(this Vector3 val){
            return new Vector4(val, 1.0f);
        }

        public static Vector4 ToDirection(this Vector3 val){
            return new Vector4(val, 0.0f);
        }
    }

    public static class GenericExtensions
    {
        public static uint LengthUnsigned<T>(this T[] val){
            return (uint)val.Length;
        }
    }

    public static class TypeExtensions
    {
        public static uint ToUnsigned(this int val){
            if(val < 0)
                throw new ArgumentException("Cannot cast a negative interger value");

            return (uint)val;

        }

        public static double ToDouble(this float val){
            return (double)val;
        }

        public static float ToFloat(this int val){
            return (float)val;
        }

        public static float ToFloat(this double val){
            return (float)val;
        }

        public static Vector3 ToVec3DiscardW(this Vector4 val){
            return new Vector3(val.X,val.Y,val.Z);
        }

        public static Vector3 ToVec3NormalizeW(this Vector4 val){
            return new Vector3(val.X/val.W,val.Y/val.W,val.Z/val.W);
        }
    }

}