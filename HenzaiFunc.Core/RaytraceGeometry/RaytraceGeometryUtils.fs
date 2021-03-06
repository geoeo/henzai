namespace HenzaiFunc.Core.RaytraceGeometry

open System.Numerics
open HenzaiFunc.Core.Types
open Henzai.Core.Raytracing


module RaytraceGeometryUtils = 

    /// Does the ray penetrating the surface have a t < tCompare
    let IsIntersectionInfrontOf (geometry : Hitable) (ray : Ray) (tCompare : LineParameter) = 
            let struct(hasIntersections,t) = geometry.Intersect ray
            if hasIntersections && t > geometry.TMin() then t < tCompare else false

    let PointForRay (ray : Ray) (t : LineParameter) = ray.Origin + t*ray.Direction

    let smallestIntersection (b,t,x) (b_new, t_new, x_new) =
        if t <= t_new then (b, t, x)
        else (b_new, t_new, x_new)

    let flattenIntersection ((b, t), x) = (b, t, x)
    
    /// Space inwhich points are compared if they are inside a rectangle
    /// Plane is XY
    // TODO: replace "mutable" with "in" when available
    let mutable CanonicalPlaneSpace = Vector4(0.0f, 0.0f, -1.0f, 0.0f)

    // let robustRayBounds value gamma = value * (1.0f + 2.0f*gamma)

    // let gamma (n : int) = (float32(n)*System.Single.Epsilon)/(1.0f - float32(n)*System.Single.Epsilon)
    
        