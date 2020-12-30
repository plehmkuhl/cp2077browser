using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CR2WLib.Types.Primitive
{
    [CR2WType("String", Type: typeof(string))]
    public class CString : CR2WValue
    {
        private string value;

        public override object InternalRepresentation { get => this.value; }

        public override void Read(BinaryReader reader)
        {
            byte tag = reader.ReadByte();

            if (tag == 0x80)
            {
                this.value = null;
                return;
            }

            if (tag == 0x00)
            {
                this.value = null;
                return;

                //throw new NotImplementedException();
            }

            int len = tag & 0x3F;
            if ((tag & 0x40) != 0) // Extended string
                len += 64 * reader.ReadByte();

            bool isUnicode = (tag & 0x80) == 0;

            if (isUnicode)
                this.value = System.Text.Encoding.Unicode.GetString(reader.ReadBytes(len * 2));
            else
                this.value = System.Text.Encoding.GetEncoding("ISO-8859-1").GetString(reader.ReadBytes(len));
        }
    }
}
