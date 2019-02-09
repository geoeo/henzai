module HenzaiFunc.Core.Geometry.NoHitable

open System.Numerics
open HenzaiFunc.Core.Geometry.Hitable
open HenzaiFunc.Core.Acceleration.Boundable
open HenzaiFunc.Core.Geometry
open HenzaiFunc.Core.Acceleration

type NotHitable() = 
    inherit Hitable ()


