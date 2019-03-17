using System;
using System.Numerics;
using Henzai.Core.VertexGeometry;

namespace Henzai.Geometry
{
    /// <summary>
    /// Usually used with loaded Meshes/Models
    /// Such as with Assimp
    /// </summary>
    public sealed class Model<T> where T : struct, VertexLocateable
    {
        private readonly Mesh<T>[] _meshes;
        private Matrix4x4 _world = Matrix4x4.Identity;
        public int MeshCount => _meshes.Length;

        /// <summary>
        /// Should be get only! But this is not possible to enforce with refs
        /// </summary>
        public ref Matrix4x4 GetWorld_DontMutate => ref _world;
        public string BaseDir {get;private set;}

        public Model(string directory, Mesh<T>[] meshes, ushort[][] indicies){
            BaseDir = directory;
            this._meshes = meshes;
            for(int i = 0; i < MeshCount; i++){
                this._meshes[i].MeshIndices = indicies[i];
            }
        }

        public Model(Mesh<T>[] meshesIn){
            BaseDir = string.Empty;
            _meshes = meshesIn;
        }

        public Model(string directoy,Mesh<T> meshIn){
            BaseDir = directoy;
            _meshes = new Mesh<T>[1];
            _meshes[0] = meshIn;
        }

        public Mesh<T> GetMesh(int index)
        {
            return _meshes[index];
        }

        public void SetNewWorldTransformation(ref Matrix4x4 world, bool applyToAllMeshes){
            _world = world;
            if(applyToAllMeshes){
                foreach(var mesh in _meshes)
                    mesh.SetNewWorldTransformation(ref world);
            }
        }

        public void SetNewWorldTranslation(ref Vector3 translation, bool applyToAllMeshes){
            _world.Translation = translation;
            if(applyToAllMeshes){
                foreach(var mesh in _meshes)
                    mesh.SetNewWorldTranslation(ref translation);
            }
        }

        public void SetAmbientForAllMeshes(Vector4 ambient){
            foreach(var mesh in _meshes)
                mesh.TryGetMaterial().ambient = ambient;
        }

        //TODO: Retrieve a subset of the meshes encapsulated by this model class
        public Model<T> SplitByString(string id){
            return null;
        }

        
    }
}