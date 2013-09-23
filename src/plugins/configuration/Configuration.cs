using System;

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

        public void Default(string file, string key, string value)
        {
            var p = new PropertiesFile(file);
            if (p.Get(key) == null)
            {
                p.Set(key, value);
                p.Save(file);
            }
        }

        public void Unset(string file, string key)
        {
            var p = new PropertiesFile(file);
            p.Unset(key);
            p.Save(file);
        }

        public string Get(string file, string key)
        {
            var p = new PropertiesFile(file);
            return p.Get(key);
        }
    }
}
