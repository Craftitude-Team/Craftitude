using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace Craftitude.Plugins
{
    public class Http
    {
        public static string Download(string remoteFile)
        {
            using (var wc = new WebClient())
            {
                string localFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                wc.DownloadFile(remoteFile, localFile);
                return localFile;
            }
        }
    }
}
