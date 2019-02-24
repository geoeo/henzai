module BVHPlaneTests

open System.Numerics
open Xunit
open HenzaiFunc.Core.RaytraceGeometry
open HenzaiFunc.Core.Acceleration
open HenzaiFunc.Core.Types


let planeGeomertryArray : RaytracingGeometry []  = 
    [|
        Plane(new System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(-1.0f, -1.0f, 1.0f)),20.0f),Some ((Henzai.Core.Numerics.Vector.CreateUnitVector3(-1.0f, -1.0f, 1.0f))*(-20.0f)),Some 40.0f,Some 13.0f);
        Plane(new System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, 0.0f, 1.0f)),30.0f),Some ((Vector3(10.0f,10.0f,-30.0f))),Some 25.0f,Some 20.0f)
    |]


[<Fact>]
let buildBVHPlaneTest () =
    let bvhTreeBuilder = BVHTreeBuilder<RaytracingGeometry>() 
    let (bvhTree, orderedPrimitiveList, nodeCount) = bvhTreeBuilder.build planeGeomertryArray SplitMethods.Middle
    let (value, left, right) = BVHTree.decompose bvhTree
    Assert.Equal(3, nodeCount)
    Assert.Equal(2, orderedPrimitiveList.Length)