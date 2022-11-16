using Automatica.Core.CLI.Args;
using Automatica.Core.EF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Automatica.Core.CLI.Actions
{
    internal static class MigrateDatabaseAction
    {
        internal static string DbTypeToString(DatabaseType dbType)
        {
            switch (dbType)
            {
                case DatabaseType.SQLite:
                    return "sqlite";
                case DatabaseType.MariaDb:
                    return "mariadb";
            }
            throw new NotImplementedException();
        }

        internal static int MigrateDatabase(MigrateDatabaseArguments args)
        {
            var sourceDb = DbTypeToString(args.SourceDatabaseType);
            var targetDb = DbTypeToString(args.TargetDatabaseType);

            var sourceCon = args.SourceDatabaseConnectionString;
            var targetCon = args.TargetDatabaseConnectionString;

            var sourceConfig = BuildConfiguration(sourceDb, sourceCon);
            var targetConfig = BuildConfiguration(targetDb, targetCon);


            var sourceContext = new AutomaticaContext(sourceConfig); 
            var targetContext = new AutomaticaContext(targetConfig);
            
            sourceContext.Database.Migrate(); 
            targetContext.Database.Migrate();


            targetContext.BoardTypes.AddRange(sourceContext.BoardTypes.ToList());
            targetContext.BoardInterfaces.AddRange(sourceContext.BoardInterfaces.ToList());
            targetContext.Roles.AddRange(sourceContext.Roles.ToList());

            targetContext.CategoryGroups.AddRange(sourceContext.CategoryGroups.ToList());
            targetContext.CategoryInstances.AddRange(sourceContext.CategoryInstances.ToList());

            targetContext.InterfaceTypes.AddRange(sourceContext.InterfaceTypes.ToList());
            targetContext.Slaves.AddRange(sourceContext.Slaves.ToList());
            targetContext.Settings.AddRange(sourceContext.Settings.ToList());
            targetContext.Plugins.AddRange(sourceContext.Plugins.ToList());



            targetContext.AreaTypes.AddRange(sourceContext.AreaTypes.ToList());
            targetContext.AreaTemplates.AddRange(sourceContext.AreaTemplates.ToList());
            targetContext.AreaInstances.AddRange(sourceContext.AreaInstances.ToList());

            targetContext.RulePageTypes.AddRange(sourceContext.RulePageTypes.ToList());
            targetContext.RuleInterfaceDirections.AddRange(sourceContext.RuleInterfaceDirections.ToList());
            targetContext.RuleInterfaceTemplates.AddRange(sourceContext.RuleInterfaceTemplates.ToList());
            targetContext.RuleTemplates.AddRange(sourceContext.RuleTemplates.ToList());

            targetContext.RulePages.AddRange(sourceContext.RulePages.ToList());
            targetContext.RuleInstances.AddRange(sourceContext.RuleInstances.ToList());
            targetContext.RuleInterfaceInstances.AddRange(sourceContext.RuleInterfaceInstances.ToList());
            
           
            targetContext.NodeDataTypes.AddRange(sourceContext.NodeDataTypes.ToList());
            targetContext.NodeTemplates.AddRange(sourceContext.NodeTemplates.ToList());
            targetContext.NodeInstances.AddRange(sourceContext.NodeInstances.ToList());


            targetContext.Priviledges.AddRange(sourceContext.Priviledges.ToList());
            targetContext.UserGroups.AddRange(sourceContext.UserGroups.ToList());
            targetContext.Users.AddRange(sourceContext.Users.ToList());
            targetContext.User2Roles.AddRange(sourceContext.User2Roles.ToList());
            targetContext.User2Groups.AddRange(sourceContext.User2Groups.ToList());
            targetContext.UserGroup2Roles.AddRange(sourceContext.UserGroup2Roles.ToList());
            targetContext.Priviledge2Roles.AddRange(sourceContext.Priviledge2Roles.ToList());


            targetContext.PropertyTypes.AddRange(sourceContext.PropertyTypes.ToList());
           
            targetContext.PropertyTemplateConstraintData.AddRange(sourceContext.PropertyTemplateConstraintData.ToList());
            targetContext.PropertyTemplateConstraints.AddRange(sourceContext.PropertyTemplateConstraints.ToList());
            targetContext.PropertyTemplates.AddRange(sourceContext.PropertyTemplates.ToList());

            targetContext.NodeDataTypes.AddRange(sourceContext.NodeDataTypes.ToList());


            targetContext.VisuPageTypes.AddRange(sourceContext.VisuPageTypes.ToList());
            targetContext.VisuObjectTemplates.AddRange(sourceContext.VisuObjectTemplates.ToList());
            targetContext.VisuPages.AddRange(sourceContext.VisuPages.ToList());
            targetContext.VisuObjectInstances.AddRange(sourceContext.VisuObjectInstances.ToList());


            targetContext.PropertyInstances.AddRange(sourceContext.PropertyInstances.ToList());
            targetContext.NodeInstance2RulePages.AddRange(sourceContext.NodeInstance2RulePages.ToList());
            targetContext.Links.AddRange(sourceContext.Links.ToList());

            targetContext.SaveChanges(true);

            return 0;
        }

        internal static IConfiguration BuildConfiguration(string dbType, string connectionString)
        {

            var configList = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("DATABASE_TYPE", dbType),
                new KeyValuePair<string, string>("ConnectionStrings:AutomaticaDatabaseMaria", connectionString),
                new KeyValuePair<string, string>("ConnectionStrings:AutomaticaDatabaseSqlite", connectionString),
                new KeyValuePair<string, string>("ConnectionStrings:AutomaticaDatabaseType", dbType)
            };

           
            var builder = new ConfigurationBuilder();
            var config = builder.AddInMemoryCollection(configList).Build();

            return config;
        }
    }
}
