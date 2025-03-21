﻿using System;
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
            var typesOutput = new List<string>();

            using (StreamReader r = new StreamReader($"Config/ElementProcRules/ElementByType.json"))
            {
                string json = r.ReadToEnd();
                var jsonObj = JObject.Parse(json);
                foreach (var tp in jsonObj["elements"].Children())
                {
                    foreach (var types in tp["types"].Values())
                    {
                        typesOutput = (List<string>)tp["elements"].Values();
                    }
                }
            }

            return typesOutput;
        }

        public static List<string> GetElementTypes()
        {
            var elementTypes = new List<string>();

            using (StreamReader r = new StreamReader($"Config/ElementProcRules/ElementByType.json"))
            {
                string json = r.ReadToEnd();
                var jsonObj = JObject.Parse(json);
                foreach (var tp in jsonObj["elements"].Children())
                {
                    var tps = JArray.Parse(tp["types"].ToString());
                    List<string> types = tps.ToObject<List<string>>();
                    foreach (var type in types)
                    {
                        if (!elementTypes.Contains(type))
                        {
                            elementTypes.Add(type);
                        }
                    }
                }
            }

            return elementTypes;
        }

        public static List<Classes.ElementProc> GetProcessStepsByType(string type)
        {
            var output = new List<Classes.ElementProc>();
            if (type.Equals("attribute") || type.Equals("attrib"))
            {
                return output;
            }

            if (type.Equals("enter_message"))
            {
                type = "message";
            }

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
                        var proc = new Classes.ElementProc();
                        proc.CurrentElementTypes.Add(type);
                        var els = JArray.Parse(tp["elements"].ToString());
                        var rpt = (tp["repeat_options"].ToString() == "true") ? true : false;
                        proc.ChildProcElements = els.ToObject<List<string>>();
                        proc.AllowRepeatOptions = rpt;

                        output.Add(proc);
                        break;
                    }
                }
            }

            if (output.Count == 0)
            {
                output = GetProcessStepsByType("general");
            }

            return output;
        }
    }
}
