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

            return await SendFile(args);
        }

        public static async Task<int> SendFile(DeployUpdateArguments args)
        {
            try
            {
                MultipartFormDataContent multiContent = new MultipartFormDataContent();

                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                var client = new HttpClient(httpClientHandler);

                var payload = await File.ReadAllBytesAsync(args.File);
                multiContent.Add(new ByteArrayContent(payload), "files", args.File); // name must be "files"
                var cloudUrl = $"{args.CloudUrl}/webapi/v1/coreCliData/deploy/{args.ApiKey}";

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
