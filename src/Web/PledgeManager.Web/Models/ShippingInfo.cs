using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web.Models {
    
    public class ShippingInfo {

        [BsonElement("givenName", Order = 1)]
        [BsonIgnoreIfNull]
        public string GivenName { get; set; }

        [BsonElement("surname", Order = 2)]
        [BsonIgnoreIfNull]
        public string Surname { get; set; }

        [BsonElement("address", Order = 3)]
        [BsonIgnoreIfNull]
        public string Address { get; set; }

        [BsonElement("addressSecondary", Order = 4)]
        [BsonIgnoreIfNull]
        public string AddressSecondary { get; set; }

        [BsonElement("zipCode", Order = 5)]
        [BsonIgnoreIfNull]
        public string ZipCode { get; set; }

        [BsonElement("city", Order = 6)]
        [BsonIgnoreIfNull]
        public string City { get; set; }

        [BsonElement("province", Order = 7)]
        [BsonIgnoreIfNull]
        public string Province { get; set; }

        [BsonElement("country", Order = 8)]
        [BsonIgnoreIfNull]
        public string Country { get; set; }

        [BsonExtraElements]
        public BsonDocument CatchAll { get; set; }

    }

}
