using Automatica.Core.CLI.Args;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Automatica.Core.CLI.Actions
{
    internal static class NugetUpdateAction
    {
        internal static int NugetUpdate(WorkingDirectoryArguments args)
        {
            DirectoryIterator.IterateProjectFiles(args.WorkingDirectory,  (file, document, project) =>
            {
                var nodes = document.SelectNodes("/Project/ItemGroup");

                foreach (var node in nodes)
                {
                    if (node is XmlElement item)
                    {
                        foreach (var x in item.ChildNodes)
                        {
                            if (x is XmlElement element)
                            {
                                if (element.Name == "PackageReference")
                                {
                                    var include = element.Attributes["Include"].Value;
                                    if (include.StartsWith("Automatica"))
                                    {
                                        Console.WriteLine($"Updating project {file}, package {include}");
                                        var dotnet = Process.Start("dotnet", $"add {file} package {include}");
                                        dotnet.OutputDataReceived += (se, e) =>
                                        {
                                            Console.Write(e.Data);
                                        };
                                        dotnet.ErrorDataReceived += (se, e) =>
                                        {
                                            Console.Error.Write(e.Data);
                                        };

                                        dotnet.WaitForExit();

                                        if (dotnet.ExitCode != 0)
                                        {
                                            Environment.Exit(dotnet.ExitCode);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });
            return 0;
        }
    }
}
