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

        public string Name { get => this.file.IndexedStrings[this.name]; }
        public string TypeName { get => this.file.IndexedStrings[this.typeName]; }
        public byte[] RawData { get => this.rawData; }

        public static CR2WVariant FromStream(CR2WFile file, Stream stream)
        {
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

            variant.rawData = reader.ReadBytes(variant.size - 4);

            return variant;
        }
    }
}
