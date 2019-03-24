using System;

namespace Henzai
{
    /// <summary>
    /// Encapsulates Screen Resolution.
    /// </summary>
    public struct Resolution
    {
       public int Horizontal;
       public int Vertical;

       public Resolution(int horizonral, int vertical){
           Horizontal = horizonral;
           Vertical = vertical;
       }
    }
}
