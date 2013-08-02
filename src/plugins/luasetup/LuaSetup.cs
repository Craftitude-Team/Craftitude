using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.IO;
using System.Text;
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
            using (Lua lua = new Lua())
            {
                lua.LoadCLRPackage();
                lua.RegisterFunction("GetProfile", this, this.GetType().GetMethod("GetProfile"));
                lua.RegisterFunction("GetPackage", this, this.GetType().GetMethod("GetPackage"));
                lua.DoFile(Path.Combine(Package.Path + Path.DirectorySeparatorChar, arguments[0].Replace('/', Path.DirectorySeparatorChar)));
                lua.GetFunction(arguments[1]).Call();
            }
        }

        public Profile GetProfile()
        {
            return Profile;
        }

        public string GetProfilePath()
        {
            return Profile.Path;
        }

        public Package GetPackage()
        {
            return Package;
        }

        public string GetPackagePath()
        {
            return Package.Path;
        }
    }
}
