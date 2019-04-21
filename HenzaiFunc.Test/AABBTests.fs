module AABBTests

open System.Numerics
open Xunit
open Henzai.Core.Acceleration
//open HenzaiFunc.Core.RaytraceGeometry
open Henzai.Core.Raytracing


[<Fact>]
let SimpleAABBRayIntersectionTestZFront() = 
    let aabb = new Henzai.Core.Acceleration.AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    //let aabb = new AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(0.5f, 0.5f, 1.0f), Vector3(0.0f, 0.0f, -1.0f))
    let struct(hasIntersection, tHit) = aabb.Intersect(ray)
    //let struct(hasIntersection, tHit) = aabb.AsHitable.Intersect(ray)
    Assert.True(hasIntersection)
    Assert.Equal(1.0f,tHit)


[<Fact>]
let SimpleAABBRayIntersectionTestZFront2() = 
    let aabb = new AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(0.5f, 0.5f, 2.0f), Vector3(0.0f, 0.0f, -1.0f))
    let struct(hasIntersection, tHit) = aabb.Intersect(ray)
    //let struct(hasIntersection, tHit) = aabb.AsHitable.Intersect(ray)
    Assert.True(hasIntersection)
    Assert.Equal(2.0f,tHit)


[<Fact>]
let SimpleAABBRayIntersectionTestZBack3() = 
    let aabb = AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(0.5f, 0.5f, -1.5f), Vector3(0.0f, 0.0f, 1.0f))
    let struct(hasIntersection, tHit) = aabb.Intersect(ray)
    //let struct(hasIntersection, tHit) =aabb.AsHitable.Intersect(ray)
    Assert.True(hasIntersection)
    Assert.Equal(0.5f,tHit)


[<Fact>]
let InsideAABBRayIntersectionTestZ() = 
    let aabb = new AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(0.5f, 0.5f, -0.5f), Vector3(0.0f, 0.0f, -1.0f))
    let struct(hasIntersection, tHit) = aabb.Intersect(ray)
    //let struct(hasIntersection, tHit) = aabb.AsHitable.Intersect(ray)
    Assert.True(hasIntersection)
    Assert.Equal(-0.5f,tHit)


[<Fact>]
let InsideAABBRayIntersectionTestZ2() = 
    let aabb = new AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(0.5f, 0.5f, -0.5f), Vector3(0.0f, 0.0f, 1.0f))
    let struct(hasIntersection, tHit) = aabb.Intersect(ray)
    //let struct(hasIntersection, tHit) =aabb.AsHitable.Intersect(ray)
    Assert.True(hasIntersection)
    Assert.Equal(-0.5f,tHit)


[<Fact>]
let NoAABBRayIntersectionTestZ() = 
    let aabb = new AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(0.5f, 0.5f, 1.0f), Vector3(0.0f, 0.0f, 1.0f))
    let struct(hasIntersection, tHit) = aabb.Intersect(ray)
    //let struct(hasIntersection, tHit) =aabb.AsHitable.Intersect(ray)
    Assert.False(hasIntersection)

//////

[<Fact>]
let SimpleAABBRayIntersectionTestXFront() = 
    let aabb = new AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(2.0f, 0.5f, -0.5f), Vector3(-1.0f, 0.0f, 0.0f))
    let struct(hasIntersection, tHit) = aabb.Intersect(ray)
    //let struct(hasIntersection, tHit) = aabb.AsHitable.Intersect(ray)
    Assert.True(hasIntersection)
    Assert.Equal(1.0f,tHit)


[<Fact>]
let SimpleAABBRayIntersectionTestXFront2() = 
    let aabb = new AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(3.0f, 0.5f, -0.5f), Vector3(-1.0f, 0.0f, 0.0f))
    let struct(hasIntersection, tHit) = aabb.Intersect(ray)
    //let struct(hasIntersection, tHit) =aabb.AsHitable.Intersect(ray)
    Assert.True(hasIntersection)
    Assert.Equal(2.0f,tHit)


[<Fact>]
let SimpleAABBRayIntersectionTestXBack3() = 
    let aabb = new AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(-0.5f, 0.5f, -0.5f), Vector3(1.0f, 0.0f, 0.0f))
    let struct(hasIntersection, tHit) = aabb.Intersect(ray)
    //let struct(hasIntersection, tHit) =aabb.AsHitable.Intersect(ray)
    Assert.True(hasIntersection)
    Assert.Equal(0.5f,tHit)


