package com.mcms.data.core.repositories.comments;

import java.util.UUID;

import com.mcms.api.datamodel.comments.CommentReactionDMO;
import com.mcms.data.core.repositories.IRepository;
import org.reactivestreams.Publisher;
import reactor.core.publisher.Flux;

/**
 * Represents a repository that can provide and store {@link CommentReactionDMO} objects.
 */
public interface ICommentReactionRepository extends IRepository<CommentReactionDMO> {

    /**
     * Finds all comment reactions which where directly made on the comment reaction with the given id.
     *
     * The comment reactions are sorted oldest to newest.
     *
     * @param commentReactionId The id of the comment reaction to look up comment reactions for.
     * @return The comment reactions on the given comment reaction.
     * @throws IllegalArgumentException in case the given {@literal commentReactionId} is {@literal null}.
     */
    Flux<CommentReactionDMO> findAllForComment(final UUID commentReactionId);

    /**
     * Finds all comment reactions which where directly made by the user with the given id.
     *
     * The comment reactions are sorted newest to oldest.
     *
     * @param userId The id of the user to look up comment reactions for.
     * @return The comment reactions by the given user.
     * @throws IllegalArgumentException in case the given {@literal userId} is {@literal null}.
     */
    Flux<CommentReactionDMO> findAllForUser(final UUID userId);
}
