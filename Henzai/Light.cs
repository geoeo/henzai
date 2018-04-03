using System;
using System.Numerics;
using Veldrid;

namespace Henzai
{
    public sealed class Light
    {
        public static Vector4 DEFAULT_POSITION = new Vector4(0,20,15,1);
        // W channel is 1 for point and 0 for directional
        private Vector4 _light; 
        // W channel used for intensity
        private Vector4 _color;

        public ref Vector4 Light_DontMutate => ref _light;
        public ref Vector4 Color_DontMutate => ref _color;

        public Light(){
            _light = DEFAULT_POSITION;
            _color = Vector4.One;
        }

        public Light(Vector4 lightPos){
            _light = lightPos;
            _color = Vector4.One;
        }

        public Light(Vector4 light, Vector4 color){
            _light = light;
            _color = color;
        }

        public Light(RgbaFloat color){
            _color = color.ToVector4();
            _light = DEFAULT_POSITION;
        }

        public Light(RgbaFloat color, float intensity){
            _color = color.ToVector4();
            _color.W = intensity;
            _light = DEFAULT_POSITION;
        }

    }
}
