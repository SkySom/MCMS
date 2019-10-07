using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Core.Models.Core;
using Data.Core.Models.Mapping.Mappings;

namespace Data.Core.Models.Mapping
{
    public class ReleaseComponent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public ComponentType ComponentType { get; set; }

        [Required]
        public virtual Release Release { get; set; }

        [Required]
        public virtual CommittedMapping Member { get; set; }
    }
}
