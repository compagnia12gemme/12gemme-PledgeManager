using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web.Models {

    public class PledgePayment {

        [BsonElement("orderId")]
        [BsonIgnoreIfNull]
        public string OrderId { get; set; }

        [BsonElement("payerId")]
        [BsonIgnoreIfNull]
        public string PayerId { get; set; }

        [BsonElement("payerEmail")]
        [BsonIgnoreIfNull]
        public string PayerEmail { get; set; }

        [BsonElement("value")]
        public decimal Value { get; set; }

        [BsonElement("timestamp")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Timestamp { get; set; }

        [BsonExtraElements]
        public BsonDocument CatchAll { get; set; }

    }

}
