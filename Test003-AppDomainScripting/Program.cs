using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Craftitude;
using Craftitude.Plugins;
using Craftitude.Plugins.LuaSetup;
using NLua;

namespace Test003_AppDomainScripting
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain scriptingDomain = AppDomain.CreateDomain("scripting", AppDomain.CurrentDomain.Evidence);
            var scriptingCallback = new CrossAppDomainDelegate(() =>
            {
                using (Lua lua = new Lua())
                {
                    lua.LoadCLRPackage();
                    try
                    {
                        lua.DoString("import(\"Craftitude\")");
                        Console.WriteLine("Craftitude imported successfully.");
                        lua.DoString("import(\"Craftitude.Plugins\"");
                        Console.WriteLine("Craftitude.Plugins imported successfully.");
                        lua.DoString("import(\"Craftitude.Plugins.LuaSetup\"");
                        Console.WriteLine("Craftitude.Plugins.LuaSetup imported successfully.");
                    }
                    catch
                    {
                        Console.WriteLine("Import errored out.");
                    }
                }
            });

            Console.WriteLine("== only reference ==");
            scriptingDomain.DoCallBack(scriptingCallback);


            Console.WriteLine("== explicit loading (Craftitude assembly) ==");
            scriptingDomain.Load(System.IO.File.ReadAllBytes("craftitude"));
            scriptingDomain.DoCallBack(scriptingCallback);


            Console.WriteLine("== explicit loading (Craftitude plugin assembly) ==");
            scriptingDomain.Load(System.IO.File.ReadAllBytes("plugin_luasetup"));
            scriptingDomain.DoCallBack(scriptingCallback);

            Console.ReadKey();
        }
    }
}
