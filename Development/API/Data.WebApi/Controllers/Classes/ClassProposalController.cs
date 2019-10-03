using Data.Core.Readers.Core;
using Data.Core.Readers.Mapping;
using Data.Core.Writers.Core;
using Data.Core.Writers.Mapping;
using Data.WebApi.Controllers.Base;
using Data.WebApi.Services.Core;
using Microsoft.AspNetCore.Mvc;

namespace Data.WebApi.Controllers.Classes
{
    /// <summary>
    /// Controller that handles interactions on proposal mappings for classes.
    /// </summary>
    [Route("/classes/mappings/proposals")]
    [ApiController]
    public class ClassProposalController
        : ProposalControllerBase
    {
        public ClassProposalController(IClassComponentWriter componentWriter, IUserResolvingService userResolvingService, IMappingTypeReader mappingTypeReader) : base(componentWriter, userResolvingService, mappingTypeReader)
        {
        }
    }
}
