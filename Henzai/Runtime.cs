using System;
using System.Runtime.InteropServices;
using Henzai;
using Henzai.Geometry;

namespace Henzai.Runtime
{

    public enum VertexTypes { 
        VertexPosition, 
        VertexPositionNDCColour, 
        VertexPositionNormal,
        VertexPositionTexture, 
        VertexPositionNormalTexture,
        VertexPositionNormalTextureTangent,
        VertexPositionNormalTextureTangentBitangent
    };

    public enum UVMappingTypes {
        // http://mathworld.wolfram.com/SphericalCoordinates.html
        Spherical_Coordinates,
        // https://de.wikipedia.org/wiki/UV-Koordinaten
        Central,
        // https://math.stackexchange.com/questions/1006177/compensating-for-distortion-when-projecting-a-2d-texture-onto-a-sphere
        Stereographic
    }

    public static class Verifier {
        
        public static bool verifyVertexStruct<T>(VertexTypes vertexType) where T : struct {

            string typeName = typeof(T).Name;
            string enumName = vertexType.ToString();

            return typeName.Equals(enumName);
        }
    }


    public static class ByteMarshal
    {
        //https://stackoverflow.com/questions/2871/reading-a-c-c-data-structure-in-c-sharp-from-a-byte-array
        public static T ByteArrayToStructure<T>(byte[] bytes) where T: struct 
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

        // https://social.msdn.microsoft.com/Forums/en-US/f3ac392a-43e4-4688-83bc-8caffc9a4105/convert-struct-to-byte?forum=csharplanguage
        public static byte[] ToBytes<T>(T value) where T : struct  
        {  
            Byte[] bytes = new Byte[Marshal.SizeOf(typeof(T))];  
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
    }
}