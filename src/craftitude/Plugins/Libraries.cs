using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Craftitude.Plugins
{
    public static class Libraries
    {
        public static void Autoload(Profile profile, string path, double weight = 50)
        {
            profile.ProfileInfo.NativePaths.Add(new PathEntry(path, weight));
        }

        public static void UnAutoload(Profile profile, string path)
        {
            profile.ProfileInfo.NativePaths.RemoveAll(p => p.Path == path);
        }
    }
}
