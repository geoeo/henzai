namespace HenzaiFunc.Core.RaytraceGeometry

open HenzaiFunc.Core.Types
open System.Numerics
open HenzaiFunc.Core.Acceleration

[<AbstractClass>]
type Hitable ()  =

    abstract member HasIntersection: Ray -> bool
    abstract member Intersect: Ray -> bool*LineParameter
    abstract member IntersectionAcceptable : bool -> LineParameter -> float32 -> Point -> bool
    abstract member NormalForSurfacePoint : Point -> Normal
    abstract member IsObstructedBySelf: Ray -> bool 
    abstract member TMin : float32
    abstract member TMax : float32

    // effects shadow acne
    default this.TMin = 0.001f
    default this.TMax = 500.0f
    default this.HasIntersection _ = false
    default this.Intersect _ = (false, 0.0f)
    default this.IntersectionAcceptable _ _ _ _ = false
    default this.NormalForSurfacePoint _ = Vector3.Zero
    default this.IsObstructedBySelf _ = false

    interface Boundable with
        member this.GetBounds = struct(Vector3.Zero, Vector3.Zero)
        member this.IsBoundable = false
   
    member this.AsBoundable = this :> Boundable

