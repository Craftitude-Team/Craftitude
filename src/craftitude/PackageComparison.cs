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

        public static List<T> GetMatches<T>(
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
    }
}
