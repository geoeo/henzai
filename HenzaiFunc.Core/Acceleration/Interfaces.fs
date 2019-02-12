namespace HenzaiFunc.Core.Acceleration

open HenzaiFunc.Core.Types

type Boundable =
    abstract member GetBounds: struct(MinPoint*MaxPoint)
    abstract member IsBoundable: bool

