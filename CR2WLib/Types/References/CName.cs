using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CR2WLib.Types.References
{
    [CR2WType("CName", Type: typeof(string))]
    public class CName : CR2WValue
    {
        private string value;

        public override object InternalRepresentation { get => this.value; }

        public override void Read(BinaryReader reader)
        {
            ushort nameIdx = reader.ReadUInt16();
            this.value = this.File.CNames[nameIdx];
        }
    }
}
