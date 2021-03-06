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
    let intersectionResult = AABBProc.PlaneIntersection(aabb,leftPlane)

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
    let viewMatrix = Matrix4x4.CreateLookAt(cameraOriginWS, lookAt, up)
    let perspectiveProjMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fov, aspectRatio, near, far)
    let mutable viewProjectionMatrix = viewMatrix*perspectiveProjMatrix
    let pMin = Vector4(0.0f,0.0f,-1.0f, 1.0f)
    let pMax = Vector4(1.0f,1.0f,0.0f, 1.0f)
    let aabb = AABB(pMin,pMax)
    let mutable leftPlane = Geometry.ExtractLeftPlane(&viewProjectionMatrix)
    let intersectionResult = AABBProc.PlaneIntersection(aabb,leftPlane)
    Assert.Equal(IntersectionResult.Inside, intersectionResult)

[<Fact>]
let AABBInsideFrustumLeftObjectSpace() =
    let cameraOriginWS = Vector3(0.0f, 0.0f, 0.0f)
    let lookAt = Vector3(0.0f, 0.0f, -1.0f)
    let up = Vector3.UnitY
    let fov = MathF.PI/4.0f;
    let near = 0.01f
    let far = 10.0f
    let aspectRatio = 1.0f
    let translationMatrix = Matrix4x4.CreateTranslation(5.0f, 0.0f, 0.0f)
    let viewMatrix = Matrix4x4.CreateLookAt(cameraOriginWS, lookAt, up)
    let perspectiveProjMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fov, aspectRatio, near, far)

    let mutable viewProjectionMatrix = viewMatrix*perspectiveProjMatrix
    let mutable worldViewProjectionMatrix = translationMatrix*viewMatrix*perspectiveProjMatrix
    let pMin = Vector4(-5.0f,0.0f,-1.0f, 1.0f)
    let pMax = Vector4(-4.0f,1.0f,0.0f, 1.0f)
    let aabb = AABB(pMin,pMax)
    let mutable leftPlaneObjSpace = Geometry.ExtractLeftPlane(&worldViewProjectionMatrix)
    let mutable leftPlaneWorldSpace = Geometry.ExtractLeftPlane(&viewProjectionMatrix)
    let intersectionResult1 = AABBProc.PlaneIntersection(aabb,leftPlaneObjSpace)
    let intersectionResult2 = AABBProc.PlaneIntersection(aabb,leftPlaneWorldSpace)
    Assert.Equal(IntersectionResult.Inside, intersectionResult1)
    Assert.Equal(IntersectionResult.Outside, intersectionResult2)


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
    let intersectionResult = AABBProc.PlaneIntersection(aabb,leftPlane)

    Assert.Equal(IntersectionResult.Intersecting, intersectionResult)

[<Fact>]
let AABBIntersectingFarFrustumLeft() =
    let cameraOriginWS = Vector3(10.0f, 0.0f, 0.0f)
    let lookAt = Vector3(0.0f, 0.0f, -1.0f)
    let up = Vector3.UnitY
    let fov = MathF.PI/4.0f;
    let near = 0.01f
    let far = 10.0f
    let aspectRatio = 1.0f
    let viewMatrix = Matrix4x4.CreateLookAt(cameraOriginWS, lookAt, up);
    let perspectiveProjMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fov, aspectRatio, near, far);
    let mutable viewProjectionMatrix = viewMatrix*perspectiveProjMatrix
    let pMin = Vector4(5.0f,0.0f,-1.0f, 1.0f)
    let pMax = Vector4(15.0f,1.0f,-0.0f, 1.0f)
    let aabb = AABB(pMin, pMax)
    let mutable leftPlane = Geometry.ExtractLeftPlane(&viewProjectionMatrix)
    let intersectionResult = AABBProc.PlaneIntersection(aabb,leftPlane)
    
    Assert.Equal(IntersectionResult.Intersecting, intersectionResult)

