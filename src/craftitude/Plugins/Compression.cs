using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SharpCompress;
using SharpCompress.Archive;
using SharpCompress.Archive.SevenZip;
using SharpCompress.Archive.Rar;
using SharpCompress.Archive.GZip;
using SharpCompress.Archive.Tar;
using SharpCompress.Archive.Zip;
using SharpCompress.Common;
using SharpCompress.Common.Zip;
using SharpCompress.IO;
using SharpCompress.Reader;
using SharpCompress.Writer;

namespace Craftitude.Plugins
{
    public static class Compression
    {
        public static IArchive OpenArchive(string file)
        {
            Console.WriteLine("Compression: Opening archive {0}...", file);
            var fs = File.Open(file, FileMode.Open);
            return OpenArchive(fs);
        }

        public static IArchive OpenArchive(Stream fs)
        {
            Console.WriteLine("Compression: Opening stream...");

            IArchive _arch = ArchiveFactory.Open(fs);

            /*

            if (ZipArchive.IsZipFile(fs))
            {
                fs.Seek(0, SeekOrigin.Begin);
                _arch = (IArchive)ZipArchive.Open(fs, password);
                Console.WriteLine("Compression: Zip archive detected.");
            }
            else if (RarArchive.IsRarFile(fs))
            {
                fs.Seek(0, SeekOrigin.Begin);
                _arch = (IArchive)RarArchive.Open(fs);
                Console.WriteLine("Compression: Rar archive detected.");
            }
            else if (TarArchive.IsTarFile(fs))
            {
                fs.Seek(0, SeekOrigin.Begin);
                _arch = (IArchive)TarArchive.Open(fs);
                Console.WriteLine("Compression: Tar archive detected.");
            }
            else if (SevenZipArchive.IsSevenZipFile(fs))
            {
                fs.Seek(0, SeekOrigin.Begin);
                _arch = (IArchive)SevenZipArchive.Open(fs);
                Console.WriteLine("Compression: 7-zip archive detected.");
            }
            else if (GZipArchive.IsGZipFile(fs))
            {
                fs.Seek(0, SeekOrigin.Begin);
                _arch = (IArchive)GZipArchive.Open(fs);
                Console.WriteLine("Compression: GZip archive detected.");
            }
            else
            {
                throw new InvalidOperationException("Not a valid archive.");
            }
             */

            return _arch;
        }

        public static void UnpackAll(IArchive archive, string destinationDirectory, bool overwrite = true)
        {
            Console.WriteLine("Compression: Unpacking archive...");
            Directory.CreateDirectory(destinationDirectory);
            archive.WriteToDirectory(destinationDirectory, (overwrite ? ExtractOptions.Overwrite : ExtractOptions.None) | ExtractOptions.ExtractFullPath);
        }
    }
}
