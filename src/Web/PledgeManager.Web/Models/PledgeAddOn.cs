using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web.Models {
    
    public class PledgeAddOn {

        [BsonElement("code", Order = 1)]
        public string Code { get; set; }

        [BsonElement("count", Order = 2)]
        [BsonDefaultValue(1)]
        [BsonIgnoreIfDefault]
        public int Count { get; set; } = 1;

        [BsonElement("variant")]
        [BsonDefaultValue(null)]
        [BsonIgnoreIfNull]
        public string Variant { get; set; } = null;

    }

}
