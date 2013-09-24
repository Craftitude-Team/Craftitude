using System;
using System.IO;
using System.Linq;

namespace Craftitude.Extensions
{
    namespace IO
    {
        internal static class DirectoryInfoExtensions
        {
            public static string GetRelativePathFrom(this DirectoryInfo directoryInfo, DirectoryInfo baseDirectory)
            {
                return Uri.UnescapeDataString(new Uri(baseDirectory.FullName + Path.DirectorySeparatorChar).MakeRelativeUri(new Uri(directoryInfo.FullName + Path.DirectorySeparatorChar)).ToString().Replace('/', Path.DirectorySeparatorChar));
            }
            public static string GetRelativePathFrom(this DirectoryInfo directoryInfo, FileInfo baseFile)
            {
                return Uri.UnescapeDataString(new Uri(baseFile.FullName).MakeRelativeUri(new Uri(directoryInfo.FullName + Path.DirectorySeparatorChar)).ToString().Replace('/', Path.DirectorySeparatorChar));
            }
            public static string GetRelativePathFrom(this DirectoryInfo directoryInfo, string baseDirectoryPath)
            {
                return directoryInfo.GetRelativePathFrom(new DirectoryInfo(baseDirectoryPath));
            }
            public static FileInfo GetFile(this DirectoryInfo directoryInfo, string searchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly)
            {
                var f = directoryInfo.GetFiles(searchPattern, searchOption);
                if (f.Any())
                    return f.First();
                if (!searchPattern.Contains('?') && !searchPattern.Contains('*'))
                    return new FileInfo(Path.Combine(directoryInfo.FullName + Path.DirectorySeparatorChar, searchPattern));
                return null;
            }
            public static void Copy(this DirectoryInfo dir, DirectoryInfo destinationDir, bool overwrite = true, bool copySubDirs = true)
            {
                if (!dir.Exists)
                {
                    throw new DirectoryNotFoundException(
                        "Source directory does not exist or could not be found."
                        );
                }

                // If the destination directory doesn't exist, create it. 
                if (!destinationDir.Exists)
                    destinationDir.Create();

                // Get the files in the directory and copy them to the new location.
                foreach (FileInfo file in dir.EnumerateFiles())
                {
                    var targetpath = destinationDir.GetFile(file.Name).FullName;
                    if (overwrite || !File.Exists(targetpath))
                        file.CopyTo(targetpath, true);
                }

                // If copying subdirectories, copy them and their contents to new location. 
                if (copySubDirs)
                    foreach (var subdir in dir.EnumerateDirectories())
                        subdir.Copy(destinationDir.CreateSubdirectory(subdir.Name));
            }
        }
    }
}
