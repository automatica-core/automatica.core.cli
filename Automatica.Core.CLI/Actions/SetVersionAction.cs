using Automatica.Core.CLI.Args;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace Automatica.Core.CLI.Actions
{
    internal static class SetVersionAction
    {
        internal static Task<int> SetMinCoreServerVersionInManifests(SetMinCoreServerVersionArguments args)
        {
            return Task.FromResult(UpdateMinVersionAction(args));
        }
        internal static int UpdateMinVersionAction(SetMinCoreServerVersionArguments args)
        {
            if (args.SearchRecursive)
            {
                var fileList = new DirectoryInfo(args.WorkingDirectory).GetFiles("automatica-manifest.json", SearchOption.AllDirectories);

                foreach (var file in fileList)
                {
                    UpdateMinVersion(file.DirectoryName, args.Version);
                }
            }
            else
            {
                UpdateMinVersion(args.WorkingDirectory, args.Version);
            }
            return 0;
        }

        internal static int UpdateMinVersion(string workingDir, string version)
        {
            Console.WriteLine($"Working directory {workingDir}");
            var manifest = CommonAction.GetAutomaticaManifest(workingDir);

            var versionObj = new Version(version);

            manifest.Automatica.MinCoreServerVersion = versionObj;

            CommonAction.WriteAutomaticaManifest(workingDir, manifest);
            return 0;
        }

        internal static int SetVersion(SetVersionArguments args)
        {
            Console.WriteLine($"Set assembly version to {args.Version}");
            DirectoryIterator.IterateProjectFiles(args.WorkingDirectory, (file, document, project) =>
            {
                var version = args.Version.ToString();
                var props = project.FirstChild;

                var versionProp = props.SelectNodes("Version");
                var packageVerson = props.SelectNodes("PackageVersion");

                if (packageVerson.Count == 1)
                {
                    packageVerson[0].InnerText = version;
                }

                if (versionProp.Count == 1)
                {
                    versionProp[0].InnerText = version;
                }
                else
                {
                    var node = document.CreateNode(XmlNodeType.Element, "Version", null);
                    node.InnerText = version;

                    props.AppendChild(node);
                }
                using(var w = new StreamWriter(file))
                    document.Save(w); 
            });
            return 0;
        }
    }
}
