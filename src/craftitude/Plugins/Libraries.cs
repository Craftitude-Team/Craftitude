using System;

namespace Craftitude.Plugins
{
    public static class Libraries
    {
        public static void Autoload(Profile profile, string path, double weight = 50)
        {
            path = new Uri(path).MakeRelativeUri(new Uri(profile.Directory.FullName)).ToString();
            profile.ProfileInfo.NativePaths.Add(new PathEntry(path, weight));
        }

        public static void UnAutoload(Profile profile, string path)
        {
            path = new Uri(path).MakeRelativeUri(new Uri(profile.Directory.FullName)).ToString();
            profile.ProfileInfo.NativePaths.RemoveAll(p => p.Path == path);
        }
    }
}
