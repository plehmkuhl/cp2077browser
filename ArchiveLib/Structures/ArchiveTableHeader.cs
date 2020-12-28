using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ArchiveLib.Structures
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ArchiveTableHeader
    {
        public uint Num;
        public uint Size;
        public ulong Checksum;
        public uint FileCount;
        public uint ChunkCount;
        public uint DependencyCount;
    };
}
