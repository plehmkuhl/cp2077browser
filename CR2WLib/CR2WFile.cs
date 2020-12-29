using ArchiveLib.Oodle;
using CR2WLib.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace CR2WLib
{
    public class CR2WFile 
    {
        private Stream stream;



        private CR2WFileHeader header;
        private CR2WTableEntry[] tables;
        private Dictionary<uint, string> strings;

        private string[] names;
        private CR2WPropertyEntry[] properties;
        private CR2WChunkEntry[] chunks;
        private CR2WBufferEntry[] buffers;

        private List<CR2WImport> imports;
        private List<CR2WExport> exports;
        private List<CR2WProperty> hlProperties;

        public CR2WFile()
        {

        }

        public string[] CNames { get => this.names; }
        public Dictionary<uint, string> OffsetStrings { get => this.strings; }
        public List<CR2WImport> Imports { get => this.imports; }
        public List<CR2WExport> Exports { get => this.exports; }
        public List<CR2WProperty> Properties { get => this.hlProperties; }

        private void ReadHeader()
        {
            BinaryReader reader = new BinaryReader(this.stream);
            CR2WFileHeader header = new CR2WFileHeader();

            header.Magic = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(4));

            if (header.Magic != "CR2W")
                throw new Exception("Not a CR2W file");

            header.Version = reader.ReadUInt32();

            if (header.Version > 195 || header.Version < 163)
                throw new FormatException("Unsupported CR2W version");

            header.Flags = reader.ReadUInt32();
            header.Filetime = reader.ReadUInt64();
            header.BuildVersion = reader.ReadUInt32();
            header.FileSize = reader.ReadUInt32();
            header.BufferSize = reader.ReadUInt32();
            header.Crc32 = reader.ReadUInt32();
            header.ChunkCount = reader.ReadUInt32();

            this.header = header;
            this.tables = new CR2WTableEntry[10];

            // Read table offsets
            for (int t=0; t < 10; t++)
            {
                this.tables[t] = new CR2WTableEntry();
                this.tables[t].Offset = reader.ReadUInt32();
                this.tables[t].ItemCount = reader.ReadUInt32();
                this.tables[t].Crc32 = reader.ReadUInt32();
            }

            // Read string table
            {
                byte[] stringBuffer = new byte[this.tables[0].ItemCount];
                this.strings = new Dictionary<uint, string>();

                this.stream.Read(stringBuffer, 0, stringBuffer.Length);

                int currentOffset = 0;
                int currentLength = 0;

                for (int i=0; i < stringBuffer.Length; i++)
                {
                    if (stringBuffer[i] != 0) {
                        currentLength++;
                        continue;
                    } else {
                        strings.Add((uint)currentOffset, System.Text.Encoding.GetEncoding("iso-8859-1").GetString(stringBuffer, currentOffset, currentLength));

                        currentOffset += currentLength + 1;
                        currentLength = 0;
                    }
                }
            }

            // Read names
            {
                byte[] nameBuffer = new byte[this.tables[1].ItemCount * 8];
                this.stream.Seek(this.tables[1].Offset, SeekOrigin.Begin);
                this.stream.Read(nameBuffer, 0, nameBuffer.Length);

                this.names = new string[this.tables[1].ItemCount];

                BinaryReader nameReader = new BinaryReader(new MemoryStream(nameBuffer));
                for (int i=0; i < this.tables[1].ItemCount; i++)
                {
                    uint valueIdx = nameReader.ReadUInt32();
                    uint hash = nameReader.ReadUInt32();

                    this.names[i] = this.strings[valueIdx];
                }
            }

            // Read imports
            {
                byte[] importBuffer = new byte[this.tables[2].ItemCount * Marshal.SizeOf<CR2WImportEntry>()];
                this.stream.Seek(this.tables[2].Offset, SeekOrigin.Begin);
                this.stream.Read(importBuffer, 0, importBuffer.Length);

                this.imports = new List<CR2WImport>();

                BinaryReader importReader = new BinaryReader(new MemoryStream(importBuffer));
                for (int i = 0; i < this.tables[2].ItemCount; i++)
                {
                    CR2WImportEntry entry = new CR2WImportEntry()
                    {
                        Path = importReader.ReadUInt32(),
                        ClassName = importReader.ReadUInt16(),
                        Flags = importReader.ReadUInt16(),
                    };

                    this.imports.Add(new CR2WImport(this, entry));
                }
            }

            // Read properties
            {
                byte[] propertyBuffer = new byte[this.tables[3].ItemCount * Marshal.SizeOf<CR2WPropertyEntry>()];
                this.stream.Seek(this.tables[3].Offset, SeekOrigin.Begin);
                this.stream.Read(propertyBuffer, 0, propertyBuffer.Length);

                this.properties = new CR2WPropertyEntry[this.tables[3].ItemCount];
                this.hlProperties = new List<CR2WProperty>();

                BinaryReader propertyReader = new BinaryReader(new MemoryStream(propertyBuffer));
                for (int i = 0; i < this.tables[3].ItemCount; i++)
                {
                    this.properties[i] = new CR2WPropertyEntry
                    {
                        ClassName = propertyReader.ReadUInt16(),
                        ClassFlags = propertyReader.ReadUInt16(),
                        PropertyName = propertyReader.ReadUInt16(),
                        PropertyFlags = propertyReader.ReadUInt16(),
                        Hash = propertyReader.ReadUInt64(),
                    };

                    if (this.properties[i].ClassName != 0)
                        this.hlProperties.Add(CR2WProperty.CreateFromPropertyEntry(this, this.properties[i]));
                }
            }

            // Read chunk table
            {
                byte[] chunkBuffer = new byte[this.tables[4].ItemCount * Marshal.SizeOf<CR2WChunkEntry>()];
                this.stream.Seek(this.tables[4].Offset, SeekOrigin.Begin);
                this.stream.Read(chunkBuffer, 0, chunkBuffer.Length);

                this.chunks = new CR2WChunkEntry[this.tables[4].ItemCount];
                this.exports = new List<CR2WExport>();

                BinaryReader chunkReader = new BinaryReader(new MemoryStream(chunkBuffer));
                for (int i = 0; i < this.tables[4].ItemCount; i++)
                {
                    CR2WChunkEntry entry = new CR2WChunkEntry
                    {
                        ClassName = chunkReader.ReadUInt16(),
                        ObjectFlags = chunkReader.ReadUInt16(),
                        ParentID = chunkReader.ReadUInt32(),
                        Size = chunkReader.ReadUInt32(),
                        Offset = chunkReader.ReadUInt32(),
                        Template = chunkReader.ReadUInt32(),
                        Crc32 = chunkReader.ReadUInt32()
                    };

                    this.chunks[i] = entry;
                }

                // Build chunk tree
                CR2WExport[] exportList = new CR2WExport[this.chunks.Length];

                // Create all CR2WExport instances
                for (int i=0; i < this.chunks.Length; i++)
                {
                    exportList[i] = new CR2WExport(this, this.stream, this.chunks[i]);

                    // Chunk without parent gets added to export list
                    if (this.chunks[i].ParentID == 0)
                        this.exports.Add(exportList[i]);
                }

                // Map parents
                for (int i = 0; i < this.chunks.Length; i++)
                {
                    if (this.chunks[i].ParentID > 0)
                    {
                        int parentIdx = (int)(this.chunks[i].ParentID - 1);
                        exportList[parentIdx].Children.Add(exportList[i]);
                        exportList[i].Parent = exportList[parentIdx];
                    }
                }
            }

            // Read buffer table
            {
                {
                    byte[] bufferBuffer = new byte[this.tables[5].ItemCount * Marshal.SizeOf<CR2WBufferEntry>()];
                    this.stream.Seek(this.tables[5].Offset, SeekOrigin.Begin);
                    this.stream.Read(bufferBuffer, 0, bufferBuffer.Length);

                    this.buffers = new CR2WBufferEntry[this.tables[5].ItemCount];

                    BinaryReader bufferReader = new BinaryReader(new MemoryStream(bufferBuffer));
                    for (int i = 0; i < this.tables[5].ItemCount; i++)
                    {
                        CR2WBufferEntry entry = new CR2WBufferEntry
                        {
                            Flags = bufferReader.ReadUInt32(),
                            Index = bufferReader.ReadUInt32(),
                            Offset = bufferReader.ReadUInt32(),
                            DiskSize = bufferReader.ReadUInt32(),
                            MemSize = bufferReader.ReadUInt32(),
                            Crc32 = bufferReader.ReadUInt32(),
                        };

                        this.buffers[i] = entry;
                    }
                }
            }
        }

        public static CR2WFile ReadFile(Stream stream)
        {
            CR2WFile file = new CR2WFile
            {
                stream = stream
            };

            file.ReadHeader();

            return file;
        }

        public byte[] ReadBuffer(int idx)
        {
            CR2WBufferEntry entry = this.buffers[idx];

            byte[] data = new byte[entry.MemSize];

            this.stream.Seek(entry.Offset, SeekOrigin.Begin);
            this.stream.Read(data, 0, data.Length);

            return data;
        }
    }
}
