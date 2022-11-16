using PowerArgs;

namespace Automatica.Core.CLI.Args
{
    internal enum DatabaseType
    {
        SQLite,
        MariaDb
    }

    internal class MigrateDatabaseArguments
    {
        [ArgDescription("Source database type"), ArgPosition(1), ArgRequired]
        public DatabaseType SourceDatabaseType { get; set; }

        [ArgDescription("Source database connection string"), ArgRequired, ArgPosition(2)]
        public string SourceDatabaseConnectionString { get; set; }

        [ArgDescription("Target database type"), ArgRequired, ArgPosition(3)]
        public DatabaseType TargetDatabaseType { get; set; }


        [ArgDescription("Target database connection stringe"), ArgRequired, ArgPosition(4)]
        public string TargetDatabaseConnectionString { get; set; }
    }
}
