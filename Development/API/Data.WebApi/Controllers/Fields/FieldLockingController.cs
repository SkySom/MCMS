using Data.Core.Readers.Core;
using Data.Core.Writers.Core;
using Data.Core.Writers.Mapping;
using Data.WebApi.Controllers.Base;
using Data.WebApi.Services.Core;
using Microsoft.AspNetCore.Mvc;

namespace Data.WebApi.Controllers.Fields
{
    /// <summary>
    /// Controller that handles interactions with regards to locking and unlocking fields.
    /// </summary>
    [Route("/fields/locking")]
    [ApiController]
    public class FieldLockingController
        : LockingControllerBase
    {
        public FieldLockingController(IFieldComponentWriter componentWriter, IUserResolvingService userResolvingService, IMappingTypeReader mappingTypeReader) : base(componentWriter, userResolvingService, mappingTypeReader)
        {
        }
    }
}
