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
        public const string DESCRIPTION = "Triggered in normal space when passing through the jet cone from a white dwarf or neutron star causes damage to a ship module";
        public const string SAMPLE = "{ \"timestamp\":\"2018-03-15T04:20:35Z\", \"event\":\"JetConeDamage\", \"Module\":\"$int_hyperdrive_size2_class5_name;\", \"Module_Localised\":\"$Int_Hyperdrive_Size2_Class1_Name;\" }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static JetConeDamageEvent()
        {
            VARIABLES.Add("modulename", "the name of the module that was damaged");
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
