using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CR2WLib.Types.Primitive
{
    [CR2WType("CProperty", Type: typeof(string))]
    public class CProperty : CR2WValue
    {
        private CR2WValue value;
        private string name;
        private string valueType;
        private int size;

        public string Name => this.name;
        public CR2WValue Value => this.value;

        public override object InternalRepresentation => this.value.InternalRepresentation;

        public override void Read(BinaryReader reader)
        {
            uint nameIdx = reader.ReadUInt16();
            if (this.File.CNames.Length <= nameIdx)
                throw new FormatException();

            if (nameIdx == 0) // Empty property
                return;

            uint typeIdx = reader.ReadUInt16();
            if (this.File.CNames.Length <= typeIdx)
                throw new FormatException();

            this.name = this.File.CNames[nameIdx];
            this.valueType = this.File.CNames[typeIdx];
            this.size = reader.ReadInt32();

            long endPosition = reader.BaseStream.Position + this.size - 4;

            try {
                if (this.size > 4)
                    this.value = CR2WValue.ReadValue(this.File, this.valueType, reader);
            } catch (TypeAccessException e) {
                Console.Error.WriteLine($"Failed to read proprty {this.name}: {e.ToString()}");
                throw;
            } finally {
                reader.BaseStream.Seek(endPosition, SeekOrigin.Begin);
            }
        }
    }
}
