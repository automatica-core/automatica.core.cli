using Automatica.Core.CLI.Args;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Automatica.Core.CLI.Actions
{
    internal static class DeployPluginAction
    {
        internal static async Task<int> Action(DeployPluginArguments args)
        {
            var uploadFile = args.File;

            if (!File.Exists(uploadFile))
            {
                if (Directory.Exists(uploadFile))
                {
                    var files = Directory.GetFiles(uploadFile, "*.acpkg");

                    if (files.Length > 1)
                    {
                        Console.Error.WriteLine($"multiple *.acpkg files found in {uploadFile}");
                        return -1;
                    }
                    if (files.Length == 0)
                    {
                        Console.Error.WriteLine($"no *.acpkg files found in {uploadFile}");
                        return -1;
                    }
                    uploadFile = files[0];
                }
            }
            args.File = uploadFile;

            // packages are signed when they are packed, sign here if that did not happen (no key at pack time)
            var signRet = PackageSigning.SignIfConfigured(args.File, PackageSigning.TypePlugin);
            if (signRet != 0)
            {
                return signRet;
            }
            return await SendFile(args);
        }

        public static async Task<int> SendFile(DeployPluginArguments args)
        {
            try
            {
               

                MultipartFormDataContent multiContent = new MultipartFormDataContent();

                var client = new HttpClient();

                var payload = await File.ReadAllBytesAsync(args.File);
                multiContent.Add(new ByteArrayContent(payload), "files", args.File); // name must be "files"
                var cloudUrl = $"{args.CloudUrl}/webapi/v1/coreCliData/deployPlugin/{args.DeleteOldVersions}/{args.CloudEnvironment}/{args.ApiKey}";

                // never print the url, it contains the api key
                Console.WriteLine($"Posting {args.File} to {args.CloudUrl}");

                var response = await client.PostAsync(cloudUrl, multiContent).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                Console.WriteLine("Upload successfull");
                return 0;
            }
            catch(Exception e)
            {

                Console.WriteLine($"Upload failed {e}");
                return -2;
            }
        }
    }
}
