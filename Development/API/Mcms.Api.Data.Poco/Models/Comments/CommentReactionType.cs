namespace Mcms.Api.Data.Poco.Models.Comments
{
    /// <summary>
    /// Represents a single reaction type on a comment.
    /// </summary>
    public enum CommentReactionType
    {
        /// <summary>
        /// :+1:
        /// </summary>
        PLUS_ONE,

        /// <summary>
        /// :-1:
        /// </summary>
        MINUS_ONE,

        /// <summary>
        /// :smile:
        /// </summary>
        SMILE,

        /// <summary>
        /// :confused:
        /// </summary>
        CONFUSED,

        /// <summary>
        /// :heart:
        /// </summary>
        HEART,

        /// <summary>
        /// :tada:
        /// </summary>
        HOORAY,

        /// <summary>
        /// :rocket:
        /// </summary>
        ROCKET,

        /// <summary>
        /// :eyes:
        /// </summary>
        EYES
    }
}
