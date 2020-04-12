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

            string entryPoint = string.Empty;
            switch (stage)
            {
                case ShaderStages.Vertex:
                    entryPoint = "VS";
                    break;
                case ShaderStages.Geometry:
                    entryPoint = "GS";
                    break;
                case ShaderStages.Fragment:
                    entryPoint = "FS";
                    break;

            }
            string shaderName =  String.Concat(name,stage.ToString());

            string path = Path.Combine(System.AppContext.BaseDirectory,"Shaders",$"{shaderName}.{extension}");
            //TODO: it can happend due to some effects using Geometry shaders and some dont, that a shader will not exist.
            //TODO: Think of a better solution
            if (File.Exists(path))
            {
                byte[] shaderBytes = File.ReadAllBytes(path);
                return graphicsDevice.ResourceFactory.CreateShader(new ShaderDescription(stage, shaderBytes, entryPoint));
            } else
            {
                return null;
            }

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

