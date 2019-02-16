using PowerArgs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Automatica.Core.CLI.Args
{
    internal class InstallLatestPluginsArgument
    {

        [ArgDescription("Install directory"), ArgPosition(1), ArgRequired]
        public string InstallDirectory { get; set; }

        [ArgDescription("The min required core server version"), ArgPosition(2), ArgRequired]
        public string MinCoreServerVersion { get; set; }

        [ArgDescription("The cloud api key"), ArgPosition(3), ArgRequired]
        public string ApiKey { get; set; }

        [ArgDescription("The cloud url"), ArgPosition(4), ArgRequired]
        public string CloudUrl { get; set; }
    }
}
