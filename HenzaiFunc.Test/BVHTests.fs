module BVHTests

open System.Numerics
open Xunit
open HenzaiFunc.Core.RaytraceGeometry
open HenzaiFunc.Core.Acceleration
open HenzaiFunc.Core.Types


let simpleSphereGeomertryList : AxisAlignedBoundable []  = 
    [|
        Sphere(Vector3(4.0f,-1.0f,-15.0f),3.5f);
    |]

let sphereGeomertryList : AxisAlignedBoundable []  = 
    [|
        Sphere(Vector3(4.0f,-1.0f,-15.0f),3.5f);
        Sphere(Vector3(2.0f,-4.5f,-7.0f),1.5f);
        Sphere(Vector3(10.0f,-4.5f,-7.0f),1.5f)
    |]

let overlappingSphereGeomertryList : AxisAlignedBoundable []  = 
    [|
     Sphere(Vector3(4.0f,-1.0f,-15.0f),100.0f);
     Sphere(Vector3(2.0f,-4.5f,-7.0f),1.5f);
     Sphere(Vector3(2.5f,-4.5f,-7.0f),1.5f)
    |]

[<Fact>]
let buildBVHSimpleSphereTest () =
    let (bvhTree, orderedPrimitiveList, nodeCount) = BVHTree.build simpleSphereGeomertryList SplitMethods.Middle
    Assert.Equal(1, nodeCount)
    Assert.Equal(1, orderedPrimitiveList.Length)

[<Fact>]
let buildBVHSphereTest () =
    let (bvhTree, orderedPrimitiveList, nodeCount) = BVHTree.build sphereGeomertryList SplitMethods.Middle
    Assert.Equal(5, nodeCount)
    Assert.Equal(3, orderedPrimitiveList.Length)

[<Fact>]
let buildBVHOverlappingSphereTest () =
    let (bvhTree, orderedPrimitiveList, nodeCount) = BVHTree.build overlappingSphereGeomertryList SplitMethods.Middle
    Assert.Equal(5, nodeCount)
    Assert.Equal(3, orderedPrimitiveList.Length)



