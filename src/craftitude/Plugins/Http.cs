using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.IO;

namespace Craftitude.Plugins
{
    public class Http
    {
        public static string Download(string remoteFile, Dictionary<string, string> customheaders = null)
        {
            /*
            var headers = new Dictionary<string, string>()
            {
                { "cache-control", "no-store,max-age=0,no-cache" },
                { "expires", "0" },
                { "pragma", "no-cache" }
            };
             */
            var headers = new Dictionary<string, string>();
            if (customheaders != null)
                foreach (var item in customheaders)
                {
                    if (headers.ContainsKey(item.Key.ToLower()))
                        headers[item.Key.ToLower()] = item.Value;
                    else
                        headers.Add(item.Key.ToLower(), item.Value);
                }
            using (var wc = new WebClient())
            {
                var localFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

                // TODO: Implement proxy support. Disabled for now to speed up S3 downloads.
                wc.Proxy = null;
                wc.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                Debug.WriteLine("Downloading {0} to {1}...", remoteFile, localFile);
                wc.DownloadFile(remoteFile, localFile);
                return localFile;
            }
        }
    }
}
