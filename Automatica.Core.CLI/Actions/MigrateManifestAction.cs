using Automatica.Core.CLI.Args;
using System;
using System.IO;

namespace Automatica.Core.CLI.Actions
{
    internal class MigrateManifestAction
    {
        internal static int MigrationAction(MigrateManifestArguments args)
        {
            if (args.SearchRecursive)
            {
                var fileList = new DirectoryInfo(args.WorkingDirectory).GetFiles("automatica-manifest.json", SearchOption.AllDirectories);

                foreach (var file in fileList)
                {
                    Migrate(file.DirectoryName);
                }
            }
            else
            {
                Migrate(args.WorkingDirectory);
            }
            return 0;
        }

        internal static int Migrate(string workingDir)
        {
            Console.WriteLine($"Working directory {workingDir}");
            var manifest = CommonAction.GetAutomaticaManifest(workingDir);

            if (manifest.Automatica.ManifestVersion == new Version(0, 0, 0, 1))
            {
                manifest.Automatica.ManifestVersion = new Version(0, 1, 0, 0);
                manifest.Automatica.PluginVersion = new Version(0, 1, 0, 0);
                manifest.Automatica.PluginGuid = Guid.NewGuid();
            }

            CommonAction.WriteAutomaticaManifest(workingDir, manifest);
            return 0;
        }

    }
}
