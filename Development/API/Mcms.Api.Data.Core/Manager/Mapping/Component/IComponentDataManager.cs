using System;
using System.Linq;
using System.Threading.Tasks;
using Mcms.Api.Data.Poco.Models.Mapping.Component;

namespace Mcms.Api.Data.Core.Manager.Mapping.Component
{
    /// <summary>
    /// Manager that handles interaction with components.
    /// </summary>
    public interface IComponentDataManager
    {

        /// <summary>
        /// Finds all components with the given id.
        /// </summary>
        /// <param name="id">The given id to find components by,</param>
        /// <returns>The task that looks up the components that match the given id.</returns>
        Task<IQueryable<Data.Poco.Models.Mapping.Component.Component>> FindById(Guid id);

        /// <summary>
        /// Finds all component with a given <see cref="ComponentType"/>.
        /// </summary>
        /// <param name="type">The type tho match component types against.</param>
        /// <returns>That task that looks up the components that match a given type.</returns>
        Task<IQueryable<Data.Poco.Models.Mapping.Component.Component>> FindByType(ComponentType type);

        /// <summary>
        /// Finds a all components that have a mapping (either input or output) that matches the given
        /// mapping regex, additionally the mapping has to be of a type who's name matches the given regex as well.
        ///
        /// Unions <see cref="FindByOutputMapping(string, string)"/> and <see cref="FindByInputMapping(string, string)"/> together.
        /// </summary>
        /// <param name="mappingTypeNameRegex">The regex to match a mappings mapping type name against.</param>
        /// <param name="mappingRegex">The regex against which a mapping is matched, for which components are found.</param>
        /// <returns>The task that looks up components with a mapping that match the given mapping regex.</returns>
        Task<IQueryable<Data.Poco.Models.Mapping.Component.Component>> FindByMapping(string mappingTypeNameRegex, string mappingRegex);

        /// <summary>
        /// Finds a all components that have an output mapping that match the given
        /// mapping regex, additionally the mapping has to be of a type who's name matches the given regex as well.
        /// </summary>
        /// <param name="mappingTypeNameRegex">The regex to match a mappings mapping type name against.</param>
        /// <param name="mappingRegex">The regex against which an output mapping is matched, for which components are found.</param>
        /// <returns>The task that looks up components with an output mapping that match the given mapping regex.</returns>
        Task<IQueryable<Data.Poco.Models.Mapping.Component.Component>> FindByOutputMapping(string mappingTypeNameRegex, string mappingRegex);

        /// <summary>
        /// Finds a all components that have an input mapping that match the given
        /// mapping regex, additionally the mapping has to be of a type who's name matches the given regex as well.
        /// </summary>
        /// <param name="mappingTypeNameRegex">The regex to match a mappings mapping type name against.</param>
        /// <param name="mappingRegex">The regex against which an input mapping is matched, for which components are found.</param>
        /// <returns>The task that looks up components with an input mapping that match the given mapping regex.</returns>
        Task<IQueryable<Data.Poco.Models.Mapping.Component.Component>> FindByInputMapping(string mappingTypeNameRegex, string mappingRegex);

        /// <summary>
        /// Finds all components that are part of a release who's name matches the given regex.
        /// </summary>
        /// <param name="releaseNameRegex">The regex to match release names against.</param>
        /// <returns>The task that looks up components with at least a mapping that is part of a release who's name matches the given regex.</returns>
        Task<IQueryable<Data.Poco.Models.Mapping.Component.Component>> FindByRelease(string releaseNameRegex);

        /// <summary>
        /// Finds all components that are part of a game version who's name matches the given regex.
        /// </summary>
        /// <param name="gameVersionRegex">The regex to match game version names agents.</param>
        /// <returns>The task that looks up components with a versioned component for a game version who's name matches the given regex.</returns>
        Task<IQueryable<Data.Poco.Models.Mapping.Component.Component>> FindByGameVersion(string gameVersionRegex);

        /// <summary>
        /// <para>
        /// Finds all components who match the given filter data.
        /// </para>
        /// <para>
        /// This methods finds the intersection between <see cref="FindById(Guid)"/> if the <paramref name="id"/> parameter is not <code>null</code>,
        /// <see cref="FindByType(ComponentType)"/> if the <paramref name="type"/> is not null,
        /// <see cref="FindByMapping(string, string)"/> if either, the <paramref name="mappingTypeNameRegex"/> and or <paramref name="mappingRegex"/> parameter is not <code>null</code>,
        /// <see cref="FindByRelease(string)"/> if the <paramref name="releaseNameRegex"/> parameter is not <code>null</code>, and
        /// <see cref="FindByGameVersion(string)"/> if the <paramref name="gameVersionRegex"/> parameter is not <code>null</code>.
        /// </para>
        /// <para>
        /// This means that for a component to be returned it has to match all the provided filter data.
        /// </para>
        /// <para>
        /// </para>
        /// If no parameter is provided, or all are <code>null</code>, then all entries are returned.
        /// </summary>
        /// <param name="id">The given id to find components by,</param>
        /// <param name="type">The <see cref="ComponentType"/> to match the components against.</param>
        /// <param name="mappingTypeNameRegex">The regex to match a mappings mapping type name against.</param>
        /// <param name="mappingRegex">The regex against which a mapping is matched, for which components are found.</param>
        /// <param name="releaseNameRegex">The regex to match release names against.</param>
        /// <param name="gameVersionRegex">The regex to match game version names agents.</param>
        /// <returns>The task that represents the lookup of components that match the filter data, based on intersection.</returns>
        Task<IQueryable<Data.Poco.Models.Mapping.Component.Component>> FindUsingFilter(
            Guid? id = null,
            ComponentType? type = null,
            string mappingTypeNameRegex = null,
            string mappingRegex = null,
            string releaseNameRegex = null,
            string gameVersionRegex = null
        );

        /// <summary>
        /// Creates a new component.
        /// The created component is not saved directly, but has to be saved separately.
        /// </summary>
        /// <param name="component">The component to create.</param>
        /// <returns>The task that describes the creation of the component.</returns>
        /// <exception cref="InvalidOperationException">is thrown when this method is called with either a known component or a uncommitted deleted component.</exception>
        Task CreateComponent(
            Data.Poco.Models.Mapping.Component.Component component
        );

        /// <summary>
        /// Updates an already existing component.
        /// Updates are not committed to the backing store directly, but have to be saved separately.
        /// </summary>
        /// <param name="component">The component to update.</param>
        /// <returns>The task that describes the updating of the manager with the data from the component.</returns>
        /// <exception cref="InvalidOperationException">is thrown when this method is called with either an unknown component (uncommitted deleted) or an uncommitted created component.</exception>
        Task UpdateComponent(
            Data.Poco.Models.Mapping.Component.Component component
        );

        /// <summary>
        /// Deletes an already existing component.
        /// Deletions are not committed to the backing store directly but have to be saved separately.
        /// </summary>
        /// <param name="component">The component to delete.</param>
        /// <returns>The task that describes the deletion of the component from the manager.</returns>
        Task DeleteComponent(
            Data.Poco.Models.Mapping.Component.Component component
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
