using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Craftitude.Extensions.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Craftitude.Profile
{
    public class CraftitudeProfile
    {
        private readonly FileInfo _bsonFile;

        private readonly DirectoryInfo _craftitudeDirectory;

        private readonly List<Tuple<PackageAction, Package>> _pendingPackages =
            new List<Tuple<PackageAction, Package>>();

        /// <summary>
        /// The profile information.
        /// </summary>
        public ProfileInfo ProfileInfo { get; private set; }

        /// <summary>
        /// The directory of the profile.
        /// </summary>
        public DirectoryInfo Directory { get; private set; }

        // Constructor

        /// <summary>
        /// Open a profile (create if not exist) and load it.
        /// </summary>
        /// <param name="profileDirectory">The root directory of the profile</param>
        public CraftitudeProfile(DirectoryInfo profileDirectory)
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
                using (FileStream bsonStream = _bsonFile.Open(FileMode.OpenOrCreate))
                {
                    using (var bsonReader = new BsonReader(bsonStream))
                    {
                        var jsonSerializer = new JsonSerializer();
                        ProfileInfo = jsonSerializer.Deserialize<ProfileInfo>(bsonReader) ?? new ProfileInfo();
                    }
                }
            }
        }

        // Package queue methods

        /// <summary>
        /// Add a package to the package queue.
        /// </summary>
        /// <param name="package">The package to add to the queue.</param>
        /// <param name="action">How to handle the package (e.g. installing or uninstalling?)</param>
        /// <param name="forceReappend">Force reappending package if it's already appended to be handled in the same way.</param>
        public void QueuePackage(Package package, PackageAction action = PackageAction.Install, bool forceReappend = false)
        {
            if (_pendingPackages.Any(p => p.Item2 == package && p.Item1 == action))
                if(forceReappend)
                    DequeuePackage(package);
                else
                    throw new InvalidOperationException("Package already appended with same action type. Dequeue the package before reappending or set forceReappend parameter to true.");

            _pendingPackages.Add(new Tuple<PackageAction, Package>(action, package));
        }

        /// <summary>
        /// Remove a package from the package queue.
        /// </summary>
        /// <param name="package">The package to remove from the queue.</param>
        public void DequeuePackage(Package package)
        {
            _pendingPackages.RemoveAll(p => p.Item2 == package);
        }

        /// <summary>
        /// Process the package queue.
        /// </summary>
        public void RunPackageQueue()
        {
            // Sort by action (uninstall/purge => install/update => configure)
            _pendingPackages.Sort(
                (a, b) => ((byte)a.Item1).CompareTo((byte)b.Item1));
            var pending = new Queue<Tuple<PackageAction, Package>>(_pendingPackages);
            _pendingPackages.Clear();

            while (pending.Any())
            {
                var item = pending.Dequeue();
                var action = item.Item1;
                var package = item.Item2;

                switch (action)
                {
                    case PackageAction.Install:
                        if (ProfileInfo.InstalledPackages.Any(p => p.Id == package.Metadata.Id && p.State.HasFlag(InstalledPackageState.Installed)))
                            throw new InvalidOperationException(string.Format("Package {0} already installed.",
                                item.Item2.Metadata.Id));

                        foreach (var dep in package.Metadata.Dependencies)
                        {
                            Debug.WriteLine("Analyzing {0}: {1}", dep.Type.ToString().ToLower(),
                                dep.Name + " " + dep.Versions);
                            switch (dep.Type)
                            {
                                case DependencyType.Prerequirement:
                                    if (!HasMatchingInstalledPackages(dep))
                                    {
                                        throw new InvalidOperationException(
                                            string.Format(
                                                "Package {0} needs dependency {1} (versions {2}) to be pre-installed. You need to install the dependency before installing this package.",
                                                item.Item2.Metadata.Id, dep.Name, dep.Versions));
                                    }
                                    break;
                                case DependencyType.Requirement:
                                    if (!HasMatchingInstalledPackages(dep) &&
                                        !PackageComparison.GetMatches(pending.Select(t => t.Item2), p => p.Metadata.Id,
                                            p => p.Metadata.Version, dep).Any())
                                    {
                                        throw new InvalidOperationException(
                                            string.Format(
                                                "Package {0} needs dependency {1} (versions {2}) to be installed with it. Append the dependency before installing this package.",
                                                item.Item2.Metadata.Id, dep.Name, dep.Versions));
                                    }
                                    break;
                                case DependencyType.Incompatibility:
                                    if (HasMatchingInstalledPackages(dep) &&
                                        !PackageComparison.GetMatches(pending.Select(t => t.Item2), p => p.Metadata.Id,
                                            p => p.Metadata.Version, dep).Any())
                                    {
                                        throw new InvalidOperationException(
                                            string.Format(
                                                "Package {0} is incompatible with {1} (versions {2}). Remove the incompatible package before installing this package.",
                                                item.Item2.Metadata.Id, dep.Name, dep.Versions));
                                    }
                                    break;
                            }
                        }

                        Console.WriteLine("{2} {0} {1}...", package.Metadata.Id, package.Metadata.Version, "Installing");
                        break;
                    case PackageAction.Uninstall:
                        if (!ProfileInfo.InstalledPackages.Any(p => p.Id == package.Metadata.Id &&
                                                                    Equals(p.Version, package.Metadata.Version) && p.State == (InstalledPackageState.Configured | InstalledPackageState.Installed)))
                            throw new InvalidOperationException(string.Format("Package {0} not installed and configured.",
                                item.Item2.Metadata.Id));
                        if (GetDependingInstalledPackages(package).Any())
                            throw new InvalidOperationException(
                                string.Format(
                                    "Some of the installed packages depend on the package {0} which is marked to be uninstalled. Uninstall all depending packages first.",
                                    item.Item2.Metadata.Id));

                        Console.WriteLine("{2} {0} {1}...", package.Metadata.Id, package.Metadata.Version,
                            "Uninstalling");
                        break;
                    case PackageAction.Purge:
                        if (!ProfileInfo.InstalledPackages.Any(p => p.Id == package.Metadata.Id &&
                                                                    Equals(p.Version, package.Metadata.Version) && p.State == (InstalledPackageState.Configured | InstalledPackageState.Installed)))
                            throw new InvalidOperationException(string.Format("Package {0} not installed and configured.",
                                item.Item2.Metadata.Id));
                        if (GetDependingInstalledPackages(package).Any())
                            throw new InvalidOperationException(
                                string.Format(
                                    "Some of the installed packages depend on the package {0} which is marked to be uninstalled. Uninstall all depending packages first.",
                                    item.Item2.Metadata.Id));

                        Console.WriteLine("{2} {0} {1}...", package.Metadata.Id, package.Metadata.Version, "Purging");
                        break;
                    case PackageAction.Update:
                        if (ProfileInfo.InstalledPackages.All(p => p.Id != package.Metadata.Id && p.State == (InstalledPackageState.Configured | InstalledPackageState.Installed)))
                            throw new InvalidOperationException(string.Format("Package {0} not installed and configured.",
                                item.Item2.Metadata.Id));
                        if (ProfileInfo.InstalledPackages.Any(p => p.Id == package.Metadata.Id &&
                                                                   Equals(p.Version, package.Metadata.Version)))
                            throw new InvalidOperationException(
                                string.Format("Package {0}, Version {1} already installed.", item.Item2.Metadata.Id,
                                    item.Item2.Metadata.Version));

                        Console.WriteLine("{2} {0} {1}...", package.Metadata.Id, package.Metadata.Version, "Updating");
                        break;
                    case PackageAction.Configure:
                        if (!ProfileInfo.InstalledPackages.Any(p => p.Id == package.Metadata.Id && p.State == InstalledPackageState.Installed &&
                                                                    Equals(p.Version, package.Metadata.Version)))
                            throw new InvalidOperationException(
                                string.Format("Package {0} not installed yet, install first.", item.Item2.Metadata.Id));

                        Console.WriteLine("{2} {0} {1}...", package.Metadata.Id, package.Metadata.Version, "Configuring");
                        break;
                }

                package.RunTarget(this, action.ToString().ToLower());

                switch (action)
                {
                    case PackageAction.Install:
                        // copy package setup over to <profile>/craftitude/packages/<id> for uninstallation/update/purge
                        package.Directory.Copy(
                            _craftitudeDirectory.CreateSubdirectory("packages").CreateSubdirectory(package.Metadata.Id));
                        ProfileInfo.InstalledPackages.Add(new InstalledPackageInfo
                        {
                            Id = package.Metadata.Id,
                            Version = package.Metadata.Version,
                            State = InstalledPackageState.Installed
                        });
                        break;
                    case PackageAction.Configure:
                        ProfileInfo.InstalledPackages.Single(
                            ip => ip.Id == package.Metadata.Id && Equals(ip.Version, package.Metadata.Version)).State |=
                            InstalledPackageState.Configured;
                        break;
                    case PackageAction.Uninstall:
                    case PackageAction.Purge:
                        // delete cached package setup if it's not needed anymore
                        if (_pendingPackages.All(p => p.Item2.Metadata.Name != package.Metadata.Name))
                        {
                            package.Directory.Delete(true);
                            ProfileInfo.InstalledPackages.RemoveAll(p => p.Id == package.Metadata.Id &&
                                                                         Equals(p.Version,
                                                                             package.Metadata.Version));
                        }
                        break;
                }
            }
        }

        // Installed packages

        /// <summary>
        ///     Get the actual package from the installation cache.
        /// </summary>
        /// <param name="installedPackageInfo">Installed package information</param>
        /// <returns></returns>
        public Package GetInstalledPackage(InstalledPackageInfo installedPackageInfo)
        {
            return
                new Package(
                    _craftitudeDirectory.CreateSubdirectory("packages").CreateSubdirectory(installedPackageInfo.Id));
        }

        /// <summary>
        /// Gets all packages which fit to the dependency.
        /// </summary>
        /// <param name="dependency">The dependency to match all installed packages with.</param>
        /// <returns>All packages which fit to the dependency.</returns>
        public List<InstalledPackageInfo> GetMatchingInstalledPackages(Dependency dependency)
        {
            Debug.WriteLine("Installed packages match test: {0} {1}", dependency.Name, dependency.Versions);
            return PackageComparison.GetMatches(ProfileInfo.InstalledPackages, package => package.Id,
                package => package.Version, dependency);
        }

        /// <summary>
        /// Check if a dependency matches with one or more installed packages.
        /// </summary>
        /// <param name="dependency">The dependency to match all installed packages with.</param>
        /// <returns>Returns true when a matching package has been found, otherwise false.</returns>
        public bool HasMatchingInstalledPackages(Dependency dependency)
        {
            return GetMatchingInstalledPackages(dependency).Any();
        }

        /// <summary>
        /// Get all packages which depend on the given package.
        /// </summary>
        /// <param name="package">The package for which to check as a dependency</param>
        /// <param name="type">Dependency types to search for. Defaults to all (including incompatibilities, might not be intended).</param>
        /// <returns>All packages which found to have a dependency to the given package.</returns>
        private IEnumerable<InstalledPackageInfo> GetDependingInstalledPackages(Package package, DependencyType type = DependencyType.Inclusion | DependencyType.Incompatibility | DependencyType.Prerequirement | DependencyType.Requirement | DependencyType.Suggestion)
        {
            // We expect to have only installed one version of a package at the same time, so we don't check the versions here.
            return ProfileInfo.InstalledPackages.Where(
                installedPackage => GetInstalledPackage(installedPackage).Metadata.Dependencies.Any(d => d.Name.Equals(package.Metadata.Id) && type.HasFlag(d.Type))
                );
        }

        // Profile saving (loading is done in the constructor)

        /// <summary>
        /// Save the profile.
        /// </summary>
        public void Save()
        {
#if DEBUG
            // Create a clear-text profile JSON
            using (
                var jsonStream = _craftitudeDirectory.GetFile("profile_debug.json").Open(FileMode.OpenOrCreate))
            {
                using (var jsonWriter = new StreamWriter(jsonStream))
                {
                    var jsonSerializer = new JsonSerializer();
                    jsonSerializer.Serialize(jsonWriter, ProfileInfo);
                }
            }
#endif

            // Create a binary profile JSON
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