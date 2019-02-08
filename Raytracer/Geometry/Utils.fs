module Raytracer.Geometry.Utils

open Raytracer.Geometry.Types
open Raytracer.Geometry.Ray
open Raytracer.Geometry.Hitable


/// Does the ray penetrating the surface have a t < tCompare
let IsIntersectionInfrontOf (geometry : Hitable) (ray : Ray) (tCompare : LineParameter) = 
        let (hasIntersections,t) = geometry.Intersect ray
        if hasIntersections && t > geometry.TMin then t < tCompare else false

let PointForRay (ray : Ray) (t : LineParameter) = ray.Origin + t*ray.Direction

let smallestIntersection (b,t,x) (b_new,t_new,x_new) =
    if t <= t_new then (b,t,x)
    else (b_new,t_new,x_new)

let flattenIntersection ((b,t),x) = (b,t,x)