using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PledgeManager.Web {
    
    public static class ControllerExtensions {

        public static void AddToTemp<T>(this Controller ctrl, string key, T value) {
            ctrl.TempData[key] = JsonSerializer.Serialize<T>(value);
        }

        /// <summary>
        /// Gets a serialized object from TempData, is present. Null otherwise.
        /// </summary>
        public static T FromTemp<T>(this Controller ctrl, string key) {
            if(ctrl.TempData.ContainsKey(key)) {
                try {
                    return JsonSerializer.Deserialize<T>(ctrl.TempData[key].ToString());
                }
                catch(Exception) {
                    return default;
                }
            }
            else {
                return default;
            }
        }

    }

}
