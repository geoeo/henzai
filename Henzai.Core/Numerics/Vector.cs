using System;
using System.Numerics;
using System.Runtime.CompilerServices; 

namespace Henzai.Core.Numerics {

    //TODO change all relevant "ref"s to "in"
    public static class Vector {

        public static Vector3 Normalize(ref Vector3 vec){
            if (Math.Round(vec.Length(), 5) == 1.0f)
                return vec;
            else
                Console.WriteLine($"WARN: Vector3 not normalized {vec.X} {vec.Y} {vec.Z}");
                return Vector3.Normalize(vec);
        }

        public static Vector4 Normalize(ref Vector4 vec)
        {
            if (Math.Round(vec.Length(), 5) == 1.0f)
                return vec;
            else
                Console.WriteLine($"WARN: Vector4 not normalized {vec.X} {vec.Y} {vec.Z} {vec.W}");
            return Vector4.Normalize(vec);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 RoundVec3(ref Vector3 vec3, int digits){
            if (digits == -1)
                return new Vector3(vec3.X, vec3.Y, vec3.Z);
            return new Vector3(MathF.Round(vec3.X, digits), MathF.Round(vec3.Y, digits), MathF.Round(vec3.Z, digits));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 RoundVec4(ref Vector4 vec4, int digits)
        {
            if (digits == -1)
                return new Vector4(vec4.X, vec4.Y, vec4.Z, vec4.W);
            return new Vector4(MathF.Round(vec4.X, digits), MathF.Round(vec4.Y, digits), MathF.Round(vec4.Z, digits), MathF.Round(vec4.W, digits));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToVec2(ref Vector4 vec4){
            return new Vector2(vec4.X, vec4.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVec3(ref Vector4 vec4){
            return new Vector3(vec4.X, vec4.Y, vec4.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVec3(Vector4 vec4){
            return new Vector3(vec4.X, vec4.Y, vec4.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 ToHomogeneous(ref Vector3 vec3, float c){
            return new Vector4(vec3.X, vec3.Y, vec3.Z, c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 CreateUnitVector3(float a, float b, float c){
            return Vector3.Normalize(new Vector3(a, b, c));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InMemoryDotProduct(ref Vector4 a, ref Vector4 b){
            return a.X*b.X+a.Y*b.Y+a.Z*b.Z+a.W*b.W;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InMemoryDotProduct3(ref Vector4 a, ref Vector4 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InMemoryDotProduct(ref Vector3 a, ref Vector3 b){
            return a.X*b.X+a.Y*b.Y+a.Z*b.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(ref Vector3 from, ref Vector3 to){
            return (to - from).Length();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(ref Vector4 from, ref Vector4 to)
        {
            return (to - from).Length();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSquared(ref Vector3 from, ref Vector3 to){
            return (to - from).LengthSquared();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RoundVectorInMem(ref Vector4 vec, int digits)
        {
            vec.X = MathF.Round(vec.X, digits);
            vec.Y = MathF.Round(vec.Y, digits);
            vec.Z = MathF.Round(vec.Z, digits);
            vec.W = MathF.Round(vec.W, digits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RoundVectorInMem(ref Vector3 vec, int digits)
        {
            vec.X = MathF.Round(vec.X, digits);
            vec.Y = MathF.Round(vec.Y, digits);
            vec.Z = MathF.Round(vec.Z, digits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyIntoVector4(ref Vector4 target, ref Vector3 data)
        {
            target.X = data.X;
            target.Y = data.Y;
            target.Z = data.Z;
        }



    }
}