[<Fact>]
let InsideAABBRayIntersectionTestX() = 
    let aabb = new AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(0.5f, 0.5f, -0.5f), Vector3(-1.0f, 0.0f, 0.0f))
    let struct(hasIntersection, tHit) = aabb.Intersect(ray)
    //let struct(hasIntersection, tHit) = aabb.AsHitable.Intersect(ray)
    Assert.True(hasIntersection)
    Assert.Equal(-0.5f,tHit)


[<Fact>]
let InsideAABBRayIntersectionTestX2() = 
    let aabb = new AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(0.5f, 0.5f, -0.5f), Vector3(1.0f, 0.0f, 0.0f))
    let struct(hasIntersection, tHit) = aabb.Intersect(ray)
    //let struct(hasIntersection, tHit) = aabb.AsHitable.Intersect(ray)
    Assert.True(hasIntersection)
    Assert.Equal(-0.5f,tHit)


[<Fact>]
let NoAABBRayIntersectionTestX() = 
    let aabb = new AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(1.5f, 0.5f, 0.5f), Vector3(1.0f, 0.0f, 0.0f))
    let struct(hasIntersection, tHit) = aabb.Intersect(ray)
    //let struct(hasIntersection, tHit) =aabb.AsHitable.Intersect(ray)
    Assert.False(hasIntersection)


/////

[<Fact>]
let SimpleAABBRayIntersectionTestYFront() = 
    let aabb = new AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(0.5f, 2.0f, -0.5f), Vector3(0.0f, -1.0f, 0.0f))
    let struct(hasIntersection, tHit) = aabb.Intersect(ray)
    //let struct(hasIntersection, tHit) = aabb.AsHitable.Intersect(ray)
    Assert.True(hasIntersection)
    Assert.Equal(1.0f,tHit)


[<Fact>]
let SimpleAABBRayIntersectionTestYFront2() = 
    let aabb = new AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(0.5f, 3.0f, -0.5f), Vector3(0.0f, -1.0f, 0.0f))
    let struct(hasIntersection, tHit) = aabb.Intersect(ray)
    //let struct(hasIntersection, tHit) =aabb.AsHitable.Intersect(ray)
    Assert.True(hasIntersection)
    Assert.Equal(2.0f,tHit)


[<Fact>]
let SimpleAABBRayIntersectionTestYBack3() = 
    let aabb = new AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(0.5f, -0.5f, -0.5f), Vector3(0.0f, 1.0f, 0.0f))
    let struct(hasIntersection, tHit) = aabb.Intersect(ray)
    //let struct(hasIntersection, tHit) = aabb.AsHitable.Intersect(ray)
    Assert.True(hasIntersection)
    Assert.Equal(0.5f,tHit)


[<Fact>]
let InsideAABBRayIntersectionTestY() = 
    let aabb = new AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(0.5f, 0.5f, -0.5f), Vector3(0.0f, -1.0f, 0.0f))
    let struct(hasIntersection, tHit) = aabb.Intersect(ray)
    //let struct(hasIntersection, tHit) = aabb.AsHitable.Intersect(ray)
    Assert.True(hasIntersection)
    Assert.Equal(-0.5f,tHit)


[<Fact>]
let InsideAABBRayIntersectionTestY2() = 
    let aabb = new AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(0.5f, 0.5f, -0.5f), Vector3(0.0f, 1.0f, 0.0f))
    let struct(hasIntersection, tHit) = aabb.Intersect(ray)
    //let struct(hasIntersection, tHit) = aabb.AsHitable.Intersect(ray)
    Assert.True(hasIntersection)
    Assert.Equal(-0.5f,tHit)


[<Fact>]
let NoAABBRayIntersectionTestY() = 
    let aabb = new AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(0.5f, 1.5f, 0.5f), Vector3(0.0f, 1.0f, 0.0f))
    let struct(hasIntersection, tHit) = aabb.Intersect(ray)
    //let struct(hasIntersection, tHit) = aabb.AsHitable.Intersect(ray)
    Assert.False(hasIntersection)


//////

[<Fact>]
let AABBRayIntersectionTestXYZ() = 
    let aabb = new AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(1.1f, 1.1f, -1.1f), Vector3(-1.0f, -1.0f, 1.0f))
    let struct(hasIntersection, tHit) = aabb.Intersect(ray)
    //let struct(hasIntersection, tHit) = aabb.AsHitable.Intersect(ray)
    Assert.True(hasIntersection)


