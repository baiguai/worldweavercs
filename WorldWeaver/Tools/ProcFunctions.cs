using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace WorldWeaver.Tools
{
    public class ProcFunctions
    {
        public static List<string> GetTypes()
        {
            var output = new List<string>();

            using (StreamReader r = new StreamReader($"Config/ElementProcRules/ElementByType.json"))
            {
                string json = r.ReadToEnd();
                var jsonObj = JObject.Parse(json);
                foreach (var tp in jsonObj["elements"].Children())
                {
                    foreach (var types in tp["types"].Values())
                    {
                    output = (List<string>)tp["elements"].Values();
                    }
                }
            }

            return output;
        }

        public static List<string> GetProcessStepsByType(string type)
        {
            var output = new List<string>();

            using (StreamReader r = new StreamReader($"Config/ElementProcRules/ElementByType.json"))
            {
                string json = r.ReadToEnd();
                var jsonObj = JObject.Parse(json);
                foreach (var tp in jsonObj["elements"].Children())
                {
                    var tps = JArray.Parse(tp["types"].ToString());
                    List<string> types = tps.ToObject<List<string>>();

                    if (types.Contains(type))
                    {
                        var els = JArray.Parse(tp["elements"].ToString());
                        output = els.ToObject<List<string>>();
                        break;
                    }
                }
            }

            return output;
        }
    }
}
