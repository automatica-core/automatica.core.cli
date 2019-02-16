using Automatica.Core.CLI.Args;
using System;
using System.IO;

namespace Automatica.Core.CLI.Actions
{
    internal static class DeployAction
    {
        internal static int Deploy(DeployArguments args)
        {
            Console.WriteLine($"Working directory {args.WorkingDirectory}");
            var manifest = CommonAction.GetAutomaticaManifest(args.WorkingDirectory);
            
            var deployDir = Path.Combine(args.WorkingDirectory, args.TargetDirectory, manifest.Automatica.ComponentName);

            if (Directory.Exists(deployDir))
            {
                Directory.Delete(deployDir, true);
            }
            Directory.CreateDirectory(deployDir);


            Console.WriteLine($"Check if file exists {Path.Combine(args.WorkingDirectory, args.PublishDirectory, manifest.Automatica.Output)}");
            if (!File.Exists(Path.Combine(args.WorkingDirectory, args.PublishDirectory, manifest.Automatica.Output)))
            {
                Console.WriteLine($"Could not find output file specified in manifest file ({manifest.Automatica.Output} in {Path.Combine(args.WorkingDirectory, args.PublishDirectory)})");
                return -2;
            }
            else
            {
                File.Copy(Path.Combine(args.WorkingDirectory, args.PublishDirectory, manifest.Automatica.Output), Path.Combine(deployDir, manifest.Automatica.Output));
            }

            foreach (var dep in manifest.Automatica.Dependencies)
            {
                if (!File.Exists(Path.Combine(args.WorkingDirectory, args.PublishDirectory, dep)))
                {
                    Console.WriteLine($"Could not find output file specified in manifest file ({dep} in {Path.Combine(deployDir, manifest.Automatica.Output)})");
                    return -2;
                }
                else
                {
                    File.Copy(Path.Combine(args.WorkingDirectory, args.PublishDirectory, dep), Path.Combine(deployDir, dep));
                }
            }
            foreach (var dep in manifest.Automatica.Resources)
            {
                if (!File.Exists(Path.Combine(args.WorkingDirectory, args.PublishDirectory, dep.Replace("Resources/", ""))))
                {
                    Console.WriteLine($"Could not find output file specified in manifest file ({dep.Replace("Resources/", "")} in {Path.Combine(deployDir, manifest.Automatica.Output)})");
                    return -2;
                }
                else
                {
                    File.Copy(Path.Combine(args.WorkingDirectory, args.PublishDirectory, dep.Replace("Resources/", "")), Path.Combine(deployDir, dep.Replace("Resources/", "")));
                }
            }

            return 0;
        }
    }
}
