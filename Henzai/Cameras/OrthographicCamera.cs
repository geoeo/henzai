using System;
using System.Numerics;

namespace Henzai.Cameras
{
    public class OrthographicCamera : Camera
    {

        public OrthographicCamera(float width, float height, Vector4 position, Vector4 lookDir, float far = 1000f, float near = 0.1f, float moveSpeed = 10f, bool use_supplied_up = false) : base(width, height, position, lookDir, Vector4.UnitY, far, near, MathF.PI/4 , moveSpeed, use_supplied_up)
        {
        }

        public override void UpdateProjectionMatrix(float width, float height){
            _projectionMatrix = Matrix4x4.CreateOrthographic(width, height, _near, _far);
        }
    }
}