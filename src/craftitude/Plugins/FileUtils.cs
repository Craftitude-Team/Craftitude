using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace Craftitude.Plugins
{
    public class FileUtils
    {
        public static void CopyDirectory(DirectoryInfo src, DirectoryInfo target, bool overwrite = true, bool copySubdirs = true)
        {
            src.Copy(target, overwrite, copySubdirs);
        }
        public static void CopyDirectory(string src, string target, bool overwrite = true, bool copySubdirs = true)
        {
            CopyDirectory(new DirectoryInfo(src), new DirectoryInfo(target), overwrite, copySubdirs);
        }
    }
}
