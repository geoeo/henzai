using System;
using System.Numerics;

// Implementation from https://github.com/mellinoe/veldrid-raytracer/blob/master/src/raytracer/RandUtil.cs
// Theory from Raytracing in a Weekend

namespace Henzai.Numerics
{
    public static class RandomSampling
    {
        private static Random randState = new Random();
        private static Random randState_2 = new Random();
        private static Object lockState = new Object();
        private static Object lockState_2 = new Object();

        public static uint XorShift(ref uint state)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 15;
            return state;
        }

        public static float RandomFloat(ref uint state)
        {
            float ret;
            uint rState = (uint)randState_2.Next();
            ret = XorShift(ref rState) * (1f / 4294967296f);
            return ret;
        }

        public static float RandomFloat_Sync()
        {
            float ret;
            lock(lockState_2){
                ret = (float)randState_2.NextDouble();
            }
            return ret;
        }

        

        public static Vector3 RandomInUnitDisk(ref uint state)
        {
            Vector3 p;
            do
            {
                p = 2f * new Vector3(RandomFloat(ref state), RandomFloat(ref state), 0) - new Vector3(1, 1, 0);
            } while (Vector3.Dot(p, p) >= 1f);
            return p;
        }

        public static Vector3 RandomInUnitSphere(ref uint state)
        {
            Vector3 ret;
            do
            {
                ret = 2f * new Vector3(RandomFloat(ref state), RandomFloat(ref state), RandomFloat(ref state)) - Vector3.One;
            } while (ret.LengthSquared() >= 1f);
            return ret;
        }

        //http://corysimon.github.io/articles/uniformdistn-on-sphere/
        public static Vector3 RandomInUnitSphere_Sync()
        {
            double rand1;
            double rand2;
            lock(lockState){
                rand1 = randState.NextDouble();
                rand2 = randState.NextDouble();
            }

            var theta = 2.0f * MathF.PI * rand1;
            var phi = MathF.Acos(1.0f - 2.0f * (float)rand2);
            float x = MathF.Sin(phi) * MathF.Cos((float)theta);
            float y = MathF.Sin(phi) * MathF.Sin((float)theta);
            float z = MathF.Cos(phi);
            return new Vector3(x,y,z);
        }

        //https://www.scratchapixel.com/lessons/3d-basic-rendering/global-illumination-path-tracing/global-illumination-path-tracing-practical-implementation
        public static Vector3 RandomInUnitHemisphere_Sync()
        {
            double rand1;
            double rand2;
            lock(lockState){
                rand1 = randState.NextDouble(); // theta
                rand2 = randState.NextDouble(); // phi
            }

            
            float cosTheta = 1.0f -(float)rand1; //TODO: check if this is really correct alt: cosTheta = rand1
            float sinTheta = MathF.Sqrt(1.0f - cosTheta*cosTheta);
            float phi = 2.0f * MathF.PI * (float)rand2;
            float x = sinTheta * MathF.Cos(phi);
            float z = sinTheta * MathF.Sin(phi);
            return new Vector3(x, cosTheta, z);
        }
    }
}