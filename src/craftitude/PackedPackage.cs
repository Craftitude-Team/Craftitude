using System.IO;
using SharpCompress.Archive;
using SharpCompress.Archive.SevenZip;

namespace Craftitude
{
    public class PackedPackage
    {
        public Package Package { get; private set; }

        public PackedPackage(FileInfo fileInfo)
        {
            // Generate temporary directory
            var tempDir = new DirectoryInfo(Path.GetTempPath()).CreateSubdirectory(Path.GetRandomFileName());

            // Extract the package
            if (!SevenZipArchive.IsSevenZipFile(fileInfo))
                throw new InvalidDataException("Not a valid 7-Zip archive. All Craftitude packages need to be packed in the 7-Zip format.");
            using (var szarch = SevenZipArchive.Open(fileInfo))
            {
                szarch.WriteToDirectory(tempDir.FullName);                
            }

            // Create package instance of the freshly unpacked stuff
            Package = new Package(tempDir);
        }

        public PackedPackage(string filePath)
            : this(new FileInfo(filePath))
        {
        }

        ~PackedPackage()
        {
            // Clean up directory.
            // TODO: Make this safe so it ultimately gets called when the package isn't needed anymore.
            Package.Directory.Delete(true);
        }
    }
}