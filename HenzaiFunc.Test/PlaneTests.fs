module PlaneTests

open System.Numerics
open Xunit
open HenzaiFunc.Core.RaytraceGeometry
open Henzai.Core.Acceleration
open HenzaiFunc.Core.Types


[<Fact>]
let PlaneZAABBTest() =
    let p 
        = Plane(new System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, 0.0f, 1.0f)),30.0f), Vector3(10.0f,10.0f,-30.0f), 20.0f, 20.0f)

    let aabb = (p :> AxisAlignedBoundable).GetBounds()

    Assert.Equal(0.0f, aabb.PMin.X)
    Assert.Equal(0.0f, aabb.PMin.Y)
    Assert.True(aabb.PMin.Z < -30.0f)

    Assert.Equal(20.0f, aabb.PMax.X)
    Assert.Equal(20.0f, aabb.PMax.Y)
    Assert.True(aabb.PMax.Z > -30.0f)


[<Fact>]
let PlaneYAABBTest() =
    let p 
        = Plane(new System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, 1.0f, 0.0f)),30.0f), Vector3(10.0f,10.0f,-30.0f), 20.0f, 20.0f)

    let aabb = (p :> AxisAlignedBoundable).GetBounds()


    Assert.Equal(0.0f, aabb.PMin.X)
    Assert.Equal(-40.0f, aabb.PMin.Z)
    Assert.True(aabb.PMin.Y < 10.0f)

    Assert.Equal(20.0f, aabb.PMax.X)
    Assert.Equal(-20.0f, aabb.PMax.Z)
    Assert.True(aabb.PMax.Y > 10.0f)



[<Fact>]
let PlaneXAABBTest() =
    let p 
        = Plane(new System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(1.0f, 0.0f, 0.0f)),30.0f), Vector3(30.0f,10.0f,-30.0f), 20.0f, 20.0f)

    let aabb = (p :> AxisAlignedBoundable).GetBounds()

    Assert.Equal(-40.0f, aabb.PMin.Z)
    Assert.Equal(0.0f, aabb.PMin.Y)
    Assert.True(aabb.PMin.X < 30.0f)

    Assert.Equal(-20.0f, aabb.PMax.Z)
    Assert.Equal(20.0f, aabb.PMax.Y)
    Assert.True(aabb.PMax.X > 30.0f)
        


// Dont know how explicitly test bounds on arbitrary orientation
// As of now non AA planes are not supported
[<Fact>]
let PlaneXYZAABBTest() =

    let n = Henzai.Core.Numerics.Vector.CreateUnitVector3(-1.0f, -1.0f, 1.0f)

    let p 
     = Plane(new System.Numerics.Plane(n, 20.0f), Vector3(20.0f, 20.0f, -20.0f), 10.0f, 5.0f)
    let aabb = (p :> AxisAlignedBoundable).GetBounds

    let mutable vObj = Henzai.Core.Numerics.Vector.ToVec3(Vector4.Transform(Vector4(0.0f, 0.0f, 1.0f, 0.0f),p.Get_R_canoical_orientation))
    let vObjRounded = Henzai.Core.Numerics.Vector.RoundVec3(ref vObj, 2)

    let mutable vCanon = Henzai.Core.Numerics.Vector.ToVec3(Vector4.Transform(n, p.Get_R_orientation_canoical))
    let vCanonRounded = Henzai.Core.Numerics.Vector.RoundVec3(ref vCanon, 2)

    Assert.True(false)

[<Fact>]
let PlaneXYZTest() =
    let n = Henzai.Core.Numerics.Vector.CreateUnitVector3(-1.0f, -1.0f, 1.0f)

    let p 
     = Plane(new System.Numerics.Plane(n, 0.0f), Vector3(0.0f), 10.0f, 5.0f)
    let aabb = (p :> AxisAlignedBoundable).GetBounds

    let mutable vObj = Henzai.Core.Numerics.Vector.ToVec3(Vector4.Transform(Vector4(0.0f, 0.0f, 1.0f, 0.0f),p.Get_R_canoical_orientation))
    let vObjRounded = Henzai.Core.Numerics.Vector.RoundVec3(ref vObj, 2)

    let mutable vCanon = Henzai.Core.Numerics.Vector.ToVec3(Vector4.Transform(n, p.Get_R_orientation_canoical))
    let vCanonRounded = Henzai.Core.Numerics.Vector.RoundVec3(ref vCanon, 2)

    Assert.Equal(Vector3(-0.02f, -0.02f, 1.02f), vCanonRounded)
    Assert.Equal(Vector3(-0.58f, -0.58f, 0.6f), vObjRounded)
    //Assert.True(false)
