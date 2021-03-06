using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mcms.Api.Data.Poco.Models.Core.Release;
using Mcms.Api.Data.Poco.Models.Mapping.Mappings;

namespace Mcms.Api.Data.Poco.Models.Comments
{

    /// <summary>
    /// Represents a single comment made with in the system.
    /// </summary>
    public class Comment
    {
        /// <summary>
        /// The id of the comment.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// The id of the user who created the comment.
        /// </summary>
        [Required]
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// The moment the comment was created.
        /// </summary>
        [Required]
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// The content of the comment.
        /// </summary>
        [Required]
        public string Content { get; set; }

        /// <summary>
        /// The reactions applied to the comment.
        /// </summary>
        public List<CommentReaction> Reactions { get; set; }

        /// <summary>
        /// Indicates if the comment has been edited.
        /// </summary>
        public bool HasBeenEdited { get; set; }

        /// <summary>
        /// Indicates if the comment has been deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// The user who deleted the comment, can be the creating user or any user with enough rights.
        /// </summary>
        public Guid? DeletedBy { get; set; }

        /// <summary>
        /// The moment this comment was deleted.
        /// </summary>
        public DateTime? DeletedOn { get; set; }

        /// <summary>
        /// The proposed mapping this comment was made on.
        /// </summary>
        public ProposedMapping ProposedMapping { get; set; }

        /// <summary>
        /// The release this comment was made on.
        /// </summary>
        public Release Release { get; set; }

        /// <summary>
        /// The comment this comment was made on.
        /// </summary>
        public Comment Parent { get; set; }

        /// <summary>
        /// The comments made on this comment.
        /// </summary>
        public List<Comment> Children { get; set; }
    }
}
