using System;
using System.Numerics;
using Henzai.Cameras;
using Veldrid;

namespace Henzai
{
    //TODO: Refactor Pointlight - make more efficient
    public sealed class Light
    {
        public static Light NO_POINTLIGHT = new Light(new OrthographicCamera(1.0f, 1.0f,new Vector4(0,0,0,1),new Vector4(0,-1,0,0)) , RgbaFloat.Black, 0.0f, 0.0f, Vector4.Zero);
        public static Vector4 DEFAULT_POSITION = new Vector4(0,20,15,1);
        public static Vector4 DEFAULT_LOOKAT = new Vector4(0, -1, 0, 0);
        public static Vector3 DEFAULT_UP = new Vector3(0, 0, -1);
        public static Vector4 DEFAULT_COLOR = new Vector4(1.0f,1.0f,1.0f,0.1f);
        public static float DEFAULT_WIDTH = 1.0f;
        public static float DEFAULT_HEIGHT = 1.0f;
        public static Vector4 DEFAULT_ATTENTUATION = new Vector4(1.0f,0.0014f,0.000007f,0.0f);
        // W channel is 1 for point and 0 for directional
        /// <summary>
        /// Postion, Color and Attenuation are passed to shaders. 3xVector4
        /// </summary>
        public static uint SizeInBytes => 48;
        public Camera LightCam {get; private set;}

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

        public Light(Camera camera){
            LightCam = camera;
            _color = DEFAULT_COLOR;
        }

        public Light(Camera camera, RgbaFloat color){
            LightCam = camera;
            _color = color.ToVector4();
        }

        public Light(Camera camera, RgbaFloat color, float intensity){
            LightCam = camera;
            _color = color.ToVector4();
            _color.W = intensity;
        }


        public Light(Camera camera, RgbaFloat color, Vector4 attenuation){
            LightCam = camera;
            _color = color.ToVector4();
            _attenuation = attenuation;
        }


        public Light(Camera camera, RgbaFloat color, float intensity, float attenuation, Vector4 parameters){
            LightCam = camera;
            _color = color.ToVector4();
            _color.W = intensity;
            var lookAt = camera.LookDirection;
            var direction = new Vector4(lookAt.X, lookAt.Y, lookAt.Z, attenuation);
            _direction = direction;
            _parameters = parameters;
        }

        public static Light[] GenerateOmniLights(Camera[] cameras,  RgbaFloat color, float intensity){
            var omniLights = new Light[6];
            for(var i = 0; i < 6; i++){
                var camera = cameras[i];
                 omniLights[i] = new Light(camera, color, intensity);           
            }

            return omniLights;
        }

    }
}
