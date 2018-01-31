using System;
using System.IO;
using Veldrid;

namespace Henzai
{
    class IO
    {

        public static Shader LoadShader(ShaderStages stage, GraphicsDevice graphicsDevice){
            string extension = null;
            switch(graphicsDevice.BackendType){
                case GraphicsBackend.OpenGL:
                    extension = "glsl";
                    break;
                case GraphicsBackend.Metal:
                    extension = "metallib";
                    break;
                default: throw new System.InvalidOperationException();
            }

            string entryPoint = stage == ShaderStages.Vertex ? "VS" : "FS";
            string path = Path.Combine(System.AppContext.BaseDirectory,"Shaders",$"{stage.ToString()}.{extension}");
            byte[] shaderBytes = File.ReadAllBytes(path);
            return graphicsDevice.ResourceFactory.CreateShader(new ShaderDescription(stage,shaderBytes,entryPoint));
        }

    }
 

}

