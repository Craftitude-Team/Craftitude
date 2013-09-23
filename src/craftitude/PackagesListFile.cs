using System;
using System.Collections.Generic;

namespace Craftitude
{
    [Serializable]
    public class PackagesListFile
    {
        public ulong Version { get; set; }

        public IEnumerable<PackageMetadata> Packages { get; set; }
    }
}