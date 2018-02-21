using System;
using System.Numerics;
using Veldrid;

namespace Henzai.Geometry
{
    /// <summary>
    /// Usually used with loaded Meshes/Models
    /// Such as with Assimp
    /// </summary>
    public class Model
    {
        public Mesh[] meshes;
        public uint[][] meshIndicies;

        public int meshCount => meshes.Length;

        public Model(Mesh[] meshesIn, uint[][] indiciesIn){
            meshes = meshesIn;
            meshIndicies = indiciesIn;
        }

        public Model(Mesh[] meshesIn){
            meshes = meshesIn;
            meshIndicies = new uint[meshes.Length][];
        }
        
    }
}