using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mcms.Api.Data.Poco.Models.Core;
using Mcms.Api.Data.Poco.Models.Core.Release;
using Mcms.Api.Data.Poco.Models.Mapping;
using Mcms.Api.Data.Poco.Models.Mapping.Component;
using Data.MCP.TSRG.Importer.Extensions;
using Flurl.Http;
using Mcms.Api.Data.EfCore.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Data.MCP.TSRG.Importer.TSRG
{
    public class TSRGImportHandler
        : IDataImportHandler
    {
        private readonly ILogger<TSRGImportHandler> _logger;

        public TSRGImportHandler(ILogger<TSRGImportHandler> logger)
        {
            _logger = logger;
        }

        public async Task Import(MCMSContext context, IConfiguration configuration)
        {
            _logger.LogInformation(
                $"Attempting import of TSRG data into context with database name: {context.Database.GetDbConnection().Database}");

            var tsrgMappingType = await GetOrCreateTSRGMappingType(context);

            var mcpConfigProject =
                MavenProject.Create(Constants.FORGE_MAVEN_URL, Constants.MCP_GROUP, Constants.MCP_CONFIG_NAME);

            var mcpConfigArtifacts = await mcpConfigProject.GetArtifacts();

            _logger.LogInformation($"{mcpConfigArtifacts.Count} MCP Config versions are available.");

            var mcVersions = await DetermineMCVersionsFromMCPConfigReleases(mcpConfigArtifacts);

            _logger.LogInformation($"{mcVersions.Count} Minecraft versions are available.");

            var gameVersions = await GetOrCreateGameVersionsForMCVersions(context, mcVersions);
            var releases =
                await DetermineMCPConfigReleasesToImport(context, mcpConfigArtifacts, gameVersions, tsrgMappingType);

            if (!releases.Any())
            {
                _logger.LogWarning("No new MCP Config data to import.");
                return;
            }

            _logger.LogWarning("Importing: " + releases.Count + " new MCP config releases");
            _logger.LogInformation("Importing the following MCP Config releases:");
            foreach (var releaseName in releases.Keys)
            {
                _logger.LogInformation($"  > {releaseName}");
            }

            _logger.LogWarning("Saving initial data.");
            await SaveInitialData(context, releases.Values, gameVersions.Values, tsrgMappingType);

            var newClassData = new List<Component>();

            foreach (var releasesKey in releases.Keys)
            {
                await ProcessMCPConfigArtifact(
                    mcpConfigArtifacts[releasesKey],
                    releases,
                    tsrgMappingType,
                    context,
                    newClassData
                );
            }

            await SaveData(context, newClassData);
        }

        private static async Task<MappingType> GetOrCreateTSRGMappingType(MCMSContext context) =>
            await context.MappingTypes.FirstOrDefaultAsync(m => m.Name == Constants.OBF_TO_TSRG_MAPPING_NAME) ??
            await CreateTSRGMappingType();

        private static Task<MappingType> CreateTSRGMappingType() =>
            Task.FromResult(new MappingType()
            {
                Id = Guid.NewGuid(),
                CreatedBy = Guid.Empty,
                CreatedOn = DateTime.Now,
                Name = Constants.OBF_TO_TSRG_MAPPING_NAME,
                Releases = new List<Release>()
            });

        private static Task<List<string>> DetermineMCVersionsFromMCPConfigReleases(
            Dictionary<string, MavenArtifact> mcpConfigArtifacts) =>
            Task.FromResult(mcpConfigArtifacts.Keys
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Split("-")[0])
                .Distinct()
                .ToList());

        private static async Task<Dictionary<string, GameVersion>> GetOrCreateGameVersionsForMCVersions(
            MCMSContext context,
            IEnumerable<string> mcVersions)
        {
            var newMcVersions = mcVersions.Except(await context.GameVersions.Select(g => g.Name).ToListAsync());
            var existingGameVersions = await context.GameVersions.ToListAsync();
            var newGameVersions = newMcVersions.Select(mcVersion => new GameVersion()
            {
                CreatedBy = Guid.Empty,
                CreatedOn = DateTime.Now,
                Id = Guid.NewGuid(),
                IsPreRelease = false,
                IsSnapshot = false,
                Name = mcVersion
            });

            return existingGameVersions.Union(newGameVersions).ToDictionary(gv => gv.Name);
        }

        private static async Task<Dictionary<string, Release>> DetermineMCPConfigReleasesToImport(
            MCMSContext context,
            IReadOnlyDictionary<string, MavenArtifact> mcpConfigArtifacts,
            IReadOnlyDictionary<string, GameVersion> gameVersions,
            MappingType tsrgMappingType) =>
            mcpConfigArtifacts.Keys
                .Except(await context.Releases.Select(r => r.Name).ToListAsync())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => new Release()
                {
                    Components = new List<ReleaseComponent>(),
                    CreatedBy = Guid.Empty,
                    CreatedOn = DateTime.Now,
                    GameVersion = gameVersions[v.Split("-")[0]],
                    Id = Guid.NewGuid(),
                    MappingType = tsrgMappingType,
                    Name = v
                })
                .ToDictionary(r => r.Name);

        private async Task SaveInitialData(
            MCMSContext context,
            IEnumerable<Release> releasesToSave,
            IEnumerable<GameVersion> gameVersionsToSave,
            MappingType tsrgMappingTypeToSave)
        {
            foreach (var release in releasesToSave)
            {
                var releaseEntityEntry = context.Entry(release);
                if (releaseEntityEntry.State == EntityState.Detached)
                    releaseEntityEntry.State = EntityState.Added;
            }

            foreach (var gameVersion in gameVersionsToSave)
            {
                var gameVersionEntityEntry = context.Entry(gameVersion);
                if (gameVersionEntityEntry.State == EntityState.Detached)
                    gameVersionEntityEntry.State = EntityState.Added;
            }

            var mappingTypeEntityEntry = context.Entry(tsrgMappingTypeToSave);
            if (mappingTypeEntityEntry.State == EntityState.Detached)
                mappingTypeEntityEntry.State = EntityState.Added;

            await context.SaveChangesAsync();
        }

        private async Task ProcessMCPConfigArtifact(
            MavenArtifact mcpConfigArtifact,
            IReadOnlyDictionary<string, Release> releases,
            MappingType tsrgMappingType,
            MCMSContext context,
            List<Component> newClassData)
        {
            _logger.LogInformation($"Processing: {mcpConfigArtifact.Version}");
            var release = releases[mcpConfigArtifact.Version];
            var gameVersion = release.GameVersion;

            ZipArchive zip;
            try
            {
                zip = new ZipArchive(await mcpConfigArtifact.Path.GetStreamAsync());
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Failed to download the MCP Config artifact data.");
                return;
            }

            var tsrgJoinedFileContents = zip.ReadAllLines(Constants.TSRG_JOINED_DATA, Encoding.UTF8).ToList();
            var staticMethodsFileContents = zip.ReadAllLines(Constants.TSRG_STATIC_METHOD_DATA, Encoding.UTF8).ToDictionary(e => e, e => true);

            _logger.LogInformation($"Found: {tsrgJoinedFileContents.Count} entries in the tsrg information.");

            var analysisHelper =
                new TSRGAnalysisHelper(context, ref newClassData, release, gameVersion, tsrgMappingType);

            await tsrgJoinedFileContents.ForEachWithProgressCallback(async (tsrgLine) =>
                {
                    _logger.LogDebug($"Processing tsrg line: {tsrgLine}");
                    if (!tsrgLine.StartsWith('\t'))
                    {
                        analysisHelper.FinalizeCurrentClass();

                        //New class
                        var tsrgClassData = tsrgLine.Split(' ');
                        var inputMapping = tsrgClassData[0].Trim();
                        var outputMappingIncludingPackage = tsrgClassData[1].Trim();

                        var outputMapping =
                            outputMappingIncludingPackage.Substring(outputMappingIncludingPackage.LastIndexOf('/'));
                        var package = outputMappingIncludingPackage.Replace(outputMapping, "").Replace("/", ".");
                        outputMapping = outputMapping.Substring(1);

                        _logger.LogDebug(
                            $"Processing entry as class, with mapping: {inputMapping} ->{outputMapping} in package: {package}");

                        await analysisHelper.StartNewClass(inputMapping, outputMapping, package);
                    }
                    else if (tsrgLine.Contains('('))
                    {
                        //New method
                        var tsrgMethodData = tsrgLine.Trim().Split(' ');
                        var inputMapping = tsrgMethodData[0].Trim();
                        var descriptor = tsrgMethodData[1].Trim();
                        var outputMapping = tsrgMethodData[2].Trim();

                        _logger.LogDebug(
                            $"Processing entry as method, with mapping: {inputMapping} -> {outputMapping} and descriptor: {descriptor}");

                        analysisHelper.AddMethod(inputMapping, outputMapping, descriptor,
                            staticMethodsFileContents.GetValueOrDefault(outputMapping, false));
                    }
                    else
                    {
                        var tsrgFieldData = tsrgLine.Split(' ');
                        var inputMapping = tsrgFieldData[0].Trim();
                        var outputMapping = tsrgFieldData[1].Trim();

                        _logger.LogDebug(
                            $"Processing entry as field, with mapping: {inputMapping} -> {outputMapping}");

                        analysisHelper.AddField(inputMapping, outputMapping, false);
                    }
                },
                (count, current, percentage) =>
                {
                    _logger.LogInformation(
                        $"  > {percentage}% ({current}/{count}): Processing the {mcpConfigArtifact.Version} tsrg file ...");
                });
        }

        private async Task SaveData(MCMSContext context, List<Component> classes)
        {
            _logger.LogWarning(
                $"Saving: {classes.Count} new classes");

            await context.SaveChangesAsync();

            _logger.LogWarning("TSRG Import saved.");
        }
    }
}
