using Automatica.Core.CLI.Actions;
using Automatica.Core.CLI.Args;
using PowerArgs;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Automatica.Core.CLI
{
    [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
    internal class AutomaticaCliArgs
    {
        [HelpHook, ArgShortcut("--help"), ArgDescription("Shows CLI info")]
        public bool Help { get; set; }

        [ArgActionMethod, ArgDescription("sets version in all *.csproj files found in the current working directory")]
        public void SetVersion(SetVersionArguments versionArg)
        {
            Environment.Exit(SetVersionAction.SetVersion(versionArg));
        }

        [ArgActionMethod, ArgShortcut("nuget-update"), ArgDescription("Updates all automatica.core nuget references found in the current working directory")]
        public void NugetUpdate(WorkingDirectoryArguments args)
        {
            Environment.Exit(NugetUpdateAction.NugetUpdate(args));
        }

        [ArgActionMethod, ArgShortcut("migrate-database"), ArgDescription("Migrates the database content from source to target")]
        public void MigrateDatabase(MigrateDatabaseArguments args)
        {
            Environment.Exit(MigrateDatabaseAction.MigrateDatabase(args));
        }

        [ArgActionMethod, ArgDescription("Commits and pushes all git repositories found in the current working directory")]
        public void GitCommitPush(GitCommitPushArguments args)
        {
            var dirs = Directory.GetDirectories(args.WorkingDirectory);

            foreach (var dir in dirs)
            {
                if (Directory.Exists(Path.Combine(dir, ".git")))
                {
                    Environment.CurrentDirectory = dir;

                    var git = Process.Start("git", $"commit -a -m \"{args.Comment}\"");
                    git.OutputDataReceived += (se, e) =>
                    {
                        Console.Write(e.Data);
                    };
                    git.ErrorDataReceived += (se, e) =>
                    {
                        Console.Error.Write(e.Data);
                    };

                    git.WaitForExit();

                    var gitPush = Process.Start("git", "push");
                    gitPush.OutputDataReceived += (se, e) =>
                    {
                        Console.Write(e.Data);
                    };
                    git.ErrorDataReceived += (se, e) =>
                    {
                        Console.Error.Write(e.Data);
                    };

                    gitPush.WaitForExit();
                }
                else
                {
                    Console.Error.WriteLine($"{dir} is not a valid git repository");
                }
            }
        }

        [ArgActionMethod, ArgDescription("Initializes the project in the current working directory")]
        public void Init(InitArguments args)
        {
            Environment.Exit(InitAction.Init(args));
        }

        [ArgActionMethod, ArgDescription("Publishes the project and all dependencies to a specific directory")]
        public void Publish(PublishArguments args)
        {
            Environment.Exit(PublishAction.Publish(args));
        }

        [ArgActionMethod, ArgDescription("Deploys only the needed files to a specific directory")]
        public void Deploy(DeployArguments args)
        {
            Environment.Exit(DeployAction.Deploy(args));
        }

        [ArgActionMethod, ArgShortcut("generate"), ArgShortcut("create"), ArgDescription("Generates a new Automatica driver or rule project")]
        public void GenerateProject(GenerateProjectArguments args)
        {
            Environment.Exit(GenerateProjectAction.GenerateProject(args));
        }

        [ArgActionMethod, ArgDescription("Packs a Automatica driver or rule project")]
        public void Pack(PackCommandArguments args)
        {
            Environment.Exit(PackAction.Pack(args));
        }

        [ArgActionMethod, ArgDescription("Deploys a Automatica driver or rule project to the cloud")]
        public async Task DeployPlugin(DeployPluginArguments args)
        {
            var exitCode = await DeployPluginAction.Action(args);
            Environment.Exit(exitCode);
        }

        [ArgActionMethod, ArgDescription("Packages a Automatica.Core update")]
        public void PackUpdate(PackUpdateArgs args)
        {
            Environment.Exit(PackUpdateAction.Pack(args));
        }

        [ArgActionMethod, ArgDescription("Deploys a Automatica.Core update to the cloud")]
        public async Task DeployUpdate(DeployUpdateArguments args)
        {
            var exitCode = await DeployUpdateAction.Action(args);
            Environment.Exit(exitCode);
        }

        [ArgActionMethod, ArgDescription("Migrates manifest to the latest version")]
        public void MigrateManifest(MigrateManifestArguments args)
        {
            var exitCode = MigrateManifestAction.MigrationAction(args);
            Environment.Exit(exitCode);
        }

        [ArgActionMethod, ArgDescription("Install latest plugins")]
        public async Task InstallLatestPlugins(InstallLatestPluginsArgument args)
        {
            var exitCode = await InstallLatestPluginsAction.Action(args);
            Environment.Exit(exitCode);
        }

        [ArgActionMethod, ArgDescription("Set min core server verison in plugin manifests")]
        public async Task SetMinCoreServerVersionInManifests(SetMinCoreServerVersionArguments args)
        {
            var exitCode = await SetVersionAction.SetMinCoreServerVersionInManifests(args);
            Environment.Exit(exitCode);
        }

        [ArgActionMethod, ArgDescription("Prints the current RID to the stdout")]
        public void PrintRid()
        {
            Console.WriteLine($"{NetStandardUtils.Platform.Rid}");
        }

        //[ArgActionMethod, ArgDescription("Runs a Automatica driver or rule project")]
        //public async Task Run(WorkingDirectoryArguments args)
        //{
        //    await RunAction.Run(args.WorkingDirectory);
        //}


        [ArgActionMethod, ArgDescription("Installs a plugin in a give directory")]
        public void InstallPlugin(InstallPluginArgs args)
        {
            InstallPluginAction.InstallPlugin(args);
        }
    }

    class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                if (args.Length > 0 && args[0] != "PrintRid")
                {
                    Console.WriteLine($"Automatica.Core CLI Version {NetStandardUtils.Version.GetAssemblyVersion()}");
                }

                var ret = await PowerArgs.Args.InvokeActionAsync<AutomaticaCliArgs>(args);


                if (Debugger.IsAttached)
                {
                    Console.ReadLine();
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"{e}");
            }
        }
    }
}
