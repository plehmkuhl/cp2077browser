using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;
using ArchiveLib.Structures;

namespace ArchiveLib
{
    public class Archive
    {
        private FileInfo archive;

        private ArchiveHeader header;
        private ArchiveTable table;


        public Archive(FileInfo file) {
            this.archive = file;
        }

        public FileInfo File { get => this.archive; }
        public ArchiveTable Table { get => this.table; }
        public long FileCount { get => this.table.FileCount; }

        public delegate void FileListingCallback(ArchiveFileInfo file);

        public async Task Read()
        {
            byte[] buffer = new byte[Marshal.SizeOf<ArchiveHeader>()];

            {
                FileStream stream = this.archive.OpenRead();
                await stream.ReadAsync(buffer, 0, buffer.Length);
                stream.Close();
            }

            BinaryReader reader = new BinaryReader(new MemoryStream(buffer));

            ArchiveHeader header = new ArchiveHeader();
            header.Magic = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(4));
            header.Version = reader.ReadUInt32();
            header.TableOffset = reader.ReadInt64();
            header.TableSize = reader.ReadUInt32();
            reader.BaseStream.Seek(12, SeekOrigin.Current); // Skip unknown bytes
            header.FileSize = reader.ReadUInt64();

            if (header.Magic != "RDAR")
                throw new Exception("Invalid archive file");

            if (header.Version != 12)
                throw new Exception("Unexpected archive version");

            // Read table
            ArchiveTable table = new ArchiveTable(this, header.TableOffset, header.TableSize);
            await table.Read();

            this.header = header;
            this.table = table;
        }

        public ArchiveFileInfo SearchFile(UInt64 hash)
        {
            return this.table.SearchFile(this.File.OpenRead(), hash);
        }

        public async Task ListFilesAsync(FileListingCallback callback)
        {
            await Task.Factory.StartNew(() =>
            {
                this.table.ListFiles((ArchiveFileInfo file) =>
                {
                    callback.Invoke(file);
                });
            });
        }
    }
}
