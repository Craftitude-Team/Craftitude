using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.IO;
using System.Text;
using Craftitude.Profile;
using NLua;
using NLua.Config;
using NLua.Event;
using NLua.Exceptions;
using NLua.Extensions;
using NLua.Method;

namespace Craftitude.Plugins.LuaSetup
{
    [Export("luasetup", typeof(SetupHelper))]
    public class LuaSetup : SetupHelper
    {
        public override void Run(string[] arguments)
        {
            // Correcting path => base path is package path
            arguments[0] = Path.Combine(Package.Directory.FullName + Path.DirectorySeparatorChar, arguments[0]);
            
            using (Lua lua = new Lua())
            {
                lua.LoadCLRPackage();

                lua.DoString("import(\"craftitude\")"); // Load craftitude assembly into Lua.
                lua.RegisterFunction("GetPlatformString", this, this.GetType().GetMethod("GetPlatformString"));
                lua.RegisterFunction("GetProfile", this, this.GetType().GetMethod("GetProfile"));
                lua.RegisterFunction("GetPackage", this, this.GetType().GetMethod("GetPackage"));
                lua.RegisterFunction("GetProfilePath", this, this.GetType().GetMethod("GetProfilePath"));
                lua.RegisterFunction("GetPackagePath", this, this.GetType().GetMethod("GetPackagePath"));
                lua.DoString(@"
	local mt = { }
	local methods = { }
	function mt.__index(userdata, k)
		if methods[k] then
			return methods[k]
		else
			return rawget(userdata, ""_array"")[k]
		end
	end

	function mt.__newindex(userdata, k, v)
		if methods[k] then
			error ""can't assign to method!""
		else
			rawget(userdata, ""_array"")[k] = v
		end
	end");
                lua.DoString(@"
function import_plugin(assemblyName, namespace)
    import(Path.Combine(GetPluginsDir(), assemblyName))
    import(namespace)
end");
                lua.DoString(@"function install_plugin(dllPath, assemblyName)
    System.IO.File.Copy(dllPath, Path.Combine(GetPluginsDir(), assemblyName .. "".dll""))
end");
                lua.DoString(@"function uninstall_plugin(assemblyName)
    System.IO.File.Delete(Path.Combine(GetPluginsDir(), assemblyName .. "".dll""))
end");
                lua.DoFile(Path.Combine(Package.Path + Path.DirectorySeparatorChar, arguments[0].Replace('/', Path.DirectorySeparatorChar)));
                lua.GetFunction(arguments[1]).Call();
            }
        }

        public string GetPlatformString()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.MacOSX:
                    return "macosx";
                case PlatformID.Unix:
                    return "linux";
                default:
                    return "windows";
            }
        }

        public bool Is64BitOS()
        {
            return Environment.Is64BitOperatingSystem;
        }

        public bool Is64BitProc()
        {
            return Environment.Is64BitOperatingSystem;
        }

        public CraftitudeProfile GetProfile()
        {
            return Profile;
        }

        public string GetCraftitudeDir()
        {
            return GetCraftitudeDirInfo().ToString() + Path.DirectorySeparatorChar;
        }

        public DirectoryInfo GetCraftitudeDirInfo()
        {
            return Profile.Directory.CreateSubdirectory("craftitude");
        }

        public string GetPluginsDir()
        {
            return GetCraftitudeDirInfo().CreateSubdirectory("plugins").ToString() + Path.DirectorySeparatorChar;
        }

        public string GetProfilePath()
        {
            return Profile.Directory.FullName; // + Path.DirectorySeparatorChar;
        }

        public Package GetPackage()
        {
            return Package;
        }

        public string GetPackagePath()
        {
            return Package.Directory.FullName; // + Path.DirectorySeparatorChar;
        }
    }
}
