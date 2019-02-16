using PowerArgs;

namespace Automatica.Core.CLI.Args
{
    internal class GenerateProjectArguments : WorkingDirectoryArguments
    {
        [ArgRequired, ArgDescription("The type of the new project. Either driver or rule"), ArgPosition(1), ArgDefaultValue("driver")]
        public string Type { get; set; }

        [ArgRequired, ArgDescription("The name of the new project"), ArgPosition(1)]
        public string ShortName { get; set; }

        [ArgRequired, ArgDescription("The name of the new project"), ArgPosition(1)]
        public string FullName { get; set; }

        [ArgDescription("Option to overwrite an existing destination folder"), ArgPosition(1), ArgDefaultValue(false)]
        public bool OverwriteExisting { get; set; }
    }
}
