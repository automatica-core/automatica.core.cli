using PowerArgs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Automatica.Core.CLI.Args
{
    internal class GitCommitPushArguments : WorkingDirectoryArguments
    {
        [ArgRequired, ArgDescription("Comment"), ArgPosition(1)]
        public string Comment { get; set; }
    }
}
