using System;
using System.Numerics;
using System.Diagnostics;

namespace Henzai.Core.Numerics
{
   public static class Geometry {
        /// <summary>
        /// Creates a Right Handed Coordinate System where the Normal defines the Up Vector
        /// https://www.scratchapixel.com/lessons/3d-basic-rendering/global-illumination-path-tracing/global-illumination-path-tracing-practical-implementation
        /// Peter Shirley's Raytracing Miniseries
        /// </summary>
        public static void CreateCoordinateSystemAroundNormal(ref Vector3 n, ref Vector3 nt, ref Vector3 nb){
            Vector3 a;
            if (MathF.Abs(n.X) > MathF.Abs(n.Y))
            {
                a = Vector3.UnitY;
            }
            else
            {
                a = Vector3.UnitX;
            }

            nb = Vector3.Normalize(Vector3.Cross(n, a));
            nt = Vector3.Normalize(Vector3.Cross(n, nb));

        }

        //TODO use correct keywords
        public static void CreateCoordinateSystemAroundNormal(ref Vector4 nInOut, ref Vector4 ntInOut, ref Vector4 nbInOut)
        {
            var n = Vector.ToVec3(nInOut);

            Vector3 a;
            if (MathF.Abs(n.X) > MathF.Abs(n.Y))
            {
                a = Vector3.UnitY;
            }
            else
            {
                a = Vector3.UnitX;
            }

            var nbNorm = Vector3.Normalize(Vector3.Cross(n, a));
            var ntNorm = Vector3.Normalize(Vector3.Cross(n, nbNorm));

            Vector.CopyIntoVector4(ref nbInOut, ref nbNorm );
            Vector.CopyIntoVector4(ref ntInOut, ref ntNorm);

        }

        public static Matrix4x4 Rotation(ref Matrix4x4 matrix){
            return new Matrix4x4(
                matrix.M11, matrix.M12, matrix.M13, 0.0f,
                matrix.M21, matrix.M22, matrix.M23, 0.0f,
                matrix.M31, matrix.M32, matrix.M33, 0.0f,
                0.0f, 0.0f, 0.0f, 0.0f);
        }

        public static Matrix4x4 TranseposeRot(ref Matrix4x4 matrix){
            return new Matrix4x4(
                matrix.M11,matrix.M21,matrix.M31,0.0f,
                matrix.M12,matrix.M22,matrix.M32,0.0f,
                matrix.M13,matrix.M23,matrix.M33,0.0f,
                0.0f,0.0f,0.0f,0.0f   
            );
        }

        public static Matrix4x4 ChangeOfBase(ref Vector3 nt, ref Vector3 n, ref Vector3 nb){
            return new Matrix4x4
            (
                nt.X,nt.Y,nt.Z,0.0f,
                n.X,n.Y,n.Z,0.0f,
                nb.X,nb.Y,nb.Z,0.0f,
                0.0f,0.0f,0.0f,0.0f
            );
        }

        public static Matrix4x4 ChangeOfBase(ref Vector4 nt, ref Vector4 n, ref Vector4 nb)
        {
            return new Matrix4x4
            (
                nt.X, nt.Y, nt.Z, 0.0f,
                n.X, n.Y, n.Z, 0.0f,
                nb.X, nb.Y, nb.Z, 0.0f,
                0.0f, 0.0f, 0.0f, 0.0f
            );
        }

        public static Matrix4x4 SkewSymmetric(ref Vector3 v){
            return new Matrix4x4(
                0.0f , -v.Z, v.Y,0.0f,
                v.Z, 0.0f, -v.X, 0.0f,
                -v.Y, v.X, 0.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 0.0f  
            );
        }

        /// <summary>
        /// See Closed From Rodruiguez
        /// http://ethaneade.com/lie.pdf
        /// </summary>

        public static double AngleAroundOmega(ref Vector3 omega){
            return Math.Sqrt(Vector.InMemoryDotProduct(ref omega, ref omega));
        }

        public static double AngleAroundOmega(ref Vector4 omega)
        {
            return Math.Sqrt(Vector.InMemoryDotProduct(ref omega, ref omega));
        }


        /// <summary>
        /// Computes the SO3 Matrix from a to b
        /// https://math.stackexchange.com/questions/180418/calculate-rotation-matrix-to-align-vector-a-to-vector-b-in-3d/897677#897677
        /// Problems with references from member variables make "a" be a pass-by-value
        /// </summary>
        public static Matrix4x4 RotationBetweenUnitVectors(ref Vector3 a, ref Vector3 b){
            var omega = Vector3.Cross(a, b);
            var omega_x = SkewSymmetric(ref omega);
            var omega_x_squared = Matrix4x4.Multiply(omega_x,omega_x);
            var angle = AngleAroundOmega(ref omega);
            var c = Math.Cos(angle);
            var factor = 1.0 / (1.0 + c);

            //TODO: Optimize this for less allocations
            //TODO: try this with double precision
            return Matrix4x4.Identity + omega_x + Matrix4x4.Multiply(omega_x_squared, (float)factor);
        }

        public static Matrix4x4 RotationBetweenUnitVectors(ref Vector4 a, ref Vector4 b)
        {
            var a_v3 = Vector.ToVec3(a);
            var b_v3 = Vector.ToVec3(b);
            var omega =Vector3.Cross(a_v3, b_v3);
            var omega_x = SkewSymmetric(ref omega);
            var omega_x_squared = Matrix4x4.Multiply(omega_x, omega_x);
            var angle = AngleAroundOmega(ref omega);
            var c = Math.Cos(angle);
            var factor = 1.0 / (1.0 + c);

            //TODO: Optimize this for less allocations
            //TODO: try this with double precision
            return Matrix4x4.Identity + omega_x + Matrix4x4.Multiply(omega_x_squared, (float)factor);
        }




















    }
}