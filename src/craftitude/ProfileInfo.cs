using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Craftitude.Profile;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Converters;

namespace Craftitude
{
    [Serializable]
    public class ProfileInfo
    {
        private static readonly JsonSerializer JsonSerializer = new JsonSerializer();

        static ProfileInfo()
        {
            JsonSerializer.Converters.Add(new IsoDateTimeConverter());
        }

        public ProfileInfo()
        {
            ProfileName = string.Empty;
            MinecraftVersion = string.Empty;
            MinecraftVersionType = "release";
            TweakClasses = new List<string>();
            InstalledPackages = new List<InstalledPackageInfo>();
            LastUpdateTime = DateTime.MinValue;
            Libraries = new List<PathLibraryEntry>();
            NativePaths = new List<PathEntry>();
            ExtraArguments = new Dictionary<string, string>();
            MainClasses = new List<string> {"net.minecraft.client.Minecraft"};
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
            var reader = new BsonReader(stream);
            return JsonSerializer.Deserialize<ProfileInfo>(reader);
        }

        public void ToStream(Stream stream)
        {
            var writer = new BsonWriter(stream);
            JsonSerializer.Serialize(writer, this);
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

        public List<string> TweakClasses { get; set; }

        internal List<InstalledPackageInfo> InstalledPackages { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public List<PathLibraryEntry> Libraries { get; set; }

        public List<PathEntry> NativePaths { get; set; }

        public Dictionary<string, string> ExtraArguments { get; set; }

        public List<string> MainClasses { get; set; }

        [JsonIgnore]
        public string MainClass
        {
            get { return MainClasses.Any() ? MainClasses.Last() : string.Empty; }
        }

        public void ChangeMainClass(string mainClassToAdd)
        {
            Console.Write("Adding main class {0}... ", mainClassToAdd);
            if (!MainClasses.Contains(mainClassToAdd))
            {
                MainClasses.Add(mainClassToAdd);
            }
            Console.WriteLine("New main class is {0}.", MainClass);
        }

        public void RestoreMainClass(string mainClassToRemove)
        {
            Console.Write("Removing main class {0}... ", mainClassToRemove);
            if (MainClasses.Contains(mainClassToRemove))
            {
                MainClasses.Remove(mainClassToRemove);
            }
            Console.WriteLine("New main class is {0}.", MainClass);
        }

        public void AddTweakClass(string tweakClass)
        {
            Console.Write("Adding tweak class {0}... ", tweakClass);
            if (!TweakClasses.Contains(tweakClass))
            {
                TweakClasses.Add(tweakClass);
            }
            Console.WriteLine("{0} tweak classes found.", TweakClasses.Count);
        }

        public void RemoveTweakClass(string tweakClass)
        {
            Console.Write("Removing tweak class {0}... ", tweakClass);
            if (TweakClasses.Contains(tweakClass))
            {
                TweakClasses.Add(tweakClass);
            }
            Console.WriteLine("{0} tweak classes found.", TweakClasses.Count);
        }
    }
}