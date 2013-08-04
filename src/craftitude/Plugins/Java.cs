using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Craftitude.Plugins
{
    public class Java
    {
        public static void Install(Profile profile, string groupId, string artifactId, string version, string jarFile)
        {
            var javaDirectory = profile.Directory.CreateSubdirectory("java");
            foreach (string groupPart in groupId.Split('.'))
                javaDirectory = javaDirectory.CreateSubdirectory(groupPart);
            javaDirectory = javaDirectory.CreateSubdirectory(artifactId);
            javaDirectory = javaDirectory.CreateSubdirectory(version);

            var targetJarPath = javaDirectory.GetFile(string.Format("{0}-{1}.jar", artifactId, version)).FullName;

            File.Copy(jarFile, targetJarPath, true);
        }

        public static void Uninstall(Profile profile, string groupId, string artifactId, string version)
        {
            var javaDirectory = profile.Directory.CreateSubdirectory("java");
            foreach (string groupPart in groupId.Split('.'))
                javaDirectory = javaDirectory.CreateSubdirectory(groupPart);
            javaDirectory = javaDirectory.CreateSubdirectory(artifactId);
            javaDirectory = javaDirectory.CreateSubdirectory(version);

            var targetJarPath = javaDirectory.GetFile(string.Format("{0}-{1}.jar", artifactId, version)).FullName;

            // First, delete the jar itself.
            File.Delete(targetJarPath);

            // Delete all now empty directories.
            while (!javaDirectory.EnumerateFiles().Any())
            {
                javaDirectory.Delete();
                javaDirectory = javaDirectory.Parent;
            }
        }

        public static void Autoload(Profile profile, string groupId, string artifactId, string version, double weight = 50)
        {
            profile.ProfileInfo.Libraries.Add(new PathLibraryEntry(groupId, artifactId, version, weight));
        }

        public static void UnAutoload(Profile profile, string groupId, string artifactId, string version)
        {
            profile.ProfileInfo.Libraries.RemoveAll(e => e.GroupId == groupId && e.ArtifactId == artifactId && e.VersionId == version);
        }
    }
}
