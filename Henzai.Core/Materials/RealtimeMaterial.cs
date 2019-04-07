using System.Numerics;

// TODO: Add Material coefficients, more textures, Make Color own struct (or use Veldrid.RGBA)
namespace Henzai.Core.Materials
{

    ///<summary>
    // Based on http://assimp.sourceforge.net/lib_html/materials.html
    // colors Vector4 are RGBA
    ///</summary>
    public sealed class RealtimeMaterial
    {
        //TODO refactor using Getters and Setters
        /// <summary>
        /// Currently ambient,diffuse,specular and coefficients are passed to shaders
        /// </summary>
        public const uint SizeInBytes = 176;

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


        public RealtimeMaterial()
        {
            diffuse = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        }


        //<summary>
        // If null reference is passed as an argument assignment will be skipped
        //</summary>
        public RealtimeMaterial(
            Vector4 ambient,
            Vector4 diffuse,
            Vector4 specular,
            Vector4 emissive,
            Vector4 transparent,
            Vector4 coefficients,
            string textureDiffuse,
            string textureNormal,
            string textureBump,
            string textureSpecular)
        {
            AssignVectorData(ambient, diffuse, specular, emissive, transparent, coefficients);
            AssignTexturePaths(textureDiffuse, textureNormal, textureBump, textureSpecular);

        }

        //TODO refactor using refs
        public void AssignVectorData(Vector4 ambientIn, Vector4 diffuseIn, Vector4 specularIn, Vector4 emissiveIn, Vector4 transparentIn, Vector4 coefficientsIn)
        {
            diffuse = diffuseIn;
            specular = specularIn;
            ambient = ambientIn;
            emissive = emissiveIn;
            transparent = transparentIn;
            coefficients = coefficientsIn;
        }

        public void AssignTexturePaths(string textureDiffuseIn, string textureNormalIn, string textureBumpIn, string textureSpecularIn)
        {
            if (!string.IsNullOrEmpty(textureDiffuseIn))
                textureDiffuse = textureDiffuseIn;
            if (!string.IsNullOrEmpty(textureNormalIn))
                textureNormal = textureNormalIn;
            if (!string.IsNullOrEmpty(textureBumpIn))
                textureBump = textureBumpIn;
            if (!string.IsNullOrEmpty(textureSpecularIn))
                textureSpecular = textureSpecularIn;
        }

        public void AssignCubemapPaths(string front, string back, string left, string right, string up, string down)
        {
            cubeMapFront = front;
            cubeMapBack = back;
            cubeMapLeft = left;
            cubeMapRight = right;
            cubeMapTop = up;
            cubeMapBottom = down;
        }

        public void AssignMaterialData(Vector4[] colors, string[] textureStrings, string[] cubemapStrings)
        {
            AssignVectorData(colors[0], colors[1], colors[2], colors[3], colors[4], colors[5]);
            AssignTexturePaths(textureStrings[0], textureStrings[1], textureStrings[2], textureStrings[3]);
            AssignCubemapPaths(cubemapStrings[0], cubemapStrings[1], cubemapStrings[2], cubemapStrings[3], cubemapStrings[4], cubemapStrings[0]);
            if (cubemapStrings.Length > 0)
                AssignCubemapPaths(cubemapStrings[0], cubemapStrings[1], cubemapStrings[2], cubemapStrings[3], cubemapStrings[4], cubemapStrings[5]);
        }
    }
}
