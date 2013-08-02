using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;
using NLua.Config;
using NLua.Event;
using NLua.Exceptions;
using NLua.Extensions;
using NLua.Method;

namespace Craftitude.Plugins.LuaSetup
{
    public class LuaSetup : InstallationStep
    {
        public override void Run(string[] arguments)
        {
            using (Lua lua = new Lua())
            {
                lua.LoadCLRPackage();
                lua.RegisterFunction("GetProfile", this, this.GetType().GetMethod("GetProfile"));
                lua.RegisterFunction("GetProfilePath", this, this.GetType().GetMethod("GetProfilePath"));
                lua.RegisterFunction("GetPackage", this, this.GetType().GetMethod("GetPackage"));
                lua.RegisterFunction("GetPackagePath", this, this.GetType().GetMethod("GetPackagePath"));
            }
        }

        private Profile GetProfile()
        {
            return Profile;
        }

        private string GetProfilePath()
        {
            return Profile.Path;
        }

        private Package GetPackage()
        {
            return Package;
        }

        private string GetPackagePath()
        {
            return Package.Path;
        }
    }
}
