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

let standardSphereGeomertryArray : AxisAlignedBoundable []  = 
    [|
        Sphere(Vector3(4.0f,-1.0f,-15.0f),3.5f);
        Sphere(Vector3(10.0f,-4.5f,-7.0f),1.5f);
        Sphere(Vector3(2.0f,-4.5f,-7.0f),1.5f)
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
let buildBVHStandardSphereTest () =
    let (bvhTree, orderedPrimitiveList, nodeCount) = BVHTree.build standardSphereGeomertryArray SplitMethods.Middle
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
    let out = BVHRuntime.flattenBVHTree bvhTree bvhArray 0 CoordinateSystem.XYNegZ
    let bvhRuntimeNode = bvhArray.[0]
    let leafNode = bvhRuntimeNode.leafNode 
    let primitive = simpleSphereGeomertryArray.[leafNode.primitivesOffset]
    let sphere = primitive :?> Sphere
    Assert.Equal(Vector3(4.0f,-1.0f,-15.0f), sphere.Center)
    Assert.Equal(1, nodeCount)


[<Fact>]
let flattenBVHSphereTest () =
    let (bvhTree, orderedPrimitiveArray, nodeCount) = BVHTree.build standardSphereGeomertryArray SplitMethods.Middle
    let bvhArray = BVHRuntime.allocateMemoryForBVHRuntime nodeCount
    let out = BVHRuntime.flattenBVHTree bvhTree bvhArray 0 CoordinateSystem.XYNegZ
    let bvhRuntimeNode_Zero = bvhArray.[0]
    let bvhRuntimeNode_One = bvhArray.[1]
    let bvhRuntimeNode_Two = bvhArray.[2]
    let bvhRuntimeNode_Three = bvhArray.[3]
    let bvhRuntimeNode_Four = bvhArray.[4]
    let leafNode_Zero = bvhRuntimeNode_Two.leafNode
    let leafNode_One = bvhRuntimeNode_Three.leafNode
    let leafNode_Two = bvhRuntimeNode_Four.leafNode
    let primitive_Zero = orderedPrimitiveArray.[leafNode_Zero.primitivesOffset]
    let primitive_One = orderedPrimitiveArray.[leafNode_One.primitivesOffset]
    let primitive_Two = orderedPrimitiveArray.[leafNode_Two.primitivesOffset]
    let sphere_Zero = primitive_Zero :?> Sphere
    let sphere_One = primitive_One :?> Sphere
    let sphere_Two = primitive_Two :?> Sphere
    Assert.Equal(5, nodeCount)

    Assert.Equal(bvhRuntimeNode_Zero.nPrimitives, 0)
    Assert.Equal(bvhRuntimeNode_One.nPrimitives, 0)
    Assert.Equal(bvhRuntimeNode_Two.nPrimitives, 1)
    Assert.Equal(bvhRuntimeNode_Three.nPrimitives, 1)
    Assert.Equal(bvhRuntimeNode_Four.nPrimitives, 1)

    Assert.Equal(Vector3(2.0f,-4.5f,-7.0f), sphere_Zero.Center)
    Assert.Equal(Vector3(10.0f,-4.5f,-7.0f), sphere_One.Center)  
    Assert.Equal(Vector3(4.0f,-1.0f,-15.0f), sphere_Two.Center)

    


