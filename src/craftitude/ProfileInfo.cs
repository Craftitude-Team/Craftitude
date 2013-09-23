using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Converters;

namespace Craftitude
{
    [Serializable]
    public class ProfileInfo
    {
        static JsonSerializer _jsonSerializer = new JsonSerializer();

        static ProfileInfo()
        {
            _jsonSerializer.Converters.Add(new IsoDateTimeConverter());
        }

        public ProfileInfo()
        {
            ProfileName = string.Empty;
            MinecraftVersion = string.Empty;
            MinecraftVersionType = "release";
            TweakClasses = new List<string>();
            InstalledPackages = new List<RemotePackage>();
            LastUpdateTime = DateTime.MinValue;
            Repositories = new List<RepositoryConfiguration>();
            Libraries = new List<PathLibraryEntry>();
            NativePaths = new List<PathEntry>();
            ExtraArguments = new Dictionary<string, string>();
            MainClass = "net.minecraft.client.Minecraft";
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
            BsonReader reader = new BsonReader(stream);
            return _jsonSerializer.Deserialize<ProfileInfo>(reader);
        }

        public void ToStream(Stream stream)
        {
            BsonWriter writer = new BsonWriter(stream);
            _jsonSerializer.Serialize(writer, this);
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

        public List<RepositoryConfiguration> Repositories { get; set; }

        public List<string> TweakClasses { get; set; }

        public List<RemotePackage> InstalledPackages { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public List<PathLibraryEntry> Libraries { get; set; }

        public List<PathEntry> NativePaths { get; set; }

        public Dictionary<string, string> ExtraArguments { get; set; }

        public string MainClass { get; set; }
    }
}