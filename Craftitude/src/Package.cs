using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Craftitude
{
    public class Package
    {
        DirectoryInfo _directoryInfo;

        public Package(DirectoryInfo packageDirectory)
        {
            _directoryInfo = packageDirectory;

            // load metadata
            Metadata = JsonConvert.DeserializeObject<PackageMetadata>(File.ReadAllText(_directoryInfo.GetFile("metadata.json").FullName));
        }

        public string Path { get { return _directoryInfo.ToString(); } }

        public PackageMetadata Metadata { get; private set; }
    }

    [Serializable]
    public class PackageMetadata
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("license")]
        public PackageLicense License { get; set; }

        [JsonProperty("ad-url")]
        public string AdUrl { get; set; }

        [JsonProperty("maintainers")]
        public IEnumerable<Person> Maintainers { get; set; }

        [JsonProperty("developers")]
        public IEnumerable<Person> Developers { get; set; }

        [JsonProperty("subscriptions")]
        public IEnumerable<string> Subscriptions { get; set; }

        [JsonProperty("platforms")]
        public IEnumerable<string> Platforms { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("revision")]
        public ulong Revision { get; set; }

        [JsonProperty("dependencies")]
        public IEnumerable<Dependency> Dependencies { get; set; }

        [JsonProperty("targets")]
        public Dictionary<string, IEnumerable<SetupStep>> Targets { get; set; }

    }

    [Serializable]
    public class SetupStep
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("arguments")]
        public IEnumerable<string> Arguments { get; set; }
    }

    [Serializable]
    public class Dependency
    {
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public DependencyType Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("versions")]
        public string Versions { get; set; }
    }

    //[Flags]
    public enum DependencyType : byte
    {
        Suggestion = 1,
        Prerequirement = 2,
        Requirement = 4,
        Incompatibility = 8
    }

    [Serializable]
    public class PackageLicense
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("text-url")]
        public string TextUrl { get; set; }
    }

    [Serializable]
    public class Person
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("realname")]
        public string Realname { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
