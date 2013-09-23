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
using Newtonsoft.Json;
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

    //[Flags]
}
