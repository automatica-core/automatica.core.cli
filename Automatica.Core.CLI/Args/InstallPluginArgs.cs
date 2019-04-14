using PowerArgs;

namespace Automatica.Core.CLI.Args
{
    public class InstallPluginArgs
    {
        [ArgDescription("The plugin file"), ArgPosition(1), ArgExistingFile]
        public string PluginFile { get; set; }


        [ArgDescription("The install directory"), ArgPosition(2), ArgExistingDirectory]
        public string InstallDirectory { get; set; }
    }
}
