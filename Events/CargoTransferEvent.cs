using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CargoTransferEvent (
        DateTime timestamp,
        List<CommodityAmount> toShip,
        List<CommodityAmount> toSrv,
        List<CommodityAmount> toCarrier )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Cargo transfer";
        public const string DESCRIPTION = "Triggered when transferring commodities between your ship, SRV, or carrier";
        public static readonly string[] SAMPLES =
        [
            @"{ ""timestamp"":""2023-06-09T09:18:40Z"", ""event"":""CargoTransfer"", ""Transfers"":[ { ""Type"":""thargoidgeneratortissuesample"", ""Type_Localised"":""Caustic Tissue Sample"", ""Count"":2, ""Direction"":""tocarrier"" }, { ""Type"":""drones"", ""Type_Localised"":""Limpet"", ""Count"":12, ""Direction"":""toship"" } ] }",
            @"{ ""timestamp"":""2023-05-22T08:36:19Z"", ""event"":""CargoTransfer"", ""Transfers"":[ { ""Type"":""radiationbaffle"", ""Type_Localised"":""Radiation Baffle"", ""Count"":46, ""Direction"":""tocarrier"" }, { ""Type"":""metaalloys"", ""Type_Localised"":""Meta-Alloys"", ""Count"":16, ""Direction"":""toship"" }, { ""Type"":""neofabricinsulation"", ""Type_Localised"":""Neofabric Insulation"", ""Count"":12, ""Direction"":""toship"" } ] }",
            @"{ ""timestamp"":""2022-08-20T22:11:41Z"", ""event"":""CargoTransfer"", ""Transfers"":[ { ""Type"":""unknownartifact3"", ""Type_Localised"":""Thargoid Link"", ""Count"":1, ""Direction"":""toship"" }, { ""Type"":""ancientrelic"", ""Type_Localised"":""Guardian Relic"", ""Count"":3, ""Direction"":""toship"" }, { ""Type"":""unknowntechnologysamples"", ""Type_Localised"":""Thargoid Technology Samples"", ""Count"":1, ""Direction"":""tosrv"" } ] }"
        ];

        [PublicAPI("The commodities and amounts being transferred to your ship")]
        public List<CommodityAmount> toship { get; private set; } = toShip;

        [PublicAPI( "The commodities and amounts being transferred to your SRV" )]
        public List<CommodityAmount> tosrv { get; private set; } = toSrv;

        [PublicAPI( "The commodities and amounts being transferred to your carrier" )]
        public List<CommodityAmount> tocarrier { get; private set; } = toCarrier;
    }
}
