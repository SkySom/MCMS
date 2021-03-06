using System;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Mcms.Api.Data.EfCore.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Data.MCP.TSRG.Importer.Extensions
{
    public static class AppBuilderExtensions
    {

        public static IApplicationBuilder AddMCPImport(this IApplicationBuilder app)
        {
            Task.Run(async () =>
            {
                try
                {
                    using (var scope = app.ApplicationServices.CreateScope())
                    {
                        var mcpConfiguration =
                            scope.ServiceProvider.GetRequiredService<IConfiguration>().GetSection("MCPImport");
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MCPTSRGImporter>>();
                        var database = scope.ServiceProvider.GetRequiredService<MCMSContext>();
                        //When import is running, all objects should be added to the DB by the handler, else performance goes down the drain.
                        database.ChangeTracker.AutoDetectChangesEnabled = false;
                        database.ChangeTracker.LazyLoadingEnabled = false;

                        var dataImportHandlers = scope.ServiceProvider.GetServices<IDataImportHandler>().ToList();

                        if (!mcpConfiguration.GetValue<bool>("Enabled"))
                        {
                            logger.LogWarning("MCPImport disabled via configuration. Skipping");
                            return;
                        }

                        logger.LogWarning($"Attempting to import data using {dataImportHandlers.Count} handlers.");
                        foreach (var dataImportHandler in dataImportHandlers)
                        {
                            await dataImportHandler.Import(database, mcpConfiguration);
                            await database.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception e)
                {
                    var exceptionLogger = app.ApplicationServices.GetRequiredService<ILogger<MCPTSRGImporter>>();
                    exceptionLogger.LogCritical(e, "Failed to import MCP data.");
                }
            });

            return app;
        }
    }
}
