using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web.Models {
    
    public class Campaign {

        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("code")]
        public string Code { get; set; }

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; }

        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("campaignLink")]
        public string CampaignLink { get; set; }

        [BsonElement("terminatedOn")]
        [BsonDateTimeOptions(DateOnly = true, Kind = DateTimeKind.Unspecified)]
        public DateTime TerminatedOn { get; set; }

        [BsonElement("coverUrl")]
        [BsonIgnoreIfDefault]
        public string CoverUrl { get; set; }

        [BsonElement("supportMail")]
        public string SupportEmailAddress { get; set; }

        [BsonElement("mailSignature")]
        public string MailSignature { get; set; }

        [BsonElement("rewards")]
        public List<CampaignReward> Rewards { get; set; } = new List<CampaignReward>();

        [BsonElement("addons")]
        public List<CampaignAddOn> AddOns { get; set; } = new List<CampaignAddOn>();

        [BsonElement("survey")]
        [BsonIgnoreIfDefault]
        public List<SurveyElementBase> Survey { get; set; } = new List<SurveyElementBase>();

        [BsonExtraElements]
        public BsonDocument CatchAll { get; set; }

    }

}
