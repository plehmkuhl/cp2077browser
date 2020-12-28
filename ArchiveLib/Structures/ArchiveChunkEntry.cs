using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ArchiveLib.Structures
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ArchiveChunkEntry
    {
        public long offset;
        public int compressedSize;
        public int originalSize;
    };
}
