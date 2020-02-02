using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web.Models {
    
    public class CampaignReward {

        [BsonElement("code", Order = 1)]
        public string Code { get; set; }

        [BsonElement("title", Order = 2)]
        public string Title { get; set; }

        [BsonElement("pledgeBase", Order = 3)]
        public decimal PledgeBase { get; set; }

        [BsonElement("description", Order = 4)]
        public string Description { get; set; }

        [BsonElement("upgradePaths", Order = 5)]
        public List<string> UpgradePaths { get; set; } = new List<string>();

        [BsonIgnore]
        public IEnumerable<string> FullUpgradePaths {
            get {
                return (new string[] { Code }).Concat(UpgradePaths);
            }
        }

    }

}
