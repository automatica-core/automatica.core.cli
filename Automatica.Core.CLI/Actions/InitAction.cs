using Automatica.Core.CLI.Args;
using System;
using System.IO;
using System.Reflection;

namespace Automatica.Core.CLI.Actions
{
    internal static class InitAction
    {
        internal static int Init(InitArguments args)
        {
            if (!(args.Type.ToLowerInvariant() == "driver" || args.Type.ToLowerInvariant() == "rule"))
            {
                Console.WriteLine("Type must be either driver or rule");
                return 1;
            }

            var project = CommonAction.GetProjectFile(args.WorkingDirectory);
            var csprojFile = new FileInfo(project);

            args.PluginGuid = Guid.NewGuid();
            if (string.IsNullOrEmpty(args.ProjectFullName))
            {
                args.ProjectFullName = csprojFile.Name.Replace(".csproj", "");
            }

            var manifestFile = Path.Combine(args.WorkingDirectory, "automatica-manifest.json");

            var buildPipeline = Path.Combine(args.WorkingDirectory, "azure-devops-build.json");

            var manifestRelativeDirectory = $"{args.ProjectFullName}Factory/automatica-manifest.json";

            using (var manifestStream = Assembly.GetCallingAssembly().GetManifestResourceStream("Automatica.Core.CLI.automatica-manifest.json"))
            {
                ReplaceStringInFile(manifestStream, manifestFile, args.WorkingDirectory, csprojFile.Name, args.Type, manifestFile, args.PluginGuid.ToString());
            }
            using (var manifestStream = Assembly.GetCallingAssembly().GetManifestResourceStream("Automatica.Core.CLI.BuildPipeline.json"))
            {
                ReplaceStringInFile(manifestStream, buildPipeline, args.WorkingDirectory, csprojFile.Name, args.Type, manifestRelativeDirectory, args.PluginGuid.ToString());
            }

            return 0;
        }

        private static void ReplaceStringInFile(Stream inFile, string outFile, string workingDirectory, string csProjectFile, string type, string manifestFilePath, string pluginGuid)
        {
            string fileString = "";
            using (StreamReader reader = new StreamReader(inFile))
            {
                fileString = reader.ReadToEnd();
            }

            if (!File.Exists(outFile))
            {
                var resources = "";
                var resDir = Path.Combine(workingDirectory, "Resources");
                if (Directory.Exists(resDir))
                {
                    var files = Directory.GetFiles(resDir, "*.json");

                    for (int i = 0; i < files.Length; i++)
                    {
                        var fileInfo = new FileInfo(files[i]);
                        files[i] = $"\"Resources/{fileInfo.Name}\"";
                    }

                    resources = string.Join(',', files);
                }


                using (var file = new StreamWriter(outFile, false))
                {
                    file.Write(fileString.Replace("{PROJECT_NAME}", csProjectFile.Replace(".csproj", "")).Replace("{TYPE}", type).Replace("\"{RESOURCES}\"", resources).Replace("\"{MANIFEST_PATH}\"", $"\"{manifestFilePath}\"").Replace("\"{PLUGIN_GUID}\"", $"\"{pluginGuid}\""));
                }
                Console.WriteLine("Created manifest file...");
            }
            else
            {
                Console.WriteLine("Manifest file already exists");
            }
        }
    }
}
