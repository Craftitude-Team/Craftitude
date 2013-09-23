using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Craftitude
{
    public class PackageVersion : IComparable<PackageVersion>
    {
        static readonly char[] splitChars = new char[] { ':', '.', '-', '_', ' ', '~', '+' };

        uint internalVersionBump = 0;
        string versionString;

        public static implicit operator PackageVersion(string version)
        {
            string[] s = version.Split(':');
            if (s.Count() == 1)
                return new PackageVersion()
                {
                    versionString = version
                };
            else
                return new PackageVersion()
                {
                    internalVersionBump = uint.Parse(s[0]),
                    versionString = string.Join(":", s.Skip(1))
                };
        }

        public static implicit operator string(PackageVersion pv)
        {
            return pv.internalVersionBump.ToString() + ":" + pv.versionString;
        }

        public override string ToString()
        {
            return ToString(true);
        }

        public string ToString(bool withBumpPrefix)
        {
            return (withBumpPrefix && internalVersionBump != 0 ? internalVersionBump.ToString() + ":" : "") + versionString;
        }

        public override bool Equals(object obj)
        {
            if (obj is PackageVersion)
                return CompareTo(obj as PackageVersion) == 0;
            else if (obj is string)
                return CompareTo(obj as string) == 0;
            else
                return false;
        }

        public int CompareTo(PackageVersion other)
        {
            string ver1 = this.ToString(true);
            string ver2 = other.ToString(true);

            List<string> split1 = this.ToString().Split(splitChars).ToList();
            List<string> split2 = other.ToString().Split(splitChars).ToList();

            int maxCount = Math.Max(split1.Count, split2.Count);
            int maxLength = 32; // TODO: Restrict part length in documentation

            // Fill up part counts
            while (split1.Count < maxCount)
                split1.Add(string.Empty);

            while (split2.Count < maxCount)
                split2.Add(string.Empty);

            // Fill up parts
            split1 = split1.Select(i => string.Format("{0}{1}", new string('\0', maxLength - i.Length), i)).ToList();
            split2 = split2.Select(i => string.Format("{0}{1}", new string('\0', maxLength - i.Length), i)).ToList();

            // Comparison
            foreach (var comparisonItem in
                split1.Zip(split2, (s, i) => new { s, i })
                .Select(item => new[] { item.s, item.i })
                .Select(i => i[0].CompareTo(i[1])))
            {
                if (comparisonItem != 0)
                {
                    return comparisonItem;
                }
            }

            // All elements are equal
            return 0;
        }
    }
}
