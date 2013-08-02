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

namespace Craftitude.Plugins.Compression
{
    public class Compression
    {
        public static IArchive OpenArchive(Stream fs, string password = null)
        {
            IArchive _arch;

            if (ZipArchive.IsZipFile(fs))
            {
                fs.Seek(0, SeekOrigin.Begin);
                _arch = (IArchive)ZipArchive.Open(fs, password);
            }
            else if (RarArchive.IsRarFile(fs))
            {
                fs.Seek(0, SeekOrigin.Begin);
                _arch = (IArchive)RarArchive.Open(fs);
            }
            else if (TarArchive.IsTarFile(fs))
            {
                fs.Seek(0, SeekOrigin.Begin);
                _arch = (IArchive)TarArchive.Open(fs);
            }
            else if (SevenZipArchive.IsSevenZipFile(fs))
            {
                fs.Seek(0, SeekOrigin.Begin);
                _arch = (IArchive)SevenZipArchive.Open(fs);
            }
            else if (GZipArchive.IsGZipFile(fs))
            {
                fs.Seek(0, SeekOrigin.Begin);
                _arch = (IArchive)GZipArchive.Open(fs);
            }
            else
            {
                throw new InvalidOperationException("Not a valid archive.");
            }

            return _arch;
        }
    }
}
