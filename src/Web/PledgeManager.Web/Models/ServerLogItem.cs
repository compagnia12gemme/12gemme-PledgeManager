using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web.Models {
    
    public class ServerLogItem {

        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("eventId", Order = 2)]
        [BsonIgnoreIfDefault]
        public int EventId { get; set; }

        [BsonElement("level", Order = 3)]
        [BsonRepresentation(BsonType.String)]
        public Microsoft.Extensions.Logging.LogLevel Level { get; set; }

        [BsonElement("category", Order = 4)]
        [BsonIgnoreIfDefault]
        public string Category { get; set; }

        [BsonElement("message", Order = 5)]
        public string Message { get; set; }

        [BsonElement("timestamp", Order = 10)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Timestamp { get; set; }

    }

}
