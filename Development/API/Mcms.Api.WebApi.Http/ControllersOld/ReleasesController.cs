using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mcms.Api.Data.Poco.Models.Core;
using Mcms.Api.Data.Poco.Models.Core.Release;
using Mcms.Api.Data.Poco.Models.Mapping;
using Mcms.Api.Data.Poco.Models.Mapping.Component;
using Mcms.Api.Data.Poco.Readers.Core;
using Mcms.Api.Data.Poco.Readers.Mapping;
using Mcms.Api.Data.Poco.Writers.Core;
using Mcms.Api.Data.Poco.Writers.Mapping;
using Data.EFCore.Writer.Mapping;
using Data.WebApi.Model.Creation.Core;
using Data.WebApi.Model.Read.Core;
using Data.WebApi.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Data.WebApi.Controllers
{
    //TODO: Add lookup of mapping types to releases.
    /// <summary>
    /// A controller for access to game version.
    /// </summary>
    [ApiController()]
    [Route("/releases")]
    public class ReleaseController : Controller
    {
        /// <summary>
        /// The game version reader and writer.
        /// </summary>
        private readonly IReleaseWriter _releaseWriter;

        /// <summary>
        /// The user resolving service.
        /// </summary>
        private readonly IUserResolvingService _userResolvingService;

        /// <summary>
        /// Game version reader.
        /// </summary>
        private readonly IGameVersionReader _gameVersionReader;

        private readonly IMappingTypeReader _mappingTypeReader;

        private readonly IClassComponentReader _classes;

        private readonly IMethodComponentReader _methods;

        private readonly IFieldComponentReader _fields;

        private readonly IParameterComponentReader _parameters;

        /// <summary>
        /// Creates a new game version controller.
        /// </summary>
        /// <param name="releaseWriter">The writer and reader for releases.</param>
        /// <param name="userResolvingService">The user resolving service.</param>
        /// <param name="gameVersionReader">The reader for releases.</param>
        /// <param name="mappingTypeReader">The mapping type reader.</param>
        /// <param name="classes">The class mapping reader.</param>
        /// <param name="methods">The method mapping reader.</param>
        /// <param name="fields">The field mapping reader.</param>
        /// <param name="parameters">The parameter mapping reader.</param>
        public ReleaseController(
            IReleaseWriter releaseWriter,
            IUserResolvingService userResolvingService,
            IGameVersionReader gameVersionReader,
            IMappingTypeReader mappingTypeReader,
            IClassComponentReader classes,
            IMethodComponentReader methods,
            IFieldComponentReader fields,
            IParameterComponentReader parameters)
        {
            _releaseWriter = releaseWriter;
            _userResolvingService = userResolvingService;
            _gameVersionReader = gameVersionReader;
            _mappingTypeReader = mappingTypeReader;
            _classes = classes;
            _methods = methods;
            _fields = fields;
            _parameters = parameters;
        }

        /// <summary>
        /// Allows look up of the releases by a given id.
        /// </summary>
        /// <param name="id">The id of the game version that wants to be looked up.</param>
        /// <returns>The game version api model that reflects the lookup game version table entry.</returns>
        [HttpGet("id/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<ActionResult<ReleaseReadModel>> GetById(Guid id)
        {
            var dbModel = await _releaseWriter.GetById(id);
            if (dbModel == null)
                return NotFound();

            return ConvertDbModelToApiModel(dbModel);
        }

        /// <summary>
        /// Allows look up of the releases by a given name.
        /// </summary>
        /// <param name="name">The name of the game version that wants to be looked up.</param>
        /// <returns>The game version api model that reflects the lookup game version table entry.</returns>
        [HttpGet("name/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<ActionResult<ReleaseReadModel>> GetByName(string name)
        {
            var dbModel = await _releaseWriter.GetByName(name);
            if (dbModel == null)
                return NotFound();

            return ConvertDbModelToApiModel(dbModel);
        }

        /// <summary>
        /// Allows for the lookup of the entire releases table.
        /// </summary>
        /// <returns>All known releases.</returns>
        [HttpGet("all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<ActionResult<IQueryable<ReleaseReadModel>>> AsQueryable()
        {
            var dbModels = await _releaseWriter.AsQueryable();

            return Json(dbModels.ToList().Select(ConvertDbModelToApiModel));
        }

        /// <summary>
        /// Gives access to all releases created by a given user.
        /// </summary>
        /// <param name="userId">The id of the user who created the releases in question.</param>
        /// <returns>The releases created by the user in question.</returns>
        [HttpGet("made/by/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<ActionResult<IQueryable<Release>>> GetMadeBy(Guid userId)
        {
            var dbModels = await _releaseWriter.GetMadeBy(userId);

            return Json(dbModels.ToList().Select(ConvertDbModelToApiModel));
        }

        /// <summary>
        /// Gives access to all releases created on a given date.
        /// </summary>
        /// <param name="date">The date that the release was created in question.</param>
        /// <returns>The releases created on the date in question.</returns>
        [HttpGet("made/on/{date}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<ActionResult<IQueryable<Release>>> GetMadeOn(DateTime date)
        {
            var dbModels = await _releaseWriter.GetMadeOn(date.Date);

            return Json(dbModels.ToList().Select(ConvertDbModelToApiModel));
        }

        /// <summary>
        /// Gives access to all releases made for a given game version.
        /// </summary>
        /// <param name="id">The id of the game version in question.</param>
        /// <returns>The releases created for the given release.</returns>
        [HttpGet("made/for/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<ActionResult<IQueryable<Release>>> GetMadeForVersion(Guid id)
        {
            var dbModels = await _releaseWriter.GetMadeForVersion(id);

            return Json(dbModels.ToList().Select(ConvertDbModelToApiModel));
        }

        /// <summary>
        /// Allows for the lookup of the latest game version.
        /// </summary>
        /// <returns>The latest game version.</returns>
        [HttpGet("latest")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<ActionResult<ReleaseReadModel>> GetLatest()
        {
            var dbModel = await _releaseWriter.GetLatest();
            if (dbModel == null)
                return BadRequest();

            return ConvertDbModelToApiModel(dbModel);
        }

        /// <summary>
        /// Allows for the creation of a release, from a given name and game version.
        /// Collects all current mappings in that version in one release.
        /// </summary>
        [HttpPost("latest")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [Authorize()]
        public async Task<ActionResult> Add([FromBody] CreateReleaseModel mapping)
        {
            var user = await _userResolvingService.Get();
            if (user == null || !user.CanRelease)
                return Unauthorized();

            var gameVersion = await _gameVersionReader.GetById(mapping.GameVersion);
            var mappingType = await _mappingTypeReader.GetByName(mapping.MappingTypeName);

            var release = new Release
            {
                CreatedBy = user.Id,
                CreatedOn = DateTime.Now,
                Name = mapping.Name,
                Id = Guid.NewGuid(),
                GameVersion = gameVersion,
                MappingType = mappingType,
                IsSnapshot = mapping.IsSnapshot
            };

            var releaseClasses = (await _classes.GetByVersion(gameVersion)).Select(classMapping =>
                classMapping.VersionedComponents
                    .FirstOrDefault(versionedMapping => versionedMapping.GameVersion == gameVersion).Mappings
                    .OrderByDescending(committedMappings => committedMappings.CreatedOn).FirstOrDefault()).Select(
                committedMapping => new ReleaseComponent
                {
                    Id = new Guid(),
                    ComponentType = ComponentType.CLASS,
                    Release = release,
                    Mapping = committedMapping
                }).ToList();

            var releaseMethods = (await _methods.GetByVersion(gameVersion)).Select(methodMapping =>
                methodMapping.VersionedComponents
                    .FirstOrDefault(versionedMapping => versionedMapping.GameVersion == gameVersion).Mappings
                    .OrderByDescending(committedMappings => committedMappings.CreatedOn).FirstOrDefault()).Select(
                committedMapping => new ReleaseComponent()
                {
                    Id = new Guid(),
                    ComponentType = ComponentType.METHOD,
                    Release = release,
                    Mapping = committedMapping
                }).ToList();

            var releaseFields = (await _fields.GetByVersion(gameVersion)).Select(fieldMapping =>
                fieldMapping.VersionedComponents
                    .FirstOrDefault(versionedMapping => versionedMapping.GameVersion == gameVersion).Mappings
                    .OrderByDescending(committedMappings => committedMappings.CreatedOn).FirstOrDefault()).Select(
                committedMapping => new ReleaseComponent()
                {
                    Id = new Guid(),
                    ComponentType = ComponentType.FIELD,
                    Release = release,
                    Mapping = committedMapping
                }).ToList();

            var releaseParameters = (await _parameters.GetByVersion(gameVersion)).Select(parameterMapping =>
                parameterMapping.VersionedComponents
                    .FirstOrDefault(versionedMapping => versionedMapping.GameVersion == gameVersion).Mappings
                    .OrderByDescending(committedMappings => committedMappings.CreatedOn).FirstOrDefault()).Select(
                committedMapping => new ReleaseComponent()
                {
                    Id = new Guid(),
                    ComponentType = ComponentType.FIELD,
                    Release = release,
                    Mapping = committedMapping
                }).ToList();

            release.Components.AddRange(releaseClasses);
            release.Components.AddRange(releaseMethods);
            release.Components.AddRange(releaseFields);
            release.Components.AddRange(releaseParameters);

            await _releaseWriter.Add(release);
            await _releaseWriter.SaveChanges();

            return CreatedAtAction("GetById", release.Id, release);
        }

        private ReleaseReadModel ConvertDbModelToApiModel(Release release)
        {
            return new ReleaseReadModel
            {
                Id = release.Id,
                CreatedBy = release.CreatedBy,
                CreatedOn = release.CreatedOn,
                Name = release.Name,
                MappingTypeName = release.MappingType.Name,
                IsSnapshot = release.IsSnapshot,
                GameVersion = release.GameVersion.Id,
                Classes = release.Components.Where(rc => rc.ComponentType == ComponentType.CLASS).Select(member => member.Id).ToList(),
                Methods = release.Components.Where(rc => rc.ComponentType == ComponentType.METHOD).Select(member => member.Id).ToList(),
                Fields = release.Components.Where(rc => rc.ComponentType == ComponentType.FIELD).Select(member => member.Id).ToList(),
                Parameters = release.Components.Where(rc => rc.ComponentType == ComponentType.PARAMETER).Select(member => member.Id).ToList()
            };
        }
    }
}
