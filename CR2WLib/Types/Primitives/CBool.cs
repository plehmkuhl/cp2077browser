using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CR2WLib.Types.Primitives
{
    [CR2WType("Bool", Type: typeof(bool))]
    public class CBool : CR2WValue
    {
        private bool value;

        public override object InternalRepresentation { get => this.value; }

        public override void Read(BinaryReader reader)
        {
            this.value = reader.ReadBoolean();
        }
    }
}
