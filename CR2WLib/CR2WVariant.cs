using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CR2WLib
{
    public class CR2WVariant
    {
        private CR2WFile file;

        private int name;
        private int typeName;
        private int size;
        private byte[] rawData;

        public string Name { get => this.file.CNames[this.name]; }
        public string TypeName { get => this.file.CNames[this.typeName]; }
        public byte[] RawData { get => this.rawData; }

        public static CR2WVariant FromStream(CR2WFile file, Stream stream)
        {
            if (stream.Length - stream.Position < 8) // Dont read if not enough data left
                return null;

            BinaryReader reader = new BinaryReader(stream);
            CR2WVariant variant = new CR2WVariant
            {
                file = file,
                name = reader.ReadUInt16(),
                typeName = reader.ReadUInt16(),
                size = reader.ReadInt32(),
            };

            if (variant.name == 0)
                return null;

            if (variant.size < 4)
                return variant;

            variant.rawData = reader.ReadBytes(variant.size - 4);

            return variant;
        }

        public string ToRaRef()  { return this.ToRaRef(new BinaryReader(new MemoryStream(this.rawData))); }
        private string ToRaRef(BinaryReader reader)
        {
            ushort idx = reader.ReadUInt16();
            if (idx == 0)
                return null;

            return this.file.Imports[idx - 1].Path;
        }
        public string[] ToRaRefArray()
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(this.rawData));

            uint elements = reader.ReadUInt32();

            string[] data = new string[elements];

            for (uint i=0; i < elements; i++)
            {
                data[i] = this.ToRaRef(reader);
            }

            return data;
        }
        public string[] ToStringArray() { return this.ToStringArray(new BinaryReader(new MemoryStream(this.rawData)));  }
        public string[][] To2DStringArray() { return this.To2DStringArray(new BinaryReader(new MemoryStream(this.rawData))); }

        private string[][] To2DStringArray(BinaryReader reader)
        {
            uint rows = reader.ReadUInt32();

            string[][] result = new string[rows][];

            for (uint i = 0; i < rows; i++)
            {
                result[i] = this.ToStringArray(reader);
            }

            return result;
        }

        private string[] ToStringArray(BinaryReader reader)
        {
            int elemCount = reader.ReadInt32();

            string[] elements = new string[elemCount];

            for (int i = 0; i < elemCount; i++)
            {
                byte dataTag = reader.ReadByte();
                if ((dataTag & 0xC0) == 0xC0)
                    reader.ReadByte();

                elements[i] = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(dataTag & 0x7F));
            }

            return elements;
        }
    }
}
