using Automatica.Core.CLI.Args;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Automatica.Core.CLI.Actions
{
    public static class InstallPluginAction
    {
        public static void InstallPlugin(InstallPluginArgs args)
        {
            if(!File.Exists(args.PluginFile))
            {
                if(!File.Exists(Path.Combine(Environment.CurrentDirectory, args.PluginFile)))
                {
                    Console.WriteLine($"{args.PluginFile} not found");
                    throw new FileNotFoundException(args.PluginFile);
                }
                else
                {
                    args.PluginFile = Path.Combine(Environment.CurrentDirectory, args.PluginFile);
                }
            }

            var tmpDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            Directory.CreateDirectory(tmpDirectory);

            ZipFile.ExtractToDirectory(args.PluginFile, tmpDirectory, true);


            var manifest = CommonAction.GetAutomaticaManifest(tmpDirectory);
            var componentDirectory = Path.Combine(tmpDirectory, manifest.Automatica.ComponentName);
            if (Directory.Exists(componentDirectory))
            {
                foreach(var file in Directory.GetFiles(componentDirectory))
                {
                    var fileInfo = new FileInfo(file);

                    File.Copy(file, Path.Combine(args.InstallDirectory, fileInfo.Name));
                }
            }
            else
            {
                Console.WriteLine($"Could not find component {manifest.Automatica.ComponentName}");
                return;
            }


            Directory.Delete(tmpDirectory, true);
        }
    }
}
