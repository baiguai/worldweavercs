using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WorldWeaver.Tools
{
    public class AppSettingFunctions
    {
        public static string GetConfigValue(string section, string key)
        {
            var output = "";

            using (StreamReader r = new StreamReader($"Config/AppSettings.json"))
            {
                string json = r.ReadToEnd();
                var jsonObj = JObject.Parse(json);

                try
                {
                    output = (string)jsonObj[section][key];
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving AppSetting.  {ex.Message}");
                }
            }

            if (output == null)
            {
                output = "";
            }

            return output;
        }
    }
}