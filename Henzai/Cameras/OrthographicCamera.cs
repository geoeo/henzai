using System.Numerics;

namespace Henzai.Cameras
{
    public class OrthographicCamera : Camera
    {

        public OrthographicCamera(float width, float height, Vector4 position, Vector4 lookAt, float far = 1000f, float moveSpeed = 10f) : base(width, height, position, lookAt, far, moveSpeed){
        }

        public override void UpdateProjectionMatrix(float width, float height){
            _projectionMatrix = Matrix4x4.CreateOrthographic(width, height, _near, _far);
        }
    }
}