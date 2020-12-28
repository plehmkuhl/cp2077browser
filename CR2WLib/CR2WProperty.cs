using System;
using System.Collections.Generic;
using System.Text;

namespace CR2WLib
{
    public class CR2WProperty
    {
        string className;
        string propertyName;

        public static CR2WProperty CreateFromPropertyEntry(CR2WFile file, CR2WPropertyEntry entry)
        {
            return new CR2WProperty
            {
                className = file.IndexedStrings[entry.ClassName],
                propertyName = file.IndexedStrings[entry.PropertyName],
            };
        }
    }
}
