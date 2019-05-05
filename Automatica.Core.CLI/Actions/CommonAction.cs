using Automatica.Core.CLI.Exceptions;
using Automatica.Core.Common.Update;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Automatica.Core.CLI.Actions
{
    internal static class CommonAction
    {
        internal static string GetProjectFile(string currentDir)
        {
            var csprojFiles = GetProjectFileInternal(currentDir);

            if (csprojFiles == null)
            {
                Console.WriteLine("No or multiple (*.csproj|*.sln) files found");
                throw new ProjectNotFoundException();
            }
            return csprojFiles;
        }

        internal static string GetProjectFileInternal(string currentDir)
        {
            var csprojFiles = Directory.GetFiles(currentDir, "*.csproj");

            if (csprojFiles.Length == 1)
            {
                return csprojFiles[0];
            }
            var slnFiles = Directory.GetFiles(currentDir, "*.sln");

            if (slnFiles.Length == 1)
            {
                return slnFiles[0];
            }
            return null;

        }

        internal static PluginManifest GetAutomaticaManifest(string currentDir)
        {
            if (File.Exists(currentDir))
            {
                var fileInfo = new FileInfo(currentDir));

                currentDir = fileInfo.DirectoryName;
            }

            var manifestContent = "";
            var manifestFile = Path.Combine(currentDir, "automatica-manifest.json");
            using (StreamReader reader = new StreamReader(manifestFile))
            {
                manifestContent = reader.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<PluginManifest>(manifestContent);
        }
        internal static void WriteAutomaticaManifest(string currentDir, PluginManifest manifest)
        {
            var manifestFile = Path.Combine(currentDir, "automatica-manifest.json");
            using (var writer = new StreamWriter(manifestFile, false))
            {
                writer.Write(JsonConvert.SerializeObject(manifest));
            }
        }
    }
}