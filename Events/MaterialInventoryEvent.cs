using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class MaterialInventoryEvent : Event
    {
        public const string NAME = "Material inventory";
        public const string DESCRIPTION = "Triggered when you obtain an inventory of your current materials";
        public const string SAMPLE = @"{ ""timestamp"":""2017-02-10T14:25:51Z"", ""event"":""Materials"", ""Raw"":[ { ""Name"":""chromium"", ""Count"":28 }, { ""Name"":""zinc"", ""Count"":18 }, { ""Name"":""iron"", ""Count"":23 }, { ""Name"":""sulphur"", ""Count"":19 } ], ""Manufactured"":[ { ""Name"":""refinedfocuscrystals"", ""Count"":10 }, { ""Name"":""highdensitycomposites"", ""Count"":3 }, { ""Name"":""mechanicalcomponents"", ""Count"":3 } ], ""Encoded"":[ { ""Name"":""emissiondata"", ""Count"":32 }, { ""Name"":""shielddensityreports"", ""Count"":23 } ] }";

        [PublicAPI("The materials in your inventory (as objects)")]
        public List<MaterialAmount> inventory { get; private set; }

        public MaterialInventoryEvent(DateTime timestamp, List<MaterialAmount> inventory) : base(timestamp, NAME)
        {
            this.inventory = inventory;
        }

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var materials = new List<MaterialAmount>();
            foreach ( var key in new[] { "Raw", "Manufactured", "Encoded" } )
            {
                data.TryGetValue( key, out var val );
                if ( val != null )
                {
                    var materialsJson = (List<object>)val;
                    foreach ( var materialJson in materialsJson.Cast<IDictionary<string, object>>() )
                    {
                        var material = Material.FromEDName(JsonParsing.getString(materialJson, "Name"));
                        materials.Add( new MaterialAmount( material, (int)(long)materialJson[ "Count" ] ) );
                    }
                }
            }

            events.Add( new MaterialInventoryEvent( timestamp, materials ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
