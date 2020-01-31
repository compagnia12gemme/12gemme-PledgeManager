using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web.Models {

    [BsonDiscriminator("checkbox")]
    public class SurveyElementCheckbox : SurveyElementBase {

        [BsonElement("defaultCheck")]
        [BsonDefaultValue(false)]
        [BsonIgnoreIfDefault]
        public bool DefaultToChecked { get; set; } = false;

    }

}
