using PowerArgs;

namespace Automatica.Core.CLI.Args
{
    internal class DeployDockerUpdateArguments
    {
        [ArgDescription("The image name"), ArgPosition(1), ArgRequired]
        public string ImageName { get; set; }

        [ArgDescription("The image tag"), ArgPosition(2), ArgRequired]
        public string ImageTag { get; set; }


        [ArgDescription("The image version"), ArgPosition(3), ArgRequired]
        public string Version { get; set; }

        [ArgDescription("The cloud api key"), ArgPosition(4), ArgRequired]
        public string ApiKey { get; set; }

        [ArgDescription("The cloud url"), ArgPosition(5), ArgRequired]
        public string CloudUrl { get; set; }

        [ArgDescription("Sets the cloud environment for the update"), ArgPosition(6), ArgRequired, ArgDefaultValue("develop")]
        public string CloudEnvironment { get; set; }
    }
}
