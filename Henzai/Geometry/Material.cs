using System;
using System.Numerics;
using Veldrid;

namespace Henzai.Geometry
{
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

        public static RgbaFloat ToRgbaFloat(Vector3 value, float opacity){
            return new RgbaFloat(value.X,value.Y,value.Z,opacity);
        }

    }
}
