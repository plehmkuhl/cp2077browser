using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ArchiveLib.Structures;

namespace ArchiveLib
{
    public class ArchiveTable
    {


        private Archive archive;
        private long offset;
        private uint size;

        private ArchiveTableHeader tableHeader;

        public ArchiveTable(Archive archive, long offset, uint size)
        {
            this.archive = archive;
            this.offset = offset;
            this.size = size;
        }

        public long FileCount { get => this.tableHeader.FileCount; }

        public async Task Read()
        {
            byte[] buffer = new byte[Marshal.SizeOf<ArchiveTableHeader>()];

            {
                FileStream stream = this.archive.File.OpenRead();
                stream.Seek(this.offset, SeekOrigin.Begin);
                await stream.ReadAsync(buffer, 0, buffer.Length);
                stream.Close();
            }

            BinaryReader reader = new BinaryReader(new MemoryStream(buffer));

            ArchiveTableHeader tableHeader = new ArchiveTableHeader();
            tableHeader.Num = reader.ReadUInt32();
            tableHeader.Size = reader.ReadUInt32();
            tableHeader.Checksum = reader.ReadUInt64();
            tableHeader.FileCount = reader.ReadUInt32();
            tableHeader.ChunkCount = reader.ReadUInt32();
            tableHeader.DependencyCount = reader.ReadUInt32();

            if (tableHeader.Num != 8)
                throw new Exception("Unexpected table num");

            this.tableHeader = tableHeader;
        }

        public delegate void FileListingCallback(ArchiveFileInfo file);

        public async Task ListFiles(FileListingCallback callback)
        {
            FileStream stream = this.archive.File.OpenRead();

            // Position stream after header to read file table
            stream.Seek(this.offset + Marshal.SizeOf<ArchiveTableHeader>(), SeekOrigin.Begin);

            // Read file array
            for (int i=0; i < this.tableHeader.FileCount; i++)
            {
                ArchiveFileInfo file = new ArchiveFileInfo(this.archive);
                file.Read(stream);

                callback.Invoke(file);
            }

            stream.Close();
        }

        public ArchiveChunkEntry[] ReadChunkTableRange(Stream stream, uint begin, uint end)
        {
            List<ArchiveChunkEntry> chunks = new List<ArchiveChunkEntry>();

            stream.Seek(this.offset + Marshal.SizeOf<ArchiveTableHeader>() + (this.tableHeader.FileCount * Marshal.SizeOf<ArchiveFileEntry>()) + (Marshal.SizeOf<ArchiveChunkEntry>() * begin), SeekOrigin.Begin);
            BinaryReader reader = new BinaryReader(stream);

            for (uint i=begin; i < end; i++)
            {
                ArchiveChunkEntry entry = new ArchiveChunkEntry();
                entry.offset = reader.ReadInt64();
                entry.compressedSize = reader.ReadInt32();
                entry.originalSize = reader.ReadInt32();

                chunks.Add(entry);
            }

            return chunks.ToArray();
        }
    }
}
