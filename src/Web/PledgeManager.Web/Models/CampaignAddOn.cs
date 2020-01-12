using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web.Models {
    
    public class CampaignAddOn {

        [BsonElement("code", Order = 1)]
        public string Code { get; set; }

        [BsonElement("title", Order = 2)]
        public string Title { get; set; }

        [BsonElement("cost", Order = 3)]
        public decimal Cost { get; set; }

        [BsonElement("description", Order = 4)]
        public string Description { get; set; }

        [BsonElement("multipleEnabled", Order = 5)]
        [BsonDefaultValue(false)]
        [BsonIgnoreIfDefault]
        public bool MultipleEnabled { get; set; } = false;

        [BsonElement("variants", Order = 6)]
        [BsonDefaultValue(null)]
        [BsonIgnoreIfNull]
        public string[] Variants { get; set; } = null;

        [BsonElement("excludes", Order = 7)]
        [BsonDefaultValue(null)]
        [BsonIgnoreIfNull]
        public string[] Excludes { get; set; } = null;

    }

}
