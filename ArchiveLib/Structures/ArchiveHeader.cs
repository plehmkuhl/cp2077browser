using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ArchiveLib.Structures
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ArchiveHeader
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)] public string Magic;
        public uint Version;
        public long TableOffset;
        public uint TableSize;
        public uint Unknown1;
        public uint Unknown2;
        public uint Unknown3;
        public ulong FileSize;
    }
}
