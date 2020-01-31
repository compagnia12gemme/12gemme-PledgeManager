using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PledgeManager.Web {
    
    public class SlugifyParameterTransformer : IOutboundParameterTransformer {

        private readonly Regex _replace = new Regex("([a-z])([A-Z])",
            RegexOptions.Compiled);

        public string TransformOutbound(object value) {
            if (value == null) {
                return null;
            }

            return _replace.Replace(value.ToString(), "$1-$2").ToLowerInvariant();
        }

    }

}
