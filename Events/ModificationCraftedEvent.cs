using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ModificationCraftedEvent : Event
    {
        public const string NAME = "Modification crafted";
        public const string DESCRIPTION = "Triggered when you craft a modification to a module";
        public const string SAMPLE = @"{ ""timestamp"":""2018-02-07T07:49:21Z"", ""event"":""EngineerCraft"", ""Slot"":""Military01"", ""Module"":""int_hullreinforcement_size4_class2"", ""Ingredients"":[ { ""Name"":""iron"", ""Count"":1 } ], ""Engineer"":""The Dweller"", ""EngineerID"":300180, ""BlueprintID"":128673719, ""BlueprintName"":""HullReinforcement_HeavyDuty"", ""Level"":5, ""Quality"":0.499200, ""ExperimentalEffect"":""special_hullreinforcement_chunky"", ""ExperimentalEffect_Localised"":""Deep Plating"", ""Modifiers"":[ { ""Label"":""Mass"", ""Value"":11.200000, ""OriginalValue"":8.000000, ""LessIsGood"":1 }, { ""Label"":""DefenceModifierHealthAddition"", ""Value"":602.543701, ""OriginalValue"":330.000000, ""LessIsGood"":0 }, { ""Label"":""KineticResistance"", ""Value"":13.634562, ""OriginalValue"":1.999998, ""LessIsGood"":0 }, { ""Label"":""ThermicResistance"", ""Value"":13.634562, ""OriginalValue"":1.999998, ""LessIsGood"":0 }, { ""Label"":""ExplosiveResistance"", ""Value"":13.634562, ""OriginalValue"":1.999998, ""LessIsGood"":0 } ] }";

        [PublicAPI("The name of the engineer crafting the modification")]
        public string engineer { get; private set; }

        [PublicAPI("The blueprint being crafted")]
        public string blueprint { get; private set; }

        [PublicAPI("The module being crafted")]
        public string module => compartment?.module?.localizedName;

        [PublicAPI("The level of the blueprint being crafted")]
        public int level { get; private set; }

        [PublicAPI("The progression of the blueprint at the current level, expressed as a percentage")]
        public decimal? quality { get; private set; }

        [PublicAPI("The experimental effect being crafted, if applicable")]
        public string experimentaleffect { get; private set; }

        [PublicAPI("The materials and quantities used in the crafting (as objects)")]
        public List<MaterialAmount> materials { get; private set; }

        [PublicAPI("The commodities and quantities used in the crafting (as objects)")]
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
