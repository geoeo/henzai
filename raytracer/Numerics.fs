module Raytracer.Numerics

open System.Numerics
open System

//TODO: Refactor to Henzai.Core

let Rotation (matrix : Matrix4x4)
    = Matrix4x4(matrix.M11, matrix.M12, matrix.M13, 0.0f,
                matrix.M21, matrix.M22, matrix.M23, 0.0f,
                matrix.M31, matrix.M32, matrix.M33, 0.0f,
                0.0f, 0.0f, 0.0f, 0.0f)

let TransposeRot (matrix : byref<Matrix4x4>) =
        Matrix4x4(matrix.M11,matrix.M21,matrix.M31,0.0f,
              matrix.M12,matrix.M22,matrix.M32,0.0f,
              matrix.M13,matrix.M23,matrix.M33,0.0f,
              0.0f,0.0f,0.0f,0.0f)

let ChangeOfBase (nt : byref<Vector3>) (n : byref<Vector3>) (nb : byref<Vector3>) 
    = Matrix4x4(nt.X,nt.Y,nt.Z,0.0f,
                n.X,n.Y,n.Z,0.0f,
                nb.X,nb.Y,nb.Z,0.0f,
                0.0f,0.0f,0.0f,0.0f)

let SkewSymmetric (v : byref<Vector3>) =
    Matrix4x4(0.0f , -v.Z, v.Y,0.0f,
              v.Z, 0.0f, -v.X, 0.0f,
              -v.Y, v.X, 0.0f, 0.0f,
              0.0f, 0.0f, 0.0f, 0.0f)

/// See Closed From Rodruiguez
/// http://ethaneade.com/lie.pdf
let AngleAroundOmega (omega : byref<Vector3>) = MathF.Sqrt(Vector3.Dot(omega, omega))

/// Computes the SO3 Matrix from a to b
/// https://math.stackexchange.com/questions/180418/calculate-rotation-matrix-to-align-vector-a-to-vector-b-in-3d/897677#897677
/// Problems with references from member variables make "a" be a pass-by-value
let RotationBetweenUnitVectors (a : Vector3) (b : byref<Vector3>) =

   let omega = Vector3.Cross(a, b)
   let omega_x = SkewSymmetric(&omega)
   let omega_x_squared = Matrix4x4.Multiply(omega_x, omega_x)
   let angle = AngleAroundOmega(&omega)

   let c = MathF.Cos(angle)
//    let c = Vector3.Dot(v1,v2)
   //TODO: Reduce memory copy by doing copy in memory
   Matrix4x4.Identity + omega_x + Matrix4x4.Multiply(omega_x_squared, (1.0f/(1.0f+c)))


   




        



    





