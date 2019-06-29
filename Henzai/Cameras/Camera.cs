using System;
using System.Numerics;
using Veldrid;
using Henzai.Core.Extensions;
using Henzai.Core.Numerics;

namespace Henzai.Cameras
{
    public abstract class Camera
    {
        /// <summary>
        /// Size of MVP pipeline.
        /// 3 x Matrix4x4 of type float = 192
        /// </summary>
        public static uint SizeInBytes => 192;

        public static readonly Vector4 DEFAULT_POSITION = new Vector4(0, 0, 10f, 1.0f);
        public static readonly Vector4 DEFAULT_LOOK_DIRECTION = new Vector4(0, 0, -1f, 0);
        public static readonly Vector3 DEFAULT_UP = new Vector3(0, 1, 0);

        protected float _fov;
        protected float _near;
        protected float _far;

        private Vector4 _position;
        private Vector4 _lookDirection;
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
        public Vector4 Position { get => _position; set { _position = value; UpdateViewMatrix(); } }
        public Vector4 LookDirection { get => _lookDirection; set { _lookDirection = value; UpdateViewMatrix();} }
        public Vector3 UpDirection { get => _upDirection; set { _upDirection = value; UpdateViewMatrix();} }
        public float FarDistance { get => _far; set { _far = value; UpdateViewMatrix();} }
        public float FieldOfView { get => _fov; set { _fov = value; UpdateViewMatrix();} }
        public float NearDistance { get => _near; set { _near = value; UpdateViewMatrix();} }
        public float WindowHeight => _windowHeight;
        public float WindowWidth => _windowWidth;
        public float AspectRatio => _windowWidth / _windowHeight;

        public float Yaw { get => _yaw; set { _yaw = value; UpdateViewMatrix(); } }
        public float Pitch { get => _pitch; set { _pitch = value; UpdateViewMatrix(); } }


        public Camera(float width, float height, Vector4 position, Vector4 lookAt, float far = 1000f, float moveSpeed = 10f)
        {
            _fov = MathF.PI/4; // Might be redundant as GPU assumes its always 90 degrees
            _near = 0.1f;
            _far = far;
            _position = position;
            _moveSpeed = moveSpeed;
            _lookDirection = Vector4.Normalize(lookAt);

            _windowWidth = width;
            _windowHeight = height;
            UpdateProjectionMatrix(width, height);

            //TODO: streamline vector3-4 switching
            var lookAtNorm = _lookDirection;
            var R = Core.Numerics.Geometry.RotationBetweenUnitVectors(DEFAULT_LOOK_DIRECTION, lookAtNorm);
            var l = new Vector4(DEFAULT_UP.X, DEFAULT_UP.Y, DEFAULT_UP.Z, 0.0f);
            var newUp = Vector4.Transform(l,R);
            _upDirection = newUp.ToVec3DiscardW();

            var position_Vec3 = _position.ToVec3DiscardW(); 
            var posLookAt = _position + _lookDirection;
            _viewMatrix = Matrix4x4.CreateLookAt(position_Vec3, posLookAt.ToVec3DiscardW(), _upDirection);
            _viewProjectionMatrix = _viewMatrix*_projectionMatrix;
        }

        public abstract void UpdateProjectionMatrix(float width, float height);

        private void UpdateViewMatrix()
        {
            Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(_yaw, _pitch, 0f);
            var lookDir = Vector4.Transform(DEFAULT_LOOK_DIRECTION, lookRotation);
            var up = Vector3.Transform(Vector3.UnitY, lookRotation);           
            var position_Vec3 = _position.ToVec3DiscardW(); 
            _lookDirection = Vector4.Normalize(lookDir);
            var lookAt = _position + _lookDirection;
            _upDirection = Vector3.Normalize(up);
            _viewMatrix = Matrix4x4.CreateLookAt(position_Vec3, lookAt.ToVec3DiscardW(), _upDirection);
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
                _lookDirection = Vector4.UnitY;
                _upDirection = -Vector3.UnitZ;
                _yaw = 0; _pitch = MathF.PI/2.0f;

                UpdateViewMatrix();
            }

            if (motionDir != Vector3.Zero)
            {
                Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(_yaw, _pitch, 0f);
                motionDir = Vector3.Transform(motionDir, lookRotation);
                var position_Vec3 = _position.ToVec3DiscardW(); 
                position_Vec3 += motionDir * _moveSpeed * sprintFactor * deltaSeconds;
                _position = new Vector4(position_Vec3, 1.0f);
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

    }
}