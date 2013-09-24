using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Craftitude
{
    static class PackageComparison
    {
        private static bool VersionIsOlder(string inputVersion, string matchVersion)
        {
            return ((PackageVersion) inputVersion).CompareTo(matchVersion) == -1;
        }

        private static bool VersionIsNewer(string inputVersion, string matchVersion)
        {
            return ((PackageVersion) inputVersion).CompareTo(matchVersion) == 1;
        }

        private static bool VersionIsEqual(string inputVersion, string matchVersion)
        {
            return ((PackageVersion) inputVersion).CompareTo(matchVersion) == 0;
        }

        private static bool VersionRegexMatch(string inputVersion, string matchVersionRegex)
        {
            return Regex.IsMatch(inputVersion, matchVersionRegex);
        }
        
        public static IEnumerable<T> GetMatches<T>(
            IEnumerable<T> input,
            Func<T, string> inputIdFunc, Func<T, string> inputVersionFunc,
            string searchId, string searchVersion = "#^.*$")
        {
            // Convert searchVersions to a stack of functions to call
            var predicates = new List<Func<string, string, bool>>();
            var queueChars = new Queue<char>(searchVersion);
            while (queueChars.Any())
            {
                switch (queueChars.Peek())
                {
                    case '<':
                        predicates.Add(VersionIsOlder);
                        queueChars.Dequeue();
                        break;
                    case '>':
                        predicates.Add(VersionIsNewer);
                        queueChars.Dequeue();
                        break;
                    case '=':
                        predicates.Add(VersionIsEqual);
                        queueChars.Dequeue();
                        break;
                    case '#':
                        predicates.Add(VersionRegexMatch);
                        queueChars.Dequeue();
                        break;
                    default:
                        searchVersion = new string(queueChars.ToArray());
                        queueChars.Clear();
                        break;
                }
            }
                
            // Avoid multiple enumerations
            input = input.ToList();

            // Zip items together in a way that we can process them
            var packages = input.Select(item => new { Id = inputIdFunc(item), Version = inputVersionFunc(item), Item = item });

            // Send items through comparison
            return from package in packages.Where(p => p.Id.Equals(searchId)) where predicates.Aggregate(true, (current, dg) => current && dg(package.Version, searchVersion)) select package.Item;
        }

        public static IEnumerable<T> GetMatches<T>(
            IEnumerable<T> input,
            Func<T, string> inputIdFunc, Func<T, string> inputVersionFunc,
            Dependency dependency
            )
        {
            input = input.ToList();
            var l = new List<T>();
            foreach(var version in (dependency.Versions ?? "#^.*$").Split(' '))
                l.AddRange(GetMatches(input, inputIdFunc, inputVersionFunc, dependency.Name, version));
            return l;
        }

        /*
        public static IEnumerable<RemotePackage> MatchPackages(IEnumerable<RemotePackage> packages, Dependency dependency)
        {
            // Avoid multiple enumerations
            packages = packages.ToList();

            Debug.WriteLine("** MatchPackages(<{0} items>, {1})", packages.Count(), dependency.Name + " " + dependency.Versions);

            // Filter out by ID
            packages = packages.Where(p => dependency.Name.Split('|').Contains(p.Metadata.Id)).AsEnumerable();

            Debug.WriteLine("\tFiltered out by ID:");
            foreach (var package in packages)
            {
                Debug.WriteLine("\t\t- package {0} {1}", package.Metadata.Id, package.Metadata.Version);
            }

            // Filter out by version
            var selectedPackages = new List<RemotePackage>();
            var tokens = new[] { '<', '=', '>', '#' };
            if (string.IsNullOrEmpty(dependency.Versions))
                dependency.Versions = "#^.*$";
            if (!dependency.Versions.Trim().Any()) return selectedPackages;
            foreach (string version in dependency.Versions.Split(' '))
            {
                var versionToken = new List<char>();

                var q = new Queue<char>(version.ToCharArray());
                while (tokens.Contains(q.Peek()))
                {
                    versionToken.Add(q.Dequeue());
                }

                var versionString = new string(q.ToArray());

                foreach (var token in versionToken)
                {
                    switch (token)
                    {
                        case '=':
                        {
                            var packagesFound =
                                packages.Where(
                                    p => p.Metadata.Version.CompareTo(versionString) == 0
                                    ).ToList();
                            Debug.WriteLine("\tComparison token {0}, version {2}: Found {1} packages", token,
                                packagesFound.Count(), versionString);
                            selectedPackages.AddRange(packagesFound);
                        }
                            break;
                        case '<':
                        {
                            var packagesFound =
                                packages.Where(
                                    p => p.Metadata.Version.CompareTo(versionString) == -1
                                    ).ToList();
                            Debug.WriteLine("\tComparison token {0}, version {2}: Found {1} packages", token,
                                packagesFound.Count(), versionString);
                            selectedPackages.AddRange(packagesFound);
                        }
                            break;
                        case '>':
                        {
                            var packagesFound =
                                packages.Where(
                                    p => p.Metadata.Version.CompareTo(versionString) == 1
                                    ).ToList();
                            Debug.WriteLine("\tComparison token {0}, version {2}: Found {1} packages", token,
                                packagesFound.Count(), versionString);
                            selectedPackages.AddRange(packagesFound);
                        }
                            break;
                        case '#':
                        {
                            if (versionToken.Count > 1)
                            {
                                Debug.WriteLine("Can't combine regex with other comparison types.");
                                throw new InvalidOperationException(
                                    "Can't combine regex with other comparison types.");
                            }

                            var packagesFound =
                                packages.Where(
                                    p => Regex.IsMatch(p.Metadata.Version, versionString)
                                    ).ToList();
                            Debug.WriteLine("\tComparison token {0}: Found {1} packages", token,
                                packagesFound.Count());
                            selectedPackages.AddRange(packagesFound);
                        }
                            break;
                    }
                }
            }

            return selectedPackages;
        }
        */
    }
}
