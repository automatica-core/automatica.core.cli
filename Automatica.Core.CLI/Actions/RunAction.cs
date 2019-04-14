using Automatica.Core.Driver;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Automatica.Core.CLI.Actions
{
    public static class RunAction
    {
        public static Task Run(string workingDir)
        {

            foreach(var file in Directory.GetFiles(workingDir, "*.dll"))
            {
                Assembly assembly;
                try
                {
                    assembly = Assembly.LoadFrom(file);
                }
                catch (Exception e)
                {
                    continue;
                }
                finally
                {
                    //AssemblyLoadContext.Default.Resolving -= assemblyLoader;
                }

                var resources = assembly.GetManifestResourceNames().SingleOrDefault(a => a.EndsWith("automatica-manifest.json"));

                if (resources == null)
                {
                    continue;
                }

                var manifest = Common.Update.Plugin.GetEmbeddedPluginManifest(NullLogger.Instance, assembly);

                if (manifest.Automatica.Type == "driver")
                {
                 
                    var drivers = assembly.GetExportedTypes().Where(a => a.IsSubclassOf(typeof(DriverFactory)));

                    foreach(var driver in drivers)
                    {
                        if (assembly.CreateInstance(driver.FullName) is DriverFactory factory)
                        {
                            
                        }
                    }
                }
            }

          


            return Task.CompletedTask;
        }
    }
}
