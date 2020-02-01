using PledgeManager.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PledgeManager.Web.ViewModels {

    public class PledgeShowViewModel {

        public Campaign Campaign { get; set; }

        public Pledge Pledge { get; set; }

        public CampaignReward CurrentReward { get; set; }

        public IEnumerable<(CampaignAddOn AddOn, string Variant)> AddedAddOns { get; set; }

        public IEnumerable<CampaignAddOn> AvailableAddOns { get; set; }

        public IEnumerable<(CampaignReward Reward, decimal UpgradeCost)> UpgradePaths { get; set; }

        public decimal FinalCost { get; set; }

        public decimal ToPay {
            get {
                return Math.Max(0M, FinalCost - Pledge.CurrentPledge);
            }
        }

        public bool CanBeClosed {
            get {
                return Pledge.CurrentPledge >= FinalCost;
            }
        }

        public ConfirmedPayment ConfirmedPayment { get; set; }

        public ErrorNotification Error { get; set; }

        private readonly char[] _randomCodeElements = new char[] {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
            'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'U',
            'V', 'W', 'X', 'Y', 'Z'
        };

        private const int RandomCodeLength = 8;

        public string GetRandomCode() {
            var rnd = new Random();
            var sb = new StringBuilder(RandomCodeLength);
            for(int i = 0; i < RandomCodeLength; ++i) {
                sb.Append(_randomCodeElements[rnd.Next(0, _randomCodeElements.Length)]);
            }
            return sb.ToString();
        }

    }

}
