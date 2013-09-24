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
            Debug.WriteLine(string.Format("Fetching package {0}...", name));
            return new Package(Repository.EnumerateDirectories(name).Single());
        }

        static IEnumerable<List<Package>> GetPackagesToInstall(string name)
        {
            var packages = new List<List<Package>> {new List<Package>()};

            packages[0].Add(GetPackage(name));

            bool hasPredependencies;
            do
            {
                Debug.WriteLine("Starting new list.");

                var pendingPackages = new List<Package>();
                var currentlist = packages.Last().ToList();

                foreach (var package in currentlist)
                {
                    Debug.WriteLine(string.Format("Checking out package {0}", package.Metadata.Id));
                    foreach (var dependency in package.Metadata.Dependencies.Where(dependency => dependency.Type == DependencyType.Prerequirement || dependency.Type == DependencyType.Requirement || dependency.Type == DependencyType.Suggestion).Where(dependency => !Profile.IsInstalledPackagesMatch(dependency)))
                    {
                        // TODO: abort on planned-to-install and actually wanted version mismatch
                        if (packages.Sum(ps => ps.Count(pr => pr.Metadata.Id == dependency.Name)) > 0)
                        {
                            Console.WriteLine("{0}: Installing {1} earlier", package.Metadata.Id, dependency.Name);
                            foreach (var pendingPackagesList in packages)
                            {
                                pendingPackagesList.RemoveAll(p => p.Metadata.Id == dependency.Name);
                            }
                            pendingPackages.Add(GetPackage(dependency.Name));
                            continue;
                        }
                        Console.WriteLine("{0}: {1} {2} will be installed beforehand.", package.Metadata.Id, dependency.Type.ToString(), dependency.Name); 
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

            Directory.CreateDirectory("test");

            var packages = GetPackagesToInstall("minecraftforge").ToList();

            // Print out generated installation levels
            Console.WriteLine();
            Console.WriteLine("== Generated installation levels ==");
            var level = 0;
            foreach (var levelPackages in packages)
            {
                Console.WriteLine("- Level {0}", ++level);
                foreach (var package in levelPackages)
                {
                    Console.WriteLine("\t- {0} {1}", package.Metadata.Id, package.Metadata.Version);
                }
            }
            Console.WriteLine("== End of generated installation levels ==");
            Console.WriteLine();

            // Let's print it out somewhat simplish and chaotic
            Console.WriteLine("Following packages will be installed: {0}", string.Join(", ", packages.Select(p => string.Join(", ", p.Select(p2 => p2.Metadata.Id)))));
            Console.WriteLine();

            Console.WriteLine("Press a key to continue...");
            Console.ReadKey();
            Console.WriteLine();

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
