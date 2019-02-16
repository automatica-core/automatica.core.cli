using System;

namespace Automatica.Core.CLI.Data
{
    public class UpdateManifest
    {
        public DateTime Timestamp { get; set; }
        public Version Version { get; set; }
        public string Rid { get; set; }
        public bool PreRelease { get; set; }
    }
}
