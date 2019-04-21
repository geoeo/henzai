

namespace Henzai.Core.Acceleration
{
    public interface AxisAlignedBoundable
    {
        AABB GetBounds();
        bool IsBoundable();
    }

}