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
            return await SendFile(args);
        }

        public static async Task<int> SendFile(DeployPluginArguments args)
        {
            try
            {
               

                MultipartFormDataContent multiContent = new MultipartFormDataContent();

                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                var client = new HttpClient(httpClientHandler);

                var payload = await File.ReadAllBytesAsync(args.File);
                multiContent.Add(new ByteArrayContent(payload), "files", args.File); // name must be "files"
                var cloudUrl = $"{args.CloudUrl}/webapi/v1/coreCliData/deployPlugin/{args.DeleteOldVersions}/{args.ApiKey}/{args.CloudEnvironment}";

                Console.WriteLine($"Posting to {cloudUrl}");

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
