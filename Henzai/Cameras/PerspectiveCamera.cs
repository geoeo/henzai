using System;
using System.Numerics;

namespace Henzai.Cameras
{
    public class PerspectiveCamera : Camera
    {
        public PerspectiveCamera(float width, float height, Vector4 position, Vector4 lookDir, float far = 1000f, float fov = MathF.PI / 4, float moveSpeed = 100f) : base(width, height, position, lookDir, far, fov, moveSpeed){
        }

        public override void UpdateProjectionMatrix(float width, float height){
            _projectionMatrix =  Matrix4x4.CreatePerspectiveFieldOfView(_fov, width / height, _near, _far);
        }
    }
}