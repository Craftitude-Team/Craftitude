using Craftitude.Repositories;

namespace Craftitude.Profile
{
    public class InstalledPackageInfo : PackageInfo
    {
        public InstalledPackageState State { get; internal set; }
    }
}
