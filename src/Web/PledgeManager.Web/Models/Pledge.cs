﻿using MongoDB.Bson;
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

        [BsonElement("currentPledge")]
        public decimal? CurrentPledge { get; set; }

        [BsonElement("originalRewardLevel")]
        public string OriginalRewardLevel { get; set; }

        [BsonElement("currentRewardLevel")]
        public string CurrentRewardLevel { get; set; }

        [BsonElement("lastUpdate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime LastUpdate { get; set; }

        [BsonElement("updates")]
        public List<PledgeUpdate> Updates { get; set; } = new List<PledgeUpdate>();

        [BsonExtraElements]
        public BsonDocument CatchAll { get; set; }

    }

}
