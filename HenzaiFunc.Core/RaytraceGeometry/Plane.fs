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


        /// Elements of the transformed Vector should always be rounded
        let R_orientation_canoical = Geometry.RotationBetweenUnitVectors(ref normal, ref RaytraceGeometryUtils.CanonicalPlaneSpace)

        /// Elements of the transformed Vector should always be rounded
        let R_canoical_orientation : Matrix4x4 = Matrix4x4.Transpose(R_orientation_canoical)

        // the point at which the normal form origin intersects with the plane
        let kern = plane.D*normal

        let b = if center.IsNone then Vector3.Zero else center.Value - kern

        let b_canonical = Vector4.Transform(Henzai.Core.Numerics.Vector.ToHomogeneous(ref b, 0.0f), R_orientation_canoical)

        // Profile this
        // does not wor with nonAA planes( probably due to precision in R_orientation_canoical)
        let(pMin, pMax) =
            if center.IsNone then (Vector3.Zero, Vector3.Zero)
            else
                //let withVal = width.Value
                //let heightVal = height.Value

                let withVal = widthOff
                let heightVal = heightOff

                let p1 = Vector4(-withVal, heightVal, -1.0f, 0.0f)
                let p2 = Vector4(-withVal, heightVal, 1.0f, 0.0f)
                let p3 = Vector4(-withVal, -heightVal, -1.0f, 0.0f)
                let p4 = Vector4(-withVal, -heightVal, 1.0f, 0.0f)
                let p5 = Vector4(withVal, heightVal, -1.0f, 0.0f)
                let p6 = Vector4(withVal, heightVal, 1.0f, 0.0f)
                let p7 = Vector4(withVal, -heightVal, -1.0f, 0.0f)
                let p8 = Vector4(withVal, -heightVal, 1.0f, 0.0f)

                let mutable p1Rot = Henzai.Core.Numerics.Vector.ToVec3(Vector4.Transform(p1, R_canoical_orientation))
                let mutable p2Rot = Henzai.Core.Numerics.Vector.ToVec3(Vector4.Transform(p2, R_canoical_orientation))
                let mutable p3Rot = Henzai.Core.Numerics.Vector.ToVec3(Vector4.Transform(p3, R_canoical_orientation))
                let mutable p4Rot = Henzai.Core.Numerics.Vector.ToVec3(Vector4.Transform(p4, R_canoical_orientation))
                let mutable p5Rot = Henzai.Core.Numerics.Vector.ToVec3(Vector4.Transform(p5, R_canoical_orientation))
                let mutable p6Rot = Henzai.Core.Numerics.Vector.ToVec3(Vector4.Transform(p6, R_canoical_orientation))
                let mutable p7Rot = Henzai.Core.Numerics.Vector.ToVec3(Vector4.Transform(p7, R_canoical_orientation))
                let mutable p8Rot = Henzai.Core.Numerics.Vector.ToVec3(Vector4.Transform(p8, R_canoical_orientation))

                let p1ObjSpace = center.Value + Henzai.Core.Numerics.Vector.RoundVec3(ref p1Rot, -1)
                let p2ObjSpace = center.Value + Henzai.Core.Numerics.Vector.RoundVec3(ref p2Rot, -1)
                let p3ObjSpace = center.Value + Henzai.Core.Numerics.Vector.RoundVec3(ref p3Rot, -1)
                let p4ObjSpace = center.Value + Henzai.Core.Numerics.Vector.RoundVec3(ref p4Rot, -1)
                let p5ObjSpace = center.Value + Henzai.Core.Numerics.Vector.RoundVec3(ref p5Rot, -1)
                let p6ObjSpace = center.Value + Henzai.Core.Numerics.Vector.RoundVec3(ref p6Rot, -1)
                let p7ObjSpace = center.Value + Henzai.Core.Numerics.Vector.RoundVec3(ref p7Rot, -1)
                let p8ObjSpace = center.Value + Henzai.Core.Numerics.Vector.RoundVec3(ref p8Rot, -1)

                let xMin = MathF.Min(p1ObjSpace.X, MathF.Min(p2ObjSpace.X, MathF.Min(p3ObjSpace.X, MathF.Min(p4ObjSpace.X, MathF.Min(p5ObjSpace.X, MathF.Min(p6ObjSpace.X, MathF.Min(p7ObjSpace.X, p8ObjSpace.X)))))))
                let yMin = MathF.Min(p1ObjSpace.Y, MathF.Min(p2ObjSpace.Y, MathF.Min(p3ObjSpace.Y, MathF.Min(p4ObjSpace.Y, MathF.Min(p5ObjSpace.Y, MathF.Min(p6ObjSpace.Y, MathF.Min(p7ObjSpace.Y, p8ObjSpace.Y)))))))
                let zMin = MathF.Min(p1ObjSpace.Z, MathF.Min(p2ObjSpace.Z, MathF.Min(p3ObjSpace.Z, MathF.Min(p4ObjSpace.Z, MathF.Min(p5ObjSpace.Z, MathF.Min(p6ObjSpace.Z, MathF.Min(p7ObjSpace.Z, p8ObjSpace.Z)))))))

                let xMax = MathF.Max(p1ObjSpace.X, MathF.Max(p2ObjSpace.X, MathF.Max(p3ObjSpace.X, MathF.Max(p4ObjSpace.X, MathF.Max(p5ObjSpace.X, MathF.Max(p6ObjSpace.X, MathF.Max(p7ObjSpace.X, p8ObjSpace.X)))))))
                let yMax = MathF.Max(p1ObjSpace.Y, MathF.Max(p2ObjSpace.Y, MathF.Max(p3ObjSpace.Y, MathF.Max(p4ObjSpace.Y, MathF.Max(p5ObjSpace.Y, MathF.Max(p6ObjSpace.Y, MathF.Max(p7ObjSpace.Y, p8ObjSpace.Y)))))))
                let zMax = MathF.Max(p1ObjSpace.Z, MathF.Max(p2ObjSpace.Z, MathF.Max(p3ObjSpace.Z, MathF.Max(p4ObjSpace.Z, MathF.Max(p5ObjSpace.Z, MathF.Max(p6ObjSpace.Z, MathF.Max(p7ObjSpace.Z, p8ObjSpace.Z)))))))

                let pMin = Vector3(MathF.Round(xMin), MathF.Round(yMin), MathF.Round(zMin))
                let pMax = Vector3(MathF.Round(xMax), MathF.Round(yMax), MathF.Round(zMax))


                                 
                (pMin, pMax)
            

        let pointLiesInRectangle (point : Point) =

            if center.IsNone then true
            
            else
                let v = point - kern
                let mutable v_canonical_1 = Vector4.Transform(Henzai.Core.Numerics.Vector.ToHomogeneous(ref v, 0.0f), R_orientation_canoical)
                let v_canonical = Henzai.Core.Numerics.Vector.RoundVec4(ref v_canonical_1, 3)
                let newP = kern + (Henzai.Core.Numerics.Vector.ToVec3(ref v_canonical))
                let newB = kern + (Henzai.Core.Numerics.Vector.ToVec3(ref b_canonical))
                newP.X <= newB.X + widthOff && 
                newP.X >= newB.X - widthOff && 
                newP.Y <= newB.Y + heightOff && 
                newP.Y >= newB.Y - heightOff

        /// Elements of the transformed Vector should always be rounded
        member this.Get_R_canoical_orientation = R_canoical_orientation

        /// Elements of the transformed Vector should always be rounded
        member this.Get_R_orientation_canoical = R_orientation_canoical

        interface Hitable with

            override this.Intersect (ray:Ray) =
                // https://en.wikipedia.org/wiki/Line–plane_intersection
                // plane store the distance from the plane to the origin i.e. we have to invert
                let numerator = Plane.DotNormal(plane,  -kern - ray.Origin) 
                let denominator = Plane.DotNormal(plane, ray.Direction)
                if Math.Abs(denominator) < this.AsHitable.TMin || Math.Abs(numerator) < this.AsHitable.TMin  then (false, 0.0f)
                else 
                    let t = numerator / denominator
                    if t < 0.0f then (false, 0.0f)
                    else
                        let pointOnSurface = ray.Origin + t*ray.Direction
                        (pointLiesInRectangle pointOnSurface, t)

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
