using System;
using System.IO;

namespace Craftitude.Extensions
{
	internal static class FileInfoExtensions
	{
		public static string GetRelativePathFrom(this FileInfo fileInfo, DirectoryInfo baseDirectory)
		{
			return Uri.UnescapeDataString(new Uri(baseDirectory.FullName + Path.DirectorySeparatorChar).MakeRelativeUri(new Uri(fileInfo.FullName)).ToString().Replace('/', Path.DirectorySeparatorChar));
		}
		public static string GetRelativePathFrom(this FileInfo fileInfo, FileInfo baseFile)
		{
			return Uri.UnescapeDataString(new Uri(baseFile.FullName).MakeRelativeUri(new Uri(fileInfo.FullName)).ToString().Replace('/', Path.DirectorySeparatorChar));
		}
		public static string GetRelativePathFrom(this FileInfo fileInfo, string baseDirectoryPath)
		{
			return fileInfo.GetRelativePathFrom(new DirectoryInfo(baseDirectoryPath));
		}
	}
}
