using Automatica.Core.CLI.Args;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Automatica.Core.CLI.Actions
{
    internal class ServerDockerVersion 
    {
        public string Version { get; set; }
        public string ImageName { get; set; }
        public string ImageTag { get; set; }

        public string ChangeLog { get; set; }

        public bool IsPreRelease { get; set; }

        public bool IsPublic { get; set; }
        public string Branch { get; set; }
    }

    internal static class DeployDockerUpdateAction
    {
        internal static async Task<int> Action(DeployDockerUpdateArguments args)
        {
            return await SendFile(args);
        }

        public static async Task<int> SendFile(DeployDockerUpdateArguments args)
        {
            try
            {
                var httpClientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };

                var client = new HttpClient(httpClientHandler);

                var dockerDto = new ServerDockerVersion
                {
                    Branch = args.CloudEnvironment,
                    ImageTag = args.ImageTag,
                    ImageName = args.ImageName,
                    IsPreRelease = true,
                    IsPublic = false,
                    Version = args.Version,
                    ChangeLog = String.Empty
                };
                var cloudUrl = $"{args.CloudUrl}/webapi/v1/coreCliData/deployDocker/{args.CloudEnvironment}/{args.ApiKey}";

                Console.WriteLine($"Posting to {cloudUrl}");

                var response = await client.PostAsync(cloudUrl, new StringContent(JsonConvert.SerializeObject(dockerDto), Encoding.UTF8, "application/json")).ConfigureAwait(false);

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
