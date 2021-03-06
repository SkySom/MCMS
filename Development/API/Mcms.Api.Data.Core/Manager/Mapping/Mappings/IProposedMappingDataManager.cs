using System;
using System.Linq;
using System.Threading.Tasks;
using Mcms.Api.Data.Poco.Models.Mapping.Component;
using Mcms.Api.Data.Poco.Models.Mapping.Mappings;

namespace Mcms.Api.Data.Core.Manager.Mapping.Mappings
{
    /// <summary>
    /// Manager that handles interaction with proposed mappings.
    /// </summary>
    public interface IProposedMappingDataManager
    {

        /// <summary>
        /// Finds all proposed mappings with the given id.
        /// </summary>
        /// <param name="id">The given id to find proposed mappings by,</param>
        /// <returns>The task that looks up the proposed mappings that match the given id.</returns>
        Task<IQueryable<ProposedMapping>> FindById(Guid id);

        /// <summary>
        /// Finds all proposed mappings with a given <see cref="ComponentType"/>.
        /// </summary>
        /// <param name="type">The type tho match proposed mappings type against.</param>
        /// <returns>That task that looks up the proposed mappings that match a given type.</returns>
        Task<IQueryable<ProposedMapping>> FindByType(ComponentType type);

        /// <summary>
        /// Finds all proposed mappings that are part of the component who's id matches the given one.
        /// </summary>
        /// <param name="componentId">The id of the component to find the proposed mappings for.</param>
        /// <returns>The task that looks up the proposed mappings that are part of the component who's id matches the given one.</returns>
        Task<IQueryable<ProposedMapping>> FindByComponentId(Guid componentId);

        /// <summary>
        /// Finds all proposed mappings that are part of a given versioned component, who's id matches the given one.
        /// </summary>
        /// <param name="versionedComponentId">The id of the versioned component to find the proposed mappings for.</param>
        /// <returns>The task that looks up the proposed mappings that are part of the versioned component who's id matches the given one.</returns>
        Task<IQueryable<ProposedMapping>> FindByVersionedComponentId(Guid versionedComponentId);

        /// <summary>
        /// Finds a all proposed mappings (either input or output) that matches the given
        /// mapping regex, additionally the mapping has to be of a type who's name matches the given regex as well.
        ///
        /// Unions <see cref="FindByOutputMapping(string, string)"/> and <see cref="FindByInputMapping(string, string)"/> together.
        /// </summary>
        /// <param name="mappingTypeNameRegex">The regex to match a mappings mapping type name against.</param>
        /// <param name="mappingRegex">The regex against which a mapping is matched, for which proposed mappings are found.</param>
        /// <returns>The task that looks up proposed mappings with a mapping that match the given mapping regex.</returns>
        Task<IQueryable<ProposedMapping>> FindByMapping(string mappingTypeNameRegex, string mappingRegex);

        /// <summary>
        /// Finds a all proposed mappings that have an output that matches the given
        /// mapping regex, additionally the mapping has to be of a type who's name matches the given regex as well.
        /// </summary>
        /// <param name="mappingTypeNameRegex">The regex to match a mappings mapping type name against.</param>
        /// <param name="mappingRegex">The regex against which an output mapping is matched, for which proposed mappings are found.</param>
        /// <returns>The task that looks up proposed mappings with an output mapping that match the given mapping regex.</returns>
        Task<IQueryable<ProposedMapping>> FindByOutputMapping(string mappingTypeNameRegex, string mappingRegex);

        /// <summary>
        /// Finds a all proposed mappings that have an input that matches the given
        /// mapping regex, additionally the mapping has to be of a type who's name matches the given regex as well.
        /// </summary>
        /// <param name="mappingTypeNameRegex">The regex to match a mappings mapping type name against.</param>
        /// <param name="mappingRegex">The regex against which an input mapping is matched, for which proposed mappings are found.</param>
        /// <returns>The task that looks up proposed mappings with an input mapping that match the given mapping regex.</returns>
        Task<IQueryable<ProposedMapping>> FindByInputMapping(string mappingTypeNameRegex, string mappingRegex);

        /// <summary>
        /// Finds all proposed mappings that are part of a game version who's name matches the given regex.
        /// </summary>
        /// <param name="gameVersionRegex">The regex to match game version names agents.</param>
        /// <returns>The task that looks up proposed mappings with a versioned component for a game version who's name matches the given regex.</returns>
        Task<IQueryable<ProposedMapping>> FindByGameVersion(string gameVersionRegex);

