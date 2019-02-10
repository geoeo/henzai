module HenzaiFunc.Core.RaytraceGeometry.Plane

open System
open System.Numerics
open HenzaiFunc.Core.Types
open HenzaiFunc.Core.RaytraceGeometry.Ray
open HenzaiFunc.Core.RaytraceGeometry.Hitable
open HenzaiFunc.Core.Acceleration.Boundable
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

        let widthOff = if width.IsNone then 0.0f else width.Value / 2.0f

        let heightOff = if height.IsNone then 0.0f else height.Value / 2.0f

        let R_orientation_canoical = Henzai.Core.Numerics.Geometry.RotationBetweenUnitVectors(&normal, &CanonicalPlaneSpace)

        let R_canoical_orientation = Matrix4x4.Transpose(R_orientation_canoical)

        // the point at which the normal form origin intersects with the plane
        let kern = plane.D*normal

        let b = if center.IsNone then Vector3.Zero else center.Value - kern

        let b_canonical = Vector4.Transform(Henzai.Core.Numerics.Vector.ToHomogeneous(&b, 0.0f), R_orientation_canoical)

        let pointLiesInRectangle (point : Point) =

            if center.IsNone then true
            
            else
                let v = point - kern
                let v_canonical = Vector4.Transform(Henzai.Core.Numerics.Vector.ToHomogeneous(&v, 0.0f), R_orientation_canoical)
                let newP = kern + (Henzai.Core.Numerics.Vector.ToVec3 &v_canonical)
                let newB = kern + (Henzai.Core.Numerics.Vector.ToVec3 &b_canonical)
                newP.X <= newB.X + widthOff && 
                newP.X >= newB.X - widthOff && 
                newP.Y <= newB.Y + heightOff && 
                newP.Y >= newB.Y - heightOff

        override this.Intersect (ray:Ray) =
            let numerator = -plane.D - Plane.DotNormal(plane, ray.Origin) 
            let denominator = Plane.DotNormal(plane, ray.Direction)
            if Math.Abs(denominator) < this.TMin then (false, 0.0f)
            else (true, numerator / denominator)

        override this.HasIntersection (ray:Ray) = 
            let (hasIntersection,_) = this.Intersect ray 
            hasIntersection
        // dotView factor ensures sampling "straight" at very large distances due to fov
        override this.IntersectionAcceptable hasIntersection t dotViewTrace pointOnSurface =
            let generalIntersection = hasIntersection && t > this.TMin && t <= (this.TMax/dotViewTrace)
            match center with
                | Some _ -> generalIntersection && pointLiesInRectangle pointOnSurface
                | None -> generalIntersection

        override this.NormalForSurfacePoint _ =
            normal
         
        interface Boundable with
            override this.GetBounds =

                if center.IsNone then struct(Vector3.Zero,Vector3.Zero)

                else

                    let cornerLB = center.Value + Vector3(-widthOff, -heightOff, 0.0f)

                    let cornerRU = center.Value + Vector3(widthOff, heightOff, 0.0f)

                    let zBack = (center.Value - plane.Normal).Z

                    let zFront = (center.Value + plane.Normal).Z

                    let pMin = Vector3(MathF.Min(cornerLB.X, cornerRU.X), MathF.Min(cornerLB.Y, cornerRU.Y), MathF.Min(zFront, zBack))

                    let pMax = Vector3(MathF.Max(cornerLB.X, cornerRU.X), MathF.Max(cornerLB.Y, cornerRU.Y), MathF.Max(zFront, zBack))

                    struct(pMin, pMax)

            override this.IsBoundable = center.IsSome
