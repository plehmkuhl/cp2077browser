using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CR2WLib
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class CR2WTableEntry
    {
        public uint Offset;
        public uint ItemCount;
        public uint Crc32;
    }
}
