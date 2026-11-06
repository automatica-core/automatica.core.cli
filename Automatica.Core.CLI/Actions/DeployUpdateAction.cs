using Automatica.Core.CLI.Args;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Automatica.Core.CLI.Actions
{
    internal static class DeployUpdateAction
    {
        internal static async Task<int> Action(DeployUpdateArguments args)
        {
            if(!File.Exists(args.File))
            {
                Console.WriteLine($"{args.File} does not exist!");
                return -1;
            }

            var signRet = PackageSigning.SignIfConfigured(args.File, PackageSigning.TypeUpdate);
            if (signRet != 0)
            {
                return signRet;
            }
            return await SendFile(args);
        }

        public static async Task<int> SendFile(DeployUpdateArguments args)
        {
            try
            {
                MultipartFormDataContent multiContent = new MultipartFormDataContent();

                var client = new HttpClient();

                var payload = await File.ReadAllBytesAsync(args.File);
                multiContent.Add(new ByteArrayContent(payload), "files", args.File); // name must be "files"
                var cloudUrl = $"{args.CloudUrl}/webapi/v1/coreCliData/deploy/{args.CloudEnvironment}/{args.ApiKey}";

                // never print the url, it contains the api key
                Console.WriteLine($"Posting {args.File} to {args.CloudUrl}");

                var response = await client.PostAsync(cloudUrl, multiContent).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                Console.WriteLine("Upload successfully");
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
