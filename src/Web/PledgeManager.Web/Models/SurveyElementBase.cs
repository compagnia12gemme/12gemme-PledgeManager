using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web.Models {

    [BsonKnownTypes(
        typeof(SurveyElementCheckbox),
        typeof(SurveyElementEmailAddress),
        typeof(SurveyElementShortText)
    )]
    public class SurveyElementBase {

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("optional")]
        [BsonDefaultValue(false)]
        [BsonIgnoreIfDefault]
        public bool IsOptional { get; set; } = false;

        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("description")]
        [BsonIgnoreIfNull]
        public string Description { get; set; }

        [BsonExtraElements]
        public BsonDocument CatchAll { get; set; }

    }

}
