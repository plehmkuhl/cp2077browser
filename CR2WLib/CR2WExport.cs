using CR2WLib.Structs;
using CR2WLib.Types;
using CR2WLib.Types.Primitive;
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
        private Dictionary<String, CR2WVariant> data = null;
        private Dictionary<string, CProperty> newData = null;

        public CR2WExport(CR2WFile file, Stream stream, CR2WChunkEntry entry)
        {
            this.file = file;
            this.stream = stream;
            this.entry = entry;
            this.children = new List<CR2WExport>();

        }
        private void LoadData()
        {
            if (this.data != null)
                return; 

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

            // Read new variants
            try {
                this.newData = new Dictionary<string, CProperty>();

                this.stream.Seek(this.entry.Offset, SeekOrigin.Begin);
                BinaryReader reader = new BinaryReader(this.stream);

                if (reader.ReadByte() != 0)
                    return;

                while (this.stream.Position < (this.entry.Offset + this.entry.Size))
                {
                    try {
                        CProperty prop = CR2WValue.ReadValue(this.file, reader);
                        if (prop.Name == null)
                            break;

                        this.newData.Add(prop.Name, prop);
                    } catch (Exception e) {
                        continue;
                    }
                }
            } catch (Exception e) {
                Console.Error.Write(e);
            }
        }
        public string CName { get => this.file.CNames[this.entry.ClassName]; }
        public CR2WExport Parent { get => this.parent; set => this.parent = value; }
        public List<CR2WExport> Children { get => this.children; }
        public Dictionary<String, CR2WVariant> Data { get { this.LoadData(); return this.data; } }
        public Dictionary<String, CProperty> NewData { get { this.LoadData(); return this.newData; } }



        /*public byte[] Read()
        {
            byte[] buffer = new byte[this.entry.Size];
            this.stream.Seek(this.entry.Offset, SeekOrigin.Begin);
            this.stream.Read(buffer, 0, buffer.Length);

            return buffer;
        }*/
    }
}
