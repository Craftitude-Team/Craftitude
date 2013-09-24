using System;
using System.Linq;

namespace Craftitude
{
    public class PackageVersion : IComparable<PackageVersion>
    {
        static readonly char[] SplitChars = { ':', '.', '-', '_', ' ', '~', '+' };

        public uint InternalSuperversion { get; set; }
        public string PublicVersion { get; set; }

        public static implicit operator PackageVersion(string version)
        {
            var s = version.Split(':');
            if (s.Count() == 1)
                return new PackageVersion()
                {
                    PublicVersion = version
                };
            else
                return new PackageVersion()
                {
                    InternalSuperversion = uint.Parse(s[0]),
                    PublicVersion = string.Join(":", s.Skip(1))
                };
        }

        public static implicit operator string(PackageVersion pv)
        {
            return pv.InternalSuperversion.ToString() + ":" + pv.PublicVersion;
        }

        public override string ToString()
        {
            return ToString(true);
        }

        public string ToString(bool withBumpPrefix)
        {
            return (withBumpPrefix && InternalSuperversion != 0 ? InternalSuperversion.ToString() + ":" : "") + PublicVersion;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((PackageVersion) obj);
        }

        protected bool Equals(PackageVersion other)
        {
            return InternalSuperversion == other.InternalSuperversion && string.Equals(PublicVersion, other.PublicVersion);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)InternalSuperversion * 397) ^ (PublicVersion != null ? PublicVersion.GetHashCode() : 0);
            }
        }

        public int CompareTo(PackageVersion other)
        {
            var ver1 = ToString(true);
            var ver2 = other.ToString(true);

            var split1 = ToString().Split(SplitChars).ToList();
            var split2 = other.ToString().Split(SplitChars).ToList();

            var maxCount = Math.Max(split1.Count, split2.Count);
            const int maxLength = 32; // TODO: Restrict part length in documentation

            // Fill up part counts
            while (split1.Count < maxCount)
                split1.Add(string.Empty);

            while (split2.Count < maxCount)
                split2.Add(string.Empty);

            // Fill up parts
            split1 = split1.Select(i => string.Format("{0}{1}", new string('\0', maxLength - i.Length), i)).ToList();
            split2 = split2.Select(i => string.Format("{0}{1}", new string('\0', maxLength - i.Length), i)).ToList();

            // Comparison
            return split1
                .Zip(split2, (s, i) => new {s, i})
                .Select(item => new[] {item.s, item.i})
                .Select(i => string.Compare(i[0], i[1], StringComparison.Ordinal))
                .FirstOrDefault(comparisonItem => comparisonItem != 0);

            // All elements are equal
        }
    }
}
