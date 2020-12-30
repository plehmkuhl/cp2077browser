using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CR2WLib.Types.References
{
    [CR2WType("raRef", Type: typeof(CR2WImport))]
    public class CRaRef : CR2WValue
    {
        private CR2WImport value;

        public override object InternalRepresentation { get => this.value; }

        public override void Read(BinaryReader reader)
        {
            ushort importIdx = reader.ReadUInt16();
            if (importIdx > 0)
                this.value = this.File.Imports[importIdx - 1];
            else
                this.value = null;
        }
    }
}
