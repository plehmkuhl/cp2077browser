using System;
using System.Collections.Generic;
using System.Text;

namespace CR2WLib
{
    public class CR2WProperty
    {
        CR2WFile file;
        CR2WPropertyEntry entry;

        //string className;
        //string propertyName;

        public static CR2WProperty CreateFromPropertyEntry(CR2WFile file, CR2WPropertyEntry entry)
        {
            return new CR2WProperty
            {
                file = file,
                entry = entry,
            };
        }

        public string Name { get => this.file.CNames[this.entry.PropertyName]; }
        public string ClassName { get => this.file.CNames[this.entry.ClassName]; }
    }
}
