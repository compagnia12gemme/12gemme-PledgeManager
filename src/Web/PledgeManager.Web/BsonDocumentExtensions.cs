using MongoDB.Bson;
using PledgeManager.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web {
    
    public static class BsonDocumentExtensions {

        public static string ReadSurveyValue(this BsonDocument d, SurveyElementBase survey) {
            string defValue = survey switch {
                SurveyElementCheckbox scb => scb.DefaultToChecked ? "1" : "0",
                _ => string.Empty,
            };

            if (d == null) {
                return defValue;
            }
            if(!d.TryGetValue(survey.Name, out var v)) {
                return defValue;
            }

            return survey switch {
                SurveyElementCheckbox _ => v.AsBoolean ? "1" : "0",
                _ => v.AsString
            };
        }

        public static string ReadSurveyValueToHuman(this BsonDocument d, SurveyElementBase survey) {
            string defValue = survey switch
            {
                SurveyElementCheckbox scb => scb.DefaultToChecked ? "Sì" : "No",
                _ => string.Empty,
            };

            if (d == null) {
                return defValue;
            }
            if (!d.TryGetValue(survey.Name, out var v)) {
                return defValue;
            }

            return survey switch
            {
                SurveyElementCheckbox _ => v.AsBoolean ? "Sì" : "No",
                _ => v.AsString
            };
        }

    }

}
