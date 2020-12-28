using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CR2WLib.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CR2WChunkEntry
    {
        public ushort ClassName;
        public ushort ObjectFlags;
        public uint ParentID;
        public uint Size;
        public uint Offset;
        public uint Template;
        public uint Crc32;
    };
}
