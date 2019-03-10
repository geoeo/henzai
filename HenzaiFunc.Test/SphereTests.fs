module SphereTests

open System.Numerics
open Xunit
open HenzaiFunc.Core.RaytraceGeometry
open HenzaiFunc.Core.Acceleration
open HenzaiFunc.Core.Types


[<Fact>]
let unitSphereBoundingTest () =
    let unitSphere = Sphere(Vector3(0.0f,0.0f,0.0f),1.0f)
    let aabb = unitSphere.AsBoundable.GetBounds
    Assert.Equal(Vector4(-1.0f,-1.0f,-1.0f, 1.0f), aabb.PMin)
    Assert.Equal(Vector4(1.0f,1.0f,1.0f, 1.0f), aabb.PMax)

[<Fact>]
let sphereBoundingTest () =
    let unitSphere = Sphere(Vector3(5.0f,5.0f,-5.0f),1.0f)
    let aabb = unitSphere.AsBoundable.GetBounds
    Assert.Equal(Vector4(4.0f,4.0f,-6.0f, 1.0f), aabb.PMin)
    Assert.Equal(Vector4(6.0f,6.0f,-4.0f, 1.0f), aabb.PMax)
