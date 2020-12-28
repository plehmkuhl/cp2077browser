using CR2WLib.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CR2WLib
{
    public class CR2WExport
    {
        private CR2WFile file;
        private Stream stream;
        private CR2WChunkEntry entry;
        private CR2WExport parent;
        private List<CR2WExport> children;
        private Dictionary<String, CR2WVariant> data;

        public CR2WExport(CR2WFile file, Stream stream, CR2WChunkEntry entry)
        {
            this.file = file;
            this.stream = stream;
            this.entry = entry;
            this.children = new List<CR2WExport>();
            this.data = new Dictionary<String, CR2WVariant>();

            // Read variants
            {
                this.stream.Seek(this.entry.Offset, SeekOrigin.Begin);
                BinaryReader reader = new BinaryReader(this.stream);

                if (reader.ReadByte() != 0)
                    return;

                while (true)
                {
                    CR2WVariant variant = CR2WVariant.FromStream(this.file, this.stream);
                    if (variant == null)
                        break;

                    this.data.Add(variant.Name, variant);
                }
            }
        }

        public string CName { get => this.file.IndexedStrings[this.entry.ClassName]; }
        public CR2WExport Parent { get => this.parent; set => this.parent = value; }
        public List<CR2WExport> Children { get => this.children; }
        public Dictionary<String, CR2WVariant> Data { get => this.data;  }



        /*public byte[] Read()
        {
            byte[] buffer = new byte[this.entry.Size];
            this.stream.Seek(this.entry.Offset, SeekOrigin.Begin);
            this.stream.Read(buffer, 0, buffer.Length);

            return buffer;
        }*/
    }
}
