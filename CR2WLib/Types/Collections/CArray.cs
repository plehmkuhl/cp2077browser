using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CR2WLib.Types.Collections
{
    [CR2WType("array", Type: typeof(CR2WValue[]))]
    public class CArray : CR2WValue
    {
        private CR2WValue[] values;

        public override object InternalRepresentation { get => values; }
        public override bool IsContainerType { get => true; }

        public override void Read(BinaryReader reader)
        {
            int elements = reader.ReadInt32();
            string[] subtype = new string[this.FullType.Length - 1];
            for (int i = 1; i < this.FullType.Length; i++)
                subtype[i - 1] = this.FullType[i];

            this.values = new CR2WValue[elements];

            for (int i=0; i < elements; i++)
                this.values[i] = CR2WValue.ReadValue(this.File, subtype, reader);
        }
    }
}
