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
        public class PackageInfo
        {
            public string Id { get; private set; }
            public IEnumerable<string> Versions { get; private set; } 
        }

        private WebClient wc = new WebClient();

        [JsonProperty("uri")]
        public Uri Uri { get; private set; }

        [JsonProperty("subscriptions")]
        public List<string> Subscriptions { get; private set; }

        private List<PackageInfo> _packageInfos = new List<PackageInfo>();
 
        public Repository(Uri uri, IEnumerable<string> subscriptions)
        {
            Uri = uri;
            Subscriptions = subscriptions.ToList();
        }

        public void Update()
        {
            Console.Write("Fetching available packages for {0}...", Uri);
            try
            {
                using (var yamlStream = wc.OpenRead(new Uri(Uri, "packages.yml")))
                {
                    if (yamlStream != null)
                        using (var yamlStreamReader = new StreamReader(yamlStream))
                            _packageInfos = new Deserializer().Deserialize<List<PackageInfo>>(yamlStreamReader);
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
