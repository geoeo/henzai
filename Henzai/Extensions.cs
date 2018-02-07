using System;
using System.Collections.Generic;

namespace Henzai.Extensions
{
    public static class MathExtensions
    {
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>{
            return (val.CompareTo(min) < 0) ? min : ((val.CompareTo(max) > 0) ? max : val);
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
    }
}