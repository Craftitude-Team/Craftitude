using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
        DirectoryInfo _craftitudeDirectory;
        List<Tuple<PackageAction, RemotePackage>> _pendingPackages = new List<Tuple<PackageAction, RemotePackage>>();

        FileInfo _bsonFile;

        public Profile(DirectoryInfo profileDirectory)
        {
            _directory = profileDirectory;

            _craftitudeDirectory = _directory.CreateSubdirectory("craftitude");
            _craftitudeDirectory.CreateSubdirectory("repositories"); // repository package lists
            _craftitudeDirectory.CreateSubdirectory("packages"); // cached package setups

            _bsonFile = _craftitudeDirectory.GetFile("profile.bson");

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

                        if (ProfileInfo == null)
                            ProfileInfo = new ProfileInfo();

                        // patch InstalledPackages property with cached package setups
                        // path of cached package setups: <profile>/craftitude/packages/<id>
                        foreach (var package in ProfileInfo.InstalledPackages)
                        {
                            package.Package = new Package(_craftitudeDirectory.CreateSubdirectory("packages").CreateSubdirectory(package.Metadata.Id));
                        }
                    }
                }
            }
        }

        public ProfileInfo ProfileInfo { get; private set; }

        public DirectoryInfo Directory { get { return _directory; } }

        public void AppendPackage(Package package, PackageAction action)
        {
            AppendPackage(RemotePackage.FromLocalPackage(package), action);
        }

        public void AppendPackage(RemotePackage package, PackageAction action)
        {
            if (_pendingPackages.Any(p => p.Item2 == package))
            {
                DependPackage(package);
            }
            
            _pendingPackages.Add(new Tuple<PackageAction, RemotePackage>(action, package));
        }

        public void DependPackage(Package package, PackageAction action)
        {
            AppendPackage(RemotePackage.FromLocalPackage(package), action);
        }

        public void DependPackage(RemotePackage package)
        {
            _pendingPackages.RemoveAll(p => p.Item2 == package);
        }

        public IEnumerable<RemotePackage> InstalledPackagesMatch(Dependency dependency)
        {
            Console.WriteLine("Installed packages match test: {0} {1}", dependency.Name, dependency.Versions);
            return MatchPackages(ProfileInfo.InstalledPackages, dependency);
        }

        public bool IsInstalledPackagesMatch(Dependency dependency)
        {
            return InstalledPackagesMatch(dependency).Any();
        }

        private IEnumerable<RemotePackage> GetDependingPackages(RemotePackage package)
        {
            List<RemotePackage> packages = new List<RemotePackage>();
            foreach (var installedPackage in ProfileInfo.InstalledPackages)
            {
                foreach (var dependency in installedPackage.Metadata.Dependencies.Where(d => d.Name.Split('|').Contains(installedPackage.Metadata.Id)))
                {
                    if (InstalledPackagesMatch(dependency).Contains(package))
                    {
                        packages.Add(installedPackage);
                    }
                }
            }
            return packages;
        }

        private IEnumerable<RemotePackage> RemotePackagesMatch(Dependency dependency)
        {
            var remotePackages = GetPackageMetadata(dependency.Name);
            return MatchPackages(remotePackages, dependency);
        }

        private Guid GetRepositoryGuid(Uri repositoryUrl, string subscription)
        {
            UriBuilder b = new UriBuilder(repositoryUrl);
            b.Path = b.Path.TrimEnd('/') + "/" + subscription;
            return GuidUtility.Create(GuidUtility.UrlNamespace, b.Uri.ToString());
        }

        private PackagesListFile GetRepositoryCache(Uri repositoryUrl, string subscription)
        {
            using (var stream = GetCacheFilePath(repositoryUrl, subscription).OpenRead())
            {
                using (var bsonReader = new BsonReader(stream))
                {
                    JsonSerializer s = new JsonSerializer();
                    return s.Deserialize<PackagesListFile>(bsonReader);
                }
            }
        }

        private FileInfo GetCacheFilePath(Uri repositoryUrl, string subscription)
        {
            var repoguid = GetRepositoryGuid(repositoryUrl, subscription);
            return new FileInfo(Path.Combine(this.Directory.FullName + Path.DirectorySeparatorChar, "craftitude", "caches", string.Format("{0}.bson", repoguid.ToString("N"))));
        }

        private IEnumerable<RemotePackage> GetPackageMetadata(string id)
        {
            List<RemotePackage> packages = new List<RemotePackage>();

            foreach (RepositoryConfiguration repoconf in ProfileInfo.Repositories)
            {
                foreach (string subscription in repoconf.Subscriptions)
                {
                    packages.AddRange(
                        GetRepositoryCache(repoconf.Uri, subscription)
                        .Packages
                        .Where(p => p.Id == id)
                        .Select(p => new RemotePackage() { Metadata = p, Repository = new Repository() { Uri = repoconf.Uri, Subscription = subscription } })
                        );
                }
            }

            return packages;
        }

        private IEnumerable<RemotePackage> MatchPackages(IEnumerable<RemotePackage> packages, Dependency dependency)
        {
            // Filter out by ID
            packages = packages.Where(p => dependency.Name.Split('|').Contains(p.Metadata.Id)).AsEnumerable();
            var remotePackages = new List<RemotePackage>();
            foreach(string depName in dependency.Name.Split('|'))
                remotePackages.AddRange(GetPackageMetadata(depName)); //.GroupBy(rp => { var b = new UriBuilder(rp.Repository.Uri); b.Path = b.Path.TrimEnd('/') + rp.Repository.Subscription; return b.Uri; });
            remotePackages = remotePackages.Distinct().ToList();

            // Filter out by version
            var selectedPackages = new List<RemotePackage>();
            var tokens = new char[] { '<', '=', '>', '#' };
            if (string.IsNullOrEmpty(dependency.Versions))
                dependency.Versions = "#^.*$";
            if (dependency.Versions.Trim().Any())
            {
                List<char> versionToken = new List<char>();

                foreach(string version in dependency.Versions.Split(' '))
                {
                    Queue<char> q = new Queue<char>(version.ToCharArray());
                    while (tokens.Contains(q.Peek()))
                    {
                        versionToken.Add(q.Dequeue());
                    }

                    string versionString = new string(q.ToArray());
                    Console.WriteLine("> " + versionString);

                    foreach (var token in versionToken)
                    {
                        switch (token)
                        {
                            case '>':
                                try
                                {
                                    selectedPackages.AddRange(
                                        packages.Where(
                                            p => p.Metadata.Date > remotePackages
                                                //.Where(rp => rp.Repository.Equals(p.Repository))
                                                .Single(rp => rp.Metadata.Version.Equals(versionString))
                                                .Metadata.Date));
                                }
                                catch
                                {
                                    // TODO: Handle missing packages on all repositories.
                                }
                                break;
                            case '<':
                                try
                                {
                                    selectedPackages.AddRange(
                                        packages.Where(
                                            p => p.Metadata.Date < remotePackages
                                                //.Where(rp => rp.Repository.Equals(p.Repository))
                                                .Single(rp => rp.Metadata.Version.Equals(versionString))
                                                .Metadata.Date));
                                }
                                catch
                                {
                                    // TODO: Handle missing packages on all repositories.
                                }
                                break;
                            case '=':
                                try
                                {
                                    selectedPackages.AddRange(
                                        packages.Where(
                                            p => p.Metadata.Date == remotePackages
                                                //.Where(rp => rp.Repository.Equals(p.Repository))
                                                .Single(rp => rp.Metadata.Version.Equals(versionString))
                                                .Metadata.Date));
                                }
                                catch
                                {
                                    // TODO: Handle missing packages on all repositories.
                                }
                                break;
                            case '#':
                                if (versionToken.Count > 1)
                                    throw new InvalidOperationException("Can't combine regex with other comparison types.");
                                
                                selectedPackages.AddRange(
                                    packages.Where(
                                        p => Regex.IsMatch(p.Metadata.Version, versionString)));
                                break;
                        }
                    }
                }
            }
            
            return remotePackages;
        }

        private bool IsInstalled(RemotePackage package)
        {
            return ProfileInfo.InstalledPackages.Any(p => p.Metadata.Id == package.Metadata.Id && p.Metadata.Version == package.Metadata.Version && p.Repository.Equals(package.Repository));
        }

        public void RunTasks()
        {
            // Sort by action (uninstall/purge => install/update => configure)
            _pendingPackages.Sort(new Comparison<Tuple<PackageAction, RemotePackage>>((a, b) => ((byte)a.Item1).CompareTo((byte)b.Item1)));
            var pending = new Queue<Tuple<PackageAction, RemotePackage>>(_pendingPackages);
            _pendingPackages.Clear();

            while (pending.Any())
            {
                var item = pending.Dequeue();
                var action = item.Item1;
                var package = item.Item2;

                switch (action)
                {
                    case PackageAction.Install:
                        if (ProfileInfo.InstalledPackages.Any(p => p.Metadata.Id == package.Metadata.Id))
                            throw new InvalidOperationException(string.Format("Package {0} already installed.", item.Item2.Metadata.Id));

                        foreach (var dep in package.Metadata.Dependencies)
                        {
                            Console.WriteLine("Analyzing {0}: {1}", dep.Type.ToString().ToLower(), dep.Name + " " + dep.Versions);
                            switch (dep.Type)
                            {
                                case DependencyType.Prerequirement:
                                    if (!IsInstalledPackagesMatch(dep))
                                    {
                                        throw new InvalidOperationException(string.Format("Package {0} needs dependency {1} (versions {2}) to be pre-installed. You need to install the dependency before installing this package.", item.Item2.Metadata.Id, dep.Name, dep.Versions));
                                    }
                                    break;
                                case DependencyType.Requirement:
                                    if (!IsInstalledPackagesMatch(dep) && !MatchPackages(_pendingPackages.Select(t => t.Item2), dep).Any())
                                    {
                                        throw new InvalidOperationException(string.Format("Package {0} needs dependency {1} (versions {2}) to be installed with it. Append the dependency before installing this package.", item.Item2.Metadata.Id, dep.Name, dep.Versions));
                                    }
                                    break;
                                case DependencyType.Incompatibility:
                                    if (IsInstalledPackagesMatch(dep) && MatchPackages(_pendingPackages.Select(t => t.Item2), dep).Any())
                                    {
                                        throw new InvalidOperationException(string.Format("Package {0} is incompatible with {1} (versions {2}). Remove the incompatible package before installing this package.", item.Item2.Metadata.Id, dep.Name, dep.Versions));
                                    }
                                    break;
                            }
                        }

                        Console.WriteLine("{2} {0} {1}...", package.Metadata.Id, package.Metadata.Version, "Installing");
                        break;
                    case PackageAction.Uninstall:
                        if (!ProfileInfo.InstalledPackages.Any(p => p.Metadata.Id == package.Metadata.Id && p.Metadata.Version == package.Metadata.Version))
                            throw new InvalidOperationException(string.Format("Package {0} not installed.", item.Item2.Metadata.Id));
                        if (GetDependingPackages(package).Any())
                            throw new InvalidOperationException(string.Format("Some of the installed packages depend on the package {0} which is marked to be uninstalled. Uninstall all depending packages first.", item.Item2.Metadata.Id));

                        Console.WriteLine("{2} {0} {1}...", package.Metadata.Id, package.Metadata.Version, "Uninstalling");
                        break;
                    case PackageAction.Purge:
                        if (!ProfileInfo.InstalledPackages.Any(p => p.Metadata.Id == package.Metadata.Id && p.Metadata.Version == package.Metadata.Version))
                            throw new InvalidOperationException(string.Format("Package {0} not installed.", item.Item2.Metadata.Id));
                        if (GetDependingPackages(package).Any())
                            throw new InvalidOperationException(string.Format("Some of the installed packages depend on the package {0} which is marked to be uninstalled. Uninstall all depending packages first.", item.Item2.Metadata.Id));

                        Console.WriteLine("{2} {0} {1}...", package.Metadata.Id, package.Metadata.Version, "Purging");
                        break;
                    case PackageAction.Update:
                        if (!ProfileInfo.InstalledPackages.Any(p => p.Metadata.Id == package.Metadata.Id))
                            throw new InvalidOperationException(string.Format("Package {0} not installed.", item.Item2.Metadata.Id));
                        if (ProfileInfo.InstalledPackages.Any(p => p.Metadata.Id == package.Metadata.Id && p.Metadata.Version == package.Metadata.Version))
                            throw new InvalidOperationException(string.Format("Package {0}, Version {1} already installed.", item.Item2.Metadata.Id, item.Item2.Metadata.Version));

                        Console.WriteLine("{2} {0} {1}...", package.Metadata.Id, package.Metadata.Version, "Updating");
                        break;
                    case PackageAction.Configure:
                        if (!ProfileInfo.InstalledPackages.Any(p => p.Metadata.Id == package.Metadata.Id && p.Metadata.Version == package.Metadata.Version))
                            throw new InvalidOperationException(string.Format("Package {0} not installed yet, install first.", item.Item2.Metadata.Id));

                        Console.WriteLine("{2} {0} {1}...", package.Metadata.Id, package.Metadata.Version, "Configuring");
                        break;
                }

                package.Package.RunTarget(this, action.ToString().ToLower());

                switch (action)
                {
                    case PackageAction.Install:
                        // copy package setup over to <profile>/craftitude/packages/<id> for uninstallation/update/purge
                        package.Package.Directory.Copy(_craftitudeDirectory.CreateSubdirectory("packages").CreateSubdirectory(package.Metadata.Id));
                        ProfileInfo.InstalledPackages.Add(package);
                        Console.WriteLine("{0} has been installed successfully.", package.Metadata.Id);
                        break;

                    case PackageAction.Uninstall:
                    case PackageAction.Purge:
                        // delete cached package setup if it's not needed anymore
                        if (!_pendingPackages.Any(p => p.Item2.Metadata.Name == package.Metadata.Name))
                        {
                            package.Package.Directory.Delete(true);
                            ProfileInfo.InstalledPackages.RemoveAll(p => p.Metadata.Id == package.Metadata.Id && p.Metadata.Version == package.Metadata.Version);
                            Console.WriteLine("{0} has been uninstalled successfully.", package.Metadata.Id);
                        }
                        break;
                }
            }
        }

        public void Save()
        {
#if DEBUG
            using (var jsonStream = _craftitudeDirectory.GetFile("profile_debug.json").Open(FileMode.OpenOrCreate))
            {
                using (var jsonWriter = new StreamWriter(jsonStream))
                {
                    var jsonSerializer = new JsonSerializer();
                    jsonSerializer.Serialize(jsonWriter, ProfileInfo);
                }
            }
#endif
            using (var bsonStream = _bsonFile.Open(FileMode.OpenOrCreate))
            {
                using (var bsonWriter = new BsonWriter(bsonStream))
                {
                    var jsonSerializer = new JsonSerializer();
                    jsonSerializer.Serialize(bsonWriter, ProfileInfo);
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
    public class PackagesListFile
    {
        public ulong Version { get; set; }

        public IEnumerable<PackageMetadata> Packages { get; set; }
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
            InstalledPackages = new List<RemotePackage>();
            LastUpdateTime = DateTime.MinValue;
            Repositories = new List<RepositoryConfiguration>();
            Libraries = new List<PathLibraryEntry>();
            NativePaths = new List<PathEntry>();
            ExtraArguments = new Dictionary<string, string>();
            MainClass = "net.minecraft.client.Minecraft";
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

        public List<RepositoryConfiguration> Repositories { get; set; }

        public List<string> TweakClasses { get; set; }

        public List<RemotePackage> InstalledPackages { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public List<PathLibraryEntry> Libraries { get; set; }

        public List<PathEntry> NativePaths { get; set; }

        public Dictionary<string, string> ExtraArguments { get; set; }

        public string MainClass { get; set; }
    }

    public class RepositoryConfiguration
    {
        public Uri Uri { get; set; }

        public IEnumerable<string> Subscriptions { get; set; }
    }

    public class Repository
    {
        static Repository()
        {
            // Dirty, but okay. Nevermind. -_-
            Local = new Repository()
            {
                Uri = new Uri("file:///X:/"),
                Subscription = "*"
            };
        }

        public static Repository Local { get; private set; }

        public Uri Uri { get; set; }

        public string Subscription { get; set; }
    }

    public class RemotePackage
    {
        public static RemotePackage FromLocalPackage(Package package)
        {
            return new RemotePackage()
            {
                Repository = Repository.Local,
                Package = package,
                Metadata = package.Metadata
            };
        }

        public PackageMetadata Metadata { get; set; }

        [JsonIgnore()]
        public Package Package { get; set; }

        public Repository Repository { get; set; }
    }
}
