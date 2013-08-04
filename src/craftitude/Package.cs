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
using SharpCompress;
using SharpCompress.Archive;
using SharpCompress.Archive.SevenZip;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using YamlDotNet;
using YamlDotNet.RepresentationModel;
using YamlDotNet.RepresentationModel.Serialization;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel.Serialization.NamingConventions;
using YamlDotNet.RepresentationModel.Serialization.NodeDeserializers;
using YamlDotNet.RepresentationModel.Serialization.NodeTypeResolvers;

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
            //Console.WriteLine("Reading YAML: {0}", yamlFile.FullName);
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

    public class PackedPackage
    {
        RemotePackage _package;
        public RemotePackage Package { get { return _package; } }

        public PackedPackage(FileInfo fileInfo)
        {
            // Generate temporary directory
            var tempDir = new DirectoryInfo(Path.GetTempPath()).CreateSubdirectory(Path.GetRandomFileName());

            // Extract the package
            if (!SevenZipArchive.IsSevenZipFile(fileInfo))
                throw new InvalidDataException("Not a valid 7-Zip archive. All Craftitude packages need to be packed in the 7-Zip format.");
            using (var szarch = SevenZipArchive.Open(fileInfo))
            {
                szarch.WriteToDirectory(tempDir.FullName);                
            }

            // Create package instance of the freshly unpacked stuff
            Package package = new Package(tempDir);
            _package = RemotePackage.FromLocalPackage(package);
        }

        public PackedPackage(string filePath)
            : this(new FileInfo(filePath))
        {
        }

        ~PackedPackage()
        {
            // Clean up directory.
            // TODO: Make this safe so it ultimately gets called when the package isn't needed anymore.
            _package.Package.Directory.Delete(true);
        }
    }

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
        public string Version { get; set; }

        [JsonProperty("date", Required = Required.Always)]
        public DateTime Date { get; set; }

        [JsonProperty("dependencies", Required = Required.AllowNull)]
        public IEnumerable<Dependency> Dependencies { get; set; }

        [JsonProperty("targets", Required = Required.Always)]
        public Dictionary<string, IEnumerable<SetupStep>> Targets { get; set; }

    }

    [Serializable]
    public class SetupStep
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("arguments")]
        public IEnumerable<string> Arguments { get; set; }
    }

    [Serializable]
    public class Dependency
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("type", Required = Required.Always)]
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

        [JsonProperty("name", Required = Required.Always)]
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
        [JsonProperty("username", Required = Required.Always)]
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
