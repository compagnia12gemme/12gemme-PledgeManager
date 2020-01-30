using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web.Models {
    
    public class ShippingInfo {

        [BsonElement("givenName", Order = 1)]
        public string GivenName { get; set; }

        [BsonElement("surname", Order = 2)]
        public string Surname { get; set; }

        [BsonElement("address", Order = 3)]
        public string Address { get; set; }

        [BsonElement("addressSecondary", Order = 4)]
        [BsonIgnoreIfDefault]
        public string AddressSecondary { get; set; }

        [BsonElement("zipCode", Order = 5)]
        public string ZipCode { get; set; }

        [BsonElement("city", Order = 6)]
        public string City { get; set; }

        [BsonElement("province", Order = 7)]
        public string Province { get; set; }

        [BsonElement("country", Order = 8)]
        public string Country { get; set; }

        [BsonExtraElements]
        public BsonDocument CatchAll { get; set; }

    }

}
