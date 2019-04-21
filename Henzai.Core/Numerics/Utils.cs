using System;
using System.Numerics;
using System.Runtime.CompilerServices; 

namespace Henzai.Core.Numerics{

    public static class Utils {

        public static float UnitValueOrFail(float v){
            var isUnitVal = v >= 0.0 && v <= 1.0;
            if (isUnitVal)
                return v;
            else
                throw new ArgumentException($"The value {v} is not in range [0,1]");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Square(float b) { return b*b;}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float t, float a, float b){
            return (1.0f - t) * a + t*b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RobustRayBounds(float value, float gamma) => value * (1.0f + 2.0f*gamma);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Gamma (int n) => ((float)n*System.Single.Epsilon)/(1.0f - (float)n*System.Single.Epsilon);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BoolToInt(bool b) => b ? 1 : 0;

    }
}