using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Craftitude
{
    public class Profile
    {
        DirectoryInfo _directory;

        public Profile(DirectoryInfo profileDirectory)
        {
            _directory = profileDirectory;
        }

        public ProfileInfo ProfileInfo { get; private set; }

        public DirectoryInfo Directory { get { return _directory; } }

        public string Path { get { return _directory.ToString(); } }
    }

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
            LastUpdateTime = DateTime.MinValue;
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

        public List<string> TweakClasses { get; set; }

        public DateTime LastUpdateTime { get; set; }
    }
}
