namespace HenzaiFunc.Core.Types

open System.Numerics

type Point = Vector4 // Position of a point in 3D space
type MinPoint = Vector4
type MaxPoint = Vector4
type Direction = Vector4 
type Normal = Vector4
type Offset = float32 
type Radius = float32
type LineParameter = float32
type Cosine = float32
type Angle = float32
type Radians = float32
type ID = uint64
type Color = Vector4

/// Defines the basis along the viewing direction
type CoordinateSystem = 
    | XYNegZ = 0uy //RHS
    | XYZ = 1uy //LHS

type IntersectionResult = 
    | OUTSIDE = 0uy
    | INSIDE = 1uy
    | INTERSECTING = 2uy








       

