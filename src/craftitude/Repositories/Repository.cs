using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using YamlDotNet.RepresentationModel.Serialization;

namespace Craftitude.Repositories
{
    public class Repository
    {
        private readonly WebClient _wc = new WebClient();

        [JsonProperty("uri")]
        public Uri Uri { get; private set; }

        [JsonProperty("subscriptions")]
        public List<string> Subscriptions { get; private set; }

        public List<PackageInfo> PackageInfos { get; private set; }

        public Repository(Uri uri, IEnumerable<string> subscriptions)
        {
            PackageInfos = new List<PackageInfo>();
            Uri = uri;
            Subscriptions = subscriptions.ToList();
        }

        public void Update()
        {
            Console.Write("Fetching available packages for {0}...", Uri);
            try
            {
                using (var yamlStream = _wc.OpenRead(new Uri(Uri, "packages.yml")))
                {
                    if (yamlStream != null)
                        using (var yamlStreamReader = new StreamReader(yamlStream))
                            PackageInfos = new Deserializer().Deserialize<List<PackageInfo>>(yamlStreamReader);
                    else
                        throw new Exception();
                }
                
                Console.WriteLine(" OK.");
            }
            catch
            {
                Console.WriteLine(" FAIL.");
            }
        }

    }
}
