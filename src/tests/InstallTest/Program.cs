using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Craftitude;

namespace InstallTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Creating test target...");
            
            
            var targetDirectory = Directory.CreateDirectory("test");

            Console.WriteLine("Loading package...");
            var package = new Package(new DirectoryInfo("examplepackage"));

            Console.WriteLine("\tLoaded {0} {1}.", package.Metadata.Name, package.Metadata.Version);

            Console.WriteLine("Installing package...");
            package.RunTarget(new Profile(targetDirectory), "install");

            Console.WriteLine("Configuring package...");
            package.RunTarget(new Profile(targetDirectory), "configure");

            Console.WriteLine("Done.");
        }
    }
}