[<Fact>]
let AABBInsideFrustumNearSimple() =
    let cameraOriginWS = Vector3(0.0f, 0.0f, 0.0f)
    let lookAt = Vector3(0.0f, 0.0f, -1.0f)
    let up = Vector3.UnitY
    let fov = MathF.PI/4.0f;
    let near = 0.1f
    let far = 10.0f
    let aspectRatio = 1.0f
    let translationMatrix = Matrix4x4.CreateTranslation(0.0f, 0.0f, 0.0f)
    let viewMatrix = Matrix4x4.CreateLookAt(cameraOriginWS, lookAt, up)
    let perspectiveProjMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fov, aspectRatio, near, far)

    let mutable viewProjectionMatrix = viewMatrix*perspectiveProjMatrix
    let mutable worldViewProjectionMatrix = translationMatrix*viewMatrix*perspectiveProjMatrix
    let pMin = Vector4(0.0f,0.0f,-1.0f, 1.0f)
    let pMax = Vector4(1.0f,1.0f,0.0f, 1.0f)
    let aabb = AABB(pMin,pMax)

    let mutable leftPlaneObjSpace = Geometry.ExtractLeftPlane(&worldViewProjectionMatrix)
    let intersectionResult1 = AABBProc.PlaneIntersection(aabb,leftPlaneObjSpace)
    Assert.Equal(IntersectionResult.Inside, intersectionResult1)

[<Fact>]
let AABBInsideFrustumNear_LookingDown() =
    let cameraOriginWS = Vector3(0.0f, 2.0f, 0.0f)
    let lookAt = Vector3(0.0f, -1.0f, 0.0f)
    let up = -Vector3.UnitZ
    let fov = MathF.PI/4.0f;
    let near = 0.1f
    let far = 10.0f
    let aspectRatio = 1.0f
    let translationMatrix = Matrix4x4.CreateTranslation(0.0f, 0.0f, 0.0f)
    let viewMatrix = Matrix4x4.CreateLookAt(cameraOriginWS, lookAt, up)
    let perspectiveProjMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fov, aspectRatio, near, far)

    let mutable worldViewProjectionMatrix = translationMatrix*viewMatrix*perspectiveProjMatrix
    let pMin = Vector4(0.0f,0.0f,-1.0f, 1.0f)
    let pMax = Vector4(1.0f,1.0f,0.0f, 1.0f)
    let aabb = AABB(pMin,pMax)

    let mutable leftPlaneObjSpace = Geometry.ExtractLeftPlane(&worldViewProjectionMatrix)
    let intersectionResult1 = AABBProc.PlaneIntersection(aabb,leftPlaneObjSpace)
    Assert.Equal(IntersectionResult.Inside, intersectionResult1)


[<Fact>]
let AABBInsideFrustumNear_LookingDownIntersect() =
    let cameraOriginWS = Vector3(0.0f, 2.0f, 0.0f)
    let lookAt = Vector3(0.0f, -1.0f, 0.0f)
    let up = -Vector3.UnitZ
    let fov = MathF.PI/4.0f;
    let near = 0.1f
    let far = 10.0f
    let aspectRatio = 1.0f
    let translationMatrix = Matrix4x4.CreateTranslation(0.0f, 1.5f, 0.0f)
    let viewMatrix = Matrix4x4.CreateLookAt(cameraOriginWS, lookAt, up)
    let perspectiveProjMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fov, aspectRatio, near, far)

    let mutable worldViewProjectionMatrix = translationMatrix*viewMatrix*perspectiveProjMatrix
    let pMin = Vector4(0.0f,0.0f,-1.0f, 1.0f)
    let pMax = Vector4(1.0f,1.0f,0.0f, 1.0f)
    let aabb = AABB(pMin,pMax)

    let mutable leftPlaneObjSpace = Geometry.ExtractLeftPlane(&worldViewProjectionMatrix)
    let intersectionResult1 = AABBProc.PlaneIntersection(aabb,leftPlaneObjSpace)
    Assert.Equal(IntersectionResult.Intersecting, intersectionResult1)

