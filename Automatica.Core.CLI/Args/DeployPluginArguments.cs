using PowerArgs;

namespace Automatica.Core.CLI.Args
{
    internal class DeployPluginArguments
    {
        [ArgDescription("The package file"), ArgPosition(1), ArgRequired]
        public string File { get; set; }

        [ArgDescription("The cloud api key"), ArgPosition(2), ArgRequired]
        public string ApiKey { get; set; }

        [ArgDescription("The cloud url"), ArgPosition(3), ArgRequired]
        public string CloudUrl { get; set; }

        [ArgDescription("Define if the old versions should be deleted"), ArgPosition(4), ArgRequired, ArgDefaultValue(true)]
        public bool DeleteOldVersions { get; set; }

        [ArgDescription("Sets the cloud environment for the plugin"), ArgPosition(5), ArgRequired, ArgDefaultValue("develop")]
        public string CloudEnvironment { get; set; }
    }
}
