using System;
using System.Numerics;
using Veldrid;

namespace Henzai.Geometry
{
    /// <summary>
    /// Usually used with loaded Meshes/Models
    /// Such as with Assimp
    /// </summary>
    public sealed class Model<T> where T : struct
    {
        public Mesh<T>[] meshes;
        public uint[][] meshIndicies;
        private Matrix4x4 _world = Matrix4x4.Identity;
        public int meshCount => meshes.Length;

        public ref Matrix4x4 World => ref _world;

        public Model(Mesh<T>[] meshes, uint[][] indicies){
            this.meshes = meshes;
            meshIndicies = indicies;
        }

        public Model(Mesh<T>[] meshesIn){
            meshes = meshesIn;
            meshIndicies = new uint[meshes.Length][];
        }

        
    }
}