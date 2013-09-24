using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using Craftitude;

namespace InstallTest
{
    class Program
    {
        static readonly DirectoryInfo Repository = new DirectoryInfo("repository");
        static readonly Profile Profile = new Profile(new DirectoryInfo("test"));

        static Package GetPackage(string name)
        {
            Console.WriteLine("Fetching package {0}...", name);
            return new Package(Repository.EnumerateDirectories(name).Single());
        }

        static IEnumerable<List<Package>> GetPackagesToInstall(string name)
        {
            var packages = new List<List<Package>> {new List<Package>()};

            packages[0].Add(GetPackage(name));

            bool hasPredependencies;
            do
            {
                Console.WriteLine("Starting new list.");

                var pendingPackages = new List<Package>();
                var currentlist = packages.Last();

                foreach (var package in currentlist)
                {
                    Console.WriteLine("Checking out package {0}", package.Metadata.Id);
                    foreach (var dependency in package.Metadata.Dependencies.Where(dependency => dependency.Type == DependencyType.Prerequirement || dependency.Type == DependencyType.Requirement || dependency.Type == DependencyType.Suggestion).Where(dependency => !Profile.IsInstalledPackagesMatch(dependency)))
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

                hasPredependencies = pendingPackages.Any();
                if (hasPredependencies)
                    packages.Add(pendingPackages);
            } while (hasPredependencies);

            packages.Reverse();

            return packages;
        }

        static void Main()
        {
            //if (Directory.Exists("test"))
            //    Directory.Delete("test", true);

            Console.WriteLine("Creating test target...");
            Directory.CreateDirectory("test");

            Console.WriteLine("Preparing installation...");
            var packages = GetPackagesToInstall("minecraftforge");

            try
            {
                var phase = 0;
                foreach (var packageBatch in packages)
                {
                    Debug.WriteLine("Installation phase {0}", ++phase);
                    
                    // Install
                    foreach (var package in packageBatch)
                    {
                        Profile.AppendPackage(package, PackageAction.Install);
                    }

                    // Configure
                    foreach (var package in packageBatch.Where(package => package.Metadata.Targets.ContainsKey("configure")))
                    {
                        Profile.AppendPackage(package, PackageAction.Configure);
                    }

                    Profile.RunTasks();
                }

                Profile.Save();
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
