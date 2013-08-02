namespace System
{
    using Linq;
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
                return directoryInfo.GetFiles(searchPattern, searchOption).First();
            }
        }
    }
}
