using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web.Models {
    
    public class Pledge {

        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("campaignId")]
        public string CampaignId { get; set; }

        [BsonElement("userId")]
        public int UserId { get; set; }

        [BsonElement("userToken")]
        public string UserToken { get; set; }

        [BsonElement("shipping")]
        public ShippingInfo Shipping { get; set; }

        [BsonElement("originalPledge")]
        public decimal OriginalPledge { get; set; }

        private decimal? _currentPledge = null;

        [BsonElement("currentPledge")]
        public decimal CurrentPledge {
            get {
                return _currentPledge ?? OriginalPledge;
            }
            set {
                _currentPledge = value;
            }
        }

        [BsonElement("originalRewardLevel")]
        public string OriginalRewardLevel { get; set; }

        private string _currentRewardLevel = null;

        [BsonElement("currentRewardLevel")]
        public string CurrentRewardLevel {
            get {
                return _currentRewardLevel ?? OriginalRewardLevel;
            }
            set {
                _currentRewardLevel = value;
            }
        }

        [BsonElement("addons")]
        public List<PledgeAddOn> AddOns { get; set; } = new List<PledgeAddOn>();

        [BsonElement("lastUpdate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime LastUpdate { get; set; }

        [BsonElement("updates")]
        public List<PledgeUpdate> Updates { get; set; } = new List<PledgeUpdate>();

        [BsonElement("payments")]
        [BsonIgnoreIfDefault]
        public List<PledgePayment> Payments { get; set; } = new List<PledgePayment>();

        [BsonElement("note")]
        [BsonIgnoreIfDefault]
        public string Note { get; set; }

        [BsonElement("closed")]
        [BsonDefaultValue(false)]
        [BsonIgnoreIfDefault]
        public bool IsClosed { get; set; } = false;

        [BsonElement("acceptNewsletter")]
        [BsonDefaultValue(false)]
        [BsonIgnoreIfDefault]
        public bool AcceptNewsletter { get; set; } = false;

        [BsonExtraElements]
        public BsonDocument CatchAll { get; set; }

    }

}
