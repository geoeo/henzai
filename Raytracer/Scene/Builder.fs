module Raytracer.Scene.Builder

open System
open System.Numerics
open Raytracer.Scene.Geometry
open Raytracer.Surface
open Raytracer.RuntimeParameters
open Henzai.Core;
open Henzai.Core.VertexGeometry;
open Henzai.Core.Materials;
open Henzai.Core.Numerics
open HenzaiFunc.Core.VertexGeometry
open HenzaiFunc.Core.Acceleration
open HenzaiFunc.Core.RaytraceGeometry
open SixLabors.ImageSharp.PixelFormats
open Henzai.Core.VertexGeometry
open Henzai.Core.Reflection
open Henzai.Core.VertexGeometry

#nowarn "64"
//let scene = List.concat [plane_floor;light_sphere;spheres_glass]
// let scene = List.concat [spheres_scene_2;planes_scene_2;lights]
//let sceneList = List.concat [lights_simple]

let emptySurfaceArray :Surface[] = [||]

//TODO move this no surface or surface sub module
let convertModelToSurfaceList (model: Model<'T, RaytraceMaterial>, vertexTypeTransform : 'T -> 'T, vertexRuntimeType : VertexRuntimeTypes) =
    if not (Verifier.VerifyVertexStruct<'T>(vertexRuntimeType)) then
        failwithf "Supplied vertex type and enum do not match in convertModelToSurfaceList()"
    let meshCount = model.MeshCount
    let mutable surfaceList: Surface list = []
    let mutable totalVertexCount = 0
    for i in 0..meshCount-1 do
        let mesh = model.GetMesh(i)
        let material = model.GetMaterial(i)

        let indicesCount = mesh.IndicesCount
        let indices = mesh.MeshIndices
        //let vertices : 'T[] = Array.map vertexTypeTransform mesh.Vertices
        let vertices : 'T[] = Array.map vertexTypeTransform mesh.Vertices
        let mutable counter = 0

        totalVertexCount <- totalVertexCount + mesh.VertexCount
        
        for j in 0..3..indicesCount-1 do
            let i1 = (int)indices.[j]
            let i2 = (int)indices.[j+1]
            let i3 = (int)indices.[j+2]  

            match vertexRuntimeType with
            | VertexRuntimeTypes.VertexPosition ->
                let triangle = new IndexedTriangle<'T>(i1, i2, i3, vertices)
                let surface = Lambertian(assignIDAndIncrement id, triangle, material)
                surfaceList <- surface :> Surface :: surfaceList
            | VertexRuntimeTypes.VertexPositionNormal ->

                // Warning 64 suppressed at top of file
                let v1 : VertexPositionNormal = vertices.[i1]
                let v2 : VertexPositionNormal = vertices.[i2]
                let v3 : VertexPositionNormal = vertices.[i3]

                let n1 = Vector4(v1.GetNormal(), 0.0f)
                let n2 = Vector4(v2.GetNormal(), 0.0f)
                let n3 = Vector4(v3.GetNormal(), 0.0f)
  
                let triangle = new IndexedTriangleNormal<'T>(Vector.ToVec3(v1.GetPosition()), Vector.ToVec3(v2.GetPosition()), Vector.ToVec3(v3.GetPosition()), n1,n2,n3)
                let surface = Lambertian(assignIDAndIncrement id, triangle, material)
                //let surface = NormalVis(assignIDAndIncrement id, triangle, material)
                surfaceList <- surface :> Surface :: surfaceList

            | x -> failwithf "VertexRuntimeType %u not implemented" (LanguagePrimitives.EnumToValue x)
            

            counter <- counter + 1

    printfn "Total Vertex Count: %i" totalVertexCount
    surfaceList

//TODO: profile larger models
let loadAssets = 
    //let sceneList = List.concat [lightsAA;triangle_scene;spheres_scene_2;light_sphere;planes_scene_2_AA] # old scene 
    //let sceneNonBoundableArray : Surface[] = List.concat [plane_floor_Unbounded;lightsNonAA;plane_mirror_NonAA] |> Array.ofList # old scene

    //let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/Box.dae", VertexPositionNormal.HenzaiType, AssimpLoader.RaytracePostProcessSteps)
    let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/duck.dae", VertexPositionNormal.HenzaiType, AssimpLoader.RaytracePostProcessSteps)
    //let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/teapot.dae", VertexPositionNormal.HenzaiType, AssimpLoader.RaytracePostProcessSteps)
    //let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/chinesedragon.dae", VertexPositionNormal.HenzaiType, AssimpLoader.RaytracePostProcessSteps)
    //let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/sphere_centered.obj", VertexPositionNormal.HenzaiType, AssimpLoader.RaytracePostProcessSteps)
    //let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/sphere.obj", VertexPositionNormal.HenzaiType, AssimpLoader.RaytracePostProcessSteps)
    let raytracingModel = Model<VertexPositionNormal,RealtimeMaterial>.ConvertToRaytracingModel(rtModel, Rgba32.Red.ToVector4(), Vector4.Zero)
    //let transform = Matrix4x4.Identity
    let mutable transformScale = Matrix4x4.CreateScale(0.01f)
    //let mutable transformScale = Matrix4x4.CreateScale(1.0f)
    //let mutable transformRot = Matrix4x4.CreateFromYawPitchRoll(MathF.PI,0.0f,0.0f)
    let mutable transformRot = Matrix4x4.CreateFromYawPitchRoll(0.0f,0.0f,0.0f)
    let mutable transform = Matrix4x4.Multiply(transformRot, transformScale)
    transform.Translation <- Vector3(0.0f, -1.5f, -3.5f)
    let modelSurfaceList = convertModelToSurfaceList(raytracingModel, (vertexPositionNormalTransform transform), VertexRuntimeTypes.VertexPositionNormal)
    //let sceneNonBoundableArray : Surface[] = List.concat [modelSurfaceList] |> Array.ofList
    let sceneNonBoundableArray : Surface[] = List.concat [plane_floor_Unbounded;lightsNonAA] |> Array.ofList
    //let sceneList  = List.concat[triangle_scene;spheres_scene_4;light_sphere;lightsAA; modelSurfaceList ]
    let sceneList  = List.concat[light_sphere;modelSurfaceList ]

    let sceneArray : Surface[] = sceneList|> Array.ofList
    //let sceneArray : Surface[] = sceneList |> Array.ofList
    //(sceneArray, sceneNonBoundableArray)
    //(sceneArray, emptySurfaceArray)
    (sceneArray, sceneNonBoundableArray)

let constructBVHTree surfaceArray splitMethod = 
    let bvhTreeBuilder = BVHTreeBuilder<Surface>()
    bvhTreeBuilder.build surfaceArray splitMethod

  
    