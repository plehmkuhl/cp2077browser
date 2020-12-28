using System;
using System.Collections.Generic;
using System.Text;

namespace ArchiveLib
{
    public interface IArchiveHashResolver
    {
        void Initialize();
        String ResolveFilename(UInt64 hash);
    }
}
