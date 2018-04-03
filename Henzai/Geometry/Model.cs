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

        /// <summary>
        /// Should be get only! But this is not possible to enforce with refs
        /// </summary>
        public ref Matrix4x4 GetWorld_DontMutate => ref _world;
        public string BaseDir {get;private set;}

        public Model(string directory, Mesh<T>[] meshes, uint[][] indicies){
            BaseDir = directory;
            this.meshes = meshes;
            for(int i = 0; i < meshCount; i++){
                this.meshes[i].meshIndices = indicies[i];
            }
        }

        public Model(Mesh<T>[] meshesIn){
            BaseDir = String.Empty;
            meshes = meshesIn;
        }

        public Model(string directoy,Mesh<T> meshIn){
            BaseDir = directoy;
            meshes = new Mesh<T>[1];
            meshes[0] = meshIn;
        }

        public void SetNewWorldTransformation(ref Matrix4x4 world, bool applyToAllMeshes){
            _world = world;
            if(applyToAllMeshes){
                foreach(var mesh in meshes)
                    mesh.SetNewWorldTransformation(ref world);
            }
        }

        public void SetNewWorldTranslation(ref Vector3 translation, bool applyToAllMeshes){
            _world.Translation = translation;
            if(applyToAllMeshes){
                foreach(var mesh in meshes)
                    mesh.SetNewWorldTranslation(ref translation);
            }
        }

        public void SetAmbientForAllMeshes(Vector4 ambient){
            foreach(var mesh in meshes)
                mesh.TryGetMaterial().ambient = ambient;
        }

        
    }
}