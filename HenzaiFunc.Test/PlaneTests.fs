module PlaneTests

open System.Numerics
open Xunit
open HenzaiFunc.Core.RaytraceGeometry
open HenzaiFunc.Core.Acceleration
open HenzaiFunc.Core.Types


[<Fact>]
let PlaneZAABBTest() =
    let p 
        = Plane(new System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, 0.0f, 1.0f)),30.0f),Some ((Vector3(10.0f,10.0f,-30.0f))),Some 20.0f,Some 20.0f)

    let aabb = (p :> AxisAlignedBoundable).GetBounds

    Assert.Equal(Vector3(0.0f, 0.0f, -31.0f), aabb.PMin)
    Assert.Equal(Vector3(20.0f, 20.0f, -29.0f), aabb.PMax)


[<Fact>]
let PlaneYAABBTest() =
    let p 
        = Plane(new System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, 1.0f, 0.0f)),30.0f),Some ((Vector3(10.0f,10.0f,-30.0f))),Some 20.0f,Some 20.0f)

    let aabb = (p :> AxisAlignedBoundable).GetBounds

    Assert.Equal(Vector3(0.0f, -31.0f, 0.0f), aabb.PMin)
    Assert.Equal(Vector3(20.0f, -29.0f, -20.0f), aabb.PMax)


[<Fact>]
let PlaneXAABBTest() =
    let p 
        = Plane(new System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(1.0f, 0.0f, 0.0f)),30.0f),Some ((Vector3(10.0f,10.0f,-30.0f))),Some 20.0f,Some 20.0f)

    let aabb = (p :> AxisAlignedBoundable).GetBounds

    Assert.Equal(Vector3(-31.0f, 0.0f, 0.0f), aabb.PMin)
    Assert.Equal(Vector3(-29.0f, -20.0f, -20.0f), aabb.PMax)
        


[<Fact>]
let PlaneXYZAABBTest() =

    let p 
        = Plane(new System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(-1.0f, -1.0f, 1.0f)),20.0f),Some ((Henzai.Core.Numerics.Vector.CreateUnitVector3(-1.0f, -1.0f, 1.0f))*(-20.0f)),Some 40.0f,Some 13.0f)
    let aabb = (p :> AxisAlignedBoundable).GetBounds
    Assert.False(true)
