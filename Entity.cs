using System;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Abacus.Data.MongoRepository
{
    /// <summary>
    /// Abstract Entity for all the BusinessEntities.
    /// </summary>
    [DataContract]
    [Serializable]
    [BsonIgnoreExtraElements(Inherited = true)]
    public abstract class Entity : IEntity
    {
        /// <summary>
        /// Gets or sets the id for this object (the primary record for an entity).
        /// </summary>
        /// <value>The id for this object (the primary record for an entity).</value>
        [DataMember]
        [BsonRepresentation(BsonType.ObjectId)]
        public virtual string Id { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the Entity
        /// </summary>
        [DataMember]
        [BsonRepresentation(BsonType.DateTime)]
        public virtual DateTime CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets the last updated date of the Entity
        /// </summary>
        [DataMember]
        [BsonRepresentation(BsonType.DateTime)]
        public virtual DateTime UpdatedOn { get; set; }
    }

    /// <summary>
    /// Generic Entity interface
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Gets or sets the Id of the Entity.
        /// </summary>
        /// <value>Id of the Entity.</value>
        [BsonId]
        string Id { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the Entity
        /// </summary>
        [BsonRepresentation(BsonType.DateTime)]
        DateTime CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets the last updated date of the Entity
        /// </summary>
        [BsonRepresentation(BsonType.DateTime)]
        DateTime UpdatedOn { get; set; }
    }
}