        ///  <summary>
        /// <para>
        /// Finds all proposed mappings who match the given filter data.
        /// </para>
        /// <para>
        /// This methods finds the intersection between <see cref="FindById(Guid)"/> if the <paramref name="id"/> parameter is not <code>null</code>,
        /// <see cref="FindByType(ComponentType)"/> if the <paramref name="type"/> is not null,
        /// <see cref="FindByComponentId(Guid)"/> if the <paramref name="componentId"/> parameter is not <code>null</code>,
        /// <see cref="FindByVersionedComponentId(Guid)"/> if the <paramref name="versionedComponentId"/> parameter is not <code>null</code>,
        /// <see cref="FindByMapping(string, string)"/> if either, the <paramref name="mappingTypeNameRegex"/> and or <paramref name="mappingRegex"/> parameter is not <code>null</code>,
        /// <see cref="FindByGameVersion(string)"/> if the <paramref name="gameVersionRegex"/> parameter is not <code>null</code>.
        /// </para>
        /// <para>
        /// This means that for a component to be returned it has to match all the provided filter data.
        /// </para>
        /// <para>
        /// </para>
        /// If no parameter is provided, or all are <code>null</code>, then all entries are returned.
        /// </summary>
        /// <param name="id">The given id to find proposed mappings by,</param>
        /// <param name="type">The <see cref="ComponentType"/> to match the proposed mappings against.</param>
        /// <param name="componentId">The id of the component to find the proposed mappings for.</param>
        /// <param name="versionedComponentId">The id of the versioned component to find the proposed mappings for.</param>
        /// <param name="mappingTypeNameRegex">The regex to match a mappings mapping type name against.</param>
        /// <param name="mappingRegex">The regex against which a mapping is matched, for which proposed mappings are found.</param>
        /// <param name="gameVersionRegex">The regex to match game version names agents.</param>
        ///  <param name="isOpen">Indicator used to filter proposals which are open or closed.</param>
        ///  <param name="isPublicVote">Indicator used to filter proposals which are public or not.</param>
        ///  <param name="closedBy">Indicator used to filter by who closed the proposal.</param>
        ///  <param name="closedOn">Indicator used to filter by the date the proposal was closed.</param>
        ///  <param name="merged">Indicator used to filter the proposal on merged or not status.</param>
        ///  <returns>The task that represents the lookup of proposed mappings that match the filter data, based on intersection.</returns>
        Task<IQueryable<ProposedMapping>> FindUsingFilter(
            Guid? id = null,
            ComponentType? type = null,
            Guid? componentId = null,
            Guid? versionedComponentId = null,
            string mappingTypeNameRegex = null,
            string mappingRegex = null,
            string gameVersionRegex = null,
            bool? isOpen = null,
            bool? isPublicVote = null,
            Guid? closedBy = null,
            DateTime? closedOn = null,
            bool? merged = null
        );

        /// <summary>
        /// Creates a new proposed mapping.
        /// The created proposed mapping is not saved directly, but has to be saved separately.
        /// </summary>
        /// <param name="proposedMapping">The proposed mapping to create.</param>
        /// <returns>The task that describes the creation of the proposed mapping.</returns>
        /// <exception cref="InvalidOperationException">is thrown when this method is called with either a known proposed mapping or a deleted proposed mapping.</exception>
        Task CreateProposedMapping(
            ProposedMapping proposedMapping
        );

        /// <summary>
        /// Updates an already existing proposed mapping.
        /// Updates are not proposed to the backing store directly, but have to be saved separately.
        /// </summary>
        /// <param name="proposedMapping">The proposed mapping to update.</param>
        /// <returns>The task that describes the updating of the manager with the data from the proposed mapping.</returns>
        /// <exception cref="InvalidOperationException">is thrown when this method is called with either an unknown proposed mapping (unproposed deleted) or an unproposed created proposed mapping.</exception>
        Task UpdateProposedMapping(
            ProposedMapping proposedMapping
        );

        /// <summary>
        /// Deletes an already existing proposed mapping.
        /// Deletions are not proposed to the backing store directly but have to be saved separately.
        /// </summary>
        /// <param name="proposedMapping">The proposed mapping to delete.</param>
        /// <returns>The task that describes the deletion of the proposed mapping from the manager.</returns>
        Task DeleteProposedMapping(
            ProposedMapping proposedMapping
        );

        /// <summary>
        /// Indicates if the manager has pending changes that need to be saved.
        /// </summary>
        bool HasPendingChanges { get; }

        /// <summary>
        /// Saves the pending changes.
        /// If <see cref="HasPendingChanges"/> is <code>false</code> then this method short circuits into a <code>noop;</code>
        /// </summary>
        /// <returns>The task that describes the saving of the pending changes in this manager.</returns>
        Task SaveChanges();
    }
}
