using System;
using System.Linq;
using Data.Core.Models.Mapping;
using Data.Core.Models.Mapping.Mappings;
using Data.WebApi.Model.Read.Core;

namespace Data.WebApi.Services.Converters
{
    public static class ConverterUtils
    {
        public static ProposalReadModel ConvertProposalDbModelToProposalReadModel(this ProposedMapping proposedMapping)
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

        public static MappingReadModel ConvertLiveDbModelToMappingReadModel(this CommittedMapping committedMapping)
        {
            return new MappingReadModel()
            {
                Id = committedMapping.Id,
                In = committedMapping.InputMapping,
                Out = committedMapping.OutputMapping,
                Documentation = committedMapping.Documentation,
                MappingName = committedMapping.MappingType.Name,
                Distribution = committedMapping.Distribution
            };
        }
    }
}
