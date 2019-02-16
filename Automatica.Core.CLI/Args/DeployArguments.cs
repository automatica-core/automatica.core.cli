using PowerArgs;

namespace Automatica.Core.CLI.Args
{
    internal class DeployArguments : WorkingDirectoryArguments
    {
        [ArgDescription("The target directiry"), ArgPosition(1), ArgDefaultValue("deploy")]
        public string TargetDirectory { get; set; }

        [ArgDescription("The publish directory"), ArgPosition(2), ArgDefaultValue("publish"), ArgExistingDirectory]
        public string PublishDirectory { get; set; }
        
    }
}
