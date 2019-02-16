using System;
using System.IO;
using System.IO.Compression;
using Automatica.Core.CLI.Args;
using Automatica.Core.CLI.Data;
using Newtonsoft.Json;

namespace Automatica.Core.CLI.Actions
{
    internal static class PackUpdateAction
    {
        internal static int Pack(PackUpdateArgs args)
        {
            var publishPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", ""));

            try
            {
                if (!String.IsNullOrEmpty(args.PreBuiltDirectory))
                {
                    publishPath = args.PreBuiltDirectory;
                }
                else
                {
                    var pubArgs = new PublishArguments();
                    pubArgs.Configuration = args.Configuration;
                    pubArgs.OutputDirectory = publishPath;
                    pubArgs.WorkingDirectory = args.WorkingDirectory;
                    pubArgs.Rid = args.Rid;
                    pubArgs.IgnoreManifest = true;
                    PublishAction.Publish(pubArgs);
                }
                var updateManifest = new UpdateManifest
                {
                    Timestamp = DateTime.Now,
                    PreRelease = args.PreRelease,
                    Rid = args.Rid,
                    Version = new Version(args.Version)
                };

                using (var writer = new StreamWriter(Path.Combine(publishPath, "automatica-update.manifest"), false))
                {
                    writer.Write(JsonConvert.SerializeObject(updateManifest));
                }

                var outFile = Path.Combine(args.UpdateOutput, $"automatica-update-{args.Rid}-{args.Version}.zip");

                if(File.Exists(outFile))
                {
                    File.Delete(outFile);
                }

                ZipFile.CreateFromDirectory(publishPath, outFile);

                return 0;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return -1;
            }
            finally
            {
                if (Directory.Exists(publishPath))
                {
                    Directory.Delete(publishPath, true);
                }
            }

        }
    }
}
