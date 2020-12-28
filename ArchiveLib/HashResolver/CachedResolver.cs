using System;
using System.Collections.Generic;
using System.Text;

namespace ArchiveLib.HashResolver
{
    class CachedResolver : IArchiveHashResolver
    {
        public void Initialize()
        {
        }

        public void RegisterFilename(ulong hash, String filename)
        {

        }
        public string ResolveFilename(ulong hash)
        {
            return "";
        }
    }
}
