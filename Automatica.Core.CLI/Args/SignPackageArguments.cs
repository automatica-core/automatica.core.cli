using PowerArgs;

namespace Automatica.Core.CLI.Args
{
    internal class SignPackageArguments
    {
        [ArgDescription("The package file (*.acpkg or update *.zip)"), ArgPosition(1), ArgRequired]
        public string File { get; set; }

        [ArgDescription("plugin or update"), ArgPosition(2), ArgRequired]
        public string Type { get; set; }
    }

    internal class GenerateSigningKeyArguments
    {
        [ArgDescription("Where the private key (PEM) is written"), ArgPosition(1), ArgRequired]
        public string PrivateKeyFile { get; set; }
    }
}
