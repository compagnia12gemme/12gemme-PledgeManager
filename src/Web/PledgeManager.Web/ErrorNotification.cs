using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web {
    
    public class ErrorNotification {

        public string Message { get; set; }

        public HashSet<string> ErrorKeys { get; set; } = new HashSet<string>();

        public void AddErrorKey(string key) {
            ErrorKeys.Add(key);
        }

        public bool HasError(string key) {
            return ErrorKeys.Contains(key);
        }

    }

}
