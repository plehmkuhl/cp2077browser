using CR2WLib.Types.Primitive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CR2WLib.Types.References
{
    [CR2WType("handle", Type: typeof(CR2WExport))]
    public class CHandle : CR2WValue
    {
        private CR2WExport value;

        public override object InternalRepresentation { get => this.value; }
        public override bool IsContainerType { get => true; }

        public override void Read(BinaryReader reader)
        {
            int chunkIndex = reader.ReadInt32();
            if (chunkIndex > 0) {
                this.value = this.File.Exports[chunkIndex - 1];
            } else if (chunkIndex < 0) {
                Console.WriteLine("Handle target into import list not supported!");
                this.value = null;
            } else {
                this.value = null;
            }
        }
    }
}
