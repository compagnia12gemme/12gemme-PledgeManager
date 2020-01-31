using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web.Models {

    [BsonDiscriminator("email")]
    public class SurveyElementEmailAddress : SurveyElementBase {

        [BsonElement("prefillWithEmail")]
        [BsonDefaultValue(false)]
        [BsonIgnoreIfDefault]
        public bool PrefillWithEmail { get; set; } = false;

    }

}
