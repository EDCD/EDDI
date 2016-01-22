using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EliteDangerousCompanionAppService
{
    public class Compartment
    {
        public int Size { get; set; }
        public Module Module { get; set; }

        public static Compartment FromProfile(dynamic json)
        {
            Compartment Compartment = new Compartment();

            // Compartments have name of form "Slotnn_Sizenn"
            Match matches = Regex.Match((string)json.Name, @"Size([0-9]+)");
            if (matches.Success)
            {
                Compartment.Size = Int32.Parse(matches.Groups[1].Value);

                if (json.Value is JObject)
                {
                    JToken value;
                    if (json.Value.TryGetValue("module", out value))
                    {
                        Compartment.Module = Module.FromProfile((string)json.Name, json.Value);
                    }
                }
            }
            return Compartment;
        }
    }
}
