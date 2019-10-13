using System.Numerics;
using Henzai.Core.Numerics;

namespace Henzai.Core.Raytracing
{

    //TODO: make readonly in .NetCore 3.0
    public struct Ray
    {
        public readonly Vector4 Origin;
        public readonly Vector4 Direction;

        public Ray(Vector4 origin, Vector4 direction)
        {
            Origin = origin;
            Direction = Numerics.Vector.Normalize(direction);
        }

        public Ray(Vector3 origin, Vector3 direction)
        {
            Origin = new Vector4(origin, 1.0f);
            Direction = new Vector4(Numerics.Vector.Normalize(direction), 0.0f);
        }
    }
}