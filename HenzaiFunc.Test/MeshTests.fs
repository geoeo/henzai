module MeshTests

open System
open System.Numerics
open Xunit
open Henzai.Core.Raytracing
open HenzaiFunc.Core.Acceleration
open HenzaiFunc.Core.Types
open Henzai.Core
open Henzai.Core.VertexGeometry
open Henzai.Core.Materials
open HenzaiFunc.Core.VertexGeometry
open Raytracer.Surface.Surface
open Raytracer.Surface.SurfaceFactory
open Raytracer.Surface.SurfaceTypes
open Raytracer.Surface

let rayZNeg = Ray(Vector3(0.0f, 0.0f, 1.0f), Vector3(0.0f, 0.0f, -1.0f))
let rayZPos = Ray(Vector3(0.0f, 0.0f, -1.0f), Vector3(0.0f, 0.0f, 1.0f))

let rayXNeg = Ray(Vector3(1.0f, 0.0f, 0.0f), Vector3(-1.0f, 0.0f, 0.0f))
let rayXPos = Ray(Vector3(-1.0f, 0.0f, 0.0f), Vector3(1.0f, 0.0f, 0.0f))

let rayYNeg = Ray(Vector3(0.0f, 1.0f, 0.0f), Vector3(0.0f, -1.0f, 0.0f))
let rayYPos = Ray(Vector3(0.0f, -1.0f, 0.0f), Vector3(0.0f, 1.0f, 0.0f))

let vertexArray = 
    [|
        Vector3(0.5f,0.5f,0.5f)
        Vector3(-0.5f,-0.5f,-0.5f)
        Vector3(-0.5f,0.5f,0.5f)
        Vector3(0.5f,-0.5f,0.5f)
        Vector3(0.5f,0.5f,-0.5f)
        Vector3(-0.5f,-0.5f,0.5f)
        Vector3(-0.5f,0.5f,-0.5f)
        Vector3(0.5f,-0.5f,-0.5f)
    |]

let indexArray = 
    [|
        0, false;
        1, false;
        2, false;
        3, false;
        4, false;
        5, false;
        6, false;
        7, false;

    |]

let identityTransform = Matrix4x4.Identity

// let vertexPositionNormalTransform(transform : Matrix4x4) (v : VertexPositionNormal) = 
//     let transformedVector4 = Vector4.Transform(v.GetPosition(), transform)
//     VertexPositionNormal(transformedVector4, v)

let transformFunc = vertexPositionNormalTransform identityTransform

//TODO Assigne Id in RuntimeParametrs makes thread problems. Have to run in Debug mode

[<Fact>]
let loadMeshTest () =
    let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/Box.dae", VertexPositionNormal.HenzaiType)
    let raytracingModel = Model<VertexPositionNormal,RealtimeMaterial>.ConvertToRaytracingModel(rtModel);
    let modelSurfaceList = convertModelToSurfaceList(raytracingModel, transformFunc, VertexRuntimeTypes.VertexPositionNormal, SurfaceTypes.NoSurface)
    Assert.Equal(12, modelSurfaceList.Length)

[<Fact>]
let buildBVHBoxTest () =
    let mutable indexMap = indexArray |> Map.ofArray
    let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/Box.dae", VertexPositionNormal.HenzaiType)
    let raytracingModel = Model<VertexPositionNormal,RealtimeMaterial>.ConvertToRaytracingModel(rtModel);
    let modelSurfaceList = convertModelToSurfaceList(raytracingModel, transformFunc, VertexRuntimeTypes.VertexPositionNormal, SurfaceTypes.NoSurface)
    let surfaceArray : Surface[] = modelSurfaceList |> Array.ofList
    let (bvhTree, orderedSurfaceArray, totalNodeCount) = BVHTreeBuilder<Surface>.Build surfaceArray SplitMethods.Middle
    let bvhRuntimeArray = BVHRuntime.ConstructBVHRuntime bvhTree totalNodeCount
    let mutable nPrimitives = 0
    for node in bvhRuntimeArray do
        let nodePrimitives = node.nPrimitives
        nPrimitives <- nPrimitives + nodePrimitives

    Assert.Equal(12, nPrimitives)
    Assert.Equal(12, surfaceArray.Length)
    Assert.Equal(12, orderedSurfaceArray.Length)

