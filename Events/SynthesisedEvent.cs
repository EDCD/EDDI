using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SynthesisedEvent ( DateTime timestamp, string synthesis, List<MaterialAmount> materials )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Synthesised";
        public const string DESCRIPTION = "Triggered when you synthesise something from materials";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-21T14:17:32Z\", \"event\":\"Synthesis\", \"Name\":\"Ammo Basic\", \"Materials\":{ \"sulphur\":2, \"phosphorus\":1 } }";

        [PublicAPI("The thing that has been synthesised")]
        public string synthesis { get; private set; } = synthesis;

        [PublicAPI("Materials used in the synthesis (as objects)")]
        public List<MaterialAmount> materials { get; private set; } = materials;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var synthesis = JsonParsing.getString(data, "Name");
            data.TryGetValue( "Materials", out var val );
            var materials = new List<MaterialAmount>();
            
            // 2.2 style
            if ( val is Dictionary<string, object> materialsData )
            {
                foreach ( var materialData in materialsData )
                {
                    var material = Material.FromEDName(materialData.Key);
                    materials.Add( new MaterialAmount( material, (int)(long)materialData.Value ) );
                }
            }
            else if ( val is List<object> materialsJson ) // 2.3 style
            {
                foreach ( var materialJson in materialsJson.Cast<IDictionary<string, object>>() )
                {
                    var material = Material.FromEDName(JsonParsing.getString(materialJson, "Name"));
                    materials.Add( new MaterialAmount( material, (int)(long)materialJson[ "Count" ] ) );
                }
            }

            events.Add( new SynthesisedEvent( timestamp, synthesis, materials ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
