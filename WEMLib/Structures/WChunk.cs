using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace WEMLib.Structures
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct WChunk
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)] 
        public string ID;
        public int DataSize;
        public byte[] Data;
    }
}
