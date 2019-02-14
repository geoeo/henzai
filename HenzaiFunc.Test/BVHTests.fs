module BVHTests

open System.Numerics
open Xunit
open HenzaiFunc.Core.RaytraceGeometry
open HenzaiFunc.Core.Acceleration
open HenzaiFunc.Core.Types


let simpleSphereGeomertryList : AxisAlignedBoundable []  = 
    [|
        Sphere(Vector3(4.0f,-1.0f,-15.0f),3.5f);
        Sphere(Vector3(2.0f,-4.5f,-7.0f),1.5f);
        Sphere(Vector3(10.0f,-4.5f,-7.0f),1.5f)
    |]

[<Fact>]
let buildBVHTest () =
    let (bvhTree, orderedPrimitiveList, nodeCount) = BVHTree.build simpleSphereGeomertryList SplitMethods.Middle
    Assert.Equal(5, nodeCount)
    Assert.Equal(3, orderedPrimitiveList.Length)


