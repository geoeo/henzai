using System.Numerics;

// Implementation from https://github.com/mellinoe/veldrid-raytracer/blob/master/src/raytracer/RandUtil.cs
// Theory from Raytracing in a Weekend

namespace Henzai.Sampling
{
    public static class RandomSampling
    {
        //TODO: For multithreading - under investigation
        private static readonly System.Object _lock = new System.Object();

        public static void XorShift(ref uint state)
        {
            lock (_lock)
            {
                state ^= state << 13;
                state ^= state >> 17;
                state ^= state << 15;
            }

            //return state;
        }

        public static float RandomFloat(ref uint state)
        {
            XorShift(ref state);
            return state * (1f / 4294967296f);
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
    }
}