module Raytracer.Geometry.Plane

open System
open System.Numerics
open Raytracer.Geometry.Types
open Raytracer.Geometry.Ray
open Raytracer.Geometry.Hitable
open Henzai.Core.Numerics

/// Space inwhich points are compared if they are inside a rectangle
/// Plane is XY
let mutable CanonicalPlaneSpace = Vector3(0.0f, 0.0f, -1.0f)

type Plane(plane : System.Numerics.Plane, center : Point option, width : float32 option, height : float32 option ) = 
    inherit Hitable () with

        let plane = plane

        let normal = Vector3.Normalize(plane.Normal)

        let center = center

        let width = width

        let height = height

        member this.PointLiesInRectangle (point : Point) =
            let widthOff = width.Value / 2.0f
            let heightOff = height.Value / 2.0f
            let R = Henzai.Core.Numerics.Geometry.RotationBetweenUnitVectors(&normal, &CanonicalPlaneSpace)
            let kern = if plane.D > 0.0f then -1.0f*plane.D*normal else plane.D*normal
            let v = point - kern
            let b = center.Value - kern
            let newDir = Vector4.Transform(Henzai.Core.Numerics.Vector.ToHomogeneous(&v, 0.0f), R)
            let newDir_b = Vector4.Transform(Henzai.Core.Numerics.Vector.ToHomogeneous(&b, 0.0f), R)
            let newP = kern + (Henzai.Core.Numerics.Vector.ToVec3 &newDir)
            let newB = kern + (Henzai.Core.Numerics.Vector.ToVec3 &newDir_b)
            newP.X <= newB.X + widthOff && 
            newP.X >= newB.X - widthOff && 
            newP.Y <= newB.Y + heightOff && 
            newP.Y >= newB.Y - heightOff

        override this.Intersect (ray:Ray) =
            let numerator = -plane.D - Plane.DotNormal(plane,ray.Origin) 
            let denominator = Plane.DotNormal(plane,ray.Direction)
            if Math.Abs(denominator) < this.TMin then (false, 0.0f)
            else (true, numerator / denominator)

        override this.HasIntersection (ray:Ray) = 
            let (hasIntersection,_) = this.Intersect ray 
            hasIntersection
        // dotView factor ensures sampling "straight" at very large distances due to fov
        override this.IntersectionAcceptable hasIntersection t dotViewTrace pointOnSurface =
            let generalIntersection = hasIntersection && t > this.TMin && t <= (this.TMax/dotViewTrace)
            match center with
                | Some _ -> generalIntersection && this.PointLiesInRectangle pointOnSurface
                | None -> generalIntersection

        override this.NormalForSurfacePoint _ =
            normal