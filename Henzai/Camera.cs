using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Henzai.Core.Extensions;

namespace Henzai
{
    public abstract class Camera
    {
        /// <summary>
        /// Size of MVP pipeline.
        /// 3 x Matrix4x4 of type float = 192
        /// </summary>
        public static uint SizeInBytes => 192;

        private readonly Vector3 DEFAULT_POSITION = new Vector3(0,0,10f);
        private readonly Vector3 DEFAULT_LOOK_DIRECTION = new Vector3(0,0,-1f);

        protected float _fov;
        protected float _near;
        protected float _far;

        private Vector3 _position;
        private Vector3 _lookDirection;
        private Vector3 _upDirection;
        private float _moveSpeed;

        private float _yaw;
        private float _pitch;

        private Vector2 _previousMousePos;
        private float _windowWidth;
        private float _windowHeight;

        private Matrix4x4 _viewMatrix;
        protected Matrix4x4 _projectionMatrix;
        private Matrix4x4 _viewProjectionMatrix;

        public ref Matrix4x4 ViewMatrix => ref _viewMatrix;
        public ref Matrix4x4 ProjectionMatrix => ref _projectionMatrix;
        public ref Matrix4x4 ViewProjectionMatirx => ref _viewProjectionMatrix;
        public Vector3 Position { get => _position; set { _position = value; UpdateViewMatrix(); } }
        public Vector3 LookDirection { get => _lookDirection; set { _lookDirection = value; UpdateViewMatrix();} }
        public Vector3 UpDirection { get => _upDirection; set { _upDirection = value; UpdateViewMatrix();} }
        public float FarDistance { get => _far; set { _far = value; UpdateViewMatrix();} }
        public float FieldOfView { get => _fov; set { _fov = value; UpdateViewMatrix();} }
        public float NearDistance { get => _near; set { _near = value; UpdateViewMatrix();} }
        public float WindowHeight => _windowHeight;
        public float WindowWidth => _windowWidth;
        public float AspectRatio => _windowWidth / _windowHeight;

        public float Yaw { get => _yaw; set { _yaw = value; UpdateViewMatrix(); } }
        public float Pitch { get => _pitch; set { _pitch = value; UpdateViewMatrix(); } }

        public Camera(float width, float height, float far = 1000f, float moveSpeed = 10f)
        {
            _fov = MathF.PI/4; // Might be redundant as GPU assumes its always 90 degrees
            _near = 0.1f;
            _far = far;
            _position = DEFAULT_POSITION;
            _lookDirection = DEFAULT_LOOK_DIRECTION;
            _moveSpeed = moveSpeed;

            _windowWidth = width;
            _windowHeight = height;
            UpdateProjectionMatrix(width, height);
            UpdateViewMatrix();
        }

        public abstract void UpdateProjectionMatrix(float width, float height);
        // {
        //     _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(_fov, width / height, _near, _far);
        // }

        private void UpdateViewMatrix()
        {
            Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(_yaw, _pitch, 0f);
            Vector3 lookDir = Vector3.Transform(DEFAULT_LOOK_DIRECTION, lookRotation);
            Vector3 up = Vector3.Transform(Vector3.UnitY, lookRotation);            
            _lookDirection = Vector3.Normalize(lookDir);
            _upDirection = Vector3.Normalize(up);
            _viewMatrix = Matrix4x4.CreateLookAt(_position, _position + _lookDirection, _upDirection);
            _viewProjectionMatrix = _viewMatrix*_projectionMatrix;
        }

        public void Update(float deltaSeconds, InputTracker inputTracker)
        {
            float sprintFactor = inputTracker.GetKey(Key.ControlLeft)
                ? 0.1f
                : inputTracker.GetKey(Key.ShiftLeft)
                    ? 2.5f
                    : 1f;
            Vector3 motionDir = Vector3.Zero;
            if (inputTracker.GetKey(Key.A))
            {
                motionDir += -Vector3.UnitX;
            }
            if (inputTracker.GetKey(Key.D))
            {
                motionDir += Vector3.UnitX;
            }
            if (inputTracker.GetKey(Key.W))
            {
                motionDir += -Vector3.UnitZ;
            }
            if (inputTracker.GetKey(Key.S))
            {
                motionDir += Vector3.UnitZ;
            }
            if (inputTracker.GetKey(Key.Q))
            {
                motionDir += -Vector3.UnitY;
            }
            if (inputTracker.GetKey(Key.E))
            {
                motionDir += Vector3.UnitY;
            }

            if(inputTracker.GetKey(Key.R))
            {
                _position = DEFAULT_POSITION;
                _lookDirection = DEFAULT_LOOK_DIRECTION;
                _upDirection = Vector3.UnitY;
                _yaw = 0; _pitch = 0;

                UpdateViewMatrix();
            }

            if(inputTracker.GetKey(Key.J))
            {
                _position = DEFAULT_POSITION;
                _lookDirection = Vector3.UnitY;
                _upDirection = -Vector3.UnitZ;
                _yaw = 0; _pitch = MathF.PI/2.0f;

                UpdateViewMatrix();
            }

            if (motionDir != Vector3.Zero)
            {
                Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(_yaw, _pitch, 0f);
                motionDir = Vector3.Transform(motionDir, lookRotation);
                _position += motionDir * _moveSpeed * sprintFactor * deltaSeconds;
                UpdateViewMatrix();
            }

            Vector2 mouseDelta = InputTracker.MousePosition - _previousMousePos;
            _previousMousePos = InputTracker.MousePosition;

            if (/*!ImGui.IsAnyWindowHovered() && */ (inputTracker.GetMouseButton(MouseButton.Left) || inputTracker.GetMouseButton(MouseButton.Right)))
            {
                _yaw += -mouseDelta.X * 0.01f;
                _pitch += -mouseDelta.Y * 0.01f;
                _pitch = Pitch.Clamp(-1.55f, 1.55f);

                UpdateViewMatrix();
            }
        }

        // public CameraInfo GetCameraInfo() => new CameraInfo
        // {
        //     CameraPosition_WorldSpace = _position,
        //     CameraLookDirection = _lookDirection
        // };
    }

    //[StructLayout(LayoutKind.Sequential)]
    // [StructLayout(LayoutKind.Explicit)]
    // public struct CameraInfo
    // {
    //     [FieldOffset(0)]
    //     public Vector3 CameraPosition_WorldSpace;
    //     //private float _padding1;
    //     [FieldOffset(16)]
    //     public Vector3 CameraLookDirection;
    //     //private float _padding2;
    // }
}