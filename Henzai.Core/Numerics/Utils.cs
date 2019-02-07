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

    }
}