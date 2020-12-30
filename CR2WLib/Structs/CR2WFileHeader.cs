using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace CR2WLib
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CR2WFileHeader
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)] 
        public string Magic;
        public uint Version;
        public uint Flags;
        public ulong Filetime;
        public uint BuildVersion;
        public uint FileSize;
        public uint BufferSize;
        public uint Crc32;
        public uint ChunkCount;
    };
}
