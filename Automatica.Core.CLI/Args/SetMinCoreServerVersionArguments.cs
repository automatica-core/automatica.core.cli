using PowerArgs;
using System;

namespace Automatica.Core.CLI.Args
{
    internal class SetMinCoreServerVersionArguments : MigrateManifestArguments
    {
        [ArgRequired, ArgDescription("The version to set"), ArgPosition(1)]
        public string Version { get; set; }
    }
}
