using System;
using System.Numerics;
using Veldrid;

namespace Henzai.Extensions
{
    // Not used atm
    public static class MaterialExtensions
    {

        public static Vector4 GetRepresentationFor(this Vector4 color, GraphicsBackend backend)
        {
            Vector4 value = color;
            switch (backend)
            {
                case GraphicsBackend.OpenGL:
                    break;
                case GraphicsBackend.Metal:
                    break;
                case GraphicsBackend.Direct3D11:
                    throw new NotImplementedException();
                case GraphicsBackend.Vulkan:
                    throw new NotImplementedException();
            }
            return value;
        }
    }

}