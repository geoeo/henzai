using System;
using System.Numerics;
using Veldrid;

namespace Henzai
{
    public sealed class Light
    {
        public static Vector4 DEFAULT_POSITION = new Vector4(0,20,15,1);
        public static Vector4 DEFAULT_COLOR = Vector4.One;
        public static Vector4 DEFAULT_ATTENTUATION = new Vector4(1.0f,0.06f,0.025f,0.0f);
        // W channel is 1 for point and 0 for directional
        /// <summary>
        /// Postion, Color and Attenuation are passed to shaders. 3xVector4
        /// </summary>
        public static uint SizeInBytes => 48;
        private Vector4 _light; 
        // W channel used for intensity
        private Vector4 _color;
        // Constant,Linear,Quadratic,Unsused
        private Vector4 _attenuation = DEFAULT_ATTENTUATION;

        /// <summary>
        /// W channel is 1 for point and 0 for directional
        /// </summary>
        public ref Vector4 Light_DontMutate => ref _light;
        /// <summary>
        /// W channel used for intensity
        /// </summary>
        public ref Vector4 Color_DontMutate => ref _color;
        /// <summary>
        /// Constant, Linear ,Quadratic, Unsused
        /// </summary>
        public ref Vector4 Attentuation_DontMutate => ref _attenuation;

        public Light(){
            _light = DEFAULT_POSITION;
            _color = DEFAULT_COLOR;
        }

        public Light(Vector4 lightPos){
            _light = lightPos;
            _color = DEFAULT_COLOR;
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

        public Light(Vector4 light, RgbaFloat color, float intensity){
            _color = color.ToVector4();
            _color.W = intensity;
            _light = light;
        }

    }
}
