using PowerArgs;

namespace Automatica.Core.CLI.Args
{
    internal class MigrateManifestArguments : WorkingDirectoryArguments
    {

        [ArgDescription("Search recursive for automatica-manifest.json files")]
        public bool SearchRecursive { get; set; }
    }
}
