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
            EliteDangerousCompanionAppService.CompanionAppService app = new EliteDangerousCompanionAppService.CompanionAppService(true);
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

        [TestMethod]
        public void TestCommanderFromProfile2()
        {
            string data = @"{""commander"":{""id"":209744,""name"":""SavageCore"",""credits"":22249561,""debt"":0,""currentShipId"":4,""alive"":true,""docked"":true,""rank"":{""combat"":0,""trade"":5,""explore"":3,""crime"":0,""service"":0,""empire"":2,""federation"":0,""power"":0,""cqc"":0}},""lastSystem"":{""id"":""4408507107699"",""name"":""Sui Guei"",""faction"":""Federation""},""lastStarport"":{""id"":""3227236864"",""name"":""Alcala Dock"",""faction"":""Federation"",""commodities"":[{""id"":""128049202"",""name"":""Hydrogen Fuel"",""cost_min"":113.65947826087,""cost_max"":""164.00"",""cost_mean"":""110.00"",""homebuy"":""73"",""homesell"":""70"",""consumebuy"":""3"",""baseCreationQty"":200,""baseConsumptionQty"":200,""capacity"":643346,""buyPrice"":105,""sellPrice"":99,""meanPrice"":110,""demandBracket"":0,""stockBracket"":2,""creationQty"":513065,""consumptionQty"":130281,""targetStock"":545635,""stock"":319886,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.3000"",""sec_illegal_min"":""1.13"",""sec_illegal_max"":""2.10"",""stolenmod"":""0.7500""},{""id"":""128673850"",""name"":""Hydrogen Peroxide"",""cost_min"":""589.00"",""cost_max"":846.32369565217,""cost_mean"":""917.00"",""homebuy"":""70"",""homesell"":""67"",""consumebuy"":""3"",""baseCreationQty"":0,""baseConsumptionQty"":945,""capacity"":2424326,""buyPrice"":0,""sellPrice"":847,""meanPrice"":917,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":2424326,""targetStock"":606081,""stock"":0,""demand"":1818245,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.2700"",""sec_illegal_min"":""1.14"",""sec_illegal_max"":""2.19"",""stolenmod"":""0.7500""},{""id"":""128673851"",""name"":""Liquid Oxygen"",""cost_min"":""223.00"",""cost_max"":391.43752173913,""cost_mean"":""263.00"",""homebuy"":""51"",""homesell"":""46"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":527,""capacity"":1351979,""buyPrice"":0,""sellPrice"":391,""meanPrice"":263,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":1351979,""targetStock"":337994,""stock"":0,""demand"":1009027,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.0700"",""sec_illegal_min"":""1.25"",""sec_illegal_max"":""3.04"",""stolenmod"":""0.7500""},{""id"":""128049166"",""name"":""Water"",""cost_min"":""124.00"",""cost_max"":281.05565217391,""cost_mean"":""120.00"",""homebuy"":""18"",""homesell"":""10"",""consumebuy"":""8"",""baseCreationQty"":0,""baseConsumptionQty"":163,""capacity"":418165,""buyPrice"":0,""sellPrice"":124,""meanPrice"":120,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":418165,""targetStock"":104541,""stock"":0,""demand"":78406,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128049241"",""name"":""Clothing"",""cost_min"":272.91008695652,""cost_max"":""463.00"",""cost_mean"":""285.00"",""homebuy"":""60"",""homesell"":""56"",""consumebuy"":""4"",""baseCreationQty"":35,""baseConsumptionQty"":0,""capacity"":22450,""buyPrice"":216,""sellPrice"":200,""meanPrice"":285,""demandBracket"":0,""stockBracket"":2,""creationQty"":22450,""consumptionQty"":0,""targetStock"":22450,""stock"":12567,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Consumer Items"",""volumescale"":""1.1500"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.65"",""stolenmod"":""0.7500""},{""id"":""128049240"",""name"":""Consumer Technology"",""cost_min"":""6561.00"",""cost_max"":7349.3513043478,""cost_mean"":""6769.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":27,""capacity"":31659,""buyPrice"":0,""sellPrice"":7350,""meanPrice"":6769,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":31659,""targetStock"":7914,""stock"":0,""demand"":23745,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Consumer Items"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.11"",""stolenmod"":""0.7500""},{""id"":""128049238"",""name"":""Domestic Appliances"",""cost_min"":473.77595652174,""cost_max"":""716.00"",""cost_mean"":""487.00"",""homebuy"":""69"",""homesell"":""66"",""consumebuy"":""3"",""baseCreationQty"":21,""baseConsumptionQty"":0,""capacity"":13470,""buyPrice"":404,""sellPrice"":383,""meanPrice"":487,""demandBracket"":0,""stockBracket"":2,""creationQty"":13470,""consumptionQty"":0,""targetStock"":13470,""stock"":7541,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Consumer Items"",""volumescale"":""1.2500"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128682048"",""name"":""Survival Equipment"",""cost_min"":473.77595652174,""cost_max"":""716.00"",""cost_mean"":""485.00"",""homebuy"":""69"",""homesell"":""66"",""consumebuy"":""3"",""baseCreationQty"":12,""baseConsumptionQty"":0,""capacity"":7697,""buyPrice"":331,""sellPrice"":313,""meanPrice"":485,""demandBracket"":0,""stockBracket"":3,""creationQty"":7697,""consumptionQty"":0,""targetStock"":7697,""stock"":7697,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Consumer Items"",""volumescale"":""1.2500"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128049177"",""name"":""Algae"",""cost_min"":""135.00"",""cost_max"":298.92939130435,""cost_mean"":""137.00"",""homebuy"":""27"",""homesell"":""20"",""consumebuy"":""7"",""baseCreationQty"":0,""baseConsumptionQty"":1260,""capacity"":820766,""buyPrice"":0,""sellPrice"":135,""meanPrice"":137,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":820766,""targetStock"":205191,""stock"":0,""demand"":153893.75,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.06"",""sec_illegal_max"":""1.48"",""stolenmod"":""0.7500""},{""id"":""128049182"",""name"":""Animalmeat"",""cost_min"":""1286.00"",""cost_max"":1673.8210869565,""cost_mean"":""1292.00"",""homebuy"":""80"",""homesell"":""78"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":97,""capacity"":63186,""buyPrice"":0,""sellPrice"":1286,""meanPrice"":1292,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":63186,""targetStock"":15796,""stock"":0,""demand"":11847.5,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.4000"",""sec_illegal_min"":""1.10"",""sec_illegal_max"":""1.81"",""stolenmod"":""0.7500""},{""id"":""128049189"",""name"":""Coffee"",""cost_min"":""1286.00"",""cost_max"":1673.8210869565,""cost_mean"":""1279.00"",""homebuy"":""80"",""homesell"":""78"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":97,""capacity"":15797,""buyPrice"":0,""sellPrice"":1286,""meanPrice"":1279,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":15797,""targetStock"":3949,""stock"":0,""demand"":2962,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.4000"",""sec_illegal_min"":""1.10"",""sec_illegal_max"":""1.81"",""stolenmod"":""0.7500""},{""id"":""128049183"",""name"":""Fish"",""cost_min"":""403.00"",""cost_max"":615.978,""cost_mean"":""406.00"",""homebuy"":""64"",""homesell"":""60"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":271,""capacity"":176530,""buyPrice"":0,""sellPrice"":403,""meanPrice"":406,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":176530,""targetStock"":44132,""stock"":0,""demand"":33099.5,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.2000"",""sec_illegal_min"":""1.18"",""sec_illegal_max"":""2.44"",""stolenmod"":""0.7500""},{""id"":""128049184"",""name"":""Food Cartridges"",""cost_min"":104.36139130435,""cost_max"":""267.00"",""cost_mean"":""105.00"",""homebuy"":""30"",""homesell"":""23"",""consumebuy"":""7"",""baseCreationQty"":45,""baseConsumptionQty"":0,""capacity"":115440,""buyPrice"":32,""sellPrice"":25,""meanPrice"":105,""demandBracket"":0,""stockBracket"":3,""creationQty"":115440,""consumptionQty"":0,""targetStock"":115440,""stock"":110198.735669,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128049178"",""name"":""Fruit And Vegetables"",""cost_min"":""315.00"",""cost_max"":505.08991304348,""cost_mean"":""312.00"",""homebuy"":""60"",""homesell"":""56"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":350,""capacity"":56998,""buyPrice"":0,""sellPrice"":315,""meanPrice"":312,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":56998,""targetStock"":14249,""stock"":0,""demand"":10687.25,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.1500"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.65"",""stolenmod"":""0.7500""},{""id"":""128049180"",""name"":""Grain"",""cost_min"":""207.00"",""cost_max"":371.58017391304,""cost_mean"":""210.00"",""homebuy"":""48"",""homesell"":""43"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":584,""capacity"":380419,""buyPrice"":0,""sellPrice"":207,""meanPrice"":210,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":380419,""targetStock"":95104,""stock"":0,""demand"":71328.75,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.0500"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.65"",""stolenmod"":""0.7500""},{""id"":""128049185"",""name"":""Synthetic Meat"",""cost_min"":""252.00"",""cost_max"":426.6772173913,""cost_mean"":""271.00"",""homebuy"":""54"",""homesell"":""49"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":226,""capacity"":36805,""buyPrice"":0,""sellPrice"":427,""meanPrice"":271,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":36805,""targetStock"":9201,""stock"":0,""demand"":27604,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.23"",""sec_illegal_max"":""2.88"",""stolenmod"":""0.7500""},{""id"":""128049188"",""name"":""Tea"",""cost_min"":""1459.00"",""cost_max"":1873.7669565217,""cost_mean"":""1467.00"",""homebuy"":""81"",""homesell"":""79"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":88,""capacity"":57324,""buyPrice"":0,""sellPrice"":1459,""meanPrice"":1467,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":57324,""targetStock"":14331,""stock"":0,""demand"":10748.25,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.4200"",""sec_illegal_min"":""1.09"",""sec_illegal_max"":""1.76"",""stolenmod"":""0.7500""},{""id"":""128673856"",""name"":""C M M Composite"",""cost_min"":""2966.00"",""cost_max"":3573.6862608696,""cost_mean"":""3132.00"",""homebuy"":""87"",""homesell"":""86"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":75,""capacity"":192422,""buyPrice"":0,""sellPrice"":3574,""meanPrice"":3132,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":192422,""targetStock"":48105,""stock"":0,""demand"":144317,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.3400"",""sec_illegal_min"":""1.05"",""sec_illegal_max"":""1.37"",""stolenmod"":""0.7500""},{""id"":""128672302"",""name"":""Ceramic Composites"",""cost_min"":""192.00"",""cost_max"":355.75269565217,""cost_mean"":""232.00"",""homebuy"":""46"",""homesell"":""41"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":971,""capacity"":2491223,""buyPrice"":0,""sellPrice"":356,""meanPrice"":232,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":2491223,""targetStock"":622805,""stock"":0,""demand"":1868418,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.0300"",""sec_illegal_min"":""1.28"",""sec_illegal_max"":""3.28"",""stolenmod"":""0.7500""},{""id"":""128673855"",""name"":""Insulating Membrane"",""cost_min"":""7498.00"",""cost_max"":8414.522826087,""cost_mean"":""7837.00"",""homebuy"":""93"",""homesell"":""92"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":36,""capacity"":92363,""buyPrice"":0,""sellPrice"":8412,""meanPrice"":7837,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":92363,""targetStock"":23090,""stock"":0,""demand"":69087,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.0600"",""sec_illegal_min"":""1.02"",""sec_illegal_max"":""1.15"",""stolenmod"":""0.7500""},{""id"":""128049197"",""name"":""Polymers"",""cost_min"":""152.00"",""cost_max"":311.88452173913,""cost_mean"":""171.00"",""homebuy"":""35"",""homesell"":""29"",""consumebuy"":""7"",""baseCreationQty"":0,""baseConsumptionQty"":1463,""capacity"":3753510,""buyPrice"":0,""sellPrice"":307,""meanPrice"":171,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":3753510,""targetStock"":938377,""stock"":0,""demand"":2739592,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.23"",""sec_illegal_max"":""2.88"",""stolenmod"":""0.7500""},{""id"":""128049199"",""name"":""Semiconductors"",""cost_min"":""889.00"",""cost_max"":1251.9782608696,""cost_mean"":""967.00"",""homebuy"":""76"",""homesell"":""74"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":198,""capacity"":507994,""buyPrice"":0,""sellPrice"":1238,""meanPrice"":967,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":507994,""targetStock"":126998,""stock"":0,""demand"":369928,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.3400"",""sec_illegal_min"":""1.12"",""sec_illegal_max"":""1.98"",""stolenmod"":""0.7500""},{""id"":""128049200"",""name"":""Superconductors"",""cost_min"":""6561.00"",""cost_max"":7463.6608695652,""cost_mean"":""6609.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":40,""capacity"":102626,""buyPrice"":0,""sellPrice"":7133,""meanPrice"":6609,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":102626,""targetStock"":25656,""stock"":0,""demand"":56267,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.02"",""sec_illegal_max"":""1.18"",""stolenmod"":""0.7500""},{""id"":""128064028"",""name"":""Atmospheric Extractors"",""cost_min"":356.022,""cost_max"":""569.00"",""cost_mean"":""357.00"",""homebuy"":""64"",""homesell"":""60"",""consumebuy"":""4"",""baseCreationQty"":541,""baseConsumptionQty"":0,""capacity"":347001,""buyPrice"":291,""sellPrice"":270,""meanPrice"":357,""demandBracket"":0,""stockBracket"":2,""creationQty"":347001,""consumptionQty"":0,""targetStock"":347001,""stock"":194319,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.2000"",""sec_illegal_min"":""1.18"",""sec_illegal_max"":""2.44"",""stolenmod"":""0.7500""},{""id"":""128672309"",""name"":""Building Fabricators"",""cost_min"":994.572,""cost_max"":""1344.00"",""cost_mean"":""980.00"",""homebuy"":""78"",""homesell"":""76"",""consumebuy"":""2"",""baseCreationQty"":566,""baseConsumptionQty"":0,""capacity"":1088994,""buyPrice"":784,""sellPrice"":756,""meanPrice"":980,""demandBracket"":0,""stockBracket"":3,""creationQty"":1088994,""consumptionQty"":0,""targetStock"":1088994,""stock"":1088994,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.3700"",""sec_illegal_min"":""1.11"",""sec_illegal_max"":""1.89"",""stolenmod"":""0.7500""},{""id"":""128049222"",""name"":""Crop Harvesters"",""cost_min"":2041.3407391304,""cost_max"":""2553.00"",""cost_mean"":""2021.00"",""homebuy"":""84"",""homesell"":""82"",""consumebuy"":""2"",""baseCreationQty"":643,""baseConsumptionQty"":0,""capacity"":1237143,""buyPrice"":1922,""sellPrice"":1857,""meanPrice"":2021,""demandBracket"":0,""stockBracket"":2,""creationQty"":1237143,""consumptionQty"":0,""targetStock"":1237143,""stock"":692799,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.4400"",""sec_illegal_min"":""1.06"",""sec_illegal_max"":""1.46"",""stolenmod"":""0.7500""},{""id"":""128673861"",""name"":""Emergency Power Cells"",""cost_min"":""889.00"",""cost_max"":1207.1086956522,""cost_mean"":""1011.00"",""homebuy"":""76"",""homesell"":""74"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":66,""capacity"":127006,""buyPrice"":0,""sellPrice"":1208,""meanPrice"":1011,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":127006,""targetStock"":31751,""stock"":0,""demand"":95255,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.3400"",""sec_illegal_min"":""1.12"",""sec_illegal_max"":""1.98"",""stolenmod"":""0.7500""},{""id"":""128673866"",""name"":""Exhaust Manifold"",""cost_min"":""383.00"",""cost_max"":590.846,""cost_mean"":""479.00"",""homebuy"":""63"",""homesell"":""59"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":142,""capacity"":273254,""buyPrice"":0,""sellPrice"":591,""meanPrice"":479,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":273254,""targetStock"":68313,""stock"":0,""demand"":204941,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.1900"",""sec_illegal_min"":""1.18"",""sec_illegal_max"":""2.48"",""stolenmod"":""0.7500""},{""id"":""128672307"",""name"":""Geological Equipment"",""cost_min"":1673.008,""cost_max"":""2134.00"",""cost_mean"":""1661.00"",""homebuy"":""82"",""homesell"":""80"",""consumebuy"":""2"",""baseCreationQty"":8,""baseConsumptionQty"":0,""capacity"":15393,""buyPrice"":1386,""sellPrice"":1339,""meanPrice"":1661,""demandBracket"":0,""stockBracket"":3,""creationQty"":15393,""consumptionQty"":0,""targetStock"":15393,""stock"":15393,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.4500"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.28"",""stolenmod"":""0.7500""},{""id"":""128673860"",""name"":""H N Shock Mount"",""cost_min"":447.184,""cost_max"":""683.00"",""cost_mean"":""406.00"",""homebuy"":""68"",""homesell"":""65"",""consumebuy"":""3"",""baseCreationQty"":220,""baseConsumptionQty"":0,""capacity"":423284,""buyPrice"":308,""sellPrice"":291,""meanPrice"":406,""demandBracket"":0,""stockBracket"":3,""creationQty"":423284,""consumptionQty"":0,""targetStock"":423284,""stock"":423284,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.2400"",""sec_illegal_min"":""1.16"",""sec_illegal_max"":""2.30"",""stolenmod"":""0.7500""},{""id"":""128049223"",""name"":""Marine Supplies"",""cost_min"":4002.3748695652,""cost_max"":""4723.00"",""cost_mean"":""3916.00"",""homebuy"":""89"",""homesell"":""88"",""consumebuy"":""1"",""baseCreationQty"":385,""baseConsumptionQty"":0,""capacity"":740747,""buyPrice"":3881,""sellPrice"":3799,""meanPrice"":3916,""demandBracket"":0,""stockBracket"":2,""creationQty"":740747,""consumptionQty"":0,""targetStock"":740747,""stock"":414817,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.2400"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.28"",""stolenmod"":""0.7500""},{""id"":""128049221"",""name"":""Mineral Extractors"",""cost_min"":486.25404347826,""cost_max"":""790.00"",""cost_mean"":""443.00"",""homebuy"":""70"",""homesell"":""67"",""consumebuy"":""3"",""baseCreationQty"":378,""baseConsumptionQty"":0,""capacity"":727279,""buyPrice"":438,""sellPrice"":415,""meanPrice"":443,""demandBracket"":0,""stockBracket"":2,""creationQty"":727279,""consumptionQty"":0,""targetStock"":727279,""stock"":407272,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.2700"",""sec_illegal_min"":""1.14"",""sec_illegal_max"":""2.19"",""stolenmod"":""0.7500""},{""id"":""128049217"",""name"":""Power Generators"",""cost_min"":443.53595652174,""cost_max"":""716.00"",""cost_mean"":""458.00"",""homebuy"":""69"",""homesell"":""66"",""consumebuy"":""3"",""baseCreationQty"":21,""baseConsumptionQty"":0,""capacity"":40405,""buyPrice"":392,""sellPrice"":372,""meanPrice"":458,""demandBracket"":0,""stockBracket"":2,""creationQty"":40405,""consumptionQty"":0,""targetStock"":40405,""stock"":22622,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.2500"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128672313"",""name"":""Skimer Components"",""cost_min"":841.36843478261,""cost_max"":""1203.00"",""cost_mean"":""859.00"",""homebuy"":""76"",""homesell"":""74"",""consumebuy"":""2"",""baseCreationQty"":50,""baseConsumptionQty"":0,""capacity"":32071,""buyPrice"":646,""sellPrice"":623,""meanPrice"":859,""demandBracket"":0,""stockBracket"":3,""creationQty"":32071,""consumptionQty"":0,""targetStock"":32071,""stock"":32071,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.3500"",""sec_illegal_min"":""1.11"",""sec_illegal_max"":""1.95"",""stolenmod"":""0.7500""},{""id"":""128672308"",""name"":""Thermal Cooling Units"",""cost_min"":258.47886956522,""cost_max"":""446.00"",""cost_mean"":""256.00"",""homebuy"":""59"",""homesell"":""55"",""consumebuy"":""4"",""baseCreationQty"":184,""baseConsumptionQty"":0,""capacity"":354020,""buyPrice"":155,""sellPrice"":143,""meanPrice"":256,""demandBracket"":0,""stockBracket"":3,""creationQty"":354020,""consumptionQty"":0,""targetStock"":354020,""stock"":354020,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.1400"",""sec_illegal_min"":""1.21"",""sec_illegal_max"":""2.69"",""stolenmod"":""0.7500""},{""id"":""128049218"",""name"":""Water Purifiers"",""cost_min"":258.47886956522,""cost_max"":""446.00"",""cost_mean"":""258.00"",""homebuy"":""59"",""homesell"":""55"",""consumebuy"":""4"",""baseCreationQty"":920,""baseConsumptionQty"":0,""capacity"":1770096,""buyPrice"":155,""sellPrice"":143,""meanPrice"":258,""demandBracket"":0,""stockBracket"":3,""creationQty"":1770096,""consumptionQty"":0,""targetStock"":1770096,""stock"":1527581.891511,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.1400"",""sec_illegal_min"":""1.21"",""sec_illegal_max"":""2.69"",""stolenmod"":""0.7500""},{""id"":""128682046"",""name"":""Advanced Medicines"",""cost_min"":""1136.00"",""cost_max"":1510.4237391304,""cost_mean"":""1259.00"",""homebuy"":""78"",""homesell"":""76"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":107,""capacity"":69700,""buyPrice"":0,""sellPrice"":1511,""meanPrice"":1259,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":69700,""targetStock"":17425,""stock"":0,""demand"":52275,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Medicines"",""volumescale"":""1.3800"",""sec_illegal_min"":""1.10"",""sec_illegal_max"":""1.87"",""stolenmod"":""0.7500""},{""id"":""128049210"",""name"":""Basic Medicines"",""cost_min"":268.17408695652,""cost_max"":""463.00"",""cost_mean"":""279.00"",""homebuy"":""60"",""homesell"":""56"",""consumebuy"":""4"",""baseCreationQty"":70,""baseConsumptionQty"":35,""capacity"":46357,""buyPrice"":217,""sellPrice"":200,""meanPrice"":279,""demandBracket"":0,""stockBracket"":2,""creationQty"":44899,""consumptionQty"":1458,""targetStock"":45263,""stock"":25503,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Medicines"",""volumescale"":""1.1500"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.65"",""stolenmod"":""0.7500""},{""id"":""128049209"",""name"":""Performance Enhancers"",""cost_min"":""6561.00"",""cost_max"":7448.2730434783,""cost_mean"":""6816.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":27,""capacity"":11241,""buyPrice"":0,""sellPrice"":7449,""meanPrice"":6816,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":11241,""targetStock"":2810,""stock"":0,""demand"":8431,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Medicines"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.26"",""stolenmod"":""0.7500""},{""id"":""128049669"",""name"":""Progenitor Cells"",""cost_min"":""6561.00"",""cost_max"":7416.3982608696,""cost_mean"":""6779.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":27,""capacity"":17588,""buyPrice"":0,""sellPrice"":7417,""meanPrice"":6779,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":17588,""targetStock"":4397,""stock"":0,""demand"":13191,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Medicines"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.26"",""stolenmod"":""0.7500""},{""id"":""128049176"",""name"":""Aluminium"",""cost_min"":""330.00"",""cost_max"":525.22747826087,""cost_mean"":""340.00"",""homebuy"":""61"",""homesell"":""57"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":2823,""capacity"":7242191,""buyPrice"":0,""sellPrice"":509,""meanPrice"":340,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":7242191,""targetStock"":1810547,""stock"":0,""demand"":5096742,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1600"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.60"",""stolenmod"":""0.7500""},{""id"":""128049168"",""name"":""Beryllium"",""cost_min"":""8017.00"",""cost_max"":8953.1556521739,""cost_mean"":""8288.00"",""homebuy"":""93"",""homesell"":""92"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":115,""capacity"":295024,""buyPrice"":0,""sellPrice"":8878,""meanPrice"":8288,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":295024,""targetStock"":73756,""stock"":0,""demand"":208095,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.0400"",""sec_illegal_min"":""1.02"",""sec_illegal_max"":""1.15"",""stolenmod"":""0.7500""},{""id"":""128049162"",""name"":""Cobalt"",""cost_min"":""701.00"",""cost_max"":981.54782608696,""cost_mean"":""647.00"",""homebuy"":""73"",""homesell"":""70"",""consumebuy"":""3"",""baseCreationQty"":0,""baseConsumptionQty"":162,""capacity"":415599,""buyPrice"":0,""sellPrice"":918,""meanPrice"":647,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":415599,""targetStock"":103899,""stock"":0,""demand"":259838,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.3000"",""sec_illegal_min"":""1.13"",""sec_illegal_max"":""2.10"",""stolenmod"":""0.7500""},{""id"":""128049175"",""name"":""Copper"",""cost_min"":""472.00"",""cost_max"":702.68956521739,""cost_mean"":""481.00"",""homebuy"":""67"",""homesell"":""64"",""consumebuy"":""3"",""baseCreationQty"":0,""baseConsumptionQty"":928,""capacity"":2380713,""buyPrice"":0,""sellPrice"":676,""meanPrice"":481,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":2380713,""targetStock"":595178,""stock"":0,""demand"":1632545,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.2300"",""sec_illegal_min"":""1.16"",""sec_illegal_max"":""2.33"",""stolenmod"":""0.7500""},{""id"":""128049170"",""name"":""Gallium"",""cost_min"":""5028.00"",""cost_max"":5822.7702608696,""cost_mean"":""5135.00"",""homebuy"":""90"",""homesell"":""89"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":330,""capacity"":846590,""buyPrice"":0,""sellPrice"":5722,""meanPrice"":5135,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":846590,""targetStock"":211647,""stock"":0,""demand"":575682,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1800"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.24"",""stolenmod"":""0.7500""},{""id"":""128049154"",""name"":""Gold"",""cost_min"":""9164.00"",""cost_max"":10140.858826087,""cost_mean"":""9401.00"",""homebuy"":""94"",""homesell"":""93"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":208,""capacity"":533609,""buyPrice"":0,""sellPrice"":10134,""meanPrice"":9401,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":533609,""targetStock"":133402,""stock"":0,""demand"":398018,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.09"",""stolenmod"":""0.7500""},{""id"":""128049169"",""name"":""Indium"",""cost_min"":""5743.00"",""cost_max"":6578.3104347826,""cost_mean"":""5727.00"",""homebuy"":""91"",""homesell"":""90"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":60,""capacity"":153926,""buyPrice"":0,""sellPrice"":5898,""meanPrice"":5727,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":153926,""targetStock"":38481,""stock"":0,""demand"":46420,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1400"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.22"",""stolenmod"":""0.7500""},{""id"":""128049173"",""name"":""Lithium"",""cost_min"":""1555.00"",""cost_max"":1983.1180869565,""cost_mean"":""1596.00"",""homebuy"":""81"",""homesell"":""79"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":333,""capacity"":854286,""buyPrice"":0,""sellPrice"":1827,""meanPrice"":1596,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":854286,""targetStock"":213571,""stock"":0,""demand"":469054,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.4300"",""sec_illegal_min"":""1.09"",""sec_illegal_max"":""1.74"",""stolenmod"":""0.7500""},{""id"":""128671118"",""name"":""Osmium"",""cost_min"":""6561.00"",""cost_max"":7463.6608695652,""cost_mean"":""7591.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":269,""capacity"":690099,""buyPrice"":0,""sellPrice"":7464,""meanPrice"":7591,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":690099,""targetStock"":172524,""stock"":0,""demand"":517575,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.22"",""stolenmod"":""0.7500""},{""id"":""128049153"",""name"":""Palladium"",""cost_min"":""12815.00"",""cost_max"":13928.564521739,""cost_mean"":""13298.00"",""homebuy"":""96"",""homesell"":""96"",""consumebuy"":""0"",""baseCreationQty"":0,""baseConsumptionQty"":161,""capacity"":413034,""buyPrice"":0,""sellPrice"":13928,""meanPrice"":13298,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":413034,""targetStock"":103258,""stock"":0,""demand"":309498,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.08"",""stolenmod"":""0.7500""},{""id"":""128049152"",""name"":""Platinum"",""cost_min"":""17936.00"",""cost_max"":19197.218434783,""cost_mean"":""19279.00"",""homebuy"":""97"",""homesell"":""97"",""consumebuy"":""0"",""baseCreationQty"":0,""baseConsumptionQty"":12,""capacity"":30786,""buyPrice"":0,""sellPrice"":19198,""meanPrice"":19279,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":30786,""targetStock"":7696,""stock"":0,""demand"":23090,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.04"",""stolenmod"":""0.7500""},{""id"":""128673845"",""name"":""Praseodymium"",""cost_min"":""6138.00"",""cost_max"":7014.412173913,""cost_mean"":""7156.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":142,""capacity"":364291,""buyPrice"":0,""sellPrice"":7015,""meanPrice"":7156,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":364291,""targetStock"":91072,""stock"":0,""demand"":273219,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1200"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.28"",""stolenmod"":""0.7500""},{""id"":""128673847"",""name"":""Samarium"",""cost_min"":""5373.00"",""cost_max"":6195.6016956522,""cost_mean"":""6330.00"",""homebuy"":""91"",""homesell"":""90"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":157,""capacity"":402772,""buyPrice"":0,""sellPrice"":6196,""meanPrice"":6330,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":402772,""targetStock"":100693,""stock"":0,""demand"":302079,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1600"",""sec_illegal_min"":""1.04"",""sec_illegal_max"":""1.31"",""stolenmod"":""0.7500""},{""id"":""128049155"",""name"":""Silver"",""cost_min"":""4705.00"",""cost_max"":5474.2608695652,""cost_mean"":""4775.00"",""homebuy"":""90"",""homesell"":""89"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":348,""capacity"":892768,""buyPrice"":0,""sellPrice"":5458,""meanPrice"":4775,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":892768,""targetStock"":223192,""stock"":0,""demand"":659185,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.2000"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.24"",""stolenmod"":""0.7500""},{""id"":""128049171"",""name"":""Tantalum"",""cost_min"":""3858.00"",""cost_max"":4553.8573043478,""cost_mean"":""3962.00"",""homebuy"":""89"",""homesell"":""88"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":203,""capacity"":520781,""buyPrice"":0,""sellPrice"":4470,""meanPrice"":3962,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":520781,""targetStock"":130195,""stock"":0,""demand"":355700,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.2600"",""sec_illegal_min"":""1.04"",""sec_illegal_max"":""1.29"",""stolenmod"":""0.7500""},{""id"":""128049174"",""name"":""Titanium"",""cost_min"":""1004.00"",""cost_max"":1342.4050869565,""cost_mean"":""1006.00"",""homebuy"":""77"",""homesell"":""75"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":1191,""capacity"":3055420,""buyPrice"":0,""sellPrice"":1315,""meanPrice"":1006,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":3055420,""targetStock"":763855,""stock"":0,""demand"":2154841,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.3600"",""sec_illegal_min"":""1.11"",""sec_illegal_max"":""1.92"",""stolenmod"":""0.7500""},{""id"":""128049172"",""name"":""Uranium"",""cost_min"":""2603.00"",""cost_max"":3169.14,""cost_mean"":""2705.00"",""homebuy"":""86"",""homesell"":""85"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":828,""capacity"":2124171,""buyPrice"":0,""sellPrice"":3122,""meanPrice"":2705,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":2124171,""targetStock"":531042,""stock"":0,""demand"":1494013,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.3800"",""sec_illegal_min"":""1.05"",""sec_illegal_max"":""1.39"",""stolenmod"":""0.7500""},{""id"":""128673848"",""name"":""Low Temperature Diamond"",""cost_min"":""54000.00"",""cost_max"":57594.79173913,""cost_mean"":""57445.00"",""homebuy"":""97"",""homesell"":""97"",""consumebuy"":""0"",""baseCreationQty"":0,""baseConsumptionQty"":6,""capacity"":15393,""buyPrice"":0,""sellPrice"":57595,""meanPrice"":57445,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":15393,""targetStock"":3848,""stock"":0,""demand"":11545,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.00"",""sec_illegal_max"":""1.03"",""stolenmod"":""0.7500""},{""id"":""128668550"",""name"":""Painite"",""cost_min"":""35000.00"",""cost_max"":40834.347826087,""cost_mean"":""40508.00"",""homebuy"":""100"",""homesell"":""100"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":6,""capacity"":3849,""buyPrice"":0,""sellPrice"":40835,""meanPrice"":40508,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":3849,""targetStock"":962,""stock"":0,""demand"":2887,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.04"",""stolenmod"":""0.7500""},{""id"":""128049214"",""name"":""Beer"",""cost_min"":""175.00"",""cost_max"":334.88452173913,""cost_mean"":""186.00"",""homebuy"":""42"",""homesell"":""36"",""consumebuy"":""6"",""baseCreationQty"":0,""baseConsumptionQty"":755,""capacity"":314330,""buyPrice"":0,""sellPrice"":175,""meanPrice"":186,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":314330,""targetStock"":78582,""stock"":0,""demand"":58937,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Narcotics"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.25"",""sec_illegal_max"":""3.04"",""stolenmod"":""0.7500""},{""id"":""128049216"",""name"":""Liquor"",""cost_min"":632.742,""cost_max"":765,""cost_mean"":""587.00"",""homebuy"":""71"",""homesell"":""68"",""consumebuy"":""3"",""baseCreationQty"":18,""baseConsumptionQty"":0,""capacity"":162,""buyPrice"":500,""sellPrice"":474,""meanPrice"":587,""demandBracket"":0,""stockBracket"":2,""creationQty"":162,""consumptionQty"":0,""targetStock"":162,""stock"":87,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Narcotics"",""volumescale"":""1.2800"",""sec_illegal_min"":""1.14"",""sec_illegal_max"":""2.16"",""stolenmod"":""0.7500""},{""id"":""128049215"",""name"":""Wine"",""cost_min"":""252.00"",""cost_max"":426.6772173913,""cost_mean"":""260.00"",""homebuy"":""54"",""homesell"":""49"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":452,""capacity"":253469,""buyPrice"":0,""sellPrice"":252,""meanPrice"":260,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":253469,""targetStock"":63367,""stock"":0,""demand"":47525.5,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Narcotics"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.23"",""sec_illegal_max"":""2.88"",""stolenmod"":""0.7500""},{""id"":""128066403"",""name"":""Drones"",""cost_min"":""100.00"",""cost_max"":""100.00"",""cost_mean"":""101.00"",""homebuy"":""100"",""homesell"":""100"",""consumebuy"":""1"",""baseCreationQty"":200,""baseConsumptionQty"":0,""capacity"":130281,""buyPrice"":101,""sellPrice"":100,""meanPrice"":101,""demandBracket"":0,""stockBracket"":3,""creationQty"":130281,""consumptionQty"":0,""targetStock"":130281,""stock"":130281,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""NonMarketable"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.00"",""sec_illegal_max"":""0.99"",""stolenmod"":""0.7500""},{""id"":""128049228"",""name"":""Auto Fabricators"",""cost_min"":""3612.00"",""cost_max"":4285.7520434783,""cost_mean"":""3734.00"",""homebuy"":""88"",""homesell"":""87"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":43,""capacity"":110322,""buyPrice"":0,""sellPrice"":4286,""meanPrice"":3734,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":110322,""targetStock"":27580,""stock"":0,""demand"":82742,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.2800"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.28"",""stolenmod"":""0.7500""},{""id"":""128049225"",""name"":""Computer Components"",""cost_min"":473.77595652174,""cost_max"":""716.00"",""cost_mean"":""513.00"",""homebuy"":""69"",""homesell"":""66"",""consumebuy"":""3"",""baseCreationQty"":209,""baseConsumptionQty"":0,""capacity"":134054,""buyPrice"":331,""sellPrice"":313,""meanPrice"":513,""demandBracket"":0,""stockBracket"":3,""creationQty"":134054,""consumptionQty"":0,""targetStock"":134054,""stock"":134054,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.2500"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128049226"",""name"":""Hazardous Environment Suits"",""cost_min"":""274.00"",""cost_max"":455.09917391304,""cost_mean"":""340.00"",""homebuy"":""56"",""homesell"":""52"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":612,""capacity"":1570163,""buyPrice"":0,""sellPrice"":456,""meanPrice"":340,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":1570163,""targetStock"":392540,""stock"":0,""demand"":1177623,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.1200"",""sec_illegal_min"":""1.22"",""sec_illegal_max"":""2.78"",""stolenmod"":""0.7500""},{""id"":""128673873"",""name"":""Micro Controllers"",""cost_min"":""3167.00"",""cost_max"":3794.6730434783,""cost_mean"":""3274.00"",""homebuy"":""87"",""homesell"":""86"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":24,""capacity"":61576,""buyPrice"":0,""sellPrice"":3795,""meanPrice"":3274,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":61576,""targetStock"":15394,""stock"":0,""demand"":46182,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.3200"",""sec_illegal_min"":""1.05"",""sec_illegal_max"":""1.37"",""stolenmod"":""0.7500""},{""id"":""128049227"",""name"":""Robotics"",""cost_min"":""1766.00"",""cost_max"":2226.992,""cost_mean"":""1856.00"",""homebuy"":""82"",""homesell"":""80"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":75,""capacity"":288633,""buyPrice"":0,""sellPrice"":2227,""meanPrice"":1856,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":288633,""targetStock"":72158,""stock"":0,""demand"":216475,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.4500"",""sec_illegal_min"":""1.08"",""sec_illegal_max"":""1.69"",""stolenmod"":""0.7500""},{""id"":""128682044"",""name"":""Conductive Fabrics"",""cost_min"":""472.00"",""cost_max"":702.68956521739,""cost_mean"":""507.00"",""homebuy"":""67"",""homesell"":""64"",""consumebuy"":""3"",""baseCreationQty"":0,""baseConsumptionQty"":348,""capacity"":892838,""buyPrice"":0,""sellPrice"":697,""meanPrice"":507,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":892838,""targetStock"":223209,""stock"":0,""demand"":657052,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Textiles"",""volumescale"":""1.2300"",""sec_illegal_min"":""1.16"",""sec_illegal_max"":""2.33"",""stolenmod"":""0.7500""},{""id"":""128049190"",""name"":""Leather"",""cost_min"":""175.00"",""cost_max"":334.88452173913,""cost_mean"":""205.00"",""homebuy"":""42"",""homesell"":""36"",""consumebuy"":""6"",""baseCreationQty"":0,""baseConsumptionQty"":1509,""capacity"":3871529,""buyPrice"":0,""sellPrice"":304,""meanPrice"":205,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":3871529,""targetStock"":967882,""stock"":0,""demand"":2481234,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Textiles"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.23"",""sec_illegal_max"":""2.88"",""stolenmod"":""0.7500""},{""id"":""128049191"",""name"":""Natural Fabrics"",""cost_min"":""403.00"",""cost_max"":615.978,""cost_mean"":""439.00"",""homebuy"":""64"",""homesell"":""60"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":271,""capacity"":695285,""buyPrice"":0,""sellPrice"":478,""meanPrice"":439,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":695285,""targetStock"":173821,""stock"":0,""demand"":273007,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Textiles"",""volumescale"":""1.2000"",""sec_illegal_min"":""1.18"",""sec_illegal_max"":""2.44"",""stolenmod"":""0.7500""},{""id"":""128049193"",""name"":""Synthetic Fabrics"",""cost_min"":""186.00"",""cost_max"":348.46330434783,""cost_mean"":""211.00"",""homebuy"":""45"",""homesell"":""40"",""consumebuy"":""6"",""baseCreationQty"":0,""baseConsumptionQty"":1022,""capacity"":2622070,""buyPrice"":0,""sellPrice"":343,""meanPrice"":211,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":2622070,""targetStock"":655517,""stock"":0,""demand"":1909435,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Textiles"",""volumescale"":""1.0200"",""sec_illegal_min"":""1.29"",""sec_illegal_max"":""3.34"",""stolenmod"":""0.7500""},{""id"":""128049244"",""name"":""Biowaste"",""cost_min"":36.267826086957,""cost_max"":""97.00"",""cost_mean"":""63.00"",""homebuy"":""27"",""homesell"":""20"",""consumebuy"":""7"",""baseCreationQty"":162,""baseConsumptionQty"":0,""capacity"":25977,""buyPrice"":18,""sellPrice"":13,""meanPrice"":63,""demandBracket"":0,""stockBracket"":2,""creationQty"":25977,""consumptionQty"":0,""targetStock"":25977,""stock"":14546,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Waste "",""volumescale"":""1.0000"",""sec_illegal_min"":""1.44"",""sec_illegal_max"":""4.47"",""stolenmod"":""0.7500""},{""id"":""128049248"",""name"":""Scrap"",""cost_min"":55.460869565217,""cost_max"":""120.00"",""cost_mean"":""48.00"",""homebuy"":""42"",""homesell"":""36"",""consumebuy"":""6"",""baseCreationQty"":453,""baseConsumptionQty"":0,""capacity"":290523,""buyPrice"":25,""sellPrice"":21,""meanPrice"":48,""demandBracket"":0,""stockBracket"":3,""creationQty"":290523,""consumptionQty"":0,""targetStock"":290523,""stock"":227726.405805,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Waste "",""volumescale"":""1.0000"",""sec_illegal_min"":""1.31"",""sec_illegal_max"":""3.48"",""stolenmod"":""0.7500""},{""id"":""128049236"",""name"":""Non Lethal Weapons"",""cost_min"":""1766.00"",""cost_max"":2226.992,""cost_mean"":""1837.00"",""homebuy"":""82"",""homesell"":""80"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":75,""capacity"":3425,""buyPrice"":0,""sellPrice"":2227,""meanPrice"":1837,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":3425,""targetStock"":856,""stock"":0,""demand"":2569,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Weapons"",""volumescale"":""1.4500"",""sec_illegal_min"":""1.08"",""sec_illegal_max"":""1.69"",""stolenmod"":""0.7500""},{""id"":""128049235"",""name"":""Reactive Armour"",""cost_min"":""2008.00"",""cost_max"":2501.5365217391,""cost_mean"":""2113.00"",""homebuy"":""84"",""homesell"":""82"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":68,""capacity"":9314,""buyPrice"":0,""sellPrice"":2502,""meanPrice"":2113,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":9314,""targetStock"":2328,""stock"":0,""demand"":6986,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Weapons"",""volumescale"":""1.4600"",""sec_illegal_min"":""1.08"",""sec_illegal_max"":""1.64"",""stolenmod"":""0.7500""}],""ships"":{""shipyard_list"":{""SideWinder"":{""id"":128049249,""name"":""SideWinder"",""basevalue"":32000,""sku"":""""},""Eagle"":{""id"":128049255,""name"":""Eagle"",""basevalue"":44800,""sku"":""""},""Hauler"":{""id"":128049261,""name"":""Hauler"",""basevalue"":52720,""sku"":""""},""Type7"":{""id"":128049297,""name"":""Type7"",""basevalue"":17472252,""sku"":""""},""Asp_Scout"":{""id"":128672276,""name"":""Asp_Scout"",""basevalue"":3961154,""sku"":""""},""Asp"":{""id"":128049303,""name"":""Asp"",""basevalue"":6661154,""sku"":""""},""Vulture"":{""id"":128049309,""name"":""Vulture"",""basevalue"":4925615,""sku"":""""},""Adder"":{""id"":128049267,""name"":""Adder"",""basevalue"":87808,""sku"":""""}},""unavailable_list"":[{""id"":128049321,""name"":""Federation_Dropship"",""basevalue"":14314205,""sku"":"""",""unavailableReason"":""Insufficient Rank"",""factionId"":""3"",""requiredRank"":3}]},""modules"":{""128681994"":{""id"":128681994,""category"":""weapon"",""name"":""Hpt_BeamLaser_Gimbal_Huge"",""cost"":7871544,""sku"":null},""128049434"":{""id"":128049434,""category"":""weapon"",""name"":""Hpt_BeamLaser_Gimbal_Large"",""cost"":2156544,""sku"":null},""128049430"":{""id"":128049430,""category"":""weapon"",""name"":""Hpt_BeamLaser_Fixed_Large"",""cost"":1059840,""sku"":null},""128049433"":{""id"":128049433,""category"":""weapon"",""name"":""Hpt_BeamLaser_Gimbal_Medium"",""cost"":450540,""sku"":null},""128049436"":{""id"":128049436,""category"":""weapon"",""name"":""Hpt_BeamLaser_Turret_Medium"",""cost"":1889910,""sku"":null},""128049432"":{""id"":128049432,""category"":""weapon"",""name"":""Hpt_BeamLaser_Gimbal_Small"",""cost"":67185,""sku"":null},""128049435"":{""id"":128049435,""category"":""weapon"",""name"":""Hpt_BeamLaser_Turret_Small"",""cost"":450000,""sku"":null},""128049428"":{""id"":128049428,""category"":""weapon"",""name"":""Hpt_BeamLaser_Fixed_Small"",""cost"":33687,""sku"":null},""128049389"":{""id"":128049389,""category"":""weapon"",""name"":""Hpt_PulseLaser_Turret_Medium"",""cost"":119520,""sku"":null},""128049388"":{""id"":128049388,""category"":""weapon"",""name"":""Hpt_PulseLaser_Turret_Small"",""cost"":23400,""sku"":null},""128049386"":{""id"":128049386,""category"":""weapon"",""name"":""Hpt_PulseLaser_Gimbal_Medium"",""cost"":31860,""sku"":null},""128049385"":{""id"":128049385,""category"":""weapon"",""name"":""Hpt_PulseLaser_Gimbal_Small"",""cost"":5940,""sku"":null},""128049383"":{""id"":128049383,""category"":""weapon"",""name"":""Hpt_PulseLaser_Fixed_Large"",""cost"":63360,""sku"":null},""128049382"":{""id"":128049382,""category"":""weapon"",""name"":""Hpt_PulseLaser_Fixed_Medium"",""cost"":15840,""sku"":null},""128049381"":{""id"":128049381,""category"":""weapon"",""name"":""Hpt_PulseLaser_Fixed_Small"",""cost"":1980,""sku"":null},""128049409"":{""id"":128049409,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Turret_Large"",""cost"":720360,""sku"":null},""128049408"":{""id"":128049408,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Turret_Medium"",""cost"":146520,""sku"":null},""128049407"":{""id"":128049407,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Turret_Small"",""cost"":47520,""sku"":null},""128049406"":{""id"":128049406,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Gimbal_Large"",""cost"":253440,""sku"":null},""128049404"":{""id"":128049404,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Gimbal_Small"",""cost"":7740,""sku"":null},""128049402"":{""id"":128049402,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Fixed_Large"",""cost"":126360,""sku"":null},""128049401"":{""id"":128049401,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Fixed_Medium"",""cost"":20700,""sku"":null},""128049466"":{""id"":128049466,""category"":""weapon"",""name"":""Hpt_PlasmaAccelerator_Fixed_Large"",""cost"":2746080,""sku"":null},""128049465"":{""id"":128049465,""category"":""weapon"",""name"":""Hpt_PlasmaAccelerator_Fixed_Medium"",""cost"":750780,""sku"":null},""128049450"":{""id"":128049450,""category"":""weapon"",""name"":""Hpt_Slugshot_Fixed_Large"",""cost"":1050624,""sku"":null},""128049453"":{""id"":128049453,""category"":""weapon"",""name"":""Hpt_Slugshot_Turret_Small"",""cost"":164160,""sku"":null},""128049451"":{""id"":128049451,""category"":""weapon"",""name"":""Hpt_Slugshot_Gimbal_Small"",""cost"":49248,""sku"":null},""128049452"":{""id"":128049452,""category"":""weapon"",""name"":""Hpt_Slugshot_Gimbal_Medium"",""cost"":393984,""sku"":null},""128049448"":{""id"":128049448,""category"":""weapon"",""name"":""Hpt_Slugshot_Fixed_Small"",""cost"":32400,""sku"":null},""128049449"":{""id"":128049449,""category"":""weapon"",""name"":""Hpt_Slugshot_Fixed_Medium"",""cost"":262656,""sku"":null},""128049462"":{""id"":128049462,""category"":""weapon"",""name"":""Hpt_MultiCannon_Turret_Small"",""cost"":73440,""sku"":null},""128049463"":{""id"":128049463,""category"":""weapon"",""name"":""Hpt_MultiCannon_Turret_Medium"",""cost"":1163520,""sku"":null},""128049459"":{""id"":128049459,""category"":""weapon"",""name"":""Hpt_MultiCannon_Gimbal_Small"",""cost"":12825,""sku"":null},""128049460"":{""id"":128049460,""category"":""weapon"",""name"":""Hpt_MultiCannon_Gimbal_Medium"",""cost"":51300,""sku"":null},""128049456"":{""id"":128049456,""category"":""weapon"",""name"":""Hpt_MultiCannon_Fixed_Medium"",""cost"":34200,""sku"":null},""128049510"":{""id"":128049510,""category"":""weapon"",""name"":""Hpt_AdvancedTorpPylon_Fixed_Medium"",""cost"":40320,""sku"":null},""128049509"":{""id"":128049509,""category"":""weapon"",""name"":""Hpt_AdvancedTorpPylon_Fixed_Small"",""cost"":10080,""sku"":null},""128666724"":{""id"":128666724,""category"":""weapon"",""name"":""Hpt_DumbfireMissileRack_Fixed_Small"",""cost"":28958,""sku"":null},""128671448"":{""id"":128671448,""category"":""weapon"",""name"":""Hpt_MineLauncher_Fixed_Small_Impulse"",""cost"":32751,""sku"":null},""128049500"":{""id"":128049500,""category"":""weapon"",""name"":""Hpt_MineLauncher_Fixed_Small"",""cost"":21834,""sku"":null},""128049489"":{""id"":128049489,""category"":""weapon"",""name"":""Hpt_Railgun_Fixed_Medium"",""cost"":371520,""sku"":null},""128049488"":{""id"":128049488,""category"":""weapon"",""name"":""Hpt_Railgun_Fixed_Small"",""cost"":46440,""sku"":null},""128049493"":{""id"":128049493,""category"":""weapon"",""name"":""Hpt_BasicMissileRack_Fixed_Medium"",""cost"":461160,""sku"":null},""128049492"":{""id"":128049492,""category"":""weapon"",""name"":""Hpt_BasicMissileRack_Fixed_Small"",""cost"":65340,""sku"":null},""128049525"":{""id"":128049525,""category"":""utility"",""name"":""Hpt_MiningLaser_Fixed_Small"",""cost"":6800,""sku"":null},""128049516"":{""id"":128049516,""category"":""utility"",""name"":""Hpt_ElectronicCountermeasure_Tiny"",""cost"":12500,""sku"":null},""128049549"":{""id"":128049549,""category"":""utility"",""name"":""Int_DockingComputer_Standard"",""cost"":4500,""sku"":null},""128662532"":{""id"":128662532,""category"":""utility"",""name"":""Hpt_CrimeScanner_Size0_Class3"",""cost"":121899,""sku"":null},""128662531"":{""id"":128662531,""category"":""utility"",""name"":""Hpt_CrimeScanner_Size0_Class2"",""cost"":40633,""sku"":null},""128662530"":{""id"":128662530,""category"":""utility"",""name"":""Hpt_CrimeScanner_Size0_Class1"",""cost"":13544,""sku"":null},""128662524"":{""id"":128662524,""category"":""utility"",""name"":""Hpt_CargoScanner_Size0_Class5"",""cost"":1097095,""sku"":null},""128662520"":{""id"":128662520,""category"":""utility"",""name"":""Hpt_CargoScanner_Size0_Class1"",""cost"":13544,""sku"":null},""128662527"":{""id"":128662527,""category"":""utility"",""name"":""Hpt_CloudScanner_Size0_Class3"",""cost"":121899,""sku"":null},""128662526"":{""id"":128662526,""category"":""utility"",""name"":""Hpt_CloudScanner_Size0_Class2"",""cost"":40633,""sku"":null},""128662525"":{""id"":128662525,""category"":""utility"",""name"":""Hpt_CloudScanner_Size0_Class1"",""cost"":13544,""sku"":null},""128662529"":{""id"":128662529,""category"":""utility"",""name"":""Hpt_CloudScanner_Size0_Class5"",""cost"":1097095,""sku"":null},""128662528"":{""id"":128662528,""category"":""utility"",""name"":""Hpt_CloudScanner_Size0_Class4"",""cost"":365698,""sku"":null},""128049252"":{""id"":128049252,""category"":""module"",""name"":""SideWinder_Armour_Grade3"",""cost"":80320,""sku"":null},""128049251"":{""id"":128049251,""category"":""module"",""name"":""SideWinder_Armour_Grade2"",""cost"":25600,""sku"":null},""128049250"":{""id"":128049250,""category"":""module"",""name"":""SideWinder_Armour_Grade1"",""cost"":0,""sku"":null},""128049253"":{""id"":128049253,""category"":""module"",""name"":""SideWinder_Armour_Mirrored"",""cost"":132064,""sku"":null},""128049254"":{""id"":128049254,""category"":""module"",""name"":""SideWinder_Armour_Reactive"",""cost"":139424,""sku"":null},""128049258"":{""id"":128049258,""category"":""module"",""name"":""Eagle_Armour_Grade3"",""cost"":90048,""sku"":null},""128049257"":{""id"":128049257,""category"":""module"",""name"":""Eagle_Armour_Grade2"",""cost"":26880,""sku"":null},""128049256"":{""id"":128049256,""category"":""module"",""name"":""Eagle_Armour_Grade1"",""cost"":0,""sku"":null},""128049264"":{""id"":128049264,""category"":""module"",""name"":""Hauler_Armour_Grade3"",""cost"":185047,""sku"":null},""128049263"":{""id"":128049263,""category"":""module"",""name"":""Hauler_Armour_Grade2"",""cost"":42176,""sku"":null},""128049262"":{""id"":128049262,""category"":""module"",""name"":""Hauler_Armour_Grade1"",""cost"":0,""sku"":null},""128049300"":{""id"":128049300,""category"":""module"",""name"":""Type7_Armour_Grade3"",""cost"":15725026,""sku"":null},""128049299"":{""id"":128049299,""category"":""module"",""name"":""Type7_Armour_Grade2"",""cost"":6988900,""sku"":null},""128049298"":{""id"":128049298,""category"":""module"",""name"":""Type7_Armour_Grade1"",""cost"":0,""sku"":null},""128049301"":{""id"":128049301,""category"":""module"",""name"":""Type7_Armour_Mirrored"",""cost"":37163480,""sku"":null},""128049302"":{""id"":128049302,""category"":""module"",""name"":""Type7_Armour_Reactive"",""cost"":41182097,""sku"":null},""128672280"":{""id"":128672280,""category"":""module"",""name"":""Asp_Scout_Armour_Grade3"",""cost"":3565038,""sku"":null},""128672279"":{""id"":128672279,""category"":""module"",""name"":""Asp_Scout_Armour_Grade2"",""cost"":1584461,""sku"":null},""128672278"":{""id"":128672278,""category"":""module"",""name"":""Asp_Scout_Armour_Grade1"",""cost"":0,""sku"":null},""128672281"":{""id"":128672281,""category"":""module"",""name"":""Asp_Scout_Armour_Mirrored"",""cost"":8425374,""sku"":null},""128672282"":{""id"":128672282,""category"":""module"",""name"":""Asp_Scout_Armour_Reactive"",""cost"":9336439,""sku"":null},""128049306"":{""id"":128049306,""category"":""module"",""name"":""Asp_Armour_Grade3"",""cost"":5995038,""sku"":null},""128049305"":{""id"":128049305,""category"":""module"",""name"":""Asp_Armour_Grade2"",""cost"":2664461,""sku"":null},""128049304"":{""id"":128049304,""category"":""module"",""name"":""Asp_Armour_Grade1"",""cost"":0,""sku"":null},""128049307"":{""id"":128049307,""category"":""module"",""name"":""Asp_Armour_Mirrored"",""cost"":14168274,""sku"":null},""128049308"":{""id"":128049308,""category"":""module"",""name"":""Asp_Armour_Reactive"",""cost"":15700339,""sku"":null},""128049312"":{""id"":128049312,""category"":""module"",""name"":""Vulture_Armour_Grade3"",""cost"":4433053,""sku"":null},""128049311"":{""id"":128049311,""category"":""module"",""name"":""Vulture_Armour_Grade2"",""cost"":1970246,""sku"":null},""128049310"":{""id"":128049310,""category"":""module"",""name"":""Vulture_Armour_Grade1"",""cost"":0,""sku"":null},""128049313"":{""id"":128049313,""category"":""module"",""name"":""Vulture_Armour_Mirrored"",""cost"":10476783,""sku"":null},""128049314"":{""id"":128049314,""category"":""module"",""name"":""Vulture_Armour_Reactive"",""cost"":11609674,""sku"":null},""128049270"":{""id"":128049270,""category"":""module"",""name"":""Adder_Armour_Grade3"",""cost"":79027,""sku"":null},""128049269"":{""id"":128049269,""category"":""module"",""name"":""Adder_Armour_Grade2"",""cost"":35123,""sku"":null},""128049268"":{""id"":128049268,""category"":""module"",""name"":""Adder_Armour_Grade1"",""cost"":0,""sku"":null},""128049271"":{""id"":128049271,""category"":""module"",""name"":""Adder_Armour_Mirrored"",""cost"":186767,""sku"":null},""128049272"":{""id"":128049272,""category"":""module"",""name"":""Adder_Armour_Reactive"",""cost"":206963,""sku"":null},""128049324"":{""id"":128049324,""category"":""module"",""name"":""Federation_Dropship_Armour_Grade3"",""cost"":12882784,""sku"":null},""128049323"":{""id"":128049323,""category"":""module"",""name"":""Federation_Dropship_Armour_Grade2"",""cost"":5725682,""sku"":null},""128049322"":{""id"":128049322,""category"":""module"",""name"":""Federation_Dropship_Armour_Grade1"",""cost"":0,""sku"":null},""128049325"":{""id"":128049325,""category"":""module"",""name"":""Federation_Dropship_Armour_Mirrored"",""cost"":30446314,""sku"":null},""128049326"":{""id"":128049326,""category"":""module"",""name"":""Federation_Dropship_Armour_Reactive"",""cost"":33738581,""sku"":null},""128662535"":{""id"":128662535,""category"":""module"",""name"":""Int_StellarBodyDiscoveryScanner_Standard"",""cost"":1000,""sku"":null},""128064338"":{""id"":128064338,""category"":""module"",""name"":""Int_CargoRack_Size1_Class1"",""cost"":1000,""sku"":null},""128666684"":{""id"":128666684,""category"":""module"",""name"":""Int_Refinery_Size1_Class1"",""cost"":6000,""sku"":null},""128666644"":{""id"":128666644,""category"":""module"",""name"":""Int_FuelScoop_Size1_Class1"",""cost"":309,""sku"":null},""128672317"":{""id"":128672317,""category"":""module"",""name"":""Int_PlanetApproachSuite"",""cost"":500,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128666704"":{""id"":128666704,""category"":""module"",""name"":""Int_FSDInterdictor_Size1_Class1"",""cost"":12000,""sku"":null},""128066532"":{""id"":128066532,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size1_Class1"",""cost"":600,""sku"":null},""128064263"":{""id"":128064263,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class1"",""cost"":1978,""sku"":null},""128064112"":{""id"":128064112,""category"":""module"",""name"":""Int_Hyperdrive_Size3_Class5"",""cost"":507912,""sku"":null},""128064107"":{""id"":128064107,""category"":""module"",""name"":""Int_Hyperdrive_Size2_Class5"",""cost"":160224,""sku"":null},""128064111"":{""id"":128064111,""category"":""module"",""name"":""Int_Hyperdrive_Size3_Class4"",""cost"":169304,""sku"":null},""128064110"":{""id"":128064110,""category"":""module"",""name"":""Int_Hyperdrive_Size3_Class3"",""cost"":56435,""sku"":null},""128064105"":{""id"":128064105,""category"":""module"",""name"":""Int_Hyperdrive_Size2_Class3"",""cost"":17803,""sku"":null},""128064109"":{""id"":128064109,""category"":""module"",""name"":""Int_Hyperdrive_Size3_Class2"",""cost"":18812,""sku"":null},""128064104"":{""id"":128064104,""category"":""module"",""name"":""Int_Hyperdrive_Size2_Class2"",""cost"":5934,""sku"":null},""128064108"":{""id"":128064108,""category"":""module"",""name"":""Int_Hyperdrive_Size3_Class1"",""cost"":6271,""sku"":null},""128064103"":{""id"":128064103,""category"":""module"",""name"":""Int_Hyperdrive_Size2_Class1"",""cost"":1978,""sku"":null},""128064272"":{""id"":128064272,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class5"",""cost"":507912,""sku"":null},""128064267"":{""id"":128064267,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class5"",""cost"":160224,""sku"":null},""128064271"":{""id"":128064271,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class4"",""cost"":169304,""sku"":null},""128064266"":{""id"":128064266,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class4"",""cost"":53408,""sku"":null},""128671333"":{""id"":128671333,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class3_Fast"",""cost"":84653,""sku"":null},""128064270"":{""id"":128064270,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class3"",""cost"":56435,""sku"":null},""128671332"":{""id"":128671332,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class3_Fast"",""cost"":26705,""sku"":null},""128671331"":{""id"":128671331,""category"":""module"",""name"":""Int_ShieldGenerator_Size1_Class3_Fast"",""cost"":7713,""sku"":null},""128064265"":{""id"":128064265,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class3"",""cost"":17803,""sku"":null},""128064269"":{""id"":128064269,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class2"",""cost"":18812,""sku"":null},""128064264"":{""id"":128064264,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class2"",""cost"":5934,""sku"":null},""128064268"":{""id"":128064268,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class1"",""cost"":6271,""sku"":null},""128672293"":{""id"":128672293,""category"":""module"",""name"":""Int_BuggyBay_Size6_Class2"",""cost"":691200,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128672291"":{""id"":128672291,""category"":""module"",""name"":""Int_BuggyBay_Size4_Class2"",""cost"":86400,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128672292"":{""id"":128672292,""category"":""module"",""name"":""Int_BuggyBay_Size6_Class1"",""cost"":576000,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128672289"":{""id"":128672289,""category"":""module"",""name"":""Int_BuggyBay_Size2_Class2"",""cost"":21600,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128672290"":{""id"":128672290,""category"":""module"",""name"":""Int_BuggyBay_Size4_Class1"",""cost"":72000,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128672288"":{""id"":128672288,""category"":""module"",""name"":""Int_BuggyBay_Size2_Class1"",""cost"":18000,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128671338"":{""id"":128671338,""category"":""module"",""name"":""Int_ShieldGenerator_Size8_Class3_Fast"",""cost"":27097748,""sku"":null},""128671337"":{""id"":128671337,""category"":""module"",""name"":""Int_ShieldGenerator_Size7_Class3_Fast"",""cost"":8548185,""sku"":null},""128064294"":{""id"":128064294,""category"":""module"",""name"":""Int_ShieldGenerator_Size8_Class2"",""cost"":6021722,""sku"":null},""128064293"":{""id"":128064293,""category"":""module"",""name"":""Int_ShieldGenerator_Size8_Class1"",""cost"":2007241,""sku"":null},""128064288"":{""id"":128064288,""category"":""module"",""name"":""Int_ShieldGenerator_Size7_Class1"",""cost"":633199,""sku"":null},""128666712"":{""id"":128666712,""category"":""module"",""name"":""Int_FSDInterdictor_Size1_Class3"",""cost"":108000,""sku"":null},""128666709"":{""id"":128666709,""category"":""module"",""name"":""Int_FSDInterdictor_Size2_Class2"",""cost"":100800,""sku"":null},""128666708"":{""id"":128666708,""category"":""module"",""name"":""Int_FSDInterdictor_Size1_Class2"",""cost"":36000,""sku"":null},""128666705"":{""id"":128666705,""category"":""module"",""name"":""Int_FSDInterdictor_Size2_Class1"",""cost"":33600,""sku"":null},""128064041"":{""id"":128064041,""category"":""module"",""name"":""Int_Powerplant_Size3_Class4"",""cost"":169304,""sku"":null},""128064036"":{""id"":128064036,""category"":""module"",""name"":""Int_Powerplant_Size2_Class4"",""cost"":53408,""sku"":null},""128064040"":{""id"":128064040,""category"":""module"",""name"":""Int_Powerplant_Size3_Class3"",""cost"":56435,""sku"":null},""128064035"":{""id"":128064035,""category"":""module"",""name"":""Int_Powerplant_Size2_Class3"",""cost"":17803,""sku"":null},""128064039"":{""id"":128064039,""category"":""module"",""name"":""Int_Powerplant_Size3_Class2"",""cost"":18812,""sku"":null},""128064034"":{""id"":128064034,""category"":""module"",""name"":""Int_Powerplant_Size2_Class2"",""cost"":5934,""sku"":null},""128064038"":{""id"":128064038,""category"":""module"",""name"":""Int_Powerplant_Size3_Class1"",""cost"":6271,""sku"":null},""128064033"":{""id"":128064033,""category"":""module"",""name"":""Int_Powerplant_Size2_Class1"",""cost"":1978,""sku"":null},""128064345"":{""id"":128064345,""category"":""module"",""name"":""Int_CargoRack_Size8_Class1"",""cost"":3829866,""sku"":null},""128064344"":{""id"":128064344,""category"":""module"",""name"":""Int_CargoRack_Size7_Class1"",""cost"":1178420,""sku"":null},""128064343"":{""id"":128064343,""category"":""module"",""name"":""Int_CargoRack_Size6_Class1"",""cost"":362591,""sku"":null},""128064342"":{""id"":128064342,""category"":""module"",""name"":""Int_CargoRack_Size5_Class1"",""cost"":111566,""sku"":null},""128064341"":{""id"":128064341,""category"":""module"",""name"":""Int_CargoRack_Size4_Class1"",""cost"":34328,""sku"":null},""128064340"":{""id"":128064340,""category"":""module"",""name"":""Int_CargoRack_Size3_Class1"",""cost"":10563,""sku"":null},""128064339"":{""id"":128064339,""category"":""module"",""name"":""Int_CargoRack_Size2_Class1"",""cost"":3250,""sku"":null},""128064353"":{""id"":128064353,""category"":""module"",""name"":""Int_FuelTank_Size8_Class3"",""cost"":5428429,""sku"":null},""128064352"":{""id"":128064352,""category"":""module"",""name"":""Int_FuelTank_Size7_Class3"",""cost"":1780914,""sku"":null},""128064351"":{""id"":128064351,""category"":""module"",""name"":""Int_FuelTank_Size6_Class3"",""cost"":341577,""sku"":null},""128064350"":{""id"":128064350,""category"":""module"",""name"":""Int_FuelTank_Size5_Class3"",""cost"":97754,""sku"":null},""128064349"":{""id"":128064349,""category"":""module"",""name"":""Int_FuelTank_Size4_Class3"",""cost"":24734,""sku"":null},""128064348"":{""id"":128064348,""category"":""module"",""name"":""Int_FuelTank_Size3_Class3"",""cost"":7063,""sku"":null},""128064347"":{""id"":128064347,""category"":""module"",""name"":""Int_FuelTank_Size2_Class3"",""cost"":3750,""sku"":null},""128064346"":{""id"":128064346,""category"":""module"",""name"":""Int_FuelTank_Size1_Class3"",""cost"":1000,""sku"":null},""128671252"":{""id"":128671252,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size1_Class4"",""cost"":4800,""sku"":null},""128671264"":{""id"":128671264,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size7_Class1"",""cost"":437400,""sku"":null},""128671260"":{""id"":128671260,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size5_Class2"",""cost"":97200,""sku"":null},""128671256"":{""id"":128671256,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size3_Class3"",""cost"":21600,""sku"":null},""128671251"":{""id"":128671251,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size1_Class3"",""cost"":2400,""sku"":null},""128671259"":{""id"":128671259,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size5_Class1"",""cost"":48600,""sku"":null},""128671255"":{""id"":128671255,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size3_Class2"",""cost"":10800,""sku"":null},""128671250"":{""id"":128671250,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size1_Class2"",""cost"":1200,""sku"":null},""128671254"":{""id"":128671254,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size3_Class1"",""cost"":5400,""sku"":null},""128671249"":{""id"":128671249,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size1_Class1"",""cost"":600,""sku"":null},""128666701"":{""id"":128666701,""category"":""module"",""name"":""Int_Refinery_Size2_Class5"",""cost"":1020600,""sku"":null},""128666700"":{""id"":128666700,""category"":""module"",""name"":""Int_Refinery_Size1_Class5"",""cost"":486000,""sku"":null},""128666693"":{""id"":128666693,""category"":""module"",""name"":""Int_Refinery_Size2_Class3"",""cost"":113400,""sku"":null},""128666696"":{""id"":128666696,""category"":""module"",""name"":""Int_Refinery_Size1_Class4"",""cost"":162000,""sku"":null},""128666689"":{""id"":128666689,""category"":""module"",""name"":""Int_Refinery_Size2_Class2"",""cost"":37800,""sku"":null},""128666692"":{""id"":128666692,""category"":""module"",""name"":""Int_Refinery_Size1_Class3"",""cost"":54000,""sku"":null},""128666688"":{""id"":128666688,""category"":""module"",""name"":""Int_Refinery_Size1_Class2"",""cost"":18000,""sku"":null},""128666685"":{""id"":128666685,""category"":""module"",""name"":""Int_Refinery_Size2_Class1"",""cost"":12600,""sku"":null},""128064255"":{""id"":128064255,""category"":""module"",""name"":""Int_Sensors_Size8_Class3"",""cost"":4359903,""sku"":null},""128064250"":{""id"":128064250,""category"":""module"",""name"":""Int_Sensors_Size7_Class3"",""cost"":1557108,""sku"":null},""128064254"":{""id"":128064254,""category"":""module"",""name"":""Int_Sensors_Size8_Class2"",""cost"":1743961,""sku"":null},""128064249"":{""id"":128064249,""category"":""module"",""name"":""Int_Sensors_Size7_Class2"",""cost"":622843,""sku"":null},""128064253"":{""id"":128064253,""category"":""module"",""name"":""Int_Sensors_Size8_Class1"",""cost"":697584,""sku"":null},""128064248"":{""id"":128064248,""category"":""module"",""name"":""Int_Sensors_Size7_Class1"",""cost"":249137,""sku"":null},""128064191"":{""id"":128064191,""category"":""module"",""name"":""Int_PowerDistributor_Size3_Class4"",""cost"":63333,""sku"":null},""128064181"":{""id"":128064181,""category"":""module"",""name"":""Int_PowerDistributor_Size1_Class4"",""cost"":8078,""sku"":null},""128064186"":{""id"":128064186,""category"":""module"",""name"":""Int_PowerDistributor_Size2_Class4"",""cost"":22619,""sku"":null},""128064190"":{""id"":128064190,""category"":""module"",""name"":""Int_PowerDistributor_Size3_Class3"",""cost"":25333,""sku"":null},""128064185"":{""id"":128064185,""category"":""module"",""name"":""Int_PowerDistributor_Size2_Class3"",""cost"":9048,""sku"":null},""128064180"":{""id"":128064180,""category"":""module"",""name"":""Int_PowerDistributor_Size1_Class3"",""cost"":3231,""sku"":null},""128064189"":{""id"":128064189,""category"":""module"",""name"":""Int_PowerDistributor_Size3_Class2"",""cost"":10133,""sku"":null},""128064179"":{""id"":128064179,""category"":""module"",""name"":""Int_PowerDistributor_Size1_Class2"",""cost"":1293,""sku"":null},""128064184"":{""id"":128064184,""category"":""module"",""name"":""Int_PowerDistributor_Size2_Class2"",""cost"":3619,""sku"":null},""128064188"":{""id"":128064188,""category"":""module"",""name"":""Int_PowerDistributor_Size3_Class1"",""cost"":4053,""sku"":null},""128064183"":{""id"":128064183,""category"":""module"",""name"":""Int_PowerDistributor_Size2_Class1"",""cost"":1448,""sku"":null},""128064178"":{""id"":128064178,""category"":""module"",""name"":""Int_PowerDistributor_Size1_Class1"",""cost"":517,""sku"":null},""128064215"":{""id"":128064215,""category"":""module"",""name"":""Int_PowerDistributor_Size8_Class3"",""cost"":4359903,""sku"":null},""128064210"":{""id"":128064210,""category"":""module"",""name"":""Int_PowerDistributor_Size7_Class3"",""cost"":1557108,""sku"":null},""128064209"":{""id"":128064209,""category"":""module"",""name"":""Int_PowerDistributor_Size7_Class2"",""cost"":622843,""sku"":null},""128064213"":{""id"":128064213,""category"":""module"",""name"":""Int_PowerDistributor_Size8_Class1"",""cost"":697584,""sku"":null},""128064208"":{""id"":128064208,""category"":""module"",""name"":""Int_PowerDistributor_Size7_Class1"",""cost"":249137,""sku"":null},""128064077"":{""id"":128064077,""category"":""module"",""name"":""Int_Engine_Size3_Class5"",""cost"":507912,""sku"":null},""128064076"":{""id"":128064076,""category"":""module"",""name"":""Int_Engine_Size3_Class4"",""cost"":169304,""sku"":null},""128064071"":{""id"":128064071,""category"":""module"",""name"":""Int_Engine_Size2_Class4"",""cost"":53408,""sku"":null},""128064075"":{""id"":128064075,""category"":""module"",""name"":""Int_Engine_Size3_Class3"",""cost"":56435,""sku"":null},""128064070"":{""id"":128064070,""category"":""module"",""name"":""Int_Engine_Size2_Class3"",""cost"":17803,""sku"":null},""128064074"":{""id"":128064074,""category"":""module"",""name"":""Int_Engine_Size3_Class2"",""cost"":18812,""sku"":null},""128064069"":{""id"":128064069,""category"":""module"",""name"":""Int_Engine_Size2_Class2"",""cost"":5934,""sku"":null},""128064073"":{""id"":128064073,""category"":""module"",""name"":""Int_Engine_Size3_Class1"",""cost"":6271,""sku"":null},""128064068"":{""id"":128064068,""category"":""module"",""name"":""Int_Engine_Size2_Class1"",""cost"":1978,""sku"":null},""128666677"":{""id"":128666677,""category"":""module"",""name"":""Int_FuelScoop_Size2_Class5"",""cost"":284844,""sku"":null},""128666670"":{""id"":128666670,""category"":""module"",""name"":""Int_FuelScoop_Size3_Class4"",""cost"":225738,""sku"":null},""128666662"":{""id"":128666662,""category"":""module"",""name"":""Int_FuelScoop_Size3_Class3"",""cost"":56435,""sku"":null},""128666676"":{""id"":128666676,""category"":""module"",""name"":""Int_FuelScoop_Size1_Class5"",""cost"":82270,""sku"":null},""128666661"":{""id"":128666661,""category"":""module"",""name"":""Int_FuelScoop_Size2_Class3"",""cost"":17803,""sku"":null},""128666660"":{""id"":128666660,""category"":""module"",""name"":""Int_FuelScoop_Size1_Class3"",""cost"":5142,""sku"":null},""128666654"":{""id"":128666654,""category"":""module"",""name"":""Int_FuelScoop_Size3_Class2"",""cost"":14109,""sku"":null},""128666653"":{""id"":128666653,""category"":""module"",""name"":""Int_FuelScoop_Size2_Class2"",""cost"":4451,""sku"":null},""128666646"":{""id"":128666646,""category"":""module"",""name"":""Int_FuelScoop_Size3_Class1"",""cost"":3386,""sku"":null},""128666652"":{""id"":128666652,""category"":""module"",""name"":""Int_FuelScoop_Size1_Class2"",""cost"":1285,""sku"":null},""128666645"":{""id"":128666645,""category"":""module"",""name"":""Int_FuelScoop_Size2_Class1"",""cost"":1068,""sku"":null},""128666703"":{""id"":128666703,""category"":""module"",""name"":""Int_Refinery_Size4_Class5"",""cost"":4500846,""sku"":null},""128666695"":{""id"":128666695,""category"":""module"",""name"":""Int_Refinery_Size4_Class3"",""cost"":500094,""sku"":null},""128666702"":{""id"":128666702,""category"":""module"",""name"":""Int_Refinery_Size3_Class5"",""cost"":2143260,""sku"":null},""128666698"":{""id"":128666698,""category"":""module"",""name"":""Int_Refinery_Size3_Class4"",""cost"":714420,""sku"":null},""128666694"":{""id"":128666694,""category"":""module"",""name"":""Int_Refinery_Size3_Class3"",""cost"":238140,""sku"":null},""128666691"":{""id"":128666691,""category"":""module"",""name"":""Int_Refinery_Size4_Class2"",""cost"":166698,""sku"":null},""128666687"":{""id"":128666687,""category"":""module"",""name"":""Int_Refinery_Size4_Class1"",""cost"":55566,""sku"":null},""128666690"":{""id"":128666690,""category"":""module"",""name"":""Int_Refinery_Size3_Class2"",""cost"":79380,""sku"":null},""128666686"":{""id"":128666686,""category"":""module"",""name"":""Int_Refinery_Size3_Class1"",""cost"":26460,""sku"":null},""128671284"":{""id"":128671284,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size7_Class1"",""cost"":437400,""sku"":null},""128671280"":{""id"":128671280,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size5_Class2"",""cost"":97200,""sku"":null},""128671276"":{""id"":128671276,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size3_Class3"",""cost"":21600,""sku"":null},""128671271"":{""id"":128671271,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size1_Class3"",""cost"":2400,""sku"":null},""128671279"":{""id"":128671279,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size5_Class1"",""cost"":48600,""sku"":null},""128671275"":{""id"":128671275,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size3_Class2"",""cost"":10800,""sku"":null},""128671270"":{""id"":128671270,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size1_Class2"",""cost"":1200,""sku"":null},""128671274"":{""id"":128671274,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size3_Class1"",""cost"":5400,""sku"":null},""128671269"":{""id"":128671269,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size1_Class1"",""cost"":600,""sku"":null},""128667637"":{""id"":128667637,""category"":""module"",""name"":""Int_Repairer_Size8_Class5"",""cost"":49589823,""sku"":null},""128667629"":{""id"":128667629,""category"":""module"",""name"":""Int_Repairer_Size8_Class4"",""cost"":16529941,""sku"":null},""128667636"":{""id"":128667636,""category"":""module"",""name"":""Int_Repairer_Size7_Class5"",""cost"":27549901,""sku"":null},""128667621"":{""id"":128667621,""category"":""module"",""name"":""Int_Repairer_Size8_Class3"",""cost"":5509980,""sku"":null},""128667635"":{""id"":128667635,""category"":""module"",""name"":""Int_Repairer_Size6_Class5"",""cost"":15305501,""sku"":null},""128667620"":{""id"":128667620,""category"":""module"",""name"":""Int_Repairer_Size7_Class3"",""cost"":3061100,""sku"":null},""128667627"":{""id"":128667627,""category"":""module"",""name"":""Int_Repairer_Size6_Class4"",""cost"":5101834,""sku"":null},""128667634"":{""id"":128667634,""category"":""module"",""name"":""Int_Repairer_Size5_Class5"",""cost"":8503056,""sku"":null},""128667633"":{""id"":128667633,""category"":""module"",""name"":""Int_Repairer_Size4_Class5"",""cost"":4723920,""sku"":null},""128667613"":{""id"":128667613,""category"":""module"",""name"":""Int_Repairer_Size8_Class2"",""cost"":1836660,""sku"":null},""128667619"":{""id"":128667619,""category"":""module"",""name"":""Int_Repairer_Size6_Class3"",""cost"":1700611,""sku"":null},""128667612"":{""id"":128667612,""category"":""module"",""name"":""Int_Repairer_Size7_Class2"",""cost"":1020367,""sku"":null},""128667618"":{""id"":128667618,""category"":""module"",""name"":""Int_Repairer_Size5_Class3"",""cost"":944784,""sku"":null},""128667625"":{""id"":128667625,""category"":""module"",""name"":""Int_Repairer_Size4_Class4"",""cost"":1574640,""sku"":null},""128663561"":{""id"":128663561,""category"":""module"",""name"":""Int_StellarBodyDiscoveryScanner_Advanced"",""cost"":1545000,""sku"":null},""128663560"":{""id"":128663560,""category"":""module"",""name"":""Int_StellarBodyDiscoveryScanner_Intermediate"",""cost"":505000,""sku"":null},""128666634"":{""id"":128666634,""category"":""module"",""name"":""Int_DetailedSurfaceScanner_Tiny"",""cost"":250000,""sku"":null},""128064147"":{""id"":128064147,""category"":""module"",""name"":""Int_LifeSupport_Size2_Class5"",""cost"":56547,""sku"":null},""128064142"":{""id"":128064142,""category"":""module"",""name"":""Int_LifeSupport_Size1_Class5"",""cost"":20195,""sku"":null},""128064151"":{""id"":128064151,""category"":""module"",""name"":""Int_LifeSupport_Size3_Class4"",""cost"":63333,""sku"":null},""128064141"":{""id"":128064141,""category"":""module"",""name"":""Int_LifeSupport_Size1_Class4"",""cost"":8078,""sku"":null},""128064146"":{""id"":128064146,""category"":""module"",""name"":""Int_LifeSupport_Size2_Class4"",""cost"":22619,""sku"":null},""128064150"":{""id"":128064150,""category"":""module"",""name"":""Int_LifeSupport_Size3_Class3"",""cost"":25333,""sku"":null},""128064145"":{""id"":128064145,""category"":""module"",""name"":""Int_LifeSupport_Size2_Class3"",""cost"":9048,""sku"":null},""128064140"":{""id"":128064140,""category"":""module"",""name"":""Int_LifeSupport_Size1_Class3"",""cost"":3231,""sku"":null},""128064149"":{""id"":128064149,""category"":""module"",""name"":""Int_LifeSupport_Size3_Class2"",""cost"":10133,""sku"":null},""128064139"":{""id"":128064139,""category"":""module"",""name"":""Int_LifeSupport_Size1_Class2"",""cost"":1293,""sku"":null},""128064144"":{""id"":128064144,""category"":""module"",""name"":""Int_LifeSupport_Size2_Class2"",""cost"":3619,""sku"":null},""128064148"":{""id"":128064148,""category"":""module"",""name"":""Int_LifeSupport_Size3_Class1"",""cost"":4053,""sku"":null},""128064143"":{""id"":128064143,""category"":""module"",""name"":""Int_LifeSupport_Size2_Class1"",""cost"":1448,""sku"":null},""128671263"":{""id"":128671263,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size5_Class5"",""cost"":777600,""sku"":null},""128671258"":{""id"":128671258,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size3_Class5"",""cost"":86400,""sku"":null},""128671267"":{""id"":128671267,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size7_Class4"",""cost"":3499200,""sku"":null},""128671262"":{""id"":128671262,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size5_Class4"",""cost"":388800,""sku"":null},""128671266"":{""id"":128671266,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size7_Class3"",""cost"":1749600,""sku"":null},""128671265"":{""id"":128671265,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size7_Class2"",""cost"":874800,""sku"":null},""128064231"":{""id"":128064231,""category"":""module"",""name"":""Int_Sensors_Size3_Class4"",""cost"":63333,""sku"":null},""128064221"":{""id"":128064221,""category"":""module"",""name"":""Int_Sensors_Size1_Class4"",""cost"":8078,""sku"":null},""128064226"":{""id"":128064226,""category"":""module"",""name"":""Int_Sensors_Size2_Class4"",""cost"":22619,""sku"":null},""128064230"":{""id"":128064230,""category"":""module"",""name"":""Int_Sensors_Size3_Class3"",""cost"":25333,""sku"":null},""128064225"":{""id"":128064225,""category"":""module"",""name"":""Int_Sensors_Size2_Class3"",""cost"":9048,""sku"":null},""128064220"":{""id"":128064220,""category"":""module"",""name"":""Int_Sensors_Size1_Class3"",""cost"":3231,""sku"":null},""128064229"":{""id"":128064229,""category"":""module"",""name"":""Int_Sensors_Size3_Class2"",""cost"":10133,""sku"":null},""128064219"":{""id"":128064219,""category"":""module"",""name"":""Int_Sensors_Size1_Class2"",""cost"":1293,""sku"":null},""128064224"":{""id"":128064224,""category"":""module"",""name"":""Int_Sensors_Size2_Class2"",""cost"":3619,""sku"":null},""128064228"":{""id"":128064228,""category"":""module"",""name"":""Int_Sensors_Size3_Class1"",""cost"":4053,""sku"":null},""128064223"":{""id"":128064223,""category"":""module"",""name"":""Int_Sensors_Size2_Class1"",""cost"":1448,""sku"":null},""128064218"":{""id"":128064218,""category"":""module"",""name"":""Int_Sensors_Size1_Class1"",""cost"":517,""sku"":null},""128064311"":{""id"":128064311,""category"":""module"",""name"":""Int_ShieldCellBank_Size3_Class4"",""cost"":63333,""sku"":null},""128064301"":{""id"":128064301,""category"":""module"",""name"":""Int_ShieldCellBank_Size1_Class4"",""cost"":8078,""sku"":null},""128064306"":{""id"":128064306,""category"":""module"",""name"":""Int_ShieldCellBank_Size2_Class4"",""cost"":22619,""sku"":null},""128064310"":{""id"":128064310,""category"":""module"",""name"":""Int_ShieldCellBank_Size3_Class3"",""cost"":25333,""sku"":null},""128064305"":{""id"":128064305,""category"":""module"",""name"":""Int_ShieldCellBank_Size2_Class3"",""cost"":9048,""sku"":null},""128064300"":{""id"":128064300,""category"":""module"",""name"":""Int_ShieldCellBank_Size1_Class3"",""cost"":3231,""sku"":null},""128064309"":{""id"":128064309,""category"":""module"",""name"":""Int_ShieldCellBank_Size3_Class2"",""cost"":10133,""sku"":null},""128064299"":{""id"":128064299,""category"":""module"",""name"":""Int_ShieldCellBank_Size1_Class2"",""cost"":1293,""sku"":null},""128064304"":{""id"":128064304,""category"":""module"",""name"":""Int_ShieldCellBank_Size2_Class2"",""cost"":3619,""sku"":null},""128064308"":{""id"":128064308,""category"":""module"",""name"":""Int_ShieldCellBank_Size3_Class1"",""cost"":4053,""sku"":null},""128064303"":{""id"":128064303,""category"":""module"",""name"":""Int_ShieldCellBank_Size2_Class1"",""cost"":1448,""sku"":null},""128064298"":{""id"":128064298,""category"":""module"",""name"":""Int_ShieldCellBank_Size1_Class1"",""cost"":517,""sku"":null},""128666723"":{""id"":128666723,""category"":""module"",""name"":""Int_FSDInterdictor_Size4_Class5"",""cost"":21337344,""sku"":null},""128666719"":{""id"":128666719,""category"":""module"",""name"":""Int_FSDInterdictor_Size4_Class4"",""cost"":7112448,""sku"":null},""128666715"":{""id"":128666715,""category"":""module"",""name"":""Int_FSDInterdictor_Size4_Class3"",""cost"":2370816,""sku"":null},""128666722"":{""id"":128666722,""category"":""module"",""name"":""Int_FSDInterdictor_Size3_Class5"",""cost"":7620480,""sku"":null},""128666718"":{""id"":128666718,""category"":""module"",""name"":""Int_FSDInterdictor_Size3_Class4"",""cost"":2540160,""sku"":null},""128666714"":{""id"":128666714,""category"":""module"",""name"":""Int_FSDInterdictor_Size3_Class3"",""cost"":846720,""sku"":null},""128666711"":{""id"":128666711,""category"":""module"",""name"":""Int_FSDInterdictor_Size4_Class2"",""cost"":790272,""sku"":null},""128666707"":{""id"":128666707,""category"":""module"",""name"":""Int_FSDInterdictor_Size4_Class1"",""cost"":263424,""sku"":null},""128666710"":{""id"":128666710,""category"":""module"",""name"":""Int_FSDInterdictor_Size3_Class2"",""cost"":282240,""sku"":null},""128666706"":{""id"":128666706,""category"":""module"",""name"":""Int_FSDInterdictor_Size3_Class1"",""cost"":94080,""sku"":null},""128066540"":{""id"":128066540,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size3_Class4"",""cost"":43200,""sku"":null},""128066535"":{""id"":128066535,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size1_Class4"",""cost"":4800,""sku"":null},""128066539"":{""id"":128066539,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size3_Class3"",""cost"":21600,""sku"":null},""128066534"":{""id"":128066534,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size1_Class3"",""cost"":2400,""sku"":null},""128066538"":{""id"":128066538,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size3_Class2"",""cost"":10800,""sku"":null},""128066533"":{""id"":128066533,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size1_Class2"",""cost"":1200,""sku"":null},""128066537"":{""id"":128066537,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size3_Class1"",""cost"":5400,""sku"":null},""128666675"":{""id"":128666675,""category"":""module"",""name"":""Int_FuelScoop_Size8_Class4"",""cost"":72260660,""sku"":null},""128666682"":{""id"":128666682,""category"":""module"",""name"":""Int_FuelScoop_Size7_Class5"",""cost"":91180644,""sku"":null},""128666674"":{""id"":128666674,""category"":""module"",""name"":""Int_FuelScoop_Size7_Class4"",""cost"":22795161,""sku"":null},""128666667"":{""id"":128666667,""category"":""module"",""name"":""Int_FuelScoop_Size8_Class3"",""cost"":18065165,""sku"":null},""128666666"":{""id"":128666666,""category"":""module"",""name"":""Int_FuelScoop_Size7_Class3"",""cost"":5698790,""sku"":null},""128666659"":{""id"":128666659,""category"":""module"",""name"":""Int_FuelScoop_Size8_Class2"",""cost"":4516291,""sku"":null},""128666658"":{""id"":128666658,""category"":""module"",""name"":""Int_FuelScoop_Size7_Class2"",""cost"":1424698,""sku"":null},""128666651"":{""id"":128666651,""category"":""module"",""name"":""Int_FuelScoop_Size8_Class1"",""cost"":1083910,""sku"":null},""128666650"":{""id"":128666650,""category"":""module"",""name"":""Int_FuelScoop_Size7_Class1"",""cost"":341927,""sku"":null},""128667605"":{""id"":128667605,""category"":""module"",""name"":""Int_Repairer_Size8_Class1"",""cost"":612220,""sku"":null},""128667611"":{""id"":128667611,""category"":""module"",""name"":""Int_Repairer_Size6_Class2"",""cost"":566870,""sku"":null},""128667604"":{""id"":128667604,""category"":""module"",""name"":""Int_Repairer_Size7_Class1"",""cost"":340122,""sku"":null},""128667610"":{""id"":128667610,""category"":""module"",""name"":""Int_Repairer_Size5_Class2"",""cost"":314928,""sku"":null},""128667616"":{""id"":128667616,""category"":""module"",""name"":""Int_Repairer_Size3_Class3"",""cost"":291600,""sku"":null},""128667623"":{""id"":128667623,""category"":""module"",""name"":""Int_Repairer_Size2_Class4"",""cost"":486000,""sku"":null},""128667615"":{""id"":128667615,""category"":""module"",""name"":""Int_Repairer_Size2_Class3"",""cost"":162000,""sku"":null},""128667622"":{""id"":128667622,""category"":""module"",""name"":""Int_Repairer_Size1_Class4"",""cost"":270000,""sku"":null},""128667609"":{""id"":128667609,""category"":""module"",""name"":""Int_Repairer_Size4_Class2"",""cost"":174960,""sku"":null},""128667603"":{""id"":128667603,""category"":""module"",""name"":""Int_Repairer_Size6_Class1"",""cost"":188957,""sku"":null},""128671288"":{""id"":128671288,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size7_Class5"",""cost"":6998400,""sku"":null},""128671273"":{""id"":128671273,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size1_Class5"",""cost"":9600,""sku"":null},""128671282"":{""id"":128671282,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size5_Class4"",""cost"":388800,""sku"":null},""128671286"":{""id"":128671286,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size7_Class3"",""cost"":1749600,""sku"":null},""128671277"":{""id"":128671277,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size3_Class4"",""cost"":43200,""sku"":null},""128671285"":{""id"":128671285,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size7_Class2"",""cost"":874800,""sku"":null},""128064335"":{""id"":128064335,""category"":""module"",""name"":""Int_ShieldCellBank_Size8_Class3"",""cost"":4359903,""sku"":null},""128064330"":{""id"":128064330,""category"":""module"",""name"":""Int_ShieldCellBank_Size7_Class3"",""cost"":1557108,""sku"":null},""128064334"":{""id"":128064334,""category"":""module"",""name"":""Int_ShieldCellBank_Size8_Class2"",""cost"":1743961,""sku"":null},""128064329"":{""id"":128064329,""category"":""module"",""name"":""Int_ShieldCellBank_Size7_Class2"",""cost"":622843,""sku"":null},""128064333"":{""id"":128064333,""category"":""module"",""name"":""Int_ShieldCellBank_Size8_Class1"",""cost"":697584,""sku"":null},""128064328"":{""id"":128064328,""category"":""module"",""name"":""Int_ShieldCellBank_Size7_Class1"",""cost"":249137,""sku"":null},""128066549"":{""id"":128066549,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size7_Class3"",""cost"":1749600,""sku"":null},""128066548"":{""id"":128066548,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size7_Class2"",""cost"":874800,""sku"":null},""128066544"":{""id"":128066544,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size5_Class3"",""cost"":194400,""sku"":null},""128066547"":{""id"":128066547,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size7_Class1"",""cost"":437400,""sku"":null},""128066543"":{""id"":128066543,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size5_Class2"",""cost"":97200,""sku"":null},""128066542"":{""id"":128066542,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size5_Class1"",""cost"":48600,""sku"":null},""128064175"":{""id"":128064175,""category"":""module"",""name"":""Int_LifeSupport_Size8_Class3"",""cost"":4359903,""sku"":null},""128064170"":{""id"":128064170,""category"":""module"",""name"":""Int_LifeSupport_Size7_Class3"",""cost"":1557108,""sku"":null},""128064174"":{""id"":128064174,""category"":""module"",""name"":""Int_LifeSupport_Size8_Class2"",""cost"":1743961,""sku"":null},""128064169"":{""id"":128064169,""category"":""module"",""name"":""Int_LifeSupport_Size7_Class2"",""cost"":622843,""sku"":null},""128064173"":{""id"":128064173,""category"":""module"",""name"":""Int_LifeSupport_Size8_Class1"",""cost"":697584,""sku"":null},""128064168"":{""id"":128064168,""category"":""module"",""name"":""Int_LifeSupport_Size7_Class1"",""cost"":249137,""sku"":null},""128064060"":{""id"":128064060,""category"":""module"",""name"":""Int_Powerplant_Size7_Class3"",""cost"":5698790,""sku"":null},""128064063"":{""id"":128064063,""category"":""module"",""name"":""Int_Powerplant_Size8_Class1"",""cost"":2007241,""sku"":null},""128064058"":{""id"":128064058,""category"":""module"",""name"":""Int_Powerplant_Size7_Class1"",""cost"":633199,""sku"":null},""128064100"":{""id"":128064100,""category"":""module"",""name"":""Int_Engine_Size8_Class3"",""cost"":18065165,""sku"":null},""128064099"":{""id"":128064099,""category"":""module"",""name"":""Int_Engine_Size8_Class2"",""cost"":6021722,""sku"":null},""128064094"":{""id"":128064094,""category"":""module"",""name"":""Int_Engine_Size7_Class2"",""cost"":1899597,""sku"":null},""128064098"":{""id"":128064098,""category"":""module"",""name"":""Int_Engine_Size8_Class1"",""cost"":2007241,""sku"":null},""128064093"":{""id"":128064093,""category"":""module"",""name"":""Int_Engine_Size7_Class1"",""cost"":633199,""sku"":null},""128671244"":{""id"":128671244,""category"":""module"",""name"":""Int_DroneControl_Collection_Size7_Class1"",""cost"":437400,""sku"":null},""128671240"":{""id"":128671240,""category"":""module"",""name"":""Int_DroneControl_Collection_Size5_Class2"",""cost"":97200,""sku"":null},""128671236"":{""id"":128671236,""category"":""module"",""name"":""Int_DroneControl_Collection_Size3_Class3"",""cost"":21600,""sku"":null},""128671231"":{""id"":128671231,""category"":""module"",""name"":""Int_DroneControl_Collection_Size1_Class3"",""cost"":2400,""sku"":null},""128671239"":{""id"":128671239,""category"":""module"",""name"":""Int_DroneControl_Collection_Size5_Class1"",""cost"":48600,""sku"":null},""128671235"":{""id"":128671235,""category"":""module"",""name"":""Int_DroneControl_Collection_Size3_Class2"",""cost"":10800,""sku"":null},""128671230"":{""id"":128671230,""category"":""module"",""name"":""Int_DroneControl_Collection_Size1_Class2"",""cost"":1200,""sku"":null},""128671234"":{""id"":128671234,""category"":""module"",""name"":""Int_DroneControl_Collection_Size3_Class1"",""cost"":5400,""sku"":null},""128064135"":{""id"":128064135,""category"":""module"",""name"":""Int_Hyperdrive_Size8_Class3"",""cost"":18065165,""sku"":null},""128064130"":{""id"":128064130,""category"":""module"",""name"":""Int_Hyperdrive_Size7_Class3"",""cost"":5698790,""sku"":null},""128064129"":{""id"":128064129,""category"":""module"",""name"":""Int_Hyperdrive_Size7_Class2"",""cost"":1899597,""sku"":null},""128064133"":{""id"":128064133,""category"":""module"",""name"":""Int_Hyperdrive_Size8_Class1"",""cost"":2007241,""sku"":null},""128064128"":{""id"":128064128,""category"":""module"",""name"":""Int_Hyperdrive_Size7_Class1"",""cost"":633199,""sku"":null},""128064241"":{""id"":128064241,""category"":""module"",""name"":""Int_Sensors_Size5_Class4"",""cost"":496527,""sku"":null},""128064245"":{""id"":128064245,""category"":""module"",""name"":""Int_Sensors_Size6_Class3"",""cost"":556110,""sku"":null},""128064244"":{""id"":128064244,""category"":""module"",""name"":""Int_Sensors_Size6_Class2"",""cost"":222444,""sku"":null},""128064240"":{""id"":128064240,""category"":""module"",""name"":""Int_Sensors_Size5_Class3"",""cost"":198611,""sku"":null},""128064235"":{""id"":128064235,""category"":""module"",""name"":""Int_Sensors_Size4_Class3"",""cost"":70932,""sku"":null},""128064239"":{""id"":128064239,""category"":""module"",""name"":""Int_Sensors_Size5_Class2"",""cost"":79444,""sku"":null},""128064243"":{""id"":128064243,""category"":""module"",""name"":""Int_Sensors_Size6_Class1"",""cost"":88978,""sku"":null},""128064206"":{""id"":128064206,""category"":""module"",""name"":""Int_PowerDistributor_Size6_Class4"",""cost"":1390275,""sku"":null},""128064201"":{""id"":128064201,""category"":""module"",""name"":""Int_PowerDistributor_Size5_Class4"",""cost"":496527,""sku"":null},""128064196"":{""id"":128064196,""category"":""module"",""name"":""Int_PowerDistributor_Size4_Class4"",""cost"":177331,""sku"":null},""128064205"":{""id"":128064205,""category"":""module"",""name"":""Int_PowerDistributor_Size6_Class3"",""cost"":556110,""sku"":null},""128064204"":{""id"":128064204,""category"":""module"",""name"":""Int_PowerDistributor_Size6_Class2"",""cost"":222444,""sku"":null},""128064195"":{""id"":128064195,""category"":""module"",""name"":""Int_PowerDistributor_Size4_Class3"",""cost"":70932,""sku"":null},""128668546"":{""id"":128668546,""category"":""module"",""name"":""Int_HullReinforcement_Size5_Class2"",""cost"":450000,""sku"":null},""128668544"":{""id"":128668544,""category"":""module"",""name"":""Int_HullReinforcement_Size4_Class2"",""cost"":195000,""sku"":null},""128668540"":{""id"":128668540,""category"":""module"",""name"":""Int_HullReinforcement_Size2_Class2"",""cost"":36000,""sku"":null},""128668538"":{""id"":128668538,""category"":""module"",""name"":""Int_HullReinforcement_Size1_Class2"",""cost"":15000,""sku"":null},""128666681"":{""id"":128666681,""category"":""module"",""name"":""Int_FuelScoop_Size6_Class5"",""cost"":28763610,""sku"":null},""128666673"":{""id"":128666673,""category"":""module"",""name"":""Int_FuelScoop_Size6_Class4"",""cost"":7190903,""sku"":null},""128666665"":{""id"":128666665,""category"":""module"",""name"":""Int_FuelScoop_Size6_Class3"",""cost"":1797726,""sku"":null},""128666672"":{""id"":128666672,""category"":""module"",""name"":""Int_FuelScoop_Size5_Class4"",""cost"":2268424,""sku"":null},""128666679"":{""id"":128666679,""category"":""module"",""name"":""Int_FuelScoop_Size4_Class5"",""cost"":2862364,""sku"":null},""128666671"":{""id"":128666671,""category"":""module"",""name"":""Int_FuelScoop_Size4_Class4"",""cost"":715591,""sku"":null},""128668545"":{""id"":128668545,""category"":""module"",""name"":""Int_HullReinforcement_Size5_Class1"",""cost"":150000,""sku"":null},""128668543"":{""id"":128668543,""category"":""module"",""name"":""Int_HullReinforcement_Size4_Class1"",""cost"":65000,""sku"":null},""128668541"":{""id"":128668541,""category"":""module"",""name"":""Int_HullReinforcement_Size3_Class1"",""cost"":28000,""sku"":null},""128668539"":{""id"":128668539,""category"":""module"",""name"":""Int_HullReinforcement_Size2_Class1"",""cost"":12000,""sku"":null},""128668537"":{""id"":128668537,""category"":""module"",""name"":""Int_HullReinforcement_Size1_Class1"",""cost"":5000,""sku"":null},""128064326"":{""id"":128064326,""category"":""module"",""name"":""Int_ShieldCellBank_Size6_Class4"",""cost"":1390275,""sku"":null},""128064321"":{""id"":128064321,""category"":""module"",""name"":""Int_ShieldCellBank_Size5_Class4"",""cost"":496527,""sku"":null},""128064325"":{""id"":128064325,""category"":""module"",""name"":""Int_ShieldCellBank_Size6_Class3"",""cost"":556110,""sku"":null},""128064324"":{""id"":128064324,""category"":""module"",""name"":""Int_ShieldCellBank_Size6_Class2"",""cost"":222444,""sku"":null},""128671243"":{""id"":128671243,""category"":""module"",""name"":""Int_DroneControl_Collection_Size5_Class5"",""cost"":777600,""sku"":null},""128671238"":{""id"":128671238,""category"":""module"",""name"":""Int_DroneControl_Collection_Size3_Class5"",""cost"":86400,""sku"":null},""128671233"":{""id"":128671233,""category"":""module"",""name"":""Int_DroneControl_Collection_Size1_Class5"",""cost"":9600,""sku"":null},""128671247"":{""id"":128671247,""category"":""module"",""name"":""Int_DroneControl_Collection_Size7_Class4"",""cost"":3499200,""sku"":null},""128668535"":{""id"":128668535,""category"":""module"",""name"":""Hpt_ShieldBooster_Size0_Class4"",""cost"":122000,""sku"":null},""128668533"":{""id"":128668533,""category"":""module"",""name"":""Hpt_ShieldBooster_Size0_Class2"",""cost"":23000,""sku"":null},""128668532"":{""id"":128668532,""category"":""module"",""name"":""Hpt_ShieldBooster_Size0_Class1"",""cost"":10000,""sku"":null},""128064166"":{""id"":128064166,""category"":""module"",""name"":""Int_LifeSupport_Size6_Class4"",""cost"":1390275,""sku"":null},""128064161"":{""id"":128064161,""category"":""module"",""name"":""Int_LifeSupport_Size5_Class4"",""cost"":496527,""sku"":null},""128064056"":{""id"":128064056,""category"":""module"",""name"":""Int_Powerplant_Size6_Class4"",""cost"":5393177,""sku"":null},""128064051"":{""id"":128064051,""category"":""module"",""name"":""Int_Powerplant_Size5_Class4"",""cost"":1701318,""sku"":null},""128682043"":{""id"":128682043,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Metallic"",""cost"":0,""sku"":null},""128672343"":{""id"":128672343,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Horizons_Polar"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1058""},""128672344"":{""id"":128672344,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Horizons_Desert"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1062""},""128672345"":{""id"":128672345,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Horizons_Lunar"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1063""},""128672355"":{""id"":128672355,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_PCGamer_PCGamer"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1069""},""128672417"":{""id"":128672417,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Yogscast_01"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1068""},""128667727"":{""id"":128667727,""category"":""paintjob"",""name"":""paintjob_CobraMkiii_Default_52"",""cost"":0,""sku"":null},""128066428"":{""id"":128066428,""category"":""paintjob"",""name"":""paintjob_cobramkiii_wireframe_01"",""cost"":0,""sku"":null},""128670861"":{""id"":128670861,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Onionhead1_01"",""cost"":0,""sku"":null},""128671133"":{""id"":128671133,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_green"",""cost"":0,""sku"":null},""128671134"":{""id"":128671134,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_blue"",""cost"":0,""sku"":null},""128671135"":{""id"":128671135,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_orange"",""cost"":0,""sku"":null},""128671136"":{""id"":128671136,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_red"",""cost"":0,""sku"":null},""128671137"":{""id"":128671137,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_purple"",""cost"":0,""sku"":null},""128671138"":{""id"":128671138,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_yellow"",""cost"":0,""sku"":null},""128667638"":{""id"":128667638,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Merc"",""cost"":0,""sku"":null},""128667639"":{""id"":128667639,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Merc"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_MERCENARY_PAINTJOB_1001""},""128672779"":{""id"":128672779,""category"":""paintjob"",""name"":""PaintJob_Eagle_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_EAGLE_1033""},""128681642"":{""id"":128681642,""category"":""paintjob"",""name"":""PaintJob_Eagle_Pax_South_Pax_South"",""cost"":0,""sku"":""FORC_FDEV_V_EAGLE_1034""},""128066405"":{""id"":128066405,""category"":""paintjob"",""name"":""paintjob_eagle_stripe1_02"",""cost"":0,""sku"":null},""128066406"":{""id"":128066406,""category"":""paintjob"",""name"":""paintjob_eagle_doublestripe_01"",""cost"":0,""sku"":null},""128066416"":{""id"":128066416,""category"":""paintjob"",""name"":""paintjob_eagle_thirds_01"",""cost"":0,""sku"":null},""128066419"":{""id"":128066419,""category"":""paintjob"",""name"":""paintjob_eagle_stripe1_03"",""cost"":0,""sku"":null},""128668019"":{""id"":128668019,""category"":""paintjob"",""name"":""PaintJob_Eagle_Crimson"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_MERCENARY_PAINTJOB_1003""},""128066420"":{""id"":128066420,""category"":""paintjob"",""name"":""paintjob_eagle_thirds_02"",""cost"":0,""sku"":null},""128066430"":{""id"":128066430,""category"":""paintjob"",""name"":""paintjob_eagle_stripe1_01"",""cost"":0,""sku"":null},""128066436"":{""id"":128066436,""category"":""paintjob"",""name"":""paintjob_eagle_camo_03"",""cost"":0,""sku"":null},""128066437"":{""id"":128066437,""category"":""paintjob"",""name"":""paintjob_eagle_thirds_03"",""cost"":0,""sku"":null},""128066441"":{""id"":128066441,""category"":""paintjob"",""name"":""paintjob_eagle_camo_02"",""cost"":0,""sku"":null},""128066449"":{""id"":128066449,""category"":""paintjob"",""name"":""paintjob_eagle_doublestripe_02"",""cost"":0,""sku"":null},""128066453"":{""id"":128066453,""category"":""paintjob"",""name"":""paintjob_eagle_doublestripe_03"",""cost"":0,""sku"":null},""128066456"":{""id"":128066456,""category"":""paintjob"",""name"":""paintjob_eagle_camo_01"",""cost"":0,""sku"":null},""128671139"":{""id"":128671139,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_green"",""cost"":0,""sku"":null},""128671140"":{""id"":128671140,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_blue"",""cost"":0,""sku"":null},""128671141"":{""id"":128671141,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_orange"",""cost"":0,""sku"":null},""128671142"":{""id"":128671142,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_red"",""cost"":0,""sku"":null},""128671143"":{""id"":128671143,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_purple"",""cost"":0,""sku"":null},""128671144"":{""id"":128671144,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_yellow"",""cost"":0,""sku"":null},""128671777"":{""id"":128671777,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Desert_Sand"",""cost"":0,""sku"":null},""128671778"":{""id"":128671778,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Earth_Yellow"",""cost"":0,""sku"":null},""128672802"":{""id"":128672802,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_BlackFriday_01"",""cost"":0,""sku"":null},""128671779"":{""id"":128671779,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Dark_Green"",""cost"":0,""sku"":null},""128671780"":{""id"":128671780,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Forest_Green"",""cost"":0,""sku"":null},""128671781"":{""id"":128671781,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Sand"",""cost"":0,""sku"":null},""128671782"":{""id"":128671782,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Earth_Red"",""cost"":0,""sku"":null},""128672426"":{""id"":128672426,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_SpecialEffect_01"",""cost"":0,""sku"":null},""128066404"":{""id"":128066404,""category"":""paintjob"",""name"":""paintjob_sidewinder_default_02"",""cost"":0,""sku"":null},""128066408"":{""id"":128066408,""category"":""paintjob"",""name"":""paintjob_sidewinder_default_03"",""cost"":0,""sku"":null},""128066414"":{""id"":128066414,""category"":""paintjob"",""name"":""paintjob_sidewinder_doublestripe_08"",""cost"":0,""sku"":null},""128066423"":{""id"":128066423,""category"":""paintjob"",""name"":""paintjob_sidewinder_doublestripe_05"",""cost"":0,""sku"":null},""128066431"":{""id"":128066431,""category"":""paintjob"",""name"":""paintjob_sidewinder_thirds_07"",""cost"":0,""sku"":null},""128066432"":{""id"":128066432,""category"":""paintjob"",""name"":""paintjob_sidewinder_thirds_01"",""cost"":0,""sku"":null},""128066433"":{""id"":128066433,""category"":""paintjob"",""name"":""paintjob_sidewinder_doublestripe_07"",""cost"":0,""sku"":null},""128066440"":{""id"":128066440,""category"":""paintjob"",""name"":""paintjob_sidewinder_camo_01"",""cost"":0,""sku"":null},""128066444"":{""id"":128066444,""category"":""paintjob"",""name"":""paintjob_sidewinder_thirds_06"",""cost"":0,""sku"":null},""128066447"":{""id"":128066447,""category"":""paintjob"",""name"":""paintjob_sidewinder_camo_03"",""cost"":0,""sku"":null},""128066448"":{""id"":128066448,""category"":""paintjob"",""name"":""paintjob_sidewinder_default_04"",""cost"":0,""sku"":null},""128066454"":{""id"":128066454,""category"":""paintjob"",""name"":""paintjob_sidewinder_camo_02"",""cost"":0,""sku"":null},""128671181"":{""id"":128671181,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_green"",""cost"":0,""sku"":null},""128671182"":{""id"":128671182,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_blue"",""cost"":0,""sku"":null},""128671183"":{""id"":128671183,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_orange"",""cost"":0,""sku"":null},""128671184"":{""id"":128671184,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_red"",""cost"":0,""sku"":null},""128671185"":{""id"":128671185,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_purple"",""cost"":0,""sku"":null},""128671186"":{""id"":128671186,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_yellow"",""cost"":0,""sku"":null},""128672796"":{""id"":128672796,""category"":""paintjob"",""name"":""PaintJob_Viper_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_VIPER_1049""},""128667667"":{""id"":128667667,""category"":""paintjob"",""name"":""PaintJob_Viper_Merc"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_MERCENARY_PAINTJOB_1002""},""128666726"":{""id"":128666726,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Camo1_02"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1003""},""128066407"":{""id"":128066407,""category"":""paintjob"",""name"":""paintjob_viper_flag_switzerland_01"",""cost"":0,""sku"":null},""128666727"":{""id"":128666727,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Camo2_03"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1004""},""128666728"":{""id"":128666728,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Stripe1_02"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1005""},""128066409"":{""id"":128066409,""category"":""paintjob"",""name"":""paintjob_viper_flag_belgium_01"",""cost"":0,""sku"":null},""128666729"":{""id"":128666729,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Stripe1_03"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1006""},""128066410"":{""id"":128066410,""category"":""paintjob"",""name"":""paintjob_viper_flag_australia_01"",""cost"":0,""sku"":null},""128666730"":{""id"":128666730,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Stripe2_02"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1007""},""128066411"":{""id"":128066411,""category"":""paintjob"",""name"":""paintjob_viper_default_01"",""cost"":0,""sku"":null},""128666731"":{""id"":128666731,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Stripe2_03"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1008""},""128066412"":{""id"":128066412,""category"":""paintjob"",""name"":""paintjob_viper_stripe2_02"",""cost"":0,""sku"":null},""128066413"":{""id"":128066413,""category"":""paintjob"",""name"":""paintjob_viper_flag_austria_01"",""cost"":0,""sku"":null},""128066415"":{""id"":128066415,""category"":""paintjob"",""name"":""paintjob_viper_stripe1_01"",""cost"":0,""sku"":null},""128066417"":{""id"":128066417,""category"":""paintjob"",""name"":""paintjob_viper_flag_spain_01"",""cost"":0,""sku"":null},""128066418"":{""id"":128066418,""category"":""paintjob"",""name"":""paintjob_viper_stripe1_02"",""cost"":0,""sku"":null},""128066421"":{""id"":128066421,""category"":""paintjob"",""name"":""paintjob_viper_flag_denmark_01"",""cost"":0,""sku"":null},""128066422"":{""id"":128066422,""category"":""paintjob"",""name"":""paintjob_viper_police_federation_01"",""cost"":0,""sku"":null},""128666742"":{""id"":128666742,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Hotrod_01"",""cost"":0,""sku"":null},""128666743"":{""id"":128666743,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Hotrod_03"",""cost"":0,""sku"":null},""128066424"":{""id"":128066424,""category"":""paintjob"",""name"":""paintjob_viper_flag_newzealand_01"",""cost"":0,""sku"":null},""128066425"":{""id"":128066425,""category"":""paintjob"",""name"":""paintjob_viper_flag_italy_01"",""cost"":0,""sku"":null},""128066426"":{""id"":128066426,""category"":""paintjob"",""name"":""paintjob_viper_stripe2_04"",""cost"":0,""sku"":null},""128066427"":{""id"":128066427,""category"":""paintjob"",""name"":""paintjob_viper_police_independent_01"",""cost"":0,""sku"":null},""128066429"":{""id"":128066429,""category"":""paintjob"",""name"":""paintjob_viper_default_03"",""cost"":0,""sku"":null},""128066434"":{""id"":128066434,""category"":""paintjob"",""name"":""paintjob_viper_flag_uk_01"",""cost"":0,""sku"":null},""128066435"":{""id"":128066435,""category"":""paintjob"",""name"":""paintjob_viper_flag_germany_01"",""cost"":0,""sku"":null},""128066438"":{""id"":128066438,""category"":""paintjob"",""name"":""paintjob_viper_flag_netherlands_01"",""cost"":0,""sku"":null},""128066439"":{""id"":128066439,""category"":""paintjob"",""name"":""paintjob_viper_flag_usa_01"",""cost"":0,""sku"":null},""128066442"":{""id"":128066442,""category"":""paintjob"",""name"":""paintjob_viper_flag_russia_01"",""cost"":0,""sku"":null},""128066443"":{""id"":128066443,""category"":""paintjob"",""name"":""paintjob_viper_flag_canada_01"",""cost"":0,""sku"":null},""128066445"":{""id"":128066445,""category"":""paintjob"",""name"":""paintjob_viper_flag_sweden_01"",""cost"":0,""sku"":null},""128066446"":{""id"":128066446,""category"":""paintjob"",""name"":""paintjob_viper_flag_poland_01"",""cost"":0,""sku"":null},""128066450"":{""id"":128066450,""category"":""paintjob"",""name"":""paintjob_viper_flag_finland_01"",""cost"":0,""sku"":null},""128066451"":{""id"":128066451,""category"":""paintjob"",""name"":""paintjob_viper_flag_france_01"",""cost"":0,""sku"":null},""128066452"":{""id"":128066452,""category"":""paintjob"",""name"":""paintjob_viper_police_empire_01"",""cost"":0,""sku"":null},""128066455"":{""id"":128066455,""category"":""paintjob"",""name"":""paintjob_viper_flag_norway_01"",""cost"":0,""sku"":null},""128671205"":{""id"":128671205,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_green"",""cost"":0,""sku"":null},""128671206"":{""id"":128671206,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_blue"",""cost"":0,""sku"":null},""128671207"":{""id"":128671207,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_orange"",""cost"":0,""sku"":null},""128671208"":{""id"":128671208,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_red"",""cost"":0,""sku"":null},""128671209"":{""id"":128671209,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_purple"",""cost"":0,""sku"":null},""128671210"":{""id"":128671210,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_yellow"",""cost"":0,""sku"":null},""128672806"":{""id"":128672806,""category"":""paintjob"",""name"":""PaintJob_Asp_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_ASP_1043""},""128672419"":{""id"":128672419,""category"":""paintjob"",""name"":""PaintJob_Asp_Metallic_Gold"",""cost"":0,""sku"":""FORC_FDEV_V_ASP_1038""},""128671127"":{""id"":128671127,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_green"",""cost"":0,""sku"":null},""128671128"":{""id"":128671128,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_blue"",""cost"":0,""sku"":null},""128671129"":{""id"":128671129,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_orange"",""cost"":0,""sku"":null},""128671130"":{""id"":128671130,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_red"",""cost"":0,""sku"":null},""128671131"":{""id"":128671131,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_purple"",""cost"":0,""sku"":null},""128671132"":{""id"":128671132,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_yellow"",""cost"":0,""sku"":null},""128672783"":{""id"":128672783,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1076""},""128672805"":{""id"":128672805,""category"":""paintjob"",""name"":""PaintJob_FedDropship_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_FEDDROP_1019""},""128671151"":{""id"":128671151,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_green"",""cost"":0,""sku"":null},""128671152"":{""id"":128671152,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_blue"",""cost"":0,""sku"":null},""128671153"":{""id"":128671153,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_orange"",""cost"":0,""sku"":null},""128671154"":{""id"":128671154,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_red"",""cost"":0,""sku"":null},""128671155"":{""id"":128671155,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_purple"",""cost"":0,""sku"":null},""128671156"":{""id"":128671156,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_yellow"",""cost"":0,""sku"":null},""128672788"":{""id"":128672788,""category"":""paintjob"",""name"":""PaintJob_Python_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_PYTHON_1020""},""128671175"":{""id"":128671175,""category"":""paintjob"",""name"":""paintjob_python_vibrant_green"",""cost"":0,""sku"":null},""128671176"":{""id"":128671176,""category"":""paintjob"",""name"":""paintjob_python_vibrant_blue"",""cost"":0,""sku"":null},""128671177"":{""id"":128671177,""category"":""paintjob"",""name"":""paintjob_python_vibrant_orange"",""cost"":0,""sku"":null},""128671178"":{""id"":128671178,""category"":""paintjob"",""name"":""paintjob_python_vibrant_red"",""cost"":0,""sku"":null},""128671179"":{""id"":128671179,""category"":""paintjob"",""name"":""paintjob_python_vibrant_purple"",""cost"":0,""sku"":null},""128671180"":{""id"":128671180,""category"":""paintjob"",""name"":""paintjob_python_vibrant_yellow"",""cost"":0,""sku"":null},""128672807"":{""id"":128672807,""category"":""paintjob"",""name"":""PaintJob_Adder_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_ADDER_1019""},""128671121"":{""id"":128671121,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_green"",""cost"":0,""sku"":null},""128671122"":{""id"":128671122,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_blue"",""cost"":0,""sku"":null},""128671123"":{""id"":128671123,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_orange"",""cost"":0,""sku"":null},""128671124"":{""id"":128671124,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_red"",""cost"":0,""sku"":null},""128671125"":{""id"":128671125,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_purple"",""cost"":0,""sku"":null},""128671126"":{""id"":128671126,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_yellow"",""cost"":0,""sku"":null},""128671145"":{""id"":128671145,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_green"",""cost"":0,""sku"":null},""128671146"":{""id"":128671146,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_blue"",""cost"":0,""sku"":null},""128671147"":{""id"":128671147,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_orange"",""cost"":0,""sku"":null},""128671148"":{""id"":128671148,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_red"",""cost"":0,""sku"":null},""128671149"":{""id"":128671149,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_purple"",""cost"":0,""sku"":null},""128671150"":{""id"":128671150,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_yellow"",""cost"":0,""sku"":null},""128671749"":{""id"":128671749,""category"":""paintjob"",""name"":""PaintJob_FerDeLance_Militaire_desert_Sand"",""cost"":0,""sku"":null},""128672795"":{""id"":128672795,""category"":""paintjob"",""name"":""PaintJob_FerDeLance_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_FERDELANCE_1019""},""128671157"":{""id"":128671157,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_green"",""cost"":0,""sku"":null},""128671158"":{""id"":128671158,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_blue"",""cost"":0,""sku"":null},""128671159"":{""id"":128671159,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_orange"",""cost"":0,""sku"":null},""128671160"":{""id"":128671160,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_red"",""cost"":0,""sku"":null},""128671161"":{""id"":128671161,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_purple"",""cost"":0,""sku"":null},""128671162"":{""id"":128671162,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_yellow"",""cost"":0,""sku"":null},""128672789"":{""id"":128672789,""category"":""paintjob"",""name"":""PaintJob_Hauler_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_HAULER_1024""},""128671163"":{""id"":128671163,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_green"",""cost"":0,""sku"":null},""128671164"":{""id"":128671164,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_blue"",""cost"":0,""sku"":null},""128671165"":{""id"":128671165,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_orange"",""cost"":0,""sku"":null},""128671166"":{""id"":128671166,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_red"",""cost"":0,""sku"":null},""128671167"":{""id"":128671167,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_purple"",""cost"":0,""sku"":null},""128671168"":{""id"":128671168,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_yellow"",""cost"":0,""sku"":null},""128672797"":{""id"":128672797,""category"":""paintjob"",""name"":""PaintJob_Orca_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_ORCA_1018""},""128671169"":{""id"":128671169,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_green"",""cost"":0,""sku"":null},""128671170"":{""id"":128671170,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_blue"",""cost"":0,""sku"":null},""128671171"":{""id"":128671171,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_orange"",""cost"":0,""sku"":null},""128671172"":{""id"":128671172,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_red"",""cost"":0,""sku"":null},""128671173"":{""id"":128671173,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_purple"",""cost"":0,""sku"":null},""128671174"":{""id"":128671174,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_yellow"",""cost"":0,""sku"":null},""128672800"":{""id"":128672800,""category"":""paintjob"",""name"":""PaintJob_Type6_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_TYPE6_1024""},""128671187"":{""id"":128671187,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_green"",""cost"":0,""sku"":null},""128671188"":{""id"":128671188,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_blue"",""cost"":0,""sku"":null},""128671189"":{""id"":128671189,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_orange"",""cost"":0,""sku"":null},""128671190"":{""id"":128671190,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_red"",""cost"":0,""sku"":null},""128671191"":{""id"":128671191,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_purple"",""cost"":0,""sku"":null},""128671192"":{""id"":128671192,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_yellow"",""cost"":0,""sku"":null},""128672799"":{""id"":128672799,""category"":""paintjob"",""name"":""PaintJob_Type7_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_TYPE7_1018""},""128671193"":{""id"":128671193,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_green"",""cost"":0,""sku"":null},""128671194"":{""id"":128671194,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_blue"",""cost"":0,""sku"":null},""128671195"":{""id"":128671195,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_orange"",""cost"":0,""sku"":null},""128671196"":{""id"":128671196,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_red"",""cost"":0,""sku"":null},""128671197"":{""id"":128671197,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_purple"",""cost"":0,""sku"":null},""128671198"":{""id"":128671198,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_yellow"",""cost"":0,""sku"":null},""128672793"":{""id"":128672793,""category"":""paintjob"",""name"":""PaintJob_Type9_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_TYPE9_1018""},""128671199"":{""id"":128671199,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_green"",""cost"":0,""sku"":null},""128671200"":{""id"":128671200,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_blue"",""cost"":0,""sku"":null},""128671201"":{""id"":128671201,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_orange"",""cost"":0,""sku"":null},""128671202"":{""id"":128671202,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_red"",""cost"":0,""sku"":null},""128671203"":{""id"":128671203,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_purple"",""cost"":0,""sku"":null},""128671204"":{""id"":128671204,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_yellow"",""cost"":0,""sku"":null},""128671211"":{""id"":128671211,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_green"",""cost"":0,""sku"":null},""128671212"":{""id"":128671212,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_blue"",""cost"":0,""sku"":null},""128671213"":{""id"":128671213,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_orange"",""cost"":0,""sku"":null},""128671214"":{""id"":128671214,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_red"",""cost"":0,""sku"":null},""128671215"":{""id"":128671215,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_purple"",""cost"":0,""sku"":null},""128671216"":{""id"":128671216,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_yellow"",""cost"":0,""sku"":null},""128672801"":{""id"":128672801,""category"":""paintjob"",""name"":""PaintJob_Vulture_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_VULTURE_1030""},""128672782"":{""id"":128672782,""category"":""paintjob"",""name"":""PaintJob_Anaconda_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_ANACONDA_1027""},""128672804"":{""id"":128672804,""category"":""paintjob"",""name"":""PaintJob_DiamondBack_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_DIAMOND_SCOUT_1018""},""128672784"":{""id"":128672784,""category"":""paintjob"",""name"":""PaintJob_DiamondBackXL_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_DIAMOND_EXPLORER_1020""},""128672786"":{""id"":128672786,""category"":""paintjob"",""name"":""PaintJob_Federation_Corvette_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_FEDERAL_CORVETTE_1000""},""128672781"":{""id"":128672781,""category"":""paintjob"",""name"":""PaintJob_Cutter_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_IMPERIAL_CUTTER_1000""},""128672792"":{""id"":128672792,""category"":""paintjob"",""name"":""PaintJob_Empire_Courier_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_IMPERIAL_COURIER_1018""},""128672791"":{""id"":128672791,""category"":""paintjob"",""name"":""PaintJob_FedGunship_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_FEDERAL_GUNSHIP_1000""},""128672794"":{""id"":128672794,""category"":""paintjob"",""name"":""PaintJob_FedDropshipMkII_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_FEDERAL_ASSAULT_1000""},""128672778"":{""id"":128672778,""category"":""paintjob"",""name"":""PaintJob_Empire_Eagle_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_IMPERIAL_EAGLE_1000""},""128672780"":{""id"":128672780,""category"":""paintjob"",""name"":""PaintJob_ViperMkIV_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_VIPER_1050""},""128672790"":{""id"":128672790,""category"":""paintjob"",""name"":""PaintJob_CobraMkIV_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1075""},""128672785"":{""id"":128672785,""category"":""paintjob"",""name"":""PaintJob_Independant_Trader_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_KEELBACK_1000""},""128672803"":{""id"":128672803,""category"":""paintjob"",""name"":""PaintJob_Asp_Scout_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_ASP_SCOUT_1000""},""128672798"":{""id"":128672798,""category"":""paintjob"",""name"":""PaintJob_EmpireTrader_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_CLIPPER_1019""},""128672246"":{""id"":128672246,""category"":""decal"",""name"":""Decal_PaxPrime"",""cost"":0,""sku"":""FORC_FDEV_V_DECAL_1036""},""128667650"":{""id"":128667650,""category"":""decal"",""name"":""Decal_Planet2"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_BACKERS_DECAL_1000""},""128667655"":{""id"":128667655,""category"":""decal"",""name"":""Decal_Skull3"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_MERCENARY_DECAL_1000""},""128667744"":{""id"":128667744,""category"":""decal"",""name"":""Decal_Trade_Mostly_Penniless"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1001""},""128667745"":{""id"":128667745,""category"":""decal"",""name"":""Decal_Trade_Peddler"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1002""},""128667746"":{""id"":128667746,""category"":""decal"",""name"":""Decal_Trade_Dealer"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1003""},""128667747"":{""id"":128667747,""category"":""decal"",""name"":""Decal_Trade_Merchant"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1004""},""128667748"":{""id"":128667748,""category"":""decal"",""name"":""Decal_Trade_Broker"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1005""},""128667752"":{""id"":128667752,""category"":""decal"",""name"":""Decal_Explorer_Mostly_Aimless"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_EXPLORE_DECAL_1001""},""128667753"":{""id"":128667753,""category"":""decal"",""name"":""Decal_Explorer_Scout"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_EXPLORE_DECAL_1002""},""128667754"":{""id"":128667754,""category"":""decal"",""name"":""Decal_Explorer_Surveyor"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_EXPLORE_DECAL_1003""},""128668553"":{""id"":128668553,""category"":""decal"",""name"":""Decal_Onionhead1"",""cost"":0,""sku"":""FORC_FDEV_V_DECAL_1030""},""128668554"":{""id"":128668554,""category"":""decal"",""name"":""Decal_Onionhead2"",""cost"":0,""sku"":""FORC_FDEV_V_DECAL_1032""},""128668555"":{""id"":128668555,""category"":""decal"",""name"":""Decal_Onionhead3"",""cost"":0,""sku"":""FORC_FDEV_V_DECAL_1032""},""128672768"":{""id"":128672768,""category"":""bobblehead"",""name"":""Bobble_TextPlus"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672769"":{""id"":128672769,""category"":""bobblehead"",""name"":""Bobble_TextBracket01"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672770"":{""id"":128672770,""category"":""bobblehead"",""name"":""Bobble_TextBracket02"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672771"":{""id"":128672771,""category"":""bobblehead"",""name"":""Bobble_TextUnderscore"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672772"":{""id"":128672772,""category"":""bobblehead"",""name"":""Bobble_TextMinus"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672773"":{""id"":128672773,""category"":""bobblehead"",""name"":""Bobble_TextPercent"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672774"":{""id"":128672774,""category"":""bobblehead"",""name"":""Bobble_TextEquals"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672284"":{""id"":128672284,""category"":""bobblehead"",""name"":""Bobble_ChristmasTree"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1008""},""128672808"":{""id"":128672808,""category"":""bobblehead"",""name"":""Bobble_TextN01"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672702"":{""id"":128672702,""category"":""bobblehead"",""name"":""Bobble_TextA"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672703"":{""id"":128672703,""category"":""bobblehead"",""name"":""Bobble_TextB"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672704"":{""id"":128672704,""category"":""bobblehead"",""name"":""Bobble_TextC"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672705"":{""id"":128672705,""category"":""bobblehead"",""name"":""Bobble_TextD"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672706"":{""id"":128672706,""category"":""bobblehead"",""name"":""Bobble_TextE"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672707"":{""id"":128672707,""category"":""bobblehead"",""name"":""Bobble_TextF"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672708"":{""id"":128672708,""category"":""bobblehead"",""name"":""Bobble_TextG"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672709"":{""id"":128672709,""category"":""bobblehead"",""name"":""Bobble_TextH"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672710"":{""id"":128672710,""category"":""bobblehead"",""name"":""Bobble_TextI"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672711"":{""id"":128672711,""category"":""bobblehead"",""name"":""Bobble_TextJ"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672712"":{""id"":128672712,""category"":""bobblehead"",""name"":""Bobble_TextK"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672713"":{""id"":128672713,""category"":""bobblehead"",""name"":""Bobble_TextL"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672714"":{""id"":128672714,""category"":""bobblehead"",""name"":""Bobble_TextM"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672715"":{""id"":128672715,""category"":""bobblehead"",""name"":""Bobble_TextN"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672716"":{""id"":128672716,""category"":""bobblehead"",""name"":""Bobble_TextO"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672717"":{""id"":128672717,""category"":""bobblehead"",""name"":""Bobble_TextP"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672718"":{""id"":128672718,""category"":""bobblehead"",""name"":""Bobble_TextQ"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672719"":{""id"":128672719,""category"":""bobblehead"",""name"":""Bobble_TextR"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672720"":{""id"":128672720,""category"":""bobblehead"",""name"":""Bobble_TextS"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672721"":{""id"":128672721,""category"":""bobblehead"",""name"":""Bobble_TextT"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672722"":{""id"":128672722,""category"":""bobblehead"",""name"":""Bobble_TextU"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672723"":{""id"":128672723,""category"":""bobblehead"",""name"":""Bobble_TextV"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672724"":{""id"":128672724,""category"":""bobblehead"",""name"":""Bobble_TextW"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672725"":{""id"":128672725,""category"":""bobblehead"",""name"":""Bobble_TextX"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672726"":{""id"":128672726,""category"":""bobblehead"",""name"":""Bobble_TextY"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672727"":{""id"":128672727,""category"":""bobblehead"",""name"":""Bobble_TextZ"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672728"":{""id"":128672728,""category"":""bobblehead"",""name"":""Bobble_TextA01"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672729"":{""id"":128672729,""category"":""bobblehead"",""name"":""Bobble_TextA02"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672730"":{""id"":128672730,""category"":""bobblehead"",""name"":""Bobble_TextE01"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672731"":{""id"":128672731,""category"":""bobblehead"",""name"":""Bobble_TextE02"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672732"":{""id"":128672732,""category"":""bobblehead"",""name"":""Bobble_TextE03"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672733"":{""id"":128672733,""category"":""bobblehead"",""name"":""Bobble_TextE04"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672734"":{""id"":128672734,""category"":""bobblehead"",""name"":""Bobble_TextI01"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672735"":{""id"":128672735,""category"":""bobblehead"",""name"":""Bobble_TextI02"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672736"":{""id"":128672736,""category"":""bobblehead"",""name"":""Bobble_TextI03"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672737"":{""id"":128672737,""category"":""bobblehead"",""name"":""Bobble_TextO01"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672738"":{""id"":128672738,""category"":""bobblehead"",""name"":""Bobble_TextO02"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672739"":{""id"":128672739,""category"":""bobblehead"",""name"":""Bobble_TextO03"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672740"":{""id"":128672740,""category"":""bobblehead"",""name"":""Bobble_TextU01"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672741"":{""id"":128672741,""category"":""bobblehead"",""name"":""Bobble_TextU02"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672742"":{""id"":128672742,""category"":""bobblehead"",""name"":""Bobble_TextU03"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672743"":{""id"":128672743,""category"":""bobblehead"",""name"":""Bobble_Text0"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672744"":{""id"":128672744,""category"":""bobblehead"",""name"":""Bobble_Text1"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672745"":{""id"":128672745,""category"":""bobblehead"",""name"":""Bobble_Text2"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672746"":{""id"":128672746,""category"":""bobblehead"",""name"":""Bobble_Text3"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672747"":{""id"":128672747,""category"":""bobblehead"",""name"":""Bobble_Text4"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672748"":{""id"":128672748,""category"":""bobblehead"",""name"":""Bobble_Text5"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672749"":{""id"":128672749,""category"":""bobblehead"",""name"":""Bobble_Text6"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672750"":{""id"":128672750,""category"":""bobblehead"",""name"":""Bobble_Text7"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672751"":{""id"":128672751,""category"":""bobblehead"",""name"":""Bobble_Text8"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672752"":{""id"":128672752,""category"":""bobblehead"",""name"":""Bobble_Text9"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672753"":{""id"":128672753,""category"":""bobblehead"",""name"":""Bobble_TextQuest"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672754"":{""id"":128672754,""category"":""bobblehead"",""name"":""Bobble_TextQuest01"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672755"":{""id"":128672755,""category"":""bobblehead"",""name"":""Bobble_TextC01"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672756"":{""id"":128672756,""category"":""bobblehead"",""name"":""Bobble_TextS01"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672757"":{""id"":128672757,""category"":""bobblehead"",""name"":""Bobble_TextOE"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672758"":{""id"":128672758,""category"":""bobblehead"",""name"":""Bobble_TextHash"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672759"":{""id"":128672759,""category"":""bobblehead"",""name"":""Bobble_TextAt"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672760"":{""id"":128672760,""category"":""bobblehead"",""name"":""Bobble_TextExclam"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672761"":{""id"":128672761,""category"":""bobblehead"",""name"":""Bobble_TextQuote"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672762"":{""id"":128672762,""category"":""bobblehead"",""name"":""Bobble_TextPound"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672763"":{""id"":128672763,""category"":""bobblehead"",""name"":""Bobble_TextDollar"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672764"":{""id"":128672764,""category"":""bobblehead"",""name"":""Bobble_TextCaret"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672765"":{""id"":128672765,""category"":""bobblehead"",""name"":""Bobble_TextExclam01"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672766"":{""id"":128672766,""category"":""bobblehead"",""name"":""Bobble_TextAmper"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672767"":{""id"":128672767,""category"":""bobblehead"",""name"":""Bobble_TextAsterisk"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""}}},""ship"":{""name"":""CobraMkIII"",""modules"":{""MediumHardpoint1"":{""module"":{""id"":128049382,""name"":""Hpt_PulseLaser_Fixed_Medium"",""value"":17600,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":1,""hopper"":0}}},""MediumHardpoint2"":{""module"":{""id"":128049382,""name"":""Hpt_PulseLaser_Fixed_Medium"",""value"":17600,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":1,""hopper"":0}}},""SmallHardpoint1"":{""module"":{""id"":128049459,""name"":""Hpt_MultiCannon_Gimbal_Small"",""value"":14250,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":90,""hopper"":2100}}},""SmallHardpoint2"":{""module"":{""id"":128049459,""name"":""Hpt_MultiCannon_Gimbal_Small"",""value"":14250,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":90,""hopper"":2100}}},""TinyHardpoint1"":{""module"":{""id"":128662526,""name"":""Hpt_CloudScanner_Size0_Class2"",""value"":40633,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""TinyHardpoint2"":{""module"":{""id"":128049513,""name"":""Hpt_ChaffLauncher_Tiny"",""value"":8500,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":1,""hopper"":10}}},""Decal1"":{""module"":{""id"":128668554,""name"":""Decal_Onionhead2"",""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Decal2"":{""module"":{""id"":128668555,""name"":""Decal_Onionhead3"",""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Decal3"":{""module"":{""id"":128668555,""name"":""Decal_Onionhead3"",""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""PaintJob"":{""module"":{""id"":128670861,""name"":""PaintJob_CobraMkIII_Onionhead1_01"",""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Armour"":{""module"":{""id"":128049282,""name"":""CobraMkIII_Armour_Grade3"",""value"":267535,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""PowerPlant"":{""module"":{""id"":128064047,""name"":""Int_Powerplant_Size4_Class5"",""value"":1368568,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""MainEngines"":{""module"":{""id"":128064082,""name"":""Int_Engine_Size4_Class5"",""value"":1368568,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""FrameShiftDrive"":{""module"":{""id"":128064117,""name"":""Int_Hyperdrive_Size4_Class5"",""value"":1368568,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""LifeSupport"":{""module"":{""id"":128064152,""name"":""Int_LifeSupport_Size3_Class5"",""value"":158331,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""PowerDistributor"":{""module"":{""id"":128064192,""name"":""Int_PowerDistributor_Size3_Class5"",""value"":134582,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""Radar"":{""module"":{""id"":128064232,""name"":""Int_Sensors_Size3_Class5"",""value"":134582,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""FuelTank"":{""module"":{""name"":""Int_FuelTank_Size4_Class3"",""id"":128064349,""value"":21023,""unloaned"":21023,""free"":false,""health"":1000000,""on"":true,""priority"":1}},""Slot01_Size4"":{""module"":{""id"":128064341,""name"":""Int_CargoRack_Size4_Class1"",""value"":34328,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Slot02_Size4"":{""module"":{""id"":128666679,""name"":""Int_FuelScoop_Size4_Class5"",""value"":2862364,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""Slot03_Size4"":{""module"":{""id"":128064317,""name"":""Int_ShieldCellBank_Size4_Class5"",""value"":376829,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":1,""hopper"":3}}},""Slot04_Size2"":{""module"":{""id"":128049549,""name"":""Int_DockingComputer_Standard"",""value"":3825,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""Slot05_Size2"":{""module"":{""id"":128672289,""name"":""Int_BuggyBay_Size2_Class2"",""value"":18360,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""Slot06_Size2"":{""module"":{""id"":128666634,""name"":""Int_DetailedSurfaceScanner_Tiny"",""value"":212500,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""PlanetaryApproachSuite"":{""module"":{""name"":""Int_PlanetApproachSuite"",""id"":128672317,""value"":425,""unloaned"":425,""free"":false,""health"":1000000,""on"":true,""priority"":1}},""Bobble01"":[],""Bobble02"":[],""Bobble03"":{""module"":{""id"":128672758,""name"":""Bobble_TextHash"",""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Bobble04"":{""module"":{""id"":128672716,""name"":""Bobble_TextO"",""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Bobble05"":{""module"":{""id"":128672709,""name"":""Bobble_TextH"",""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Bobble06"":{""module"":{""id"":128672716,""name"":""Bobble_TextO"",""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Bobble07"":{""module"":{""id"":128672708,""name"":""Bobble_TextG"",""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Bobble08"":[],""Bobble09"":[],""Bobble10"":[],""ShipKitSpoiler"":[],""ShipKitWings"":[],""ShipKitTail"":[],""ShipKitBumper"":[]},""value"":{""hull"":174498,""modules"":8443221,""cargo"":0,""total"":8617719,""unloaned"":21448},""free"":false,""alive"":true,""health"":{""hull"":1000000,""shield"":0,""shieldup"":false,""integrity"":0,""paintwork"":0},""wear"":{""dirt"":0,""fade"":0,""tear"":0,""game"":0},""cockpitBreached"":false,""oxygenRemaining"":1500000,""fuel"":{""main"":{""capacity"":16,""level"":16},""reserve"":{""capacity"":0.49,""level"":0.49}},""cargo"":{""capacity"":16,""qty"":0,""items"":[],""lock"":309574046,""ts"":{""sec"":1465244951,""usec"":946000}},""passengers"":[],""refinery"":null,""id"":4},""ships"":[{""name"":""SideWinder"",""alive"":true,""station"":{""id"":3227236352,""name"":""Pascal Orbital""},""starsystem"":{""id"":""4408507107699"",""name"":""Sui Guei"",""systemaddress"":""4408507107699""},""id"":0},{""name"":""Eagle"",""alive"":true,""station"":{""id"":3226686976,""name"":""Crampton Port""},""starsystem"":{""id"":""4064909724011"",""name"":""Chemaku"",""systemaddress"":""4064909724011""},""id"":1},{""name"":""Hauler"",""alive"":true,""station"":{""id"":3223470848,""name"":""Hiyya Orbital""},""starsystem"":{""id"":""9467315627393"",""name"":""Arjung"",""systemaddress"":""9467315627393""},""id"":2},{""name"":""Type6"",""alive"":true,""station"":{""id"":3227236864,""name"":""Alcala Dock""},""starsystem"":{""id"":""4408507107699"",""name"":""Sui Guei"",""systemaddress"":""4408507107699""},""id"":3},{""name"":""CobraMkIII"",""alive"":true,""station"":{""id"":3227236864,""name"":""Alcala Dock""},""starsystem"":{""id"":""4408507107699"",""name"":""Sui Guei"",""systemaddress"":""4408507107699""},""id"":4}]}";
            EliteDangerousCompanionAppService.CompanionAppService app = new EliteDangerousCompanionAppService.CompanionAppService(true);
            Commander cmdr = EliteDangerousCompanionAppService.CompanionAppService.CommanderFromProfile(data);

            Assert.AreEqual("SavageCore", cmdr.Name);

            Assert.AreEqual(4, cmdr.StoredShips.Count);
        }
    }
}
