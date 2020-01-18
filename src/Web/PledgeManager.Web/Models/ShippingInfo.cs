using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web.Models {
    
    public class ShippingInfo {

        [BsonElement("name", Order = 1)]
        public string Name { get; set; }

        [BsonElement("address", Order = 2)]
        public string Address { get; set; }

        [BsonElement("addressSecundary", Order = 3)]
        public string AddressSecundary { get; set; }

        [BsonElement("zipCode", Order = 4)]
        public string ZipCode { get; set; }

        [BsonElement("city", Order = 5)]
        public string City { get; set; }

        [BsonElement("province", Order = 6)]
        public string Province { get; set; }

        [BsonElement("country", Order = 7)]
        public string Country { get; set; }

    }

}
