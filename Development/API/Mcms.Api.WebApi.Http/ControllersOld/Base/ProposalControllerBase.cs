using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mcms.Api.Data.Poco.Models.Core.Release;
using Mcms.Api.Data.Poco.Models.Mapping;
using Mcms.Api.Data.Poco.Models.Mapping.Mappings;
using Mcms.Api.Data.Poco.Readers.Core;
using Mcms.Api.Data.Poco.Writers.Core;
using Data.WebApi.Model;
using Data.WebApi.Model.Creation.Core;
using Data.WebApi.Model.Read.Core;
using Data.WebApi.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Data.WebApi.Controllers.Base
{
    public abstract class ProposalControllerBase : Controller
    {
        private readonly IComponentWriter _componentWriter;

        private readonly IUserResolvingService _userResolvingService;

        private readonly IMappingTypeReader _mappingTypeReader;

        protected ProposalControllerBase(IComponentWriter componentWriter, IUserResolvingService userResolvingService, IMappingTypeReader mappingTypeReader)
        {
            _componentWriter = componentWriter;
            _userResolvingService = userResolvingService;
            _mappingTypeReader = mappingTypeReader;
        }

        /// <summary>
        /// Returns the proposal with the given id.
        /// </summary>
        /// <param name="id">The id of the component to lookup.</param>
        /// <returns>200-The component with the requested id. 404-Not found.</returns>
        [HttpGet("id/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        public async Task<ActionResult<ProposalReadModel>> GetById(Guid id)
        {
            var byId = await _componentWriter.GetProposalMapping(id);

            if (byId == null)
                return NotFound();

            return Json(ConvertProposalDbModelToProposalReadModel(byId));
        }

        /// <summary>
        /// Method used to create a new proposal.
        /// </summary>
        /// <param name="proposalModel">The model for the proposal.</param>
        /// <returns>An http response code: 201-Created new proposal, 404-Unknown class, 401-Unauthorized user.</returns>
        [HttpPost("")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize()]
        public async Task<ActionResult> Propose([FromBody] CreateProposalModel proposalModel)
        {
            var classVersionedEntry = await _componentWriter.GetVersionedComponent(proposalModel.ProposedFor);
            if (classVersionedEntry == null)
                return NotFound(
                    $"Their is no component with a version component with id: {proposalModel.ProposedFor}");

            var user = await _userResolvingService.Get();
            if (user == null)
                return Unauthorized();

            var requestedMappingType = _mappingTypeReader.GetByName(proposalModel.MappingTypeName);
            if (requestedMappingType == null)
                return NotFound(
                    $"Their is no mapping type with the given name: {proposalModel.MappingTypeName}");

            if (classVersionedEntry.LockedMappingTypes.Any(l => l.MappingType.Name == proposalModel.MappingTypeName))
                return Unauthorized("The component is locked for the given version and mapping name.");

            var initialVotedFor = new List<Guid> {user.Id};
            var initialVotedAgainst = new List<Guid>();

            var proposalEntry = new ProposedMapping()
            {
                VersionedComponent = classVersionedEntry,
                InputMapping = proposalModel.NewInput,
                OutputMapping = proposalModel.NewOutput,
                ProposedBy = user.Id,
                ProposedOn = DateTime.Now,
                IsOpen = true,
                IsPublicVote = proposalModel.IsPublicVote,
                VotedFor = initialVotedFor,
                VotedAgainst = initialVotedAgainst,
                Comment = proposalModel.Comment,
                CreatedOn = DateTime.Now,
                ClosedBy = null,
                ClosedOn = null,
                Merged = null,
                CommittedWith = null,
                CommittedWithId = null
            };

            classVersionedEntry.Proposals.Add(proposalEntry);

            await this._componentWriter.Update(classVersionedEntry);
            await this._componentWriter.SaveChanges();

            return CreatedAtAction("GetById", proposalEntry.Id, proposalEntry);
        }

        /// <summary>
        /// Marks the current user as a person who voted for the proposal.
        /// </summary>
        /// <param name="proposalId">The id of the proposal for which is voted for.</param>
        /// <returns>An http response code: 200-Ok, 400-closed proposal, 401-Unauthorized user, 404-Unknown proposal.</returns>
        [HttpPatch("vote/{proposalId}/for")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [Authorize()]
        public async Task<ActionResult> VoteFor(Guid proposalId)
        {
            var currentProposal = await _componentWriter.GetProposalMapping(proposalId);
            if (currentProposal == null)
                return NotFound("No proposal with the given id exists.");

            if (!currentProposal.IsOpen)
                return BadRequest("Proposal is not open");

            var user = await _userResolvingService.Get();
            if (user == null)
                return Unauthorized();

            if (!currentProposal.IsPublicVote && user.CanReview)
                return Unauthorized();

            if (currentProposal.VotedFor.Contains(user.Id))
                return Conflict();

            currentProposal.VotedAgainst.Remove(user.Id);
            currentProposal.VotedFor.Add(user.Id);
            await _componentWriter.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// Marks the current user as a person who voted against the proposal.
        /// </summary>
        /// <param name="proposalId">The id of the proposal for which is voted against.</param>
        /// <returns>An http response code: 200-Ok, 400-Closed proposal, 401-Unauthorized user, 404-Unknown proposal</returns>
        [HttpPatch("vote/{proposalId}/against")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [Authorize()]
        public async Task<ActionResult> VoteAgainst(Guid proposalId)
        {
            var currentProposal = await _componentWriter.GetProposalMapping(proposalId);
            if (currentProposal == null)
                return NotFound("No proposal with the given id exists.");

            if (!currentProposal.IsOpen)
                return BadRequest("Proposal is not open");

            var user = await _userResolvingService.Get();
            if (user == null)
                return Unauthorized();

            if (!currentProposal.IsPublicVote && user.CanReview)
                return Unauthorized();

            if (currentProposal.VotedAgainst.Contains(user.Id))
                return Conflict();

            currentProposal.VotedFor.Remove(user.Id);
            currentProposal.VotedAgainst.Add(user.Id);
            await _componentWriter.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// Closes an open proposal.
        /// </summary>
        /// <param name="proposalId">The id of the proposal to close.</param>
        /// <param name="merge">True to merge a proposal as a committed mapping, false when not.</param>
        /// <returns>An http response code: 200-Ok proposal closed, 201-Created proposal merged, 400-Unknown or closed proposal, 401-Unauthorized user.</returns>
        [HttpPatch("close/{proposalId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [Authorize()]
        public async Task<ActionResult> Close(Guid proposalId, bool merge)
        {
            var currentProposal = await _componentWriter.GetProposalMapping(proposalId);
            if (currentProposal == null)
                return NotFound("No proposal with the given id exists.");

            if (!currentProposal.IsOpen)
                return BadRequest("Proposal is not open");

            var user = await _userResolvingService.Get();
            if (user == null)
                return Unauthorized();

            if (!currentProposal.IsPublicVote && user.CanCommit)
                return Unauthorized();

            return await ProcessClosing(merge, currentProposal, user);
        }

        protected ProposalReadModel ConvertProposalDbModelToProposalReadModel(ProposedMapping proposedMapping)
        {
            return new ProposalReadModel()
            {
                Id = proposedMapping.Id,
                ProposedFor = proposedMapping.VersionedComponent.Id,
                GameVersion = proposedMapping.VersionedComponent.GameVersion.Id,
                ProposedBy = proposedMapping.ProposedBy,
                ProposedOn = proposedMapping.ProposedOn,
                IsOpen = proposedMapping.IsOpen,
                IsPublicVote = proposedMapping.IsPublicVote,
                VotedFor = proposedMapping.VotedFor.ToList(),
                VotedAgainst = proposedMapping.VotedAgainst.ToList(),
                Comment = proposedMapping.Comment,
                ClosedBy = proposedMapping.ClosedBy,
                ClosedOn = proposedMapping.ClosedOn,
                In = proposedMapping.InputMapping,
                Out = proposedMapping.OutputMapping,
                Documentation = proposedMapping.Documentation,
                MappingName = proposedMapping.MappingType.Name,
                Distribution = proposedMapping.Distribution
            };
        }

        private async Task<ActionResult> ProcessClosing(bool merge, ProposedMapping currentProposedMapping, User user)
        {
            currentProposedMapping.ClosedBy = user.Id;
            currentProposedMapping.ClosedOn = DateTime.Now;
            currentProposedMapping.Merged = merge;

            if (merge)
            {
                var newCommittedMapping = new CommittedMapping()
                {
                    InputMapping = currentProposedMapping.InputMapping,
                    OutputMapping = currentProposedMapping.OutputMapping,
                    ProposedMapping = currentProposedMapping,
                    Releases = new List<ReleaseComponent>(),
                    VersionedComponent = currentProposedMapping.VersionedComponent,
                    CreatedOn = DateTime.Now
                };

                currentProposedMapping.VersionedComponent.Mappings.Add(newCommittedMapping);
                await _componentWriter.SaveChanges();

                return CreatedAtAction("GetById", newCommittedMapping.VersionedComponent.Component.Id, newCommittedMapping);
            }

            await _componentWriter.SaveChanges();
            return Ok();
        }
    }
}
