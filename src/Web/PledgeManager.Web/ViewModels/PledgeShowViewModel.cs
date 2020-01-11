using PledgeManager.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web.ViewModels {

    public class PledgeShowViewModel {

        public Campaign Campaign { get; set; }

        public Pledge Pledge { get; set; }

        public CampaignReward CurrentReward { get; set; }

        public IEnumerable<(CampaignAddOn AddOn, int Count, string Variant)> AddOns { get; set; }

        public IEnumerable<(CampaignReward Reward, decimal UpgradeCost)> UpgradePaths { get; set; }

        public decimal FinalCost;

        public bool CanBeClosed;

    }

}
