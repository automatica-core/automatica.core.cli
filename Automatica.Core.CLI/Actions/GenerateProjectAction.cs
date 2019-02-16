using Automatica.Core.CLI.Args;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

namespace Automatica.Core.CLI.Actions
{
    internal static class GenerateProjectAction
    {
        public static int GenerateProject(GenerateProjectArguments args)
        {
            var outDir = Path.Combine(args.WorkingDirectory, args.FullName);

            if (args.OverwriteExisting)
            {
                if (Directory.Exists(outDir))
                {
                    Directory.Delete(outDir, true);
                }
            }

            if (Directory.Exists(outDir))
            {
                Console.Error.WriteLine($"{outDir} exists, we will not use an existing folder for a new project");
                return 2;
            }

            var manifestResource = "Automatica.Core.CLI.DriverTemplate.zip";

            if(args.Type.ToLowerInvariant() == "rule")
            {
                manifestResource = "Automatica.Core.CLI.RuleTemplate.zip";
            }

            using (var manifestStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(manifestResource))
            {
                var tmpFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", ""));
                using (var fileStream = new FileStream(tmpFile, FileMode.CreateNew))
                {
                    manifestStream.CopyTo(fileStream);
                }

                ZipFile.ExtractToDirectory(tmpFile, outDir);
            }

            var dirs = Directory.GetDirectories(outDir).ToList();
            dirs.Add(outDir);

            foreach (var dir in dirs)
            {
                if (args.Type.ToLowerInvariant() == "driver")
                {
                    EditDriverFiles(dir, args);
                }
                if (args.Type.ToLowerInvariant() == "rule")
                {
                    EditRuleFiles(dir, args);
                }

                var dirInfo = new DirectoryInfo(dir);
                if (args.Type.ToLowerInvariant() == "driver")
                {
                    if (dirInfo.Name.Contains("DriverTemplate"))
                    {
                        var newDirName = dirInfo.Name.Replace("DriverTemplate", args.FullName);
                        Directory.Move(dir, Path.Combine(dirInfo.Parent.FullName, newDirName));
                    }
                }
                if (args.Type.ToLowerInvariant() == "rule")
                {
                    if (dirInfo.Name.Contains("RuleTemplateName"))
                    {
                        var newDirName = dirInfo.Name.Replace("RuleTemplateName", args.FullName);
                        Directory.Move(dir, Path.Combine(dirInfo.Parent.FullName, newDirName));
                    }
                }
            }

            var projectDir = Directory.GetDirectories(outDir).SingleOrDefault(a => a.EndsWith("Factory"));
            Guid guid;
            if (args.Type.ToLowerInvariant() == "rule")
            {
                guid = EditRuleFiles(Path.Combine(projectDir, "Resources"), args);
            }
            else if (args.Type.ToLowerInvariant() == "driver")
            {
                guid = EditDriverFiles(Path.Combine(projectDir, "Resources"), args);
            }
            else
            {
                Console.Error.WriteLine($"Invalid plugin type {args.Type}");
                return 10;
            }

            InitAction.Init(new InitArguments()
            {
                Type = args.Type,
                WorkingDirectory = Path.Combine(outDir, projectDir),
                ProjectFullName = args.FullName,
                PluginGuid = guid
            });

            return 0;
        }
        private static Guid EditRuleFiles(string directory, GenerateProjectArguments args)
        {
            var files = Directory.GetFiles(directory);
            var guid = Guid.NewGuid();
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);

                if (fileInfo.Name.Contains("RuleTemplateName"))
                {
                    var name = args.ShortName;

                    if (fileInfo.Extension == ".csproj" || fileInfo.Extension == ".sln")
                    {
                        name = args.FullName;
                    }

                    var newFileName = fileInfo.Name.Replace("RuleTemplateName", name);
                    var newFilePath = Path.Combine(fileInfo.DirectoryName, newFileName);
                    File.Move(file, newFilePath);

                    string text = "";
                    using (var reader = new StreamReader(newFilePath))
                    {
                        text = reader.ReadToEnd();

                    }
                    text = text.Replace("RuleTemplateName", name);

                    if (fileInfo.Extension == ".cs" || fileInfo.Extension == ".json")
                    {
                        text = text.Replace("RULE_GUID", guid.ToString());
                        text = text.Replace("RULETEMPLATENAME", args.ShortName.ToUpperInvariant());
                        text = text.Replace("ruletemplatename", args.ShortName.ToLowerInvariant());
                    }

                    using (var streamWriter = new StreamWriter(newFilePath))
                    {
                        streamWriter.Write(text);
                    }

                }
            }
            return guid;
        }
        private static Guid EditDriverFiles(string directory, GenerateProjectArguments args)
        {
            var files = Directory.GetFiles(directory);
            var guid = Guid.NewGuid();
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);

                if (fileInfo.Name.Contains("DriverTemplate"))
                {
                    var name = args.ShortName;

                    if (fileInfo.Extension == ".csproj" || fileInfo.Extension == ".sln")
                    {
                        name = args.FullName;
                    }

                    var newFileName = fileInfo.Name.Replace("DriverTemplate", name);
                    var newFilePath = Path.Combine(fileInfo.DirectoryName, newFileName);
                    File.Move(file, newFilePath);

                    string text = "";
                    using (var reader = new StreamReader(newFilePath))
                    {
                        text = reader.ReadToEnd();

                    }
                    text = text.Replace("DriverTemplate", name);

                    if (fileInfo.Extension == ".cs" || fileInfo.Extension == ".json")
                    {
                        text = text.Replace("DRIVER_GUID", guid.ToString());
                        text = text.Replace("DRIVERTEMPLATE", args.ShortName.ToUpperInvariant());
                        text = text.Replace("drivertemplate", args.ShortName.ToLowerInvariant());
                    }

                    using (var streamWriter = new StreamWriter(newFilePath))
                    {
                        streamWriter.Write(text);
                    }

                }
            }
            return guid;
        }
    }
}
