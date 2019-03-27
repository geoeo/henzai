using System;
using System.Numerics;

namespace Henzai.Core.Extensions
{
    public static class TypeExtensions
    {
        public static uint ToUnsigned(this int val)
        {
            if (val < 0)
                throw new ArgumentException("Cannot cast a negative interger value");

            return (uint)val;

        }

        public static ushort ToUnsignedShort(this int val)
        {
            if (val < 0)
                throw new ArgumentException("Cannot cast a negative interger value");
            if (val > ushort.MaxValue)
                throw new ArgumentException("Cannot cast to ushort: Value too large");

            return (ushort)val;

        }

        public static double ToDouble(this float val)
        {
            return val;
        }

        public static float ToFloat(this int val)
        {
            return val;
        }

        public static float ToFloat(this double val)
        {
            return (float)val;
        }

        public static Vector3 ToVec3DiscardW(this Vector4 val)
        {
            return new Vector3(val.X, val.Y, val.Z);
        }

        public static Vector3 ToVec3NormalizeW(this Vector4 val)
        {
            return new Vector3(val.X / val.W, val.Y / val.W, val.Z / val.W);
        }
    }
}
