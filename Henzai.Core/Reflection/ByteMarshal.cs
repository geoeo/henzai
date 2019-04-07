using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace Henzai.Core.Reflection
{
    public static class ByteMarshal
    {
        //https://stackoverflow.com/questions/2871/reading-a-c-c-data-structure-in-c-sharp-from-a-byte-array
        public static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            T value;
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                value = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
            return value;
        }

        public static string ByteArrayToUTF8<T>(byte[] bytes) 
        {
            string value;
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                value = Marshal.PtrToStringUTF8(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
            return value;
        }

        // http://www.java2s.com/Code/CSharp/File-Stream/ConvertabytearraytoanObject.htm
        public static T ByteArrayToClass<T>(byte[] bytes) where T : class
        {
            var memStream = new MemoryStream();
            var binForm = new BinaryFormatter();

            memStream.Write(bytes, 0, bytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);

            T obj = (T)binForm.Deserialize(memStream);

            return obj;
        }

        // https://social.msdn.microsoft.com/Forums/en-US/f3ac392a-43e4-4688-83bc-8caffc9a4105/convert-struct-to-byte?forum=csharplanguage
        public static byte[] StructToBytes<T>(T value) where T : struct
        {
            byte[] bytes = new byte[Marshal.SizeOf(typeof(T))];
            GCHandle handle = GCHandle.Alloc(value, GCHandleType.Pinned);
            try
            {
                Marshal.Copy(handle.AddrOfPinnedObject(), bytes, 0, bytes.Length);
                return bytes;
            }
            finally
            {
                handle.Free();
            }
        }

        public static byte[] UTF8ToBytes(string value)
        {
            System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
            return enc.GetBytes(value);
        }
    }
}
