using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Abacus.Data.MongoRepository
{
    public abstract class Repository<T> : IRepository<T> where T : IEntity
    {
        /// <summary>
        /// MongoCollection field.
        /// </summary>
        protected internal IMongoCollection<T> _collection;

        /// <summary>
        /// Initializes a new instance of the MongoRepository class.
        /// Uses the Default App/Web.Config connectionstrings to fetch the connectionString and Database name.
        /// </summary>
        /// <remarks>Default constructor defaults to "MongoServerSettings" key for connectionstring.</remarks>
        protected Repository()
            : this(Util<string>.GetDefaultConnectionString())
        {
        }

        /// <summary>
        /// Initializes a new instance of the MongoRepository class.
        /// </summary>
        /// <param name="connectionString">Connectionstring to use for connecting to MongoDB.</param>
        public Repository(string connectionString)
        {
            _collection = Util<string>.GetCollectionFromConnectionString<T>(connectionString);
        }

        /// <summary>
        /// Initializes a new instance of the MongoRepository class.
        /// </summary>
        /// <param name="connectionString">Connectionstring to use for connecting to MongoDB.</param>
        /// <param name="collectionName">The name of the collection to use.</param>
        public Repository(string connectionString, string collectionName)
        {
            _collection = Util<string>.GetCollectionFromConnectionString<T>(connectionString, collectionName);
        }

        /// <summary>
        /// Initializes a new instance of the MongoRepository class.
        /// </summary>
        /// <param name="url">Url to use for connecting to MongoDB.</param>
        public Repository(MongoUrl url)
        {
            _collection = Util<string>.GetCollectionFromUrl<T>(url);
        }

        /// <summary>
        /// Initializes a new instance of the MongoRepository class.
        /// </summary>
        /// <param name="url">Url to use for connecting to MongoDB.</param>
        /// <param name="collectionName">The name of the collection to use.</param>
        public Repository(MongoUrl url, string collectionName)
        {
            _collection = Util<string>.GetCollectionFromUrl<T>(url, collectionName);
        }

        /// <summary>
        /// Gets the Mongo collection (to perform advanced operations).
        /// </summary>
        /// <remarks>
        /// One can argue that exposing this property (and with that, access to it's Database property for instance
        /// (which is a "parent")) is not the responsibility of this class. Use of this property is highly discouraged;
        /// for most purposes you can use the MongoRepositoryManager&lt;T&gt;
        /// </remarks>
        /// <value>The Mongo collection (to perform advanced operations).</value>
        public IMongoCollection<T> Collection
        {
            get { return _collection; }
        }

        /// <summary>
        /// Gets the name of the collection
        /// </summary>
        public string CollectionName //todo?? test
        {
            get { return _collection.OfType<T>().CollectionNamespace.CollectionName; }
        }

        /// <summary>
        /// Returns the T by its given id.
        /// </summary>
        /// <param name="id">The Id of the entity to retrieve.</param>
        /// <returns>The Entity T.</returns>
        public async Task<T> GetById(string id)
        {
            if (typeof(T).IsSubclassOf(typeof(Entity)))
            {
                return await GetById(new ObjectId(id as string));
            }

            var filter = Builders<T>.Filter.Eq(s => s.Id, id);

            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        ///// <summary>
        ///// Returns the T by its given id.
        ///// </summary>
        ///// <param name="id">The Id of the entity to retrieve.</param>
        ///// <returns>The Entity T.</returns>
        public virtual async Task<T> GetById(ObjectId id)
        {
            var filter = Builders<T>.Filter.Eq(s => new ObjectId(s.Id), id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets all entities from the collection.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>IEnumerable&lt;T&gt;</returns>
        public virtual async Task<IEnumerable<T>> GetAll(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _collection.Find(e => true).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Adds the new entity in the repository.
        /// </summary>
        /// <param name="entity">The entity T.</param>
        /// <returns>The added entity including its new ObjectId.</returns>
        public virtual async Task<T> Insert(T entity)
        {
            entity.CreatedOn = DateTime.Now;
            entity.UpdatedOn = DateTime.Now;

            await _collection.InsertOneAsync(entity);

            return entity;
        }

        /// <summary>
        /// Adds the new entities in the repository.
        /// </summary>
        /// <param name="entities">The entities of type T.</param>
        public virtual async Task Insert(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                entity.CreatedOn = DateTime.Now;
                entity.UpdatedOn = DateTime.Now;
            }
            await _collection.InsertManyAsync(entities);
        }

        /// <summary>
        /// Updates an entity and inserts if it does not exist.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The updated entity.</returns>
        public async Task<T> Update(T entity)
        {
            var filter = Builders<T>.Filter.Eq(s => s.Id, entity.Id);

            var upsert = string.IsNullOrEmpty(entity.Id);

            if (upsert)
            {
                await Insert(entity);

                return entity;
                //entity.CreatedOn = DateTime.Now;
                //entity.Id = ObjectId.GenerateNewId().ToString();
            }

            entity.UpdatedOn = DateTime.Now;

            await _collection.ReplaceOneAsync(filter, entity, new UpdateOptions { IsUpsert = false });

            return entity;
        }

        /// <summary>
        /// Updates all entities and inserts if it does not exist.
        /// </summary>
        /// <param name="entities">The entity.</param>
        public async Task Update(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                await Update(entity);
            }
        }

        /// <summary>
        /// Deletes an entity from the repository by its id.
        /// </summary>
        /// <param name="id">The entity's id.</param>
        public async Task Delete(string id)
        {
            if (typeof(T).IsSubclassOf(typeof(Entity)))
            {
                var filter = Builders<T>.Filter.Eq(s => s.Id, id);
                await _collection.DeleteOneAsync(filter);
            }
        }

        /// <summary>
        /// Deletes an entity from the repository by its ObjectId.
        /// </summary>
        /// <param name="id">The ObjectId of the entity.</param>
        public virtual async Task Delete(ObjectId id)
        {
            var filter = Builders<T>.Filter.Eq(s => new ObjectId(s.Id), id);
            await _collection.DeleteOneAsync(filter);
        }

        /// <summary>
        /// Deletes the given entity.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        public async Task Delete(T entity)
        {
            await Delete(entity.Id);
        }

        /// <summary>
        /// Deletes the entities matching the predicate.
        /// </summary>
        /// <param name="predicate">The expression.</param>
        public async Task Delete(Expression<Func<T, bool>> predicate)
        {
            foreach (T entity in _collection.AsQueryable<T>().Where(predicate))
            {
                await Delete(entity.Id);
            }
        }

        /// <summary>
        /// Deletes all entities in the repository.
        /// </summary>
        public async Task DeleteAll()
        {
            await _collection.DeleteManyAsync(new BsonDocument());
        }

        /// <summary>
        /// Counts the total entities in the repository.
        /// </summary>
        /// <returns>Count of entities in the collection.</returns>
        public async Task<long> Count()
        {
            return await _collection.CountAsync(new BsonDocument());
        }

        /// <summary>
        /// Checks if the entity exists for given predicate.
        /// </summary>
        /// <param name="predicate">The expression.</param>
        /// <returns>True when an entity matching the predicate exists, false otherwise.</returns>
        public virtual async Task<bool> Exists(Expression<Func<T, bool>> predicate)
        {
            return await Task.Run(()=> _collection.AsQueryable<T>().Any(predicate));
        }

        /// <summary>
        /// Paginations the entites.
        /// </summary>
        /// <param name="top">The top.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="ascending">if set to <c>true</c> [ascending].</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<T>> Pagination(int top, int skip, Func<T, object> orderBy, bool ascending = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = this._collection.Find(e => true).Skip(skip).Limit(top);

            if (ascending)
                return await query.SortBy(e => e.Id).ToListAsync(cancellationToken);
            else
                return await query.SortByDescending(e => e.Id).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An IEnumerator&lt;T&gt; object that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _collection.AsQueryable<T>().GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An IEnumerator object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _collection.AsQueryable<T>().GetEnumerator();
        }

        /// <summary>
        /// Gets the type of the element(s) that are returned when the expression tree associated with this instance of IQueryable is executed.
        /// </summary>
        public Type ElementType
        {
            get { return _collection.AsQueryable<T>().ElementType; }
        }

        /// <summary>
        /// Gets the expression tree that is associated with the instance of IQueryable.
        /// </summary>
        public Expression Expression
        {
            get { return _collection.AsQueryable<T>().Expression; }
        }

        /// <summary>
        /// Gets the query provider that is associated with this data source.
        /// </summary>
        public IQueryProvider Provider
        {
            get { return _collection.AsQueryable<T>().Provider; }
        }

        /// <summary>
        /// Drops all indexes on this repository.
        /// </summary>
        public virtual async Task DropAllIndexes()
        {
            await _collection.Indexes.DropAllAsync();
        }

        /// <summary>
        /// Drops specified index on the repository.
        /// </summary>
        /// <param name="keyname">The name of the indexed field.</param>
        public virtual async Task DropIndex(string keyname)
        {
            await _collection.Indexes.DropOneAsync(keyname);
        }

        /// <summary>
        /// Drops specified indexes on the repository.
        /// </summary>
        /// <param name="keynames">The names of the indexed fields.</param>
        public virtual async Task DropIndexes(IEnumerable<string> keynames)
        {
            foreach (var keyname in keynames)
            {
                await _collection.Indexes.DropOneAsync(keyname);
            }
        }

        /// <summary>
        /// Creates a new index for the specified keyname.
        /// </summary>
        /// <param name="keyname">The name of the index field</param>
        /// <param name="descending">Set to true for descending, default is false for ascending.</param>
        public virtual async Task CreateIndex(string keyname, bool descending = false)
        {
            if (!descending)
            {
                var key = Builders<T>.IndexKeys.Ascending(keyname);
                await _collection.Indexes.CreateOneAsync(key);
            }
            else
            {
                var key = Builders<T>.IndexKeys.Descending(keyname);
                await _collection.Indexes.CreateOneAsync(key);
            }
        }
    }

    ///// <summary>
    ///// Deals with entities in MongoDb.
    ///// </summary>
    ///// <typeparam name="T">The type contained in the repository.</typeparam>
    ///// <remarks>Entities are assumed to use strings for Id's.</remarks>
    //public class Repository<T> : Repository<T, string>, IRepository<T>
    //    where T : IEntity<string>
    //{
    //    /// <summary>
    //    /// Initializes a new instance of the MongoRepository class.
    //    /// Uses the Default App/Web.Config connectionstrings to fetch the connectionString and Database name.
    //    /// </summary>
    //    /// <remarks>Default constructor defaults to "MongoServerSettings" key for connectionstring.</remarks>
    //    public Repository()
    //        : base() { }

    //    /// <summary>
    //    /// Initializes a new instance of the MongoRepository class.
    //    /// </summary>
    //    /// <param name="url">Url to use for connecting to MongoDB.</param>
    //    public Repository(MongoUrl url)
    //        : base(url) { }

    //    /// <summary>
    //    /// Initializes a new instance of the MongoRepository class.
    //    /// </summary>
    //    /// <param name="url">Url to use for connecting to MongoDB.</param>
    //    /// <param name="collectionName">The name of the collection to use.</param>
    //    public Repository(MongoUrl url, string collectionName)
    //        : base(url, collectionName) { }

    //    /// <summary>
    //    /// Initializes a new instance of the MongoRepository class.
    //    /// </summary>
    //    /// <param name="connectionString">Connectionstring to use for connecting to MongoDB.</param>
    //    public Repository(string connectionString)
    //        : base(connectionString) { }

    //    /// <summary>
    //    /// Initializes a new instance of the MongoRepository class.
    //    /// </summary>
    //    /// <param name="connectionString">Connectionstring to use for connecting to MongoDB.</param>
    //    /// <param name="collectionName">The name of the collection to use.</param>
    //    public Repository(string connectionString, string collectionName)
    //        : base(connectionString, collectionName) { }
    //}
}