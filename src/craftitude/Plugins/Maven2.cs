using System;
using Craftitude.Profile;

namespace Craftitude.Plugins
{
    public class Maven2
    {
        public static string ComposeUrl(string groupId, string artifactId, string versionId, bool natives = false, string repository = "http://repo1.maven.org/maven2/")
        {
            var realpath = groupId.Replace('.', '/') + "/" + artifactId + "/" + versionId + "/" + artifactId + "-" + versionId + (natives ? "-natives-" + (Environment.OSVersion.Platform == PlatformID.Unix ? "linux" : Environment.OSVersion.Platform == PlatformID.MacOSX ? "osx" : "windows") : "") + ".jar";

            var b = new UriBuilder(repository);
            b.Path = b.Path.TrimEnd('/') + "/" + realpath;

            return b.Uri.ToString();
        }

        public static void Install(CraftitudeProfile profile, string groupId, string artifactId, string versionId, string repository = "http://repo1.maven.org/maven2/")
        {
            Java.Install(profile, groupId, artifactId, versionId, Http.Download(ComposeUrl(groupId, artifactId, versionId, false, repository)));
        }

        public static void Uninstall(CraftitudeProfile profile, string groupId, string artifactId, string versionId)
        {
            Java.Uninstall(profile, groupId, artifactId, versionId);
        }
    }
}
