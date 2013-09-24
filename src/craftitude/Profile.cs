using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Craftitude
{
    public class Profile
    {
        readonly DirectoryInfo _craftitudeDirectory;

        readonly List<Tuple<PackageAction, RemotePackage>> _pendingPackages = new List<Tuple<PackageAction, RemotePackage>>();

        readonly FileInfo _bsonFile;

        public Profile(DirectoryInfo profileDirectory)
        {
            Directory = profileDirectory;

            _craftitudeDirectory = Directory.CreateSubdirectory("craftitude");
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
                        ProfileInfo = jsonSerializer.Deserialize<ProfileInfo>(bsonReader) ?? new ProfileInfo();

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

        public DirectoryInfo Directory { get; private set; }

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
            Debug.WriteLine("Installed packages match test: {0} {1}", dependency.Name, dependency.Versions);
            return PackageComparison.GetMatches(ProfileInfo.InstalledPackages, package => package.Metadata.Id,
                package => package.Metadata.Version, dependency);
        }

        public bool IsInstalledPackagesMatch(Dependency dependency)
        {
            return InstalledPackagesMatch(dependency).Any();
        }

        private IEnumerable<RemotePackage> GetDependingPackages(RemotePackage package)
        {
            return (from installedPackage in ProfileInfo.InstalledPackages from dependency in installedPackage.Metadata.Dependencies.Where(d => d.Name.Split('|').Contains(installedPackage.Metadata.Id)) where InstalledPackagesMatch(dependency).Contains(package) select installedPackage).ToList();
        }

        public void RunTasks()
        {
            // Sort by action (uninstall/purge => install/update => configure)
            _pendingPackages.Sort(
                (a, b) => ((byte) a.Item1).CompareTo((byte) b.Item1));
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
                        // TODO: Craftitude needs to be able to differ between installed and actually configured packages.

                        if (ProfileInfo.InstalledPackages.Any(p => p.Metadata.Id == package.Metadata.Id))
                            throw new InvalidOperationException(string.Format("Package {0} already installed.", item.Item2.Metadata.Id));

                        foreach (var dep in package.Metadata.Dependencies)
                        {
                            Debug.WriteLine("Analyzing {0}: {1}", dep.Type.ToString().ToLower(), dep.Name + " " + dep.Versions);
                            switch (dep.Type)
                            {
                                case DependencyType.Prerequirement:
                                    if (!IsInstalledPackagesMatch(dep))
                                    {
                                        throw new InvalidOperationException(string.Format("Package {0} needs dependency {1} (versions {2}) to be pre-installed. You need to install the dependency before installing this package.", item.Item2.Metadata.Id, dep.Name, dep.Versions));
                                    }
                                    break;
                                case DependencyType.Requirement:
                                    if (!IsInstalledPackagesMatch(dep) && !PackageComparison.GetMatches(pending.Select(t => t.Item2), p => p.Metadata.Id, p => p.Metadata.Version, dep).Any())
                                    {
                                        throw new InvalidOperationException(string.Format("Package {0} needs dependency {1} (versions {2}) to be installed with it. Append the dependency before installing this package.", item.Item2.Metadata.Id, dep.Name, dep.Versions));
                                    }
                                    break;
                                case DependencyType.Incompatibility:
                                    if (IsInstalledPackagesMatch(dep) && !PackageComparison.GetMatches(pending.Select(t => t.Item2), p => p.Metadata.Id, p => p.Metadata.Version, dep).Any())
                                    {
                                        throw new InvalidOperationException(string.Format("Package {0} is incompatible with {1} (versions {2}). Remove the incompatible package before installing this package.", item.Item2.Metadata.Id, dep.Name, dep.Versions));
                                    }
                                    break;
                            }
                        }

                        Console.WriteLine("{2} {0} {1}...", package.Metadata.Id, package.Metadata.Version, "Installing");
                        break;
                    case PackageAction.Uninstall:
                        if (!ProfileInfo.InstalledPackages.Any(p => p.Metadata.Id == package.Metadata.Id &&
                                                                    Equals(p.Metadata.Version, package.Metadata.Version)))
                            throw new InvalidOperationException(string.Format("Package {0} not installed.", item.Item2.Metadata.Id));
                        if (GetDependingPackages(package).Any())
                            throw new InvalidOperationException(string.Format("Some of the installed packages depend on the package {0} which is marked to be uninstalled. Uninstall all depending packages first.", item.Item2.Metadata.Id));

                        Console.WriteLine("{2} {0} {1}...", package.Metadata.Id, package.Metadata.Version, "Uninstalling");
                        break;
                    case PackageAction.Purge:
                        if (!ProfileInfo.InstalledPackages.Any(p => p.Metadata.Id == package.Metadata.Id &&
                                                                    Equals(p.Metadata.Version, package.Metadata.Version)))
                            throw new InvalidOperationException(string.Format("Package {0} not installed.", item.Item2.Metadata.Id));
                        if (GetDependingPackages(package).Any())
                            throw new InvalidOperationException(string.Format("Some of the installed packages depend on the package {0} which is marked to be uninstalled. Uninstall all depending packages first.", item.Item2.Metadata.Id));

                        Console.WriteLine("{2} {0} {1}...", package.Metadata.Id, package.Metadata.Version, "Purging");
                        break;
                    case PackageAction.Update:
                        if (ProfileInfo.InstalledPackages.All(p => p.Metadata.Id != package.Metadata.Id))
                            throw new InvalidOperationException(string.Format("Package {0} not installed.", item.Item2.Metadata.Id));
                        if (ProfileInfo.InstalledPackages.Any(p => p.Metadata.Id == package.Metadata.Id &&
                                                                   Equals(p.Metadata.Version, package.Metadata.Version)))
                            throw new InvalidOperationException(string.Format("Package {0}, Version {1} already installed.", item.Item2.Metadata.Id, item.Item2.Metadata.Version));

                        Console.WriteLine("{2} {0} {1}...", package.Metadata.Id, package.Metadata.Version, "Updating");
                        break;
                    case PackageAction.Configure:
                        if (!ProfileInfo.InstalledPackages.Any(p => p.Metadata.Id == package.Metadata.Id &&
                                                                    Equals(p.Metadata.Version, package.Metadata.Version)))
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
                        if (_pendingPackages.All(p => p.Item2.Metadata.Name != package.Metadata.Name))
                        {
                            package.Package.Directory.Delete(true);
                            ProfileInfo.InstalledPackages.RemoveAll(p => p.Metadata.Id == package.Metadata.Id &&
                                                                         Equals(p.Metadata.Version,
                                                                             package.Metadata.Version));
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
}
