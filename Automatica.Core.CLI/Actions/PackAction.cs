using Automatica.Core.CLI.Args;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;

namespace Automatica.Core.CLI.Actions
{
    internal static class PackAction
    {
        internal static int Pack(PackCommandArguments args)
        {
            var manifest = CommonAction.GetAutomaticaManifest(args.WorkingDirectory);

            if(string.IsNullOrEmpty(args.OutputDirectory))
            {
                args.OutputDirectory = args.WorkingDirectory;
            }


            var publishPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", ""));
            var deployPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", ""));
            manifest.Automatica.PluginVersion = new Version(args.Version);
            var manifestSerialized = JsonConvert.SerializeObject(manifest);

            using (StreamWriter man = new StreamWriter(Path.Combine(args.WorkingDirectory, "automatica-manifest.json")))
            {
                man.Write(manifestSerialized);
            }


            var setVersionArgs = new SetVersionArguments();
            setVersionArgs.WorkingDirectory = args.WorkingDirectory;
            setVersionArgs.Version = args.Version;

            SetVersionAction.SetVersion(setVersionArgs);

            if (args.UpdateNugetPackages)
            {
                var nugetUpdateRet = NugetUpdateAction.NugetUpdate(args);

                if (nugetUpdateRet != 0)
                {
                    return nugetUpdateRet;
                }
            }

            var pubArgs = new PublishArguments();
            pubArgs.Configuration = args.Configuration;
            pubArgs.OutputDirectory = publishPath;
            pubArgs.WorkingDirectory = args.WorkingDirectory;

            var pubArgsRet = PublishAction.Publish(pubArgs);
            if (pubArgsRet != 0)
            {
                return pubArgsRet;
            }

            var deployArgs = new DeployArguments();
            deployArgs.WorkingDirectory = args.WorkingDirectory;
            deployArgs.PublishDirectory = publishPath;
            deployArgs.TargetDirectory = deployPath;

            var deployArgsRet = DeployAction.Deploy(deployArgs);

            if(deployArgsRet != 0)
            {
                return deployArgsRet;
            }

            
            using (StreamWriter man = new StreamWriter(Path.Combine(deployPath, "automatica-manifest.json")))
            {
                man.Write(manifestSerialized);
            }

            var zipFileName = Path.Combine(args.OutputDirectory, $"{manifest.Automatica.Name}-{args.Version}.acpkg");
            if(File.Exists(zipFileName))
            {
                File.Delete(zipFileName);
            }
            ZipFile.CreateFromDirectory(deployPath, zipFileName);


            Directory.Delete(publishPath, true);
            Directory.Delete(deployPath, true);
            return 0;
        }
    }
}
