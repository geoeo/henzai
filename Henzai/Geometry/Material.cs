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
    // Colours Vector4 are RGBA
    ///</summary>
    public sealed class Material
    {
        public Vector4 diffuse = Vector4.Zero;
        public Vector4 specular = Vector4.Zero;
        public Vector4 ambient = Vector4.Zero;
        public Vector4 emissive = Vector4.Zero;
        public Vector4 transparent = Vector4.Zero;
        public string textureDiffuse = string.Empty;


        ///<summary>
        /// If null reference is passed as an argument assignment will be skipped
        ///</summary>
        public Material(
            Vector4 diffuse, 
            Vector4 specular, 
            Vector4 ambient, 
            Vector4 emissive, 
            Vector4 transparent,  
            string textureDiffuse){

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
                if(!String.IsNullOrEmpty(textureDiffuse))
                    this.textureDiffuse = textureDiffuse;
             
            }

        public static RgbaFloat ToRgbaFloat(Vector4 value){
            return new RgbaFloat(value.X,value.Y,value.Z,value.W);
        }

    }
}
