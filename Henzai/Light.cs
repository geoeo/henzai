using System;
using System.Numerics;
using Veldrid;

namespace Henzai
{
    //TODO: Refactor Pointlight - make more efficient
    public sealed class Light
    {
        public static Light NO_POINTLIGHT = new Light(Vector4.Zero,RgbaFloat.Black,0.0f,Vector4.Zero,Vector4.Zero);
        public static Vector4 DEFAULT_POSITION = new Vector4(0,20,15,1);
        public static Vector4 DEFAULT_COLOR = new Vector4(1.0f,1.0f,1.0f,0.1f);
        public static Vector4 DEFAULT_ATTENTUATION = new Vector4(1.0f,0.0014f,0.000007f,0.0f);
        // W channel is 1 for point and 0 for directional
        /// <summary>
        /// Postion, Color and Attenuation are passed to shaders. 3xVector4
        /// </summary>
        public static uint SizeInBytes => 48;
        private Vector4 _lightPos; 

        // W channel used for intensity
        private Vector4 _color;

        // Constant,Linear,Quadratic,Unsused
        private Vector4 _attenuation = DEFAULT_ATTENTUATION;

        // Pointlight - xyz are direction, w is linear attenuation
        private Vector4 _direction;
        // outer cutoff, inner cutoff, epsilon
        private Vector4 _parameters;

        /// <summary>
        /// W channel is 1 for point and 0 for directional
        /// </summary>
        public ref Vector4 LightPos_DontMutate => ref _lightPos;
        /// <summary>
        /// W channel used for intensity
        /// </summary>
        public ref Vector4 Color_DontMutate => ref _color;
        /// <summary>
        /// Constant, Linear ,Quadratic, Unsused
        /// </summary>
        public ref Vector4 Attentuation_DontMutate => ref _attenuation;
        /// <summary>
        /// PointL: Direction (XYZ), Linear Attenuation
        /// </summary>
        public ref Vector4 Direction_DontMutate => ref _direction;
        /// <summary>
        /// PointL: outer cutoff, inner cutoff, Unused, Unused
        /// </summary>s
        public ref Vector4 Parameters_DontMutate => ref _parameters;

        public Light(){
            _lightPos = DEFAULT_POSITION;
            _color = DEFAULT_COLOR;
        }

        public Light(Vector4 lightPos){
            _lightPos = lightPos;
            _color = DEFAULT_COLOR;
        }

        public Light(Vector4 lightPos, Vector4 color){
            _lightPos = lightPos;
            _color = color;
        }

        public Light(RgbaFloat color){
            _color = color.ToVector4();
            _lightPos = DEFAULT_POSITION;
        }

        public Light(RgbaFloat color, float intensity){
            _color = color.ToVector4();
            _color.W = intensity;
            _lightPos = DEFAULT_POSITION;
        }

        public Light(Vector4 lightPos, RgbaFloat color, float intensity){
            _color = color.ToVector4();
            _color.W = intensity;
            _lightPos = lightPos;
        }

        public Light(Vector4 lightPos, RgbaFloat color){
            _color = color.ToVector4();
            _lightPos = lightPos;
        }

        public Light(Vector4 lightPos, RgbaFloat color, Vector4 attenuation){
            _color = color.ToVector4();
            _attenuation = attenuation;
            _lightPos = lightPos;
        }

        public Light(Vector4 light, RgbaFloat color, float intensity,Vector4 direction, Vector4 parameters){
            _color = color.ToVector4();
            _color.W = intensity;
            _lightPos = light;
            _direction = direction;
            _parameters = parameters;
        }

    }
}
