namespace HenzaiFunc.Core.RaytraceGeometry

open HenzaiFunc.Core.Types

type Hitable =

    abstract member HasIntersection: Ray -> bool
    abstract member Intersect: Ray -> bool*LineParameter
    //TODO: Make Point optional
    abstract member IntersectionAcceptable : bool -> LineParameter -> float32 -> Point -> bool
    abstract member NormalForSurfacePoint : Point -> Normal
    abstract member IsObstructedBySelf: Ray -> bool 
    abstract member TMin : float32
    abstract member TMax : float32
