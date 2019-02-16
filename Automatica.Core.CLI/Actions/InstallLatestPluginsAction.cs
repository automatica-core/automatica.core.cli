using Automatica.Core.CLI.Args;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Automatica.Core.CLI.Actions
{
    internal static class InstallLatestPluginsAction
    {
        internal async static Task<int> Action(InstallLatestPluginsArgument args)
        {
            if(!Directory.Exists(args.InstallDirectory))
            {
                Directory.CreateDirectory(args.InstallDirectory);
            }

            if (!Directory.Exists(Path.Combine(args.InstallDirectory, "Drivers")))
            {
                Directory.CreateDirectory(Path.Combine(args.InstallDirectory, "Drivers"));
            }

            if (!Directory.Exists(Path.Combine(args.InstallDirectory, "Rules")))
            {
                Directory.CreateDirectory(Path.Combine(args.InstallDirectory, "Rules"));
            }

            var plugins = await GetPluginList(args.ApiKey, args.CloudUrl, args.MinCoreServerVersion);

            foreach(var p in plugins)
            {
                var installDir = Path.Combine(args.InstallDirectory, p.PluginType == EF.Models.PluginType.Driver ? "Drivers" : "Rules");

                Console.WriteLine($"Download {p.Name} and install to {installDir}");
                await DownloadAndInstallPlugin(p.AzureUrl, installDir);
            }

            return 0;
        }

        private static async Task<IList<EF.Models.Plugin>> GetPluginList(string apiKey, string cloudUrl, string minCoreServerVersion)
        {
            return await GetRequest<IList<EF.Models.Plugin>>($"/webapi/v1/coreCliData/plugins/{minCoreServerVersion}", apiKey, cloudUrl);
        }

        private static HttpClient SetupClient()
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
            // Three versions in one.
            HttpClient client = new HttpClient(httpClientHandler);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }


        private static async Task<T> GetRequest<T>(string apiUrl, string apiKey, string cloudUrl) where T : class
        {
            T result = null;
            try
            {
                using (var client = SetupClient())
                {
                    var uri = new Uri(new Uri(cloudUrl), apiUrl + "/" + apiKey);
                    Console.WriteLine($"Calling {uri}");
                    var response = await client.GetAsync(uri).ConfigureAwait(false);

                    response.EnsureSuccessStatusCode();

                    await response.Content.ReadAsStringAsync().ContinueWith(x =>
                    {
                        if (x.IsFaulted)
                            throw x.Exception;

                        Console.WriteLine($"Received {x.Result} from {apiUrl}");
                        result = JsonConvert.DeserializeObject<T>(x.Result);
                    });
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine( $"{e}: Could not execute get request to cloud");
                throw;
            }

            return result;
        }

        private static async Task DownloadAndInstallPlugin(string downloadUrl, string installDirectory)
        {
            var webClient = new WebClient();
            var tmpFile = Path.GetTempFileName();
            try
            {
                await webClient.DownloadFileTaskAsync(downloadUrl, tmpFile);


                ZipFile.ExtractToDirectory(tmpFile, installDirectory, true);
            }
            catch(Exception e)
            {
                Console.Error.WriteLine($"Could not download file {e}");
            }
            finally
            {
                File.Delete(tmpFile);
            }
            File.Delete(Path.Combine(installDirectory, "automatica-manifest.json"));
        }
    }
}
