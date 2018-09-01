using System;
using System.Numerics;

namespace Henzai.Core.Numerics
{
    //TODO: Make a GramSchmidt Method for a sample
    // let mutable nb = Vector3.Normalize(rand_norm - normal*Vector3.Dot(rand_norm,normal))
    // let mutable nt = Vector3.Normalize(Vector3.Cross(normal,nb))
   public static class Geometry {
        /// <summary>
        /// Creates a Right Handed Coordinate System where the Normal defines the Up Vector
        /// https://www.scratchapixel.com/lessons/3d-basic-rendering/global-illumination-path-tracing/global-illumination-path-tracing-practical-implementation
        /// Peter Shirley's Raytracing Miniseries
        /// </summary>
       //
       public static void CreateCoordinateSystemAroundNormal(ref Vector3 N, ref Vector3 Nt, ref Vector3 Nb){
            Vector3 a;
            if (MathF.Abs(N.X) > MathF.Abs(N.Y))
            {
                a = Vector3.UnitY;
            }
            else
            {
                a = Vector3.UnitX;
            }

            Nb = Vector3.Normalize(Vector3.Cross(N, a));
            Nt = Vector3.Normalize(Vector3.Cross(N, Nb));

        }

   }
}