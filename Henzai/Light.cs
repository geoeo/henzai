using System;
using System.Numerics;
using Henzai.Cameras;
using Veldrid;

namespace Henzai
{
    //TODO: Refactor Pointlight - make more efficient
    public sealed class Light
    {
        public static Light NO_POINTLIGHT = new Light(Vector4.Zero, RgbaFloat.Black, 0.0f, Vector4.Zero, Vector4.Zero);
        public static Vector4 DEFAULT_POSITION = new Vector4(0,20,15,1);
        public static Vector4 DEFAULT_LOOKAT = new Vector4(0, -1, 0, 0);
        public static Vector4 DEFAULT_COLOR = new Vector4(1.0f,1.0f,1.0f,0.1f);
        public static Vector4 DEFAULT_ATTENTUATION = new Vector4(1.0f,0.0014f,0.000007f,0.0f);
        // W channel is 1 for point and 0 for directional
        /// <summary>
        /// Postion, Color and Attenuation are passed to shaders. 3xVector4
        /// </summary>
        public static uint SizeInBytes => 48;
        public Camera LightCam {get; private set;}
        //private Vector4 _lightPos; 

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
        public Vector4 LightPos => LightCam.Position;
        public Matrix4x4 LightViewProj => LightCam.ViewProjectionMatirx;
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
            LightCam = new OrthographicCamera(0, 0, Light.DEFAULT_POSITION, Light.DEFAULT_LOOKAT);
            _color = DEFAULT_COLOR;
        }

        public Light(Vector4 lightPos, Vector4 lookAt){
            LightCam = new OrthographicCamera(0, 0, lightPos, lookAt);
            _color = DEFAULT_COLOR;
        }

        public Light(Vector4 lightPos,Vector4 lookAt, Vector4 color){
            LightCam = new OrthographicCamera(0, 0, lightPos, lookAt);
            _color = color;
        }

        public Light(RgbaFloat color){
            LightCam = new OrthographicCamera(0, 0, Light.DEFAULT_POSITION, Light.DEFAULT_LOOKAT);
            _color = color.ToVector4();
        }

        public Light(RgbaFloat color, Vector4 lookDir, float intensity){
            LightCam = new OrthographicCamera(0, 0, Light.DEFAULT_POSITION, lookDir);
            _color = color.ToVector4();
            _color.W = intensity;
        }

        public Light(RgbaFloat color, float intensity){
            LightCam = new OrthographicCamera(0, 0, Light.DEFAULT_POSITION, Light.DEFAULT_LOOKAT);
            _color = color.ToVector4();
            _color.W = intensity;
        }

        public Light(Vector4 lightPos,  RgbaFloat color, float intensity){
            LightCam = new OrthographicCamera(0, 0, lightPos, Light.DEFAULT_LOOKAT);
            _color = color.ToVector4();
            _color.W = intensity;
        }

        public Light(Vector4 lightPos, Vector4 lookAt,  RgbaFloat color, float intensity){
            LightCam = new OrthographicCamera(0, 0, lightPos, lookAt);
            _color = color.ToVector4();
            _color.W = intensity;
        }

        public Light(Vector4 lightPos, Vector4 lookAt, RgbaFloat color){
            LightCam = new OrthographicCamera(0, 0, lightPos, lookAt);
            _color = color.ToVector4();
        }


        public Light(Vector4 lightPos, RgbaFloat color, Vector4 attenuation){
            LightCam = new OrthographicCamera(0, 0, lightPos, Light.DEFAULT_LOOKAT);
            _color = color.ToVector4();
            _attenuation = attenuation;
        }

        public Light(Vector4 lightPos, Vector4 lookAt, RgbaFloat color, Vector4 attenuation){
            LightCam = new OrthographicCamera(0, 0, lightPos, lookAt);
            _color = color.ToVector4();
            _attenuation = attenuation;
        }


        public Light(Vector4 lightPos, RgbaFloat color, float intensity, Vector4 direction, Vector4 parameters){
            var lookAt = new Vector4(direction.X, direction.Y, direction.Z, 0);
            LightCam = new OrthographicCamera(0, 0, lightPos, lookAt);
            _color = color.ToVector4();
            _color.W = intensity;
            _direction = direction;
            _parameters = parameters;
        }

    }
}
