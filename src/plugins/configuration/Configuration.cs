using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Craftitude.Plugins.Configuration
{
    public class Configuration
    {
        public void Set(string file, string key, string value)
        {
            var p = new PropertiesFile(file);
            p.Set(key, value);
            p.Save(file);
        }

        public void Unset(string file, string key)
        {
            var p = new PropertiesFile(file);
            p.Unset(key);
            p.Save(file);
        }
    }

    public class PropertiesFile
    {
        public PropertiesFile()
        {
            Properties = new Dictionary<string, string>();
        }

        public PropertiesFile(string file)
        {
            Properties = new Dictionary<string, string>();
            Load(file);
        }

        public PropertiesFile(Stream stream)
        {
            Properties = new Dictionary<string, string>();
            Load(stream);
        }

        public PropertiesFile(StreamReader streamReader)
        {
            Properties = new Dictionary<string, string>();
            Load(streamReader);
        }

        public PropertiesFile(Dictionary<string, string> items)
        {
            Properties = items;
        }

        public Dictionary<string, string> Properties { get; set; }

        public void Save(string file)
        {
            using (var stream = File.Open(file, FileMode.OpenOrCreate))
            {
                Save(stream);
            }
        }

        public void Save(Stream stream)
        {
            using (var streamWriter = new StreamWriter(stream, Encoding.UTF8))
            {
                Save(streamWriter);
            }
        }

        public void Save(StreamWriter streamWriter)
        {
            foreach (var item in Properties)
            {
                streamWriter.Write(item.Key);
                streamWriter.Write("=");
                streamWriter.WriteLine(item.Value);
            }
        }

        public void Load(string file)
        {
            using (var stream = File.Open(file, FileMode.OpenOrCreate))
            {
                Load(stream);
            }
        }

        public void Load(Stream stream)
        {
            using (var streamReader = new StreamReader(stream, Encoding.UTF8, false))
            {
                Load(streamReader);
            }
        }

        public void Load(StreamReader streamReader)
        {
            // TODO: Improve comment handling
            while (!streamReader.EndOfStream)
            {
                string line = streamReader.ReadLine();
                line = line.Split('#').First();
                if (line.Trim() == string.Empty)
                    continue;

                string[] i = line.Split('=');
                string k = i.First();
                string v = string.Join("=", i.Skip(1));

                Set(k, v);
            }
        }

        public void Set(string k, string v)
        {
            if (Properties.ContainsKey(k))
                Properties[k] = v;
            else
                Properties.Add(k, v);
        }

        public void Unset(string k)
        {
            if (Properties.ContainsKey(k))
                Properties.Remove(k);
        }
    }
}
