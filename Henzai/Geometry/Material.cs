using System;
using System.Numerics;
using Veldrid;

// TODO: Add Material coefficients, more textures, Make Color own struct (or use Veldrid.RGBA)
namespace Henzai.Geometry
{

    // Not used atm
    public static class MaterialExtensions
    {

        public static Vector4 GetRepresentationFor(this Vector4 color, GraphicsBackend backend){
            Vector4 value = color;
            switch(backend){
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

    ///<summary>
    // Based on http://assimp.sourceforge.net/lib_html/materials.html
    // colors Vector4 are RGBA
    ///</summary>
    public sealed class Material
    {
        /// <summary>
        /// Currently ambient,diffuse,specular and coefficients are passed to shaders
        /// </summary>
        public static uint SizeInBytes => 64;
        public Vector4 ambient = Vector4.Zero;
        public Vector4 diffuse = Vector4.Zero;
        public Vector4 specular = Vector4.Zero;
        public Vector4 emissive = Vector4.Zero;
        public Vector4 transparent = Vector4.Zero;
        public Vector4 coefficients = Vector4.Zero; // shininess,shininess_str,opacity,reflectivity
        public string textureDiffuse = string.Empty;
        public string textureNormal = string.Empty;
        public string textureBump = string.Empty;
        public string textureSpecular = string.Empty;

        public string cubeMapFront = string.Empty;
        public string cubeMapBack = string.Empty;
        public string cubeMapLeft = string.Empty;
        public string cubeMapRight = string.Empty;
        public string cubeMapTop = string.Empty;
        public string cubeMapBottom = string.Empty;
        

        public Material(){
            diffuse = new Vector4(1.0f,1.0f,1.0f,1.0f);
        }


        //<summary>
        // If null reference is passed as an argument assignment will be skipped
        //</summary>
        public Material(
            Vector4 ambient, 
            Vector4 diffuse, 
            Vector4 specular, 
            Vector4 emissive, 
            Vector4 transparent, 
            Vector4 coefficients, 
            string textureDiffuse,
            string textureNormal,
            string textureBump,
            string textureSpecular){

                if(diffuse != null)
                    this.diffuse = diffuse;
                if(specular != null)
                    this.specular = specular;
                if(ambient != null)
                    this.ambient = ambient;
                if(emissive != null)
                    this.emissive = emissive;
                if(transparent != null)
                    this.transparent = transparent;
                if(coefficients != null)
                    this.coefficients = coefficients;
                if(!String.IsNullOrEmpty(textureDiffuse))
                    this.textureDiffuse = textureDiffuse;
                if(!String.IsNullOrEmpty(textureNormal))
                    this.textureNormal = textureNormal;
                if(!String.IsNullOrEmpty(textureBump))
                    this.textureBump = textureBump;
                if(!String.IsNullOrEmpty(textureSpecular))
                    this.textureSpecular = textureSpecular;
             
            }

        public static RgbaFloat ToRgbaFloat(Vector4 value){
            return new RgbaFloat(value.X,value.Y,value.Z,value.W);
        }

    }
}
