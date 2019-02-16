using PowerArgs;

namespace Automatica.Core.CLI.Args
{
    internal class PublishArguments : WorkingDirectoryArguments
    {
        [ArgDescription("The runtime identifier"), ArgPosition(1), ArgDefaultValue("")]
        public string Rid { get; set; }

        [ArgDescription("Build configuration"), ArgPosition(2), ArgDefaultValue("Debug")]
        public string Configuration { get; set; }

        [ArgDescription("Publish output directory"), ArgPosition(3), ArgDefaultValue("publish")]
        public string OutputDirectory { get; set; }

        public bool IgnoreManifest { get; set; } = false;
    }
}
