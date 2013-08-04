using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Craftitude.Plugins
{
    public static class AmazonS3
    {
        public static void DownloadBucket(string url, string targetPath)
        {
            Dictionary<string, string> etags = null;

            DirectoryInfo di = new DirectoryInfo(targetPath);

            using (var s = di.GetFile("bucket.bson").Open(FileMode.OpenOrCreate))
            {
                using (BsonReader br = new BsonReader(s))
                {
                    JsonSerializer se = new JsonSerializer();
                    etags = se.Deserialize<Dictionary<string, string>>(br);
                    if (etags == null)
                        etags = new Dictionary<string, string>();
                }
            }

            Console.WriteLine("Loading S3 bucket...");
            XDocument doc = XDocument.Load(url);

            foreach (var content in doc.Descendants("Contents"))
            {
                // Key as relative path
                string relpath = content.Element("Key").Value;

                // Compare etags
                string etag = content.Element("ETag").Value.Trim('"');
                if (!etags.ContainsKey(relpath))
                    etags.Add(relpath, null);
                if (etags[relpath] == etag)
                    continue; // File or directory not changed

                // Local path
                string rellpath = relpath.Replace('/', Path.DirectorySeparatorChar);

                // Check if directory
                if (relpath.EndsWith("/")) // is a directory
                    Directory.CreateDirectory(rellpath);
                else
                {
                    // Download file
                    FileInfo fi = new FileInfo(Path.Combine(di.FullName + Path.DirectorySeparatorChar, rellpath));
                    if (!fi.Directory.Exists)
                        fi.Directory.Create();

                    UriBuilder b = new UriBuilder(url);
                    b.Path = b.Path.TrimEnd('/') + "/" + relpath;

                    Console.WriteLine("Downloading from S3 bucket: {0}", relpath);
                    File.Move(Http.Download(b.Uri.ToString()), fi.FullName);

                    etags[relpath] = etag;
                }
            }

            using (var s = di.GetFile("bucket.bson").Open(FileMode.OpenOrCreate))
            {
                using (BsonWriter bw = new BsonWriter(s))
                {
                    JsonSerializer se = new JsonSerializer();
                    se.Serialize(bw, etags);
                }
            }
        }
    }
}
