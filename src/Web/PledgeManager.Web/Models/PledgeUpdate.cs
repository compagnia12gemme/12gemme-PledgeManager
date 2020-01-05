using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web.Models {
    
    public class PledgeUpdate {

        [BsonElement("updateOn")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedOn { get; set; }

        [BsonElement("pledgeAmount")]
        public decimal PledgeAmount { get; set; }

    }

}
