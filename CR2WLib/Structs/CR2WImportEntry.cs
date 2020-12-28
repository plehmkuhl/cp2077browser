using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CR2WLib.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CR2WImportEntry
    {
        public uint Path;
        public ushort ClassName;
        public ushort Flags;
    };
}
