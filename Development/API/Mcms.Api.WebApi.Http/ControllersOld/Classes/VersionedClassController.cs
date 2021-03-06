using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mcms.Api.Data.Poco.Models.Core.Release;
using Mcms.Api.Data.Poco.Models.Mapping;
using Mcms.Api.Data.Poco.Models.Mapping.Component;
using Mcms.Api.Data.Poco.Models.Mapping.Mappings;
using Mcms.Api.Data.Poco.Models.Mapping.Metadata;
using Mcms.Api.Data.Poco.Readers.Core;
using Mcms.Api.Data.Poco.Readers.Mapping;
using Mcms.Api.Data.Poco.Writers.Core;
using Mcms.Api.Data.Poco.Writers.Mapping;
using Data.WebApi.Controllers.Base;
using Data.WebApi.Model.Creation.Class;
using Data.WebApi.Model.Read.Class;
using Data.WebApi.Services.Converters;
using Data.WebApi.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Data.WebApi.Controllers.Classes
{
    /// <summary>
    /// Controller that handles interactions on versioned class levels.
    /// </summary>
    [Route("/classes/versioned")]
    [ApiController]
    public class VersionedClassController
        : VersionedComponentControllerBase<ClassVersionedReadModel>
    {
        private readonly IGameVersionReader _gameVersionReader;

        private readonly IUserResolvingService _userResolvingService;

        private readonly IMappingTypeReader _mappingTypeReader;

        public VersionedClassController(IClassComponentWriter componentWriter, IGameVersionReader gameVersionReader, IUserResolvingService userResolvingService, IMappingTypeReader mappingTypeReader) : base(componentWriter)
        {
            _gameVersionReader = gameVersionReader;
            _userResolvingService = userResolvingService;
            _mappingTypeReader = mappingTypeReader;
        }

        /// <summary>
        /// Creates a new versioned class entry for an already existing class mapping.
        /// Creates a versioned mapping class for the given version, as well a single committed mapping.
        /// </summary>
        /// <param name="mapping">The versioned mapping to create.</param>
        /// <returns>An http response code:201-New mapping created,400-The request was invalid,404-Certain data could not be found,401-Unauthorized,409-A versioned class for that version already exists with the class.</returns>
        [HttpPost("create/{versionId}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [Authorize()]
        public async Task<ActionResult> Create([FromBody] CreateVersionedClassModel mapping)
        {
            var currentGameVersion = await _gameVersionReader.GetById(mapping.GameVersion);
            if (currentGameVersion == null)
                return BadRequest("No game version with that id has been registered yet.");

            var user = await _userResolvingService.Get();
            if (user == null || !user.CanCommit)
                return Unauthorized();

            VersionedComponent outer = null;
            if (mapping.Outer.HasValue)
            {
                outer = await ComponentWriter.GetVersionedComponent(mapping.Outer.Value);
                if (outer == null)
                    return BadRequest("Unknown outer class");
            }

            var inheritsFrom =
                (await Task.WhenAll(
                    mapping.InheritsFrom.Select(async id => (await ComponentWriter.GetVersionedComponent(id)).Metadata as ClassMetadata))).ToList();

            if (inheritsFrom.Any(m => m == null))
                return BadRequest("Unknown inheriting class.");

            var classMapping = await ComponentWriter.GetById(mapping.VersionedMappingFor);
            if (classMapping == null)
                return BadRequest("Unknown class mapping to create the versioned mapping for.");

            if (classMapping.VersionedComponents.Any(versionedMapping =>
                versionedMapping.GameVersion.Id == mapping.GameVersion))
                return Conflict();

            var versionedClassMapping = new VersionedComponent
            {
                CreatedBy = user.Id,
                CreatedOn = DateTime.Now,
                GameVersion = currentGameVersion,
                Mappings = new List<CommittedMapping>(),
                Proposals = new List<ProposedMapping>()
            };

            versionedClassMapping.Metadata = new ClassMetadata
            {
                VersionedComponent = versionedClassMapping,
                VersionedComponentForeignKey = versionedClassMapping.Id,
                Outer=outer?.Metadata as ClassMetadata,
                Package = mapping.Package,
                InheritsFrom = inheritsFrom,
            };

            var initialLiveMappings = mapping.Mappings
                .Select(mappingData => new CommittedMapping()
                {
                    Documentation = mappingData.Documentation,
                    Distribution = mappingData.Distribution,
                    InputMapping = mappingData.In,
                    OutputMapping = mappingData.Out,
                    MappingType = _mappingTypeReader.GetByName(mappingData.MappingTypeName).Result,
                    ProposedMapping = null,
                    Releases = new List<ReleaseComponent>(),
                    VersionedComponent = versionedClassMapping,
                    CreatedOn = DateTime.Now
                });

            versionedClassMapping.Mappings.AddRange(initialLiveMappings);
            await ComponentWriter.SaveChanges();

            return CreatedAtAction("GetById", versionedClassMapping.Id, classMapping);
        }

        protected override ClassVersionedReadModel ConvertVersionedDbModelToReadModel(VersionedComponent versionedComponent)
        {
            var outerId = (versionedComponent.Metadata as ClassMetadata)?.Outer?.VersionedComponent.Id;

            return new ClassVersionedReadModel
            {
                Id = versionedComponent.Id,
                VersionedViewModelFor = versionedComponent.Component.Id,
                GameVersion = versionedComponent.GameVersion.Id,
                Outer = outerId,
                Package = (versionedComponent.Metadata as ClassMetadata)?.Package,
                InheritsFrom = (versionedComponent.Metadata as ClassMetadata)?.InheritsFrom.ToList().Select(parentClass => parentClass.VersionedComponent.Id),
                CurrentMappings = versionedComponent.Mappings.ToList().Select(ConverterUtils.ConvertLiveDbModelToMappingReadModel),
                Proposals = versionedComponent.Proposals.ToList().Select(ConverterUtils.ConvertProposalDbModelToProposalReadModel),
                LockedMappingNames = versionedComponent.LockedMappingTypes.ToList().Select(lm => lm.MappingType.Name),
                Methods = (versionedComponent.Metadata as ClassMetadata)?.Methods.Select(m => m.VersionedComponent.Id),
                Fields = (versionedComponent.Metadata as ClassMetadata)?.Fields.Select(f => f.VersionedComponent.Id)
            };
        }
    }
}
