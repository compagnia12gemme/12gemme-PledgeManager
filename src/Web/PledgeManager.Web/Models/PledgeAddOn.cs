using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web.Models {
    
    public class PledgeAddOn {

        [BsonElement("code", Order = 1)]
        public string Code { get; set; }

        [BsonElement("variant")]
        [BsonDefaultValue(null)]
        [BsonIgnoreIfNull]
        public string Variant { get; set; } = null;

    }

}
