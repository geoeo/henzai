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
        private Matrix4x4 _world = Matrix4x4.Identity;
        public int meshCount => meshes.Length;

        public ref Matrix4x4 World => ref _world;
        public string BaseDir {get;private set;}

        public Model(string directory, Mesh<T>[] meshes, uint[][] indicies){
            BaseDir = directory;
            this.meshes = meshes;
            for(int i = 0; i < meshCount; i++){
                this.meshes[i].meshIndices = indicies[i];
            }
        }

        public Model(Mesh<T>[] meshesIn){
            meshes = meshesIn;
        }

        public Model(Mesh<T> meshIn){
            meshes = new Mesh<T>[1];
            meshes[0] = meshIn;
        }

        public void SetNewWorldTransformation(Matrix4x4 world){
            _world = world;
        }

        
    }
}