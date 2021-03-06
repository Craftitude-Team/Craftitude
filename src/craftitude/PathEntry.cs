﻿namespace Craftitude
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
}
