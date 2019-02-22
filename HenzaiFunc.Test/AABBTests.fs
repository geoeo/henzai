module AABBTests

open System.Numerics
open Xunit
open HenzaiFunc.Core.RaytraceGeometry

[<Fact>]
let SimpleAABBRayIntersectionTest() = 
    let aabb = AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(0.5f, 0.5f, 1.0f), Vector3(0.0f, 0.0f, -1.0f))
    let (hasIntersection, tHit) = aabb.AsHitable.Intersect ray
    Assert.True(hasIntersection)
    Assert.Equal(1.0f,tHit)


[<Fact>]
let SimpleAABBRayIntersectionTest2() = 
    let aabb = AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(0.5f, 0.5f, 2.0f), Vector3(0.0f, 0.0f, -1.0f))
    let (hasIntersection, tHit) = aabb.AsHitable.Intersect ray
    Assert.True(hasIntersection)
    Assert.Equal(2.0f,tHit)


[<Fact>]
let SimpleAABBRayIntersectionTest3() = 
    let aabb = AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(0.5f, 0.5f, -1.5f), Vector3(0.0f, 0.0f, 1.0f))
    let (hasIntersection, tHit) = aabb.AsHitable.Intersect ray
    Assert.True(hasIntersection)
    Assert.Equal(0.5f,tHit)


[<Fact>]
let InsideAABBRayIntersectionTest() = 
    let aabb = AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(0.5f, 0.5f, -0.5f), Vector3(0.0f, 0.0f, -1.0f))
    let (hasIntersection, tHit) = aabb.AsHitable.Intersect ray
    Assert.True(hasIntersection)
    Assert.Equal(0.5f,tHit)


[<Fact>]
let InsideAABBRayIntersectionTest2() = 
    let aabb = AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(0.5f, 0.5f, -0.5f), Vector3(0.0f, 0.0f, 1.0f))
    let (hasIntersection, tHit) = aabb.AsHitable.Intersect ray
    Assert.True(hasIntersection)
    Assert.Equal(0.5f,tHit)


[<Fact>]
let NoAABBRayIntersectionTest() = 
    let aabb = AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(0.5f, 0.5f, 1.0f), Vector3(0.0f, 0.0f, 1.0f))
    let (hasIntersection, tHit) = aabb.AsHitable.Intersect ray
    Assert.False(hasIntersection)

