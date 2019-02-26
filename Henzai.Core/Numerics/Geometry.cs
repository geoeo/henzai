using System;
using System.Numerics;
using System.Runtime.CompilerServices; 

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

        public static float AngleAroundOmega(ref Vector3 omega){
            return MathF.Sqrt(Vector.InMemoryDotProduct(ref omega, ref omega));
        }


        /// <summary>
        /// Computes the SO3 Matrix from a to b
        /// https://math.stackexchange.com/questions/180418/calculate-rotation-matrix-to-align-vector-a-to-vector-b-in-3d/897677#897677
        /// Problems with references from member variables make "a" be a pass-by-value
        /// </summary>
        public static Matrix4x4 RotationBetweenUnitVectors(ref Vector3 a, ref Vector3 b){
            var omega = Vector3.Cross(a,b);
            var omega_x = SkewSymmetric(ref omega);
            var omega_x_squared = Matrix4x4.Multiply(omega_x,omega_x);
            var angle = AngleAroundOmega(ref omega);
            var c = MathF.Cos(angle);

            //TODO: Optimize this for less allocations
            //TODO: try this with double precision
            return Matrix4x4.Identity + omega_x + Matrix4x4.Multiply(omega_x_squared, (1.0f/(1.0f+c)));
        }



   




        



    







   }
}