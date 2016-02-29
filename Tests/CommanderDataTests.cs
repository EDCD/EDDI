using Microsoft.VisualStudio.TestTools.UnitTesting;
using EliteDangerousCompanionAppService;
using EliteDangerousDataDefinitions;
using System.Collections.Generic;
using System;

namespace Tests
{
    [TestClass]
    public class CommanderDataTests
    {
        [TestMethod]
        public void TestCommanderFromProfile()
        {
            string data = @"{
  ""commander"": {
                ""id"": 123456,
    ""name"": ""Testy"",
    ""credits"": 14572598,
    ""debt"": 0,
    ""currentShipId"": 3,
    ""alive"": true,
    ""docked"": true,
    ""rank"": {
                    ""combat"": 6,
      ""trade"": 5,
      ""explore"": 3,
      ""crime"": 0,
      ""service"": 0,
      ""empire"": 0,
      ""federation"": 2,
      ""power"": 1,
      ""cqc"": 0
    }
            },
  ""lastSystem"": {
                ""id"": ""50420"",
    ""name"": ""Lalande 37120"",
    ""faction"": ""Federation""
  },
  ""lastStarport"": {
                ""id"": ""3226643968"",
    ""name"": ""Voss Dock"",
    ""faction"": ""Federation"",
    ""commodities"": [
      {
        ""id"": ""128049202"",
        ""name"": ""Hydrogen Fuel"",
        ""cost_min"": 125,
        ""cost_max"": 168,
        ""cost_mean"": ""147.00"",
        ""homebuy"": ""74"",
        ""homesell"": ""71"",
        ""consumebuy"": ""3"",
        ""baseCreationQty"": 200,
        ""baseConsumptionQty"": 200,
        ""capacity"": 1263791,
        ""buyPrice"": 0,
        ""sellPrice"": 162,
        ""meanPrice"": 147,
        ""demandBracket"": 2,
        ""stockBracket"": 0,
        ""creationQty"": 12513,
        ""consumptionQty"": 1251278,
        ""targetStock"": 325332,
        ""stock"": 0,
        ""demand"": 840232,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Chemicals"",
        ""volumescale"": ""1.3000""
      },
      {
        ""id"": ""128049203"",
        ""name"": ""Mineral Oil"",
        ""cost_min"": 192,
        ""cost_max"": 325,
        ""cost_mean"": ""259.00"",
        ""homebuy"": ""47"",
        ""homesell"": ""42"",
        ""consumebuy"": ""5"",
        ""baseCreationQty"": 647,
        ""baseConsumptionQty"": 0,
        ""capacity"": 40479,
        ""buyPrice"": 119,
        ""sellPrice"": 106,
        ""meanPrice"": 259,
        ""demandBracket"": 0,
        ""stockBracket"": 2,
        ""creationQty"": 40479,
        ""consumptionQty"": 0,
        ""targetStock"": 40479,
        ""stock"": 22665,
        ""demand"": 1,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Chemicals"",
        ""volumescale"": ""1.0300""
      },
      {
        ""id"": ""128049241"",
        ""name"": ""Clothing"",
        ""cost_min"": 315,
        ""cost_max"": 474,
        ""cost_mean"": ""395.00"",
        ""homebuy"": ""61"",
        ""homesell"": ""57"",
        ""consumebuy"": ""4"",
        ""baseCreationQty"": 0,
        ""baseConsumptionQty"": 350,
        ""capacity"": 2189737,
        ""buyPrice"": 0,
        ""sellPrice"": 470,
        ""meanPrice"": 395,
        ""demandBracket"": 3,
        ""stockBracket"": 0,
        ""creationQty"": 0,
        ""consumptionQty"": 2189737,
        ""targetStock"": 547434,
        ""stock"": 0,
        ""demand"": 1606808,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Consumer Items"",
        ""volumescale"": ""1.1500""
      },
      {
        ""id"": ""128049240"",
        ""name"": ""Consumer Technology"",
        ""cost_min"": 6561,
        ""cost_max"": 7500,
        ""cost_mean"": ""7031.00"",
        ""homebuy"": ""93"",
        ""homesell"": ""92"",
        ""consumebuy"": ""1"",
        ""baseCreationQty"": 0,
        ""baseConsumptionQty"": 27,
        ""capacity"": 304061,
        ""buyPrice"": 0,
        ""sellPrice"": 7499,
        ""meanPrice"": 7031,
        ""demandBracket"": 3,
        ""stockBracket"": 0,
        ""creationQty"": 0,
        ""consumptionQty"": 304061,
        ""targetStock"": 76015,
        ""stock"": 0,
        ""demand"": 228046,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Consumer Items"",
        ""volumescale"": ""1.1000""
      },
      {
        ""id"": ""128049238"",
        ""name"": ""Domestic Appliances"",
        ""cost_min"": 527,
        ""cost_max"": 734,
        ""cost_mean"": ""631.00"",
        ""homebuy"": ""70"",
        ""homesell"": ""67"",
        ""consumebuy"": ""3"",
        ""baseCreationQty"": 0,
        ""baseConsumptionQty"": 209,
        ""capacity"": 653793,
        ""buyPrice"": 0,
        ""sellPrice"": 728,
        ""meanPrice"": 631,
        ""demandBracket"": 3,
        ""stockBracket"": 0,
        ""creationQty"": 0,
        ""consumptionQty"": 653793,
        ""targetStock"": 163448,
        ""stock"": 0,
        ""demand"": 479376,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Consumer Items"",
        ""volumescale"": ""1.2500""
      },
      {
        ""id"": ""128049177"",
        ""name"": ""Algae"",
        ""cost_min"": 135,
        ""cost_max"": 265,
        ""cost_mean"": ""200.00"",
        ""homebuy"": ""27"",
        ""homesell"": ""20"",
        ""consumebuy"": ""7"",
        ""baseCreationQty"": 6302,
        ""baseConsumptionQty"": 0,
        ""capacity"": 394278,
        ""buyPrice"": 53,
        ""sellPrice"": 39,
        ""meanPrice"": 200,
        ""demandBracket"": 0,
        ""stockBracket"": 2,
        ""creationQty"": 394278,
        ""consumptionQty"": 0,
        ""targetStock"": 394278,
        ""stock"": 220795,
        ""demand"": 1,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Foods"",
        ""volumescale"": ""1.0000""
      },
      {
        ""id"": ""128049182"",
        ""name"": ""Animal Meat"",
        ""cost_min"": 1286,
        ""cost_max"": 1633,
        ""cost_mean"": ""1460.00"",
        ""homebuy"": ""81"",
        ""homesell"": ""79"",
        ""consumebuy"": ""2"",
        ""baseCreationQty"": 19,
        ""baseConsumptionQty"": 0,
        ""capacity"": 1189,
        ""buyPrice"": 1178,
        ""sellPrice"": 1137,
        ""meanPrice"": 1460,
        ""demandBracket"": 0,
        ""stockBracket"": 2,
        ""creationQty"": 1189,
        ""consumptionQty"": 0,
        ""targetStock"": 1189,
        ""stock"": 663,
        ""demand"": 1,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Foods"",
        ""volumescale"": ""1.4000""
      },
      {
        ""id"": ""128049189"",
        ""name"": ""Coffee"",
        ""cost_min"": 1286,
        ""cost_max"": 1633,
        ""cost_mean"": ""1460.00"",
        ""homebuy"": ""81"",
        ""homesell"": ""79"",
        ""consumebuy"": ""2"",
        ""baseCreationQty"": 19,
        ""baseConsumptionQty"": 0,
        ""capacity"": 1189,
        ""buyPrice"": 1178,
        ""sellPrice"": 1137,
        ""meanPrice"": 1460,
        ""demandBracket"": 0,
        ""stockBracket"": 2,
        ""creationQty"": 1189,
        ""consumptionQty"": 0,
        ""targetStock"": 1189,
        ""stock"": 663,
        ""demand"": 1,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Foods"",
        ""volumescale"": ""1.4000""
      },
      {
        ""id"": ""128049183"",
        ""name"": ""Fish"",
        ""cost_min"": 403,
        ""cost_max"": 583,
        ""cost_mean"": ""493.00"",
        ""homebuy"": ""66"",
        ""homesell"": ""63"",
        ""consumebuy"": ""3"",
        ""baseCreationQty"": 541,
        ""baseConsumptionQty"": 3,
        ""capacity"": 52618,
        ""buyPrice"": 346,
        ""sellPrice"": 327,
        ""meanPrice"": 493,
        ""demandBracket"": 0,
        ""stockBracket"": 2,
        ""creationQty"": 33848,
        ""consumptionQty"": 18770,
        ""targetStock"": 38540,
        ""stock"": 23642,
        ""demand"": 1,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Foods"",
        ""volumescale"": ""1.2000""
      },
      {
        ""id"": ""128049178"",
        ""name"": ""Fruit And Vegetables"",
        ""cost_min"": 315,
        ""cost_max"": 474,
        ""cost_mean"": ""395.00"",
        ""homebuy"": ""61"",
        ""homesell"": ""57"",
        ""consumebuy"": ""4"",
        ""baseCreationQty"": 35,
        ""baseConsumptionQty"": 0,
        ""capacity"": 2190,
        ""buyPrice"": 238,
        ""sellPrice"": 220,
        ""meanPrice"": 395,
        ""demandBracket"": 0,
        ""stockBracket"": 2,
        ""creationQty"": 2190,
        ""consumptionQty"": 0,
        ""targetStock"": 2190,
        ""stock"": 1221,
        ""demand"": 1,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Foods"",
        ""volumescale"": ""1.1500""
      },
      {
        ""id"": ""128049180"",
        ""name"": ""Grain"",
        ""cost_min"": 207,
        ""cost_max"": 342,
        ""cost_mean"": ""275.00"",
        ""homebuy"": ""50"",
        ""homesell"": ""45"",
        ""consumebuy"": ""5"",
        ""baseCreationQty"": 58,
        ""baseConsumptionQty"": 0,
        ""capacity"": 3629,
        ""buyPrice"": 135,
        ""sellPrice"": 120,
        ""meanPrice"": 275,
        ""demandBracket"": 0,
        ""stockBracket"": 2,
        ""creationQty"": 3629,
        ""consumptionQty"": 0,
        ""targetStock"": 3629,
        ""stock"": 2028,
        ""demand"": 1,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Foods"",
        ""volumescale"": ""1.0500""
      },
      {
        ""id"": ""128049188"",
        ""name"": ""Tea"",
        ""cost_min"": 1459,
        ""cost_max"": 1833,
        ""cost_mean"": ""1646.00"",
        ""homebuy"": ""82"",
        ""homesell"": ""80"",
        ""consumebuy"": ""2"",
        ""baseCreationQty"": 18,
        ""baseConsumptionQty"": 0,
        ""capacity"": 1127,
        ""buyPrice"": 1345,
        ""sellPrice"": 1300,
        ""meanPrice"": 1646,
        ""demandBracket"": 0,
        ""stockBracket"": 2,
        ""creationQty"": 1127,
        ""consumptionQty"": 0,
        ""targetStock"": 1127,
        ""stock"": 628,
        ""demand"": 1,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Foods"",
        ""volumescale"": ""1.4200""
      },
      {
        ""id"": ""128064028"",
        ""name"": ""Atmospheric Extractors"",
        ""cost_min"": 403,
        ""cost_max"": 583,
        ""cost_mean"": ""493.00"",
        ""homebuy"": ""66"",
        ""homesell"": ""63"",
        ""consumebuy"": ""3"",
        ""baseCreationQty"": 0,
        ""baseConsumptionQty"": 27,
        ""capacity"": 168923,
        ""buyPrice"": 0,
        ""sellPrice"": 467,
        ""meanPrice"": 493,
        ""demandBracket"": 2,
        ""stockBracket"": 0,
        ""creationQty"": 0,
        ""consumptionQty"": 168923,
        ""targetStock"": 42230,
        ""stock"": 0,
        ""demand"": 66560,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Machinery"",
        ""volumescale"": ""1.2000""
      },
      {
        ""id"": ""128049222"",
        ""name"": ""Crop Harvesters"",
        ""cost_min"": 2142,
        ""cost_max"": 2613,
        ""cost_mean"": ""2378.00"",
        ""homebuy"": ""86"",
        ""homesell"": ""85"",
        ""consumebuy"": ""1"",
        ""baseCreationQty"": 0,
        ""baseConsumptionQty"": 64,
        ""capacity"": 16017,
        ""buyPrice"": 0,
        ""sellPrice"": 2142,
        ""meanPrice"": 2378,
        ""demandBracket"": 1,
        ""stockBracket"": 0,
        ""creationQty"": 0,
        ""consumptionQty"": 16017,
        ""targetStock"": 4004,
        ""stock"": 0,
        ""demand"": 3003.25,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Machinery"",
        ""volumescale"": ""1.4400""
      },
      {
        ""id"": ""128049223"",
        ""name"": ""Marine Supplies"",
        ""cost_min"": 4122,
        ""cost_max"": 4826,
        ""cost_mean"": ""4474.00"",
        ""homebuy"": ""90"",
        ""homesell"": ""89"",
        ""consumebuy"": ""1"",
        ""baseCreationQty"": 0,
        ""baseConsumptionQty"": 193,
        ""capacity"": 48300,
        ""buyPrice"": 0,
        ""sellPrice"": 4122,
        ""meanPrice"": 4474,
        ""demandBracket"": 1,
        ""stockBracket"": 0,
        ""creationQty"": 0,
        ""consumptionQty"": 48300,
        ""targetStock"": 12075,
        ""stock"": 0,
        ""demand"": 9056.25,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Machinery"",
        ""volumescale"": ""1.2400""
      },
      {
        ""id"": ""128049217"",
        ""name"": ""Power Generators"",
        ""cost_min"": 527,
        ""cost_max"": 734,
        ""cost_mean"": ""631.00"",
        ""homebuy"": ""70"",
        ""homesell"": ""67"",
        ""consumebuy"": ""3"",
        ""baseCreationQty"": 0,
        ""baseConsumptionQty"": 21,
        ""capacity"": 5256,
        ""buyPrice"": 0,
        ""sellPrice"": 527,
        ""meanPrice"": 631,
        ""demandBracket"": 1,
        ""stockBracket"": 0,
        ""creationQty"": 0,
        ""consumptionQty"": 5256,
        ""targetStock"": 1314,
        ""stock"": 0,
        ""demand"": 985.5,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Machinery"",
        ""volumescale"": ""1.2500""
      },
      {
        ""id"": ""128049218"",
        ""name"": ""Water Purifiers"",
        ""cost_min"": 300,
        ""cost_max"": 456,
        ""cost_mean"": ""378.00"",
        ""homebuy"": ""60"",
        ""homesell"": ""56"",
        ""consumebuy"": ""4"",
        ""baseCreationQty"": 0,
        ""baseConsumptionQty"": 37,
        ""capacity"": 9260,
        ""buyPrice"": 0,
        ""sellPrice"": 300,
        ""meanPrice"": 378,
        ""demandBracket"": 1,
        ""stockBracket"": 0,
        ""creationQty"": 0,
        ""consumptionQty"": 9260,
        ""targetStock"": 2315,
        ""stock"": 0,
        ""demand"": 1736.25,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Machinery"",
        ""volumescale"": ""1.1400""
      },
      {
        ""id"": ""128049208"",
        ""name"": ""Agricultural Medicines"",
        ""cost_min"": 1004,
        ""cost_max"": 1303,
        ""cost_mean"": ""1154.00"",
        ""homebuy"": ""79"",
        ""homesell"": ""77"",
        ""consumebuy"": ""2"",
        ""baseCreationQty"": 0,
        ""baseConsumptionQty"": 119,
        ""capacity"": 29781,
        ""buyPrice"": 0,
        ""sellPrice"": 1303,
        ""meanPrice"": 1154,
        ""demandBracket"": 3,
        ""stockBracket"": 0,
        ""creationQty"": 0,
        ""consumptionQty"": 29781,
        ""targetStock"": 7445,
        ""stock"": 0,
        ""demand"": 22336,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Medicines"",
        ""volumescale"": ""1.3600""
      },
      {
        ""id"": ""128049210"",
        ""name"": ""Basic Medicines"",
        ""cost_min"": 315,
        ""cost_max"": 474,
        ""cost_mean"": ""395.00"",
        ""homebuy"": ""61"",
        ""homesell"": ""57"",
        ""consumebuy"": ""4"",
        ""baseCreationQty"": 0,
        ""baseConsumptionQty"": 350,
        ""capacity"": 109487,
        ""buyPrice"": 0,
        ""sellPrice"": 381,
        ""meanPrice"": 395,
        ""demandBracket"": 2,
        ""stockBracket"": 0,
        ""creationQty"": 0,
        ""consumptionQty"": 109487,
        ""targetStock"": 27371,
        ""stock"": 0,
        ""demand"": 46621,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Medicines"",
        ""volumescale"": ""1.1500""
      },
      {
        ""id"": ""128049209"",
        ""name"": ""Performance Enhancers"",
        ""cost_min"": 6561,
        ""cost_max"": 7500,
        ""cost_mean"": ""7031.00"",
        ""homebuy"": ""93"",
        ""homesell"": ""92"",
        ""consumebuy"": ""1"",
        ""baseCreationQty"": 0,
        ""baseConsumptionQty"": 67,
        ""capacity"": 209590,
        ""buyPrice"": 0,
        ""sellPrice"": 7499,
        ""meanPrice"": 7031,
        ""demandBracket"": 3,
        ""stockBracket"": 0,
        ""creationQty"": 0,
        ""consumptionQty"": 209590,
        ""targetStock"": 52397,
        ""stock"": 0,
        ""demand"": 157193,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Medicines"",
        ""volumescale"": ""1.1000""
      },
      {
        ""id"": ""128049669"",
        ""name"": ""Progenitor Cells"",
        ""cost_min"": 6561,
        ""cost_max"": 7500,
        ""cost_mean"": ""7031.00"",
        ""homebuy"": ""93"",
        ""homesell"": ""92"",
        ""consumebuy"": ""1"",
        ""baseCreationQty"": 0,
        ""baseConsumptionQty"": 27,
        ""capacity"": 168923,
        ""buyPrice"": 0,
        ""sellPrice"": 7499,
        ""meanPrice"": 7031,
        ""demandBracket"": 3,
        ""stockBracket"": 0,
        ""creationQty"": 0,
        ""consumptionQty"": 168923,
        ""targetStock"": 42230,
        ""stock"": 0,
        ""demand"": 126693,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Medicines"",
        ""volumescale"": ""1.1000""
      },
      {
        ""id"": ""128668550"",
        ""name"": ""Painite"",
        ""cost_min"": 30000,
        ""cost_max"": 36000,
        ""cost_mean"": ""33000.00"",
        ""homebuy"": ""100"",
        ""homesell"": ""100"",
        ""consumebuy"": ""1"",
        ""baseCreationQty"": 0,
        ""baseConsumptionQty"": 15,
        ""capacity"": 93846,
        ""buyPrice"": 0,
        ""sellPrice"": 35995,
        ""meanPrice"": 33000,
        ""demandBracket"": 3,
        ""stockBracket"": 0,
        ""creationQty"": 0,
        ""consumptionQty"": 93846,
        ""targetStock"": 23461,
        ""stock"": 0,
        ""demand"": 70385,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Minerals"",
        ""volumescale"": ""1.0000""
      },
      {
        ""id"": ""128049214"",
        ""name"": ""Beer"",
        ""cost_min"": 175,
        ""cost_max"": 304,
        ""cost_mean"": ""240.00"",
        ""homebuy"": ""43"",
        ""homesell"": ""37"",
        ""consumebuy"": ""6"",
        ""baseCreationQty"": 75,
        ""baseConsumptionQty"": 0,
        ""capacity"": 4693,
        ""buyPrice"": 101,
        ""sellPrice"": 86,
        ""meanPrice"": 240,
        ""demandBracket"": 0,
        ""stockBracket"": 2,
        ""creationQty"": 4693,
        ""consumptionQty"": 0,
        ""targetStock"": 4693,
        ""stock"": 2625,
        ""demand"": 1,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Narcotics"",
        ""volumescale"": ""1.0000""
      },
      {
        ""id"": ""128049216"",
        ""name"": ""Liquor"",
        ""cost_min"": 681,
        ""cost_max"": 795,
        ""cost_mean"": ""738.00"",
        ""homebuy"": ""73"",
        ""homesell"": ""70"",
        ""consumebuy"": ""3"",
        ""baseCreationQty"": 18,
        ""baseConsumptionQty"": 0,
        ""capacity"": 5,
        ""buyPrice"": 503,
        ""sellPrice"": 477,
        ""meanPrice"": 738,
        ""demandBracket"": 0,
        ""stockBracket"": 3,
        ""creationQty"": 5,
        ""consumptionQty"": 0,
        ""targetStock"": 5,
        ""stock"": 4,
        ""demand"": 1,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Narcotics"",
        ""volumescale"": ""1.2800""
      },
      {
        ""id"": ""128049215"",
        ""name"": ""Wine"",
        ""cost_min"": 252,
        ""cost_max"": 396,
        ""cost_mean"": ""324.00"",
        ""homebuy"": ""56"",
        ""homesell"": ""52"",
        ""consumebuy"": ""4"",
        ""baseCreationQty"": 45,
        ""baseConsumptionQty"": 0,
        ""capacity"": 2816,
        ""buyPrice"": 179,
        ""sellPrice"": 164,
        ""meanPrice"": 324,
        ""demandBracket"": 0,
        ""stockBracket"": 2,
        ""creationQty"": 2816,
        ""consumptionQty"": 0,
        ""targetStock"": 2816,
        ""stock"": 1573,
        ""demand"": 1,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Narcotics"",
        ""volumescale"": ""1.1000""
      },
      {
        ""id"": ""128066403"",
        ""name"": ""Drones"",
        ""cost_min"": 100,
        ""cost_max"": 100,
        ""cost_mean"": ""100.00"",
        ""homebuy"": ""100"",
        ""homesell"": ""100"",
        ""consumebuy"": ""1"",
        ""baseCreationQty"": 200,
        ""baseConsumptionQty"": 0,
        ""capacity"": 1251278,
        ""buyPrice"": 101,
        ""sellPrice"": 100,
        ""meanPrice"": 100,
        ""demandBracket"": 0,
        ""stockBracket"": 3,
        ""creationQty"": 1251278,
        ""consumptionQty"": 0,
        ""targetStock"": 1251278,
        ""stock"": 1251278,
        ""demand"": 1,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""NonMarketable"",
        ""volumescale"": ""1.0000""
      },
      {
        ""id"": ""128049229"",
        ""name"": ""Animal Monitors"",
        ""cost_min"": 300,
        ""cost_max"": 456,
        ""cost_mean"": ""378.00"",
        ""homebuy"": ""60"",
        ""homesell"": ""56"",
        ""consumebuy"": ""4"",
        ""baseCreationQty"": 0,
        ""baseConsumptionQty"": 920,
        ""capacity"": 230236,
        ""buyPrice"": 0,
        ""sellPrice"": 456,
        ""meanPrice"": 378,
        ""demandBracket"": 3,
        ""stockBracket"": 0,
        ""creationQty"": 0,
        ""consumptionQty"": 230236,
        ""targetStock"": 57559,
        ""stock"": 0,
        ""demand"": 172677,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Technology"",
        ""volumescale"": ""1.1400""
      },
      {
        ""id"": ""128049230"",
        ""name"": ""Aquaponic Systems"",
        ""cost_min"": 274,
        ""cost_max"": 424,
        ""cost_mean"": ""349.00"",
        ""homebuy"": ""58"",
        ""homesell"": ""54"",
        ""consumebuy"": ""4"",
        ""baseCreationQty"": 0,
        ""baseConsumptionQty"": 1020,
        ""capacity"": 255261,
        ""buyPrice"": 0,
        ""sellPrice"": 424,
        ""meanPrice"": 349,
        ""demandBracket"": 3,
        ""stockBracket"": 0,
        ""creationQty"": 0,
        ""consumptionQty"": 255261,
        ""targetStock"": 63815,
        ""stock"": 0,
        ""demand"": 191446,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Technology"",
        ""volumescale"": ""1.1200""
      },
      {
        ""id"": ""128049232"",
        ""name"": ""Terrain Enrichment Systems"",
        ""cost_min"": 4705,
        ""cost_max"": 5470,
        ""cost_mean"": ""5088.00"",
        ""homebuy"": ""91"",
        ""homesell"": ""90"",
        ""consumebuy"": ""1"",
        ""baseCreationQty"": 0,
        ""baseConsumptionQty"": 174,
        ""capacity"": 43545,
        ""buyPrice"": 0,
        ""sellPrice"": 5470,
        ""meanPrice"": 5088,
        ""demandBracket"": 3,
        ""stockBracket"": 0,
        ""creationQty"": 0,
        ""consumptionQty"": 43545,
        ""targetStock"": 10886,
        ""stock"": 0,
        ""demand"": 32659,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Technology"",
        ""volumescale"": ""1.2000""
      },
      {
        ""id"": ""128049190"",
        ""name"": ""Leather"",
        ""cost_min"": 175,
        ""cost_max"": 304,
        ""cost_mean"": ""240.00"",
        ""homebuy"": ""43"",
        ""homesell"": ""37"",
        ""consumebuy"": ""6"",
        ""baseCreationQty"": 75,
        ""baseConsumptionQty"": 0,
        ""capacity"": 4693,
        ""buyPrice"": 101,
        ""sellPrice"": 86,
        ""meanPrice"": 240,
        ""demandBracket"": 0,
        ""stockBracket"": 2,
        ""creationQty"": 4693,
        ""consumptionQty"": 0,
        ""targetStock"": 4693,
        ""stock"": 2627,
        ""demand"": 1,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Textiles"",
        ""volumescale"": ""1.0000""
      },
      {
        ""id"": ""128049191"",
        ""name"": ""Natural Fabrics"",
        ""cost_min"": 403,
        ""cost_max"": 583,
        ""cost_mean"": ""493.00"",
        ""homebuy"": ""66"",
        ""homesell"": ""63"",
        ""consumebuy"": ""3"",
        ""baseCreationQty"": 27,
        ""baseConsumptionQty"": 0,
        ""capacity"": 1690,
        ""buyPrice"": 322,
        ""sellPrice"": 304,
        ""meanPrice"": 493,
        ""demandBracket"": 0,
        ""stockBracket"": 2,
        ""creationQty"": 1690,
        ""consumptionQty"": 0,
        ""targetStock"": 1690,
        ""stock"": 946,
        ""demand"": 1,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Textiles"",
        ""volumescale"": ""1.2000""
      },
      {
        ""id"": ""128049244"",
        ""name"": ""Biowaste"",
        ""cost_min"": 50,
        ""cost_max"": 98,
        ""cost_mean"": ""74.00"",
        ""homebuy"": ""27"",
        ""homesell"": ""20"",
        ""consumebuy"": ""7"",
        ""baseCreationQty"": 0,
        ""baseConsumptionQty"": 1620,
        ""capacity"": 10135351,
        ""buyPrice"": 0,
        ""sellPrice"": 98,
        ""meanPrice"": 74,
        ""demandBracket"": 3,
        ""stockBracket"": 0,
        ""creationQty"": 0,
        ""consumptionQty"": 10135351,
        ""targetStock"": 2533837,
        ""stock"": 0,
        ""demand"": 7589299,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Waste "",
        ""volumescale"": ""1.0000""
      },
      {
        ""id"": ""128049236"",
        ""name"": ""Non Lethal Weapons"",
        ""cost_min"": 1766,
        ""cost_max"": 2185,
        ""cost_mean"": ""1976.00"",
        ""homebuy"": ""84"",
        ""homesell"": ""82"",
        ""consumebuy"": ""2"",
        ""baseCreationQty"": 0,
        ""baseConsumptionQty"": 75,
        ""capacity"": 9385,
        ""buyPrice"": 0,
        ""sellPrice"": 2185,
        ""meanPrice"": 1976,
        ""demandBracket"": 3,
        ""stockBracket"": 0,
        ""creationQty"": 0,
        ""consumptionQty"": 9385,
        ""targetStock"": 2346,
        ""stock"": 0,
        ""demand"": 7039,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Weapons"",
        ""volumescale"": ""1.4500""
      },
      {
        ""id"": ""128049235"",
        ""name"": ""Reactive Armour"",
        ""cost_min"": 2008,
        ""cost_max"": 2461,
        ""cost_mean"": ""2235.00"",
        ""homebuy"": ""85"",
        ""homesell"": ""84"",
        ""consumebuy"": ""1"",
        ""baseCreationQty"": 0,
        ""baseConsumptionQty"": 68,
        ""capacity"": 25527,
        ""buyPrice"": 0,
        ""sellPrice"": 2461,
        ""meanPrice"": 2235,
        ""demandBracket"": 3,
        ""stockBracket"": 0,
        ""creationQty"": 0,
        ""consumptionQty"": 25527,
        ""targetStock"": 6381,
        ""stock"": 0,
        ""demand"": 19146,
        ""rare_min_stock"": ""0"",
        ""rare_max_stock"": ""0"",
        ""market_id"": null,
        ""parent_id"": null,
        ""statusFlags"": [],
        ""categoryname"": ""Weapons"",
        ""volumescale"": ""1.4600""
      }
    ],
    ""ships"": {
      ""shipyard_list"": {
        ""Type7"": {
          ""id"": 128049297,
          ""name"": ""Type7"",
          ""basevalue"": 17472252,
          ""sku"": """"
        },
        ""CobraMkIII"": {
          ""id"": 128049279,
          ""name"": ""CobraMkIII"",
          ""basevalue"": 349718,
          ""sku"": """"
        },
        ""Type6"": {
          ""id"": 128049285,
          ""name"": ""Type6"",
          ""basevalue"": 1045945,
          ""sku"": """"
        },
        ""SideWinder"": {
          ""id"": 128049249,
          ""name"": ""SideWinder"",
          ""basevalue"": 32000,
          ""sku"": """"
        },
        ""Eagle"": {
          ""id"": 128049255,
          ""name"": ""Eagle"",
          ""basevalue"": 44800,
          ""sku"": """"
        },
        ""Independant_Trader"": {
          ""id"": 128672269,
          ""name"": ""Independant_Trader"",
          ""basevalue"": 3126154,
          ""sku"": """"
        },
        ""Hauler"": {
          ""id"": 128049261,
          ""name"": ""Hauler"",
          ""basevalue"": 52720,
          ""sku"": """"
        },
        ""Vulture"": {
          ""id"": 128049309,
          ""name"": ""Vulture"",
          ""basevalue"": 4925615,
          ""sku"": """"
        }
      },
      ""unavailable_list"": [
        {
          ""id"": 128049321,
          ""name"": ""Federation_Dropship"",
          ""basevalue"": 14314205,
          ""sku"": """",
          ""unavailableReason"": ""Insufficient Rank"",
          ""factionId"": ""3"",
          ""requiredRank"": 3
        }
      ]
    },
    ""modules"": {
      ""128049489"": {
        ""id"": 128049489,
        ""category"": ""weapon"",
        ""name"": ""Hpt_Railgun_Fixed_Medium"",
        ""cost"": 412800,
        ""sku"": null
      },
      ""128049488"": {
        ""id"": 128049488,
        ""category"": ""weapon"",
        ""name"": ""Hpt_Railgun_Fixed_Small"",
        ""cost"": 51600,
        ""sku"": null
      },
      ""128049493"": {
        ""id"": 128049493,
        ""category"": ""weapon"",
        ""name"": ""Hpt_BasicMissileRack_Fixed_Medium"",
        ""cost"": 512400,
        ""sku"": null
      },
      ""128049492"": {
        ""id"": 128049492,
        ""category"": ""weapon"",
        ""name"": ""Hpt_BasicMissileRack_Fixed_Small"",
        ""cost"": 72600,
        ""sku"": null
      },
      ""128049500"": {
        ""id"": 128049500,
        ""category"": ""weapon"",
        ""name"": ""Hpt_MineLauncher_Fixed_Small"",
        ""cost"": 24260,
        ""sku"": null
      },
      ""128049510"": {
        ""id"": 128049510,
        ""category"": ""weapon"",
        ""name"": ""Hpt_AdvancedTorpPylon_Fixed_Medium"",
        ""cost"": 44800,
        ""sku"": null
      },
      ""128049509"": {
        ""id"": 128049509,
        ""category"": ""weapon"",
        ""name"": ""Hpt_AdvancedTorpPylon_Fixed_Small"",
        ""cost"": 11200,
        ""sku"": null
      },
      ""128666724"": {
        ""id"": 128666724,
        ""category"": ""weapon"",
        ""name"": ""Hpt_DumbfireMissileRack_Fixed_Small"",
        ""cost"": 32175,
        ""sku"": null
      },
      ""128049462"": {
        ""id"": 128049462,
        ""category"": ""weapon"",
        ""name"": ""Hpt_MultiCannon_Turret_Small"",
        ""cost"": 81600,
        ""sku"": null
      },
      ""128049463"": {
        ""id"": 128049463,
        ""category"": ""weapon"",
        ""name"": ""Hpt_MultiCannon_Turret_Medium"",
        ""cost"": 1292800,
        ""sku"": null
      },
      ""128049459"": {
        ""id"": 128049459,
        ""category"": ""weapon"",
        ""name"": ""Hpt_MultiCannon_Gimbal_Small"",
        ""cost"": 14250,
        ""sku"": null
      },
      ""128049460"": {
        ""id"": 128049460,
        ""category"": ""weapon"",
        ""name"": ""Hpt_MultiCannon_Gimbal_Medium"",
        ""cost"": 57000,
        ""sku"": null
      },
      ""128049456"": {
        ""id"": 128049456,
        ""category"": ""weapon"",
        ""name"": ""Hpt_MultiCannon_Fixed_Medium"",
        ""cost"": 38000,
        ""sku"": null
      },
      ""128049455"": {
        ""id"": 128049455,
        ""category"": ""weapon"",
        ""name"": ""Hpt_MultiCannon_Fixed_Small"",
        ""cost"": 9500,
        ""sku"": null
      },
      ""128049445"": {
        ""id"": 128049445,
        ""category"": ""weapon"",
        ""name"": ""Hpt_Cannon_Turret_Small"",
        ""cost"": 506400,
        ""sku"": null
      },
      ""128049440"": {
        ""id"": 128049440,
        ""category"": ""weapon"",
        ""name"": ""Hpt_Cannon_Fixed_Large"",
        ""cost"": 675200,
        ""sku"": null
      },
      ""128049442"": {
        ""id"": 128049442,
        ""category"": ""weapon"",
        ""name"": ""Hpt_Cannon_Gimbal_Small"",
        ""cost"": 42200,
        ""sku"": null
      },
      ""128049443"": {
        ""id"": 128049443,
        ""category"": ""weapon"",
        ""name"": ""Hpt_Cannon_Gimbal_Medium"",
        ""cost"": 337600,
        ""sku"": null
      },
      ""128049439"": {
        ""id"": 128049439,
        ""category"": ""weapon"",
        ""name"": ""Hpt_Cannon_Fixed_Medium"",
        ""cost"": 168430,
        ""sku"": null
      },
      ""128049438"": {
        ""id"": 128049438,
        ""category"": ""weapon"",
        ""name"": ""Hpt_Cannon_Fixed_Small"",
        ""cost"": 21100,
        ""sku"": null
      },
      ""128049450"": {
        ""id"": 128049450,
        ""category"": ""weapon"",
        ""name"": ""Hpt_Slugshot_Fixed_Large"",
        ""cost"": 1167360,
        ""sku"": null
      },
      ""128049453"": {
        ""id"": 128049453,
        ""category"": ""weapon"",
        ""name"": ""Hpt_Slugshot_Turret_Small"",
        ""cost"": 182400,
        ""sku"": null
      },
      ""128049451"": {
        ""id"": 128049451,
        ""category"": ""weapon"",
        ""name"": ""Hpt_Slugshot_Gimbal_Small"",
        ""cost"": 54720,
        ""sku"": null
      },
      ""128049452"": {
        ""id"": 128049452,
        ""category"": ""weapon"",
        ""name"": ""Hpt_Slugshot_Gimbal_Medium"",
        ""cost"": 437760,
        ""sku"": null
      },
      ""128049448"": {
        ""id"": 128049448,
        ""category"": ""weapon"",
        ""name"": ""Hpt_Slugshot_Fixed_Small"",
        ""cost"": 36000,
        ""sku"": null
      },
      ""128049466"": {
        ""id"": 128049466,
        ""category"": ""weapon"",
        ""name"": ""Hpt_PlasmaAccelerator_Fixed_Large"",
        ""cost"": 3051200,
        ""sku"": null
      },
      ""128049465"": {
        ""id"": 128049465,
        ""category"": ""weapon"",
        ""name"": ""Hpt_PlasmaAccelerator_Fixed_Medium"",
        ""cost"": 834200,
        ""sku"": null
      },
      ""128049390"": {
        ""id"": 128049390,
        ""category"": ""weapon"",
        ""name"": ""Hpt_PulseLaser_Turret_Large"",
        ""cost"": 400400,
        ""sku"": null
      },
      ""128049389"": {
        ""id"": 128049389,
        ""category"": ""weapon"",
        ""name"": ""Hpt_PulseLaser_Turret_Medium"",
        ""cost"": 132800,
        ""sku"": null
      },
      ""128049388"": {
        ""id"": 128049388,
        ""category"": ""weapon"",
        ""name"": ""Hpt_PulseLaser_Turret_Small"",
        ""cost"": 26000,
        ""sku"": null
      },
      ""128049386"": {
        ""id"": 128049386,
        ""category"": ""weapon"",
        ""name"": ""Hpt_PulseLaser_Gimbal_Medium"",
        ""cost"": 35400,
        ""sku"": null
      },
      ""128049434"": {
        ""id"": 128049434,
        ""category"": ""weapon"",
        ""name"": ""Hpt_BeamLaser_Gimbal_Large"",
        ""cost"": 2396160,
        ""sku"": null
      },
      ""128049437"": {
        ""id"": 128049437,
        ""category"": ""weapon"",
        ""name"": ""Hpt_BeamLaser_Turret_Large"",
        ""cost"": 19399600,
        ""sku"": null
      },
      ""128049430"": {
        ""id"": 128049430,
        ""category"": ""weapon"",
        ""name"": ""Hpt_BeamLaser_Fixed_Large"",
        ""cost"": 1177600,
        ""sku"": null
      },
      ""128049408"": {
        ""id"": 128049408,
        ""category"": ""weapon"",
        ""name"": ""Hpt_PulseLaserBurst_Turret_Medium"",
        ""cost"": 162800,
        ""sku"": null
      },
      ""128049406"": {
        ""id"": 128049406,
        ""category"": ""weapon"",
        ""name"": ""Hpt_PulseLaserBurst_Gimbal_Large"",
        ""cost"": 281600,
        ""sku"": null
      },
      ""128049404"": {
        ""id"": 128049404,
        ""category"": ""weapon"",
        ""name"": ""Hpt_PulseLaserBurst_Gimbal_Small"",
        ""cost"": 8600,
        ""sku"": null
      },
      ""128662534"": {
        ""id"": 128662534,
        ""category"": ""utility"",
        ""name"": ""Hpt_CrimeScanner_Size0_Class5"",
        ""cost"": 1097095,
        ""sku"": null
      },
      ""128662533"": {
        ""id"": 128662533,
        ""category"": ""utility"",
        ""name"": ""Hpt_CrimeScanner_Size0_Class4"",
        ""cost"": 365698,
        ""sku"": null
      },
      ""128662532"": {
        ""id"": 128662532,
        ""category"": ""utility"",
        ""name"": ""Hpt_CrimeScanner_Size0_Class3"",
        ""cost"": 121899,
        ""sku"": null
      },
      ""128662531"": {
        ""id"": 128662531,
        ""category"": ""utility"",
        ""name"": ""Hpt_CrimeScanner_Size0_Class2"",
        ""cost"": 40633,
        ""sku"": null
      },
      ""128662530"": {
        ""id"": 128662530,
        ""category"": ""utility"",
        ""name"": ""Hpt_CrimeScanner_Size0_Class1"",
        ""cost"": 13544,
        ""sku"": null
      },
      ""128662524"": {
        ""id"": 128662524,
        ""category"": ""utility"",
        ""name"": ""Hpt_CargoScanner_Size0_Class5"",
        ""cost"": 1097095,
        ""sku"": null
      },
      ""128662523"": {
        ""id"": 128662523,
        ""category"": ""utility"",
        ""name"": ""Hpt_CargoScanner_Size0_Class4"",
        ""cost"": 365698,
        ""sku"": null
      },
      ""128662522"": {
        ""id"": 128662522,
        ""category"": ""utility"",
        ""name"": ""Hpt_CargoScanner_Size0_Class3"",
        ""cost"": 121899,
        ""sku"": null
      },
      ""128662521"": {
        ""id"": 128662521,
        ""category"": ""utility"",
        ""name"": ""Hpt_CargoScanner_Size0_Class2"",
        ""cost"": 40633,
        ""sku"": null
      },
      ""128049522"": {
        ""id"": 128049522,
        ""category"": ""utility"",
        ""name"": ""Hpt_PlasmaPointDefence_Turret_Tiny"",
        ""cost"": 18546,
        ""sku"": null
      },
      ""128049516"": {
        ""id"": 128049516,
        ""category"": ""utility"",
        ""name"": ""Hpt_ElectronicCountermeasure_Tiny"",
        ""cost"": 12500,
        ""sku"": null
      },
      ""128049513"": {
        ""id"": 128049513,
        ""category"": ""utility"",
        ""name"": ""Hpt_ChaffLauncher_Tiny"",
        ""cost"": 8500,
        ""sku"": null
      },
      ""128662527"": {
        ""id"": 128662527,
        ""category"": ""utility"",
        ""name"": ""Hpt_CloudScanner_Size0_Class3"",
        ""cost"": 121899,
        ""sku"": null
      },
      ""128662525"": {
        ""id"": 128662525,
        ""category"": ""utility"",
        ""name"": ""Hpt_CloudScanner_Size0_Class1"",
        ""cost"": 13544,
        ""sku"": null
      },
      ""128662528"": {
        ""id"": 128662528,
        ""category"": ""utility"",
        ""name"": ""Hpt_CloudScanner_Size0_Class4"",
        ""cost"": 365698,
        ""sku"": null
      },
      ""128049526"": {
        ""id"": 128049526,
        ""category"": ""utility"",
        ""name"": ""Hpt_MiningLaser_Fixed_Medium"",
        ""cost"": 22576,
        ""sku"": null
      },
      ""128049525"": {
        ""id"": 128049525,
        ""category"": ""utility"",
        ""name"": ""Hpt_MiningLaser_Fixed_Small"",
        ""cost"": 6800,
        ""sku"": null
      },
      ""128049300"": {
        ""id"": 128049300,
        ""category"": ""module"",
        ""name"": ""Type7_Armour_Grade3"",
        ""cost"": 15725026,
        ""sku"": null
      },
      ""128049299"": {
        ""id"": 128049299,
        ""category"": ""module"",
        ""name"": ""Type7_Armour_Grade2"",
        ""cost"": 6988900,
        ""sku"": null
      },
      ""128049298"": {
        ""id"": 128049298,
        ""category"": ""module"",
        ""name"": ""Type7_Armour_Grade1"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128049301"": {
        ""id"": 128049301,
        ""category"": ""module"",
        ""name"": ""Type7_Armour_Mirrored"",
        ""cost"": 37163480,
        ""sku"": null
      },
      ""128049302"": {
        ""id"": 128049302,
        ""category"": ""module"",
        ""name"": ""Type7_Armour_Reactive"",
        ""cost"": 41182097,
        ""sku"": null
      },
      ""128049282"": {
        ""id"": 128049282,
        ""category"": ""module"",
        ""name"": ""CobraMkIII_Armour_Grade3"",
        ""cost"": 314746,
        ""sku"": null
      },
      ""128049281"": {
        ""id"": 128049281,
        ""category"": ""module"",
        ""name"": ""CobraMkIII_Armour_Grade2"",
        ""cost"": 139887,
        ""sku"": null
      },
      ""128049280"": {
        ""id"": 128049280,
        ""category"": ""module"",
        ""name"": ""CobraMkIII_Armour_Grade1"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128049283"": {
        ""id"": 128049283,
        ""category"": ""module"",
        ""name"": ""CobraMkIII_Armour_Mirrored"",
        ""cost"": 734407,
        ""sku"": null
      },
      ""128049284"": {
        ""id"": 128049284,
        ""category"": ""module"",
        ""name"": ""CobraMkIII_Armour_Reactive"",
        ""cost"": 824285,
        ""sku"": null
      },
      ""128049288"": {
        ""id"": 128049288,
        ""category"": ""module"",
        ""name"": ""Type6_Armour_Grade3"",
        ""cost"": 941350,
        ""sku"": null
      },
      ""128049287"": {
        ""id"": 128049287,
        ""category"": ""module"",
        ""name"": ""Type6_Armour_Grade2"",
        ""cost"": 418378,
        ""sku"": null
      },
      ""128049286"": {
        ""id"": 128049286,
        ""category"": ""module"",
        ""name"": ""Type6_Armour_Grade1"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128049289"": {
        ""id"": 128049289,
        ""category"": ""module"",
        ""name"": ""Type6_Armour_Mirrored"",
        ""cost"": 2224725,
        ""sku"": null
      },
      ""128049290"": {
        ""id"": 128049290,
        ""category"": ""module"",
        ""name"": ""Type6_Armour_Reactive"",
        ""cost"": 2465292,
        ""sku"": null
      },
      ""128672266"": {
        ""id"": 128672266,
        ""category"": ""module"",
        ""name"": ""CobraMkIV_Armour_Grade3"",
        ""cost"": 688246,
        ""sku"": null
      },
      ""128672265"": {
        ""id"": 128672265,
        ""category"": ""module"",
        ""name"": ""CobraMkIV_Armour_Grade2"",
        ""cost"": 305887,
        ""sku"": null
      },
      ""128672264"": {
        ""id"": 128672264,
        ""category"": ""module"",
        ""name"": ""CobraMkIV_Armour_Grade1"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128672267"": {
        ""id"": 128672267,
        ""category"": ""module"",
        ""name"": ""CobraMkIV_Armour_Mirrored"",
        ""cost"": 1605907,
        ""sku"": null
      },
      ""128672268"": {
        ""id"": 128672268,
        ""category"": ""module"",
        ""name"": ""CobraMkIV_Armour_Reactive"",
        ""cost"": 1802440,
        ""sku"": null
      },
      ""128049252"": {
        ""id"": 128049252,
        ""category"": ""module"",
        ""name"": ""SideWinder_Armour_Grade3"",
        ""cost"": 80320,
        ""sku"": null
      },
      ""128049251"": {
        ""id"": 128049251,
        ""category"": ""module"",
        ""name"": ""SideWinder_Armour_Grade2"",
        ""cost"": 25600,
        ""sku"": null
      },
      ""128049250"": {
        ""id"": 128049250,
        ""category"": ""module"",
        ""name"": ""SideWinder_Armour_Grade1"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128049253"": {
        ""id"": 128049253,
        ""category"": ""module"",
        ""name"": ""SideWinder_Armour_Mirrored"",
        ""cost"": 132064,
        ""sku"": null
      },
      ""128049254"": {
        ""id"": 128049254,
        ""category"": ""module"",
        ""name"": ""SideWinder_Armour_Reactive"",
        ""cost"": 139424,
        ""sku"": null
      },
      ""128049258"": {
        ""id"": 128049258,
        ""category"": ""module"",
        ""name"": ""Eagle_Armour_Grade3"",
        ""cost"": 90048,
        ""sku"": null
      },
      ""128049257"": {
        ""id"": 128049257,
        ""category"": ""module"",
        ""name"": ""Eagle_Armour_Grade2"",
        ""cost"": 26880,
        ""sku"": null
      },
      ""128049256"": {
        ""id"": 128049256,
        ""category"": ""module"",
        ""name"": ""Eagle_Armour_Grade1"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128672273"": {
        ""id"": 128672273,
        ""category"": ""module"",
        ""name"": ""Independant_Trader_Armour_Grade3"",
        ""cost"": 2813538,
        ""sku"": null
      },
      ""128672272"": {
        ""id"": 128672272,
        ""category"": ""module"",
        ""name"": ""Independant_Trader_Armour_Grade2"",
        ""cost"": 1250461,
        ""sku"": null
      },
      ""128672271"": {
        ""id"": 128672271,
        ""category"": ""module"",
        ""name"": ""Independant_Trader_Armour_Grade1"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128672274"": {
        ""id"": 128672274,
        ""category"": ""module"",
        ""name"": ""Independant_Trader_Armour_Mirrored"",
        ""cost"": 6649329,
        ""sku"": null
      },
      ""128672275"": {
        ""id"": 128672275,
        ""category"": ""module"",
        ""name"": ""Independant_Trader_Armour_Reactive"",
        ""cost"": 7368344,
        ""sku"": null
      },
      ""128049264"": {
        ""id"": 128049264,
        ""category"": ""module"",
        ""name"": ""Hauler_Armour_Grade3"",
        ""cost"": 185047,
        ""sku"": null
      },
      ""128049263"": {
        ""id"": 128049263,
        ""category"": ""module"",
        ""name"": ""Hauler_Armour_Grade2"",
        ""cost"": 42176,
        ""sku"": null
      },
      ""128049262"": {
        ""id"": 128049262,
        ""category"": ""module"",
        ""name"": ""Hauler_Armour_Grade1"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128662535"": {
        ""id"": 128662535,
        ""category"": ""module"",
        ""name"": ""Int_StellarBodyDiscoveryScanner_Standard"",
        ""cost"": 1000,
        ""sku"": null
      },
      ""128064338"": {
        ""id"": 128064338,
        ""category"": ""module"",
        ""name"": ""Int_CargoRack_Size1_Class1"",
        ""cost"": 1000,
        ""sku"": null
      },
      ""128666684"": {
        ""id"": 128666684,
        ""category"": ""module"",
        ""name"": ""Int_Refinery_Size1_Class1"",
        ""cost"": 6000,
        ""sku"": null
      },
      ""128666644"": {
        ""id"": 128666644,
        ""category"": ""module"",
        ""name"": ""Int_FuelScoop_Size1_Class1"",
        ""cost"": 309,
        ""sku"": null
      },
      ""128666704"": {
        ""id"": 128666704,
        ""category"": ""module"",
        ""name"": ""Int_FSDInterdictor_Size1_Class1"",
        ""cost"": 12000,
        ""sku"": null
      },
      ""128066532"": {
        ""id"": 128066532,
        ""category"": ""module"",
        ""name"": ""Int_DroneControl_ResourceSiphon_Size1_Class1"",
        ""cost"": 600,
        ""sku"": null
      },
      ""128064263"": {
        ""id"": 128064263,
        ""category"": ""module"",
        ""name"": ""Int_ShieldGenerator_Size2_Class1"",
        ""cost"": 1978,
        ""sku"": null
      },
      ""128064345"": {
        ""id"": 128064345,
        ""category"": ""module"",
        ""name"": ""Int_CargoRack_Size8_Class1"",
        ""cost"": 3829866,
        ""sku"": null
      },
      ""128064344"": {
        ""id"": 128064344,
        ""category"": ""module"",
        ""name"": ""Int_CargoRack_Size7_Class1"",
        ""cost"": 1178420,
        ""sku"": null
      },
      ""128064343"": {
        ""id"": 128064343,
        ""category"": ""module"",
        ""name"": ""Int_CargoRack_Size6_Class1"",
        ""cost"": 362591,
        ""sku"": null
      },
      ""128064342"": {
        ""id"": 128064342,
        ""category"": ""module"",
        ""name"": ""Int_CargoRack_Size5_Class1"",
        ""cost"": 111566,
        ""sku"": null
      },
      ""128064341"": {
        ""id"": 128064341,
        ""category"": ""module"",
        ""name"": ""Int_CargoRack_Size4_Class1"",
        ""cost"": 34328,
        ""sku"": null
      },
      ""128064340"": {
        ""id"": 128064340,
        ""category"": ""module"",
        ""name"": ""Int_CargoRack_Size3_Class1"",
        ""cost"": 10563,
        ""sku"": null
      },
      ""128064339"": {
        ""id"": 128064339,
        ""category"": ""module"",
        ""name"": ""Int_CargoRack_Size2_Class1"",
        ""cost"": 3250,
        ""sku"": null
      },
      ""128064112"": {
        ""id"": 128064112,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size3_Class5"",
        ""cost"": 507912,
        ""sku"": null
      },
      ""128064107"": {
        ""id"": 128064107,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size2_Class5"",
        ""cost"": 160224,
        ""sku"": null
      },
      ""128064111"": {
        ""id"": 128064111,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size3_Class4"",
        ""cost"": 169304,
        ""sku"": null
      },
      ""128064110"": {
        ""id"": 128064110,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size3_Class3"",
        ""cost"": 56435,
        ""sku"": null
      },
      ""128064105"": {
        ""id"": 128064105,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size2_Class3"",
        ""cost"": 17803,
        ""sku"": null
      },
      ""128064109"": {
        ""id"": 128064109,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size3_Class2"",
        ""cost"": 18812,
        ""sku"": null
      },
      ""128064104"": {
        ""id"": 128064104,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size2_Class2"",
        ""cost"": 5934,
        ""sku"": null
      },
      ""128064108"": {
        ""id"": 128064108,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size3_Class1"",
        ""cost"": 6271,
        ""sku"": null
      },
      ""128064103"": {
        ""id"": 128064103,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size2_Class1"",
        ""cost"": 1978,
        ""sku"": null
      },
      ""128064321"": {
        ""id"": 128064321,
        ""category"": ""module"",
        ""name"": ""Int_ShieldCellBank_Size5_Class4"",
        ""cost"": 496527,
        ""sku"": null
      },
      ""128064324"": {
        ""id"": 128064324,
        ""category"": ""module"",
        ""name"": ""Int_ShieldCellBank_Size6_Class2"",
        ""cost"": 222444,
        ""sku"": null
      },
      ""128064315"": {
        ""id"": 128064315,
        ""category"": ""module"",
        ""name"": ""Int_ShieldCellBank_Size4_Class3"",
        ""cost"": 70932,
        ""sku"": null
      },
      ""128064319"": {
        ""id"": 128064319,
        ""category"": ""module"",
        ""name"": ""Int_ShieldCellBank_Size5_Class2"",
        ""cost"": 79444,
        ""sku"": null
      },
      ""128064323"": {
        ""id"": 128064323,
        ""category"": ""module"",
        ""name"": ""Int_ShieldCellBank_Size6_Class1"",
        ""cost"": 88978,
        ""sku"": null
      },
      ""128064314"": {
        ""id"": 128064314,
        ""category"": ""module"",
        ""name"": ""Int_ShieldCellBank_Size4_Class2"",
        ""cost"": 28373,
        ""sku"": null
      },
      ""128064318"": {
        ""id"": 128064318,
        ""category"": ""module"",
        ""name"": ""Int_ShieldCellBank_Size5_Class1"",
        ""cost"": 31778,
        ""sku"": null
      },
      ""128064313"": {
        ""id"": 128064313,
        ""category"": ""module"",
        ""name"": ""Int_ShieldCellBank_Size4_Class1"",
        ""cost"": 11349,
        ""sku"": null
      },
      ""128668546"": {
        ""id"": 128668546,
        ""category"": ""module"",
        ""name"": ""Int_HullReinforcement_Size5_Class2"",
        ""cost"": 450000,
        ""sku"": null
      },
      ""128668544"": {
        ""id"": 128668544,
        ""category"": ""module"",
        ""name"": ""Int_HullReinforcement_Size4_Class2"",
        ""cost"": 195000,
        ""sku"": null
      },
      ""128668542"": {
        ""id"": 128668542,
        ""category"": ""module"",
        ""name"": ""Int_HullReinforcement_Size3_Class2"",
        ""cost"": 84000,
        ""sku"": null
      },
      ""128668540"": {
        ""id"": 128668540,
        ""category"": ""module"",
        ""name"": ""Int_HullReinforcement_Size2_Class2"",
        ""cost"": 36000,
        ""sku"": null
      },
      ""128668538"": {
        ""id"": 128668538,
        ""category"": ""module"",
        ""name"": ""Int_HullReinforcement_Size1_Class2"",
        ""cost"": 15000,
        ""sku"": null
      },
      ""128064056"": {
        ""id"": 128064056,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size6_Class4"",
        ""cost"": 5393177,
        ""sku"": null
      },
      ""128064051"": {
        ""id"": 128064051,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size5_Class4"",
        ""cost"": 1701318,
        ""sku"": null
      },
      ""128064046"": {
        ""id"": 128064046,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size4_Class4"",
        ""cost"": 536693,
        ""sku"": null
      },
      ""128064055"": {
        ""id"": 128064055,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size6_Class3"",
        ""cost"": 1797726,
        ""sku"": null
      },
      ""128064054"": {
        ""id"": 128064054,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size6_Class2"",
        ""cost"": 599242,
        ""sku"": null
      },
      ""128064050"": {
        ""id"": 128064050,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size5_Class3"",
        ""cost"": 567106,
        ""sku"": null
      },
      ""128064045"": {
        ""id"": 128064045,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size4_Class3"",
        ""cost"": 178898,
        ""sku"": null
      },
      ""128064049"": {
        ""id"": 128064049,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size5_Class2"",
        ""cost"": 189035,
        ""sku"": null
      },
      ""128064053"": {
        ""id"": 128064053,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size6_Class1"",
        ""cost"": 199747,
        ""sku"": null
      },
      ""128064044"": {
        ""id"": 128064044,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size4_Class2"",
        ""cost"": 59633,
        ""sku"": null
      },
      ""128064048"": {
        ""id"": 128064048,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size5_Class1"",
        ""cost"": 63012,
        ""sku"": null
      },
      ""128064043"": {
        ""id"": 128064043,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size4_Class1"",
        ""cost"": 19878,
        ""sku"": null
      },
      ""128064042"": {
        ""id"": 128064042,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size3_Class5"",
        ""cost"": 507912,
        ""sku"": null
      },
      ""128064037"": {
        ""id"": 128064037,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size2_Class5"",
        ""cost"": 160224,
        ""sku"": null
      },
      ""128064041"": {
        ""id"": 128064041,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size3_Class4"",
        ""cost"": 169304,
        ""sku"": null
      },
      ""128064036"": {
        ""id"": 128064036,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size2_Class4"",
        ""cost"": 53408,
        ""sku"": null
      },
      ""128064040"": {
        ""id"": 128064040,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size3_Class3"",
        ""cost"": 56435,
        ""sku"": null
      },
      ""128064035"": {
        ""id"": 128064035,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size2_Class3"",
        ""cost"": 17803,
        ""sku"": null
      },
      ""128064039"": {
        ""id"": 128064039,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size3_Class2"",
        ""cost"": 18812,
        ""sku"": null
      },
      ""128064034"": {
        ""id"": 128064034,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size2_Class2"",
        ""cost"": 5934,
        ""sku"": null
      },
      ""128064038"": {
        ""id"": 128064038,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size3_Class1"",
        ""cost"": 6271,
        ""sku"": null
      },
      ""128064033"": {
        ""id"": 128064033,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size2_Class1"",
        ""cost"": 1978,
        ""sku"": null
      },
      ""128668536"": {
        ""id"": 128668536,
        ""category"": ""module"",
        ""name"": ""Hpt_ShieldBooster_Size0_Class5"",
        ""cost"": 281000,
        ""sku"": null
      },
      ""128668535"": {
        ""id"": 128668535,
        ""category"": ""module"",
        ""name"": ""Hpt_ShieldBooster_Size0_Class4"",
        ""cost"": 122000,
        ""sku"": null
      },
      ""128668534"": {
        ""id"": 128668534,
        ""category"": ""module"",
        ""name"": ""Hpt_ShieldBooster_Size0_Class3"",
        ""cost"": 53000,
        ""sku"": null
      },
      ""128668533"": {
        ""id"": 128668533,
        ""category"": ""module"",
        ""name"": ""Hpt_ShieldBooster_Size0_Class2"",
        ""cost"": 23000,
        ""sku"": null
      },
      ""128668532"": {
        ""id"": 128668532,
        ""category"": ""module"",
        ""name"": ""Hpt_ShieldBooster_Size0_Class1"",
        ""cost"": 10000,
        ""sku"": null
      },
      ""128064126"": {
        ""id"": 128064126,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size6_Class4"",
        ""cost"": 5393177,
        ""sku"": null
      },
      ""128064121"": {
        ""id"": 128064121,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size5_Class4"",
        ""cost"": 1701318,
        ""sku"": null
      },
      ""128064116"": {
        ""id"": 128064116,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size4_Class4"",
        ""cost"": 536693,
        ""sku"": null
      },
      ""128064125"": {
        ""id"": 128064125,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size6_Class3"",
        ""cost"": 1797726,
        ""sku"": null
      },
      ""128064124"": {
        ""id"": 128064124,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size6_Class2"",
        ""cost"": 599242,
        ""sku"": null
      },
      ""128064115"": {
        ""id"": 128064115,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size4_Class3"",
        ""cost"": 178898,
        ""sku"": null
      },
      ""128064119"": {
        ""id"": 128064119,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size5_Class2"",
        ""cost"": 189035,
        ""sku"": null
      },
      ""128064123"": {
        ""id"": 128064123,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size6_Class1"",
        ""cost"": 199747,
        ""sku"": null
      },
      ""128064114"": {
        ""id"": 128064114,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size4_Class2"",
        ""cost"": 59633,
        ""sku"": null
      },
      ""128064118"": {
        ""id"": 128064118,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size5_Class1"",
        ""cost"": 63012,
        ""sku"": null
      },
      ""128064113"": {
        ""id"": 128064113,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size4_Class1"",
        ""cost"": 19878,
        ""sku"": null
      },
      ""128064272"": {
        ""id"": 128064272,
        ""category"": ""module"",
        ""name"": ""Int_ShieldGenerator_Size3_Class5"",
        ""cost"": 507912,
        ""sku"": null
      },
      ""128064271"": {
        ""id"": 128064271,
        ""category"": ""module"",
        ""name"": ""Int_ShieldGenerator_Size3_Class4"",
        ""cost"": 169304,
        ""sku"": null
      },
      ""128064266"": {
        ""id"": 128064266,
        ""category"": ""module"",
        ""name"": ""Int_ShieldGenerator_Size2_Class4"",
        ""cost"": 53408,
        ""sku"": null
      },
      ""128064270"": {
        ""id"": 128064270,
        ""category"": ""module"",
        ""name"": ""Int_ShieldGenerator_Size3_Class3"",
        ""cost"": 56435,
        ""sku"": null
      },
      ""128064265"": {
        ""id"": 128064265,
        ""category"": ""module"",
        ""name"": ""Int_ShieldGenerator_Size2_Class3"",
        ""cost"": 17803,
        ""sku"": null
      },
      ""128064269"": {
        ""id"": 128064269,
        ""category"": ""module"",
        ""name"": ""Int_ShieldGenerator_Size3_Class2"",
        ""cost"": 18812,
        ""sku"": null
      },
      ""128064264"": {
        ""id"": 128064264,
        ""category"": ""module"",
        ""name"": ""Int_ShieldGenerator_Size2_Class2"",
        ""cost"": 5934,
        ""sku"": null
      },
      ""128064268"": {
        ""id"": 128064268,
        ""category"": ""module"",
        ""name"": ""Int_ShieldGenerator_Size3_Class1"",
        ""cost"": 6271,
        ""sku"": null
      },
      ""128666697"": {
        ""id"": 128666697,
        ""category"": ""module"",
        ""name"": ""Int_Refinery_Size2_Class4"",
        ""cost"": 340200,
        ""sku"": null
      },
      ""128666700"": {
        ""id"": 128666700,
        ""category"": ""module"",
        ""name"": ""Int_Refinery_Size1_Class5"",
        ""cost"": 486000,
        ""sku"": null
      },
      ""128666693"": {
        ""id"": 128666693,
        ""category"": ""module"",
        ""name"": ""Int_Refinery_Size2_Class3"",
        ""cost"": 113400,
        ""sku"": null
      },
      ""128666689"": {
        ""id"": 128666689,
        ""category"": ""module"",
        ""name"": ""Int_Refinery_Size2_Class2"",
        ""cost"": 37800,
        ""sku"": null
      },
      ""128666692"": {
        ""id"": 128666692,
        ""category"": ""module"",
        ""name"": ""Int_Refinery_Size1_Class3"",
        ""cost"": 54000,
        ""sku"": null
      },
      ""128666688"": {
        ""id"": 128666688,
        ""category"": ""module"",
        ""name"": ""Int_Refinery_Size1_Class2"",
        ""cost"": 18000,
        ""sku"": null
      },
      ""128666685"": {
        ""id"": 128666685,
        ""category"": ""module"",
        ""name"": ""Int_Refinery_Size2_Class1"",
        ""cost"": 12600,
        ""sku"": null
      },
      ""128064255"": {
        ""id"": 128064255,
        ""category"": ""module"",
        ""name"": ""Int_Sensors_Size8_Class3"",
        ""cost"": 4359903,
        ""sku"": null
      },
      ""128064250"": {
        ""id"": 128064250,
        ""category"": ""module"",
        ""name"": ""Int_Sensors_Size7_Class3"",
        ""cost"": 1557108,
        ""sku"": null
      },
      ""128064254"": {
        ""id"": 128064254,
        ""category"": ""module"",
        ""name"": ""Int_Sensors_Size8_Class2"",
        ""cost"": 1743961,
        ""sku"": null
      },
      ""128064249"": {
        ""id"": 128064249,
        ""category"": ""module"",
        ""name"": ""Int_Sensors_Size7_Class2"",
        ""cost"": 622843,
        ""sku"": null
      },
      ""128064253"": {
        ""id"": 128064253,
        ""category"": ""module"",
        ""name"": ""Int_Sensors_Size8_Class1"",
        ""cost"": 697584,
        ""sku"": null
      },
      ""128064248"": {
        ""id"": 128064248,
        ""category"": ""module"",
        ""name"": ""Int_Sensors_Size7_Class1"",
        ""cost"": 249137,
        ""sku"": null
      },
      ""128064072"": {
        ""id"": 128064072,
        ""category"": ""module"",
        ""name"": ""Int_Engine_Size2_Class5"",
        ""cost"": 160224,
        ""sku"": null
      },
      ""128064076"": {
        ""id"": 128064076,
        ""category"": ""module"",
        ""name"": ""Int_Engine_Size3_Class4"",
        ""cost"": 169304,
        ""sku"": null
      },
      ""128064071"": {
        ""id"": 128064071,
        ""category"": ""module"",
        ""name"": ""Int_Engine_Size2_Class4"",
        ""cost"": 53408,
        ""sku"": null
      },
      ""128064075"": {
        ""id"": 128064075,
        ""category"": ""module"",
        ""name"": ""Int_Engine_Size3_Class3"",
        ""cost"": 56435,
        ""sku"": null
      },
      ""128064070"": {
        ""id"": 128064070,
        ""category"": ""module"",
        ""name"": ""Int_Engine_Size2_Class3"",
        ""cost"": 17803,
        ""sku"": null
      },
      ""128064074"": {
        ""id"": 128064074,
        ""category"": ""module"",
        ""name"": ""Int_Engine_Size3_Class2"",
        ""cost"": 18812,
        ""sku"": null
      },
      ""128064069"": {
        ""id"": 128064069,
        ""category"": ""module"",
        ""name"": ""Int_Engine_Size2_Class2"",
        ""cost"": 5934,
        ""sku"": null
      },
      ""128064073"": {
        ""id"": 128064073,
        ""category"": ""module"",
        ""name"": ""Int_Engine_Size3_Class1"",
        ""cost"": 6271,
        ""sku"": null
      },
      ""128064068"": {
        ""id"": 128064068,
        ""category"": ""module"",
        ""name"": ""Int_Engine_Size2_Class1"",
        ""cost"": 1978,
        ""sku"": null
      },
      ""128064311"": {
        ""id"": 128064311,
        ""category"": ""module"",
        ""name"": ""Int_ShieldCellBank_Size3_Class4"",
        ""cost"": 63333,
        ""sku"": null
      },
      ""128064301"": {
        ""id"": 128064301,
        ""category"": ""module"",
        ""name"": ""Int_ShieldCellBank_Size1_Class4"",
        ""cost"": 8078,
        ""sku"": null
      },
      ""128064310"": {
        ""id"": 128064310,
        ""category"": ""module"",
        ""name"": ""Int_ShieldCellBank_Size3_Class3"",
        ""cost"": 25333,
        ""sku"": null
      },
      ""128064305"": {
        ""id"": 128064305,
        ""category"": ""module"",
        ""name"": ""Int_ShieldCellBank_Size2_Class3"",
        ""cost"": 9048,
        ""sku"": null
      },
      ""128064300"": {
        ""id"": 128064300,
        ""category"": ""module"",
        ""name"": ""Int_ShieldCellBank_Size1_Class3"",
        ""cost"": 3231,
        ""sku"": null
      },
      ""128064309"": {
        ""id"": 128064309,
        ""category"": ""module"",
        ""name"": ""Int_ShieldCellBank_Size3_Class2"",
        ""cost"": 10133,
        ""sku"": null
      },
      ""128064299"": {
        ""id"": 128064299,
        ""category"": ""module"",
        ""name"": ""Int_ShieldCellBank_Size1_Class2"",
        ""cost"": 1293,
        ""sku"": null
      },
      ""128064304"": {
        ""id"": 128064304,
        ""category"": ""module"",
        ""name"": ""Int_ShieldCellBank_Size2_Class2"",
        ""cost"": 3619,
        ""sku"": null
      },
      ""128064308"": {
        ""id"": 128064308,
        ""category"": ""module"",
        ""name"": ""Int_ShieldCellBank_Size3_Class1"",
        ""cost"": 4053,
        ""sku"": null
      },
      ""128064303"": {
        ""id"": 128064303,
        ""category"": ""module"",
        ""name"": ""Int_ShieldCellBank_Size2_Class1"",
        ""cost"": 1448,
        ""sku"": null
      },
      ""128064298"": {
        ""id"": 128064298,
        ""category"": ""module"",
        ""name"": ""Int_ShieldCellBank_Size1_Class1"",
        ""cost"": 517,
        ""sku"": null
      },
      ""128666678"": {
        ""id"": 128666678,
        ""category"": ""module"",
        ""name"": ""Int_FuelScoop_Size3_Class5"",
        ""cost"": 902954,
        ""sku"": null
      },
      ""128666677"": {
        ""id"": 128666677,
        ""category"": ""module"",
        ""name"": ""Int_FuelScoop_Size2_Class5"",
        ""cost"": 284844,
        ""sku"": null
      },
      ""128666670"": {
        ""id"": 128666670,
        ""category"": ""module"",
        ""name"": ""Int_FuelScoop_Size3_Class4"",
        ""cost"": 225738,
        ""sku"": null
      },
      ""128666662"": {
        ""id"": 128666662,
        ""category"": ""module"",
        ""name"": ""Int_FuelScoop_Size3_Class3"",
        ""cost"": 56435,
        ""sku"": null
      },
      ""128666676"": {
        ""id"": 128666676,
        ""category"": ""module"",
        ""name"": ""Int_FuelScoop_Size1_Class5"",
        ""cost"": 82270,
        ""sku"": null
      },
      ""128666668"": {
        ""id"": 128666668,
        ""category"": ""module"",
        ""name"": ""Int_FuelScoop_Size1_Class4"",
        ""cost"": 20568,
        ""sku"": null
      },
      ""128666661"": {
        ""id"": 128666661,
        ""category"": ""module"",
        ""name"": ""Int_FuelScoop_Size2_Class3"",
        ""cost"": 17803,
        ""sku"": null
      },
      ""128666660"": {
        ""id"": 128666660,
        ""category"": ""module"",
        ""name"": ""Int_FuelScoop_Size1_Class3"",
        ""cost"": 5142,
        ""sku"": null
      },
      ""128666654"": {
        ""id"": 128666654,
        ""category"": ""module"",
        ""name"": ""Int_FuelScoop_Size3_Class2"",
        ""cost"": 14109,
        ""sku"": null
      },
      ""128666653"": {
        ""id"": 128666653,
        ""category"": ""module"",
        ""name"": ""Int_FuelScoop_Size2_Class2"",
        ""cost"": 4451,
        ""sku"": null
      },
      ""128666646"": {
        ""id"": 128666646,
        ""category"": ""module"",
        ""name"": ""Int_FuelScoop_Size3_Class1"",
        ""cost"": 3386,
        ""sku"": null
      },
      ""128666703"": {
        ""id"": 128666703,
        ""category"": ""module"",
        ""name"": ""Int_Refinery_Size4_Class5"",
        ""cost"": 4500846,
        ""sku"": null
      },
      ""128666699"": {
        ""id"": 128666699,
        ""category"": ""module"",
        ""name"": ""Int_Refinery_Size4_Class4"",
        ""cost"": 1500282,
        ""sku"": null
      },
      ""128666695"": {
        ""id"": 128666695,
        ""category"": ""module"",
        ""name"": ""Int_Refinery_Size4_Class3"",
        ""cost"": 500094,
        ""sku"": null
      },
      ""128666702"": {
        ""id"": 128666702,
        ""category"": ""module"",
        ""name"": ""Int_Refinery_Size3_Class5"",
        ""cost"": 2143260,
        ""sku"": null
      },
      ""128666698"": {
        ""id"": 128666698,
        ""category"": ""module"",
        ""name"": ""Int_Refinery_Size3_Class4"",
        ""cost"": 714420,
        ""sku"": null
      },
      ""128666694"": {
        ""id"": 128666694,
        ""category"": ""module"",
        ""name"": ""Int_Refinery_Size3_Class3"",
        ""cost"": 238140,
        ""sku"": null
      },
      ""128666691"": {
        ""id"": 128666691,
        ""category"": ""module"",
        ""name"": ""Int_Refinery_Size4_Class2"",
        ""cost"": 166698,
        ""sku"": null
      },
      ""128666687"": {
        ""id"": 128666687,
        ""category"": ""module"",
        ""name"": ""Int_Refinery_Size4_Class1"",
        ""cost"": 55566,
        ""sku"": null
      },
      ""128666690"": {
        ""id"": 128666690,
        ""category"": ""module"",
        ""name"": ""Int_Refinery_Size3_Class2"",
        ""cost"": 79380,
        ""sku"": null
      },
      ""128666686"": {
        ""id"": 128666686,
        ""category"": ""module"",
        ""name"": ""Int_Refinery_Size3_Class1"",
        ""cost"": 26460,
        ""sku"": null
      },
      ""128667629"": {
        ""id"": 128667629,
        ""category"": ""module"",
        ""name"": ""Int_Repairer_Size8_Class4"",
        ""cost"": 16529941,
        ""sku"": null
      },
      ""128667621"": {
        ""id"": 128667621,
        ""category"": ""module"",
        ""name"": ""Int_Repairer_Size8_Class3"",
        ""cost"": 5509980,
        ""sku"": null
      },
      ""128667628"": {
        ""id"": 128667628,
        ""category"": ""module"",
        ""name"": ""Int_Repairer_Size7_Class4"",
        ""cost"": 9183300,
        ""sku"": null
      },
      ""128667620"": {
        ""id"": 128667620,
        ""category"": ""module"",
        ""name"": ""Int_Repairer_Size7_Class3"",
        ""cost"": 3061100,
        ""sku"": null
      },
      ""128667627"": {
        ""id"": 128667627,
        ""category"": ""module"",
        ""name"": ""Int_Repairer_Size6_Class4"",
        ""cost"": 5101834,
        ""sku"": null
      },
      ""128667633"": {
        ""id"": 128667633,
        ""category"": ""module"",
        ""name"": ""Int_Repairer_Size4_Class5"",
        ""cost"": 4723920,
        ""sku"": null
      },
      ""128667626"": {
        ""id"": 128667626,
        ""category"": ""module"",
        ""name"": ""Int_Repairer_Size5_Class4"",
        ""cost"": 2834352,
        ""sku"": null
      },
      ""128667613"": {
        ""id"": 128667613,
        ""category"": ""module"",
        ""name"": ""Int_Repairer_Size8_Class2"",
        ""cost"": 1836660,
        ""sku"": null
      },
      ""128667619"": {
        ""id"": 128667619,
        ""category"": ""module"",
        ""name"": ""Int_Repairer_Size6_Class3"",
        ""cost"": 1700611,
        ""sku"": null
      },
      ""128667612"": {
        ""id"": 128667612,
        ""category"": ""module"",
        ""name"": ""Int_Repairer_Size7_Class2"",
        ""cost"": 1020367,
        ""sku"": null
      },
      ""128667605"": {
        ""id"": 128667605,
        ""category"": ""module"",
        ""name"": ""Int_Repairer_Size8_Class1"",
        ""cost"": 612220,
        ""sku"": null
      },
      ""128667611"": {
        ""id"": 128667611,
        ""category"": ""module"",
        ""name"": ""Int_Repairer_Size6_Class2"",
        ""cost"": 566870,
        ""sku"": null
      },
      ""128667604"": {
        ""id"": 128667604,
        ""category"": ""module"",
        ""name"": ""Int_Repairer_Size7_Class1"",
        ""cost"": 340122,
        ""sku"": null
      },
      ""128667610"": {
        ""id"": 128667610,
        ""category"": ""module"",
        ""name"": ""Int_Repairer_Size5_Class2"",
        ""cost"": 314928,
        ""sku"": null
      },
      ""128667616"": {
        ""id"": 128667616,
        ""category"": ""module"",
        ""name"": ""Int_Repairer_Size3_Class3"",
        ""cost"": 291600,
        ""sku"": null
      },
      ""128667623"": {
        ""id"": 128667623,
        ""category"": ""module"",
        ""name"": ""Int_Repairer_Size2_Class4"",
        ""cost"": 486000,
        ""sku"": null
      },
      ""128667615"": {
        ""id"": 128667615,
        ""category"": ""module"",
        ""name"": ""Int_Repairer_Size2_Class3"",
        ""cost"": 162000,
        ""sku"": null
      },
      ""128667622"": {
        ""id"": 128667622,
        ""category"": ""module"",
        ""name"": ""Int_Repairer_Size1_Class4"",
        ""cost"": 270000,
        ""sku"": null
      },
      ""128667609"": {
        ""id"": 128667609,
        ""category"": ""module"",
        ""name"": ""Int_Repairer_Size4_Class2"",
        ""cost"": 174960,
        ""sku"": null
      },
      ""128667603"": {
        ""id"": 128667603,
        ""category"": ""module"",
        ""name"": ""Int_Repairer_Size6_Class1"",
        ""cost"": 188957,
        ""sku"": null
      },
      ""128671272"": {
        ""id"": 128671272,
        ""category"": ""module"",
        ""name"": ""Int_DroneControl_Prospector_Size1_Class4"",
        ""cost"": 4800,
        ""sku"": null
      },
      ""128671284"": {
        ""id"": 128671284,
        ""category"": ""module"",
        ""name"": ""Int_DroneControl_Prospector_Size7_Class1"",
        ""cost"": 437400,
        ""sku"": null
      },
      ""128671280"": {
        ""id"": 128671280,
        ""category"": ""module"",
        ""name"": ""Int_DroneControl_Prospector_Size5_Class2"",
        ""cost"": 97200,
        ""sku"": null
      },
      ""128671276"": {
        ""id"": 128671276,
        ""category"": ""module"",
        ""name"": ""Int_DroneControl_Prospector_Size3_Class3"",
        ""cost"": 21600,
        ""sku"": null
      },
      ""128671271"": {
        ""id"": 128671271,
        ""category"": ""module"",
        ""name"": ""Int_DroneControl_Prospector_Size1_Class3"",
        ""cost"": 2400,
        ""sku"": null
      },
      ""128671279"": {
        ""id"": 128671279,
        ""category"": ""module"",
        ""name"": ""Int_DroneControl_Prospector_Size5_Class1"",
        ""cost"": 48600,
        ""sku"": null
      },
      ""128671275"": {
        ""id"": 128671275,
        ""category"": ""module"",
        ""name"": ""Int_DroneControl_Prospector_Size3_Class2"",
        ""cost"": 10800,
        ""sku"": null
      },
      ""128671270"": {
        ""id"": 128671270,
        ""category"": ""module"",
        ""name"": ""Int_DroneControl_Prospector_Size1_Class2"",
        ""cost"": 1200,
        ""sku"": null
      },
      ""128671274"": {
        ""id"": 128671274,
        ""category"": ""module"",
        ""name"": ""Int_DroneControl_Prospector_Size3_Class1"",
        ""cost"": 5400,
        ""sku"": null
      },
      ""128666675"": {
        ""id"": 128666675,
        ""category"": ""module"",
        ""name"": ""Int_FuelScoop_Size8_Class4"",
        ""cost"": 72260660,
        ""sku"": null
      },
      ""128666682"": {
        ""id"": 128666682,
        ""category"": ""module"",
        ""name"": ""Int_FuelScoop_Size7_Class5"",
        ""cost"": 91180644,
        ""sku"": null
      },
      ""128666674"": {
        ""id"": 128666674,
        ""category"": ""module"",
        ""name"": ""Int_FuelScoop_Size7_Class4"",
        ""cost"": 22795161,
        ""sku"": null
      },
      ""128666667"": {
        ""id"": 128666667,
        ""category"": ""module"",
        ""name"": ""Int_FuelScoop_Size8_Class3"",
        ""cost"": 18065165,
        ""sku"": null
      },
      ""128666666"": {
        ""id"": 128666666,
        ""category"": ""module"",
        ""name"": ""Int_FuelScoop_Size7_Class3"",
        ""cost"": 5698790,
        ""sku"": null
      },
      ""128666659"": {
        ""id"": 128666659,
        ""category"": ""module"",
        ""name"": ""Int_FuelScoop_Size8_Class2"",
        ""cost"": 4516291,
        ""sku"": null
      },
      ""128666658"": {
        ""id"": 128666658,
        ""category"": ""module"",
        ""name"": ""Int_FuelScoop_Size7_Class2"",
        ""cost"": 1424698,
        ""sku"": null
      },
      ""128666651"": {
        ""id"": 128666651,
        ""category"": ""module"",
        ""name"": ""Int_FuelScoop_Size8_Class1"",
        ""cost"": 1083910,
        ""sku"": null
      },
      ""128666650"": {
        ""id"": 128666650,
        ""category"": ""module"",
        ""name"": ""Int_FuelScoop_Size7_Class1"",
        ""cost"": 341927,
        ""sku"": null
      },
      ""128668545"": {
        ""id"": 128668545,
        ""category"": ""module"",
        ""name"": ""Int_HullReinforcement_Size5_Class1"",
        ""cost"": 150000,
        ""sku"": null
      },
      ""128668543"": {
        ""id"": 128668543,
        ""category"": ""module"",
        ""name"": ""Int_HullReinforcement_Size4_Class1"",
        ""cost"": 65000,
        ""sku"": null
      },
      ""128668539"": {
        ""id"": 128668539,
        ""category"": ""module"",
        ""name"": ""Int_HullReinforcement_Size2_Class1"",
        ""cost"": 12000,
        ""sku"": null
      },
      ""128668537"": {
        ""id"": 128668537,
        ""category"": ""module"",
        ""name"": ""Int_HullReinforcement_Size1_Class1"",
        ""cost"": 5000,
        ""sku"": null
      },
      ""128064231"": {
        ""id"": 128064231,
        ""category"": ""module"",
        ""name"": ""Int_Sensors_Size3_Class4"",
        ""cost"": 63333,
        ""sku"": null
      },
      ""128064221"": {
        ""id"": 128064221,
        ""category"": ""module"",
        ""name"": ""Int_Sensors_Size1_Class4"",
        ""cost"": 8078,
        ""sku"": null
      },
      ""128064230"": {
        ""id"": 128064230,
        ""category"": ""module"",
        ""name"": ""Int_Sensors_Size3_Class3"",
        ""cost"": 25333,
        ""sku"": null
      },
      ""128064225"": {
        ""id"": 128064225,
        ""category"": ""module"",
        ""name"": ""Int_Sensors_Size2_Class3"",
        ""cost"": 9048,
        ""sku"": null
      },
      ""128064220"": {
        ""id"": 128064220,
        ""category"": ""module"",
        ""name"": ""Int_Sensors_Size1_Class3"",
        ""cost"": 3231,
        ""sku"": null
      },
      ""128064229"": {
        ""id"": 128064229,
        ""category"": ""module"",
        ""name"": ""Int_Sensors_Size3_Class2"",
        ""cost"": 10133,
        ""sku"": null
      },
      ""128064219"": {
        ""id"": 128064219,
        ""category"": ""module"",
        ""name"": ""Int_Sensors_Size1_Class2"",
        ""cost"": 1293,
        ""sku"": null
      },
      ""128064224"": {
        ""id"": 128064224,
        ""category"": ""module"",
        ""name"": ""Int_Sensors_Size2_Class2"",
        ""cost"": 3619,
        ""sku"": null
      },
      ""128064135"": {
        ""id"": 128064135,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size8_Class3"",
        ""cost"": 18065165,
        ""sku"": null
      },
      ""128064130"": {
        ""id"": 128064130,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size7_Class3"",
        ""cost"": 5698790,
        ""sku"": null
      },
      ""128064134"": {
        ""id"": 128064134,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size8_Class2"",
        ""cost"": 6021722,
        ""sku"": null
      },
      ""128064129"": {
        ""id"": 128064129,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size7_Class2"",
        ""cost"": 1899597,
        ""sku"": null
      },
      ""128064133"": {
        ""id"": 128064133,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size8_Class1"",
        ""cost"": 2007241,
        ""sku"": null
      },
      ""128064128"": {
        ""id"": 128064128,
        ""category"": ""module"",
        ""name"": ""Int_Hyperdrive_Size7_Class1"",
        ""cost"": 633199,
        ""sku"": null
      },
      ""128064065"": {
        ""id"": 128064065,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size8_Class3"",
        ""cost"": 18065165,
        ""sku"": null
      },
      ""128064060"": {
        ""id"": 128064060,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size7_Class3"",
        ""cost"": 5698790,
        ""sku"": null
      },
      ""128064064"": {
        ""id"": 128064064,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size8_Class2"",
        ""cost"": 6021722,
        ""sku"": null
      },
      ""128064059"": {
        ""id"": 128064059,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size7_Class2"",
        ""cost"": 1899597,
        ""sku"": null
      },
      ""128064063"": {
        ""id"": 128064063,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size8_Class1"",
        ""cost"": 2007241,
        ""sku"": null
      },
      ""128064058"": {
        ""id"": 128064058,
        ""category"": ""module"",
        ""name"": ""Int_Powerplant_Size7_Class1"",
        ""cost"": 633199,
        ""sku"": null
      },
      ""128066545"": {
        ""id"": 128066545,
        ""category"": ""module"",
        ""name"": ""Int_DroneControl_ResourceSiphon_Size5_Class4"",
        ""cost"": 388800,
        ""sku"": null
      },
      ""128066548"": {
        ""id"": 128066548,
        ""category"": ""module"",
        ""name"": ""Int_DroneControl_ResourceSiphon_Size7_Class2"",
        ""cost"": 874800,
        ""sku"": null
      },
      ""128066544"": {
        ""id"": 128066544,
        ""category"": ""module"",
        ""name"": ""Int_DroneControl_ResourceSiphon_Size5_Class3"",
        ""cost"": 194400,
        ""sku"": null
      },
      ""128066547"": {
        ""id"": 128066547,
        ""category"": ""module"",
        ""name"": ""Int_DroneControl_ResourceSiphon_Size7_Class1"",
        ""cost"": 437400,
        ""sku"": null
      },
      ""128066543"": {
        ""id"": 128066543,
        ""category"": ""module"",
        ""name"": ""Int_DroneControl_ResourceSiphon_Size5_Class2"",
        ""cost"": 97200,
        ""sku"": null
      },
      ""128066542"": {
        ""id"": 128066542,
        ""category"": ""module"",
        ""name"": ""Int_DroneControl_ResourceSiphon_Size5_Class1"",
        ""cost"": 48600,
        ""sku"": null
      },
      ""128064206"": {
        ""id"": 128064206,
        ""category"": ""module"",
        ""name"": ""Int_PowerDistributor_Size6_Class4"",
        ""cost"": 1390275,
        ""sku"": null
      },
      ""128064201"": {
        ""id"": 128064201,
        ""category"": ""module"",
        ""name"": ""Int_PowerDistributor_Size5_Class4"",
        ""cost"": 496527,
        ""sku"": null
      },
      ""128064196"": {
        ""id"": 128064196,
        ""category"": ""module"",
        ""name"": ""Int_PowerDistributor_Size4_Class4"",
        ""cost"": 177331,
        ""sku"": null
      },
      ""128064205"": {
        ""id"": 128064205,
        ""category"": ""module"",
        ""name"": ""Int_PowerDistributor_Size6_Class3"",
        ""cost"": 556110,
        ""sku"": null
      },
      ""128064204"": {
        ""id"": 128064204,
        ""category"": ""module"",
        ""name"": ""Int_PowerDistributor_Size6_Class2"",
        ""cost"": 222444,
        ""sku"": null
      },
      ""128064200"": {
        ""id"": 128064200,
        ""category"": ""module"",
        ""name"": ""Int_PowerDistributor_Size5_Class3"",
        ""cost"": 198611,
        ""sku"": null
      },
      ""128064166"": {
        ""id"": 128064166,
        ""category"": ""module"",
        ""name"": ""Int_LifeSupport_Size6_Class4"",
        ""cost"": 1390275,
        ""sku"": null
      },
      ""128064161"": {
        ""id"": 128064161,
        ""category"": ""module"",
        ""name"": ""Int_LifeSupport_Size5_Class4"",
        ""cost"": 496527,
        ""sku"": null
      },
      ""128064156"": {
        ""id"": 128064156,
        ""category"": ""module"",
        ""name"": ""Int_LifeSupport_Size4_Class4"",
        ""cost"": 177331,
        ""sku"": null
      },
      ""128064164"": {
        ""id"": 128064164,
        ""category"": ""module"",
        ""name"": ""Int_LifeSupport_Size6_Class2"",
        ""cost"": 222444,
        ""sku"": null
      },
      ""128064160"": {
        ""id"": 128064160,
        ""category"": ""module"",
        ""name"": ""Int_LifeSupport_Size5_Class3"",
        ""cost"": 198611,
        ""sku"": null
      },
      ""128666709"": {
        ""id"": 128666709,
        ""category"": ""module"",
        ""name"": ""Int_FSDInterdictor_Size2_Class2"",
        ""cost"": 100800,
        ""sku"": null
      },
      ""128666708"": {
        ""id"": 128666708,
        ""category"": ""module"",
        ""name"": ""Int_FSDInterdictor_Size1_Class2"",
        ""cost"": 36000,
        ""sku"": null
      },
      ""128666705"": {
        ""id"": 128666705,
        ""category"": ""module"",
        ""name"": ""Int_FSDInterdictor_Size2_Class1"",
        ""cost"": 33600,
        ""sku"": null
      },
      ""128671248"": {
        ""id"": 128671248,
        ""category"": ""module"",
        ""name"": ""Int_DroneControl_Collection_Size7_Class5"",
        ""cost"": 6998400,
        ""sku"": null
      },
      ""128671238"": {
        ""id"": 128671238,
        ""category"": ""module"",
        ""name"": ""Int_DroneControl_Collection_Size3_Class5"",
        ""cost"": 86400,
        ""sku"": null
      },
      ""128671233"": {
        ""id"": 128671233,
        ""category"": ""module"",
        ""name"": ""Int_DroneControl_Collection_Size1_Class5"",
        ""cost"": 9600,
        ""sku"": null
      },
      ""128671247"": {
        ""id"": 128671247,
        ""category"": ""module"",
        ""name"": ""Int_DroneControl_Collection_Size7_Class4"",
        ""cost"": 3499200,
        ""sku"": null
      },
      ""128064086"": {
        ""id"": 128064086,
        ""category"": ""module"",
        ""name"": ""Int_Engine_Size5_Class4"",
        ""cost"": 1701318,
        ""sku"": null
      },
      ""128064081"": {
        ""id"": 128064081,
        ""category"": ""module"",
        ""name"": ""Int_Engine_Size4_Class4"",
        ""cost"": 536693,
        ""sku"": null
      },
      ""128064090"": {
        ""id"": 128064090,
        ""category"": ""module"",
        ""name"": ""Int_Engine_Size6_Class3"",
        ""cost"": 1797726,
        ""sku"": null
      },
      ""128064089"": {
        ""id"": 128064089,
        ""category"": ""module"",
        ""name"": ""Int_Engine_Size6_Class2"",
        ""cost"": 599242,
        ""sku"": null
      },
      ""128064353"": {
        ""id"": 128064353,
        ""category"": ""module"",
        ""name"": ""Int_FuelTank_Size8_Class3"",
        ""cost"": 5428429,
        ""sku"": null
      },
      ""128064352"": {
        ""id"": 128064352,
        ""category"": ""module"",
        ""name"": ""Int_FuelTank_Size7_Class3"",
        ""cost"": 1780914,
        ""sku"": null
      },
      ""128064351"": {
        ""id"": 128064351,
        ""category"": ""module"",
        ""name"": ""Int_FuelTank_Size6_Class3"",
        ""cost"": 341577,
        ""sku"": null
      },
      ""128064286"": {
        ""id"": 128064286,
        ""category"": ""module"",
        ""name"": ""Int_ShieldGenerator_Size6_Class4"",
        ""cost"": 5393177,
        ""sku"": null
      },
      ""128064285"": {
        ""id"": 128064285,
        ""category"": ""module"",
        ""name"": ""Int_ShieldGenerator_Size6_Class3"",
        ""cost"": 1797726,
        ""sku"": null
      },
      ""128663561"": {
        ""id"": 128663561,
        ""category"": ""module"",
        ""name"": ""Int_StellarBodyDiscoveryScanner_Advanced"",
        ""cost"": 1545000,
        ""sku"": null
      },
      ""128663560"": {
        ""id"": 128663560,
        ""category"": ""module"",
        ""name"": ""Int_StellarBodyDiscoveryScanner_Intermediate"",
        ""cost"": 505000,
        ""sku"": null
      },
      ""128049310"": {
        ""id"": 128049310,
        ""category"": ""module"",
        ""name"": ""Vulture_Armour_Grade1"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128049311"": {
        ""id"": 128049311,
        ""category"": ""module"",
        ""name"": ""Vulture_Armour_Grade2"",
        ""cost"": 1970246,
        ""sku"": null
      },
      ""128049312"": {
        ""id"": 128049312,
        ""category"": ""module"",
        ""name"": ""Vulture_Armour_Grade3"",
        ""cost"": 4433053,
        ""sku"": null
      },
      ""128049313"": {
        ""id"": 128049313,
        ""category"": ""module"",
        ""name"": ""Vulture_Armour_Mirrored"",
        ""cost"": 10476783,
        ""sku"": null
      },
      ""128049314"": {
        ""id"": 128049314,
        ""category"": ""module"",
        ""name"": ""Vulture_Armour_Reactive"",
        ""cost"": 11609674,
        ""sku"": null
      },
      ""128049322"": {
        ""id"": 128049322,
        ""category"": ""module"",
        ""name"": ""Federation_Dropship_Armour_Grade1"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128049323"": {
        ""id"": 128049323,
        ""category"": ""module"",
        ""name"": ""Federation_Dropship_Armour_Grade2"",
        ""cost"": 5725682,
        ""sku"": null
      },
      ""128049324"": {
        ""id"": 128049324,
        ""category"": ""module"",
        ""name"": ""Federation_Dropship_Armour_Grade3"",
        ""cost"": 12882784,
        ""sku"": null
      },
      ""128049325"": {
        ""id"": 128049325,
        ""category"": ""module"",
        ""name"": ""Federation_Dropship_Armour_Mirrored"",
        ""cost"": 30446314,
        ""sku"": null
      },
      ""128049326"": {
        ""id"": 128049326,
        ""category"": ""module"",
        ""name"": ""Federation_Dropship_Armour_Reactive"",
        ""cost"": 33738581,
        ""sku"": null
      },
      ""128667727"": {
        ""id"": 128667727,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_CobraMkiii_Default_52"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066428"": {
        ""id"": 128066428,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_cobramkiii_wireframe_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128670861"": {
        ""id"": 128670861,
        ""category"": ""paintjob"",
        ""name"": ""PaintJob_CobraMkIII_Onionhead1_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671133"": {
        ""id"": 128671133,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_cobramkiii_vibrant_green"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671134"": {
        ""id"": 128671134,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_cobramkiii_vibrant_blue"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671135"": {
        ""id"": 128671135,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_cobramkiii_vibrant_orange"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671136"": {
        ""id"": 128671136,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_cobramkiii_vibrant_red"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671137"": {
        ""id"": 128671137,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_cobramkiii_vibrant_purple"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671138"": {
        ""id"": 128671138,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_cobramkiii_vibrant_yellow"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128667638"": {
        ""id"": 128667638,
        ""category"": ""paintjob"",
        ""name"": ""PaintJob_Sidewinder_Merc"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066405"": {
        ""id"": 128066405,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_eagle_stripe1_02"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066406"": {
        ""id"": 128066406,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_eagle_doublestripe_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066416"": {
        ""id"": 128066416,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_eagle_thirds_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066419"": {
        ""id"": 128066419,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_eagle_stripe1_03"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066420"": {
        ""id"": 128066420,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_eagle_thirds_02"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066430"": {
        ""id"": 128066430,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_eagle_stripe1_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066436"": {
        ""id"": 128066436,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_eagle_camo_03"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066437"": {
        ""id"": 128066437,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_eagle_thirds_03"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066441"": {
        ""id"": 128066441,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_eagle_camo_02"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066449"": {
        ""id"": 128066449,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_eagle_doublestripe_02"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066453"": {
        ""id"": 128066453,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_eagle_doublestripe_03"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066456"": {
        ""id"": 128066456,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_eagle_camo_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671139"": {
        ""id"": 128671139,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_eagle_vibrant_green"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671140"": {
        ""id"": 128671140,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_eagle_vibrant_blue"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671141"": {
        ""id"": 128671141,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_eagle_vibrant_orange"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671142"": {
        ""id"": 128671142,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_eagle_vibrant_red"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671143"": {
        ""id"": 128671143,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_eagle_vibrant_purple"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671144"": {
        ""id"": 128671144,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_eagle_vibrant_yellow"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671777"": {
        ""id"": 128671777,
        ""category"": ""paintjob"",
        ""name"": ""PaintJob_Sidewinder_Militaire_Desert_Sand"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671778"": {
        ""id"": 128671778,
        ""category"": ""paintjob"",
        ""name"": ""PaintJob_Sidewinder_Militaire_Earth_Yellow"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128672802"": {
        ""id"": 128672802,
        ""category"": ""paintjob"",
        ""name"": ""PaintJob_Sidewinder_BlackFriday_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671779"": {
        ""id"": 128671779,
        ""category"": ""paintjob"",
        ""name"": ""PaintJob_Sidewinder_Militaire_Dark_Green"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671780"": {
        ""id"": 128671780,
        ""category"": ""paintjob"",
        ""name"": ""PaintJob_Sidewinder_Militaire_Forest_Green"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671781"": {
        ""id"": 128671781,
        ""category"": ""paintjob"",
        ""name"": ""PaintJob_Sidewinder_Militaire_Sand"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671782"": {
        ""id"": 128671782,
        ""category"": ""paintjob"",
        ""name"": ""PaintJob_Sidewinder_Militaire_Earth_Red"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128672426"": {
        ""id"": 128672426,
        ""category"": ""paintjob"",
        ""name"": ""PaintJob_Sidewinder_SpecialEffect_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066404"": {
        ""id"": 128066404,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_sidewinder_default_02"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066408"": {
        ""id"": 128066408,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_sidewinder_default_03"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066414"": {
        ""id"": 128066414,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_sidewinder_doublestripe_08"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066423"": {
        ""id"": 128066423,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_sidewinder_doublestripe_05"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066431"": {
        ""id"": 128066431,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_sidewinder_thirds_07"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066432"": {
        ""id"": 128066432,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_sidewinder_thirds_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066433"": {
        ""id"": 128066433,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_sidewinder_doublestripe_07"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066440"": {
        ""id"": 128066440,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_sidewinder_camo_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066444"": {
        ""id"": 128066444,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_sidewinder_thirds_06"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066447"": {
        ""id"": 128066447,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_sidewinder_camo_03"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066448"": {
        ""id"": 128066448,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_sidewinder_default_04"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066454"": {
        ""id"": 128066454,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_sidewinder_camo_02"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671181"": {
        ""id"": 128671181,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_sidewinder_vibrant_green"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671182"": {
        ""id"": 128671182,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_sidewinder_vibrant_blue"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671183"": {
        ""id"": 128671183,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_sidewinder_vibrant_orange"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671184"": {
        ""id"": 128671184,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_sidewinder_vibrant_red"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671185"": {
        ""id"": 128671185,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_sidewinder_vibrant_purple"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671186"": {
        ""id"": 128671186,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_sidewinder_vibrant_yellow"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066407"": {
        ""id"": 128066407,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_flag_switzerland_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066409"": {
        ""id"": 128066409,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_flag_belgium_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066410"": {
        ""id"": 128066410,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_flag_australia_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066411"": {
        ""id"": 128066411,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_default_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066412"": {
        ""id"": 128066412,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_stripe2_02"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066413"": {
        ""id"": 128066413,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_flag_austria_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066415"": {
        ""id"": 128066415,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_stripe1_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066417"": {
        ""id"": 128066417,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_flag_spain_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066418"": {
        ""id"": 128066418,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_stripe1_02"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066421"": {
        ""id"": 128066421,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_flag_denmark_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066422"": {
        ""id"": 128066422,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_police_federation_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128666742"": {
        ""id"": 128666742,
        ""category"": ""paintjob"",
        ""name"": ""PaintJob_Sidewinder_Hotrod_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128666743"": {
        ""id"": 128666743,
        ""category"": ""paintjob"",
        ""name"": ""PaintJob_Sidewinder_Hotrod_03"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066424"": {
        ""id"": 128066424,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_flag_newzealand_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066425"": {
        ""id"": 128066425,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_flag_italy_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066426"": {
        ""id"": 128066426,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_stripe2_04"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066427"": {
        ""id"": 128066427,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_police_independent_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066429"": {
        ""id"": 128066429,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_default_03"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066434"": {
        ""id"": 128066434,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_flag_uk_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066435"": {
        ""id"": 128066435,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_flag_germany_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066438"": {
        ""id"": 128066438,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_flag_netherlands_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066439"": {
        ""id"": 128066439,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_flag_usa_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066442"": {
        ""id"": 128066442,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_flag_russia_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066443"": {
        ""id"": 128066443,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_flag_canada_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066445"": {
        ""id"": 128066445,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_flag_sweden_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066446"": {
        ""id"": 128066446,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_flag_poland_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066450"": {
        ""id"": 128066450,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_flag_finland_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066451"": {
        ""id"": 128066451,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_flag_france_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066452"": {
        ""id"": 128066452,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_police_empire_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128066455"": {
        ""id"": 128066455,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_flag_norway_01"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671205"": {
        ""id"": 128671205,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_vibrant_green"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671206"": {
        ""id"": 128671206,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_vibrant_blue"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671207"": {
        ""id"": 128671207,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_vibrant_orange"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671208"": {
        ""id"": 128671208,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_vibrant_red"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671209"": {
        ""id"": 128671209,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_vibrant_purple"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671210"": {
        ""id"": 128671210,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_viper_vibrant_yellow"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671127"": {
        ""id"": 128671127,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_asp_vibrant_green"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671128"": {
        ""id"": 128671128,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_asp_vibrant_blue"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671129"": {
        ""id"": 128671129,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_asp_vibrant_orange"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671130"": {
        ""id"": 128671130,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_asp_vibrant_red"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671131"": {
        ""id"": 128671131,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_asp_vibrant_purple"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671132"": {
        ""id"": 128671132,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_asp_vibrant_yellow"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671151"": {
        ""id"": 128671151,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_feddropship_vibrant_green"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671152"": {
        ""id"": 128671152,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_feddropship_vibrant_blue"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671153"": {
        ""id"": 128671153,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_feddropship_vibrant_orange"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671154"": {
        ""id"": 128671154,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_feddropship_vibrant_red"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671155"": {
        ""id"": 128671155,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_feddropship_vibrant_purple"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671156"": {
        ""id"": 128671156,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_feddropship_vibrant_yellow"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671175"": {
        ""id"": 128671175,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_python_vibrant_green"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671176"": {
        ""id"": 128671176,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_python_vibrant_blue"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671177"": {
        ""id"": 128671177,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_python_vibrant_orange"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671178"": {
        ""id"": 128671178,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_python_vibrant_red"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671179"": {
        ""id"": 128671179,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_python_vibrant_purple"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671180"": {
        ""id"": 128671180,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_python_vibrant_yellow"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671121"": {
        ""id"": 128671121,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_adder_vibrant_green"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671122"": {
        ""id"": 128671122,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_adder_vibrant_blue"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671123"": {
        ""id"": 128671123,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_adder_vibrant_orange"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671124"": {
        ""id"": 128671124,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_adder_vibrant_red"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671125"": {
        ""id"": 128671125,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_adder_vibrant_purple"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671126"": {
        ""id"": 128671126,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_adder_vibrant_yellow"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671145"": {
        ""id"": 128671145,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_empiretrader_vibrant_green"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671146"": {
        ""id"": 128671146,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_empiretrader_vibrant_blue"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671147"": {
        ""id"": 128671147,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_empiretrader_vibrant_orange"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671148"": {
        ""id"": 128671148,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_empiretrader_vibrant_red"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671149"": {
        ""id"": 128671149,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_empiretrader_vibrant_purple"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671150"": {
        ""id"": 128671150,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_empiretrader_vibrant_yellow"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671749"": {
        ""id"": 128671749,
        ""category"": ""paintjob"",
        ""name"": ""PaintJob_FerDeLance_Militaire_desert_Sand"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671750"": {
        ""id"": 128671750,
        ""category"": ""paintjob"",
        ""name"": ""PaintJob_FerDeLance_Militaire_Earth_Yellow"",
        ""cost"": 0,
        ""sku"": ""FORC_FDEV_V_FERDELANCE_1013""
      },
      ""128671751"": {
        ""id"": 128671751,
        ""category"": ""paintjob"",
        ""name"": ""PaintJob_FerDeLance_Militaire_Dark_Green"",
        ""cost"": 0,
        ""sku"": ""FORC_FDEV_V_FERDELANCE_1014""
      },
      ""128671752"": {
        ""id"": 128671752,
        ""category"": ""paintjob"",
        ""name"": ""PaintJob_FerDeLance_Militaire_Forest_Green"",
        ""cost"": 0,
        ""sku"": ""FORC_FDEV_V_FERDELANCE_1015""
      },
      ""128671753"": {
        ""id"": 128671753,
        ""category"": ""paintjob"",
        ""name"": ""PaintJob_FerDeLance_Militaire_Sand"",
        ""cost"": 0,
        ""sku"": ""FORC_FDEV_V_FERDELANCE_1016""
      },
      ""128671754"": {
        ""id"": 128671754,
        ""category"": ""paintjob"",
        ""name"": ""PaintJob_FerDeLance_Militaire_Earth_Red"",
        ""cost"": 0,
        ""sku"": ""FORC_FDEV_V_FERDELANCE_1017""
      },
      ""128671383"": {
        ""id"": 128671383,
        ""category"": ""paintjob"",
        ""name"": ""PaintJob_FerDeLance_Tactical_Brown"",
        ""cost"": 0,
        ""sku"": ""FORC_FDEV_V_FERDELANCE_1006""
      },
      ""128671384"": {
        ""id"": 128671384,
        ""category"": ""paintjob"",
        ""name"": ""PaintJob_FerDeLance_Tactical_Green"",
        ""cost"": 0,
        ""sku"": ""FORC_FDEV_V_FERDELANCE_1007""
      },
      ""128671385"": {
        ""id"": 128671385,
        ""category"": ""paintjob"",
        ""name"": ""PaintJob_FerDeLance_Tactical_Red"",
        ""cost"": 0,
        ""sku"": ""FORC_FDEV_V_FERDELANCE_1008""
      },
      ""128671386"": {
        ""id"": 128671386,
        ""category"": ""paintjob"",
        ""name"": ""PaintJob_FerDeLance_Tactical_White"",
        ""cost"": 0,
        ""sku"": ""FORC_FDEV_V_FERDELANCE_1009""
      },
      ""128671387"": {
        ""id"": 128671387,
        ""category"": ""paintjob"",
        ""name"": ""PaintJob_FerDeLance_Tactical_Blue"",
        ""cost"": 0,
        ""sku"": ""FORC_FDEV_V_FERDELANCE_1010""
      },
      ""128671388"": {
        ""id"": 128671388,
        ""category"": ""paintjob"",
        ""name"": ""PaintJob_FerDeLance_Tactical_Grey"",
        ""cost"": 0,
        ""sku"": ""FORC_FDEV_V_FERDELANCE_1011""
      },
      ""128671157"": {
        ""id"": 128671157,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_ferdelance_vibrant_green"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671158"": {
        ""id"": 128671158,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_ferdelance_vibrant_blue"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671159"": {
        ""id"": 128671159,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_ferdelance_vibrant_orange"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671160"": {
        ""id"": 128671160,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_ferdelance_vibrant_red"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671161"": {
        ""id"": 128671161,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_ferdelance_vibrant_purple"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671162"": {
        ""id"": 128671162,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_ferdelance_vibrant_yellow"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671163"": {
        ""id"": 128671163,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_hauler_vibrant_green"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671164"": {
        ""id"": 128671164,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_hauler_vibrant_blue"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671165"": {
        ""id"": 128671165,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_hauler_vibrant_orange"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671166"": {
        ""id"": 128671166,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_hauler_vibrant_red"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671167"": {
        ""id"": 128671167,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_hauler_vibrant_purple"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671168"": {
        ""id"": 128671168,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_hauler_vibrant_yellow"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671169"": {
        ""id"": 128671169,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_orca_vibrant_green"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671170"": {
        ""id"": 128671170,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_orca_vibrant_blue"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671171"": {
        ""id"": 128671171,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_orca_vibrant_orange"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671172"": {
        ""id"": 128671172,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_orca_vibrant_red"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671173"": {
        ""id"": 128671173,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_orca_vibrant_purple"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671174"": {
        ""id"": 128671174,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_orca_vibrant_yellow"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671187"": {
        ""id"": 128671187,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_type6_vibrant_green"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671188"": {
        ""id"": 128671188,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_type6_vibrant_blue"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671189"": {
        ""id"": 128671189,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_type6_vibrant_orange"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671190"": {
        ""id"": 128671190,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_type6_vibrant_red"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671191"": {
        ""id"": 128671191,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_type6_vibrant_purple"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671192"": {
        ""id"": 128671192,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_type6_vibrant_yellow"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671193"": {
        ""id"": 128671193,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_type7_vibrant_green"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671194"": {
        ""id"": 128671194,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_type7_vibrant_blue"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671195"": {
        ""id"": 128671195,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_type7_vibrant_orange"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671196"": {
        ""id"": 128671196,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_type7_vibrant_red"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671197"": {
        ""id"": 128671197,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_type7_vibrant_purple"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671198"": {
        ""id"": 128671198,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_type7_vibrant_yellow"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671199"": {
        ""id"": 128671199,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_type9_vibrant_green"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671200"": {
        ""id"": 128671200,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_type9_vibrant_blue"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671201"": {
        ""id"": 128671201,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_type9_vibrant_orange"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671202"": {
        ""id"": 128671202,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_type9_vibrant_red"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671203"": {
        ""id"": 128671203,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_type9_vibrant_purple"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671204"": {
        ""id"": 128671204,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_type9_vibrant_yellow"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671211"": {
        ""id"": 128671211,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_vulture_vibrant_green"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671212"": {
        ""id"": 128671212,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_vulture_vibrant_blue"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671213"": {
        ""id"": 128671213,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_vulture_vibrant_orange"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671214"": {
        ""id"": 128671214,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_vulture_vibrant_red"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671215"": {
        ""id"": 128671215,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_vulture_vibrant_purple"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128671216"": {
        ""id"": 128671216,
        ""category"": ""paintjob"",
        ""name"": ""paintjob_vulture_vibrant_yellow"",
        ""cost"": 0,
        ""sku"": null
      },
      ""128667736"": {
        ""id"": 128667736,
        ""category"": ""decal"",
        ""name"": ""Decal_Combat_Mostly_Harmless"",
        ""cost"": 0,
        ""sku"": ""ELITE_SPECIFIC_V_COMBAT_DECAL_1001""
      },
      ""128667737"": {
        ""id"": 128667737,
        ""category"": ""decal"",
        ""name"": ""Decal_Combat_Novice"",
        ""cost"": 0,
        ""sku"": ""ELITE_SPECIFIC_V_COMBAT_DECAL_1002""
      },
      ""128667738"": {
        ""id"": 128667738,
        ""category"": ""decal"",
        ""name"": ""Decal_Combat_Competent"",
        ""cost"": 0,
        ""sku"": ""ELITE_SPECIFIC_V_COMBAT_DECAL_1003""
      },
      ""128667739"": {
        ""id"": 128667739,
        ""category"": ""decal"",
        ""name"": ""Decal_Combat_Expert"",
        ""cost"": 0,
        ""sku"": ""ELITE_SPECIFIC_V_COMBAT_DECAL_1004""
      },
      ""128667740"": {
        ""id"": 128667740,
        ""category"": ""decal"",
        ""name"": ""Decal_Combat_Master"",
        ""cost"": 0,
        ""sku"": ""ELITE_SPECIFIC_V_COMBAT_DECAL_1005""
      },
      ""128667741"": {
        ""id"": 128667741,
        ""category"": ""decal"",
        ""name"": ""Decal_Combat_Dangerous"",
        ""cost"": 0,
        ""sku"": ""ELITE_SPECIFIC_V_COMBAT_DECAL_1006""
      },
      ""128667744"": {
        ""id"": 128667744,
        ""category"": ""decal"",
        ""name"": ""Decal_Trade_Mostly_Penniless"",
        ""cost"": 0,
        ""sku"": ""ELITE_SPECIFIC_V_TRADE_DECAL_1001""
      },
      ""128667745"": {
        ""id"": 128667745,
        ""category"": ""decal"",
        ""name"": ""Decal_Trade_Peddler"",
        ""cost"": 0,
        ""sku"": ""ELITE_SPECIFIC_V_TRADE_DECAL_1002""
      },
      ""128667746"": {
        ""id"": 128667746,
        ""category"": ""decal"",
        ""name"": ""Decal_Trade_Dealer"",
        ""cost"": 0,
        ""sku"": ""ELITE_SPECIFIC_V_TRADE_DECAL_1003""
      },
      ""128667747"": {
        ""id"": 128667747,
        ""category"": ""decal"",
        ""name"": ""Decal_Trade_Merchant"",
        ""cost"": 0,
        ""sku"": ""ELITE_SPECIFIC_V_TRADE_DECAL_1004""
      },
      ""128667748"": {
        ""id"": 128667748,
        ""category"": ""decal"",
        ""name"": ""Decal_Trade_Broker"",
        ""cost"": 0,
        ""sku"": ""ELITE_SPECIFIC_V_TRADE_DECAL_1005""
      },
      ""128667752"": {
        ""id"": 128667752,
        ""category"": ""decal"",
        ""name"": ""Decal_Explorer_Mostly_Aimless"",
        ""cost"": 0,
        ""sku"": ""ELITE_SPECIFIC_V_EXPLORE_DECAL_1001""
      },
      ""128667753"": {
        ""id"": 128667753,
        ""category"": ""decal"",
        ""name"": ""Decal_Explorer_Scout"",
        ""cost"": 0,
        ""sku"": ""ELITE_SPECIFIC_V_EXPLORE_DECAL_1002""
      },
      ""128667754"": {
        ""id"": 128667754,
        ""category"": ""decal"",
        ""name"": ""Decal_Explorer_Surveyor"",
        ""cost"": 0,
        ""sku"": ""ELITE_SPECIFIC_V_EXPLORE_DECAL_1003""
      },
      ""128671331"": {
        ""id"": 128671331,
        ""category"": ""powerplay"",
        ""name"": ""Int_ShieldGenerator_Size1_Class3_Fast"",
        ""cost"": 7713,
        ""sku"": null
      },
      ""128671332"": {
        ""id"": 128671332,
        ""category"": ""powerplay"",
        ""name"": ""Int_ShieldGenerator_Size2_Class3_Fast"",
        ""cost"": 26705,
        ""sku"": null
      },
      ""128671333"": {
        ""id"": 128671333,
        ""category"": ""powerplay"",
        ""name"": ""Int_ShieldGenerator_Size3_Class3_Fast"",
        ""cost"": 84653,
        ""sku"": null
      },
      ""128671334"": {
        ""id"": 128671334,
        ""category"": ""powerplay"",
        ""name"": ""Int_ShieldGenerator_Size4_Class3_Fast"",
        ""cost"": 268347,
        ""sku"": null
      },
      ""128671335"": {
        ""id"": 128671335,
        ""category"": ""powerplay"",
        ""name"": ""Int_ShieldGenerator_Size5_Class3_Fast"",
        ""cost"": 850659,
        ""sku"": null
      },
      ""128671336"": {
        ""id"": 128671336,
        ""category"": ""powerplay"",
        ""name"": ""Int_ShieldGenerator_Size6_Class3_Fast"",
        ""cost"": 2696589,
        ""sku"": null
      },
      ""128671337"": {
        ""id"": 128671337,
        ""category"": ""powerplay"",
        ""name"": ""Int_ShieldGenerator_Size7_Class3_Fast"",
        ""cost"": 8548185,
        ""sku"": null
      },
      ""128671338"": {
        ""id"": 128671338,
        ""category"": ""powerplay"",
        ""name"": ""Int_ShieldGenerator_Size8_Class3_Fast"",
        ""cost"": 27097748,
        ""sku"": null
      },
      ""128671448"": {
        ""id"": 128671448,
        ""category"": ""powerplay"",
        ""name"": ""Hpt_MineLauncher_Fixed_Small_Impulse"",
        ""cost"": 36390,
        ""sku"": null
      }
    }
  },
  ""ship"": {
    ""name"": ""Python"",
    ""modules"": {
      ""LargeHardpoint1"": {
        ""module"": {
          ""id"": 128049387,
          ""name"": ""Hpt_PulseLaser_Gimbal_Large"",
          ""value"": 126540,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 0,
          ""ammo"": {
            ""clip"": 1,
            ""hopper"": 0
          }
        }
      },
      ""LargeHardpoint2"": {
        ""module"": {
          ""id"": 128049387,
          ""name"": ""Hpt_PulseLaser_Gimbal_Large"",
          ""value"": 140600,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 0,
          ""ammo"": {
            ""clip"": 1,
            ""hopper"": 0
          }
        }
      },
      ""LargeHardpoint3"": {
        ""module"": {
          ""id"": 128049387,
          ""name"": ""Hpt_PulseLaser_Gimbal_Large"",
          ""value"": 140600,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 0,
          ""ammo"": {
            ""clip"": 1,
            ""hopper"": 0
          }
        }
      },
      ""MediumHardpoint1"": {
        ""module"": {
          ""id"": 128049452,
          ""name"": ""Hpt_Slugshot_Gimbal_Medium"",
          ""value"": 437760,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 0,
          ""ammo"": {
            ""clip"": 3,
            ""hopper"": 90
          }
        }
      },
      ""MediumHardpoint2"": {
        ""module"": {
          ""id"": 128049452,
          ""name"": ""Hpt_Slugshot_Gimbal_Medium"",
          ""value"": 437760,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 0,
          ""ammo"": {
            ""clip"": 3,
            ""hopper"": 90
          }
        }
      },
      ""TinyHardpoint1"": {
        ""module"": {
          ""id"": 128668536,
          ""name"": ""Hpt_ShieldBooster_Size0_Class5"",
          ""value"": 281000,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 0,
          ""ammo"": {
            ""clip"": 0,
            ""hopper"": 0
          }
        }
      },
      ""TinyHardpoint2"": {
        ""module"": {
          ""id"": 128668536,
          ""name"": ""Hpt_ShieldBooster_Size0_Class5"",
          ""value"": 281000,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 0,
          ""ammo"": {
            ""clip"": 0,
            ""hopper"": 0
          }
        }
      },
      ""TinyHardpoint3"": {
        ""module"": {
          ""id"": 128668536,
          ""name"": ""Hpt_ShieldBooster_Size0_Class5"",
          ""value"": 281000,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 0,
          ""ammo"": {
            ""clip"": 0,
            ""hopper"": 0
          }
        }
      },
      ""TinyHardpoint4"": {
        ""module"": {
          ""id"": 128668536,
          ""name"": ""Hpt_ShieldBooster_Size0_Class5"",
          ""value"": 281000,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 0,
          ""ammo"": {
            ""clip"": 0,
            ""hopper"": 0
          }
        }
      },
      ""PaintJob"": {
        ""module"": {
          ""id"": 128671177,
          ""name"": ""paintjob_python_vibrant_orange"",
          ""value"": 0,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 1
        }
      },
      ""Decal1"": {
        ""module"": {
          ""id"": 128667741,
          ""name"": ""Decal_Combat_Dangerous"",
          ""value"": 0,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 1
        }
      },
      ""Decal2"": {
        ""module"": {
          ""id"": 128667741,
          ""name"": ""Decal_Combat_Dangerous"",
          ""value"": 0,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 1
        }
      },
      ""Decal3"": {
        ""module"": {
          ""id"": 128667741,
          ""name"": ""Decal_Combat_Dangerous"",
          ""value"": 0,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 1
        }
      },
      ""Armour"": {
        ""module"": {
          ""id"": 128049341,
          ""name"": ""Python_Armour_Grade2"",
          ""value"": 22791271,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 1,
          ""ammo"": {
            ""clip"": 0,
            ""hopper"": 0
          }
        }
      },
      ""PowerPlant"": {
        ""module"": {
          ""id"": 128064060,
          ""name"": ""Int_Powerplant_Size7_Class3"",
          ""value"": 5698790,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 1,
          ""ammo"": {
            ""clip"": 0,
            ""hopper"": 0
          }
        }
      },
      ""MainEngines"": {
        ""module"": {
          ""id"": 128064089,
          ""name"": ""Int_Engine_Size6_Class2"",
          ""value"": 599242,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 0,
          ""ammo"": {
            ""clip"": 0,
            ""hopper"": 0
          }
        }
      },
      ""FrameShiftDrive"": {
        ""module"": {
          ""id"": 128064122,
          ""name"": ""Int_Hyperdrive_Size5_Class5"",
          ""value"": 5103953,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 3,
          ""ammo"": {
            ""clip"": 0,
            ""hopper"": 0
          }
        }
      },
      ""LifeSupport"": {
        ""module"": {
          ""id"": 128064157,
          ""name"": ""Int_LifeSupport_Size4_Class5"",
          ""value"": 443328,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 0,
          ""ammo"": {
            ""clip"": 0,
            ""hopper"": 0
          }
        }
      },
      ""PowerDistributor"": {
        ""module"": {
          ""id"": 128064212,
          ""name"": ""Int_PowerDistributor_Size7_Class5"",
          ""value"": 9731925,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 0,
          ""ammo"": {
            ""clip"": 0,
            ""hopper"": 0
          }
        }
      },
      ""Radar"": {
        ""module"": {
          ""id"": 128064244,
          ""name"": ""Int_Sensors_Size6_Class2"",
          ""value"": 222444,
          ""unloaned"": 222444,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 0
        }
      },
      ""FuelTank"": {
        ""module"": {
          ""id"": 128064350,
          ""name"": ""Int_FuelTank_Size5_Class3"",
          ""value"": 97754,
          ""unloaned"": 97754,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 1
        }
      },
      ""Slot01_Size6"": {
        ""module"": {
          ""id"": 128064287,
          ""name"": ""Int_ShieldGenerator_Size6_Class5"",
          ""value"": 16179531,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 0,
          ""ammo"": {
            ""clip"": 0,
            ""hopper"": 0
          }
        }
      },
      ""Slot02_Size6"": {
        ""module"": {
          ""id"": 128668546,
          ""name"": ""Int_HullReinforcement_Size5_Class2"",
          ""value"": 450000,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 1,
          ""ammo"": {
            ""clip"": 0,
            ""hopper"": 0
          }
        }
      },
      ""Slot03_Size6"": {
        ""module"": {
          ""id"": 128668546,
          ""name"": ""Int_HullReinforcement_Size5_Class2"",
          ""value"": 450000,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 1,
          ""ammo"": {
            ""clip"": 0,
            ""hopper"": 0
          }
        }
      },
      ""Slot04_Size5"": {
        ""module"": {
          ""id"": 128668546,
          ""name"": ""Int_HullReinforcement_Size5_Class2"",
          ""value"": 450000,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 1,
          ""ammo"": {
            ""clip"": 0,
            ""hopper"": 0
          }
        }
      },
      ""Slot05_Size5"": {
        ""module"": {
          ""id"": 128668546,
          ""name"": ""Int_HullReinforcement_Size5_Class2"",
          ""value"": 450000,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 1,
          ""ammo"": {
            ""clip"": 0,
            ""hopper"": 0
          }
        }
      },
      ""Slot06_Size4"": {
        ""module"": {
          ""id"": 128668544,
          ""name"": ""Int_HullReinforcement_Size4_Class2"",
          ""value"": 195000,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 1,
          ""ammo"": {
            ""clip"": 0,
            ""hopper"": 0
          }
        }
      },
      ""Slot07_Size3"": {
        ""module"": {
          ""id"": 128668542,
          ""name"": ""Int_HullReinforcement_Size3_Class2"",
          ""value"": 84000,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 1,
          ""ammo"": {
            ""clip"": 0,
            ""hopper"": 0
          }
        }
      },
      ""Slot08_Size3"": {
        ""module"": {
          ""id"": 128668542,
          ""name"": ""Int_HullReinforcement_Size3_Class2"",
          ""value"": 84000,
          ""unloaned"": 0,
          ""free"": false,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 1,
          ""ammo"": {
            ""clip"": 0,
            ""hopper"": 0
          }
        }
      },
      ""Slot09_Size2"": [],
      ""PlanetaryApproachSuite"": {
        ""module"": {
          ""name"": ""Int_PlanetApproachSuite"",
          ""id"": 128672317,
          ""unloaned"": 500,
          ""free"": false,
          ""value"": 500,
          ""health"": 1000000,
          ""on"": true,
          ""priority"": 1
        }
      },
      ""Bobble01"": [],
      ""Bobble02"": [],
      ""Bobble03"": [],
      ""Bobble04"": [],
      ""Bobble05"": [],
      ""Bobble06"": [],
      ""Bobble07"": [],
      ""Bobble08"": [],
      ""Bobble09"": [],
      ""Bobble10"": []
    },
    ""value"": {
      ""hull"": 55171396,
      ""modules"": 65489058,
      ""cargo"": 0,
      ""total"": 120660454,
      ""unloaned"": 320698
    },
    ""free"": false,
    ""health"": {
      ""hull"": 1000000,
      ""shield"": 1000000,
      ""shieldup"": true,
      ""integrity"": 0,
      ""paintwork"": 0
    },
    ""wear"": {
      ""dirt"": 0,
      ""fade"": 0,
      ""tear"": 0,
      ""game"": 0
    },
    ""cockpitBreached"": false,
    ""oxygenRemaining"": 1500000,
    ""fuel"": {
      ""main"": {
        ""level"": 32,
        ""capacity"": 32
      },
      ""reserve"": {
        ""level"": 0.83,
        ""capacity"": 0.83
      }
    },
    ""cargo"": {
      ""capacity"": 10,
      ""qty"": 6,
      ""items"": [
       {
          ""commodity"": ""drones"",
          ""origin"": 3226643968,
          ""powerplayOrigin"": null,
          ""masq"": null,
          ""owner"": 593409,
          ""mission"": null,
          ""qty"": 4,
          ""value"": 404,
          ""xyz"": {
                ""x"": 49889.5,
            ""y"": 40996.3125,
            ""z"": 24144.90625
          },
          ""marked"": 0
        },
        {
          ""commodity"": ""beer"",
          ""origin"": 3226643968,
          ""powerplayOrigin"": null,
          ""masq"": null,
          ""owner"": 593409,
          ""mission"": null,
          ""qty"": 2,
          ""value"": 202,
          ""xyz"": {
            ""x"": 49889.5,
            ""y"": 40996.3125,
            ""z"": 24144.90625
          },
          ""marked"": 0
        }
        ]
    },
    ""passengers"": [],
    ""refinery"": null,
    ""alive"": true,
    ""id"": 3
  },
  ""ships"": {
    ""2"": {
      ""name"": ""Vulture"",
      ""alive"": true,
      ""station"": {
        ""id"": 3228843776,
        ""name"": ""Snyder Enterprise""
      },
      ""starsystem"": {
        ""id"": ""115296"",
        ""name"": ""TZ Arietis"",
        ""systemaddress"": ""13864825529761""
      },
      ""id"": 2
    },
    ""0"": {
      ""name"": ""CobraMkIII"",
      ""alive"": true,
      ""station"": {
        ""id"": 3224125440,
        ""name"": ""Marley City""
      },
      ""starsystem"": {
        ""id"": ""3510"",
        ""name"": ""Alectrona"",
        ""systemaddress"": ""9467047323049""
      },
      ""id"": 0
    },
    ""1"": {
      ""name"": ""Viper"",
      ""alive"": true,
      ""station"": {
        ""id"": 3223529472,
        ""name"": ""Lundwall City""
      },
      ""starsystem"": {
        ""id"": ""7220"",
        ""name"": ""LTT 1349"",
        ""systemaddress"": ""633675387594""
      },
      ""id"": 1
    },
    ""3"": {
      ""name"": ""Python"",
      ""alive"": true,
      ""station"": {
        ""id"": 3226643968,
        ""name"": ""Voss Dock""
      },
      ""starsystem"": {
        ""id"": ""50420"",
        ""name"": ""Lalande 37120"",
        ""systemaddress"": ""422777457003""
      },
      ""id"": 3
    },
    ""5"": {
      ""name"": ""Viper"",
      ""alive"": true,
      ""station"": {
        ""id"": 3229104896,
        ""name"": ""Neville Horizons""
      },
      ""starsystem"": {
        ""id"": ""5069001205129"",
        ""name"": ""Kaushpoos"",
        ""systemaddress"": ""5069001205129""
      },
      ""id"": 5
    },
    ""6"": {
      ""name"": ""CobraMkIII"",
      ""alive"": true,
      ""station"": {
        ""id"": 3229104896,
        ""name"": ""Neville Horizons""
      },
      ""starsystem"": {
        ""id"": ""5069001205129"",
        ""name"": ""Kaushpoos"",
        ""systemaddress"": ""5069001205129""
      },
      ""id"": 6
    },
    ""7"": {
      ""name"": ""Eagle"",
      ""alive"": true,
      ""station"": {
        ""id"": 3229104896,
        ""name"": ""Neville Horizons""
      },
      ""starsystem"": {
        ""id"": ""5069001205129"",
        ""name"": ""Kaushpoos"",
        ""systemaddress"": ""5069001205129""
      },
      ""id"": 7
    },
    ""8"": {
      ""name"": ""Eagle"",
      ""alive"": true,
      ""station"": {
        ""id"": 3229104896,
        ""name"": ""Neville Horizons""
      },
      ""starsystem"": {
        ""id"": ""5069001205129"",
        ""name"": ""Kaushpoos"",
        ""systemaddress"": ""5069001205129""
      },
      ""id"": 8
    }
  }
}
";
            EliteDangerousCompanionAppService.CompanionAppService app = new EliteDangerousCompanionAppService.CompanionAppService(CompanionAppCredentials.FromFile());
            Commander cmdr = EliteDangerousCompanionAppService.CompanionAppService.CommanderFromProfile(data);

            Assert.AreEqual("Testy", cmdr.Name);

            Assert.AreEqual("Python", cmdr.Ship.Model);

            Assert.AreEqual(7, cmdr.Ship.PowerPlant.Class);
            Assert.AreEqual("C", cmdr.Ship.PowerPlant.Grade);
            Assert.AreEqual(9, cmdr.Ship.Hardpoints.Count);

            Hardpoint hardpoint1 = cmdr.Ship.Hardpoints[0];
            Assert.AreEqual(3, hardpoint1.Size);

            Assert.IsNotNull(hardpoint1.Module);
            Assert.AreEqual(3, hardpoint1.Size);
            Assert.AreEqual(3, hardpoint1.Module.Class);
            Assert.AreEqual("E", hardpoint1.Module.Grade);
            Assert.AreEqual(126540, hardpoint1.Module.Cost);
            Assert.AreEqual(140600, hardpoint1.Module.Value);

            Assert.AreEqual("7C", cmdr.Ship.PowerPlant.Class + cmdr.Ship.PowerPlant.Grade);
            Assert.AreEqual(9, cmdr.Ship.Compartments.Count);
            Assert.AreEqual(2, cmdr.Ship.Compartments[8].Size);
            Assert.AreEqual(null, cmdr.Ship.Compartments[8].Module);

            Assert.AreEqual(10, cmdr.Ship.CargoCapacity);
            Assert.AreEqual(6, cmdr.Ship.CargoCarried);

            /// 7 stored ships
            Assert.AreEqual(7, cmdr.StoredShips.Count);

            // First stored ship is a Vulture at Snyder Enterprise
            Ship StoredShip1 = cmdr.StoredShips[0];
            Assert.AreEqual("Vulture", StoredShip1.Model);
            Assert.AreEqual("TZ Arietis", StoredShip1.StarSystem);
            Assert.AreEqual("Snyder Enterprise", StoredShip1.Station);

            // Two lots of cargo
            Assert.AreEqual(2, cmdr.Ship.Cargo.Count);
            // Ensure that we have some drones
            // Add number of limpets carried
            int limpets = 0;
            foreach (Cargo cargo in cmdr.Ship.Cargo)
            {
                if (cargo.Commodity.Name == "Limpet")
                {
                    limpets += cargo.Quantity;
                }
            }
            Assert.AreEqual(4, limpets);
        }
    }
}
