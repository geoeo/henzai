using System;
using System.Numerics;
using Veldrid;

namespace Henzai
{
    public sealed class Light
    {
        private Vector4 _light; 

        public ref Vector4 Light_DontMutate => ref _light;

        public Light(){
            _light = new Vector4(0,20,15,1);
        }

        public Light(Vector4 light){
            _light = light;
        }

    }
}
