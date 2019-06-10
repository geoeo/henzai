using System.Numerics;

namespace Henzai.Cameras
{
    public class OrthographicCamera : Camera
    {

        public OrthographicCamera(float width, float height, float far = 1000f, float moveSpeed = 10f) : base(width,height,far,moveSpeed){
        }

        public override void UpdateProjectionMatrix(float width, float height){
            _projectionMatrix = Matrix4x4.CreateOrthographic(_fov, width / height, _near, _far);
        }
    }
}