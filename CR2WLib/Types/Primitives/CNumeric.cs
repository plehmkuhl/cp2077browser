using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CR2WLib.Types.Primitive
{
    [CR2WType("Int64", Type: typeof(Int64))]
    public class CInt64 : CNumeric<Int64> { }

    [CR2WType("Int32", Type: typeof(Int32))]
    public class CInt32 : CNumeric<Int32> { }

    [CR2WType("Int16", Type: typeof(Int16))]
    public class CInt16 : CNumeric<Int16> { }
    [CR2WType("Int8", Type: typeof(SByte))]
    public class CInt8 : CNumeric<SByte> { }

    [CR2WType("Uint64", Type: typeof(UInt64))]
    public class CUInt64 : CNumeric<UInt64> { }

    [CR2WType("Uint32", Type: typeof(UInt32))]
    public class CUInt32 : CNumeric<UInt32> { }

    [CR2WType("Uint16", Type: typeof(UInt16))]
    public class CUInt16 : CNumeric<UInt16> { }
    [CR2WType("Uint8", Type: typeof(Byte))]
    public class CUInt8 : CNumeric<Byte> { }

    [CR2WType("Float", Type: typeof(Single))]
    public class CFloat : CNumeric<Single> { }

    public class CNumeric<T> : CR2WValue
    {
        private T value;

        public override object InternalRepresentation => this.value;

        private object ReadValue<T>(BinaryReader reader)
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Int64: return reader.ReadInt64();
                case TypeCode.Int32: return reader.ReadInt32();
                case TypeCode.Int16: return reader.ReadInt16();
                case TypeCode.SByte: return reader.ReadSByte();
                case TypeCode.UInt64: return reader.ReadUInt64();
                case TypeCode.UInt32: return reader.ReadUInt32();
                case TypeCode.UInt16: return reader.ReadUInt16();
                case TypeCode.Byte: return reader.ReadByte();
                case TypeCode.Single: return reader.ReadSingle();
                default:
                    throw new NotImplementedException();
            }
        }

        public override void Read(BinaryReader reader)
        {
            this.value = (T)this.ReadValue<T>(reader);
        }
    }
}
