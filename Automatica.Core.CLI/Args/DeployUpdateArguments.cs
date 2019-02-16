using PowerArgs;

namespace Automatica.Core.CLI.Args
{
    internal class DeployUpdateArguments
    {
        [ArgDescription("The update file"), ArgPosition(1), ArgRequired]
        public string File { get; set; }

        [ArgDescription("The cloud api key"), ArgPosition(2), ArgRequired]
        public string ApiKey { get; set; }

        [ArgDescription("The cloud url"), ArgPosition(3), ArgRequired]
        public string CloudUrl { get; set; }
    }
}
