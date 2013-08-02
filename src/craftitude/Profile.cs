using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Craftitude
{
    public class Profile
    {
        DirectoryInfo _directory;
        List<Tuple<PackageAction, Package>> _pendingPackages = new List<Tuple<PackageAction, Package>>();

        FileInfo _bsonFile;

        public Profile(DirectoryInfo profileDirectory)
        {
            _directory = profileDirectory;
            _bsonFile = profileDirectory.GetFile("profile.bson");

            if (!_bsonFile.Exists)
            {
                ProfileInfo = new ProfileInfo();
            }
            else
            {
                using (var bsonStream = _bsonFile.Open(FileMode.OpenOrCreate))
                {
                    using (var bsonReader = new BsonReader(bsonStream))
                    {
                        var jsonSerializer = new JsonSerializer();
                        ProfileInfo = jsonSerializer.Deserialize<ProfileInfo>(bsonReader);
                    }
                }
            }
        }

        public ProfileInfo ProfileInfo { get; private set; }

        public DirectoryInfo Directory { get { return _directory; } }

        public string Path { get { return _directory.ToString(); } }

        public void AppendPackage(Package package, PackageAction action)
        {
            if (_pendingPackages.Any(p => p.Item2 == package))
            {
                DependPackage(package);
            }
            
            _pendingPackages.Add(new Tuple<PackageAction, Package>(action, package));
        }

        public void DependPackage(Package package)
        {
            _pendingPackages.RemoveAll(p => p.Item2 == package);
        }

        public void RunTasks()
        {
            // Sort by action (uninstall/purge => install/update => configure)
            _pendingPackages.Sort(new Comparison<Tuple<PackageAction, Package>>((a, b) => ((byte)a.Item1).CompareTo((byte)b.Item1)));

            foreach (var item in _pendingPackages)
            {
                PackageAction action = item.Item1;
                Package package = item.Item2;

                switch (action)
                {
                    case PackageAction.Install:
                        if (ProfileInfo.InstalledPackages.Any(p => p.Id == package.Metadata.Id))
                            throw new InvalidOperationException(string.Format("Package {0} already installed.", item.Item2.Metadata.Id));

                        Console.WriteLine("{2} {0} {1}...", package.Metadata.Id, package.Metadata.Version, "Installing");
                        break;
                    case PackageAction.Uninstall:
                        if (!ProfileInfo.InstalledPackages.Any(p => p.Id == package.Metadata.Id))
                            throw new InvalidOperationException(string.Format("Package {0} not installed.", item.Item2.Metadata.Id));

                        Console.WriteLine("{2} {0} {1}...", package.Metadata.Id, package.Metadata.Version, "Uninstalling");
                        break;
                    case PackageAction.Purge:
                        if (!ProfileInfo.InstalledPackages.Any(p => p.Id == package.Metadata.Id))
                            throw new InvalidOperationException(string.Format("Package {0} not installed.", item.Item2.Metadata.Id));

                        Console.WriteLine("{2} {0} {1}...", package.Metadata.Id, package.Metadata.Version, "Purging");
                        break;
                    case PackageAction.Update:
                        if (!ProfileInfo.InstalledPackages.Any(p => p.Id == package.Metadata.Id))
                            throw new InvalidOperationException(string.Format("Package {0} not installed.", item.Item2.Metadata.Id));

                        Console.WriteLine("{2} {0} {1}...", package.Metadata.Id, package.Metadata.Version, "Updating");
                        break;
                    case PackageAction.Configure:
                        if (!ProfileInfo.InstalledPackages.Any(p => p.Id == package.Metadata.Id))
                            throw new InvalidOperationException(string.Format("Package {0} not installed yet, install first.", item.Item2.Metadata.Id));

                        Console.WriteLine("{2} {0} {1}...", package.Metadata.Id, package.Metadata.Version, "Configuring");
                        break;
                }

                package.RunTarget(this, action.ToString().ToLower());
            }
        }

        public void Save()
        {
            using (var bsonStream = _bsonFile.Open(FileMode.OpenOrCreate))
            {
                using (var bsonWriter = new BsonWriter(bsonStream))
                {
                    var jsonSerializer = new JsonSerializer();
                    jsonSerializer.Serialize(bsonWriter, this);
                }
            }
        }
    }

    public enum PackageAction : byte
    {
        Uninstall = 1,
        Purge = 2,
        Install = 3,
        Update = 4,
        Configure = 5
    }

    [Serializable]
    public class ProfileInfo
    {
        static JsonSerializer _jsonSerializer = new JsonSerializer();

        static ProfileInfo()
        {
            _jsonSerializer.Converters.Add(new IsoDateTimeConverter());
        }

        public ProfileInfo()
        {
            ProfileName = string.Empty;
            MinecraftVersion = string.Empty;
            MinecraftVersionType = "release";
            TweakClasses = new List<string>();
            InstalledPackages = new List<PackageMetadata>();
            LastUpdateTime = DateTime.MinValue;
        }

        #region Serialization stuff

        public static ProfileInfo FromFile(string file)
        {
            using (var stream = File.OpenRead(file))
            {
                return FromStream(stream);
            }
        }

        public static ProfileInfo FromStream(Stream stream)
        {
            BsonReader reader = new BsonReader(stream);
            return _jsonSerializer.Deserialize<ProfileInfo>(reader);
        }

        public void ToStream(Stream stream)
        {
            BsonWriter writer = new BsonWriter(stream);
            _jsonSerializer.Serialize(writer, this);
        }

        public void ToFile(string file)
        {
            using (var stream = File.Open(file, FileMode.OpenOrCreate))
            {
                ToStream(stream);
            }
        }

        #endregion

        public string ProfileName { get; set; }

        public string MinecraftVersion { get; set; }

        public string MinecraftVersionType { get; set; }

        public List<string> TweakClasses { get; set; }

        public List<PackageMetadata> InstalledPackages { get; set; }

        public DateTime LastUpdateTime { get; set; }
    }
}
