using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test002_MetadataTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Craftitude.Package p = new Craftitude.Package(new System.IO.DirectoryInfo("package"));
            Console.WriteLine("Package data:");
            Console.WriteLine("\tTitle: {0}", p.Metadata.Name);
            Console.WriteLine("\tDescription: {0}", p.Metadata.Description);
            Console.WriteLine();
            Console.WriteLine("Targets:");
            foreach (var target in p.Metadata.Targets)
            {
                Console.WriteLine("\t{0}", target.Key);
                foreach (var step in target.Value)
                {
                    Console.WriteLine("\t\t{0} with {1} arguments", step.Name, step.Arguments != null ? step.Arguments.Count() : 0);
                }
            }
            Console.WriteLine();
            Console.WriteLine("Press a key to exit.");
            Console.ReadKey();
        }
    }
}
