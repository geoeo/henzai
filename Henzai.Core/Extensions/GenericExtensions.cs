
namespace Henzai.Core.Extensions
{
    public static class GenericExtensions
    {
        public static uint LengthUnsigned<T>(this T[] val)
        {
            return (uint)val.Length;
        }
    }
}
