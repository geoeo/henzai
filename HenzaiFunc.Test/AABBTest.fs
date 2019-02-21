module AABBTests

open System.Numerics
open Xunit
open HenzaiFunc.Core.RaytraceGeometry

[<Fact>]
let AABBRayIntersectionTest() = 
    let aabb = AABB(Vector3(0.0f, 0.0f, -1.0f), Vector3(1.0f, 1.0f, 0.0f))
    let ray = Ray(Vector3(0.5f, 0.5f, 1.0f), Vector3(0.0f, 0.0f, -1.0f))
    let (hasIntersection, tHit) = aabb.AsHitable.Intersect ray
    Assert.True(hasIntersection)
    
