using System;
using System.Numerics;

namespace Henzai.Cameras
{
    public class PerspectiveCamera : Camera
    {
        public PerspectiveCamera(float width, float height, Vector4 position, Vector4 lookDir, Vector4 up, float far = 1000f,float near = 0.5f, float fov = MathF.PI / 4, float moveSpeed = 100f, bool use_supplied_up = false) : base(width, height, position, lookDir,up, far, near, fov, moveSpeed, use_supplied_up)
        {
        }

        public override void UpdateProjectionMatrix(float width, float height){
            _projectionMatrix =  Matrix4x4.CreatePerspectiveFieldOfView(_fov, width / height, _near, _far);
        }
    }
}