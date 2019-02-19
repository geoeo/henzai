namespace HenzaiFunc.Core.Types

open HenzaiFunc.Core.RaytraceGeometry

type AxisAlignedBoundable =
    abstract member GetBounds: AABB
    abstract member IsBoundable: bool