using System;
using System.IO;
using System.Reflection;
using Veldrid;

namespace Henzai
{
    public static class IO
    {
        public static Shader LoadShader(string name, ShaderStages stage, GraphicsDevice graphicsDevice){
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
            string shaderName =  String.Concat(name,stage.ToString());

            string path = Path.Combine(System.AppContext.BaseDirectory,"Shaders",$"{shaderName}.{extension}");
            byte[] shaderBytes = File.ReadAllBytes(path);
            return graphicsDevice.ResourceFactory.CreateShader(new ShaderDescription(stage,shaderBytes,entryPoint));
        }

        public static byte[] GetEmbeddedResourceBytes(Assembly assembly, string resourceName)
        {
            using (Stream s = assembly.GetManifestResourceStream(resourceName))
            {
                byte[] ret = new byte[s.Length];
                s.Read(ret, 0, (int)s.Length);
                return ret;
            }
        }

    }
 
}

