module HenzaiFunc.Core.RaytraceGeometry.NoHitable

open System.Numerics
open HenzaiFunc.Core.RaytraceGeometry.Hitable
open HenzaiFunc.Core.Acceleration.Boundable
open HenzaiFunc.Core.RaytraceGeometry
open HenzaiFunc.Core.Acceleration

type NotHitable() = 
    inherit Hitable ()