[<Fact>]
let AABBInsideFrustumNear_LookingDownOutside() =
    let cameraOriginWS = Vector3(0.0f, 2.0f, 0.0f)
    let lookAt = Vector3(0.0f, -1.0f, 0.0f)
    let up = -Vector3.UnitZ
    let fov = MathF.PI/4.0f;
    let near = 0.1f
    let far = 10.0f
    let aspectRatio = 1.0f
    let translationMatrix = Matrix4x4.CreateTranslation(0.0f, 2.5f, 0.0f)
    let viewMatrix = Matrix4x4.CreateLookAt(cameraOriginWS, lookAt, up)
    let perspectiveProjMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fov, aspectRatio, near, far)

    let mutable worldViewProjectionMatrix = translationMatrix*viewMatrix*perspectiveProjMatrix
    let pMin = Vector4(0.0f,0.0f,-1.0f, 1.0f)
    let pMax = Vector4(1.0f,1.0f,0.0f, 1.0f)
    let aabb = AABB(pMin,pMax)

    let mutable leftPlaneObjSpace = Geometry.ExtractLeftPlane(&worldViewProjectionMatrix)
    let intersectionResult1 = AABBProc.PlaneIntersection(aabb,leftPlaneObjSpace)
    Assert.Equal(IntersectionResult.Intersecting, intersectionResult1)

[<Fact>]
let AABBInsideFrustumFar_LookingDown() =
    let cameraOriginWS = Vector3(0.0f, 2.0f, 0.0f)
    let lookAt = Vector3(0.0f, -1.0f, 0.0f)
    let up = -Vector3.UnitZ
    let fov = MathF.PI/4.0f;
    let near = 0.1f
    let far = 10.0f
    let aspectRatio = 1.0f
    let translationMatrix = Matrix4x4.CreateTranslation(0.0f, 0.0f, 0.0f)
    let viewMatrix = Matrix4x4.CreateLookAt(cameraOriginWS, lookAt, up)
    let perspectiveProjMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fov, aspectRatio, near, far)

    let mutable worldViewProjectionMatrix = translationMatrix*viewMatrix*perspectiveProjMatrix
    let pMin = Vector4(0.0f,0.0f,-1.0f, 1.0f)
    let pMax = Vector4(1.0f,1.0f,0.0f, 1.0f)
    let aabb = AABB(pMin,pMax)

    let mutable leftPlaneObjSpace = Geometry.ExtractLeftPlane(&worldViewProjectionMatrix)
    let intersectionResult1 = AABBProc.PlaneIntersection(aabb,leftPlaneObjSpace)
    Assert.Equal(IntersectionResult.Inside, intersectionResult1)

[<Fact>]
let AABBInsideFrustumFar_LookingDownOutside() =
    let cameraOriginWS = Vector3(0.0f, 2.0f, 0.0f)
    let lookAt = Vector3(0.0f, -1.0f, 0.0f)
    let up = -Vector3.UnitZ
    let fov = MathF.PI/4.0f;
    let near = 0.1f
    let far = 10.0f
    let aspectRatio = 1.0f
    let translationMatrix = Matrix4x4.CreateTranslation(0.0f, -11.0f, 0.0f)
    let viewMatrix = Matrix4x4.CreateLookAt(cameraOriginWS, lookAt, up)
    let perspectiveProjMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fov, aspectRatio, near, far)

    let mutable worldViewProjectionMatrix = translationMatrix*viewMatrix*perspectiveProjMatrix
    let pMin = Vector4(0.0f,0.0f,-1.0f, 1.0f)
    let pMax = Vector4(1.0f,1.0f,0.0f, 1.0f)
    let aabb = AABB(pMin,pMax)

    let mutable leftPlaneObjSpace = Geometry.ExtractLeftPlane(&worldViewProjectionMatrix)
    let intersectionResult1 = AABBProc.PlaneIntersection(aabb,leftPlaneObjSpace)
    Assert.Equal(IntersectionResult.Inside, intersectionResult1)


