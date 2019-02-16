using Automatica.Core.CLI.Args;
using System;
using System.Diagnostics;
using System.IO;

namespace Automatica.Core.CLI.Actions
{
    internal static class PublishAction
    {
        internal static int Publish(PublishArguments args)
        {
            var outDir = Path.Combine(args.WorkingDirectory, args.OutputDirectory);
            if (Directory.Exists(outDir))
            {
                Directory.Delete(outDir, true);
                Directory.CreateDirectory(outDir);
            }
            else
            {
                Directory.CreateDirectory(outDir);
            }

            var project = CommonAction.GetProjectFile(args.WorkingDirectory);
            if (!args.IgnoreManifest)
            {
                var manifest = CommonAction.GetAutomaticaManifest(args.WorkingDirectory);
                foreach (var dep in manifest.Automatica.Resources)
                {
                    if (!File.Exists(Path.Combine(args.WorkingDirectory, dep)))
                    {
                        Console.WriteLine($"Could not find resource file specified in manifest file ({dep} in {args.WorkingDirectory})");
                        return -2;
                    }
                    else
                    {
                        FileInfo resource = new FileInfo(Path.Combine(args.WorkingDirectory, dep));
                        File.Copy(Path.Combine(args.WorkingDirectory, dep), Path.Combine(outDir, resource.Name));
                    }
                }
            }
            var environment = args.Rid;
            Process dotnet;
            if (string.IsNullOrEmpty(args.Rid))
            {
                Console.WriteLine($"running: dotnet publish -c {args.Configuration} -o {Path.Combine(args.WorkingDirectory, args.OutputDirectory)} {project}");
                dotnet = Process.Start("dotnet", $"publish -c {args.Configuration} -o {Path.Combine(args.WorkingDirectory, args.OutputDirectory)} {project}");
            }
            else
            {
                Console.WriteLine($"running: dotnet publish -r {args.Rid} -c {args.Configuration} -o {Path.Combine(args.WorkingDirectory, args.OutputDirectory)} {project}");
                dotnet = Process.Start("dotnet", $"publish -r {args.Rid} -c {args.Configuration} -o {Path.Combine(args.WorkingDirectory, args.OutputDirectory)} {project}");
            }

            dotnet.ErrorDataReceived += (se, e) =>
            {
                Console.Error.Write(e.Data);
            };
            dotnet.OutputDataReceived += (se, e) =>
            {
                Console.Write(e.Data);
            };

            dotnet.WaitForExit();
            return dotnet.ExitCode;
        }
    }
}
