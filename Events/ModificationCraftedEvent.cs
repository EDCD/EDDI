using EddiDataDefinitions;
using System;
using System.Collections.Generic;
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
    }
}
