using System;
using System.Numerics;

namespace Henzai.Core.Extensions
{
    public static class MathExtensions
    {
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            return (val.CompareTo(min) < 0) ? min : ((val.CompareTo(max) > 0) ? max : val);
        }

        public static Matrix4x4 Invert(this Matrix4x4 val)
        {
            Matrix4x4 inverted;
            bool success = Matrix4x4.Invert(val, out inverted);
            return success ? inverted : Matrix4x4.Identity;
        }

        public static Matrix4x4 Transpose(this Matrix4x4 val)
        {
            return Matrix4x4.Transpose(val);
        }

        /// <summary> 
        ///Pass-By-Value. For more performant option see <see cref="Henzai.Core.Numerics"/>
        /// </summary> 
        public static Vector4 ToHomogeneous(this Vector3 val)
        {
            return new Vector4(val, 1.0f);
        }

        /// <summary> 
        ///Pass-By-Value. For more performant option see <see cref="Henzai.Core.Numerics"/>
        /// </summary> 
        public static Vector4 ToDirection(this Vector3 val)
        {
            return new Vector4(val, 0.0f);
        }

        public static int ToInt32AwayFromZero(this double val)
        {
            return Convert.ToInt32(Math.Round(val, MidpointRounding.AwayFromZero));
        }

        public static float ToRadians(this float degrees)
        {
            return degrees * Math.PI.ToFloat() / 180.0f;
        }

        public static float ToDegrees(this float rad)
        {
            return rad * 180.0f / Math.PI.ToFloat();
        }
    }
}
