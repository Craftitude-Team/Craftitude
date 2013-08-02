using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test001_AppDomainUnload
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain ad = AppDomain.CreateDomain("customdomain", AppDomain.CurrentDomain.Evidence);
            ad.AssemblyLoad += (s, e) => { Console.WriteLine("ASSEMBLYLOAD {0}", e.LoadedAssembly.Location); };
            ad.DomainUnload += (s, e) => { Console.WriteLine("DOMAINUNLOAD"); };
            ad.FirstChanceException += (s, e) => { Console.WriteLine("1CHEXCEPTION {0}", e.Exception.ToString()); };
            ad.UnhandledException += (s, e) => { Console.WriteLine("UNHEXCEPTION {0}", e.ExceptionObject.ToString()); };
            //System.Reflection.Assembly assembly = System.Reflection.Assembly.Load(b);
            ad.DoCallBack(() =>
            {
                Console.WriteLine("CALLBACK");
                byte[] b = System.IO.File.ReadAllBytes("plugin_luasetup.dll");
                System.Reflection.Assembly.Load(b);
            });
            Console.WriteLine("Assembly loaded now. Press ENTER to unload.");
            Console.ReadLine();
            AppDomain.Unload(ad);
            ad = null;
            GC.Collect(); // collects all unused memory
            GC.WaitForPendingFinalizers(); // wait until GC has finished its work
            GC.Collect();
            System.IO.File.Delete("plugin_luasetup.dll");
            Console.WriteLine("Assembly unloaded now. Press ENTER to close.");
            Console.ReadLine();
        }
    }
}
