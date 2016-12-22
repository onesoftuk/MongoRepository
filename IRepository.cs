using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Abacus.Data.MongoRepository
{
    /// <summary>
    /// IRepository definition.
    /// </summary>
    /// <typeparam name="T">The type contained in the repository.</typeparam>
    public interface IRepository<T> : IQueryable<T>
        where T : IEntity
    {
        /// <summary>
        /// Gets the Mongo collection (to perform advanced operations).
        /// </summary>
        /// <remarks>
        /// One can argue that exposing this property (and with that, access to it's Database property for instance
        /// (which is a "parent")) is not the responsibility of this class. Use of this property is highly discouraged;
        /// for most purposes you can use the MongoRepositoryManager&lt;T&gt;
        /// </remarks>
        /// <value>The Mongo collection (to perform advanced operations).</value>
        IMongoCollection<T> Collection { get; }

        /// <summary>
        /// Returns the T by its given id.
        /// </summary>
        /// <param name="id">The value representing the ObjectId of the entity to retrieve.</param>
        /// <returns>The Entity T.</returns>
        Task<T> GetById(string id);

        /// <summary>
        /// Returns the T by its given ObjectId.
        /// </summary>
        /// <param name="id">The ObjectId of the entity to retrieve.</param>
        /// <returns>The Entity T</returns>
        Task<T> GetById(ObjectId id);


        /// <summary>
        /// Returns IEnumerable&lt;T&gt;
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>IEnumerable&lt;T&gt;</returns>
        Task<IEnumerable<T>> GetAll(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Adds the new entity in the repository.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>The added entity including its new ObjectId.</returns>
        Task<T> Insert(T entity);

        /// <summary>
        /// Adds the new entities in the repository.
        /// </summary>
        /// <param name="entities">The entities of type T.</param>
        Task Insert(IEnumerable<T> entities);

        /// <summary>
        /// Upserts an entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The updated entity.</returns>
        Task<T> Update(T entity);

        /// <summary>
        /// Upserts the entities.
        /// </summary>
        /// <param name="entities">The entities to update.</param>
        Task Update(IEnumerable<T> entities);

        /// <summary>
        /// Deletes an entity from the repository by its id.
        /// </summary>
        /// <param name="id">The entity's id.</param>
        Task Delete(string id);

        /// <summary>
        /// Deletes the given entity.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        Task Delete(T entity);

        /// <summary>
        /// Deletes the entities matching the predicate.
        /// </summary>
        /// <param name="predicate">The expression.</param>
        Task Delete(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Deletes all entities in the repository.
        /// </summary>
        Task DeleteAll();

        /// <summary>
        /// Counts the total entities in the repository.
        /// </summary>
        /// <returns>Count of entities in the repository.</returns>
        Task<long> Count();

        /// <summary>
        /// Checks if the entity exists for given predicate.
        /// </summary>
        /// <param name="predicate">The expression.</param>
        /// <returns>True when an entity matching the predicate exists, false otherwise.</returns>
        Task<bool> Exists(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Drops specified index on the repository.
        /// </summary>
        /// <param name="keyname">The name of the indexed field.</param>
        Task DropIndex(string keyname);

        /// <summary>
        /// Drops specified indexes on the repository.
        /// </summary>
        /// <param name="keynames">The names of the indexed fields.</param>
        Task DropIndexes(IEnumerable<string> keynames);

        /// <summary>
        /// Drops all indexes on this repository.
        /// </summary>
        Task DropAllIndexes();

        /// <summary>
        /// Creates a new index for the specified keyname.
        /// </summary>
        /// <param name="keyname">The name of the index field</param>
        /// <param name="descending">Set to true for descending, default is false for ascending.</param>
        Task CreateIndex(string keyname, bool descending = false);
    }

    ///// <summary>
    ///// IRepository definition.
    ///// </summary>
    ///// <typeparam name="T">The type contained in the repository.</typeparam>
    ///// <remarks>Entities are assumed to use strings for Id's.</remarks>
    //public interface IRepository<T> : IQueryable<T>, IRepository<T, string>
    //    where T : IEntity<string> { }
}