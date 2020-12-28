using ArchiveLib.Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveLib
{
    public class ArchiveFileInfo
    {
        private Archive ar;
        private ArchiveFileEntry fileHeader;

        private long fileLength;

        private string resolvedName;

        public ArchiveFileInfo(Archive ar)
        {
            this.ar = ar;
        }

        public ulong NameHash
        {
            get { return this.fileHeader.NameHash; }
        }

        public string Name
        {
            get { 
                if (this.resolvedName != "") {
                    return this.resolvedName;
                } else {
                    return this.fileHeader.NameHash.ToString();
                }
            }
        }

        public bool IsNamed
        {
            get { return this.resolvedName != ""; }
        }

        public DateTime Filetime
        {
            get { return new DateTime(this.fileHeader.Filetime, DateTimeKind.Utc);  }
        }

        public async Task Read(Stream stream)
        {
            byte[] buffer = new byte[Marshal.SizeOf<ArchiveFileEntry>()];
            stream.Read(buffer, 0, buffer.Length);

            BinaryReader reader = new BinaryReader(new MemoryStream(buffer));

            ArchiveFileEntry fileHeader = new ArchiveFileEntry();
            fileHeader.NameHash = reader.ReadUInt64();
            fileHeader.Filetime = reader.ReadInt64();
            fileHeader.Flags = reader.ReadUInt32();
            fileHeader.firstChunkIdx = reader.ReadUInt32();
            fileHeader.lastChunkIdx = reader.ReadUInt32();
            fileHeader.firstDependencyIdx = reader.ReadUInt32();
            fileHeader.lastDependencyIdx = reader.ReadUInt32();

            this.fileHeader = fileHeader;

            try
            {
                this.resolvedName = ArchiveManager.ResolveFileHash(this.fileHeader.NameHash);
            } catch (Exception) {}
        }

        public ArchiveFile OpenRead()
        {
            return new ArchiveFile(this.ar, this, this.fileHeader);
        }
    }
}
