using System;
using System.IO;

namespace Craftitude
{
    public class Repository
    {
        static Repository()
        {
            // Dirty, but okay. Nevermind. -_-
        }

        public static Repository GetLocalRepositories(Package package, DirectoryInfo directory)
        {
            UriBuilder b = new UriBuilder();
            b.Scheme = "file";
            b.Path = directory.FullName;
            //return new Repository() { Uri = b.Uri, Subscription = package.Metadata.Subscriptions };
            return null;
        }

        public Uri Uri { get; set; }

        public string Subscription { get; set; }
    }
}