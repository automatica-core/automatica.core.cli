using PowerArgs;

namespace Automatica.Core.CLI.Args
{
    internal class PackCommandArguments : WorkingDirectoryArguments
    {
        [ArgRequired, ArgDescription("The package version")]
        public string Version { get; set; }

        [ArgDescription("The package configuration"), ArgDefaultValue("Release")]
        public string Configuration { get; set; }

        [ArgDescription("The package output directory, if not set the working directory will be used")]
        public string OutputDirectory { get; set; }

        [ArgDescription("Defines if you want to automatically update all your Automatica.Core nuget references"), ArgDefaultValue(true)]
        public bool UpdateNugetPackages { get; set; }
    }
}
