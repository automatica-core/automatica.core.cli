using PowerArgs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Automatica.Core.CLI.Args
{
    internal class PackUpdateArgs : WorkingDirectoryArguments
    {
        [ArgRequired, ArgDescription("The package version")]
        public string Version { get; set; }

        [ArgDescription("The package rid"), ArgRequired]
        public string Rid { get; set; }

        [ArgDescription("Defines if the update is a pre-release"), ArgDefaultValue(false)]
        public bool PreRelease { get; set; }

        [ArgDescription("The package configuration"), ArgDefaultValue("Release"), ArgRequired]
        public string Configuration { get; set; }

        [ArgDescription("The path where the generated update file should be stored"), ArgRequired]
        public string UpdateOutput { get; set; }


        [ArgDescription("If the sources are already built, use this paramater to define where the binaries are"), ArgDefaultValue(null)]
        public string PreBuiltDirectory { get; set; }
    }
}
