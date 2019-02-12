namespace HenzaiFunc.Core.Acceleration

open HenzaiFunc.Core.Types


type AxisAlignedBoundable =
    abstract member GetBounds: AABB
    abstract member IsBoundable: bool

