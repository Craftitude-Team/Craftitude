using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using YamlDotNet;
using YamlDotNet.RepresentationModel;
using YamlDotNet.RepresentationModel.Serialization;

namespace Craftitude
{
    public class Package
    {
        DirectoryInfo _directoryInfo;

        public Package(DirectoryInfo packageDirectory)
        {
            _directoryInfo = packageDirectory;

            FileInfo jsonFile = _directoryInfo.GetFile("metadata.json");
            FileInfo yamlFile = _directoryInfo.GetFile("metadata.yml");
            
            if (jsonFile.Exists)
            {
                Serializer se = new Serializer();
                Metadata = JsonConvert.DeserializeObject<PackageMetadata>(File.ReadAllText(_directoryInfo.GetFile("metadata.json").FullName));
                using (var yamlStream = yamlFile.Open(FileMode.OpenOrCreate))
                {
                    using (var yamlWriter = new StreamWriter(yamlStream, Encoding.UTF8))
                    {
                        se.Serialize(yamlWriter, Metadata);
                    }
                }
                jsonFile.Delete();
            }

            Deserializer dse = new Deserializer();
            using (var yamlStream = yamlFile.Open(FileMode.Open))
            {
                using (var yamlReader = new StreamReader(yamlStream, Encoding.UTF8, true))
                {
                    Metadata = dse.Deserialize<PackageMetadata>(yamlReader);
                }
            }
        }

        public string Path { get { return _directoryInfo.ToString(); } }

        public DirectoryInfo Directory { get { return _directoryInfo; } }

        public PackageMetadata Metadata { get; private set; }

        public void RunTarget(Profile profile, string target)
        {
            RunSteps(profile, Metadata.Targets[target]);
        }

        protected void RunSteps(Profile profile, IEnumerable<SetupStep> steps)
        {
            foreach (var step in steps)
            {
                string[] stepName = step.Name.Split(':');
                switch (stepName[0].ToLower())
                {
                    case "target":
                        if (stepName.Length != 2)
                            throw new InvalidOperationException("Invalid step name syntax. Syntax of target step name is: Name=target:<target name>");
                        RunTarget(profile, stepName[1]);
                        break;
                    case "plugin":
                        if (stepName.Length != 2)
                            throw new InvalidOperationException("Invalid step name syntax. Syntax of plugin step name is: Name=plugin:<setup handler name>");
                                                
                        // Define where to get assemblies from
                        var pluginCatalog = new AggregateCatalog();
                        pluginCatalog.Catalogs.Add(new DirectoryCatalog(".")); // ./*.dll
                        if (System.IO.Directory.Exists("plugins"))
                            pluginCatalog.Catalogs.Add(new DirectoryCatalog("plugins")); // plugins/*.dll
                        if (profile.Directory.GetDirectories("plugins").Any())
                            pluginCatalog.Catalogs.Add(new DirectoryCatalog(profile.Directory.GetDirectories("plugins").Single().FullName)); // <profile>/plugins/*.dll
                        pluginCatalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly())); // this assembly

                        var container = new CompositionContainer(pluginCatalog, true);
                        var setuphelper = container.GetExportedValue<SetupHelper>(stepName[1]);

                        setuphelper._package = this;
                        setuphelper._profile = profile;
                        setuphelper.Run(step.Arguments.ToArray());
                        break;
                }
            }
        }
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
        [YamlAlias("Ad-Url")]
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
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public DependencyType Type { get; set; }

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
        [YamlAlias("Text-Url")]
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
        [YamlAlias("E-Mail")]
        public string Email { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
