using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CR2WLib.Types.Special
{
    [CR2WType("!NOTIMPLEMENTED!", Type: typeof(string))]
    public class CNotImplemented : CR2WValue
    {
        public override object InternalRepresentation { get => $"Not implemented type: {this.FullTypeName}"; }

        public override void Read(BinaryReader reader)
        {
        }
    }
}
