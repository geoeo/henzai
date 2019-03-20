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
        private readonly Material[] _materials;
        private Matrix4x4 _world = Matrix4x4.Identity;

        public int MeshCount => _meshes.Length;
        public int MaterialCount => _materials.Length;

        /// <summary>
        /// Should be get only! But this is not possible to enforce with refs
        /// </summary>
        public ref Matrix4x4 GetWorld_DontMutate => ref _world;
        public string BaseDir {get;private set;}

        public Model(string directory, Mesh<T>[] meshes, Material[] materials){
            BaseDir = directory;
            _meshes = meshes;
            _materials = materials;

        }

        public Model(Mesh<T>[] meshesIn, Material[] materials){
            BaseDir = string.Empty;
            _meshes = meshesIn;
            _materials = materials;
        }

        public Model(string directoy, Mesh<T> meshIn, Material material){
            BaseDir = directoy;
            _meshes = new Mesh<T>[1];
            _meshes[0] = meshIn;

            _materials = new Material[1];
            _materials[0] = material;
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

        /// <summary>
        /// This returns the material associated with a mesh.
        /// It can be indexed via the mesh index
        /// </summary>
        public Material TryGetMaterial(int index)
        {
            return _materials[index] ?? throw new NullReferenceException("The material you are trying to access is null");
        }

        public Material GetMaterialRuntime(int index)
        {
            return _materials[index];
        }

        public void SetAmbientForAllMeshes(Vector4 ambient) {
            for (int i = 0; i < _materials.Length; i++)
                TryGetMaterial(i).ambient = ambient;
        }

        //TODO: Retrieve a subset of the meshes encapsulated by this model class
        public Model<T> SplitByString(string id){
            return null;
        }

        
    }
}