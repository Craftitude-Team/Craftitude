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

            XDocument doc = null;
            XNamespace xmlns = "http://s3.amazonaws.com/doc/2006-03-01/";
            Console.Write("Loading S3 bucket... ");
            while (true)
            {
                try
                {
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.Load(url);
                    var xnr = new XmlNodeReader(xdoc);
                    xnr.MoveToContent();
                    doc = XDocument.Load(xnr);
                    Console.WriteLine("found {0} items", doc.Descendants(xmlns + "Contents").Count());
                    if (!doc.Descendants(xmlns + "Contents").Any())
                    {
                        continue;
                    }
                    break;
                }
                catch
                {
                    continue;
                }
            }

            foreach (var content in doc.Descendants(xmlns + "Contents"))
            {
                // Key as relative path
                string relpath = content.Element(xmlns + "Key").Value;

                // Compare etags
                string etag = content.Element(xmlns + "ETag").Value.Trim('"');
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
                    else if (fi.Exists)
                        fi.Delete();

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
