namespace HenzaiFunc.Core.RaytraceGeometry

open System
open System.Numerics
open HenzaiFunc.Core.Types
open Henzai.Core.Numerics

type Plane(plane : System.Numerics.Plane, center : Point option, width : float32 option, height : float32 option ) = 
    inherit RaytracingGeometry () with

        let plane = plane

        let normal = Vector3.Normalize(plane.Normal)

        let center = center

        let width = width

        let height = height

        let widthOff = if width.IsNone then 0.0f else width.Value / 2.0f

        let heightOff = if height.IsNone then 0.0f else height.Value / 2.0f

        let R_orientation_canoical = Geometry.RotationBetweenUnitVectors(ref normal, ref RaytraceGeometryUtils.CanonicalPlaneSpace)

        let R_canoical_orientation = Matrix4x4.Transpose(R_orientation_canoical)

        // the point at which the normal form origin intersects with the plane
        let kern = plane.D*normal

        let b = if center.IsNone then Vector3.Zero else center.Value - kern

        let b_canonical = Vector4.Transform(Henzai.Core.Numerics.Vector.ToHomogeneous(ref b, 0.0f), R_orientation_canoical)

        let(pMin, pMax) =
            if center.IsNone then (Vector3.Zero, Vector3.Zero)
            else
                let xCanonical = Vector4(1.0f, 0.0f, 0.0f, 0.0f)
                let yCanonical = Vector4(0.0f, 1.0f, 0.0f, 0.0f)
                let zCanonical = Vector4(0.0f, 0.0f, -1.0f, 0.0f)

                let xObj = widthOff*Henzai.Core.Numerics.Vector.ToVec3(Vector4.Transform(xCanonical, R_canoical_orientation))
                let yObj = heightOff*Henzai.Core.Numerics.Vector.ToVec3(Vector4.Transform(yCanonical, R_canoical_orientation))
                let zObj = Henzai.Core.Numerics.Vector.ToVec3(Vector4.Transform(zCanonical, R_canoical_orientation))

                let pXA = center.Value + xObj
                let pXB = center.Value - xObj

                let pYA = center.Value + yObj
                let pYB = center.Value - yObj

                let pZA = center.Value + zObj
                let pZB = center.Value - zObj

                let xMin = MathF.Min(pXA.X, MathF.Min(pXB.X, MathF.Min(pYA.X, MathF.Min(pYB.X, MathF.Min(pZA.X, pZB.X)))))
                let yMin = MathF.Min(pXA.Y, MathF.Min(pXB.Y, MathF.Min(pYA.Y, MathF.Min(pYB.Y, MathF.Min(pZA.Y, pZB.Y)))))
                let zMin = MathF.Min(pXA.Z, MathF.Min(pXB.Z, MathF.Min(pYA.Z, MathF.Min(pYB.Z, MathF.Min(pZA.Z, pZB.Z)))))

                let xMax = MathF.Max(pXA.X, MathF.Max(pXB.X, MathF.Max(pYA.X, MathF.Max(pYB.X, MathF.Max(pZA.X, pZB.X)))))
                let yMax = MathF.Max(pXA.Y, MathF.Max(pXB.Y, MathF.Max(pYA.Y, MathF.Max(pYB.Y, MathF.Max(pZA.Y, pZB.Y)))))
                let zMax = MathF.Max(pXA.Z, MathF.Max(pXB.Z, MathF.Max(pYA.Z, MathF.Max(pYB.Z, MathF.Max(pZA.Z, pZB.Z)))))

                let pMin = Vector3(xMin, yMin, zMin)
                let pMax = Vector3(xMax, yMax, zMax)

                (pMin, pMax)
            

        let pointLiesInRectangle (point : Point) =

            if center.IsNone then true
            
            else
                let v = point - kern
                let v_canonical = Vector4.Transform(Henzai.Core.Numerics.Vector.ToHomogeneous(ref v, 0.0f), R_orientation_canoical)
                let newP = kern + (Henzai.Core.Numerics.Vector.ToVec3(ref v_canonical))
                let newB = kern + (Henzai.Core.Numerics.Vector.ToVec3(ref b_canonical))
                newP.X <= newB.X + widthOff && 
                newP.X >= newB.X - widthOff && 
                newP.Y <= newB.Y + heightOff && 
                newP.Y >= newB.Y - heightOff

        interface Hitable with

            override this.Intersect (ray:Ray) =
                let numerator = -plane.D - Plane.DotNormal(plane, ray.Origin) 
                let denominator = Plane.DotNormal(plane, ray.Direction)
                if Math.Abs(denominator) < this.AsHitable.TMin then (false, 0.0f)
                else (true, numerator / denominator)

            override this.HasIntersection (ray:Ray) = 
                let (hasIntersection, _) = this.AsHitable.Intersect ray 
                hasIntersection
            // dotView factor ensures sampling "straight" at very large distances due to fov
            override this.IntersectionAcceptable hasIntersection t dotViewTrace pointOnSurface =
                let generalIntersection = hasIntersection && t > this.AsHitable.TMin && t <= (this.AsHitable.TMax/dotViewTrace)
                match center with
                    | Some _ -> generalIntersection && pointLiesInRectangle pointOnSurface
                    | None -> generalIntersection

            override this.NormalForSurfacePoint _ =
                normal

        interface AxisAlignedBoundable with
            override this.GetBounds = AABB(pMin, pMax)

            override this.IsBoundable = center.IsSome
