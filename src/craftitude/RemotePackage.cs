using Newtonsoft.Json;

namespace Craftitude
{
    public class RemotePackage
    {
        public static RemotePackage FromLocalPackage(Package package)
        {
            return new RemotePackage()
            {
                Repository = null,
                Package = package,
                Metadata = package.Metadata
            };
        }

        public PackageMetadata Metadata { get; set; }

        [JsonIgnore()]
        public Package Package { get; set; }

        public Repository Repository { get; set; }
    }
}