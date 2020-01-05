using PledgeManager.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web.ViewModels {

    public class PledgeShowViewModel {

        public Campaign Campaign { get; set; }

        public Pledge Pledge { get; set; }

    }

}
