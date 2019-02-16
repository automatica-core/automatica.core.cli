using PowerArgs;
using System;

namespace Automatica.Core.CLI.Args
{
    internal class InitArguments : WorkingDirectoryArguments
    {
        [ArgRequired, ArgDescription("The type of plugin, either Driver or Rule")]
        public string Type { get; set; }
        public string ProjectFullName { get; internal set; }

        public Guid PluginGuid { get; internal set; }
    }
}
