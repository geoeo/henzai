module Raytracer.Geometry.Sphere

open System
open System.Numerics
open Raytracer.Geometry.Types
open Raytracer.Geometry.Ray
open Raytracer.Geometry.Hitable
open Henzai.Core.Numerics

type Sphere(sphereCenter : Point,radius : Radius) =
    inherit Hitable () with

        // override this.TMin = 0.000001f// 0.0001// 0.000001f
        let center = sphereCenter

        let radius = radius

        member this.IntersectWith (t : LineParameter) (ray : Ray) =
            ((ray.Origin + t*ray.Direction) - center).Length() <= radius

        member this.Intersections (ray : Ray) = 
            //A is always one
            let centerToRay = ray.Origin - center
            let B = 2.0f*Vector3.Dot(centerToRay,ray.Direction)
            let C = Vector3.Dot(centerToRay,centerToRay)-(Henzai.Core.Numerics.Utils.Square(radius))
            let discriminant = B*B - 4.0f*C
            if discriminant < 0.0f then (false, 0.0f,0.0f)
            // TODO: may cause alsiasing investigate around sphere edges
            else if MathF.Round(discriminant, 3) = 0.0f then (false,-B/(2.0f),System.Single.MinValue)
            else (true,((-B + MathF.Sqrt(discriminant))/(2.0f)),((-B - MathF.Sqrt(discriminant))/(2.0f)))

        override this.Intersect (ray : Ray) = 
            let (hasIntersection,i1,i2) = this.Intersections (ray : Ray)
            if i1 >= this.TMin && i2 >= this.TMin then
                (hasIntersection,MathF.Min(i1,i2))
            else if i1 < 0.0f then
                (hasIntersection,i2)
            else
                (hasIntersection,i1)

        override this.NormalForSurfacePoint (positionOnSphere:Point) =
            Vector3.Normalize((positionOnSphere - center))

        override this.HasIntersection ray =
            let (hasIntersection,_,_) = this.Intersections ray 
            hasIntersection

        override this.IntersectionAcceptable hasIntersection t _ _ =
            hasIntersection && t > this.TMin

        override this.IsObstructedBySelf ray =
            let (b,i1,i2) = this.Intersections ray
            this.IntersectionAcceptable b (MathF.Max(i1, i2)) 1.0f Vector3.Zero