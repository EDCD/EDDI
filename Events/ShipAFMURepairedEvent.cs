using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipAfmuRepairedEvent ( DateTime timestamp, string item, bool repairedfully, decimal health )
        : Event( timestamp, NAME )
    {
        public const string NAME = "AFMU repairs";
        public const string DESCRIPTION = "Triggered when repairing modules using the Auto Field Maintenance Unit (AFMU)";
        public static readonly string[] SAMPLES = 
        { 
            "{ \"timestamp\":\"2017-08-14T15:41:50Z\", \"event\":\"AfmuRepairs\", \"Module\":\"$modularcargobaydoor_name;\", \"Module_Localised\":\"Cargo Hatch\", \"FullyRepaired\":true, \"Health\":1.000000 }" ,
            "{ \"timestamp\":\"2026-05-16T20:09:46Z\", \"event\":\"AfmuRepairs\", \"Module\":\"$modularcargobaydoorfdl_name;\", \"Module_Localised\":\"Cargo Hatch\", \"FullyRepaired\":true, \"Health\":1.000000 }"
        };

        [PublicAPI("The module that was repaired")]
        public string item { get; private set; } = item;

        [PublicAPI("Whether the module was fully repaired (true/false)")]
        public bool repairedfully { get; private set; } = repairedfully;

        [PublicAPI("The health of the module (1.000000 = fully repaired)")]
        public decimal health { get; private set; } = health;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var item = JsonParsing.getString(data, "Module");
            // Item might be a module
            var module = Module.FromEDName(item);
            if ( module != null )
            {
                if ( module.Mount != null )
                {
                    // This is a weapon so provide a bit more information
                    string mount;
                    if ( module.Mount == ModuleMount.Fixed )
                    {
                        mount = "fixed";
                    }
                    else if ( module.Mount == ModuleMount.Gimballed )
                    {
                        mount = "gimballed";
                    }
                    else
                    {
                        mount = "turreted";
                    }
                    item = "" + module.@class.ToString() + module.grade + " " + mount + " " + module.localizedName;
                }
                else
                {
                    item = module.localizedName;
                }
            }

            // There is an FDev bug that can set `FullyRepaired` to false even when the module health is full,
            // so we work around this by relying on the `Health` property rather than the `FullyRepaired` property.
            // This appears to be a unique problem with Module Reinforcement Packages.

            var health = JsonParsing.getDecimal(data, "Health");
            var repairedfully = health == 1M;

            events.Add( new ShipAfmuRepairedEvent( timestamp, item, repairedfully, health ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
