using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class JetConeDamageEvent : Event
    {
        public const string NAME = "Jet cone damage";
        public const string DESCRIPTION = "Triggered when passing through the jet cone from a white dwarf or neutron star has caused damage to a ship module";
        public const string SAMPLE = "";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static JetConeDamageEvent()
        {
            VARIABLES.Add("modulename", "the value of the boost");
            VARIABLES.Add("module", "the module that was damaged (this is a module object)");
        }

        public string modulename { get; private set; }

        public Module module { get; private set; }

        public JetConeDamageEvent(DateTime timestamp, string modulename, Module module) : base(timestamp, NAME)
        {
            this.modulename = modulename;
            this.module = module;
        }
    }
}
