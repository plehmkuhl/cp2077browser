using CR2WLib.Structs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CR2WLib
{
    public class CR2WImport
    {
        private CR2WFile file;
        private CR2WImportEntry entry;

        public CR2WImport(CR2WFile file, CR2WImportEntry entry)
        {
            this.file = file;
            this.entry = entry;
        }

        public string Path { get => this.file.OffsetStrings[this.entry.Path];  }
        public string ClassName { get => this.file.CNames[this.entry.ClassName]; }
        public ushort Flags { get => this.entry.Flags; }
    }
}
