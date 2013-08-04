using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Craftitude
{
    /// <summary>
    /// Represents a weighted file or folder path optionally depending on other paths.
    /// This means this path will be intelligently sorted into a classpath
    /// or a librarypath variable depending on when other paths are loaded.
    /// </summary>
    public class PathEntry
    {
        public string Path { get; set; }

        public double Weight { get; set; }

        /*

        public List<string> PrependingLibraries { get; set; }

        public List<string> AppendingLibraries { get; set; }

         */

        internal PathEntry()
            : this(string.Empty)
        {
        }

        public PathEntry(string path, double weight = 50)
        {
            Path = path;

            Weight = weight;

            /*
            if (PrependingPaths == null)
                PrependingPaths = new List<string>();

            if (AppendingPaths == null)
                AppendingPaths = new List<string>();
             */
        }

        /*
        public PathEntry(string path, IEnumerable<string> prependingPaths, double weight = 50)
            : this(path, weight)
        {
            PrependingPaths = prependingPaths.ToList();
        }

        public PathEntry(string path, IEnumerable<string> prependingPaths, IEnumerable<string> appendingPaths, double weight = 50)
            : this(path, prependingPaths, weight)
        {
            AppendingPaths = appendingPaths.ToList();
        }
         */
    }

    /// <summary>
    /// Represents a weighted library optionally depending on other libraries or paths.
    /// This means this path will be intelligently sorted into a classpath
    /// or a librarypath variable depending on when other paths are loaded.
    /// </summary>
    public class PathLibraryEntry
    {
        public double Weight { get; set; }

        /*

        public List<string> PrependingLibraries { get; set; }

        public List<string> AppendingLibraries { get; set; }

         */

        public string Path
        {
            get
            {
                return System.IO.Path.Combine(  // java/<groupId>/<artifactId>/<versionId>/<artifactId>-<versionId>.jar
                    "java", // java/
                    GroupId.Replace('.', System.IO.Path.DirectorySeparatorChar), // net/minecraft/client/
                    ArtifactId, // minecraft/
                    VersionId, // 1.6.2/
                    ArtifactId + "-" + VersionId + ".jar" // 
                    );
            }
        }

        internal PathLibraryEntry()
            : this(string.Empty, string.Empty, string.Empty)
        {
        }

        public PathLibraryEntry(string groupId, string artifactId, string versionId, double weight = 50)
        {
            GroupId = groupId;
            ArtifactId = artifactId;
            VersionId = versionId;

            Weight = weight;

            /*
            if (PrependingPaths == null)
                PrependingPaths = new List<string>();

            if (AppendingPaths == null)
                AppendingPaths = new List<string>();
        
             */
        }

        /*
        public PathLibraryEntry(string groupId, string artifactId, string versionId, IEnumerable<string> prependingPaths, double weight = 50)
            : this(groupId, artifactId, versionId, weight)
        {
            PrependingPaths = prependingPaths.ToList();
        }

        public PathLibraryEntry(string groupId, string artifactId, string versionId, IEnumerable<string> prependingPaths, IEnumerable<string> appendingPaths, double weight = 50)
            : this(groupId, artifactId, versionId, prependingPaths, weight)
        {
            AppendingPaths = appendingPaths.ToList();
        }
         */

        public string GroupId { get; set; }
        public string ArtifactId { get; set; }
        public string VersionId { get; set; }

        public string Id { get; set; }
    }
}
