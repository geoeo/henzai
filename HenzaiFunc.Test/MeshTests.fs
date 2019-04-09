module MeshTests

open System
open System.Numerics
open Xunit
open HenzaiFunc.Core.RaytraceGeometry
open HenzaiFunc.Core.Acceleration
open HenzaiFunc.Core.Types
open Henzai.Core
open Henzai.Core.VertexGeometry
open Henzai.Core.Materials
open HenzaiFunc.Core.RaytraceGeometry
open Raytracer.Scene.Builder
open Raytracer.Surface

[<Fact>]
let loadMeshTest () =
    let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/Box.dae", VertexPositionNormal.HenzaiType)
    let raytracingModel = Model<VertexPositionNormal,RealtimeMaterial>.ConvertToRaytracingModel(rtModel);
    let modelSurfaceList = convertModelToSurfaceList raytracingModel
    Assert.Equal(12, modelSurfaceList.Length)

[<Fact>]
let buildBVHBoxTest () =
    let bvhTreeBuilder = BVHTreeBuilder<Surface>()
    let bvhRuntime = BVHRuntime<Surface>()
    let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/Box.dae", VertexPositionNormal.HenzaiType)
    let raytracingModel = Model<VertexPositionNormal,RealtimeMaterial>.ConvertToRaytracingModel(rtModel);
    let modelSurfaceList = convertModelToSurfaceList raytracingModel
    let surfaceArray : Surface[] = modelSurfaceList |> Array.ofList
    let (bvhTree, orderedSurfaceArray, totalNodeCount) = bvhTreeBuilder.build surfaceArray SplitMethods.Middle
    let bvhRuntimeArray = bvhRuntime.constructBVHRuntime bvhTree totalNodeCount
    let mutable nPrimitives = 0
    for node in bvhRuntimeArray do
        nPrimitives <- nPrimitives + node.nPrimitives

    Assert.Equal(12, nPrimitives)

[<Fact>]
let intersectBVHBoxTest () =
    let bvhTreeBuilder = BVHTreeBuilder<Surface>()
    let bvhRuntime = BVHRuntime<Surface>()
    let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/Box.dae", VertexPositionNormal.HenzaiType)
    let raytracingModel = Model<VertexPositionNormal,RealtimeMaterial>.ConvertToRaytracingModel(rtModel);
    let modelSurfaceList = convertModelToSurfaceList raytracingModel
    let surfaceArray : Surface[] = modelSurfaceList |> Array.ofList
    let (bvhTree, orderedSurfaceArray, totalNodeCount) = bvhTreeBuilder.build surfaceArray SplitMethods.Middle
    let bvhRuntimeArray = bvhRuntime.constructBVHRuntime bvhTree totalNodeCount
    let bvhTraversalStack = Array.zeroCreate bvhRuntimeArray.Length

    let rayZNeg = Ray(Vector3(0.0f, 0.0f, 1.0f), Vector3(0.0f, 0.0f, -1.0f))
    let rayZPos = Ray(Vector3(0.0f, 0.0f, -1.0f), Vector3(0.0f, 0.0f, 1.0f))

    let rayXNeg = Ray(Vector3(1.0f, 0.0f, 0.0f), Vector3(-1.0f, 0.0f, 0.0f))
    let rayXPos = Ray(Vector3(-1.0f, 0.0f, 0.0f), Vector3(1.0f, 0.0f, 0.0f))

    let rayYNeg = Ray(Vector3(0.0f, 1.0f, 0.0f), Vector3(0.0f, -1.0f, 0.0f))
    let rayYPos = Ray(Vector3(0.0f, -1.0f, 0.0f), Vector3(0.0f, 1.0f, 0.0f))

    let struct(isHit_ZNeg, tHit_ZNeg, surfaceOption_ZNeg) = bvhRuntime.traverse bvhRuntimeArray orderedSurfaceArray bvhTraversalStack rayZNeg
    let struct(isHit_ZPos, tHit_ZPos, surfaceOption_ZPos) = bvhRuntime.traverse bvhRuntimeArray orderedSurfaceArray bvhTraversalStack rayZPos

    let struct(isHit_XNeg, tHit_XNeg, surfaceOption_XNeg) = bvhRuntime.traverse bvhRuntimeArray orderedSurfaceArray bvhTraversalStack rayXNeg
    let struct(isHit_XPos, tHit_XPos, surfaceOption_XPos) = bvhRuntime.traverse bvhRuntimeArray orderedSurfaceArray bvhTraversalStack rayXPos

    let struct(isHit_YNeg, tHit_YNeg, surfaceOption_YNeg) = bvhRuntime.traverse bvhRuntimeArray orderedSurfaceArray bvhTraversalStack rayYNeg
    let struct(isHit_YPos, tHit_YPos, surfaceOption_YPos) = bvhRuntime.traverse bvhRuntimeArray orderedSurfaceArray bvhTraversalStack rayYPos

    Assert.Equal(true, isHit_ZNeg)
    Assert.Equal(true, isHit_ZPos)

    Assert.Equal(true, isHit_XNeg)
    Assert.Equal(true, isHit_XPos)

    Assert.Equal(true, isHit_YNeg)
    Assert.Equal(true, isHit_YPos)