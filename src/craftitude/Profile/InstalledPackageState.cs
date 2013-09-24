using System;

namespace Craftitude.Profile
{
    [Flags]
    public enum InstalledPackageState
    {
        Installed = 1,
        Configured = 2
    }
}
