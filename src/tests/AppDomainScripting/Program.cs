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
                    try
                    {
                        lua.LoadCLRPackage();
                        lua.DoString("import 'System'");
                        lua.DoString("import 'plugin_luasetup'");
                        lua.DoString("import 'Craftitude.Plugins.LuaSetup'");

                        lua.DoFile("TestFunction.lua");

                        lua.GetFunction("CreateFile").Call("test.lua");
                        Console.WriteLine(System.IO.File.ReadAllText("testfile.txt"));
                        lua.GetFunction("DeleteFile").Call("test.lua");
                    }
                    catch(Exception err)
                    {
                        Console.WriteLine("Import errored out: {0}", err.ToString());
                    }
                }
            });

            Console.WriteLine("== only reference ==");
            scriptingDomain.DoCallBack(scriptingCallback);
            Console.WriteLine();
            Console.ReadKey();
        }
    }
}
