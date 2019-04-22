module AABBPlaneTests

open System
open System.Numerics
open Xunit
open Henzai.Core.Acceleration
open Henzai.Core.Numerics
open Henzai.Core.Raytracing


[<Fact>]
let AABBOutsideFrustumLeft() =
    let cameraOriginWS = Vector3(0.0f, 0.0f, 0.0f)
    let lookAt = Vector3(0.0f, 0.0f, -1.0f)
    let up = Vector3.UnitY
    let fov = MathF.PI/4.0f;
    let near = 0.01f
    let far = 10.0f
    let aspectRatio = 1.0f
    let viewMatrix = Matrix4x4.CreateLookAt(cameraOriginWS, lookAt, up);
    let perspectiveProjMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fov, aspectRatio, near, far);
    let mutable viewProjectionMatrix = viewMatrix*perspectiveProjMatrix
    let pMin = Vector4(-20.0f,0.0f,-1.0f,1.0f)
    let pMax = Vector4(-19.0f,1.0f,-0.0f,1.0f)
    let aabb = AABB(pMin,pMax)
    let mutable leftPlane = Geometry.ExtractLeftPlane(&viewProjectionMatrix)
    let intersectionResult = AABBProc.PlaneIntersection(aabb,&leftPlane)


    Assert.Equal(IntersectionResult.Outside, intersectionResult)

[<Fact>]
let AABBInsideFrustumLeft() =
    let cameraOriginWS = Vector3(0.0f, 0.0f, 0.0f)
    let lookAt = Vector3(0.0f, 0.0f, -1.0f)
    let up = Vector3.UnitY
    let fov = MathF.PI/4.0f;
    let near = 0.01f
    let far = 10.0f
    let aspectRatio = 1.0f
    let viewMatrix = Matrix4x4.CreateLookAt(cameraOriginWS, lookAt, up);
    let perspectiveProjMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fov, aspectRatio, near, far);
    let mutable viewProjectionMatrix = viewMatrix*perspectiveProjMatrix
    let pMin = Vector4(19.0f,0.0f,-1.0f, 1.0f)
    let pMax = Vector4(20.0f,1.0f,-0.0f, 1.0f)
    let aabb = AABB(pMin,pMax)
    let mutable leftPlane = Geometry.ExtractLeftPlane(&viewProjectionMatrix)
    let intersectionResult = AABBProc.PlaneIntersection(aabb,&leftPlane)


    Assert.Equal(IntersectionResult.Inside, intersectionResult)


[<Fact>]
let AABBIntersectingFrustumLeft() =
    let cameraOriginWS = Vector3(0.0f, 0.0f, 0.0f)
    let lookAt = Vector3(0.0f, 0.0f, -1.0f)
    let up = Vector3.UnitY
    let fov = MathF.PI/4.0f;
    let near = 0.01f
    let far = 10.0f
    let aspectRatio = 1.0f
    let viewMatrix = Matrix4x4.CreateLookAt(cameraOriginWS, lookAt, up);
    let perspectiveProjMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fov, aspectRatio, near, far);
    let mutable viewProjectionMatrix = viewMatrix*perspectiveProjMatrix
    let pMin = Vector4(-5.0f,0.0f,-1.0f, 1.0f)
    let pMax = Vector4(5.0f,1.0f,-0.0f, 1.0f)
    let aabb = AABB(pMin, pMax)
    let mutable leftPlane = Geometry.ExtractLeftPlane(&viewProjectionMatrix)
    let intersectionResult = AABBProc.PlaneIntersection(aabb,&leftPlane)


    Assert.Equal(IntersectionResult.Intersecting, intersectionResult)


