using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ModificationCraftedEvent (
        DateTime timestamp,
        string engineer,
        long engineerId,
        string blueprint,
        long blueprintId,
        int level,
        decimal? quality,
        string experimentalEffect,
        List<MaterialAmount> materials,
        List<CommodityAmount> commodities,
        string slot,
        Module module )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Modification crafted";
        public const string DESCRIPTION = "Triggered when you craft a modification to a module";
        public const string SAMPLE = @"{ ""timestamp"":""2018-02-07T07:49:21Z"", ""event"":""EngineerCraft"", ""Slot"":""Military01"", ""Module"":""int_hullreinforcement_size4_class2"", ""Ingredients"":[ { ""Name"":""iron"", ""Count"":1 } ], ""Engineer"":""The Dweller"", ""EngineerID"":300180, ""BlueprintID"":128673719, ""BlueprintName"":""HullReinforcement_HeavyDuty"", ""Level"":5, ""Quality"":0.499200, ""ExperimentalEffect"":""special_hullreinforcement_chunky"", ""ExperimentalEffect_Localised"":""Deep Plating"", ""Modifiers"":[ { ""Label"":""Mass"", ""Value"":11.200000, ""OriginalValue"":8.000000, ""LessIsGood"":1 }, { ""Label"":""DefenceModifierHealthAddition"", ""Value"":602.543701, ""OriginalValue"":330.000000, ""LessIsGood"":0 }, { ""Label"":""KineticResistance"", ""Value"":13.634562, ""OriginalValue"":1.999998, ""LessIsGood"":0 }, { ""Label"":""ThermicResistance"", ""Value"":13.634562, ""OriginalValue"":1.999998, ""LessIsGood"":0 }, { ""Label"":""ExplosiveResistance"", ""Value"":13.634562, ""OriginalValue"":1.999998, ""LessIsGood"":0 } ] }";

        [PublicAPI("The name of the engineer crafting the modification")]
        public string engineer { get; private set; } = engineer;

        [PublicAPI("The blueprint being crafted")]
        public string blueprint { get; private set; } = blueprint;

        [PublicAPI("The module being crafted")]
        public string module => Module?.localizedName;

        [PublicAPI("The level of the blueprint being crafted")]
        public int level { get; private set; } = level;

        [PublicAPI("The progression of the blueprint at the current level, expressed as a percentage")]
        public decimal? quality { get; private set; } = (quality == null) ? (decimal?)null : Math.Round((decimal)quality * 100, 2);

        [PublicAPI("The experimental effect being crafted, if applicable")]
        public string experimentaleffect { get; private set; } = experimentalEffect;

        [PublicAPI("The materials and quantities used in the crafting (as objects)")]
        public List<MaterialAmount> materials { get; private set; } = materials;

        [PublicAPI("The commodities and quantities used in the crafting (as objects)")]
        public List<CommodityAmount> commodities { get; private set; } = commodities;

        // Not intended to be user facing

        public long engineerId { get; private set; } = -engineerId;

        public long blueprintId { get; private set; } = blueprintId;

        public string slot { get; private set; } = slot;

        public Module Module { get; private set; } = module;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            var engineer = JsonParsing.getString(data, "Engineer");
            var engineerId = JsonParsing.getLong(data, "EngineerID");
            var blueprintpEdName = JsonParsing.getString(data, "BlueprintName");
            var blueprintId = JsonParsing.getLong(data, "BlueprintID");

            data.TryGetValue( "Level", out var val );
            var level = (int)(long)val;

            var quality = JsonParsing.getOptionalDecimal(data, "Quality");
            var experimentalEffect = JsonParsing.getString(data, "ApplyExperimentalEffect");

            var slot = JsonParsing.getString( data, "Slot" );
            var module = Module.FromEDName( JsonParsing.getString( data, "Module" ) );

            var commodities = new List<CommodityAmount>();
            var materials = new List<MaterialAmount>();
            if ( data.TryGetValue( "Ingredients", out val ) )
            {
                // 2.2 style
                if ( val is Dictionary<string, object> usedData )
                {
                    foreach ( var used in usedData )
                    {
                        // Used could be a material or a commodity
                        var commodity = CommodityDefinition.FromEDName(used.Key);
                        if ( commodity.Category != null )
                        {
                            // This is a real commodity
                            commodities.Add( new CommodityAmount( commodity, (int)(long)used.Value ) );
                        }
                        else
                        {
                            // Probably a material then
                            var material = Material.FromEDName(used.Key);
                            materials.Add( new MaterialAmount( material, (int)(long)used.Value ) );
                        }
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
            }
            events.Add( new ModificationCraftedEvent( timestamp, engineer, engineerId, blueprintpEdName, blueprintId, level, quality, experimentalEffect, materials, commodities, slot, module ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
