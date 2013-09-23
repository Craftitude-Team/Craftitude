using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using YamlDotNet.RepresentationModel.Serialization;

namespace Craftitude
{
    [Serializable]
    public class PackageMetadata
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("license")]
        public PackageLicense License { get; set; }

        [JsonProperty("ad-url")]
        [YamlAlias("Ad-Url")]
        public string AdUrl { get; set; }

        [JsonProperty("maintainers")]
        public IEnumerable<Person> Maintainers { get; set; }

        [JsonProperty("developers")]
        public IEnumerable<Person> Developers { get; set; }

        [JsonProperty("subscriptions", Required = Required.Always)]
        public IEnumerable<string> Subscriptions { get; set; }

        [JsonProperty("platforms", Required = Required.Always)]
        public IEnumerable<string> Platforms { get; set; }

        [JsonProperty("version", Required = Required.Always)]
        public PackageVersion Version { get; set; }

        [JsonProperty("date", Required = Required.Always)]
        public DateTime Date { get; set; }

        [JsonProperty("dependencies", Required = Required.Always)]
        public IEnumerable<Dependency> Dependencies { get; set; }

        [JsonProperty("targets", Required = Required.Always)]
        public Dictionary<string, IEnumerable<SetupStep>> Targets { get; set; }

    }
}