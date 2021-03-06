using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mcms.Api.Data.Poco.Models.Mapping.Component;
using Mcms.Api.Data.Poco.Models.Mapping.Mappings;

namespace Mcms.Api.Data.Poco.Models.Core.Release
{
    /// <summary>
    /// A single part of a release.
    /// Represents a mapping of a component from its input to its output, by linking the release entry and the committed mapping in the database.
    /// </summary>
    public class ReleaseComponent
    {
        /// <summary>
        /// The id of the release component
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// The release that is release component is part of.
        /// </summary>
        [Required]
        public virtual Release Release { get; set; }

        /// <summary>
        /// The mapping that is part of this release.
        /// </summary>
        [Required]
        public virtual CommittedMapping Mapping { get; set; }
    }
}
