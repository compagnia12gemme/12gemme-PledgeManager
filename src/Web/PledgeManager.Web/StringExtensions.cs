using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web {
    
    public static class StringExtensions {

        /// <summary>
        /// Checks whether a string and a set of other string are all not null and not empty.
        /// </summary>
        public static bool IsSetWith(this string s, params string[] other) {
            if(string.IsNullOrWhiteSpace(s)) {
                return false;
            }
            return other.Any(s => string.IsNullOrWhiteSpace(s));
        }

    }

}
