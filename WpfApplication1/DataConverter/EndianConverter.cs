using System;

namespace View.DataConverter.EndianConverter {
    class EndianConverter {

        static public UInt16 IntConverter(UInt16 data) {
            if ( BitConverter.IsLittleEndian ) {
                Byte[] bytebuff = BitConverter.GetBytes(data);
                Array.Reverse(bytebuff); 
                return BitConverter.ToUInt16(bytebuff, 0);
            } else {
                return data;
            }
        }

        static public UInt32 IntConverter(UInt32 data) {
            if ( BitConverter.IsLittleEndian ) {
                Byte[] bytebuff = BitConverter.GetBytes(data);
                Array.Reverse(bytebuff); 
                return BitConverter.ToUInt32(bytebuff, 0);
            } else {
                return data;
            }
        }
    }
}
