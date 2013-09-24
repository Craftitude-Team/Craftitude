using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Craftitude.Profile;

namespace Craftitude.Plugins.Forge
{
    public static class Forge
    {
        /// <summary>
        /// Strip illegal chars and reserved words from a candidate filename (should not include the directory path)
        /// </summary>
        /// <remarks>
        /// http://stackoverflow.com/questions/309485/c-sharp-sanitize-file-name
        /// </remarks>
        private static string CoerceValidFileName(string filename)
        {
            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidReStr = string.Format(@"[{0}]+", invalidChars);

            var reservedWords = new[]
                                    {
                                        "CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4",
                                        "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4",
                                        "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
                                    };

            var sanitisedNamePart = Regex.Replace(filename, invalidReStr, "_");
            foreach (var reservedWord in reservedWords)
            {
                var reservedWordPattern = string.Format("^{0}\\.", reservedWord);
                sanitisedNamePart = Regex.Replace(sanitisedNamePart, reservedWordPattern, "_reservedWord_.", RegexOptions.IgnoreCase);
            }

            return sanitisedNamePart;
        }

        public static void InstallMod(CraftitudeProfile profile, PackageMetadata metadata, string jarFile)
        {
            File.Copy(jarFile, GenerateModPath(profile, metadata), true);
        }

        public static void UninstallMod(CraftitudeProfile profile, PackageMetadata metadata)
        {
            File.Delete(GenerateModPath(profile, metadata));
        }

        public static string GenerateModPath(CraftitudeProfile profile, PackageMetadata metadata)
        {
            return Path.Combine(profile.Directory.CreateSubdirectory("mods").FullName + Path.DirectorySeparatorChar, CoerceValidFileName((metadata.Id + "-" + metadata.Version + ".jar").Replace(" ", "")));

        }
    }
}
