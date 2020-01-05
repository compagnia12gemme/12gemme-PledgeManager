using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web.Models {
    
    public class Campaign {

        [BsonId]
        public string Id { get; set; }

        public string Code { get; set; }

    }

}
