module BVHTests

open System.Numerics
open Xunit
open HenzaiFunc.Core.RaytraceGeometry
open HenzaiFunc.Core.Acceleration
open HenzaiFunc.Core.Types


let simpleSphereGeomertryArray : AxisAlignedBoundable []  = 
    [|
        Sphere(Vector3(4.0f,-1.0f,-15.0f),3.5f);
    |]

let sphereGeomertryArray : AxisAlignedBoundable []  = 
    [|
        Sphere(Vector3(4.0f,-1.0f,-15.0f),3.5f);
        Sphere(Vector3(2.0f,-4.5f,-7.0f),1.5f);
        Sphere(Vector3(10.0f,-4.5f,-7.0f),1.5f)
    |]

let overlappingSphereGeomertryArray : AxisAlignedBoundable []  = 
    [|
     Sphere(Vector3(4.0f,-1.0f,-15.0f),100.0f);
     Sphere(Vector3(2.0f,-4.5f,-7.0f),1.5f);
     Sphere(Vector3(2.5f,-4.5f,-7.0f),1.5f)
    |]

let identicalSphereGeomertryArray : AxisAlignedBoundable []  = 
    [|
     Sphere(Vector3(2.0f,-4.5f,-7.0f),1.5f);
     Sphere(Vector3(2.0f,-4.5f,-7.0f),1.5f);
     Sphere(Vector3(2.0f,-4.5f,-7.0f),1.5f)
    |]

[<Fact>]
let buildBVHSimpleSphereTest () =
    let (bvhTree, orderedPrimitiveList, nodeCount) = BVHTree.build simpleSphereGeomertryArray SplitMethods.Middle
    Assert.Equal(1, nodeCount)
    Assert.Equal(1, orderedPrimitiveList.Length)

[<Fact>]
let buildBVHSphereTest () =
    let (bvhTree, orderedPrimitiveList, nodeCount) = BVHTree.build sphereGeomertryArray SplitMethods.Middle
    Assert.Equal(5, nodeCount)
    Assert.Equal(3, orderedPrimitiveList.Length)

[<Fact>]
let buildBVHOverlappingSphereTest () =
    let (bvhTree, orderedPrimitiveList, nodeCount) = BVHTree.build overlappingSphereGeomertryArray SplitMethods.Middle
    Assert.Equal(5, nodeCount)
    Assert.Equal(3, orderedPrimitiveList.Length)

[<Fact>]
let buildBVHIdenticalSphereTest () =
    let (bvhTree, orderedPrimitiveList, nodeCount) = BVHTree.build identicalSphereGeomertryArray SplitMethods.Middle
    Assert.Equal(1, nodeCount)
    Assert.Equal(3, orderedPrimitiveList.Length)

[<Fact>]
let flattenBVHSimpleSphereTest () =
    let (bvhTree, orderedPrimitiveList, nodeCount) = BVHTree.build simpleSphereGeomertryArray SplitMethods.Middle
    let bvhArray = BVHRuntime.allocateMemoryForBVHRuntime nodeCount
    let runtimeNodeCount = BVHRuntime.flattenBVHTree bvhTree bvhArray 0 CoordinateSystem.XYNegZ
    let bvhRuntimeNode = bvhArray.[0]
    let leafNode = bvhRuntimeNode.leafNode 
    let primitive = sphereGeomertryArray.[leafNode.primitivesOffset]
    let sphere = primitive :?> Sphere
    Assert.Equal(Vector3(4.0f,-1.0f,-15.0f), sphere.Center)


[<Fact>]
let flattenBVHSphereTest () =
    let (bvhTree, orderedPrimitiveList, nodeCount) = BVHTree.build sphereGeomertryArray SplitMethods.Middle
    let bvhArray = BVHRuntime.allocateMemoryForBVHRuntime nodeCount
    let out = BVHRuntime.flattenBVHTree bvhTree bvhArray 0 CoordinateSystem.XYNegZ
    Assert.Equal(5, nodeCount)
    


