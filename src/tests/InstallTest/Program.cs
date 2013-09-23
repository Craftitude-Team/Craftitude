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
        static DirectoryInfo repository = new DirectoryInfo("repository");
        static Profile profile = new Profile(new DirectoryInfo("test"));

        static Package GetPackage(string name)
        {
            Console.WriteLine("Fetching package {0}...", name);
            return new Package(repository.EnumerateDirectories(name).Single());
        }

        static List<List<Package>> GetPackagesToInstall(string name)
        {
            List<List<Package>> packages = new List<List<Package>>();

            packages.Add(new List<Package>());
            packages[0].Add(GetPackage(name));

            bool hasPredependencies = false;
            do
            {
                Console.WriteLine("Starting new list.");

                List<Package> pendingPackages = new List<Package>();
                var currentlist = packages.Last();

                foreach (var package in currentlist)
                {
                    Console.WriteLine("Checking out package {0}", package.Metadata.Id);
                    foreach (var dependency in package.Metadata.Dependencies)
                    {
                        if (dependency.Type == DependencyType.Prerequirement || dependency.Type == DependencyType.Requirement || dependency.Type == DependencyType.Suggestion)
                        {
                            if (!profile.IsInstalledPackagesMatch(dependency))
                            {
                                // TODO: abort on planned-to-install and actually wanted version mismatch
                                if (packages.Sum(ps => ps.Count(pr => pr.Metadata.Id == dependency.Name)) > 0)
                                {
                                    Console.WriteLine("{0}: {1} is already planned to be installed. Ignoring.", package.Metadata.Id, dependency.Name);
                                    continue;
                                }
                                Console.WriteLine("{0}: {2} not installed but is a {1}, will be installed beforehand.", package.Metadata.Id, dependency.Type.ToString().ToLower(), dependency.Name); 
                                pendingPackages.Add(GetPackage(dependency.Name));
                            }
                        }
                    }
                }

                if (hasPredependencies = pendingPackages.Any())
                    packages.Add(pendingPackages);
            } while (hasPredependencies);

            packages.Reverse();

            return packages;
        }

        static void Main(string[] args)
        {
            //if (Directory.Exists("test"))
            //    Directory.Delete("test", true);

            Console.WriteLine("Creating test target...");
            var targetDirectory = Directory.CreateDirectory("test");

            Console.WriteLine("Preparing installation...");
            List<List<Package>> packages = GetPackagesToInstall("minecraft-client");

            try
            {
                int phase = 0;
                foreach (var packageBatch in packages)
                {
                    Console.WriteLine("Installation phase {0}", ++phase);
                    foreach (var package in packageBatch)
                    {
                        profile.AppendPackage(package, PackageAction.Install);
                    }
                    profile.RunTasks();
                }

                profile.Save();
            }
            catch (InvalidOperationException error)
            {
                Console.WriteLine("Setup error:");
                Console.WriteLine(error.ToString());
            }

            Console.WriteLine("Done.");

            Console.ReadLine();
        }
    }
}
