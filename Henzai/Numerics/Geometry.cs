using System;
using System.Numerics;

namespace Henzai.Numerics
{
   public static class Geometry {
        /// <summary>
        /// Creates a Right Handed Coordinate System where the Normal defines the Up Vector
        /// https://www.scratchapixel.com/lessons/3d-basic-rendering/global-illumination-path-tracing/global-illumination-path-tracing-practical-implementation
        /// </summary>
       //
       public static void CreateCoordinateSystemAroundNormal(ref Vector3 N, ref Vector3 Nt, ref Vector3 Nb){
           if(MathF.Abs(N.X)  > MathF.Abs(N.Y)){
               float mag = MathF.Sqrt(N.X*N.X + N.Y*N.Y);
               Nt.X = N.Z / mag;
               Nt.Y = 0.0f;
               Nt.Z = -N.X / mag;
           } 
           else {
              float mag = MathF.Sqrt(N.Z*N.Z + N.Y*N.Y); 
              Nt.X = 0.0f;
              Nt.Y = -N.Z / mag;
              Nt.Z = N.Y / mag;
           }

           Nb = Vector3.Cross(N,Nt);
       }

   }
}