module HenzaiFunc.Core.Acceleration.Boundable

open HenzaiFunc.Core.Types

type Boundable =
    abstract member GetBounds: struct(MinPoint*MaxPoint)

