using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class ModificationCraftedEvent : Event
    {
        public const string NAME = "Modification crafted";
        public const string DESCRIPTION = "Triggered when you craft a modification to a module";
        public const string SAMPLE = @"{ ""timestamp"":""2018-02-07T07:49:21Z"", ""event"":""EngineerCraft"", ""Slot"":""Military01"", ""Module"":""int_hullreinforcement_size4_class2"", ""Ingredients"":[ { ""Name"":""iron"", ""Count"":1 } ], ""Engineer"":""The Dweller"", ""EngineerID"":300180, ""BlueprintID"":128673719, ""BlueprintName"":""HullReinforcement_HeavyDuty"", ""Level"":5, ""Quality"":0.499200, ""ExperimentalEffect"":""special_hullreinforcement_chunky"", ""ExperimentalEffect_Localised"":""Deep Plating"", ""Modifiers"":[ { ""Label"":""Mass"", ""Value"":11.200000, ""OriginalValue"":8.000000, ""LessIsGood"":1 }, { ""Label"":""DefenceModifierHealthAddition"", ""Value"":602.543701, ""OriginalValue"":330.000000, ""LessIsGood"":0 }, { ""Label"":""KineticResistance"", ""Value"":13.634562, ""OriginalValue"":1.999998, ""LessIsGood"":0 }, { ""Label"":""ThermicResistance"", ""Value"":13.634562, ""OriginalValue"":1.999998, ""LessIsGood"":0 }, { ""Label"":""ExplosiveResistance"", ""Value"":13.634562, ""OriginalValue"":1.999998, ""LessIsGood"":0 } ] }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ModificationCraftedEvent()
        {
            VARIABLES.Add("engineer", "The name of the engineer crafting the modification");
            VARIABLES.Add("blueprint", "The blueprint being crafted");
            VARIABLES.Add("module", "The module being crafted");
            VARIABLES.Add("level", "The level of the blueprint being crafted");
            VARIABLES.Add("quality", "The progression of the blueprint at the current level, expressed as a percentage");
            VARIABLES.Add("experimentaleffect", "The experimental effect being crafted, if applicable");
            VARIABLES.Add("materials", "The materials and quantities used in the crafting (MaterialAmount object)");
            VARIABLES.Add("commodities", "The commodities and quantities used in the crafting (CommodityAmount object)");
        }

        public string engineer { get; private set; }

        public string blueprint { get; private set; }

        public string module => compartment?.module?.localizedName;

        public int level { get; private set; }

        public decimal? quality { get; private set; }

        public string experimentaleffect { get; private set; }

        public List<MaterialAmount> materials { get; private set; }

        public List<CommodityAmount> commodities { get; private set; }

        // Not intended to be user facing
        public long engineerId { get; private set; }
        public long blueprintId { get; private set; }
        public Compartment compartment { get; private set; }

        public ModificationCraftedEvent(DateTime timestamp, string engineer, long engineerId, string blueprint, long blueprintId, int level, decimal? quality, string experimentalEffect, List<MaterialAmount> materials, List<CommodityAmount> commodities, Compartment compartment) : base(timestamp, NAME)
        {
            this.engineer = engineer;
            this.engineerId = -engineerId;
            this.blueprint = blueprint;
            this.blueprintId = blueprintId;
            this.level = level;
            this.quality = (quality == null) ? (decimal?)null : Math.Round((decimal)quality * 100, 2);
            this.experimentaleffect = experimentalEffect;
            this.materials = materials;
            this.commodities = commodities;
            this.compartment = compartment;
        }
    }
}
