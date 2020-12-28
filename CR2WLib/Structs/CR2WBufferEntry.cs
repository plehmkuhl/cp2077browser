using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CR2WLib.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CR2WBufferEntry
    {
        public uint Flags;
        public uint Index;
        public uint Offset;
        public uint DiskSize;
        public uint MemSize;
        public uint Crc32;
    };
}
