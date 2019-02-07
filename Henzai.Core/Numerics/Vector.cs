using System;
using System.Numerics;
using System.Runtime.CompilerServices; 

namespace Henzai.Core.Numerics{

    public static class Vector {

        public static Vector3 Normalize(ref Vector3 vec){
            if (Math.Round(vec.Length(),5) == 1.0f)
                return vec;
            else
                Console.WriteLine($"WARN: Vector3 not normalized {vec.X} {vec.Y} {vec.Z}");
                return Vector3.Normalize(vec);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 RoundVec3(ref Vector3 vec3, int digits){
            return new Vector3(MathF.Round(vec3.X, digits), MathF.Round(vec3.Y, digits), MathF.Round(vec3.Z, digits));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVec3(ref Vector4 vec4){
            return new Vector3(vec4.X, vec4.Y, vec4.Z);
        }

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


// let ApplyFuncToVector3 func (vec : Vector3) = Vector3(func vec.X, func vec.Y, func vec.Z)

    }
}