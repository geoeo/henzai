module Raytracer.Numerics

open System.Numerics
open System

let UnitParameter (value : float32) = 
    if value >= 0.0f && value <= 1.0f then value else failwith "parameter not in range [0,1]"

let Round (num :float32) (digits:int) = MathF.Round(num, digits)

//TODO: Investigate Vector3 normalization in fsharp
let NormalizedOrFail(value : Vector3) = 
    if Round (value.Length()) 5 = 1.0f then value 
        else
            printfn "WARN: Vector3 not normalized %f %f %f" value.X value.Y value.Z
            Vector3.Normalize(value)
            // failwith "Vector3 not normalized"

let RoundVec3 (vec3:Vector3) (digits:int) =
    Vector3(Round vec3.X digits, Round vec3.Y digits, Round vec3.Z digits)

let ToVec3 (vec4 : Vector4) =
    Vector3(vec4.X, vec4.Y, vec4.Z)

let ToHomogeneous (v : Vector3) (c : float32) = Vector4(v, c)
let ApplyFuncToVector3 func (vec : Vector3) = Vector3(func vec.X, func vec.Y, func vec.Z)

let SurfaceNormal a b c = Vector3.Normalize(Vector3(a, b, c))

let Power exp b = MathF.Pow(b, exp)
let inline Square b : float32 = b*b

let Rotation (matrix : Matrix4x4)
    = Matrix4x4(matrix.M11, matrix.M12, matrix.M13, 0.0f,
                matrix.M21, matrix.M22, matrix.M23, 0.0f,
                matrix.M31, matrix.M32, matrix.M33, 0.0f,
                0.0f, 0.0f, 0.0f, 0.0f)

let TransposeRot (matrix : Matrix4x4) =
        Matrix4x4(matrix.M11,matrix.M21,matrix.M31,0.0f,
              matrix.M12,matrix.M22,matrix.M32,0.0f,
              matrix.M13,matrix.M23,matrix.M33,0.0f,
              0.0f,0.0f,0.0f,0.0f)

let ChangeOfBase (nt : byref<Vector3>) (n : byref<Vector3>) (nb : byref<Vector3>) 
    = Matrix4x4(nt.X,nt.Y,nt.Z,0.0f,
                n.X,n.Y,n.Z,0.0f,
                nb.X,nb.Y,nb.Z,0.0f,
                0.0f,0.0f,0.0f,0.0f)

let SkewSymmetric (v : Vector3) =
    Matrix4x4(0.0f , -v.Z, v.Y,0.0f,
              v.Z, 0.0f, -v.X, 0.0f,
              -v.Y, v.X, 0.0f, 0.0f,
              0.0f, 0.0f, 0.0f, 0.0f)

/// See Closed From Rodruiguez
/// http://ethaneade.com/lie.pdf
let AngleAroundOmega (omega : Vector3) = MathF.Sqrt(Vector3.Dot(omega, omega))

/// Computes the SO3 Matrix from a to b
/// https://math.stackexchange.com/questions/180418/calculate-rotation-matrix-to-align-vector-a-to-vector-b-in-3d/897677#897677
let RotationBetweenUnitVectors (a : Vector3) (b : Vector3) (mat : byref<Matrix4x4>) =
   let v1 = a
   let v2 = b

   let omega = Vector3.Cross(v1, v2)
   let omega_x = SkewSymmetric(omega)
   let omega_x_squared = Matrix4x4.Multiply(omega_x, omega_x)
   let angle = AngleAroundOmega(omega)

   let c = MathF.Cos(angle)
//    let c = Vector3.Dot(v1,v2)
   //TODO: Reduce memory copy by doing copy in memory
   mat <- Matrix4x4.Identity + omega_x + Matrix4x4.Multiply(omega_x_squared, (1.0f/(1.0f+c)))
   ()


   




        



    





