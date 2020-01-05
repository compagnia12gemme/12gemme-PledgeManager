using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web.Models {
    
    public class ShippingInfo {

        [BsonElement("address", Order = 1)]
        public string Address { get; set; }

        [BsonElement("zipCode", Order = 2)]
        public string ZipCode { get; set; }

        [BsonElement("city", Order = 3)]
        public string City { get; set; }

        [BsonElement("province", Order = 4)]
        public string Province { get; set; }

        [BsonElement("country", Order = 5)]
        public string Country { get; set; }

    }

}
