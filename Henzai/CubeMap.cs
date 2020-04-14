using System;
using System.Numerics;
using Henzai.Cameras;
using Veldrid;

namespace Henzai
{
    public class CubeMap {

        public enum FaceIndices : ushort {
            Right = 0, // +X
            Left = 1, // -X
            Top = 2, // +Y
            Bottom = 3, // -Y
            Back = 4, // +Z
            Front = 5 // -Z
        }

        public static Camera[] GenerateOmniCameras(Vector4 position, float width, float height){

            var cameras = new Camera[6];
            var far = 1000;
            cameras[(uint)FaceIndices.Right] = new PerspectiveCamera(width,height,position,Vector4.UnitX, far, MathF.PI / 2);
            cameras[(uint)FaceIndices.Left] = new PerspectiveCamera(width,height,position,-Vector4.UnitX, far, MathF.PI / 2);
            cameras[(uint)FaceIndices.Top] = new PerspectiveCamera(width,height,position,Vector4.UnitY, far, MathF.PI / 2);
            cameras[(uint)FaceIndices.Bottom] = new PerspectiveCamera(width,height,position,-Vector4.UnitY, far, MathF.PI / 2);
            cameras[(uint)FaceIndices.Back] = new PerspectiveCamera(width,height,position,Vector4.UnitZ, far, MathF.PI / 2);
            cameras[(uint)FaceIndices.Front] = new PerspectiveCamera(width,height,position,-Vector4.UnitZ, far, MathF.PI / 2);

            return cameras;
        }

    }
}