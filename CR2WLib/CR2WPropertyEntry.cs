using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CR2WLib
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CR2WPropertyEntry
    {
        public ushort ClassName;
        public ushort ClassFlags;
        public ushort PropertyName;
        public ushort PropertyFlags;
        public ulong Hash;
    };
}
