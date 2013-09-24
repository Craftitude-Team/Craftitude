using System.IO;
using SharpCompress.Archive;
using SharpCompress.Common;

namespace Craftitude.Plugins
{
    public static class Compression
    {
        public static IArchive OpenArchive(string file)
        {
            return OpenArchive(File.Open(file, FileMode.Open));
        }

        public static IArchive OpenArchive(Stream fs)
        {
            return ArchiveFactory.Open(fs);
        }

        public static void UnpackAll(IArchive archive, string destinationDirectory, bool overwrite = true)
        {
            Directory.CreateDirectory(destinationDirectory);
            archive.WriteToDirectory(destinationDirectory, (overwrite ? ExtractOptions.Overwrite : ExtractOptions.None) | ExtractOptions.ExtractFullPath);
        }
    }
}
