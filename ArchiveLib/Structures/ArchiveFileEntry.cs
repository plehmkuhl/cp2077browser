using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ArchiveLib.Structures
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ArchiveFileEntry
    {
        public ulong NameHash;
        public long Filetime;
        public uint Flags;
        public uint firstChunkIdx;
        public uint lastChunkIdx;
        public uint firstDependencyIdx;
        public uint lastDependencyIdx;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] sha1Hash;
    };
}
