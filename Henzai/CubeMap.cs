using System;
using System.Numerics;
using Henzai.Cameras;
using Veldrid;

namespace Henzai
{
    public class CubeMap {

        //TODO: Bug in veldrid?
        public enum FaceIndices : ushort {
            Right = 1, // +X
            Left = 0, // -X
            Top = 3, // +Y
            Bottom = 2, // -Y
            Back = 4, // +Z
            Front = 5 // -Z
        }

        public static Camera[] GenerateOmniCameras(Vector4 position, float width, float height){

            var cameras = new Camera[6];
            var far = 500;
            var near = 0.1f;
            cameras[(uint)FaceIndices.Right] = new PerspectiveCamera(width,height,position,Vector4.UnitX, Vector4.UnitY,  far, near, MathF.PI / 2);
            cameras[(uint)FaceIndices.Left] = new PerspectiveCamera(width,height,position,-Vector4.UnitX, Vector4.UnitY, far, near, MathF.PI / 2);
            cameras[(uint)FaceIndices.Top] = new PerspectiveCamera(width,height,position,Vector4.UnitY, Vector4.UnitZ, far, near, MathF.PI / 2);
            cameras[(uint)FaceIndices.Bottom] = new PerspectiveCamera(width,height,position,-Vector4.UnitY, Vector4.UnitZ, far, near, MathF.PI / 2);
            cameras[(uint)FaceIndices.Back] = new PerspectiveCamera(width,height,position,Vector4.UnitZ, Vector4.UnitY, far, near, MathF.PI / 2);
            cameras[(uint)FaceIndices.Front] = new PerspectiveCamera(width,height,position,-Vector4.UnitZ, Vector4.UnitY, far, near, MathF.PI / 2);

            return cameras;
        }

    }
}