module HenzaiFunc.Core.RaytraceGeometry.Utils

open HenzaiFunc.Core.Types
open HenzaiFunc.Core.RaytraceGeometry.Ray
open HenzaiFunc.Core.RaytraceGeometry.Hitable


/// Does the ray penetrating the surface have a t < tCompare
let IsIntersectionInfrontOf (geometry : Hitable) (ray : Ray) (tCompare : LineParameter) = 
        let (hasIntersections,t) = geometry.Intersect ray
        if hasIntersections && t > geometry.TMin then t < tCompare else false

let PointForRay (ray : Ray) (t : LineParameter) = ray.Origin + t*ray.Direction

let smallestIntersection (b,t,x) (b_new,t_new,x_new) =
    if t <= t_new then (b,t,x)
    else (b_new,t_new,x_new)

let flattenIntersection ((b,t),x) = (b,t,x)