[<Fact>]
let intersectBVHBoxZTest () =
    let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/Box.dae", VertexPositionNormal.HenzaiType)
    let raytracingModel = Model<VertexPositionNormal,RealtimeMaterial>.ConvertToRaytracingModel(rtModel);
    let modelSurfaceList = convertModelToSurfaceList(raytracingModel, transformFunc, VertexRuntimeTypes.VertexPositionNormal, SurfaceTypes.NoSurface)
    let surfaceArray : Surface[] = modelSurfaceList |> Array.ofList

    let (bvhTree, orderedSurfaceArray, totalNodeCount) = BVHTreeBuilder<Surface>.Build surfaceArray SplitMethods.Middle
    let bvhRuntimeArray = BVHRuntime.ConstructBVHRuntime bvhTree totalNodeCount
    let bvhTraversalStack = Array.zeroCreate bvhRuntimeArray.Length

    let struct(isHit_ZNeg, tHit_ZNeg, surfaceOption_ZNeg) = BVHRuntime.Traverse<Surface> bvhRuntimeArray orderedSurfaceArray bvhTraversalStack rayZNeg
    let struct(isHit_ZPos, tHit_ZPos, surfaceOption_ZPos) = BVHRuntime.Traverse<Surface> bvhRuntimeArray orderedSurfaceArray bvhTraversalStack rayZPos

    Assert.Equal(true, isHit_ZNeg)
    Assert.Equal(true, isHit_ZPos)

[<Fact>]
let intersectBVHBoxXTest () =
    let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/Box.dae", VertexPositionNormal.HenzaiType)
    let raytracingModel = Model<VertexPositionNormal,RealtimeMaterial>.ConvertToRaytracingModel(rtModel);
    let modelSurfaceList = convertModelToSurfaceList(raytracingModel, transformFunc, VertexRuntimeTypes.VertexPositionNormal, SurfaceTypes.NoSurface)
    let surfaceArray : Surface[] = modelSurfaceList |> Array.ofList
    let (bvhTree, orderedSurfaceArray, totalNodeCount) = BVHTreeBuilder<Surface>.Build surfaceArray SplitMethods.Middle
    let bvhRuntimeArray = BVHRuntime.ConstructBVHRuntime bvhTree totalNodeCount
    let bvhTraversalStack = Array.zeroCreate bvhRuntimeArray.Length

    let struct(isHit_XNeg, tHit_XNeg, surfaceOption_XNeg) = BVHRuntime.Traverse<Surface> bvhRuntimeArray orderedSurfaceArray bvhTraversalStack rayXNeg
    let struct(isHit_XPos, tHit_XPos, surfaceOption_XPos) = BVHRuntime.Traverse<Surface> bvhRuntimeArray orderedSurfaceArray bvhTraversalStack rayXPos

    Assert.Equal(true, isHit_XNeg)
    Assert.Equal(true, isHit_XPos)

[<Fact>]
let intersectBVHBoxYTest () =
    let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/Box.dae", VertexPositionNormal.HenzaiType)
    let raytracingModel = Model<VertexPositionNormal,RealtimeMaterial>.ConvertToRaytracingModel(rtModel);
    let modelSurfaceList = convertModelToSurfaceList(raytracingModel, transformFunc, VertexRuntimeTypes.VertexPositionNormal, SurfaceTypes.NoSurface)
    let surfaceArray : Surface[] = modelSurfaceList |> Array.ofList
    let (bvhTree, orderedSurfaceArray, totalNodeCount) = BVHTreeBuilder<Surface>.Build surfaceArray SplitMethods.Middle
    let bvhRuntimeArray = BVHRuntime.ConstructBVHRuntime bvhTree totalNodeCount
    let bvhTraversalStack = Array.zeroCreate bvhRuntimeArray.Length

    let struct(isHit_YNeg, tHit_YNeg, surfaceOption_YNeg) = BVHRuntime.Traverse<Surface> bvhRuntimeArray orderedSurfaceArray bvhTraversalStack rayXNeg
    let struct(isHit_YPos, tHit_YPos, surfaceOption_YPos) = BVHRuntime.Traverse<Surface> bvhRuntimeArray orderedSurfaceArray bvhTraversalStack rayXPos 

    Assert.Equal(true, isHit_YNeg)
    Assert.Equal(true, isHit_YPos)