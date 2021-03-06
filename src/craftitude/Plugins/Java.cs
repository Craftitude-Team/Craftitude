﻿using System.Linq;
using System.IO;
using Craftitude.Extensions.IO;
using Craftitude.Profile;

namespace Craftitude.Plugins
{
    public class Java
    {
        public static void Install(CraftitudeProfile profile, string groupId, string artifactId, string version, string jarFile, bool copy = false)
        {
            var javaDirectory = profile.Directory.CreateSubdirectory("java");
            javaDirectory = groupId.Split('.').Aggregate(javaDirectory, (current, groupPart) => current.CreateSubdirectory(groupPart));
            javaDirectory = javaDirectory.CreateSubdirectory(artifactId);
            javaDirectory = javaDirectory.CreateSubdirectory(version);

            var targetJarPath = javaDirectory.GetFile(string.Format("{0}-{1}.jar", artifactId, version)).FullName;

            if (copy)
                File.Copy(jarFile, targetJarPath, true);
            else
            {
                if(File.Exists(targetJarPath))
                    File.Delete(targetJarPath);
                File.Move(jarFile, targetJarPath);
            }
        }

        public static void Uninstall(CraftitudeProfile profile, string groupId, string artifactId, string version)
        {
            var javaDirectory = profile.Directory.CreateSubdirectory("java");
            javaDirectory = groupId.Split('.').Aggregate(javaDirectory, (current, groupPart) => current.CreateSubdirectory(groupPart));
            javaDirectory = javaDirectory.CreateSubdirectory(artifactId);
            javaDirectory = javaDirectory.CreateSubdirectory(version);

            var targetJarPath = javaDirectory.GetFile(string.Format("{0}-{1}.jar", artifactId, version)).FullName;

            // First, delete the jar itself.
            File.Delete(targetJarPath);

            // Delete all now empty directories.
            while (javaDirectory != null && !javaDirectory.EnumerateFiles().Any())
            {
                javaDirectory.Delete();
                javaDirectory = javaDirectory.Parent;
            }
        }

        public static void Autoload(CraftitudeProfile profile, string groupId, string artifactId, string version, double weight = 50)
        {
            profile.ProfileInfo.Libraries.Add(new PathLibraryEntry(groupId, artifactId, version, weight));
        }

        public static void UnAutoload(CraftitudeProfile profile, string groupId, string artifactId, string version)
        {
            profile.ProfileInfo.Libraries.RemoveAll(e => e.GroupId == groupId && e.ArtifactId == artifactId && e.VersionId == version);
        }
    }
}
