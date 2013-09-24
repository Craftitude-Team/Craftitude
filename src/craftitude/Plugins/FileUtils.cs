using System.IO;
using Craftitude.Extensions.IO;

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
