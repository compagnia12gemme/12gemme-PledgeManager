using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web {
    
    public class KickstarterBackerInfo {

        [Name("Backer Number")]
        public int BackerNumber { get; set; }

        [Name("Backer UID")]
        public string BackerUID { get; set; }

        [Name("Backer Name")]
        public string BackerName { get; set; }

        [Name("Email")]
        public string Email { get; set; }

        [Name("Shipping Country")]
        public string ShippingCountry { get; set; }

        [Name("Shipping Amount")]
        public string ShippingAmount { get; set; }

        [Name("Pledge Amount")]
        public string PledgeAmount { get; set; }

        [Name("Reward ID")]
        [Default(0)]
        public int RewardID { get; set; }

    }

}
