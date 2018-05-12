using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiCompanionAppService;
using EddiDataDefinitions;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using EddiShipMonitor;
using Rollbar;

namespace UnitTests
{
    [TestClass]
    public class CommanderDataTests
    {
        [TestInitialize]
        public void start()
        {
            // Prevent telemetry data from being reported based on test results
            RollbarLocator.RollbarInstance.Config.Enabled = false;
        }

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
          ""id"": 128064340,
          ""name"": ""Int_CargoRack_Size1_Class3"",
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
          ""id"": 128064338,
          ""name"": ""Int_CargoRack_Size1_Class1"",
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
            Profile profile = CompanionAppService.ProfileFromJson(data);

            JObject json = JObject.Parse(data);

            Ship ship = FrontierApi.ShipFromJson((JObject)json["ship"]);
            Assert.IsNotNull(ship);
            List<Ship> shipyard = FrontierApi.ShipyardFromJson(ship, json);

            Assert.AreEqual("Testy", profile.Cmdr.name);

            Assert.AreEqual("Python", ship.model);

            Assert.AreEqual(7, ship.powerplant.@class);
            Assert.AreEqual("C", ship.powerplant.grade);
            Assert.AreEqual(9, ship.hardpoints.Count);

            Hardpoint hardpoint1 = ship.hardpoints[0];
            Assert.AreEqual(3, hardpoint1.size);

            Assert.IsNotNull(hardpoint1.module);
            Assert.AreEqual(3, hardpoint1.size);
            Assert.AreEqual(3, hardpoint1.module.@class);
            Assert.AreEqual("E", hardpoint1.module.grade);
            Assert.AreEqual(126540, hardpoint1.module.price);
            Assert.AreEqual(140600, hardpoint1.module.value);

            Assert.AreEqual("7C", ship.powerplant.@class + ship.powerplant.grade);
            Assert.AreEqual(9, ship.compartments.Count);
            Assert.AreEqual(2, ship.compartments[8].size);
            Assert.AreEqual(null, ship.compartments[8].module);

            Assert.AreEqual(10, ship.cargocapacity);

            /// 7 stored ships plus active ship
            Assert.AreEqual(8, shipyard.Count);

            // First stored ship is a Vulture at Snyder Enterprise
            Ship StoredShip1 = shipyard[0];
            Assert.AreEqual("Vulture", StoredShip1.model);
            Assert.AreEqual("TZ Arietis", StoredShip1.starsystem);
            Assert.AreEqual("Snyder Enterprise", StoredShip1.station);
        }

        [TestMethod]
        public void TestCommanderFromProfile2()
        {
            string data = @"{""commander"":{""id"":1,""name"":""TestForStoredShips"",""credits"":2,""debt"":0,""currentShipId"":4,""alive"":true,""docked"":true,""rank"":{""combat"":0,""trade"":5,""explore"":3,""crime"":0,""service"":0,""empire"":2,""federation"":0,""power"":0,""cqc"":0}},""lastSystem"":{""id"":""4408507107699"",""name"":""Sui Guei"",""faction"":""Federation""},""lastStarport"":{""id"":""3227236864"",""name"":""Alcala Dock"",""faction"":""Federation"",""commodities"":[{""id"":""128049202"",""name"":""Hydrogen Fuel"",""cost_min"":113.65947826087,""cost_max"":""164.00"",""cost_mean"":""110.00"",""homebuy"":""73"",""homesell"":""70"",""consumebuy"":""3"",""baseCreationQty"":200,""baseConsumptionQty"":200,""capacity"":643346,""buyPrice"":105,""sellPrice"":99,""meanPrice"":110,""demandBracket"":0,""stockBracket"":2,""creationQty"":513065,""consumptionQty"":130281,""targetStock"":545635,""stock"":319886,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.3000"",""sec_illegal_min"":""1.13"",""sec_illegal_max"":""2.10"",""stolenmod"":""0.7500""},{""id"":""128673850"",""name"":""Hydrogen Peroxide"",""cost_min"":""589.00"",""cost_max"":846.32369565217,""cost_mean"":""917.00"",""homebuy"":""70"",""homesell"":""67"",""consumebuy"":""3"",""baseCreationQty"":0,""baseConsumptionQty"":945,""capacity"":2424326,""buyPrice"":0,""sellPrice"":847,""meanPrice"":917,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":2424326,""targetStock"":606081,""stock"":0,""demand"":1818245,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.2700"",""sec_illegal_min"":""1.14"",""sec_illegal_max"":""2.19"",""stolenmod"":""0.7500""},{""id"":""128673851"",""name"":""Liquid Oxygen"",""cost_min"":""223.00"",""cost_max"":391.43752173913,""cost_mean"":""263.00"",""homebuy"":""51"",""homesell"":""46"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":527,""capacity"":1351979,""buyPrice"":0,""sellPrice"":391,""meanPrice"":263,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":1351979,""targetStock"":337994,""stock"":0,""demand"":1009027,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.0700"",""sec_illegal_min"":""1.25"",""sec_illegal_max"":""3.04"",""stolenmod"":""0.7500""},{""id"":""128049166"",""name"":""Water"",""cost_min"":""124.00"",""cost_max"":281.05565217391,""cost_mean"":""120.00"",""homebuy"":""18"",""homesell"":""10"",""consumebuy"":""8"",""baseCreationQty"":0,""baseConsumptionQty"":163,""capacity"":418165,""buyPrice"":0,""sellPrice"":124,""meanPrice"":120,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":418165,""targetStock"":104541,""stock"":0,""demand"":78406,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128049241"",""name"":""Clothing"",""cost_min"":272.91008695652,""cost_max"":""463.00"",""cost_mean"":""285.00"",""homebuy"":""60"",""homesell"":""56"",""consumebuy"":""4"",""baseCreationQty"":35,""baseConsumptionQty"":0,""capacity"":22450,""buyPrice"":216,""sellPrice"":200,""meanPrice"":285,""demandBracket"":0,""stockBracket"":2,""creationQty"":22450,""consumptionQty"":0,""targetStock"":22450,""stock"":12567,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Consumer Items"",""volumescale"":""1.1500"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.65"",""stolenmod"":""0.7500""},{""id"":""128049240"",""name"":""Consumer Technology"",""cost_min"":""6561.00"",""cost_max"":7349.3513043478,""cost_mean"":""6769.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":27,""capacity"":31659,""buyPrice"":0,""sellPrice"":7350,""meanPrice"":6769,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":31659,""targetStock"":7914,""stock"":0,""demand"":23745,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Consumer Items"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.11"",""stolenmod"":""0.7500""},{""id"":""128049238"",""name"":""Domestic Appliances"",""cost_min"":473.77595652174,""cost_max"":""716.00"",""cost_mean"":""487.00"",""homebuy"":""69"",""homesell"":""66"",""consumebuy"":""3"",""baseCreationQty"":21,""baseConsumptionQty"":0,""capacity"":13470,""buyPrice"":404,""sellPrice"":383,""meanPrice"":487,""demandBracket"":0,""stockBracket"":2,""creationQty"":13470,""consumptionQty"":0,""targetStock"":13470,""stock"":7541,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Consumer Items"",""volumescale"":""1.2500"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128682048"",""name"":""Survival Equipment"",""cost_min"":473.77595652174,""cost_max"":""716.00"",""cost_mean"":""485.00"",""homebuy"":""69"",""homesell"":""66"",""consumebuy"":""3"",""baseCreationQty"":12,""baseConsumptionQty"":0,""capacity"":7697,""buyPrice"":331,""sellPrice"":313,""meanPrice"":485,""demandBracket"":0,""stockBracket"":3,""creationQty"":7697,""consumptionQty"":0,""targetStock"":7697,""stock"":7697,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Consumer Items"",""volumescale"":""1.2500"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128049177"",""name"":""Algae"",""cost_min"":""135.00"",""cost_max"":298.92939130435,""cost_mean"":""137.00"",""homebuy"":""27"",""homesell"":""20"",""consumebuy"":""7"",""baseCreationQty"":0,""baseConsumptionQty"":1260,""capacity"":820766,""buyPrice"":0,""sellPrice"":135,""meanPrice"":137,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":820766,""targetStock"":205191,""stock"":0,""demand"":153893.75,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.06"",""sec_illegal_max"":""1.48"",""stolenmod"":""0.7500""},{""id"":""128049182"",""name"":""Animalmeat"",""cost_min"":""1286.00"",""cost_max"":1673.8210869565,""cost_mean"":""1292.00"",""homebuy"":""80"",""homesell"":""78"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":97,""capacity"":63186,""buyPrice"":0,""sellPrice"":1286,""meanPrice"":1292,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":63186,""targetStock"":15796,""stock"":0,""demand"":11847.5,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.4000"",""sec_illegal_min"":""1.10"",""sec_illegal_max"":""1.81"",""stolenmod"":""0.7500""},{""id"":""128049189"",""name"":""Coffee"",""cost_min"":""1286.00"",""cost_max"":1673.8210869565,""cost_mean"":""1279.00"",""homebuy"":""80"",""homesell"":""78"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":97,""capacity"":15797,""buyPrice"":0,""sellPrice"":1286,""meanPrice"":1279,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":15797,""targetStock"":3949,""stock"":0,""demand"":2962,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.4000"",""sec_illegal_min"":""1.10"",""sec_illegal_max"":""1.81"",""stolenmod"":""0.7500""},{""id"":""128049183"",""name"":""Fish"",""cost_min"":""403.00"",""cost_max"":615.978,""cost_mean"":""406.00"",""homebuy"":""64"",""homesell"":""60"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":271,""capacity"":176530,""buyPrice"":0,""sellPrice"":403,""meanPrice"":406,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":176530,""targetStock"":44132,""stock"":0,""demand"":33099.5,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.2000"",""sec_illegal_min"":""1.18"",""sec_illegal_max"":""2.44"",""stolenmod"":""0.7500""},{""id"":""128049184"",""name"":""Food Cartridges"",""cost_min"":104.36139130435,""cost_max"":""267.00"",""cost_mean"":""105.00"",""homebuy"":""30"",""homesell"":""23"",""consumebuy"":""7"",""baseCreationQty"":45,""baseConsumptionQty"":0,""capacity"":115440,""buyPrice"":32,""sellPrice"":25,""meanPrice"":105,""demandBracket"":0,""stockBracket"":3,""creationQty"":115440,""consumptionQty"":0,""targetStock"":115440,""stock"":110198.735669,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128049178"",""name"":""Fruit And Vegetables"",""cost_min"":""315.00"",""cost_max"":505.08991304348,""cost_mean"":""312.00"",""homebuy"":""60"",""homesell"":""56"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":350,""capacity"":56998,""buyPrice"":0,""sellPrice"":315,""meanPrice"":312,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":56998,""targetStock"":14249,""stock"":0,""demand"":10687.25,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.1500"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.65"",""stolenmod"":""0.7500""},{""id"":""128049180"",""name"":""Grain"",""cost_min"":""207.00"",""cost_max"":371.58017391304,""cost_mean"":""210.00"",""homebuy"":""48"",""homesell"":""43"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":584,""capacity"":380419,""buyPrice"":0,""sellPrice"":207,""meanPrice"":210,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":380419,""targetStock"":95104,""stock"":0,""demand"":71328.75,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.0500"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.65"",""stolenmod"":""0.7500""},{""id"":""128049185"",""name"":""Synthetic Meat"",""cost_min"":""252.00"",""cost_max"":426.6772173913,""cost_mean"":""271.00"",""homebuy"":""54"",""homesell"":""49"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":226,""capacity"":36805,""buyPrice"":0,""sellPrice"":427,""meanPrice"":271,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":36805,""targetStock"":9201,""stock"":0,""demand"":27604,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.23"",""sec_illegal_max"":""2.88"",""stolenmod"":""0.7500""},{""id"":""128049188"",""name"":""Tea"",""cost_min"":""1459.00"",""cost_max"":1873.7669565217,""cost_mean"":""1467.00"",""homebuy"":""81"",""homesell"":""79"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":88,""capacity"":57324,""buyPrice"":0,""sellPrice"":1459,""meanPrice"":1467,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":57324,""targetStock"":14331,""stock"":0,""demand"":10748.25,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.4200"",""sec_illegal_min"":""1.09"",""sec_illegal_max"":""1.76"",""stolenmod"":""0.7500""},{""id"":""128673856"",""name"":""C M M Composite"",""cost_min"":""2966.00"",""cost_max"":3573.6862608696,""cost_mean"":""3132.00"",""homebuy"":""87"",""homesell"":""86"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":75,""capacity"":192422,""buyPrice"":0,""sellPrice"":3574,""meanPrice"":3132,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":192422,""targetStock"":48105,""stock"":0,""demand"":144317,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.3400"",""sec_illegal_min"":""1.05"",""sec_illegal_max"":""1.37"",""stolenmod"":""0.7500""},{""id"":""128672302"",""name"":""Ceramic Composites"",""cost_min"":""192.00"",""cost_max"":355.75269565217,""cost_mean"":""232.00"",""homebuy"":""46"",""homesell"":""41"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":971,""capacity"":2491223,""buyPrice"":0,""sellPrice"":356,""meanPrice"":232,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":2491223,""targetStock"":622805,""stock"":0,""demand"":1868418,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.0300"",""sec_illegal_min"":""1.28"",""sec_illegal_max"":""3.28"",""stolenmod"":""0.7500""},{""id"":""128673855"",""name"":""Insulating Membrane"",""cost_min"":""7498.00"",""cost_max"":8414.522826087,""cost_mean"":""7837.00"",""homebuy"":""93"",""homesell"":""92"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":36,""capacity"":92363,""buyPrice"":0,""sellPrice"":8412,""meanPrice"":7837,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":92363,""targetStock"":23090,""stock"":0,""demand"":69087,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.0600"",""sec_illegal_min"":""1.02"",""sec_illegal_max"":""1.15"",""stolenmod"":""0.7500""},{""id"":""128049197"",""name"":""Polymers"",""cost_min"":""152.00"",""cost_max"":311.88452173913,""cost_mean"":""171.00"",""homebuy"":""35"",""homesell"":""29"",""consumebuy"":""7"",""baseCreationQty"":0,""baseConsumptionQty"":1463,""capacity"":3753510,""buyPrice"":0,""sellPrice"":307,""meanPrice"":171,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":3753510,""targetStock"":938377,""stock"":0,""demand"":2739592,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.23"",""sec_illegal_max"":""2.88"",""stolenmod"":""0.7500""},{""id"":""128049199"",""name"":""Semiconductors"",""cost_min"":""889.00"",""cost_max"":1251.9782608696,""cost_mean"":""967.00"",""homebuy"":""76"",""homesell"":""74"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":198,""capacity"":507994,""buyPrice"":0,""sellPrice"":1238,""meanPrice"":967,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":507994,""targetStock"":126998,""stock"":0,""demand"":369928,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.3400"",""sec_illegal_min"":""1.12"",""sec_illegal_max"":""1.98"",""stolenmod"":""0.7500""},{""id"":""128049200"",""name"":""Superconductors"",""cost_min"":""6561.00"",""cost_max"":7463.6608695652,""cost_mean"":""6609.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":40,""capacity"":102626,""buyPrice"":0,""sellPrice"":7133,""meanPrice"":6609,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":102626,""targetStock"":25656,""stock"":0,""demand"":56267,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.02"",""sec_illegal_max"":""1.18"",""stolenmod"":""0.7500""},{""id"":""128064028"",""name"":""Atmospheric Extractors"",""cost_min"":356.022,""cost_max"":""569.00"",""cost_mean"":""357.00"",""homebuy"":""64"",""homesell"":""60"",""consumebuy"":""4"",""baseCreationQty"":541,""baseConsumptionQty"":0,""capacity"":347001,""buyPrice"":291,""sellPrice"":270,""meanPrice"":357,""demandBracket"":0,""stockBracket"":2,""creationQty"":347001,""consumptionQty"":0,""targetStock"":347001,""stock"":194319,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.2000"",""sec_illegal_min"":""1.18"",""sec_illegal_max"":""2.44"",""stolenmod"":""0.7500""},{""id"":""128672309"",""name"":""Building Fabricators"",""cost_min"":994.572,""cost_max"":""1344.00"",""cost_mean"":""980.00"",""homebuy"":""78"",""homesell"":""76"",""consumebuy"":""2"",""baseCreationQty"":566,""baseConsumptionQty"":0,""capacity"":1088994,""buyPrice"":784,""sellPrice"":756,""meanPrice"":980,""demandBracket"":0,""stockBracket"":3,""creationQty"":1088994,""consumptionQty"":0,""targetStock"":1088994,""stock"":1088994,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.3700"",""sec_illegal_min"":""1.11"",""sec_illegal_max"":""1.89"",""stolenmod"":""0.7500""},{""id"":""128049222"",""name"":""Crop Harvesters"",""cost_min"":2041.3407391304,""cost_max"":""2553.00"",""cost_mean"":""2021.00"",""homebuy"":""84"",""homesell"":""82"",""consumebuy"":""2"",""baseCreationQty"":643,""baseConsumptionQty"":0,""capacity"":1237143,""buyPrice"":1922,""sellPrice"":1857,""meanPrice"":2021,""demandBracket"":0,""stockBracket"":2,""creationQty"":1237143,""consumptionQty"":0,""targetStock"":1237143,""stock"":692799,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.4400"",""sec_illegal_min"":""1.06"",""sec_illegal_max"":""1.46"",""stolenmod"":""0.7500""},{""id"":""128673861"",""name"":""Emergency Power Cells"",""cost_min"":""889.00"",""cost_max"":1207.1086956522,""cost_mean"":""1011.00"",""homebuy"":""76"",""homesell"":""74"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":66,""capacity"":127006,""buyPrice"":0,""sellPrice"":1208,""meanPrice"":1011,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":127006,""targetStock"":31751,""stock"":0,""demand"":95255,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.3400"",""sec_illegal_min"":""1.12"",""sec_illegal_max"":""1.98"",""stolenmod"":""0.7500""},{""id"":""128673866"",""name"":""Exhaust Manifold"",""cost_min"":""383.00"",""cost_max"":590.846,""cost_mean"":""479.00"",""homebuy"":""63"",""homesell"":""59"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":142,""capacity"":273254,""buyPrice"":0,""sellPrice"":591,""meanPrice"":479,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":273254,""targetStock"":68313,""stock"":0,""demand"":204941,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.1900"",""sec_illegal_min"":""1.18"",""sec_illegal_max"":""2.48"",""stolenmod"":""0.7500""},{""id"":""128672307"",""name"":""Geological Equipment"",""cost_min"":1673.008,""cost_max"":""2134.00"",""cost_mean"":""1661.00"",""homebuy"":""82"",""homesell"":""80"",""consumebuy"":""2"",""baseCreationQty"":8,""baseConsumptionQty"":0,""capacity"":15393,""buyPrice"":1386,""sellPrice"":1339,""meanPrice"":1661,""demandBracket"":0,""stockBracket"":3,""creationQty"":15393,""consumptionQty"":0,""targetStock"":15393,""stock"":15393,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.4500"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.28"",""stolenmod"":""0.7500""},{""id"":""128673860"",""name"":""H N Shock Mount"",""cost_min"":447.184,""cost_max"":""683.00"",""cost_mean"":""406.00"",""homebuy"":""68"",""homesell"":""65"",""consumebuy"":""3"",""baseCreationQty"":220,""baseConsumptionQty"":0,""capacity"":423284,""buyPrice"":308,""sellPrice"":291,""meanPrice"":406,""demandBracket"":0,""stockBracket"":3,""creationQty"":423284,""consumptionQty"":0,""targetStock"":423284,""stock"":423284,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.2400"",""sec_illegal_min"":""1.16"",""sec_illegal_max"":""2.30"",""stolenmod"":""0.7500""},{""id"":""128049223"",""name"":""Marine Supplies"",""cost_min"":4002.3748695652,""cost_max"":""4723.00"",""cost_mean"":""3916.00"",""homebuy"":""89"",""homesell"":""88"",""consumebuy"":""1"",""baseCreationQty"":385,""baseConsumptionQty"":0,""capacity"":740747,""buyPrice"":3881,""sellPrice"":3799,""meanPrice"":3916,""demandBracket"":0,""stockBracket"":2,""creationQty"":740747,""consumptionQty"":0,""targetStock"":740747,""stock"":414817,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.2400"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.28"",""stolenmod"":""0.7500""},{""id"":""128049221"",""name"":""Mineral Extractors"",""cost_min"":486.25404347826,""cost_max"":""790.00"",""cost_mean"":""443.00"",""homebuy"":""70"",""homesell"":""67"",""consumebuy"":""3"",""baseCreationQty"":378,""baseConsumptionQty"":0,""capacity"":727279,""buyPrice"":438,""sellPrice"":415,""meanPrice"":443,""demandBracket"":0,""stockBracket"":2,""creationQty"":727279,""consumptionQty"":0,""targetStock"":727279,""stock"":407272,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.2700"",""sec_illegal_min"":""1.14"",""sec_illegal_max"":""2.19"",""stolenmod"":""0.7500""},{""id"":""128049217"",""name"":""Power Generators"",""cost_min"":443.53595652174,""cost_max"":""716.00"",""cost_mean"":""458.00"",""homebuy"":""69"",""homesell"":""66"",""consumebuy"":""3"",""baseCreationQty"":21,""baseConsumptionQty"":0,""capacity"":40405,""buyPrice"":392,""sellPrice"":372,""meanPrice"":458,""demandBracket"":0,""stockBracket"":2,""creationQty"":40405,""consumptionQty"":0,""targetStock"":40405,""stock"":22622,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.2500"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128672313"",""name"":""Skimer Components"",""cost_min"":841.36843478261,""cost_max"":""1203.00"",""cost_mean"":""859.00"",""homebuy"":""76"",""homesell"":""74"",""consumebuy"":""2"",""baseCreationQty"":50,""baseConsumptionQty"":0,""capacity"":32071,""buyPrice"":646,""sellPrice"":623,""meanPrice"":859,""demandBracket"":0,""stockBracket"":3,""creationQty"":32071,""consumptionQty"":0,""targetStock"":32071,""stock"":32071,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.3500"",""sec_illegal_min"":""1.11"",""sec_illegal_max"":""1.95"",""stolenmod"":""0.7500""},{""id"":""128672308"",""name"":""Thermal Cooling Units"",""cost_min"":258.47886956522,""cost_max"":""446.00"",""cost_mean"":""256.00"",""homebuy"":""59"",""homesell"":""55"",""consumebuy"":""4"",""baseCreationQty"":184,""baseConsumptionQty"":0,""capacity"":354020,""buyPrice"":155,""sellPrice"":143,""meanPrice"":256,""demandBracket"":0,""stockBracket"":3,""creationQty"":354020,""consumptionQty"":0,""targetStock"":354020,""stock"":354020,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.1400"",""sec_illegal_min"":""1.21"",""sec_illegal_max"":""2.69"",""stolenmod"":""0.7500""},{""id"":""128049218"",""name"":""Water Purifiers"",""cost_min"":258.47886956522,""cost_max"":""446.00"",""cost_mean"":""258.00"",""homebuy"":""59"",""homesell"":""55"",""consumebuy"":""4"",""baseCreationQty"":920,""baseConsumptionQty"":0,""capacity"":1770096,""buyPrice"":155,""sellPrice"":143,""meanPrice"":258,""demandBracket"":0,""stockBracket"":3,""creationQty"":1770096,""consumptionQty"":0,""targetStock"":1770096,""stock"":1527581.891511,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.1400"",""sec_illegal_min"":""1.21"",""sec_illegal_max"":""2.69"",""stolenmod"":""0.7500""},{""id"":""128682046"",""name"":""Advanced Medicines"",""cost_min"":""1136.00"",""cost_max"":1510.4237391304,""cost_mean"":""1259.00"",""homebuy"":""78"",""homesell"":""76"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":107,""capacity"":69700,""buyPrice"":0,""sellPrice"":1511,""meanPrice"":1259,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":69700,""targetStock"":17425,""stock"":0,""demand"":52275,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Medicines"",""volumescale"":""1.3800"",""sec_illegal_min"":""1.10"",""sec_illegal_max"":""1.87"",""stolenmod"":""0.7500""},{""id"":""128049210"",""name"":""Basic Medicines"",""cost_min"":268.17408695652,""cost_max"":""463.00"",""cost_mean"":""279.00"",""homebuy"":""60"",""homesell"":""56"",""consumebuy"":""4"",""baseCreationQty"":70,""baseConsumptionQty"":35,""capacity"":46357,""buyPrice"":217,""sellPrice"":200,""meanPrice"":279,""demandBracket"":0,""stockBracket"":2,""creationQty"":44899,""consumptionQty"":1458,""targetStock"":45263,""stock"":25503,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Medicines"",""volumescale"":""1.1500"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.65"",""stolenmod"":""0.7500""},{""id"":""128049209"",""name"":""Performance Enhancers"",""cost_min"":""6561.00"",""cost_max"":7448.2730434783,""cost_mean"":""6816.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":27,""capacity"":11241,""buyPrice"":0,""sellPrice"":7449,""meanPrice"":6816,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":11241,""targetStock"":2810,""stock"":0,""demand"":8431,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Medicines"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.26"",""stolenmod"":""0.7500""},{""id"":""128049669"",""name"":""Progenitor Cells"",""cost_min"":""6561.00"",""cost_max"":7416.3982608696,""cost_mean"":""6779.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":27,""capacity"":17588,""buyPrice"":0,""sellPrice"":7417,""meanPrice"":6779,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":17588,""targetStock"":4397,""stock"":0,""demand"":13191,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Medicines"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.26"",""stolenmod"":""0.7500""},{""id"":""128049176"",""name"":""Aluminium"",""cost_min"":""330.00"",""cost_max"":525.22747826087,""cost_mean"":""340.00"",""homebuy"":""61"",""homesell"":""57"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":2823,""capacity"":7242191,""buyPrice"":0,""sellPrice"":509,""meanPrice"":340,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":7242191,""targetStock"":1810547,""stock"":0,""demand"":5096742,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1600"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.60"",""stolenmod"":""0.7500""},{""id"":""128049168"",""name"":""Beryllium"",""cost_min"":""8017.00"",""cost_max"":8953.1556521739,""cost_mean"":""8288.00"",""homebuy"":""93"",""homesell"":""92"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":115,""capacity"":295024,""buyPrice"":0,""sellPrice"":8878,""meanPrice"":8288,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":295024,""targetStock"":73756,""stock"":0,""demand"":208095,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.0400"",""sec_illegal_min"":""1.02"",""sec_illegal_max"":""1.15"",""stolenmod"":""0.7500""},{""id"":""128049162"",""name"":""Cobalt"",""cost_min"":""701.00"",""cost_max"":981.54782608696,""cost_mean"":""647.00"",""homebuy"":""73"",""homesell"":""70"",""consumebuy"":""3"",""baseCreationQty"":0,""baseConsumptionQty"":162,""capacity"":415599,""buyPrice"":0,""sellPrice"":918,""meanPrice"":647,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":415599,""targetStock"":103899,""stock"":0,""demand"":259838,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.3000"",""sec_illegal_min"":""1.13"",""sec_illegal_max"":""2.10"",""stolenmod"":""0.7500""},{""id"":""128049175"",""name"":""Copper"",""cost_min"":""472.00"",""cost_max"":702.68956521739,""cost_mean"":""481.00"",""homebuy"":""67"",""homesell"":""64"",""consumebuy"":""3"",""baseCreationQty"":0,""baseConsumptionQty"":928,""capacity"":2380713,""buyPrice"":0,""sellPrice"":676,""meanPrice"":481,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":2380713,""targetStock"":595178,""stock"":0,""demand"":1632545,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.2300"",""sec_illegal_min"":""1.16"",""sec_illegal_max"":""2.33"",""stolenmod"":""0.7500""},{""id"":""128049170"",""name"":""Gallium"",""cost_min"":""5028.00"",""cost_max"":5822.7702608696,""cost_mean"":""5135.00"",""homebuy"":""90"",""homesell"":""89"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":330,""capacity"":846590,""buyPrice"":0,""sellPrice"":5722,""meanPrice"":5135,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":846590,""targetStock"":211647,""stock"":0,""demand"":575682,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1800"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.24"",""stolenmod"":""0.7500""},{""id"":""128049154"",""name"":""Gold"",""cost_min"":""9164.00"",""cost_max"":10140.858826087,""cost_mean"":""9401.00"",""homebuy"":""94"",""homesell"":""93"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":208,""capacity"":533609,""buyPrice"":0,""sellPrice"":10134,""meanPrice"":9401,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":533609,""targetStock"":133402,""stock"":0,""demand"":398018,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.09"",""stolenmod"":""0.7500""},{""id"":""128049169"",""name"":""Indium"",""cost_min"":""5743.00"",""cost_max"":6578.3104347826,""cost_mean"":""5727.00"",""homebuy"":""91"",""homesell"":""90"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":60,""capacity"":153926,""buyPrice"":0,""sellPrice"":5898,""meanPrice"":5727,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":153926,""targetStock"":38481,""stock"":0,""demand"":46420,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1400"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.22"",""stolenmod"":""0.7500""},{""id"":""128049173"",""name"":""Lithium"",""cost_min"":""1555.00"",""cost_max"":1983.1180869565,""cost_mean"":""1596.00"",""homebuy"":""81"",""homesell"":""79"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":333,""capacity"":854286,""buyPrice"":0,""sellPrice"":1827,""meanPrice"":1596,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":854286,""targetStock"":213571,""stock"":0,""demand"":469054,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.4300"",""sec_illegal_min"":""1.09"",""sec_illegal_max"":""1.74"",""stolenmod"":""0.7500""},{""id"":""128671118"",""name"":""Osmium"",""cost_min"":""6561.00"",""cost_max"":7463.6608695652,""cost_mean"":""7591.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":269,""capacity"":690099,""buyPrice"":0,""sellPrice"":7464,""meanPrice"":7591,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":690099,""targetStock"":172524,""stock"":0,""demand"":517575,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.22"",""stolenmod"":""0.7500""},{""id"":""128049153"",""name"":""Palladium"",""cost_min"":""12815.00"",""cost_max"":13928.564521739,""cost_mean"":""13298.00"",""homebuy"":""96"",""homesell"":""96"",""consumebuy"":""0"",""baseCreationQty"":0,""baseConsumptionQty"":161,""capacity"":413034,""buyPrice"":0,""sellPrice"":13928,""meanPrice"":13298,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":413034,""targetStock"":103258,""stock"":0,""demand"":309498,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.08"",""stolenmod"":""0.7500""},{""id"":""128049152"",""name"":""Platinum"",""cost_min"":""17936.00"",""cost_max"":19197.218434783,""cost_mean"":""19279.00"",""homebuy"":""97"",""homesell"":""97"",""consumebuy"":""0"",""baseCreationQty"":0,""baseConsumptionQty"":12,""capacity"":30786,""buyPrice"":0,""sellPrice"":19198,""meanPrice"":19279,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":30786,""targetStock"":7696,""stock"":0,""demand"":23090,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.04"",""stolenmod"":""0.7500""},{""id"":""128673845"",""name"":""Praseodymium"",""cost_min"":""6138.00"",""cost_max"":7014.412173913,""cost_mean"":""7156.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":142,""capacity"":364291,""buyPrice"":0,""sellPrice"":7015,""meanPrice"":7156,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":364291,""targetStock"":91072,""stock"":0,""demand"":273219,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1200"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.28"",""stolenmod"":""0.7500""},{""id"":""128673847"",""name"":""Samarium"",""cost_min"":""5373.00"",""cost_max"":6195.6016956522,""cost_mean"":""6330.00"",""homebuy"":""91"",""homesell"":""90"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":157,""capacity"":402772,""buyPrice"":0,""sellPrice"":6196,""meanPrice"":6330,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":402772,""targetStock"":100693,""stock"":0,""demand"":302079,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1600"",""sec_illegal_min"":""1.04"",""sec_illegal_max"":""1.31"",""stolenmod"":""0.7500""},{""id"":""128049155"",""name"":""Silver"",""cost_min"":""4705.00"",""cost_max"":5474.2608695652,""cost_mean"":""4775.00"",""homebuy"":""90"",""homesell"":""89"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":348,""capacity"":892768,""buyPrice"":0,""sellPrice"":5458,""meanPrice"":4775,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":892768,""targetStock"":223192,""stock"":0,""demand"":659185,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.2000"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.24"",""stolenmod"":""0.7500""},{""id"":""128049171"",""name"":""Tantalum"",""cost_min"":""3858.00"",""cost_max"":4553.8573043478,""cost_mean"":""3962.00"",""homebuy"":""89"",""homesell"":""88"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":203,""capacity"":520781,""buyPrice"":0,""sellPrice"":4470,""meanPrice"":3962,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":520781,""targetStock"":130195,""stock"":0,""demand"":355700,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.2600"",""sec_illegal_min"":""1.04"",""sec_illegal_max"":""1.29"",""stolenmod"":""0.7500""},{""id"":""128049174"",""name"":""Titanium"",""cost_min"":""1004.00"",""cost_max"":1342.4050869565,""cost_mean"":""1006.00"",""homebuy"":""77"",""homesell"":""75"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":1191,""capacity"":3055420,""buyPrice"":0,""sellPrice"":1315,""meanPrice"":1006,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":3055420,""targetStock"":763855,""stock"":0,""demand"":2154841,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.3600"",""sec_illegal_min"":""1.11"",""sec_illegal_max"":""1.92"",""stolenmod"":""0.7500""},{""id"":""128049172"",""name"":""Uranium"",""cost_min"":""2603.00"",""cost_max"":3169.14,""cost_mean"":""2705.00"",""homebuy"":""86"",""homesell"":""85"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":828,""capacity"":2124171,""buyPrice"":0,""sellPrice"":3122,""meanPrice"":2705,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":2124171,""targetStock"":531042,""stock"":0,""demand"":1494013,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.3800"",""sec_illegal_min"":""1.05"",""sec_illegal_max"":""1.39"",""stolenmod"":""0.7500""},{""id"":""128673848"",""name"":""Low Temperature Diamond"",""cost_min"":""54000.00"",""cost_max"":57594.79173913,""cost_mean"":""57445.00"",""homebuy"":""97"",""homesell"":""97"",""consumebuy"":""0"",""baseCreationQty"":0,""baseConsumptionQty"":6,""capacity"":15393,""buyPrice"":0,""sellPrice"":57595,""meanPrice"":57445,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":15393,""targetStock"":3848,""stock"":0,""demand"":11545,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.00"",""sec_illegal_max"":""1.03"",""stolenmod"":""0.7500""},{""id"":""128668550"",""name"":""Painite"",""cost_min"":""35000.00"",""cost_max"":40834.347826087,""cost_mean"":""40508.00"",""homebuy"":""100"",""homesell"":""100"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":6,""capacity"":3849,""buyPrice"":0,""sellPrice"":40835,""meanPrice"":40508,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":3849,""targetStock"":962,""stock"":0,""demand"":2887,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.04"",""stolenmod"":""0.7500""},{""id"":""128049214"",""name"":""Beer"",""cost_min"":""175.00"",""cost_max"":334.88452173913,""cost_mean"":""186.00"",""homebuy"":""42"",""homesell"":""36"",""consumebuy"":""6"",""baseCreationQty"":0,""baseConsumptionQty"":755,""capacity"":314330,""buyPrice"":0,""sellPrice"":175,""meanPrice"":186,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":314330,""targetStock"":78582,""stock"":0,""demand"":58937,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Narcotics"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.25"",""sec_illegal_max"":""3.04"",""stolenmod"":""0.7500""},{""id"":""128049216"",""name"":""Liquor"",""cost_min"":632.742,""cost_max"":765,""cost_mean"":""587.00"",""homebuy"":""71"",""homesell"":""68"",""consumebuy"":""3"",""baseCreationQty"":18,""baseConsumptionQty"":0,""capacity"":162,""buyPrice"":500,""sellPrice"":474,""meanPrice"":587,""demandBracket"":0,""stockBracket"":2,""creationQty"":162,""consumptionQty"":0,""targetStock"":162,""stock"":87,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Narcotics"",""volumescale"":""1.2800"",""sec_illegal_min"":""1.14"",""sec_illegal_max"":""2.16"",""stolenmod"":""0.7500""},{""id"":""128049215"",""name"":""Wine"",""cost_min"":""252.00"",""cost_max"":426.6772173913,""cost_mean"":""260.00"",""homebuy"":""54"",""homesell"":""49"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":452,""capacity"":253469,""buyPrice"":0,""sellPrice"":252,""meanPrice"":260,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":253469,""targetStock"":63367,""stock"":0,""demand"":47525.5,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Narcotics"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.23"",""sec_illegal_max"":""2.88"",""stolenmod"":""0.7500""},{""id"":""128066403"",""name"":""Drones"",""cost_min"":""100.00"",""cost_max"":""100.00"",""cost_mean"":""101.00"",""homebuy"":""100"",""homesell"":""100"",""consumebuy"":""1"",""baseCreationQty"":200,""baseConsumptionQty"":0,""capacity"":130281,""buyPrice"":101,""sellPrice"":100,""meanPrice"":101,""demandBracket"":0,""stockBracket"":3,""creationQty"":130281,""consumptionQty"":0,""targetStock"":130281,""stock"":130281,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""NonMarketable"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.00"",""sec_illegal_max"":""0.99"",""stolenmod"":""0.7500""},{""id"":""128049228"",""name"":""Auto Fabricators"",""cost_min"":""3612.00"",""cost_max"":4285.7520434783,""cost_mean"":""3734.00"",""homebuy"":""88"",""homesell"":""87"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":43,""capacity"":110322,""buyPrice"":0,""sellPrice"":4286,""meanPrice"":3734,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":110322,""targetStock"":27580,""stock"":0,""demand"":82742,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.2800"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.28"",""stolenmod"":""0.7500""},{""id"":""128049225"",""name"":""Computer Components"",""cost_min"":473.77595652174,""cost_max"":""716.00"",""cost_mean"":""513.00"",""homebuy"":""69"",""homesell"":""66"",""consumebuy"":""3"",""baseCreationQty"":209,""baseConsumptionQty"":0,""capacity"":134054,""buyPrice"":331,""sellPrice"":313,""meanPrice"":513,""demandBracket"":0,""stockBracket"":3,""creationQty"":134054,""consumptionQty"":0,""targetStock"":134054,""stock"":134054,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.2500"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128049226"",""name"":""Hazardous Environment Suits"",""cost_min"":""274.00"",""cost_max"":455.09917391304,""cost_mean"":""340.00"",""homebuy"":""56"",""homesell"":""52"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":612,""capacity"":1570163,""buyPrice"":0,""sellPrice"":456,""meanPrice"":340,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":1570163,""targetStock"":392540,""stock"":0,""demand"":1177623,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.1200"",""sec_illegal_min"":""1.22"",""sec_illegal_max"":""2.78"",""stolenmod"":""0.7500""},{""id"":""128673873"",""name"":""Micro Controllers"",""cost_min"":""3167.00"",""cost_max"":3794.6730434783,""cost_mean"":""3274.00"",""homebuy"":""87"",""homesell"":""86"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":24,""capacity"":61576,""buyPrice"":0,""sellPrice"":3795,""meanPrice"":3274,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":61576,""targetStock"":15394,""stock"":0,""demand"":46182,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.3200"",""sec_illegal_min"":""1.05"",""sec_illegal_max"":""1.37"",""stolenmod"":""0.7500""},{""id"":""128049227"",""name"":""Robotics"",""cost_min"":""1766.00"",""cost_max"":2226.992,""cost_mean"":""1856.00"",""homebuy"":""82"",""homesell"":""80"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":75,""capacity"":288633,""buyPrice"":0,""sellPrice"":2227,""meanPrice"":1856,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":288633,""targetStock"":72158,""stock"":0,""demand"":216475,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.4500"",""sec_illegal_min"":""1.08"",""sec_illegal_max"":""1.69"",""stolenmod"":""0.7500""},{""id"":""128682044"",""name"":""Conductive Fabrics"",""cost_min"":""472.00"",""cost_max"":702.68956521739,""cost_mean"":""507.00"",""homebuy"":""67"",""homesell"":""64"",""consumebuy"":""3"",""baseCreationQty"":0,""baseConsumptionQty"":348,""capacity"":892838,""buyPrice"":0,""sellPrice"":697,""meanPrice"":507,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":892838,""targetStock"":223209,""stock"":0,""demand"":657052,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Textiles"",""volumescale"":""1.2300"",""sec_illegal_min"":""1.16"",""sec_illegal_max"":""2.33"",""stolenmod"":""0.7500""},{""id"":""128049190"",""name"":""Leather"",""cost_min"":""175.00"",""cost_max"":334.88452173913,""cost_mean"":""205.00"",""homebuy"":""42"",""homesell"":""36"",""consumebuy"":""6"",""baseCreationQty"":0,""baseConsumptionQty"":1509,""capacity"":3871529,""buyPrice"":0,""sellPrice"":304,""meanPrice"":205,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":3871529,""targetStock"":967882,""stock"":0,""demand"":2481234,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Textiles"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.23"",""sec_illegal_max"":""2.88"",""stolenmod"":""0.7500""},{""id"":""128049191"",""name"":""Natural Fabrics"",""cost_min"":""403.00"",""cost_max"":615.978,""cost_mean"":""439.00"",""homebuy"":""64"",""homesell"":""60"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":271,""capacity"":695285,""buyPrice"":0,""sellPrice"":478,""meanPrice"":439,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":695285,""targetStock"":173821,""stock"":0,""demand"":273007,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Textiles"",""volumescale"":""1.2000"",""sec_illegal_min"":""1.18"",""sec_illegal_max"":""2.44"",""stolenmod"":""0.7500""},{""id"":""128049193"",""name"":""Synthetic Fabrics"",""cost_min"":""186.00"",""cost_max"":348.46330434783,""cost_mean"":""211.00"",""homebuy"":""45"",""homesell"":""40"",""consumebuy"":""6"",""baseCreationQty"":0,""baseConsumptionQty"":1022,""capacity"":2622070,""buyPrice"":0,""sellPrice"":343,""meanPrice"":211,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":2622070,""targetStock"":655517,""stock"":0,""demand"":1909435,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Textiles"",""volumescale"":""1.0200"",""sec_illegal_min"":""1.29"",""sec_illegal_max"":""3.34"",""stolenmod"":""0.7500""},{""id"":""128049244"",""name"":""Biowaste"",""cost_min"":36.267826086957,""cost_max"":""97.00"",""cost_mean"":""63.00"",""homebuy"":""27"",""homesell"":""20"",""consumebuy"":""7"",""baseCreationQty"":162,""baseConsumptionQty"":0,""capacity"":25977,""buyPrice"":18,""sellPrice"":13,""meanPrice"":63,""demandBracket"":0,""stockBracket"":2,""creationQty"":25977,""consumptionQty"":0,""targetStock"":25977,""stock"":14546,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Waste "",""volumescale"":""1.0000"",""sec_illegal_min"":""1.44"",""sec_illegal_max"":""4.47"",""stolenmod"":""0.7500""},{""id"":""128049248"",""name"":""Scrap"",""cost_min"":55.460869565217,""cost_max"":""120.00"",""cost_mean"":""48.00"",""homebuy"":""42"",""homesell"":""36"",""consumebuy"":""6"",""baseCreationQty"":453,""baseConsumptionQty"":0,""capacity"":290523,""buyPrice"":25,""sellPrice"":21,""meanPrice"":48,""demandBracket"":0,""stockBracket"":3,""creationQty"":290523,""consumptionQty"":0,""targetStock"":290523,""stock"":227726.405805,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Waste "",""volumescale"":""1.0000"",""sec_illegal_min"":""1.31"",""sec_illegal_max"":""3.48"",""stolenmod"":""0.7500""},{""id"":""128049236"",""name"":""Non Lethal Weapons"",""cost_min"":""1766.00"",""cost_max"":2226.992,""cost_mean"":""1837.00"",""homebuy"":""82"",""homesell"":""80"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":75,""capacity"":3425,""buyPrice"":0,""sellPrice"":2227,""meanPrice"":1837,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":3425,""targetStock"":856,""stock"":0,""demand"":2569,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Weapons"",""volumescale"":""1.4500"",""sec_illegal_min"":""1.08"",""sec_illegal_max"":""1.69"",""stolenmod"":""0.7500""},{""id"":""128049235"",""name"":""Reactive Armour"",""cost_min"":""2008.00"",""cost_max"":2501.5365217391,""cost_mean"":""2113.00"",""homebuy"":""84"",""homesell"":""82"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":68,""capacity"":9314,""buyPrice"":0,""sellPrice"":2502,""meanPrice"":2113,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":9314,""targetStock"":2328,""stock"":0,""demand"":6986,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Weapons"",""volumescale"":""1.4600"",""sec_illegal_min"":""1.08"",""sec_illegal_max"":""1.64"",""stolenmod"":""0.7500""}],""ships"":{""shipyard_list"":{""SideWinder"":{""id"":128049249,""name"":""SideWinder"",""basevalue"":32000,""sku"":""""},""Eagle"":{""id"":128049255,""name"":""Eagle"",""basevalue"":44800,""sku"":""""},""Hauler"":{""id"":128049261,""name"":""Hauler"",""basevalue"":52720,""sku"":""""},""Type7"":{""id"":128049297,""name"":""Type7"",""basevalue"":17472252,""sku"":""""},""Asp_Scout"":{""id"":128672276,""name"":""Asp_Scout"",""basevalue"":3961154,""sku"":""""},""Asp"":{""id"":128049303,""name"":""Asp"",""basevalue"":6661154,""sku"":""""},""Vulture"":{""id"":128049309,""name"":""Vulture"",""basevalue"":4925615,""sku"":""""},""Adder"":{""id"":128049267,""name"":""Adder"",""basevalue"":87808,""sku"":""""}},""unavailable_list"":[{""id"":128049321,""name"":""Federation_Dropship"",""basevalue"":14314205,""sku"":"""",""unavailableReason"":""Insufficient Rank"",""factionId"":""3"",""requiredRank"":3}]},""modules"":{""128681994"":{""id"":128681994,""category"":""weapon"",""name"":""Hpt_BeamLaser_Gimbal_Huge"",""cost"":7871544,""sku"":null},""128049434"":{""id"":128049434,""category"":""weapon"",""name"":""Hpt_BeamLaser_Gimbal_Large"",""cost"":2156544,""sku"":null},""128049430"":{""id"":128049430,""category"":""weapon"",""name"":""Hpt_BeamLaser_Fixed_Large"",""cost"":1059840,""sku"":null},""128049433"":{""id"":128049433,""category"":""weapon"",""name"":""Hpt_BeamLaser_Gimbal_Medium"",""cost"":450540,""sku"":null},""128049436"":{""id"":128049436,""category"":""weapon"",""name"":""Hpt_BeamLaser_Turret_Medium"",""cost"":1889910,""sku"":null},""128049432"":{""id"":128049432,""category"":""weapon"",""name"":""Hpt_BeamLaser_Gimbal_Small"",""cost"":67185,""sku"":null},""128049435"":{""id"":128049435,""category"":""weapon"",""name"":""Hpt_BeamLaser_Turret_Small"",""cost"":450000,""sku"":null},""128049428"":{""id"":128049428,""category"":""weapon"",""name"":""Hpt_BeamLaser_Fixed_Small"",""cost"":33687,""sku"":null},""128049389"":{""id"":128049389,""category"":""weapon"",""name"":""Hpt_PulseLaser_Turret_Medium"",""cost"":119520,""sku"":null},""128049388"":{""id"":128049388,""category"":""weapon"",""name"":""Hpt_PulseLaser_Turret_Small"",""cost"":23400,""sku"":null},""128049386"":{""id"":128049386,""category"":""weapon"",""name"":""Hpt_PulseLaser_Gimbal_Medium"",""cost"":31860,""sku"":null},""128049385"":{""id"":128049385,""category"":""weapon"",""name"":""Hpt_PulseLaser_Gimbal_Small"",""cost"":5940,""sku"":null},""128049383"":{""id"":128049383,""category"":""weapon"",""name"":""Hpt_PulseLaser_Fixed_Large"",""cost"":63360,""sku"":null},""128049382"":{""id"":128049382,""category"":""weapon"",""name"":""Hpt_PulseLaser_Fixed_Medium"",""cost"":15840,""sku"":null},""128049381"":{""id"":128049381,""category"":""weapon"",""name"":""Hpt_PulseLaser_Fixed_Small"",""cost"":1980,""sku"":null},""128049409"":{""id"":128049409,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Turret_Large"",""cost"":720360,""sku"":null},""128049408"":{""id"":128049408,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Turret_Medium"",""cost"":146520,""sku"":null},""128049407"":{""id"":128049407,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Turret_Small"",""cost"":47520,""sku"":null},""128049406"":{""id"":128049406,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Gimbal_Large"",""cost"":253440,""sku"":null},""128049404"":{""id"":128049404,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Gimbal_Small"",""cost"":7740,""sku"":null},""128049402"":{""id"":128049402,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Fixed_Large"",""cost"":126360,""sku"":null},""128049401"":{""id"":128049401,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Fixed_Medium"",""cost"":20700,""sku"":null},""128049466"":{""id"":128049466,""category"":""weapon"",""name"":""Hpt_PlasmaAccelerator_Fixed_Large"",""cost"":2746080,""sku"":null},""128049465"":{""id"":128049465,""category"":""weapon"",""name"":""Hpt_PlasmaAccelerator_Fixed_Medium"",""cost"":750780,""sku"":null},""128049450"":{""id"":128049450,""category"":""weapon"",""name"":""Hpt_Slugshot_Fixed_Large"",""cost"":1050624,""sku"":null},""128049453"":{""id"":128049453,""category"":""weapon"",""name"":""Hpt_Slugshot_Turret_Small"",""cost"":164160,""sku"":null},""128049451"":{""id"":128049451,""category"":""weapon"",""name"":""Hpt_Slugshot_Gimbal_Small"",""cost"":49248,""sku"":null},""128049452"":{""id"":128049452,""category"":""weapon"",""name"":""Hpt_Slugshot_Gimbal_Medium"",""cost"":393984,""sku"":null},""128049448"":{""id"":128049448,""category"":""weapon"",""name"":""Hpt_Slugshot_Fixed_Small"",""cost"":32400,""sku"":null},""128049449"":{""id"":128049449,""category"":""weapon"",""name"":""Hpt_Slugshot_Fixed_Medium"",""cost"":262656,""sku"":null},""128049462"":{""id"":128049462,""category"":""weapon"",""name"":""Hpt_MultiCannon_Turret_Small"",""cost"":73440,""sku"":null},""128049463"":{""id"":128049463,""category"":""weapon"",""name"":""Hpt_MultiCannon_Turret_Medium"",""cost"":1163520,""sku"":null},""128049459"":{""id"":128049459,""category"":""weapon"",""name"":""Hpt_MultiCannon_Gimbal_Small"",""cost"":12825,""sku"":null},""128049460"":{""id"":128049460,""category"":""weapon"",""name"":""Hpt_MultiCannon_Gimbal_Medium"",""cost"":51300,""sku"":null},""128049456"":{""id"":128049456,""category"":""weapon"",""name"":""Hpt_MultiCannon_Fixed_Medium"",""cost"":34200,""sku"":null},""128049510"":{""id"":128049510,""category"":""weapon"",""name"":""Hpt_AdvancedTorpPylon_Fixed_Medium"",""cost"":40320,""sku"":null},""128049509"":{""id"":128049509,""category"":""weapon"",""name"":""Hpt_AdvancedTorpPylon_Fixed_Small"",""cost"":10080,""sku"":null},""128666724"":{""id"":128666724,""category"":""weapon"",""name"":""Hpt_DumbfireMissileRack_Fixed_Small"",""cost"":28958,""sku"":null},""128671448"":{""id"":128671448,""category"":""weapon"",""name"":""Hpt_MineLauncher_Fixed_Small_Impulse"",""cost"":32751,""sku"":null},""128049500"":{""id"":128049500,""category"":""weapon"",""name"":""Hpt_MineLauncher_Fixed_Small"",""cost"":21834,""sku"":null},""128049489"":{""id"":128049489,""category"":""weapon"",""name"":""Hpt_Railgun_Fixed_Medium"",""cost"":371520,""sku"":null},""128049488"":{""id"":128049488,""category"":""weapon"",""name"":""Hpt_Railgun_Fixed_Small"",""cost"":46440,""sku"":null},""128049493"":{""id"":128049493,""category"":""weapon"",""name"":""Hpt_BasicMissileRack_Fixed_Medium"",""cost"":461160,""sku"":null},""128049492"":{""id"":128049492,""category"":""weapon"",""name"":""Hpt_BasicMissileRack_Fixed_Small"",""cost"":65340,""sku"":null},""128049525"":{""id"":128049525,""category"":""utility"",""name"":""Hpt_MiningLaser_Fixed_Small"",""cost"":6800,""sku"":null},""128049516"":{""id"":128049516,""category"":""utility"",""name"":""Hpt_ElectronicCountermeasure_Tiny"",""cost"":12500,""sku"":null},""128049549"":{""id"":128049549,""category"":""utility"",""name"":""Int_DockingComputer_Standard"",""cost"":4500,""sku"":null},""128662532"":{""id"":128662532,""category"":""utility"",""name"":""Hpt_CrimeScanner_Size0_Class3"",""cost"":121899,""sku"":null},""128662531"":{""id"":128662531,""category"":""utility"",""name"":""Hpt_CrimeScanner_Size0_Class2"",""cost"":40633,""sku"":null},""128662530"":{""id"":128662530,""category"":""utility"",""name"":""Hpt_CrimeScanner_Size0_Class1"",""cost"":13544,""sku"":null},""128662524"":{""id"":128662524,""category"":""utility"",""name"":""Hpt_CargoScanner_Size0_Class5"",""cost"":1097095,""sku"":null},""128662520"":{""id"":128662520,""category"":""utility"",""name"":""Hpt_CargoScanner_Size0_Class1"",""cost"":13544,""sku"":null},""128662527"":{""id"":128662527,""category"":""utility"",""name"":""Hpt_CloudScanner_Size0_Class3"",""cost"":121899,""sku"":null},""128662526"":{""id"":128662526,""category"":""utility"",""name"":""Hpt_CloudScanner_Size0_Class2"",""cost"":40633,""sku"":null},""128662525"":{""id"":128662525,""category"":""utility"",""name"":""Hpt_CloudScanner_Size0_Class1"",""cost"":13544,""sku"":null},""128662529"":{""id"":128662529,""category"":""utility"",""name"":""Hpt_CloudScanner_Size0_Class5"",""cost"":1097095,""sku"":null},""128662528"":{""id"":128662528,""category"":""utility"",""name"":""Hpt_CloudScanner_Size0_Class4"",""cost"":365698,""sku"":null},""128049252"":{""id"":128049252,""category"":""module"",""name"":""SideWinder_Armour_Grade3"",""cost"":80320,""sku"":null},""128049251"":{""id"":128049251,""category"":""module"",""name"":""SideWinder_Armour_Grade2"",""cost"":25600,""sku"":null},""128049250"":{""id"":128049250,""category"":""module"",""name"":""SideWinder_Armour_Grade1"",""cost"":0,""sku"":null},""128049253"":{""id"":128049253,""category"":""module"",""name"":""SideWinder_Armour_Mirrored"",""cost"":132064,""sku"":null},""128049254"":{""id"":128049254,""category"":""module"",""name"":""SideWinder_Armour_Reactive"",""cost"":139424,""sku"":null},""128049258"":{""id"":128049258,""category"":""module"",""name"":""Eagle_Armour_Grade3"",""cost"":90048,""sku"":null},""128049257"":{""id"":128049257,""category"":""module"",""name"":""Eagle_Armour_Grade2"",""cost"":26880,""sku"":null},""128049256"":{""id"":128049256,""category"":""module"",""name"":""Eagle_Armour_Grade1"",""cost"":0,""sku"":null},""128049264"":{""id"":128049264,""category"":""module"",""name"":""Hauler_Armour_Grade3"",""cost"":185047,""sku"":null},""128049263"":{""id"":128049263,""category"":""module"",""name"":""Hauler_Armour_Grade2"",""cost"":42176,""sku"":null},""128049262"":{""id"":128049262,""category"":""module"",""name"":""Hauler_Armour_Grade1"",""cost"":0,""sku"":null},""128049300"":{""id"":128049300,""category"":""module"",""name"":""Type7_Armour_Grade3"",""cost"":15725026,""sku"":null},""128049299"":{""id"":128049299,""category"":""module"",""name"":""Type7_Armour_Grade2"",""cost"":6988900,""sku"":null},""128049298"":{""id"":128049298,""category"":""module"",""name"":""Type7_Armour_Grade1"",""cost"":0,""sku"":null},""128049301"":{""id"":128049301,""category"":""module"",""name"":""Type7_Armour_Mirrored"",""cost"":37163480,""sku"":null},""128049302"":{""id"":128049302,""category"":""module"",""name"":""Type7_Armour_Reactive"",""cost"":41182097,""sku"":null},""128672280"":{""id"":128672280,""category"":""module"",""name"":""Asp_Scout_Armour_Grade3"",""cost"":3565038,""sku"":null},""128672279"":{""id"":128672279,""category"":""module"",""name"":""Asp_Scout_Armour_Grade2"",""cost"":1584461,""sku"":null},""128672278"":{""id"":128672278,""category"":""module"",""name"":""Asp_Scout_Armour_Grade1"",""cost"":0,""sku"":null},""128672281"":{""id"":128672281,""category"":""module"",""name"":""Asp_Scout_Armour_Mirrored"",""cost"":8425374,""sku"":null},""128672282"":{""id"":128672282,""category"":""module"",""name"":""Asp_Scout_Armour_Reactive"",""cost"":9336439,""sku"":null},""128049306"":{""id"":128049306,""category"":""module"",""name"":""Asp_Armour_Grade3"",""cost"":5995038,""sku"":null},""128049305"":{""id"":128049305,""category"":""module"",""name"":""Asp_Armour_Grade2"",""cost"":2664461,""sku"":null},""128049304"":{""id"":128049304,""category"":""module"",""name"":""Asp_Armour_Grade1"",""cost"":0,""sku"":null},""128049307"":{""id"":128049307,""category"":""module"",""name"":""Asp_Armour_Mirrored"",""cost"":14168274,""sku"":null},""128049308"":{""id"":128049308,""category"":""module"",""name"":""Asp_Armour_Reactive"",""cost"":15700339,""sku"":null},""128049312"":{""id"":128049312,""category"":""module"",""name"":""Vulture_Armour_Grade3"",""cost"":4433053,""sku"":null},""128049311"":{""id"":128049311,""category"":""module"",""name"":""Vulture_Armour_Grade2"",""cost"":1970246,""sku"":null},""128049310"":{""id"":128049310,""category"":""module"",""name"":""Vulture_Armour_Grade1"",""cost"":0,""sku"":null},""128049313"":{""id"":128049313,""category"":""module"",""name"":""Vulture_Armour_Mirrored"",""cost"":10476783,""sku"":null},""128049314"":{""id"":128049314,""category"":""module"",""name"":""Vulture_Armour_Reactive"",""cost"":11609674,""sku"":null},""128049270"":{""id"":128049270,""category"":""module"",""name"":""Adder_Armour_Grade3"",""cost"":79027,""sku"":null},""128049269"":{""id"":128049269,""category"":""module"",""name"":""Adder_Armour_Grade2"",""cost"":35123,""sku"":null},""128049268"":{""id"":128049268,""category"":""module"",""name"":""Adder_Armour_Grade1"",""cost"":0,""sku"":null},""128049271"":{""id"":128049271,""category"":""module"",""name"":""Adder_Armour_Mirrored"",""cost"":186767,""sku"":null},""128049272"":{""id"":128049272,""category"":""module"",""name"":""Adder_Armour_Reactive"",""cost"":206963,""sku"":null},""128049324"":{""id"":128049324,""category"":""module"",""name"":""Federation_Dropship_Armour_Grade3"",""cost"":12882784,""sku"":null},""128049323"":{""id"":128049323,""category"":""module"",""name"":""Federation_Dropship_Armour_Grade2"",""cost"":5725682,""sku"":null},""128049322"":{""id"":128049322,""category"":""module"",""name"":""Federation_Dropship_Armour_Grade1"",""cost"":0,""sku"":null},""128049325"":{""id"":128049325,""category"":""module"",""name"":""Federation_Dropship_Armour_Mirrored"",""cost"":30446314,""sku"":null},""128049326"":{""id"":128049326,""category"":""module"",""name"":""Federation_Dropship_Armour_Reactive"",""cost"":33738581,""sku"":null},""128662535"":{""id"":128662535,""category"":""module"",""name"":""Int_StellarBodyDiscoveryScanner_Standard"",""cost"":1000,""sku"":null},""128064338"":{""id"":128064338,""category"":""module"",""name"":""Int_CargoRack_Size1_Class1"",""cost"":1000,""sku"":null},""128666684"":{""id"":128666684,""category"":""module"",""name"":""Int_Refinery_Size1_Class1"",""cost"":6000,""sku"":null},""128666644"":{""id"":128666644,""category"":""module"",""name"":""Int_FuelScoop_Size1_Class1"",""cost"":309,""sku"":null},""128672317"":{""id"":128672317,""category"":""module"",""name"":""Int_PlanetApproachSuite"",""cost"":500,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128666704"":{""id"":128666704,""category"":""module"",""name"":""Int_FSDInterdictor_Size1_Class1"",""cost"":12000,""sku"":null},""128066532"":{""id"":128066532,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size1_Class1"",""cost"":600,""sku"":null},""128064263"":{""id"":128064263,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class1"",""cost"":1978,""sku"":null},""128064112"":{""id"":128064112,""category"":""module"",""name"":""Int_Hyperdrive_Size3_Class5"",""cost"":507912,""sku"":null},""128064107"":{""id"":128064107,""category"":""module"",""name"":""Int_Hyperdrive_Size2_Class5"",""cost"":160224,""sku"":null},""128064111"":{""id"":128064111,""category"":""module"",""name"":""Int_Hyperdrive_Size3_Class4"",""cost"":169304,""sku"":null},""128064110"":{""id"":128064110,""category"":""module"",""name"":""Int_Hyperdrive_Size3_Class3"",""cost"":56435,""sku"":null},""128064105"":{""id"":128064105,""category"":""module"",""name"":""Int_Hyperdrive_Size2_Class3"",""cost"":17803,""sku"":null},""128064109"":{""id"":128064109,""category"":""module"",""name"":""Int_Hyperdrive_Size3_Class2"",""cost"":18812,""sku"":null},""128064104"":{""id"":128064104,""category"":""module"",""name"":""Int_Hyperdrive_Size2_Class2"",""cost"":5934,""sku"":null},""128064108"":{""id"":128064108,""category"":""module"",""name"":""Int_Hyperdrive_Size3_Class1"",""cost"":6271,""sku"":null},""128064103"":{""id"":128064103,""category"":""module"",""name"":""Int_Hyperdrive_Size2_Class1"",""cost"":1978,""sku"":null},""128064272"":{""id"":128064272,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class5"",""cost"":507912,""sku"":null},""128064267"":{""id"":128064267,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class5"",""cost"":160224,""sku"":null},""128064271"":{""id"":128064271,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class4"",""cost"":169304,""sku"":null},""128064266"":{""id"":128064266,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class4"",""cost"":53408,""sku"":null},""128671333"":{""id"":128671333,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class3_Fast"",""cost"":84653,""sku"":null},""128064270"":{""id"":128064270,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class3"",""cost"":56435,""sku"":null},""128671332"":{""id"":128671332,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class3_Fast"",""cost"":26705,""sku"":null},""128671331"":{""id"":128671331,""category"":""module"",""name"":""Int_ShieldGenerator_Size1_Class3_Fast"",""cost"":7713,""sku"":null},""128064265"":{""id"":128064265,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class3"",""cost"":17803,""sku"":null},""128064269"":{""id"":128064269,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class2"",""cost"":18812,""sku"":null},""128064264"":{""id"":128064264,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class2"",""cost"":5934,""sku"":null},""128064268"":{""id"":128064268,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class1"",""cost"":6271,""sku"":null},""128672293"":{""id"":128672293,""category"":""module"",""name"":""Int_BuggyBay_Size6_Class2"",""cost"":691200,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128672291"":{""id"":128672291,""category"":""module"",""name"":""Int_BuggyBay_Size4_Class2"",""cost"":86400,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128672292"":{""id"":128672292,""category"":""module"",""name"":""Int_BuggyBay_Size6_Class1"",""cost"":576000,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128672289"":{""id"":128672289,""category"":""module"",""name"":""Int_BuggyBay_Size2_Class2"",""cost"":21600,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128672290"":{""id"":128672290,""category"":""module"",""name"":""Int_BuggyBay_Size4_Class1"",""cost"":72000,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128672288"":{""id"":128672288,""category"":""module"",""name"":""Int_BuggyBay_Size2_Class1"",""cost"":18000,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128671338"":{""id"":128671338,""category"":""module"",""name"":""Int_ShieldGenerator_Size8_Class3_Fast"",""cost"":27097748,""sku"":null},""128671337"":{""id"":128671337,""category"":""module"",""name"":""Int_ShieldGenerator_Size7_Class3_Fast"",""cost"":8548185,""sku"":null},""128064294"":{""id"":128064294,""category"":""module"",""name"":""Int_ShieldGenerator_Size8_Class2"",""cost"":6021722,""sku"":null},""128064293"":{""id"":128064293,""category"":""module"",""name"":""Int_ShieldGenerator_Size8_Class1"",""cost"":2007241,""sku"":null},""128064288"":{""id"":128064288,""category"":""module"",""name"":""Int_ShieldGenerator_Size7_Class1"",""cost"":633199,""sku"":null},""128666712"":{""id"":128666712,""category"":""module"",""name"":""Int_FSDInterdictor_Size1_Class3"",""cost"":108000,""sku"":null},""128666709"":{""id"":128666709,""category"":""module"",""name"":""Int_FSDInterdictor_Size2_Class2"",""cost"":100800,""sku"":null},""128666708"":{""id"":128666708,""category"":""module"",""name"":""Int_FSDInterdictor_Size1_Class2"",""cost"":36000,""sku"":null},""128666705"":{""id"":128666705,""category"":""module"",""name"":""Int_FSDInterdictor_Size2_Class1"",""cost"":33600,""sku"":null},""128064041"":{""id"":128064041,""category"":""module"",""name"":""Int_Powerplant_Size3_Class4"",""cost"":169304,""sku"":null},""128064036"":{""id"":128064036,""category"":""module"",""name"":""Int_Powerplant_Size2_Class4"",""cost"":53408,""sku"":null},""128064040"":{""id"":128064040,""category"":""module"",""name"":""Int_Powerplant_Size3_Class3"",""cost"":56435,""sku"":null},""128064035"":{""id"":128064035,""category"":""module"",""name"":""Int_Powerplant_Size2_Class3"",""cost"":17803,""sku"":null},""128064039"":{""id"":128064039,""category"":""module"",""name"":""Int_Powerplant_Size3_Class2"",""cost"":18812,""sku"":null},""128064034"":{""id"":128064034,""category"":""module"",""name"":""Int_Powerplant_Size2_Class2"",""cost"":5934,""sku"":null},""128064038"":{""id"":128064038,""category"":""module"",""name"":""Int_Powerplant_Size3_Class1"",""cost"":6271,""sku"":null},""128064033"":{""id"":128064033,""category"":""module"",""name"":""Int_Powerplant_Size2_Class1"",""cost"":1978,""sku"":null},""128064345"":{""id"":128064345,""category"":""module"",""name"":""Int_CargoRack_Size8_Class1"",""cost"":3829866,""sku"":null},""128064344"":{""id"":128064344,""category"":""module"",""name"":""Int_CargoRack_Size7_Class1"",""cost"":1178420,""sku"":null},""128064343"":{""id"":128064343,""category"":""module"",""name"":""Int_CargoRack_Size6_Class1"",""cost"":362591,""sku"":null},""128064342"":{""id"":128064342,""category"":""module"",""name"":""Int_CargoRack_Size5_Class1"",""cost"":111566,""sku"":null},""128064341"":{""id"":128064341,""category"":""module"",""name"":""Int_CargoRack_Size4_Class1"",""cost"":34328,""sku"":null},""128064340"":{""id"":128064340,""category"":""module"",""name"":""Int_CargoRack_Size3_Class1"",""cost"":10563,""sku"":null},""128064339"":{""id"":128064339,""category"":""module"",""name"":""Int_CargoRack_Size2_Class1"",""cost"":3250,""sku"":null},""128064353"":{""id"":128064353,""category"":""module"",""name"":""Int_FuelTank_Size8_Class3"",""cost"":5428429,""sku"":null},""128064352"":{""id"":128064352,""category"":""module"",""name"":""Int_FuelTank_Size7_Class3"",""cost"":1780914,""sku"":null},""128064351"":{""id"":128064351,""category"":""module"",""name"":""Int_FuelTank_Size6_Class3"",""cost"":341577,""sku"":null},""128064350"":{""id"":128064350,""category"":""module"",""name"":""Int_FuelTank_Size5_Class3"",""cost"":97754,""sku"":null},""128064349"":{""id"":128064349,""category"":""module"",""name"":""Int_FuelTank_Size4_Class3"",""cost"":24734,""sku"":null},""128064348"":{""id"":128064348,""category"":""module"",""name"":""Int_FuelTank_Size3_Class3"",""cost"":7063,""sku"":null},""128064347"":{""id"":128064347,""category"":""module"",""name"":""Int_FuelTank_Size2_Class3"",""cost"":3750,""sku"":null},""128064346"":{""id"":128064346,""category"":""module"",""name"":""Int_FuelTank_Size1_Class3"",""cost"":1000,""sku"":null},""128671252"":{""id"":128671252,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size1_Class4"",""cost"":4800,""sku"":null},""128671264"":{""id"":128671264,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size7_Class1"",""cost"":437400,""sku"":null},""128671260"":{""id"":128671260,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size5_Class2"",""cost"":97200,""sku"":null},""128671256"":{""id"":128671256,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size3_Class3"",""cost"":21600,""sku"":null},""128671251"":{""id"":128671251,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size1_Class3"",""cost"":2400,""sku"":null},""128671259"":{""id"":128671259,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size5_Class1"",""cost"":48600,""sku"":null},""128671255"":{""id"":128671255,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size3_Class2"",""cost"":10800,""sku"":null},""128671250"":{""id"":128671250,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size1_Class2"",""cost"":1200,""sku"":null},""128671254"":{""id"":128671254,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size3_Class1"",""cost"":5400,""sku"":null},""128671249"":{""id"":128671249,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size1_Class1"",""cost"":600,""sku"":null},""128666701"":{""id"":128666701,""category"":""module"",""name"":""Int_Refinery_Size2_Class5"",""cost"":1020600,""sku"":null},""128666700"":{""id"":128666700,""category"":""module"",""name"":""Int_Refinery_Size1_Class5"",""cost"":486000,""sku"":null},""128666693"":{""id"":128666693,""category"":""module"",""name"":""Int_Refinery_Size2_Class3"",""cost"":113400,""sku"":null},""128666696"":{""id"":128666696,""category"":""module"",""name"":""Int_Refinery_Size1_Class4"",""cost"":162000,""sku"":null},""128666689"":{""id"":128666689,""category"":""module"",""name"":""Int_Refinery_Size2_Class2"",""cost"":37800,""sku"":null},""128666692"":{""id"":128666692,""category"":""module"",""name"":""Int_Refinery_Size1_Class3"",""cost"":54000,""sku"":null},""128666688"":{""id"":128666688,""category"":""module"",""name"":""Int_Refinery_Size1_Class2"",""cost"":18000,""sku"":null},""128666685"":{""id"":128666685,""category"":""module"",""name"":""Int_Refinery_Size2_Class1"",""cost"":12600,""sku"":null},""128064255"":{""id"":128064255,""category"":""module"",""name"":""Int_Sensors_Size8_Class3"",""cost"":4359903,""sku"":null},""128064250"":{""id"":128064250,""category"":""module"",""name"":""Int_Sensors_Size7_Class3"",""cost"":1557108,""sku"":null},""128064254"":{""id"":128064254,""category"":""module"",""name"":""Int_Sensors_Size8_Class2"",""cost"":1743961,""sku"":null},""128064249"":{""id"":128064249,""category"":""module"",""name"":""Int_Sensors_Size7_Class2"",""cost"":622843,""sku"":null},""128064253"":{""id"":128064253,""category"":""module"",""name"":""Int_Sensors_Size8_Class1"",""cost"":697584,""sku"":null},""128064248"":{""id"":128064248,""category"":""module"",""name"":""Int_Sensors_Size7_Class1"",""cost"":249137,""sku"":null},""128064191"":{""id"":128064191,""category"":""module"",""name"":""Int_PowerDistributor_Size3_Class4"",""cost"":63333,""sku"":null},""128064181"":{""id"":128064181,""category"":""module"",""name"":""Int_PowerDistributor_Size1_Class4"",""cost"":8078,""sku"":null},""128064186"":{""id"":128064186,""category"":""module"",""name"":""Int_PowerDistributor_Size2_Class4"",""cost"":22619,""sku"":null},""128064190"":{""id"":128064190,""category"":""module"",""name"":""Int_PowerDistributor_Size3_Class3"",""cost"":25333,""sku"":null},""128064185"":{""id"":128064185,""category"":""module"",""name"":""Int_PowerDistributor_Size2_Class3"",""cost"":9048,""sku"":null},""128064180"":{""id"":128064180,""category"":""module"",""name"":""Int_PowerDistributor_Size1_Class3"",""cost"":3231,""sku"":null},""128064189"":{""id"":128064189,""category"":""module"",""name"":""Int_PowerDistributor_Size3_Class2"",""cost"":10133,""sku"":null},""128064179"":{""id"":128064179,""category"":""module"",""name"":""Int_PowerDistributor_Size1_Class2"",""cost"":1293,""sku"":null},""128064184"":{""id"":128064184,""category"":""module"",""name"":""Int_PowerDistributor_Size2_Class2"",""cost"":3619,""sku"":null},""128064188"":{""id"":128064188,""category"":""module"",""name"":""Int_PowerDistributor_Size3_Class1"",""cost"":4053,""sku"":null},""128064183"":{""id"":128064183,""category"":""module"",""name"":""Int_PowerDistributor_Size2_Class1"",""cost"":1448,""sku"":null},""128064178"":{""id"":128064178,""category"":""module"",""name"":""Int_PowerDistributor_Size1_Class1"",""cost"":517,""sku"":null},""128064215"":{""id"":128064215,""category"":""module"",""name"":""Int_PowerDistributor_Size8_Class3"",""cost"":4359903,""sku"":null},""128064210"":{""id"":128064210,""category"":""module"",""name"":""Int_PowerDistributor_Size7_Class3"",""cost"":1557108,""sku"":null},""128064209"":{""id"":128064209,""category"":""module"",""name"":""Int_PowerDistributor_Size7_Class2"",""cost"":622843,""sku"":null},""128064213"":{""id"":128064213,""category"":""module"",""name"":""Int_PowerDistributor_Size8_Class1"",""cost"":697584,""sku"":null},""128064208"":{""id"":128064208,""category"":""module"",""name"":""Int_PowerDistributor_Size7_Class1"",""cost"":249137,""sku"":null},""128064077"":{""id"":128064077,""category"":""module"",""name"":""Int_Engine_Size3_Class5"",""cost"":507912,""sku"":null},""128064076"":{""id"":128064076,""category"":""module"",""name"":""Int_Engine_Size3_Class4"",""cost"":169304,""sku"":null},""128064071"":{""id"":128064071,""category"":""module"",""name"":""Int_Engine_Size2_Class4"",""cost"":53408,""sku"":null},""128064075"":{""id"":128064075,""category"":""module"",""name"":""Int_Engine_Size3_Class3"",""cost"":56435,""sku"":null},""128064070"":{""id"":128064070,""category"":""module"",""name"":""Int_Engine_Size2_Class3"",""cost"":17803,""sku"":null},""128064074"":{""id"":128064074,""category"":""module"",""name"":""Int_Engine_Size3_Class2"",""cost"":18812,""sku"":null},""128064069"":{""id"":128064069,""category"":""module"",""name"":""Int_Engine_Size2_Class2"",""cost"":5934,""sku"":null},""128064073"":{""id"":128064073,""category"":""module"",""name"":""Int_Engine_Size3_Class1"",""cost"":6271,""sku"":null},""128064068"":{""id"":128064068,""category"":""module"",""name"":""Int_Engine_Size2_Class1"",""cost"":1978,""sku"":null},""128666677"":{""id"":128666677,""category"":""module"",""name"":""Int_FuelScoop_Size2_Class5"",""cost"":284844,""sku"":null},""128666670"":{""id"":128666670,""category"":""module"",""name"":""Int_FuelScoop_Size3_Class4"",""cost"":225738,""sku"":null},""128666662"":{""id"":128666662,""category"":""module"",""name"":""Int_FuelScoop_Size3_Class3"",""cost"":56435,""sku"":null},""128666676"":{""id"":128666676,""category"":""module"",""name"":""Int_FuelScoop_Size1_Class5"",""cost"":82270,""sku"":null},""128666661"":{""id"":128666661,""category"":""module"",""name"":""Int_FuelScoop_Size2_Class3"",""cost"":17803,""sku"":null},""128666660"":{""id"":128666660,""category"":""module"",""name"":""Int_FuelScoop_Size1_Class3"",""cost"":5142,""sku"":null},""128666654"":{""id"":128666654,""category"":""module"",""name"":""Int_FuelScoop_Size3_Class2"",""cost"":14109,""sku"":null},""128666653"":{""id"":128666653,""category"":""module"",""name"":""Int_FuelScoop_Size2_Class2"",""cost"":4451,""sku"":null},""128666646"":{""id"":128666646,""category"":""module"",""name"":""Int_FuelScoop_Size3_Class1"",""cost"":3386,""sku"":null},""128666652"":{""id"":128666652,""category"":""module"",""name"":""Int_FuelScoop_Size1_Class2"",""cost"":1285,""sku"":null},""128666645"":{""id"":128666645,""category"":""module"",""name"":""Int_FuelScoop_Size2_Class1"",""cost"":1068,""sku"":null},""128666703"":{""id"":128666703,""category"":""module"",""name"":""Int_Refinery_Size4_Class5"",""cost"":4500846,""sku"":null},""128666695"":{""id"":128666695,""category"":""module"",""name"":""Int_Refinery_Size4_Class3"",""cost"":500094,""sku"":null},""128666702"":{""id"":128666702,""category"":""module"",""name"":""Int_Refinery_Size3_Class5"",""cost"":2143260,""sku"":null},""128666698"":{""id"":128666698,""category"":""module"",""name"":""Int_Refinery_Size3_Class4"",""cost"":714420,""sku"":null},""128666694"":{""id"":128666694,""category"":""module"",""name"":""Int_Refinery_Size3_Class3"",""cost"":238140,""sku"":null},""128666691"":{""id"":128666691,""category"":""module"",""name"":""Int_Refinery_Size4_Class2"",""cost"":166698,""sku"":null},""128666687"":{""id"":128666687,""category"":""module"",""name"":""Int_Refinery_Size4_Class1"",""cost"":55566,""sku"":null},""128666690"":{""id"":128666690,""category"":""module"",""name"":""Int_Refinery_Size3_Class2"",""cost"":79380,""sku"":null},""128666686"":{""id"":128666686,""category"":""module"",""name"":""Int_Refinery_Size3_Class1"",""cost"":26460,""sku"":null},""128671284"":{""id"":128671284,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size7_Class1"",""cost"":437400,""sku"":null},""128671280"":{""id"":128671280,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size5_Class2"",""cost"":97200,""sku"":null},""128671276"":{""id"":128671276,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size3_Class3"",""cost"":21600,""sku"":null},""128671271"":{""id"":128671271,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size1_Class3"",""cost"":2400,""sku"":null},""128671279"":{""id"":128671279,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size5_Class1"",""cost"":48600,""sku"":null},""128671275"":{""id"":128671275,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size3_Class2"",""cost"":10800,""sku"":null},""128671270"":{""id"":128671270,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size1_Class2"",""cost"":1200,""sku"":null},""128671274"":{""id"":128671274,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size3_Class1"",""cost"":5400,""sku"":null},""128671269"":{""id"":128671269,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size1_Class1"",""cost"":600,""sku"":null},""128667637"":{""id"":128667637,""category"":""module"",""name"":""Int_Repairer_Size8_Class5"",""cost"":49589823,""sku"":null},""128667629"":{""id"":128667629,""category"":""module"",""name"":""Int_Repairer_Size8_Class4"",""cost"":16529941,""sku"":null},""128667636"":{""id"":128667636,""category"":""module"",""name"":""Int_Repairer_Size7_Class5"",""cost"":27549901,""sku"":null},""128667621"":{""id"":128667621,""category"":""module"",""name"":""Int_Repairer_Size8_Class3"",""cost"":5509980,""sku"":null},""128667635"":{""id"":128667635,""category"":""module"",""name"":""Int_Repairer_Size6_Class5"",""cost"":15305501,""sku"":null},""128667620"":{""id"":128667620,""category"":""module"",""name"":""Int_Repairer_Size7_Class3"",""cost"":3061100,""sku"":null},""128667627"":{""id"":128667627,""category"":""module"",""name"":""Int_Repairer_Size6_Class4"",""cost"":5101834,""sku"":null},""128667634"":{""id"":128667634,""category"":""module"",""name"":""Int_Repairer_Size5_Class5"",""cost"":8503056,""sku"":null},""128667633"":{""id"":128667633,""category"":""module"",""name"":""Int_Repairer_Size4_Class5"",""cost"":4723920,""sku"":null},""128667613"":{""id"":128667613,""category"":""module"",""name"":""Int_Repairer_Size8_Class2"",""cost"":1836660,""sku"":null},""128667619"":{""id"":128667619,""category"":""module"",""name"":""Int_Repairer_Size6_Class3"",""cost"":1700611,""sku"":null},""128667612"":{""id"":128667612,""category"":""module"",""name"":""Int_Repairer_Size7_Class2"",""cost"":1020367,""sku"":null},""128667618"":{""id"":128667618,""category"":""module"",""name"":""Int_Repairer_Size5_Class3"",""cost"":944784,""sku"":null},""128667625"":{""id"":128667625,""category"":""module"",""name"":""Int_Repairer_Size4_Class4"",""cost"":1574640,""sku"":null},""128663561"":{""id"":128663561,""category"":""module"",""name"":""Int_StellarBodyDiscoveryScanner_Advanced"",""cost"":1545000,""sku"":null},""128663560"":{""id"":128663560,""category"":""module"",""name"":""Int_StellarBodyDiscoveryScanner_Intermediate"",""cost"":505000,""sku"":null},""128666634"":{""id"":128666634,""category"":""module"",""name"":""Int_DetailedSurfaceScanner_Tiny"",""cost"":250000,""sku"":null},""128064147"":{""id"":128064147,""category"":""module"",""name"":""Int_LifeSupport_Size2_Class5"",""cost"":56547,""sku"":null},""128064142"":{""id"":128064142,""category"":""module"",""name"":""Int_LifeSupport_Size1_Class5"",""cost"":20195,""sku"":null},""128064151"":{""id"":128064151,""category"":""module"",""name"":""Int_LifeSupport_Size3_Class4"",""cost"":63333,""sku"":null},""128064141"":{""id"":128064141,""category"":""module"",""name"":""Int_LifeSupport_Size1_Class4"",""cost"":8078,""sku"":null},""128064146"":{""id"":128064146,""category"":""module"",""name"":""Int_LifeSupport_Size2_Class4"",""cost"":22619,""sku"":null},""128064150"":{""id"":128064150,""category"":""module"",""name"":""Int_LifeSupport_Size3_Class3"",""cost"":25333,""sku"":null},""128064145"":{""id"":128064145,""category"":""module"",""name"":""Int_LifeSupport_Size2_Class3"",""cost"":9048,""sku"":null},""128064140"":{""id"":128064140,""category"":""module"",""name"":""Int_LifeSupport_Size1_Class3"",""cost"":3231,""sku"":null},""128064149"":{""id"":128064149,""category"":""module"",""name"":""Int_LifeSupport_Size3_Class2"",""cost"":10133,""sku"":null},""128064139"":{""id"":128064139,""category"":""module"",""name"":""Int_LifeSupport_Size1_Class2"",""cost"":1293,""sku"":null},""128064144"":{""id"":128064144,""category"":""module"",""name"":""Int_LifeSupport_Size2_Class2"",""cost"":3619,""sku"":null},""128064148"":{""id"":128064148,""category"":""module"",""name"":""Int_LifeSupport_Size3_Class1"",""cost"":4053,""sku"":null},""128064143"":{""id"":128064143,""category"":""module"",""name"":""Int_LifeSupport_Size2_Class1"",""cost"":1448,""sku"":null},""128671263"":{""id"":128671263,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size5_Class5"",""cost"":777600,""sku"":null},""128671258"":{""id"":128671258,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size3_Class5"",""cost"":86400,""sku"":null},""128671267"":{""id"":128671267,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size7_Class4"",""cost"":3499200,""sku"":null},""128671262"":{""id"":128671262,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size5_Class4"",""cost"":388800,""sku"":null},""128671266"":{""id"":128671266,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size7_Class3"",""cost"":1749600,""sku"":null},""128671265"":{""id"":128671265,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size7_Class2"",""cost"":874800,""sku"":null},""128064231"":{""id"":128064231,""category"":""module"",""name"":""Int_Sensors_Size3_Class4"",""cost"":63333,""sku"":null},""128064221"":{""id"":128064221,""category"":""module"",""name"":""Int_Sensors_Size1_Class4"",""cost"":8078,""sku"":null},""128064226"":{""id"":128064226,""category"":""module"",""name"":""Int_Sensors_Size2_Class4"",""cost"":22619,""sku"":null},""128064230"":{""id"":128064230,""category"":""module"",""name"":""Int_Sensors_Size3_Class3"",""cost"":25333,""sku"":null},""128064225"":{""id"":128064225,""category"":""module"",""name"":""Int_Sensors_Size2_Class3"",""cost"":9048,""sku"":null},""128064220"":{""id"":128064220,""category"":""module"",""name"":""Int_Sensors_Size1_Class3"",""cost"":3231,""sku"":null},""128064229"":{""id"":128064229,""category"":""module"",""name"":""Int_Sensors_Size3_Class2"",""cost"":10133,""sku"":null},""128064219"":{""id"":128064219,""category"":""module"",""name"":""Int_Sensors_Size1_Class2"",""cost"":1293,""sku"":null},""128064224"":{""id"":128064224,""category"":""module"",""name"":""Int_Sensors_Size2_Class2"",""cost"":3619,""sku"":null},""128064228"":{""id"":128064228,""category"":""module"",""name"":""Int_Sensors_Size3_Class1"",""cost"":4053,""sku"":null},""128064223"":{""id"":128064223,""category"":""module"",""name"":""Int_Sensors_Size2_Class1"",""cost"":1448,""sku"":null},""128064218"":{""id"":128064218,""category"":""module"",""name"":""Int_Sensors_Size1_Class1"",""cost"":517,""sku"":null},""128064311"":{""id"":128064311,""category"":""module"",""name"":""Int_ShieldCellBank_Size3_Class4"",""cost"":63333,""sku"":null},""128064301"":{""id"":128064301,""category"":""module"",""name"":""Int_ShieldCellBank_Size1_Class4"",""cost"":8078,""sku"":null},""128064306"":{""id"":128064306,""category"":""module"",""name"":""Int_ShieldCellBank_Size2_Class4"",""cost"":22619,""sku"":null},""128064310"":{""id"":128064310,""category"":""module"",""name"":""Int_ShieldCellBank_Size3_Class3"",""cost"":25333,""sku"":null},""128064305"":{""id"":128064305,""category"":""module"",""name"":""Int_ShieldCellBank_Size2_Class3"",""cost"":9048,""sku"":null},""128064300"":{""id"":128064300,""category"":""module"",""name"":""Int_ShieldCellBank_Size1_Class3"",""cost"":3231,""sku"":null},""128064309"":{""id"":128064309,""category"":""module"",""name"":""Int_ShieldCellBank_Size3_Class2"",""cost"":10133,""sku"":null},""128064299"":{""id"":128064299,""category"":""module"",""name"":""Int_ShieldCellBank_Size1_Class2"",""cost"":1293,""sku"":null},""128064304"":{""id"":128064304,""category"":""module"",""name"":""Int_ShieldCellBank_Size2_Class2"",""cost"":3619,""sku"":null},""128064308"":{""id"":128064308,""category"":""module"",""name"":""Int_ShieldCellBank_Size3_Class1"",""cost"":4053,""sku"":null},""128064303"":{""id"":128064303,""category"":""module"",""name"":""Int_ShieldCellBank_Size2_Class1"",""cost"":1448,""sku"":null},""128064298"":{""id"":128064298,""category"":""module"",""name"":""Int_ShieldCellBank_Size1_Class1"",""cost"":517,""sku"":null},""128666723"":{""id"":128666723,""category"":""module"",""name"":""Int_FSDInterdictor_Size4_Class5"",""cost"":21337344,""sku"":null},""128666719"":{""id"":128666719,""category"":""module"",""name"":""Int_FSDInterdictor_Size4_Class4"",""cost"":7112448,""sku"":null},""128666715"":{""id"":128666715,""category"":""module"",""name"":""Int_FSDInterdictor_Size4_Class3"",""cost"":2370816,""sku"":null},""128666722"":{""id"":128666722,""category"":""module"",""name"":""Int_FSDInterdictor_Size3_Class5"",""cost"":7620480,""sku"":null},""128666718"":{""id"":128666718,""category"":""module"",""name"":""Int_FSDInterdictor_Size3_Class4"",""cost"":2540160,""sku"":null},""128666714"":{""id"":128666714,""category"":""module"",""name"":""Int_FSDInterdictor_Size3_Class3"",""cost"":846720,""sku"":null},""128666711"":{""id"":128666711,""category"":""module"",""name"":""Int_FSDInterdictor_Size4_Class2"",""cost"":790272,""sku"":null},""128666707"":{""id"":128666707,""category"":""module"",""name"":""Int_FSDInterdictor_Size4_Class1"",""cost"":263424,""sku"":null},""128666710"":{""id"":128666710,""category"":""module"",""name"":""Int_FSDInterdictor_Size3_Class2"",""cost"":282240,""sku"":null},""128666706"":{""id"":128666706,""category"":""module"",""name"":""Int_FSDInterdictor_Size3_Class1"",""cost"":94080,""sku"":null},""128066540"":{""id"":128066540,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size3_Class4"",""cost"":43200,""sku"":null},""128066535"":{""id"":128066535,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size1_Class4"",""cost"":4800,""sku"":null},""128066539"":{""id"":128066539,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size3_Class3"",""cost"":21600,""sku"":null},""128066534"":{""id"":128066534,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size1_Class3"",""cost"":2400,""sku"":null},""128066538"":{""id"":128066538,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size3_Class2"",""cost"":10800,""sku"":null},""128066533"":{""id"":128066533,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size1_Class2"",""cost"":1200,""sku"":null},""128066537"":{""id"":128066537,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size3_Class1"",""cost"":5400,""sku"":null},""128666675"":{""id"":128666675,""category"":""module"",""name"":""Int_FuelScoop_Size8_Class4"",""cost"":72260660,""sku"":null},""128666682"":{""id"":128666682,""category"":""module"",""name"":""Int_FuelScoop_Size7_Class5"",""cost"":91180644,""sku"":null},""128666674"":{""id"":128666674,""category"":""module"",""name"":""Int_FuelScoop_Size7_Class4"",""cost"":22795161,""sku"":null},""128666667"":{""id"":128666667,""category"":""module"",""name"":""Int_FuelScoop_Size8_Class3"",""cost"":18065165,""sku"":null},""128666666"":{""id"":128666666,""category"":""module"",""name"":""Int_FuelScoop_Size7_Class3"",""cost"":5698790,""sku"":null},""128666659"":{""id"":128666659,""category"":""module"",""name"":""Int_FuelScoop_Size8_Class2"",""cost"":4516291,""sku"":null},""128666658"":{""id"":128666658,""category"":""module"",""name"":""Int_FuelScoop_Size7_Class2"",""cost"":1424698,""sku"":null},""128666651"":{""id"":128666651,""category"":""module"",""name"":""Int_FuelScoop_Size8_Class1"",""cost"":1083910,""sku"":null},""128666650"":{""id"":128666650,""category"":""module"",""name"":""Int_FuelScoop_Size7_Class1"",""cost"":341927,""sku"":null},""128667605"":{""id"":128667605,""category"":""module"",""name"":""Int_Repairer_Size8_Class1"",""cost"":612220,""sku"":null},""128667611"":{""id"":128667611,""category"":""module"",""name"":""Int_Repairer_Size6_Class2"",""cost"":566870,""sku"":null},""128667604"":{""id"":128667604,""category"":""module"",""name"":""Int_Repairer_Size7_Class1"",""cost"":340122,""sku"":null},""128667610"":{""id"":128667610,""category"":""module"",""name"":""Int_Repairer_Size5_Class2"",""cost"":314928,""sku"":null},""128667616"":{""id"":128667616,""category"":""module"",""name"":""Int_Repairer_Size3_Class3"",""cost"":291600,""sku"":null},""128667623"":{""id"":128667623,""category"":""module"",""name"":""Int_Repairer_Size2_Class4"",""cost"":486000,""sku"":null},""128667615"":{""id"":128667615,""category"":""module"",""name"":""Int_Repairer_Size2_Class3"",""cost"":162000,""sku"":null},""128667622"":{""id"":128667622,""category"":""module"",""name"":""Int_Repairer_Size1_Class4"",""cost"":270000,""sku"":null},""128667609"":{""id"":128667609,""category"":""module"",""name"":""Int_Repairer_Size4_Class2"",""cost"":174960,""sku"":null},""128667603"":{""id"":128667603,""category"":""module"",""name"":""Int_Repairer_Size6_Class1"",""cost"":188957,""sku"":null},""128671288"":{""id"":128671288,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size7_Class5"",""cost"":6998400,""sku"":null},""128671273"":{""id"":128671273,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size1_Class5"",""cost"":9600,""sku"":null},""128671282"":{""id"":128671282,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size5_Class4"",""cost"":388800,""sku"":null},""128671286"":{""id"":128671286,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size7_Class3"",""cost"":1749600,""sku"":null},""128671277"":{""id"":128671277,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size3_Class4"",""cost"":43200,""sku"":null},""128671285"":{""id"":128671285,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size7_Class2"",""cost"":874800,""sku"":null},""128064335"":{""id"":128064335,""category"":""module"",""name"":""Int_ShieldCellBank_Size8_Class3"",""cost"":4359903,""sku"":null},""128064330"":{""id"":128064330,""category"":""module"",""name"":""Int_ShieldCellBank_Size7_Class3"",""cost"":1557108,""sku"":null},""128064334"":{""id"":128064334,""category"":""module"",""name"":""Int_ShieldCellBank_Size8_Class2"",""cost"":1743961,""sku"":null},""128064329"":{""id"":128064329,""category"":""module"",""name"":""Int_ShieldCellBank_Size7_Class2"",""cost"":622843,""sku"":null},""128064333"":{""id"":128064333,""category"":""module"",""name"":""Int_ShieldCellBank_Size8_Class1"",""cost"":697584,""sku"":null},""128064328"":{""id"":128064328,""category"":""module"",""name"":""Int_ShieldCellBank_Size7_Class1"",""cost"":249137,""sku"":null},""128066549"":{""id"":128066549,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size7_Class3"",""cost"":1749600,""sku"":null},""128066548"":{""id"":128066548,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size7_Class2"",""cost"":874800,""sku"":null},""128066544"":{""id"":128066544,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size5_Class3"",""cost"":194400,""sku"":null},""128066547"":{""id"":128066547,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size7_Class1"",""cost"":437400,""sku"":null},""128066543"":{""id"":128066543,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size5_Class2"",""cost"":97200,""sku"":null},""128066542"":{""id"":128066542,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size5_Class1"",""cost"":48600,""sku"":null},""128064175"":{""id"":128064175,""category"":""module"",""name"":""Int_LifeSupport_Size8_Class3"",""cost"":4359903,""sku"":null},""128064170"":{""id"":128064170,""category"":""module"",""name"":""Int_LifeSupport_Size7_Class3"",""cost"":1557108,""sku"":null},""128064174"":{""id"":128064174,""category"":""module"",""name"":""Int_LifeSupport_Size8_Class2"",""cost"":1743961,""sku"":null},""128064169"":{""id"":128064169,""category"":""module"",""name"":""Int_LifeSupport_Size7_Class2"",""cost"":622843,""sku"":null},""128064173"":{""id"":128064173,""category"":""module"",""name"":""Int_LifeSupport_Size8_Class1"",""cost"":697584,""sku"":null},""128064168"":{""id"":128064168,""category"":""module"",""name"":""Int_LifeSupport_Size7_Class1"",""cost"":249137,""sku"":null},""128064060"":{""id"":128064060,""category"":""module"",""name"":""Int_Powerplant_Size7_Class3"",""cost"":5698790,""sku"":null},""128064063"":{""id"":128064063,""category"":""module"",""name"":""Int_Powerplant_Size8_Class1"",""cost"":2007241,""sku"":null},""128064058"":{""id"":128064058,""category"":""module"",""name"":""Int_Powerplant_Size7_Class1"",""cost"":633199,""sku"":null},""128064100"":{""id"":128064100,""category"":""module"",""name"":""Int_Engine_Size8_Class3"",""cost"":18065165,""sku"":null},""128064099"":{""id"":128064099,""category"":""module"",""name"":""Int_Engine_Size8_Class2"",""cost"":6021722,""sku"":null},""128064094"":{""id"":128064094,""category"":""module"",""name"":""Int_Engine_Size7_Class2"",""cost"":1899597,""sku"":null},""128064098"":{""id"":128064098,""category"":""module"",""name"":""Int_Engine_Size8_Class1"",""cost"":2007241,""sku"":null},""128064093"":{""id"":128064093,""category"":""module"",""name"":""Int_Engine_Size7_Class1"",""cost"":633199,""sku"":null},""128671244"":{""id"":128671244,""category"":""module"",""name"":""Int_DroneControl_Collection_Size7_Class1"",""cost"":437400,""sku"":null},""128671240"":{""id"":128671240,""category"":""module"",""name"":""Int_DroneControl_Collection_Size5_Class2"",""cost"":97200,""sku"":null},""128671236"":{""id"":128671236,""category"":""module"",""name"":""Int_DroneControl_Collection_Size3_Class3"",""cost"":21600,""sku"":null},""128671231"":{""id"":128671231,""category"":""module"",""name"":""Int_DroneControl_Collection_Size1_Class3"",""cost"":2400,""sku"":null},""128671239"":{""id"":128671239,""category"":""module"",""name"":""Int_DroneControl_Collection_Size5_Class1"",""cost"":48600,""sku"":null},""128671235"":{""id"":128671235,""category"":""module"",""name"":""Int_DroneControl_Collection_Size3_Class2"",""cost"":10800,""sku"":null},""128671230"":{""id"":128671230,""category"":""module"",""name"":""Int_DroneControl_Collection_Size1_Class2"",""cost"":1200,""sku"":null},""128671234"":{""id"":128671234,""category"":""module"",""name"":""Int_DroneControl_Collection_Size3_Class1"",""cost"":5400,""sku"":null},""128064135"":{""id"":128064135,""category"":""module"",""name"":""Int_Hyperdrive_Size8_Class3"",""cost"":18065165,""sku"":null},""128064130"":{""id"":128064130,""category"":""module"",""name"":""Int_Hyperdrive_Size7_Class3"",""cost"":5698790,""sku"":null},""128064129"":{""id"":128064129,""category"":""module"",""name"":""Int_Hyperdrive_Size7_Class2"",""cost"":1899597,""sku"":null},""128064133"":{""id"":128064133,""category"":""module"",""name"":""Int_Hyperdrive_Size8_Class1"",""cost"":2007241,""sku"":null},""128064128"":{""id"":128064128,""category"":""module"",""name"":""Int_Hyperdrive_Size7_Class1"",""cost"":633199,""sku"":null},""128064241"":{""id"":128064241,""category"":""module"",""name"":""Int_Sensors_Size5_Class4"",""cost"":496527,""sku"":null},""128064245"":{""id"":128064245,""category"":""module"",""name"":""Int_Sensors_Size6_Class3"",""cost"":556110,""sku"":null},""128064244"":{""id"":128064244,""category"":""module"",""name"":""Int_Sensors_Size6_Class2"",""cost"":222444,""sku"":null},""128064240"":{""id"":128064240,""category"":""module"",""name"":""Int_Sensors_Size5_Class3"",""cost"":198611,""sku"":null},""128064235"":{""id"":128064235,""category"":""module"",""name"":""Int_Sensors_Size4_Class3"",""cost"":70932,""sku"":null},""128064239"":{""id"":128064239,""category"":""module"",""name"":""Int_Sensors_Size5_Class2"",""cost"":79444,""sku"":null},""128064243"":{""id"":128064243,""category"":""module"",""name"":""Int_Sensors_Size6_Class1"",""cost"":88978,""sku"":null},""128064206"":{""id"":128064206,""category"":""module"",""name"":""Int_PowerDistributor_Size6_Class4"",""cost"":1390275,""sku"":null},""128064201"":{""id"":128064201,""category"":""module"",""name"":""Int_PowerDistributor_Size5_Class4"",""cost"":496527,""sku"":null},""128064196"":{""id"":128064196,""category"":""module"",""name"":""Int_PowerDistributor_Size4_Class4"",""cost"":177331,""sku"":null},""128064205"":{""id"":128064205,""category"":""module"",""name"":""Int_PowerDistributor_Size6_Class3"",""cost"":556110,""sku"":null},""128064204"":{""id"":128064204,""category"":""module"",""name"":""Int_PowerDistributor_Size6_Class2"",""cost"":222444,""sku"":null},""128064195"":{""id"":128064195,""category"":""module"",""name"":""Int_PowerDistributor_Size4_Class3"",""cost"":70932,""sku"":null},""128668546"":{""id"":128668546,""category"":""module"",""name"":""Int_HullReinforcement_Size5_Class2"",""cost"":450000,""sku"":null},""128668544"":{""id"":128668544,""category"":""module"",""name"":""Int_HullReinforcement_Size4_Class2"",""cost"":195000,""sku"":null},""128668540"":{""id"":128668540,""category"":""module"",""name"":""Int_HullReinforcement_Size2_Class2"",""cost"":36000,""sku"":null},""128668538"":{""id"":128668538,""category"":""module"",""name"":""Int_HullReinforcement_Size1_Class2"",""cost"":15000,""sku"":null},""128666681"":{""id"":128666681,""category"":""module"",""name"":""Int_FuelScoop_Size6_Class5"",""cost"":28763610,""sku"":null},""128666673"":{""id"":128666673,""category"":""module"",""name"":""Int_FuelScoop_Size6_Class4"",""cost"":7190903,""sku"":null},""128666665"":{""id"":128666665,""category"":""module"",""name"":""Int_FuelScoop_Size6_Class3"",""cost"":1797726,""sku"":null},""128666672"":{""id"":128666672,""category"":""module"",""name"":""Int_FuelScoop_Size5_Class4"",""cost"":2268424,""sku"":null},""128666679"":{""id"":128666679,""category"":""module"",""name"":""Int_FuelScoop_Size4_Class5"",""cost"":2862364,""sku"":null},""128666671"":{""id"":128666671,""category"":""module"",""name"":""Int_FuelScoop_Size4_Class4"",""cost"":715591,""sku"":null},""128668545"":{""id"":128668545,""category"":""module"",""name"":""Int_HullReinforcement_Size5_Class1"",""cost"":150000,""sku"":null},""128668543"":{""id"":128668543,""category"":""module"",""name"":""Int_HullReinforcement_Size4_Class1"",""cost"":65000,""sku"":null},""128668541"":{""id"":128668541,""category"":""module"",""name"":""Int_HullReinforcement_Size3_Class1"",""cost"":28000,""sku"":null},""128668539"":{""id"":128668539,""category"":""module"",""name"":""Int_HullReinforcement_Size2_Class1"",""cost"":12000,""sku"":null},""128668537"":{""id"":128668537,""category"":""module"",""name"":""Int_HullReinforcement_Size1_Class1"",""cost"":5000,""sku"":null},""128064326"":{""id"":128064326,""category"":""module"",""name"":""Int_ShieldCellBank_Size6_Class4"",""cost"":1390275,""sku"":null},""128064321"":{""id"":128064321,""category"":""module"",""name"":""Int_ShieldCellBank_Size5_Class4"",""cost"":496527,""sku"":null},""128064325"":{""id"":128064325,""category"":""module"",""name"":""Int_ShieldCellBank_Size6_Class3"",""cost"":556110,""sku"":null},""128064324"":{""id"":128064324,""category"":""module"",""name"":""Int_ShieldCellBank_Size6_Class2"",""cost"":222444,""sku"":null},""128671243"":{""id"":128671243,""category"":""module"",""name"":""Int_DroneControl_Collection_Size5_Class5"",""cost"":777600,""sku"":null},""128671238"":{""id"":128671238,""category"":""module"",""name"":""Int_DroneControl_Collection_Size3_Class5"",""cost"":86400,""sku"":null},""128671233"":{""id"":128671233,""category"":""module"",""name"":""Int_DroneControl_Collection_Size1_Class5"",""cost"":9600,""sku"":null},""128671247"":{""id"":128671247,""category"":""module"",""name"":""Int_DroneControl_Collection_Size7_Class4"",""cost"":3499200,""sku"":null},""128668535"":{""id"":128668535,""category"":""module"",""name"":""Hpt_ShieldBooster_Size0_Class4"",""cost"":122000,""sku"":null},""128668533"":{""id"":128668533,""category"":""module"",""name"":""Hpt_ShieldBooster_Size0_Class2"",""cost"":23000,""sku"":null},""128668532"":{""id"":128668532,""category"":""module"",""name"":""Hpt_ShieldBooster_Size0_Class1"",""cost"":10000,""sku"":null},""128064166"":{""id"":128064166,""category"":""module"",""name"":""Int_LifeSupport_Size6_Class4"",""cost"":1390275,""sku"":null},""128064161"":{""id"":128064161,""category"":""module"",""name"":""Int_LifeSupport_Size5_Class4"",""cost"":496527,""sku"":null},""128064056"":{""id"":128064056,""category"":""module"",""name"":""Int_Powerplant_Size6_Class4"",""cost"":5393177,""sku"":null},""128064051"":{""id"":128064051,""category"":""module"",""name"":""Int_Powerplant_Size5_Class4"",""cost"":1701318,""sku"":null},""128682043"":{""id"":128682043,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Metallic"",""cost"":0,""sku"":null},""128672343"":{""id"":128672343,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Horizons_Polar"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1058""},""128672344"":{""id"":128672344,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Horizons_Desert"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1062""},""128672345"":{""id"":128672345,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Horizons_Lunar"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1063""},""128672355"":{""id"":128672355,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_PCGamer_PCGamer"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1069""},""128672417"":{""id"":128672417,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Yogscast_01"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1068""},""128667727"":{""id"":128667727,""category"":""paintjob"",""name"":""paintjob_CobraMkiii_Default_52"",""cost"":0,""sku"":null},""128066428"":{""id"":128066428,""category"":""paintjob"",""name"":""paintjob_cobramkiii_wireframe_01"",""cost"":0,""sku"":null},""128670861"":{""id"":128670861,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Onionhead1_01"",""cost"":0,""sku"":null},""128671133"":{""id"":128671133,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_green"",""cost"":0,""sku"":null},""128671134"":{""id"":128671134,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_blue"",""cost"":0,""sku"":null},""128671135"":{""id"":128671135,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_orange"",""cost"":0,""sku"":null},""128671136"":{""id"":128671136,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_red"",""cost"":0,""sku"":null},""128671137"":{""id"":128671137,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_purple"",""cost"":0,""sku"":null},""128671138"":{""id"":128671138,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_yellow"",""cost"":0,""sku"":null},""128667638"":{""id"":128667638,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Merc"",""cost"":0,""sku"":null},""128667639"":{""id"":128667639,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Merc"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_MERCENARY_PAINTJOB_1001""},""128672779"":{""id"":128672779,""category"":""paintjob"",""name"":""PaintJob_Eagle_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_EAGLE_1033""},""128681642"":{""id"":128681642,""category"":""paintjob"",""name"":""PaintJob_Eagle_Pax_South_Pax_South"",""cost"":0,""sku"":""FORC_FDEV_V_EAGLE_1034""},""128066405"":{""id"":128066405,""category"":""paintjob"",""name"":""paintjob_eagle_stripe1_02"",""cost"":0,""sku"":null},""128066406"":{""id"":128066406,""category"":""paintjob"",""name"":""paintjob_eagle_doublestripe_01"",""cost"":0,""sku"":null},""128066416"":{""id"":128066416,""category"":""paintjob"",""name"":""paintjob_eagle_thirds_01"",""cost"":0,""sku"":null},""128066419"":{""id"":128066419,""category"":""paintjob"",""name"":""paintjob_eagle_stripe1_03"",""cost"":0,""sku"":null},""128668019"":{""id"":128668019,""category"":""paintjob"",""name"":""PaintJob_Eagle_Crimson"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_MERCENARY_PAINTJOB_1003""},""128066420"":{""id"":128066420,""category"":""paintjob"",""name"":""paintjob_eagle_thirds_02"",""cost"":0,""sku"":null},""128066430"":{""id"":128066430,""category"":""paintjob"",""name"":""paintjob_eagle_stripe1_01"",""cost"":0,""sku"":null},""128066436"":{""id"":128066436,""category"":""paintjob"",""name"":""paintjob_eagle_camo_03"",""cost"":0,""sku"":null},""128066437"":{""id"":128066437,""category"":""paintjob"",""name"":""paintjob_eagle_thirds_03"",""cost"":0,""sku"":null},""128066441"":{""id"":128066441,""category"":""paintjob"",""name"":""paintjob_eagle_camo_02"",""cost"":0,""sku"":null},""128066449"":{""id"":128066449,""category"":""paintjob"",""name"":""paintjob_eagle_doublestripe_02"",""cost"":0,""sku"":null},""128066453"":{""id"":128066453,""category"":""paintjob"",""name"":""paintjob_eagle_doublestripe_03"",""cost"":0,""sku"":null},""128066456"":{""id"":128066456,""category"":""paintjob"",""name"":""paintjob_eagle_camo_01"",""cost"":0,""sku"":null},""128671139"":{""id"":128671139,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_green"",""cost"":0,""sku"":null},""128671140"":{""id"":128671140,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_blue"",""cost"":0,""sku"":null},""128671141"":{""id"":128671141,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_orange"",""cost"":0,""sku"":null},""128671142"":{""id"":128671142,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_red"",""cost"":0,""sku"":null},""128671143"":{""id"":128671143,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_purple"",""cost"":0,""sku"":null},""128671144"":{""id"":128671144,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_yellow"",""cost"":0,""sku"":null},""128671777"":{""id"":128671777,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Desert_Sand"",""cost"":0,""sku"":null},""128671778"":{""id"":128671778,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Earth_Yellow"",""cost"":0,""sku"":null},""128672802"":{""id"":128672802,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_BlackFriday_01"",""cost"":0,""sku"":null},""128671779"":{""id"":128671779,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Dark_Green"",""cost"":0,""sku"":null},""128671780"":{""id"":128671780,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Forest_Green"",""cost"":0,""sku"":null},""128671781"":{""id"":128671781,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Sand"",""cost"":0,""sku"":null},""128671782"":{""id"":128671782,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Earth_Red"",""cost"":0,""sku"":null},""128672426"":{""id"":128672426,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_SpecialEffect_01"",""cost"":0,""sku"":null},""128066404"":{""id"":128066404,""category"":""paintjob"",""name"":""paintjob_sidewinder_default_02"",""cost"":0,""sku"":null},""128066408"":{""id"":128066408,""category"":""paintjob"",""name"":""paintjob_sidewinder_default_03"",""cost"":0,""sku"":null},""128066414"":{""id"":128066414,""category"":""paintjob"",""name"":""paintjob_sidewinder_doublestripe_08"",""cost"":0,""sku"":null},""128066423"":{""id"":128066423,""category"":""paintjob"",""name"":""paintjob_sidewinder_doublestripe_05"",""cost"":0,""sku"":null},""128066431"":{""id"":128066431,""category"":""paintjob"",""name"":""paintjob_sidewinder_thirds_07"",""cost"":0,""sku"":null},""128066432"":{""id"":128066432,""category"":""paintjob"",""name"":""paintjob_sidewinder_thirds_01"",""cost"":0,""sku"":null},""128066433"":{""id"":128066433,""category"":""paintjob"",""name"":""paintjob_sidewinder_doublestripe_07"",""cost"":0,""sku"":null},""128066440"":{""id"":128066440,""category"":""paintjob"",""name"":""paintjob_sidewinder_camo_01"",""cost"":0,""sku"":null},""128066444"":{""id"":128066444,""category"":""paintjob"",""name"":""paintjob_sidewinder_thirds_06"",""cost"":0,""sku"":null},""128066447"":{""id"":128066447,""category"":""paintjob"",""name"":""paintjob_sidewinder_camo_03"",""cost"":0,""sku"":null},""128066448"":{""id"":128066448,""category"":""paintjob"",""name"":""paintjob_sidewinder_default_04"",""cost"":0,""sku"":null},""128066454"":{""id"":128066454,""category"":""paintjob"",""name"":""paintjob_sidewinder_camo_02"",""cost"":0,""sku"":null},""128671181"":{""id"":128671181,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_green"",""cost"":0,""sku"":null},""128671182"":{""id"":128671182,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_blue"",""cost"":0,""sku"":null},""128671183"":{""id"":128671183,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_orange"",""cost"":0,""sku"":null},""128671184"":{""id"":128671184,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_red"",""cost"":0,""sku"":null},""128671185"":{""id"":128671185,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_purple"",""cost"":0,""sku"":null},""128671186"":{""id"":128671186,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_yellow"",""cost"":0,""sku"":null},""128672796"":{""id"":128672796,""category"":""paintjob"",""name"":""PaintJob_Viper_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_VIPER_1049""},""128667667"":{""id"":128667667,""category"":""paintjob"",""name"":""PaintJob_Viper_Merc"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_MERCENARY_PAINTJOB_1002""},""128666726"":{""id"":128666726,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Camo1_02"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1003""},""128066407"":{""id"":128066407,""category"":""paintjob"",""name"":""paintjob_viper_flag_switzerland_01"",""cost"":0,""sku"":null},""128666727"":{""id"":128666727,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Camo2_03"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1004""},""128666728"":{""id"":128666728,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Stripe1_02"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1005""},""128066409"":{""id"":128066409,""category"":""paintjob"",""name"":""paintjob_viper_flag_belgium_01"",""cost"":0,""sku"":null},""128666729"":{""id"":128666729,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Stripe1_03"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1006""},""128066410"":{""id"":128066410,""category"":""paintjob"",""name"":""paintjob_viper_flag_australia_01"",""cost"":0,""sku"":null},""128666730"":{""id"":128666730,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Stripe2_02"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1007""},""128066411"":{""id"":128066411,""category"":""paintjob"",""name"":""paintjob_viper_default_01"",""cost"":0,""sku"":null},""128666731"":{""id"":128666731,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Stripe2_03"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1008""},""128066412"":{""id"":128066412,""category"":""paintjob"",""name"":""paintjob_viper_stripe2_02"",""cost"":0,""sku"":null},""128066413"":{""id"":128066413,""category"":""paintjob"",""name"":""paintjob_viper_flag_austria_01"",""cost"":0,""sku"":null},""128066415"":{""id"":128066415,""category"":""paintjob"",""name"":""paintjob_viper_stripe1_01"",""cost"":0,""sku"":null},""128066417"":{""id"":128066417,""category"":""paintjob"",""name"":""paintjob_viper_flag_spain_01"",""cost"":0,""sku"":null},""128066418"":{""id"":128066418,""category"":""paintjob"",""name"":""paintjob_viper_stripe1_02"",""cost"":0,""sku"":null},""128066421"":{""id"":128066421,""category"":""paintjob"",""name"":""paintjob_viper_flag_denmark_01"",""cost"":0,""sku"":null},""128066422"":{""id"":128066422,""category"":""paintjob"",""name"":""paintjob_viper_police_federation_01"",""cost"":0,""sku"":null},""128666742"":{""id"":128666742,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Hotrod_01"",""cost"":0,""sku"":null},""128666743"":{""id"":128666743,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Hotrod_03"",""cost"":0,""sku"":null},""128066424"":{""id"":128066424,""category"":""paintjob"",""name"":""paintjob_viper_flag_newzealand_01"",""cost"":0,""sku"":null},""128066425"":{""id"":128066425,""category"":""paintjob"",""name"":""paintjob_viper_flag_italy_01"",""cost"":0,""sku"":null},""128066426"":{""id"":128066426,""category"":""paintjob"",""name"":""paintjob_viper_stripe2_04"",""cost"":0,""sku"":null},""128066427"":{""id"":128066427,""category"":""paintjob"",""name"":""paintjob_viper_police_independent_01"",""cost"":0,""sku"":null},""128066429"":{""id"":128066429,""category"":""paintjob"",""name"":""paintjob_viper_default_03"",""cost"":0,""sku"":null},""128066434"":{""id"":128066434,""category"":""paintjob"",""name"":""paintjob_viper_flag_uk_01"",""cost"":0,""sku"":null},""128066435"":{""id"":128066435,""category"":""paintjob"",""name"":""paintjob_viper_flag_germany_01"",""cost"":0,""sku"":null},""128066438"":{""id"":128066438,""category"":""paintjob"",""name"":""paintjob_viper_flag_netherlands_01"",""cost"":0,""sku"":null},""128066439"":{""id"":128066439,""category"":""paintjob"",""name"":""paintjob_viper_flag_usa_01"",""cost"":0,""sku"":null},""128066442"":{""id"":128066442,""category"":""paintjob"",""name"":""paintjob_viper_flag_russia_01"",""cost"":0,""sku"":null},""128066443"":{""id"":128066443,""category"":""paintjob"",""name"":""paintjob_viper_flag_canada_01"",""cost"":0,""sku"":null},""128066445"":{""id"":128066445,""category"":""paintjob"",""name"":""paintjob_viper_flag_sweden_01"",""cost"":0,""sku"":null},""128066446"":{""id"":128066446,""category"":""paintjob"",""name"":""paintjob_viper_flag_poland_01"",""cost"":0,""sku"":null},""128066450"":{""id"":128066450,""category"":""paintjob"",""name"":""paintjob_viper_flag_finland_01"",""cost"":0,""sku"":null},""128066451"":{""id"":128066451,""category"":""paintjob"",""name"":""paintjob_viper_flag_france_01"",""cost"":0,""sku"":null},""128066452"":{""id"":128066452,""category"":""paintjob"",""name"":""paintjob_viper_police_empire_01"",""cost"":0,""sku"":null},""128066455"":{""id"":128066455,""category"":""paintjob"",""name"":""paintjob_viper_flag_norway_01"",""cost"":0,""sku"":null},""128671205"":{""id"":128671205,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_green"",""cost"":0,""sku"":null},""128671206"":{""id"":128671206,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_blue"",""cost"":0,""sku"":null},""128671207"":{""id"":128671207,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_orange"",""cost"":0,""sku"":null},""128671208"":{""id"":128671208,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_red"",""cost"":0,""sku"":null},""128671209"":{""id"":128671209,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_purple"",""cost"":0,""sku"":null},""128671210"":{""id"":128671210,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_yellow"",""cost"":0,""sku"":null},""128672806"":{""id"":128672806,""category"":""paintjob"",""name"":""PaintJob_Asp_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_ASP_1043""},""128672419"":{""id"":128672419,""category"":""paintjob"",""name"":""PaintJob_Asp_Metallic_Gold"",""cost"":0,""sku"":""FORC_FDEV_V_ASP_1038""},""128671127"":{""id"":128671127,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_green"",""cost"":0,""sku"":null},""128671128"":{""id"":128671128,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_blue"",""cost"":0,""sku"":null},""128671129"":{""id"":128671129,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_orange"",""cost"":0,""sku"":null},""128671130"":{""id"":128671130,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_red"",""cost"":0,""sku"":null},""128671131"":{""id"":128671131,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_purple"",""cost"":0,""sku"":null},""128671132"":{""id"":128671132,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_yellow"",""cost"":0,""sku"":null},""128672783"":{""id"":128672783,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1076""},""128672805"":{""id"":128672805,""category"":""paintjob"",""name"":""PaintJob_FedDropship_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_FEDDROP_1019""},""128671151"":{""id"":128671151,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_green"",""cost"":0,""sku"":null},""128671152"":{""id"":128671152,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_blue"",""cost"":0,""sku"":null},""128671153"":{""id"":128671153,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_orange"",""cost"":0,""sku"":null},""128671154"":{""id"":128671154,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_red"",""cost"":0,""sku"":null},""128671155"":{""id"":128671155,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_purple"",""cost"":0,""sku"":null},""128671156"":{""id"":128671156,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_yellow"",""cost"":0,""sku"":null},""128672788"":{""id"":128672788,""category"":""paintjob"",""name"":""PaintJob_Python_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_PYTHON_1020""},""128671175"":{""id"":128671175,""category"":""paintjob"",""name"":""paintjob_python_vibrant_green"",""cost"":0,""sku"":null},""128671176"":{""id"":128671176,""category"":""paintjob"",""name"":""paintjob_python_vibrant_blue"",""cost"":0,""sku"":null},""128671177"":{""id"":128671177,""category"":""paintjob"",""name"":""paintjob_python_vibrant_orange"",""cost"":0,""sku"":null},""128671178"":{""id"":128671178,""category"":""paintjob"",""name"":""paintjob_python_vibrant_red"",""cost"":0,""sku"":null},""128671179"":{""id"":128671179,""category"":""paintjob"",""name"":""paintjob_python_vibrant_purple"",""cost"":0,""sku"":null},""128671180"":{""id"":128671180,""category"":""paintjob"",""name"":""paintjob_python_vibrant_yellow"",""cost"":0,""sku"":null},""128672807"":{""id"":128672807,""category"":""paintjob"",""name"":""PaintJob_Adder_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_ADDER_1019""},""128671121"":{""id"":128671121,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_green"",""cost"":0,""sku"":null},""128671122"":{""id"":128671122,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_blue"",""cost"":0,""sku"":null},""128671123"":{""id"":128671123,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_orange"",""cost"":0,""sku"":null},""128671124"":{""id"":128671124,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_red"",""cost"":0,""sku"":null},""128671125"":{""id"":128671125,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_purple"",""cost"":0,""sku"":null},""128671126"":{""id"":128671126,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_yellow"",""cost"":0,""sku"":null},""128671145"":{""id"":128671145,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_green"",""cost"":0,""sku"":null},""128671146"":{""id"":128671146,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_blue"",""cost"":0,""sku"":null},""128671147"":{""id"":128671147,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_orange"",""cost"":0,""sku"":null},""128671148"":{""id"":128671148,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_red"",""cost"":0,""sku"":null},""128671149"":{""id"":128671149,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_purple"",""cost"":0,""sku"":null},""128671150"":{""id"":128671150,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_yellow"",""cost"":0,""sku"":null},""128671749"":{""id"":128671749,""category"":""paintjob"",""name"":""PaintJob_FerDeLance_Militaire_desert_Sand"",""cost"":0,""sku"":null},""128672795"":{""id"":128672795,""category"":""paintjob"",""name"":""PaintJob_FerDeLance_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_FERDELANCE_1019""},""128671157"":{""id"":128671157,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_green"",""cost"":0,""sku"":null},""128671158"":{""id"":128671158,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_blue"",""cost"":0,""sku"":null},""128671159"":{""id"":128671159,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_orange"",""cost"":0,""sku"":null},""128671160"":{""id"":128671160,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_red"",""cost"":0,""sku"":null},""128671161"":{""id"":128671161,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_purple"",""cost"":0,""sku"":null},""128671162"":{""id"":128671162,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_yellow"",""cost"":0,""sku"":null},""128672789"":{""id"":128672789,""category"":""paintjob"",""name"":""PaintJob_Hauler_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_HAULER_1024""},""128671163"":{""id"":128671163,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_green"",""cost"":0,""sku"":null},""128671164"":{""id"":128671164,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_blue"",""cost"":0,""sku"":null},""128671165"":{""id"":128671165,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_orange"",""cost"":0,""sku"":null},""128671166"":{""id"":128671166,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_red"",""cost"":0,""sku"":null},""128671167"":{""id"":128671167,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_purple"",""cost"":0,""sku"":null},""128671168"":{""id"":128671168,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_yellow"",""cost"":0,""sku"":null},""128672797"":{""id"":128672797,""category"":""paintjob"",""name"":""PaintJob_Orca_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_ORCA_1018""},""128671169"":{""id"":128671169,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_green"",""cost"":0,""sku"":null},""128671170"":{""id"":128671170,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_blue"",""cost"":0,""sku"":null},""128671171"":{""id"":128671171,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_orange"",""cost"":0,""sku"":null},""128671172"":{""id"":128671172,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_red"",""cost"":0,""sku"":null},""128671173"":{""id"":128671173,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_purple"",""cost"":0,""sku"":null},""128671174"":{""id"":128671174,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_yellow"",""cost"":0,""sku"":null},""128672800"":{""id"":128672800,""category"":""paintjob"",""name"":""PaintJob_Type6_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_TYPE6_1024""},""128671187"":{""id"":128671187,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_green"",""cost"":0,""sku"":null},""128671188"":{""id"":128671188,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_blue"",""cost"":0,""sku"":null},""128671189"":{""id"":128671189,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_orange"",""cost"":0,""sku"":null},""128671190"":{""id"":128671190,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_red"",""cost"":0,""sku"":null},""128671191"":{""id"":128671191,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_purple"",""cost"":0,""sku"":null},""128671192"":{""id"":128671192,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_yellow"",""cost"":0,""sku"":null},""128672799"":{""id"":128672799,""category"":""paintjob"",""name"":""PaintJob_Type7_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_TYPE7_1018""},""128671193"":{""id"":128671193,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_green"",""cost"":0,""sku"":null},""128671194"":{""id"":128671194,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_blue"",""cost"":0,""sku"":null},""128671195"":{""id"":128671195,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_orange"",""cost"":0,""sku"":null},""128671196"":{""id"":128671196,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_red"",""cost"":0,""sku"":null},""128671197"":{""id"":128671197,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_purple"",""cost"":0,""sku"":null},""128671198"":{""id"":128671198,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_yellow"",""cost"":0,""sku"":null},""128672793"":{""id"":128672793,""category"":""paintjob"",""name"":""PaintJob_Type9_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_TYPE9_1018""},""128671199"":{""id"":128671199,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_green"",""cost"":0,""sku"":null},""128671200"":{""id"":128671200,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_blue"",""cost"":0,""sku"":null},""128671201"":{""id"":128671201,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_orange"",""cost"":0,""sku"":null},""128671202"":{""id"":128671202,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_red"",""cost"":0,""sku"":null},""128671203"":{""id"":128671203,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_purple"",""cost"":0,""sku"":null},""128671204"":{""id"":128671204,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_yellow"",""cost"":0,""sku"":null},""128671211"":{""id"":128671211,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_green"",""cost"":0,""sku"":null},""128671212"":{""id"":128671212,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_blue"",""cost"":0,""sku"":null},""128671213"":{""id"":128671213,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_orange"",""cost"":0,""sku"":null},""128671214"":{""id"":128671214,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_red"",""cost"":0,""sku"":null},""128671215"":{""id"":128671215,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_purple"",""cost"":0,""sku"":null},""128671216"":{""id"":128671216,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_yellow"",""cost"":0,""sku"":null},""128672801"":{""id"":128672801,""category"":""paintjob"",""name"":""PaintJob_Vulture_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_VULTURE_1030""},""128672782"":{""id"":128672782,""category"":""paintjob"",""name"":""PaintJob_Anaconda_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_ANACONDA_1027""},""128672804"":{""id"":128672804,""category"":""paintjob"",""name"":""PaintJob_DiamondBack_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_DIAMOND_SCOUT_1018""},""128672784"":{""id"":128672784,""category"":""paintjob"",""name"":""PaintJob_DiamondBackXL_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_DIAMOND_EXPLORER_1020""},""128672786"":{""id"":128672786,""category"":""paintjob"",""name"":""PaintJob_Federation_Corvette_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_FEDERAL_CORVETTE_1000""},""128672781"":{""id"":128672781,""category"":""paintjob"",""name"":""PaintJob_Cutter_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_IMPERIAL_CUTTER_1000""},""128672792"":{""id"":128672792,""category"":""paintjob"",""name"":""PaintJob_Empire_Courier_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_IMPERIAL_COURIER_1018""},""128672791"":{""id"":128672791,""category"":""paintjob"",""name"":""PaintJob_FedGunship_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_FEDERAL_GUNSHIP_1000""},""128672794"":{""id"":128672794,""category"":""paintjob"",""name"":""PaintJob_FedDropshipMkII_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_FEDERAL_ASSAULT_1000""},""128672778"":{""id"":128672778,""category"":""paintjob"",""name"":""PaintJob_Empire_Eagle_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_IMPERIAL_EAGLE_1000""},""128672780"":{""id"":128672780,""category"":""paintjob"",""name"":""PaintJob_ViperMkIV_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_VIPER_1050""},""128672790"":{""id"":128672790,""category"":""paintjob"",""name"":""PaintJob_CobraMkIV_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1075""},""128672785"":{""id"":128672785,""category"":""paintjob"",""name"":""PaintJob_Independant_Trader_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_KEELBACK_1000""},""128672803"":{""id"":128672803,""category"":""paintjob"",""name"":""PaintJob_Asp_Scout_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_ASP_SCOUT_1000""},""128672798"":{""id"":128672798,""category"":""paintjob"",""name"":""PaintJob_EmpireTrader_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_CLIPPER_1019""},""128672246"":{""id"":128672246,""category"":""decal"",""name"":""Decal_PaxPrime"",""cost"":0,""sku"":""FORC_FDEV_V_DECAL_1036""},""128667650"":{""id"":128667650,""category"":""decal"",""name"":""Decal_Planet2"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_BACKERS_DECAL_1000""},""128667655"":{""id"":128667655,""category"":""decal"",""name"":""Decal_Skull3"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_MERCENARY_DECAL_1000""},""128667744"":{""id"":128667744,""category"":""decal"",""name"":""Decal_Trade_Mostly_Penniless"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1001""},""128667745"":{""id"":128667745,""category"":""decal"",""name"":""Decal_Trade_Peddler"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1002""},""128667746"":{""id"":128667746,""category"":""decal"",""name"":""Decal_Trade_Dealer"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1003""},""128667747"":{""id"":128667747,""category"":""decal"",""name"":""Decal_Trade_Merchant"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1004""},""128667748"":{""id"":128667748,""category"":""decal"",""name"":""Decal_Trade_Broker"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1005""},""128667752"":{""id"":128667752,""category"":""decal"",""name"":""Decal_Explorer_Mostly_Aimless"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_EXPLORE_DECAL_1001""},""128667753"":{""id"":128667753,""category"":""decal"",""name"":""Decal_Explorer_Scout"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_EXPLORE_DECAL_1002""},""128667754"":{""id"":128667754,""category"":""decal"",""name"":""Decal_Explorer_Surveyor"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_EXPLORE_DECAL_1003""},""128668553"":{""id"":128668553,""category"":""decal"",""name"":""Decal_Onionhead1"",""cost"":0,""sku"":""FORC_FDEV_V_DECAL_1030""},""128668554"":{""id"":128668554,""category"":""decal"",""name"":""Decal_Onionhead2"",""cost"":0,""sku"":""FORC_FDEV_V_DECAL_1032""},""128668555"":{""id"":128668555,""category"":""decal"",""name"":""Decal_Onionhead3"",""cost"":0,""sku"":""FORC_FDEV_V_DECAL_1032""},""128672768"":{""id"":128672768,""category"":""bobblehead"",""name"":""Bobble_TextPlus"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672769"":{""id"":128672769,""category"":""bobblehead"",""name"":""Bobble_TextBracket01"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672770"":{""id"":128672770,""category"":""bobblehead"",""name"":""Bobble_TextBracket02"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672771"":{""id"":128672771,""category"":""bobblehead"",""name"":""Bobble_TextUnderscore"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672772"":{""id"":128672772,""category"":""bobblehead"",""name"":""Bobble_TextMinus"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672773"":{""id"":128672773,""category"":""bobblehead"",""name"":""Bobble_TextPercent"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672774"":{""id"":128672774,""category"":""bobblehead"",""name"":""Bobble_TextEquals"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672284"":{""id"":128672284,""category"":""bobblehead"",""name"":""Bobble_ChristmasTree"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1008""},""128672808"":{""id"":128672808,""category"":""bobblehead"",""name"":""Bobble_TextN01"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672702"":{""id"":128672702,""category"":""bobblehead"",""name"":""Bobble_TextA"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672703"":{""id"":128672703,""category"":""bobblehead"",""name"":""Bobble_TextB"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672704"":{""id"":128672704,""category"":""bobblehead"",""name"":""Bobble_TextC"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672705"":{""id"":128672705,""category"":""bobblehead"",""name"":""Bobble_TextD"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672706"":{""id"":128672706,""category"":""bobblehead"",""name"":""Bobble_TextE"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672707"":{""id"":128672707,""category"":""bobblehead"",""name"":""Bobble_TextF"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672708"":{""id"":128672708,""category"":""bobblehead"",""name"":""Bobble_TextG"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672709"":{""id"":128672709,""category"":""bobblehead"",""name"":""Bobble_TextH"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672710"":{""id"":128672710,""category"":""bobblehead"",""name"":""Bobble_TextI"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672711"":{""id"":128672711,""category"":""bobblehead"",""name"":""Bobble_TextJ"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672712"":{""id"":128672712,""category"":""bobblehead"",""name"":""Bobble_TextK"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672713"":{""id"":128672713,""category"":""bobblehead"",""name"":""Bobble_TextL"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672714"":{""id"":128672714,""category"":""bobblehead"",""name"":""Bobble_TextM"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672715"":{""id"":128672715,""category"":""bobblehead"",""name"":""Bobble_TextN"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672716"":{""id"":128672716,""category"":""bobblehead"",""name"":""Bobble_TextO"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672717"":{""id"":128672717,""category"":""bobblehead"",""name"":""Bobble_TextP"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672718"":{""id"":128672718,""category"":""bobblehead"",""name"":""Bobble_TextQ"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672719"":{""id"":128672719,""category"":""bobblehead"",""name"":""Bobble_TextR"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672720"":{""id"":128672720,""category"":""bobblehead"",""name"":""Bobble_TextS"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672721"":{""id"":128672721,""category"":""bobblehead"",""name"":""Bobble_TextT"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672722"":{""id"":128672722,""category"":""bobblehead"",""name"":""Bobble_TextU"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672723"":{""id"":128672723,""category"":""bobblehead"",""name"":""Bobble_TextV"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672724"":{""id"":128672724,""category"":""bobblehead"",""name"":""Bobble_TextW"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672725"":{""id"":128672725,""category"":""bobblehead"",""name"":""Bobble_TextX"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672726"":{""id"":128672726,""category"":""bobblehead"",""name"":""Bobble_TextY"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672727"":{""id"":128672727,""category"":""bobblehead"",""name"":""Bobble_TextZ"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672728"":{""id"":128672728,""category"":""bobblehead"",""name"":""Bobble_TextA01"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672729"":{""id"":128672729,""category"":""bobblehead"",""name"":""Bobble_TextA02"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672730"":{""id"":128672730,""category"":""bobblehead"",""name"":""Bobble_TextE01"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672731"":{""id"":128672731,""category"":""bobblehead"",""name"":""Bobble_TextE02"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672732"":{""id"":128672732,""category"":""bobblehead"",""name"":""Bobble_TextE03"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672733"":{""id"":128672733,""category"":""bobblehead"",""name"":""Bobble_TextE04"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672734"":{""id"":128672734,""category"":""bobblehead"",""name"":""Bobble_TextI01"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672735"":{""id"":128672735,""category"":""bobblehead"",""name"":""Bobble_TextI02"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672736"":{""id"":128672736,""category"":""bobblehead"",""name"":""Bobble_TextI03"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672737"":{""id"":128672737,""category"":""bobblehead"",""name"":""Bobble_TextO01"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672738"":{""id"":128672738,""category"":""bobblehead"",""name"":""Bobble_TextO02"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672739"":{""id"":128672739,""category"":""bobblehead"",""name"":""Bobble_TextO03"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672740"":{""id"":128672740,""category"":""bobblehead"",""name"":""Bobble_TextU01"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672741"":{""id"":128672741,""category"":""bobblehead"",""name"":""Bobble_TextU02"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672742"":{""id"":128672742,""category"":""bobblehead"",""name"":""Bobble_TextU03"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672743"":{""id"":128672743,""category"":""bobblehead"",""name"":""Bobble_Text0"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672744"":{""id"":128672744,""category"":""bobblehead"",""name"":""Bobble_Text1"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672745"":{""id"":128672745,""category"":""bobblehead"",""name"":""Bobble_Text2"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672746"":{""id"":128672746,""category"":""bobblehead"",""name"":""Bobble_Text3"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672747"":{""id"":128672747,""category"":""bobblehead"",""name"":""Bobble_Text4"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672748"":{""id"":128672748,""category"":""bobblehead"",""name"":""Bobble_Text5"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672749"":{""id"":128672749,""category"":""bobblehead"",""name"":""Bobble_Text6"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672750"":{""id"":128672750,""category"":""bobblehead"",""name"":""Bobble_Text7"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672751"":{""id"":128672751,""category"":""bobblehead"",""name"":""Bobble_Text8"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672752"":{""id"":128672752,""category"":""bobblehead"",""name"":""Bobble_Text9"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672753"":{""id"":128672753,""category"":""bobblehead"",""name"":""Bobble_TextQuest"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672754"":{""id"":128672754,""category"":""bobblehead"",""name"":""Bobble_TextQuest01"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672755"":{""id"":128672755,""category"":""bobblehead"",""name"":""Bobble_TextC01"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672756"":{""id"":128672756,""category"":""bobblehead"",""name"":""Bobble_TextS01"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672757"":{""id"":128672757,""category"":""bobblehead"",""name"":""Bobble_TextOE"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672758"":{""id"":128672758,""category"":""bobblehead"",""name"":""Bobble_TextHash"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672759"":{""id"":128672759,""category"":""bobblehead"",""name"":""Bobble_TextAt"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672760"":{""id"":128672760,""category"":""bobblehead"",""name"":""Bobble_TextExclam"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672761"":{""id"":128672761,""category"":""bobblehead"",""name"":""Bobble_TextQuote"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672762"":{""id"":128672762,""category"":""bobblehead"",""name"":""Bobble_TextPound"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672763"":{""id"":128672763,""category"":""bobblehead"",""name"":""Bobble_TextDollar"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672764"":{""id"":128672764,""category"":""bobblehead"",""name"":""Bobble_TextCaret"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672765"":{""id"":128672765,""category"":""bobblehead"",""name"":""Bobble_TextExclam01"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672766"":{""id"":128672766,""category"":""bobblehead"",""name"":""Bobble_TextAmper"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""},""128672767"":{""id"":128672767,""category"":""bobblehead"",""name"":""Bobble_TextAsterisk"",""cost"":0,""sku"":""FORC_FDEV_EDV_BHEAD_1018""}}},""ship"":{""name"":""CobraMkIII"",""modules"":{""MediumHardpoint1"":{""module"":{""id"":128049382,""name"":""Hpt_PulseLaser_Fixed_Medium"",""value"":17600,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":1,""hopper"":0}}},""MediumHardpoint2"":{""module"":{""id"":128049382,""name"":""Hpt_PulseLaser_Fixed_Medium"",""value"":17600,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":1,""hopper"":0}}},""SmallHardpoint1"":{""module"":{""id"":128049459,""name"":""Hpt_MultiCannon_Gimbal_Small"",""value"":14250,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":90,""hopper"":2100}}},""SmallHardpoint2"":{""module"":{""id"":128049459,""name"":""Hpt_MultiCannon_Gimbal_Small"",""value"":14250,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":90,""hopper"":2100}}},""TinyHardpoint1"":{""module"":{""id"":128662526,""name"":""Hpt_CloudScanner_Size0_Class2"",""value"":40633,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""TinyHardpoint2"":{""module"":{""id"":128049513,""name"":""Hpt_ChaffLauncher_Tiny"",""value"":8500,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":1,""hopper"":10}}},""Decal1"":{""module"":{""id"":128668554,""name"":""Decal_Onionhead2"",""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Decal2"":{""module"":{""id"":128668555,""name"":""Decal_Onionhead3"",""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Decal3"":{""module"":{""id"":128668555,""name"":""Decal_Onionhead3"",""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""PaintJob"":{""module"":{""id"":128670861,""name"":""PaintJob_CobraMkIII_Onionhead1_01"",""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Armour"":{""module"":{""id"":128049282,""name"":""CobraMkIII_Armour_Grade3"",""value"":267535,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""PowerPlant"":{""module"":{""id"":128064047,""name"":""Int_Powerplant_Size4_Class5"",""value"":1368568,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""MainEngines"":{""module"":{""id"":128064082,""name"":""Int_Engine_Size4_Class5"",""value"":1368568,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""FrameShiftDrive"":{""module"":{""id"":128064117,""name"":""Int_Hyperdrive_Size4_Class5"",""value"":1368568,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""LifeSupport"":{""module"":{""id"":128064152,""name"":""Int_LifeSupport_Size3_Class5"",""value"":158331,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""PowerDistributor"":{""module"":{""id"":128064192,""name"":""Int_PowerDistributor_Size3_Class5"",""value"":134582,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""Radar"":{""module"":{""id"":128064232,""name"":""Int_Sensors_Size3_Class5"",""value"":134582,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""FuelTank"":{""module"":{""name"":""Int_FuelTank_Size4_Class3"",""id"":128064349,""value"":21023,""unloaned"":21023,""free"":false,""health"":1000000,""on"":true,""priority"":1}},""Slot01_Size4"":{""module"":{""id"":128064341,""name"":""Int_CargoRack_Size4_Class1"",""value"":34328,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Slot02_Size4"":{""module"":{""id"":128666679,""name"":""Int_FuelScoop_Size4_Class5"",""value"":2862364,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""Slot03_Size4"":{""module"":{""id"":128064317,""name"":""Int_ShieldCellBank_Size4_Class5"",""value"":376829,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":1,""hopper"":3}}},""Slot04_Size2"":{""module"":{""id"":128049549,""name"":""Int_DockingComputer_Standard"",""value"":3825,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""Slot05_Size2"":{""module"":{""id"":128672289,""name"":""Int_BuggyBay_Size2_Class2"",""value"":18360,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""Slot06_Size2"":{""module"":{""id"":128666634,""name"":""Int_DetailedSurfaceScanner_Tiny"",""value"":212500,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""PlanetaryApproachSuite"":{""module"":{""name"":""Int_PlanetApproachSuite"",""id"":128672317,""value"":425,""unloaned"":425,""free"":false,""health"":1000000,""on"":true,""priority"":1}},""Bobble01"":[],""Bobble02"":[],""Bobble03"":{""module"":{""id"":128672758,""name"":""Bobble_TextHash"",""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Bobble04"":{""module"":{""id"":128672716,""name"":""Bobble_TextO"",""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Bobble05"":{""module"":{""id"":128672709,""name"":""Bobble_TextH"",""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Bobble06"":{""module"":{""id"":128672716,""name"":""Bobble_TextO"",""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Bobble07"":{""module"":{""id"":128672708,""name"":""Bobble_TextG"",""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Bobble08"":[],""Bobble09"":[],""Bobble10"":[],""ShipKitSpoiler"":[],""ShipKitWings"":[],""ShipKitTail"":[],""ShipKitBumper"":[]},""value"":{""hull"":174498,""modules"":8443221,""cargo"":0,""total"":8617719,""unloaned"":21448},""free"":false,""alive"":true,""health"":{""hull"":1000000,""shield"":0,""shieldup"":false,""integrity"":0,""paintwork"":0},""wear"":{""dirt"":0,""fade"":0,""tear"":0,""game"":0},""cockpitBreached"":false,""oxygenRemaining"":1500000,""fuel"":{""main"":{""capacity"":16,""level"":16},""reserve"":{""capacity"":0.49,""level"":0.49}},""cargo"":{""capacity"":16,""qty"":0,""items"":[],""lock"":309574046,""ts"":{""sec"":1465244951,""usec"":946000}},""passengers"":[],""refinery"":null,""id"":4},""ships"":[{""name"":""SideWinder"",""alive"":true,""station"":{""id"":3227236352,""name"":""Pascal Orbital""},""starsystem"":{""id"":""4408507107699"",""name"":""Sui Guei"",""systemaddress"":""4408507107699""},""id"":0},{""name"":""Eagle"",""alive"":true,""station"":{""id"":3226686976,""name"":""Crampton Port""},""starsystem"":{""id"":""4064909724011"",""name"":""Chemaku"",""systemaddress"":""4064909724011""},""id"":1},{""name"":""Hauler"",""alive"":true,""station"":{""id"":3223470848,""name"":""Hiyya Orbital""},""starsystem"":{""id"":""9467315627393"",""name"":""Arjung"",""systemaddress"":""9467315627393""},""id"":2},{""name"":""Type6"",""alive"":true,""station"":{""id"":3227236864,""name"":""Alcala Dock""},""starsystem"":{""id"":""4408507107699"",""name"":""Sui Guei"",""systemaddress"":""4408507107699""},""id"":3},{""name"":""CobraMkIII"",""alive"":true,""station"":{""id"":3227236864,""name"":""Alcala Dock""},""starsystem"":{""id"":""4408507107699"",""name"":""Sui Guei"",""systemaddress"":""4408507107699""},""id"":4}]}";
            JObject json = JObject.Parse(data);

            List<Ship> shipyard = FrontierApi.ShipyardFromJson(null, json);

            Assert.AreEqual(5, shipyard.Count);
        }

        [TestMethod]
        public void TestCommanderFromProfile3()
        {
            string data = @"{""commander"":{""id"":1,""name"":""TestForHardpointOrdering"",""credits"":2,""debt"":0,""currentShipId"":5,""alive"":true,""docked"":true,""rank"":{""combat"":6,""trade"":8,""explore"":4,""crime"":0,""service"":0,""empire"":14,""federation"":14,""power"":0,""cqc"":0}},""lastSystem"":{""id"":""5031721865930"",""name"":""Devane"",""faction"":""Empire""},""lastStarport"":{""id"":""3222794496"",""name"":""Davies City"",""faction"":""Empire"",""commodities"":[{""id"":""128049204"",""name"":""Explosives"",""cost_min"":369.39496956522,""cost_max"":742.14503043478,""cost_mean"":""261.00"",""homebuy"":""59"",""homesell"":""55"",""consumebuy"":""4"",""baseCreationQty"":73.6,""baseConsumptionQty"":0,""capacity"":172520,""buyPrice"":441,""sellPrice"":409,""meanPrice"":261,""demandBracket"":0,""stockBracket"":1,""creationQty"":43130,""consumptionQty"":0,""targetStock"":43130,""stock"":13962.5183575,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.1400"",""sec_illegal_min"":""1.21"",""sec_illegal_max"":""2.69"",""stolenmod"":""0.7500""},{""id"":""128049202"",""name"":""Hydrogen Fuel"",""cost_min"":133.38,""cost_max"":197.22,""cost_mean"":""110.00"",""homebuy"":""73"",""homesell"":""70"",""consumebuy"":""3"",""baseCreationQty"":200,""baseConsumptionQty"":200,""capacity"":1524432,""buyPrice"":145,""sellPrice"":139,""meanPrice"":110,""demandBracket"":0,""stockBracket"":1,""creationQty"":234398,""consumptionQty"":58762,""targetStock"":249088,""stock"":161907.5,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[""powerplay""],""categoryname"":""Chemicals"",""volumescale"":""1.3000"",""sec_illegal_min"":""1.13"",""sec_illegal_max"":""2.10"",""stolenmod"":""0.7500""},{""id"":""128673850"",""name"":""Hydrogen Peroxide"",""cost_min"":518.34413043478,""cost_max"":860.65586956522,""cost_mean"":""917.00"",""homebuy"":""70"",""homesell"":""67"",""consumebuy"":""3"",""baseCreationQty"":0,""baseConsumptionQty"":56.6,""capacity"":132670,""buyPrice"":0,""sellPrice"":861,""meanPrice"":917,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":66335,""targetStock"":16583,""stock"":0,""demand"":116087,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.2700"",""sec_illegal_min"":""1.14"",""sec_illegal_max"":""2.19"",""stolenmod"":""0.7500""},{""id"":""128673851"",""name"":""Liquid Oxygen"",""cost_min"":176.03934782609,""cost_max"":400.96065217391,""cost_mean"":""263.00"",""homebuy"":""51"",""homesell"":""46"",""consumebuy"":""5"",""baseCreationQty"":10.6,""baseConsumptionQty"":0,""capacity"":18636,""buyPrice"":200,""sellPrice"":179,""meanPrice"":263,""demandBracket"":0,""stockBracket"":1,""creationQty"":9318,""consumptionQty"":0,""targetStock"":9318,""stock"":5217,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.0700"",""sec_illegal_min"":""1.25"",""sec_illegal_max"":""3.04"",""stolenmod"":""0.7500""},{""id"":""128049203"",""name"":""Mineral Oil"",""cost_min"":264.36702608696,""cost_max"":660.54297391304,""cost_mean"":""181.00"",""homebuy"":""46"",""homesell"":""41"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":194.2,""capacity"":910420,""buyPrice"":0,""sellPrice"":661,""meanPrice"":181,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":227605,""targetStock"":56901,""stock"":0,""demand"":817955.5,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.0300"",""sec_illegal_min"":""1.21"",""sec_illegal_max"":""2.69"",""stolenmod"":""0.7500""},{""id"":""128049205"",""name"":""Pesticides"",""cost_min"":176.03934782609,""cost_max"":""354.00"",""cost_mean"":""241.00"",""homebuy"":""51"",""homesell"":""46"",""consumebuy"":""5"",""baseCreationQty"":421.6,""baseConsumptionQty"":0,""capacity"":247060,""buyPrice"":177,""sellPrice"":159,""meanPrice"":241,""demandBracket"":0,""stockBracket"":1,""creationQty"":123530,""consumptionQty"":0,""targetStock"":123530,""stock"":69176,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.0700"",""sec_illegal_min"":""1.25"",""sec_illegal_max"":""3.04"",""stolenmod"":""0.7500""},{""id"":""128672305"",""name"":""Surface Stabilisers"",""cost_min"":552.28043478261,""cost_max"":939.31956521739,""cost_mean"":""467.00"",""homebuy"":""69"",""homesell"":""66"",""consumebuy"":""3"",""baseCreationQty"":41.8,""baseConsumptionQty"":0,""capacity"":146968,""buyPrice"":652,""sellPrice"":620,""meanPrice"":467,""demandBracket"":0,""stockBracket"":1,""creationQty"":36742,""consumptionQty"":0,""targetStock"":36742,""stock"":18371,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.2500"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128672303"",""name"":""Synthetic Reagents"",""cost_min"":""6561.00"",""cost_max"":7507.6260869565,""cost_mean"":""6675.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":2.4,""capacity"":5626,""buyPrice"":0,""sellPrice"":7508,""meanPrice"":6675,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":2813,""targetStock"":703,""stock"":0,""demand"":4923,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.02"",""sec_illegal_max"":""1.18"",""stolenmod"":""0.7500""},{""id"":""128049166"",""name"":""Water"",""cost_min"":""124.00"",""cost_max"":287.06956521739,""cost_mean"":""120.00"",""homebuy"":""18"",""homesell"":""10"",""consumebuy"":""8"",""baseCreationQty"":0,""baseConsumptionQty"":45.4,""capacity"":106418,""buyPrice"":0,""sellPrice"":241,""meanPrice"":120,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":53209,""targetStock"":13301,""stock"":0,""demand"":63186,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128049241"",""name"":""Clothing"",""cost_min"":""315.00"",""cost_max"":515.79739130435,""cost_mean"":""285.00"",""homebuy"":""60"",""homesell"":""56"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":350,""capacity"":205666,""buyPrice"":0,""sellPrice"":459,""meanPrice"":285,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":102833,""targetStock"":25707,""stock"":0,""demand"":122114.5,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Consumer Items"",""volumescale"":""1.1500"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.65"",""stolenmod"":""0.7500""},{""id"":""128049240"",""name"":""Consumer Technology"",""cost_min"":6875,""cost_max"":7719,""cost_mean"":""6769.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":4,""baseConsumptionQty"":5.4,""capacity"":7846,""buyPrice"":0,""sellPrice"":7688,""meanPrice"":6769,""demandBracket"":3,""stockBracket"":0,""creationQty"":1173,""consumptionQty"":1844,""targetStock"":1634,""stock"":0,""demand"":5721,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[""powerplay""],""categoryname"":""Consumer Items"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.11"",""stolenmod"":""0.7500""},{""id"":""128049238"",""name"":""Domestic Appliances"",""cost_min"":""527.00"",""cost_max"":782.76630434783,""cost_mean"":""487.00"",""homebuy"":""69"",""homesell"":""66"",""consumebuy"":""3"",""baseCreationQty"":0,""baseConsumptionQty"":209,""capacity"":71640,""buyPrice"":0,""sellPrice"":722,""meanPrice"":487,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":35820,""targetStock"":8955,""stock"":0,""demand"":44235,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Consumer Items"",""volumescale"":""1.2500"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128672314"",""name"":""Evacuation Shelter"",""cost_min"":262.20260869565,""cost_max"":""463.00"",""cost_mean"":""343.00"",""homebuy"":""60"",""homesell"":""56"",""consumebuy"":""4"",""baseCreationQty"":11.2,""baseConsumptionQty"":0,""capacity"":6564,""buyPrice"":225,""sellPrice"":209,""meanPrice"":343,""demandBracket"":0,""stockBracket"":2,""creationQty"":3282,""consumptionQty"":0,""targetStock"":3282,""stock"":3282,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Consumer Items"",""volumescale"":""1.1500"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.65"",""stolenmod"":""0.7500""},{""id"":""128049182"",""name"":""Animalmeat"",""cost_min"":1351,""cost_max"":1780,""cost_mean"":""1292.00"",""homebuy"":""80"",""homesell"":""78"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":97,""capacity"":52286,""buyPrice"":0,""sellPrice"":1659,""meanPrice"":1292,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":20110,""targetStock"":5027,""stock"":0,""demand"":31044,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[""powerplay""],""categoryname"":""Foods"",""volumescale"":""1.4000"",""sec_illegal_min"":""1.10"",""sec_illegal_max"":""1.81"",""stolenmod"":""0.7500""},{""id"":""128049189"",""name"":""Coffee"",""cost_min"":1351,""cost_max"":1780,""cost_mean"":""1279.00"",""homebuy"":""80"",""homesell"":""78"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":97,""capacity"":29394,""buyPrice"":0,""sellPrice"":1659,""meanPrice"":1279,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":11305,""targetStock"":2826,""stock"":0,""demand"":17453,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[""powerplay""],""categoryname"":""Foods"",""volumescale"":""1.4000"",""sec_illegal_min"":""1.10"",""sec_illegal_max"":""1.81"",""stolenmod"":""0.7500""},{""id"":""128049183"",""name"":""Fish"",""cost_min"":""403.00"",""cost_max"":627.93,""cost_mean"":""406.00"",""homebuy"":""64"",""homesell"":""60"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":271,""capacity"":134944,""buyPrice"":0,""sellPrice"":565,""meanPrice"":406,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":67472,""targetStock"":16867,""stock"":0,""demand"":80123.25,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.2000"",""sec_illegal_min"":""1.18"",""sec_illegal_max"":""2.44"",""stolenmod"":""0.7500""},{""id"":""128049184"",""name"":""Food Cartridges"",""cost_min"":""141.00"",""cost_max"":312.79826086957,""cost_mean"":""105.00"",""homebuy"":""30"",""homesell"":""23"",""consumebuy"":""7"",""baseCreationQty"":0,""baseConsumptionQty"":452,""capacity"":43206,""buyPrice"":0,""sellPrice"":264,""meanPrice"":105,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":21603,""targetStock"":5400,""stock"":0,""demand"":25653.75,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128049178"",""name"":""Fruit And Vegetables"",""cost_min"":""315.00"",""cost_max"":515.79739130435,""cost_mean"":""312.00"",""homebuy"":""60"",""homesell"":""56"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":350,""capacity"":81584,""buyPrice"":0,""sellPrice"":459,""meanPrice"":312,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":40792,""targetStock"":10197,""stock"":0,""demand"":48440.75,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.1500"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.65"",""stolenmod"":""0.7500""},{""id"":""128049180"",""name"":""Grain"",""cost_min"":""207.00"",""cost_max"":380.8852173913,""cost_mean"":""210.00"",""homebuy"":""48"",""homesell"":""43"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":584,""capacity"":343168,""buyPrice"":0,""sellPrice"":332,""meanPrice"":210,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":171584,""targetStock"":42895,""stock"":0,""demand"":203756.25,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.0500"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.65"",""stolenmod"":""0.7500""},{""id"":""128049185"",""name"":""Synthetic Meat"",""cost_min"":203.48347826087,""cost_max"":""388.00"",""cost_mean"":""271.00"",""homebuy"":""54"",""homesell"":""49"",""consumebuy"":""5"",""baseCreationQty"":36,""baseConsumptionQty"":45.2,""capacity"":94920,""buyPrice"":207,""sellPrice"":187,""meanPrice"":271,""demandBracket"":0,""stockBracket"":1,""creationQty"":42192,""consumptionQty"":5268,""targetStock"":43509,""stock"":25794.553285,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.23"",""sec_illegal_max"":""2.88"",""stolenmod"":""0.7500""},{""id"":""128049188"",""name"":""Tea"",""cost_min"":1532,""cost_max"":1992,""cost_mean"":""1467.00"",""homebuy"":""81"",""homesell"":""79"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":88,""capacity"":50532,""buyPrice"":0,""sellPrice"":1862,""meanPrice"":1467,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":19435,""targetStock"":4858,""stock"":0,""demand"":30004,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[""powerplay""],""categoryname"":""Foods"",""volumescale"":""1.4200"",""sec_illegal_min"":""1.09"",""sec_illegal_max"":""1.76"",""stolenmod"":""0.7500""},{""id"":""128673856"",""name"":""C M M Composite"",""cost_min"":""2966.00"",""cost_max"":3605.947826087,""cost_mean"":""3132.00"",""homebuy"":""87"",""homesell"":""86"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":80,""capacity"":187522,""buyPrice"":0,""sellPrice"":3606,""meanPrice"":3132,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":93761,""targetStock"":23440,""stock"":0,""demand"":164082,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.3400"",""sec_illegal_min"":""1.05"",""sec_illegal_max"":""1.37"",""stolenmod"":""0.7500""},{""id"":""128672302"",""name"":""Ceramic Composites"",""cost_min"":503.04,""cost_max"":956.14507826087,""cost_mean"":""232.00"",""homebuy"":""46"",""homesell"":""41"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":52,""capacity"":243780,""buyPrice"":0,""sellPrice"":957,""meanPrice"":232,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":60945,""targetStock"":15236,""stock"":0,""demand"":236162,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.0300"",""sec_illegal_min"":""1.28"",""sec_illegal_max"":""3.28"",""stolenmod"":""0.7500""},{""id"":""128673855"",""name"":""Insulating Membrane"",""cost_min"":""7498.00"",""cost_max"":8450.2097826087,""cost_mean"":""7837.00"",""homebuy"":""93"",""homesell"":""92"",""consumebuy"":""1"",""baseCreationQty"":0.4,""baseConsumptionQty"":39.2,""capacity"":92590,""buyPrice"":0,""sellPrice"":8451,""meanPrice"":7837,""demandBracket"":3,""stockBracket"":0,""creationQty"":352,""consumptionQty"":45943,""targetStock"":11837,""stock"":0,""demand"":80528,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.0600"",""sec_illegal_min"":""1.02"",""sec_illegal_max"":""1.15"",""stolenmod"":""0.7500""},{""id"":""128672701"",""name"":""Meta Alloys"",""cost_min"":""75936.00"",""cost_max"":87425.295652174,""cost_mean"":""88148.00"",""homebuy"":""97"",""homesell"":""97"",""consumebuy"":""0"",""baseCreationQty"":0,""baseConsumptionQty"":15.2,""capacity"":35630,""buyPrice"":0,""sellPrice"":87426,""meanPrice"":88148,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":17815,""targetStock"":4453,""stock"":0,""demand"":31177,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.00"",""sec_illegal_max"":""1.03"",""stolenmod"":""0.7500""},{""id"":""128049197"",""name"":""Polymers"",""cost_min"":186.43116521739,""cost_max"":558.28883478261,""cost_mean"":""171.00"",""homebuy"":""35"",""homesell"":""29"",""consumebuy"":""7"",""baseCreationQty"":195,""baseConsumptionQty"":0,""capacity"":685616,""buyPrice"":197,""sellPrice"":162,""meanPrice"":171,""demandBracket"":0,""stockBracket"":1,""creationQty"":171404,""consumptionQty"":0,""targetStock"":171404,""stock"":47992.5,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.23"",""sec_illegal_max"":""2.88"",""stolenmod"":""0.7500""},{""id"":""128049199"",""name"":""Semiconductors"",""cost_min"":871.35,""cost_max"":1492.95,""cost_mean"":""967.00"",""homebuy"":""76"",""homesell"":""74"",""consumebuy"":""2"",""baseCreationQty"":26.4,""baseConsumptionQty"":0,""capacity"":120672,""buyPrice"":1141,""sellPrice"":1105,""meanPrice"":967,""demandBracket"":0,""stockBracket"":1,""creationQty"":23206,""consumptionQty"":0,""targetStock"":23206,""stock"":8446.5,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[""powerplay""],""categoryname"":""Industrial Materials"",""volumescale"":""1.3400"",""sec_illegal_min"":""1.12"",""sec_illegal_max"":""1.98"",""stolenmod"":""0.7500""},{""id"":""128049200"",""name"":""Superconductors"",""cost_min"":6626.61,""cost_max"":7582.7023478261,""cost_mean"":""6609.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":53.8,""baseConsumptionQty"":108,""capacity"":695468,""buyPrice"":0,""sellPrice"":7583,""meanPrice"":6609,""demandBracket"":3,""stockBracket"":0,""creationQty"":47290,""consumptionQty"":126577,""targetStock"":78934,""stock"":0,""demand"":645732.5,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.02"",""sec_illegal_max"":""1.18"",""stolenmod"":""0.7500""},{""id"":""128673860"",""name"":""H N Shock Mount"",""cost_min"":""499.00"",""cost_max"":748,""cost_mean"":""406.00"",""homebuy"":""68"",""homesell"":""65"",""consumebuy"":""3"",""baseCreationQty"":0,""baseConsumptionQty"":88,""capacity"":154708,""buyPrice"":0,""sellPrice"":678,""meanPrice"":406,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":77354,""targetStock"":19338,""stock"":0,""demand"":91858,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.2400"",""sec_illegal_min"":""1.16"",""sec_illegal_max"":""2.30"",""stolenmod"":""0.7500""},{""id"":""128673868"",""name"":""Heatsink Interlink"",""cost_min"":""661.00"",""cost_max"":947.80913043478,""cost_mean"":""729.00"",""homebuy"":""72"",""homesell"":""69"",""consumebuy"":""3"",""baseCreationQty"":0,""baseConsumptionQty"":68,""capacity"":119548,""buyPrice"":0,""sellPrice"":948,""meanPrice"":729,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":59774,""targetStock"":14943,""stock"":0,""demand"":104605,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.2900"",""sec_illegal_min"":""1.14"",""sec_illegal_max"":""2.13"",""stolenmod"":""0.7500""},{""id"":""128049220"",""name"":""Heliostatic Furnaces"",""cost_min"":152.69739130435,""cost_max"":""327.00"",""cost_mean"":""236.00"",""homebuy"":""47"",""homesell"":""42"",""consumebuy"":""5"",""baseCreationQty"":48.8,""baseConsumptionQty"":61.4,""capacity"":229714,""buyPrice"":0,""sellPrice"":289,""meanPrice"":236,""demandBracket"":2,""stockBracket"":0,""creationQty"":42895,""consumptionQty"":71962,""targetStock"":60885,""stock"":0,""demand"":144615.8115,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.0400"",""sec_illegal_min"":""1.22"",""sec_illegal_max"":""2.78"",""stolenmod"":""0.7500""},{""id"":""128673869"",""name"":""Magnetic Emitter Coil"",""cost_min"":""164.00"",""cost_max"":332.85565217391,""cost_mean"":""199.00"",""homebuy"":""39"",""homesell"":""33"",""consumebuy"":""6"",""baseCreationQty"":0,""baseConsumptionQty"":334.4,""capacity"":587886,""buyPrice"":0,""sellPrice"":333,""meanPrice"":199,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":293943,""targetStock"":73485,""stock"":0,""demand"":514401,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.27"",""sec_illegal_max"":""3.21"",""stolenmod"":""0.7500""},{""id"":""128673862"",""name"":""Power Converter"",""cost_min"":""199.00"",""cost_max"":373.30260869565,""cost_mean"":""246.00"",""homebuy"":""47"",""homesell"":""42"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":245.6,""capacity"":431774,""buyPrice"":0,""sellPrice"":374,""meanPrice"":246,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":215887,""targetStock"":53971,""stock"":0,""demand"":377803,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.0400"",""sec_illegal_min"":""1.24"",""sec_illegal_max"":""2.98"",""stolenmod"":""0.7500""},{""id"":""128049217"",""name"":""Power Generators"",""cost_min"":632.4,""cost_max"":984.67956521739,""cost_mean"":""458.00"",""homebuy"":""69"",""homesell"":""66"",""consumebuy"":""3"",""baseCreationQty"":0,""baseConsumptionQty"":58.6,""capacity"":274720,""buyPrice"":0,""sellPrice"":985,""meanPrice"":458,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":68680,""targetStock"":17169,""stock"":0,""demand"":246818.875,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.2500"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128673863"",""name"":""Power Grid Assembly"",""cost_min"":""1555.00"",""cost_max"":2006.6026086957,""cost_mean"":""1684.00"",""homebuy"":""81"",""homesell"":""79"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":33.6,""capacity"":59070,""buyPrice"":0,""sellPrice"":2007,""meanPrice"":1684,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":29535,""targetStock"":7383,""stock"":0,""demand"":51687,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.4300"",""sec_illegal_min"":""1.09"",""sec_illegal_max"":""1.74"",""stolenmod"":""0.7500""},{""id"":""128049218"",""name"":""Water Purifiers"",""cost_min"":447,""cost_max"":742.14503043478,""cost_mean"":""258.00"",""homebuy"":""59"",""homesell"":""55"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":66.4,""capacity"":311288,""buyPrice"":0,""sellPrice"":743,""meanPrice"":258,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":77822,""targetStock"":19455,""stock"":0,""demand"":279672.875,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.1400"",""sec_illegal_min"":""1.21"",""sec_illegal_max"":""2.69"",""stolenmod"":""0.7500""},{""id"":""128682046"",""name"":""Advanced Medicines"",""cost_min"":1024.397826087,""cost_max"":""1422.00"",""cost_mean"":""1259.00"",""homebuy"":""78"",""homesell"":""76"",""consumebuy"":""2"",""baseCreationQty"":25.6,""baseConsumptionQty"":21.4,""capacity"":27578,""buyPrice"":1116,""sellPrice"":1081,""meanPrice"":1259,""demandBracket"":0,""stockBracket"":1,""creationQty"":7501,""consumptionQty"":6288,""targetStock"":9073,""stock"":5770,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Medicines"",""volumescale"":""1.3800"",""sec_illegal_min"":""1.10"",""sec_illegal_max"":""1.87"",""stolenmod"":""0.7500""},{""id"":""128049208"",""name"":""Agricultural Medicines"",""cost_min"":960,""cost_max"":1335,""cost_mean"":""1038.00"",""homebuy"":""77"",""homesell"":""75"",""consumebuy"":""2"",""baseCreationQty"":9.6,""baseConsumptionQty"":0,""capacity"":29256,""buyPrice"":1018,""sellPrice"":986,""meanPrice"":1038,""demandBracket"":0,""stockBracket"":1,""creationQty"":11252,""consumptionQty"":0,""targetStock"":11252,""stock"":8192,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[""powerplay""],""categoryname"":""Medicines"",""volumescale"":""1.3600"",""sec_illegal_min"":""1.11"",""sec_illegal_max"":""1.92"",""stolenmod"":""0.7500""},{""id"":""128049210"",""name"":""Basic Medicines"",""cost_min"":256.28260869565,""cost_max"":""463.00"",""cost_mean"":""279.00"",""homebuy"":""60"",""homesell"":""56"",""consumebuy"":""4"",""baseCreationQty"":56,""baseConsumptionQty"":98,""capacity"":41616,""buyPrice"":280,""sellPrice"":260,""meanPrice"":279,""demandBracket"":0,""stockBracket"":1,""creationQty"":16409,""consumptionQty"":4399,""targetStock"":17508,""stock"":10283,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Medicines"",""volumescale"":""1.1500"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.65"",""stolenmod"":""0.7500""},{""id"":""128049209"",""name"":""Performance Enhancers"",""cost_min"":6745,""cost_max"":7719,""cost_mean"":""6816.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":108,""baseConsumptionQty"":5.4,""capacity"":84828,""buyPrice"":7099,""sellPrice"":6984,""meanPrice"":6816,""demandBracket"":0,""stockBracket"":1,""creationQty"":31645,""consumptionQty"":981,""targetStock"":31890,""stock"":23351,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[""powerplay""],""categoryname"":""Medicines"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.26"",""stolenmod"":""0.7500""},{""id"":""128049669"",""name"":""Progenitor Cells"",""cost_min"":6787,""cost_max"":7719,""cost_mean"":""6779.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":8.8,""baseConsumptionQty"":5.4,""capacity"":9254,""buyPrice"":7141,""sellPrice"":7025,""meanPrice"":6779,""demandBracket"":0,""stockBracket"":1,""creationQty"":2579,""consumptionQty"":980,""targetStock"":2824,""stock"":2196,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[""powerplay""],""categoryname"":""Medicines"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.26"",""stolenmod"":""0.7500""},{""id"":""128049176"",""name"":""Aluminium"",""cost_min"":303.3532173913,""cost_max"":589.8467826087,""cost_mean"":""340.00"",""homebuy"":""61"",""homesell"":""57"",""consumebuy"":""4"",""baseCreationQty"":199.4,""baseConsumptionQty"":0,""capacity"":350542,""buyPrice"":353,""sellPrice"":328,""meanPrice"":340,""demandBracket"":0,""stockBracket"":1,""creationQty"":175271,""consumptionQty"":0,""targetStock"":175271,""stock"":98150,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1600"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.60"",""stolenmod"":""0.7500""},{""id"":""128049168"",""name"":""Beryllium"",""cost_min"":""8017.00"",""cost_max"":8987.1695652174,""cost_mean"":""8288.00"",""homebuy"":""93"",""homesell"":""92"",""consumebuy"":""1"",""baseCreationQty"":4.6,""baseConsumptionQty"":36.8,""capacity"":94348,""buyPrice"":0,""sellPrice"":8988,""meanPrice"":8288,""demandBracket"":3,""stockBracket"":0,""creationQty"":4044,""consumptionQty"":43130,""targetStock"":14826,""stock"":0,""demand"":74807,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.0400"",""sec_illegal_min"":""1.02"",""sec_illegal_max"":""1.15"",""stolenmod"":""0.7500""},{""id"":""128672300"",""name"":""Bismuth"",""cost_min"":2307.85,""cost_max"":2870.9386130435,""cost_mean"":""2284.00"",""homebuy"":""85"",""homesell"":""84"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":244.8,""capacity"":573808,""buyPrice"":0,""sellPrice"":2871,""meanPrice"":2284,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":286904,""targetStock"":71726,""stock"":0,""demand"":502082,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.4200"",""sec_illegal_min"":""1.07"",""sec_illegal_max"":""1.59"",""stolenmod"":""0.7500""},{""id"":""128049162"",""name"":""Cobalt"",""cost_min"":""701.00"",""cost_max"":997.2347826087,""cost_mean"":""647.00"",""homebuy"":""73"",""homesell"":""70"",""consumebuy"":""3"",""baseCreationQty"":324,""baseConsumptionQty"":1036.8,""capacity"":2999832,""buyPrice"":0,""sellPrice"":998,""meanPrice"":647,""demandBracket"":3,""stockBracket"":0,""creationQty"":284793,""consumptionQty"":1215123,""targetStock"":588573,""stock"":0,""demand"":2375165,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.3000"",""sec_illegal_min"":""1.13"",""sec_illegal_max"":""2.10"",""stolenmod"":""0.7500""},{""id"":""128049175"",""name"":""Copper"",""cost_min"":432.91782608696,""cost_max"":758.52217391304,""cost_mean"":""481.00"",""homebuy"":""67"",""homesell"":""64"",""consumebuy"":""3"",""baseCreationQty"":232,""baseConsumptionQty"":0,""capacity"":407852,""buyPrice"":500,""sellPrice"":475,""meanPrice"":481,""demandBracket"":0,""stockBracket"":1,""creationQty"":203926,""consumptionQty"":0,""targetStock"":203926,""stock"":114198,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.2300"",""sec_illegal_min"":""1.16"",""sec_illegal_max"":""2.33"",""stolenmod"":""0.7500""},{""id"":""128049170"",""name"":""Gallium"",""cost_min"":""5028.00"",""cost_max"":5863.447826087,""cost_mean"":""5135.00"",""homebuy"":""90"",""homesell"":""89"",""consumebuy"":""1"",""baseCreationQty"":66,""baseConsumptionQty"":264,""capacity"":734842,""buyPrice"":0,""sellPrice"":5864,""meanPrice"":5135,""demandBracket"":3,""stockBracket"":0,""creationQty"":58014,""consumptionQty"":309407,""targetStock"":135365,""stock"":0,""demand"":582148,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1800"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.24"",""stolenmod"":""0.7500""},{""id"":""128049154"",""name"":""Gold"",""cost_min"":""9164.00"",""cost_max"":10172.009782609,""cost_mean"":""9401.00"",""homebuy"":""94"",""homesell"":""93"",""consumebuy"":""1"",""baseCreationQty"":2,""baseConsumptionQty"":166.4,""capacity"":394728,""buyPrice"":0,""sellPrice"":10173,""meanPrice"":9401,""demandBracket"":3,""stockBracket"":0,""creationQty"":2344,""consumptionQty"":195020,""targetStock"":51099,""stock"":0,""demand"":340195,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.09"",""stolenmod"":""0.7500""},{""id"":""128049169"",""name"":""Indium"",""cost_min"":""5743.00"",""cost_max"":6617.4130434783,""cost_mean"":""5727.00"",""homebuy"":""91"",""homesell"":""90"",""consumebuy"":""1"",""baseCreationQty"":59.6,""baseConsumptionQty"":119.2,""capacity"":384180,""buyPrice"":0,""sellPrice"":6514,""meanPrice"":5727,""demandBracket"":2,""stockBracket"":0,""creationQty"":52388,""consumptionQty"":139702,""targetStock"":87313,""stock"":0,""demand"":262991,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1400"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.22"",""stolenmod"":""0.7500""},{""id"":""128672298"",""name"":""Lanthanum"",""cost_min"":""8571.00"",""cost_max"":9553.3097826087,""cost_mean"":""8766.00"",""homebuy"":""94"",""homesell"":""93"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":175.2,""capacity"":410668,""buyPrice"":0,""sellPrice"":9554,""meanPrice"":8766,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":205334,""targetStock"":51333,""stock"":0,""demand"":359335,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.0200"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.11"",""stolenmod"":""0.7500""},{""id"":""128049173"",""name"":""Lithium"",""cost_min"":1570.55,""cost_max"":2026.6686347826,""cost_mean"":""1596.00"",""homebuy"":""81"",""homesell"":""79"",""consumebuy"":""2"",""baseCreationQty"":16.6,""baseConsumptionQty"":665.6,""capacity"":1589342,""buyPrice"":0,""sellPrice"":2027,""meanPrice"":1596,""demandBracket"":3,""stockBracket"":0,""creationQty"":14592,""consumptionQty"":780079,""targetStock"":209611,""stock"":0,""demand"":1333046,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.4300"",""sec_illegal_min"":""1.09"",""sec_illegal_max"":""1.74"",""stolenmod"":""0.7500""},{""id"":""128671118"",""name"":""Osmium"",""cost_min"":""6561.00"",""cost_max"":7507.6260869565,""cost_mean"":""7591.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":215.2,""capacity"":504426,""buyPrice"":0,""sellPrice"":7508,""meanPrice"":7591,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":252213,""targetStock"":63053,""stock"":0,""demand"":441373,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.22"",""stolenmod"":""0.7500""},{""id"":""128049153"",""name"":""Palladium"",""cost_min"":""12815.00"",""cost_max"":13960.415652174,""cost_mean"":""13298.00"",""homebuy"":""96"",""homesell"":""96"",""consumebuy"":""0"",""baseCreationQty"":0,""baseConsumptionQty"":128.8,""capacity"":301906,""buyPrice"":0,""sellPrice"":13961,""meanPrice"":13298,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":150953,""targetStock"":37738,""stock"":0,""demand"":263558,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.08"",""stolenmod"":""0.7500""},{""id"":""128049152"",""name"":""Platinum"",""cost_min"":""17936.00"",""cost_max"":19235.073043478,""cost_mean"":""19279.00"",""homebuy"":""97"",""homesell"":""97"",""consumebuy"":""0"",""baseCreationQty"":0,""baseConsumptionQty"":9.6,""capacity"":22504,""buyPrice"":0,""sellPrice"":19236,""meanPrice"":19279,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":11252,""targetStock"":2813,""stock"":0,""demand"":19691,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.04"",""stolenmod"":""0.7500""},{""id"":""128673845"",""name"":""Praseodymium"",""cost_min"":""6138.00"",""cost_max"":7057.7652173913,""cost_mean"":""7156.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":113.6,""capacity"":266278,""buyPrice"":0,""sellPrice"":7058,""meanPrice"":7156,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":133139,""targetStock"":33284,""stock"":0,""demand"":232994,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1200"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.28"",""stolenmod"":""0.7500""},{""id"":""128673847"",""name"":""Samarium"",""cost_min"":""5373.00"",""cost_max"":6236.9258695652,""cost_mean"":""6330.00"",""homebuy"":""91"",""homesell"":""90"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":125.6,""capacity"":294406,""buyPrice"":0,""sellPrice"":6237,""meanPrice"":6330,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":147203,""targetStock"":36800,""stock"":0,""demand"":257606,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1600"",""sec_illegal_min"":""1.04"",""sec_illegal_max"":""1.31"",""stolenmod"":""0.7500""},{""id"":""128049155"",""name"":""Silver"",""cost_min"":""4705.00"",""cost_max"":5513.8260869565,""cost_mean"":""4775.00"",""homebuy"":""90"",""homesell"":""89"",""consumebuy"":""1"",""baseCreationQty"":7,""baseConsumptionQty"":41.6,""capacity"":109816,""buyPrice"":0,""sellPrice"":5514,""meanPrice"":4775,""demandBracket"":3,""stockBracket"":0,""creationQty"":6153,""consumptionQty"":48755,""targetStock"":18341,""stock"":0,""demand"":84155,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.2000"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.24"",""stolenmod"":""0.7500""},{""id"":""128049171"",""name"":""Tantalum"",""cost_min"":3896.58,""cost_max"":4636.2122217391,""cost_mean"":""3962.00"",""homebuy"":""89"",""homesell"":""88"",""consumebuy"":""1"",""baseCreationQty"":8.2,""baseConsumptionQty"":648.8,""capacity"":1535196,""buyPrice"":0,""sellPrice"":4637,""meanPrice"":3962,""demandBracket"":3,""stockBracket"":0,""creationQty"":7208,""consumptionQty"":760390,""targetStock"":197305,""stock"":0,""demand"":1307022,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.2600"",""sec_illegal_min"":""1.04"",""sec_illegal_max"":""1.29"",""stolenmod"":""0.7500""},{""id"":""128672299"",""name"":""Thallium"",""cost_min"":3648.12,""cost_max"":4364.4183673913,""cost_mean"":""3618.00"",""homebuy"":""88"",""homesell"":""87"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":170.4,""capacity"":399416,""buyPrice"":0,""sellPrice"":4365,""meanPrice"":3618,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":199708,""targetStock"":49927,""stock"":0,""demand"":349489,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.2800"",""sec_illegal_min"":""1.04"",""sec_illegal_max"":""1.31"",""stolenmod"":""0.7500""},{""id"":""128672301"",""name"":""Thorium"",""cost_min"":""11205.00"",""cost_max"":12284.286956522,""cost_mean"":""11513.00"",""homebuy"":""95"",""homesell"":""95"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":285.6,""capacity"":669444,""buyPrice"":0,""sellPrice"":12285,""meanPrice"":11513,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":334722,""targetStock"":83680,""stock"":0,""demand"":585764,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.11"",""stolenmod"":""0.7500""},{""id"":""128049174"",""name"":""Titanium"",""cost_min"":932.01723913043,""cost_max"":1388.4827608696,""cost_mean"":""1006.00"",""homebuy"":""77"",""homesell"":""75"",""consumebuy"":""2"",""baseCreationQty"":59.6,""baseConsumptionQty"":0,""capacity"":104776,""buyPrice"":1056,""sellPrice"":1023,""meanPrice"":1006,""demandBracket"":0,""stockBracket"":1,""creationQty"":52388,""consumptionQty"":0,""targetStock"":52388,""stock"":29336,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.3600"",""sec_illegal_min"":""1.11"",""sec_illegal_max"":""1.92"",""stolenmod"":""0.7500""},{""id"":""128049172"",""name"":""Uranium"",""cost_min"":2629.03,""cost_max"":3231.697,""cost_mean"":""2705.00"",""homebuy"":""86"",""homesell"":""85"",""consumebuy"":""1"",""baseCreationQty"":110.4,""baseConsumptionQty"":441.6,""capacity"":1229188,""buyPrice"":0,""sellPrice"":3232,""meanPrice"":2705,""demandBracket"":3,""stockBracket"":0,""creationQty"":97041,""consumptionQty"":517553,""targetStock"":226429,""stock"":0,""demand"":973787,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.3800"",""sec_illegal_min"":""1.05"",""sec_illegal_max"":""1.39"",""stolenmod"":""0.7500""},{""id"":""128049165"",""name"":""Bauxite"",""cost_min"":107.14434782609,""cost_max"":320.85565217391,""cost_mean"":""120.00"",""homebuy"":""35"",""homesell"":""29"",""consumebuy"":""7"",""baseCreationQty"":0,""baseConsumptionQty"":292.6,""capacity"":685852,""buyPrice"":0,""sellPrice"":321,""meanPrice"":120,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":342926,""targetStock"":85731,""stock"":0,""demand"":532314,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.24"",""sec_illegal_max"":""2.93"",""stolenmod"":""0.7500""},{""id"":""128049156"",""name"":""Bertrandite"",""cost_min"":2304.6704347826,""cost_max"":3015.3295652174,""cost_mean"":""2374.00"",""homebuy"":""85"",""homesell"":""84"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":116.2,""capacity"":272372,""buyPrice"":0,""sellPrice"":3016,""meanPrice"":2374,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":136186,""targetStock"":34046,""stock"":0,""demand"":232971,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.4000"",""sec_illegal_min"":""1.07"",""sec_illegal_max"":""1.57"",""stolenmod"":""0.7500""},{""id"":""128673846"",""name"":""Bromellite"",""cost_min"":""6138.00"",""cost_max"":7019.4347826087,""cost_mean"":""7062.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":113.6,""capacity"":266278,""buyPrice"":0,""sellPrice"":7020,""meanPrice"":7062,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":133139,""targetStock"":33284,""stock"":0,""demand"":232994,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.1200"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.24"",""stolenmod"":""0.7500""},{""id"":""128049159"",""name"":""Coltan"",""cost_min"":1264.7143478261,""cost_max"":1793.2856521739,""cost_mean"":""1319.00"",""homebuy"":""80"",""homesell"":""78"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":184.2,""capacity"":431764,""buyPrice"":0,""sellPrice"":1794,""meanPrice"":1319,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":215882,""targetStock"":53970,""stock"":0,""demand"":376880,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.4100"",""sec_illegal_min"":""1.10"",""sec_illegal_max"":""1.79"",""stolenmod"":""0.7500""},{""id"":""128672294"",""name"":""Cryolite"",""cost_min"":""2142.00"",""cost_max"":2681.392826087,""cost_mean"":""2266.00"",""homebuy"":""84"",""homesell"":""82"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":540.6,""capacity"":1267162,""buyPrice"":0,""sellPrice"":2682,""meanPrice"":2266,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":633581,""targetStock"":158394,""stock"":0,""demand"":1108768,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.4400"",""sec_illegal_min"":""1.07"",""sec_illegal_max"":""1.61"",""stolenmod"":""0.7500""},{""id"":""128049158"",""name"":""Gallite"",""cost_min"":""1883.00"",""cost_max"":2384.3463043478,""cost_mean"":""1819.00"",""homebuy"":""83"",""homesell"":""81"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":713,""capacity"":1671264,""buyPrice"":0,""sellPrice"":2385,""meanPrice"":1819,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":835632,""targetStock"":208907,""stock"":0,""demand"":1451673,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.4600"",""sec_illegal_min"":""1.08"",""sec_illegal_max"":""1.66"",""stolenmod"":""0.7500""},{""id"":""128672295"",""name"":""Goslarite"",""cost_min"":753.56260869565,""cost_max"":1162.4373913043,""cost_mean"":""916.00"",""homebuy"":""75"",""homesell"":""73"",""consumebuy"":""3"",""baseCreationQty"":0,""baseConsumptionQty"":138.8,""capacity"":325346,""buyPrice"":0,""sellPrice"":1163,""meanPrice"":916,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":162673,""targetStock"":40668,""stock"":0,""demand"":284678,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.3300"",""sec_illegal_min"":""1.12"",""sec_illegal_max"":""2.01"",""stolenmod"":""0.7500""},{""id"":""128049157"",""name"":""Indite"",""cost_min"":2013.607173913,""cost_max"":2681.392826087,""cost_mean"":""2088.00"",""homebuy"":""84"",""homesell"":""82"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":128.6,""capacity"":301438,""buyPrice"":0,""sellPrice"":2682,""meanPrice"":2088,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":150719,""targetStock"":37679,""stock"":0,""demand"":254775,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.4400"",""sec_illegal_min"":""1.07"",""sec_illegal_max"":""1.61"",""stolenmod"":""0.7500""},{""id"":""128049161"",""name"":""Lepidolite"",""cost_min"":518.34413043478,""cost_max"":860.65586956522,""cost_mean"":""544.00"",""homebuy"":""70"",""homesell"":""67"",""consumebuy"":""3"",""baseCreationQty"":0,""baseConsumptionQty"":56.6,""capacity"":132670,""buyPrice"":0,""sellPrice"":818,""meanPrice"":544,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":66335,""targetStock"":16583,""stock"":0,""demand"":90246,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.2700"",""sec_illegal_min"":""1.14"",""sec_illegal_max"":""2.19"",""stolenmod"":""0.7500""},{""id"":""128673853"",""name"":""Lithium Hydroxide"",""cost_min"":""4705.00"",""cost_max"":5513.8260869565,""cost_mean"":""5646.00"",""homebuy"":""90"",""homesell"":""89"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":52,""capacity"":121888,""buyPrice"":0,""sellPrice"":5514,""meanPrice"":5646,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":60944,""targetStock"":15235,""stock"":0,""demand"":106653,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.2000"",""sec_illegal_min"":""1.04"",""sec_illegal_max"":""1.35"",""stolenmod"":""0.7500""},{""id"":""128673848"",""name"":""Low Temperature Diamond"",""cost_min"":""54000.00"",""cost_max"":57684.802173913,""cost_mean"":""57445.00"",""homebuy"":""97"",""homesell"":""97"",""consumebuy"":""0"",""baseCreationQty"":0,""baseConsumptionQty"":9.8,""capacity"":22974,""buyPrice"":0,""sellPrice"":57685,""meanPrice"":57445,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":11487,""targetStock"":2871,""stock"":0,""demand"":20103,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.00"",""sec_illegal_max"":""1.03"",""stolenmod"":""0.7500""},{""id"":""128673854"",""name"":""Methane Clathrate"",""cost_min"":307.555,""cost_max"":579.445,""cost_mean"":""629.00"",""homebuy"":""63"",""homesell"":""59"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":299.8,""capacity"":702728,""buyPrice"":0,""sellPrice"":580,""meanPrice"":629,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":351364,""targetStock"":87841,""stock"":0,""demand"":614887,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.1800"",""sec_illegal_min"":""1.19"",""sec_illegal_max"":""2.52"",""stolenmod"":""0.7500""},{""id"":""128673852"",""name"":""Methanol Monohydrate Crystals"",""cost_min"":""1766.00"",""cost_max"":2252.08,""cost_mean"":""2282.00"",""homebuy"":""82"",""homesell"":""80"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":90.4,""capacity"":211898,""buyPrice"":0,""sellPrice"":2253,""meanPrice"":2282,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":105949,""targetStock"":26487,""stock"":0,""demand"":185411,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.4500"",""sec_illegal_min"":""1.08"",""sec_illegal_max"":""1.69"",""stolenmod"":""0.7500""},{""id"":""128668550"",""name"":""Painite"",""cost_min"":""35000.00"",""cost_max"":40980.434782609,""cost_mean"":""40508.00"",""homebuy"":""100"",""homesell"":""100"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":9.8,""capacity"":5744,""buyPrice"":0,""sellPrice"":40981,""meanPrice"":40508,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":2872,""targetStock"":717,""stock"":0,""demand"":5027,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.04"",""stolenmod"":""0.7500""},{""id"":""128672297"",""name"":""Pyrophyllite"",""cost_min"":""1459.00"",""cost_max"":1896.6086956522,""cost_mean"":""1565.00"",""homebuy"":""81"",""homesell"":""79"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":988.6,""capacity"":2317266,""buyPrice"":0,""sellPrice"":1897,""meanPrice"":1565,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":1158633,""targetStock"":289657,""stock"":0,""demand"":2027609,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.4200"",""sec_illegal_min"":""1.09"",""sec_illegal_max"":""1.76"",""stolenmod"":""0.7500""},{""id"":""128049163"",""name"":""Rutile"",""cost_min"":""330.00"",""cost_max"":536.22434782609,""cost_mean"":""299.00"",""homebuy"":""61"",""homesell"":""57"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":2989.8,""capacity"":7008052,""buyPrice"":0,""sellPrice"":537,""meanPrice"":299,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":3504026,""targetStock"":876006,""stock"":0,""demand"":6113349,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.1600"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.60"",""stolenmod"":""0.7500""},{""id"":""128049160"",""name"":""Uraninite"",""cost_min"":803.23913043478,""cost_max"":1224.7608695652,""cost_mean"":""836.00"",""homebuy"":""76"",""homesell"":""74"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":263.8,""capacity"":618344,""buyPrice"":0,""sellPrice"":1225,""meanPrice"":836,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":309172,""targetStock"":77293,""stock"":0,""demand"":529856,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.3400"",""sec_illegal_min"":""1.12"",""sec_illegal_max"":""1.98"",""stolenmod"":""0.7500""},{""id"":""128049214"",""name"":""Beer"",""cost_min"":""175.00"",""cost_max"":343.85565217391,""cost_mean"":""186.00"",""homebuy"":""42"",""homesell"":""36"",""consumebuy"":""6"",""baseCreationQty"":0,""baseConsumptionQty"":573.4,""capacity"":227066,""buyPrice"":0,""sellPrice"":296,""meanPrice"":186,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":113533,""targetStock"":28382,""stock"":0,""demand"":134820.75,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Narcotics"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.25"",""sec_illegal_max"":""3.04"",""stolenmod"":""0.7500""},{""id"":""128672306"",""name"":""Bootleg Liquor"",""cost_min"":108.86956521739,""cost_max"":691.13043478261,""cost_mean"":""855.00"",""homebuy"":""35"",""homesell"":""29"",""consumebuy"":""7"",""baseCreationQty"":0,""baseConsumptionQty"":7,""capacity"":1100,""buyPrice"":0,""sellPrice"":692,""meanPrice"":855,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":550,""targetStock"":137,""stock"":0,""demand"":963,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Narcotics"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.16"",""sec_illegal_max"":""2.33"",""stolenmod"":""0.7500""},{""id"":""128049216"",""name"":""Liquor"",""cost_min"":""624.00"",""cost_max"":903.09,""cost_mean"":""587.00"",""homebuy"":""71"",""homesell"":""68"",""consumebuy"":""3"",""baseCreationQty"":0,""baseConsumptionQty"":136.6,""capacity"":21458,""buyPrice"":0,""sellPrice"":904,""meanPrice"":587,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":10729,""targetStock"":2682,""stock"":0,""demand"":18018,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Narcotics"",""volumescale"":""1.2800"",""sec_illegal_min"":""1.14"",""sec_illegal_max"":""2.16"",""stolenmod"":""0.7500""},{""id"":""128049215"",""name"":""Wine"",""cost_min"":""252.00"",""cost_max"":436.51652173913,""cost_mean"":""260.00"",""homebuy"":""54"",""homesell"":""49"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":343.2,""capacity"":125226,""buyPrice"":0,""sellPrice"":385,""meanPrice"":260,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":62613,""targetStock"":15653,""stock"":0,""demand"":74353,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Narcotics"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.23"",""sec_illegal_max"":""2.88"",""stolenmod"":""0.7500""},{""id"":""128066403"",""name"":""Drones"",""cost_min"":""100.00"",""cost_max"":""100.00"",""cost_mean"":""101.00"",""homebuy"":""100"",""homesell"":""100"",""consumebuy"":""1"",""baseCreationQty"":200,""baseConsumptionQty"":0,""capacity"":58762,""buyPrice"":101,""sellPrice"":100,""meanPrice"":101,""demandBracket"":0,""stockBracket"":3,""creationQty"":58762,""consumptionQty"":0,""targetStock"":58762,""stock"":58762,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""NonMarketable"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.00"",""sec_illegal_max"":""0.99"",""stolenmod"":""0.7500""},{""id"":""128667728"",""name"":""Imperial Slaves"",""cost_min"":""15678.00"",""cost_max"":16903.149130435,""cost_mean"":""15984.00"",""homebuy"":""97"",""homesell"":""97"",""consumebuy"":""0"",""baseCreationQty"":3,""baseConsumptionQty"":11.2,""capacity"":33290,""buyPrice"":0,""sellPrice"":16511,""meanPrice"":15984,""demandBracket"":2,""stockBracket"":0,""creationQty"":3517,""consumptionQty"":13128,""targetStock"":6799,""stock"":0,""demand"":19106.5,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Slaves"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.00"",""sec_illegal_max"":""1.03"",""stolenmod"":""0.7500""},{""id"":""128049231"",""name"":""Advanced Catalysers"",""cost_min"":2770,""cost_max"":3419,""cost_mean"":""2947.00"",""homebuy"":""86"",""homesell"":""85"",""consumebuy"":""1"",""baseCreationQty"":209.6,""baseConsumptionQty"":104.8,""capacity"":798368,""buyPrice"":2957,""sellPrice"":2907,""meanPrice"":2947,""demandBracket"":0,""stockBracket"":1,""creationQty"":184237,""consumptionQty"":122827,""targetStock"":214943,""stock"":174037,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[""powerplay""],""categoryname"":""Technology"",""volumescale"":""1.3600"",""sec_illegal_min"":""1.05"",""sec_illegal_max"":""1.39"",""stolenmod"":""0.7500""},{""id"":""128049229"",""name"":""Animal Monitors"",""cost_min"":247.91608695652,""cost_max"":""446.00"",""cost_mean"":""324.00"",""homebuy"":""59"",""homesell"":""55"",""consumebuy"":""4"",""baseCreationQty"":29.6,""baseConsumptionQty"":0,""capacity"":52038,""buyPrice"":259,""sellPrice"":240,""meanPrice"":324,""demandBracket"":0,""stockBracket"":1,""creationQty"":26019,""consumptionQty"":0,""targetStock"":26019,""stock"":14570,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.1400"",""sec_illegal_min"":""1.21"",""sec_illegal_max"":""2.69"",""stolenmod"":""0.7500""},{""id"":""128049230"",""name"":""Aquaponic Systems"",""cost_min"":223.6997826087,""cost_max"":""415.00"",""cost_mean"":""314.00"",""homebuy"":""56"",""homesell"":""52"",""consumebuy"":""4"",""baseCreationQty"":32.8,""baseConsumptionQty"":0,""capacity"":57662,""buyPrice"":228,""sellPrice"":211,""meanPrice"":314,""demandBracket"":0,""stockBracket"":1,""creationQty"":28831,""consumptionQty"":0,""targetStock"":28831,""stock"":16145,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.1200"",""sec_illegal_min"":""1.22"",""sec_illegal_max"":""2.78"",""stolenmod"":""0.7500""},{""id"":""128049228"",""name"":""Auto Fabricators"",""cost_min"":3633,""cost_max"":4378,""cost_mean"":""3734.00"",""homebuy"":""88"",""homesell"":""87"",""consumebuy"":""1"",""baseCreationQty"":13.6,""baseConsumptionQty"":0,""capacity"":31084,""buyPrice"":3838,""sellPrice"":3774,""meanPrice"":3734,""demandBracket"":0,""stockBracket"":1,""creationQty"":11955,""consumptionQty"":0,""targetStock"":11955,""stock"":8703,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[""powerplay""],""categoryname"":""Technology"",""volumescale"":""1.2800"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.28"",""stolenmod"":""0.7500""},{""id"":""128049672"",""name"":""Bio Reducing Lichen"",""cost_min"":898,""cost_max"":1264,""cost_mean"":""998.00"",""homebuy"":""76"",""homesell"":""74"",""consumebuy"":""2"",""baseCreationQty"":100,""baseConsumptionQty"":0,""capacity"":228540,""buyPrice"":951,""sellPrice"":921,""meanPrice"":998,""demandBracket"":0,""stockBracket"":1,""creationQty"":87900,""consumptionQty"":0,""targetStock"":87900,""stock"":63985,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[""powerplay""],""categoryname"":""Technology"",""volumescale"":""1.3500"",""sec_illegal_min"":""1.11"",""sec_illegal_max"":""1.95"",""stolenmod"":""0.7500""},{""id"":""128049225"",""name"":""Computer Components"",""cost_min"":""527.00"",""cost_max"":782.76630434783,""cost_mean"":""513.00"",""homebuy"":""69"",""homesell"":""66"",""consumebuy"":""3"",""baseCreationQty"":0,""baseConsumptionQty"":251.2,""capacity"":147608,""buyPrice"":0,""sellPrice"":783,""meanPrice"":513,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":73804,""targetStock"":18451,""stock"":0,""demand"":114193,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.2500"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128673875"",""name"":""Diagnostic Sensor"",""cost_min"":""4122.00"",""cost_max"":4880.0439130435,""cost_mean"":""4337.00"",""homebuy"":""89"",""homesell"":""88"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":15.2,""capacity"":35630,""buyPrice"":0,""sellPrice"":4881,""meanPrice"":4337,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":17815,""targetStock"":4453,""stock"":0,""demand"":31177,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.2400"",""sec_illegal_min"":""1.02"",""sec_illegal_max"":""1.20"",""stolenmod"":""0.7500""},{""id"":""128049226"",""name"":""Hazardous Environment Suits"",""cost_min"":223.6997826087,""cost_max"":""415.00"",""cost_mean"":""340.00"",""homebuy"":""56"",""homesell"":""52"",""consumebuy"":""4"",""baseCreationQty"":32.8,""baseConsumptionQty"":81.6,""capacity"":248934,""buyPrice"":0,""sellPrice"":394,""meanPrice"":340,""demandBracket"":2,""stockBracket"":0,""creationQty"":28831,""consumptionQty"":95636,""targetStock"":52740,""stock"":0,""demand"":171298,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.1200"",""sec_illegal_min"":""1.22"",""sec_illegal_max"":""2.78"",""stolenmod"":""0.7500""},{""id"":""128682047"",""name"":""Medical Diagnostic Equipment"",""cost_min"":2637.6134782609,""cost_max"":""3256.00"",""cost_mean"":""2848.00"",""homebuy"":""86"",""homesell"":""85"",""consumebuy"":""1"",""baseCreationQty"":4,""baseConsumptionQty"":0,""capacity"":7032,""buyPrice"":2573,""sellPrice"":2529,""meanPrice"":2848,""demandBracket"":0,""stockBracket"":2,""creationQty"":3516,""consumptionQty"":0,""targetStock"":3516,""stock"":3516,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.3600"",""sec_illegal_min"":""1.05"",""sec_illegal_max"":""1.44"",""stolenmod"":""0.7500""},{""id"":""128673873"",""name"":""Micro Controllers"",""cost_min"":3021.0086956522,""cost_max"":""3682.00"",""cost_mean"":""3274.00"",""homebuy"":""87"",""homesell"":""86"",""consumebuy"":""1"",""baseCreationQty"":15.2,""baseConsumptionQty"":0,""capacity"":26722,""buyPrice"":3190,""sellPrice"":3136,""meanPrice"":3274,""demandBracket"":0,""stockBracket"":1,""creationQty"":13361,""consumptionQty"":0,""targetStock"":13361,""stock"":7481,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.3200"",""sec_illegal_min"":""1.05"",""sec_illegal_max"":""1.37"",""stolenmod"":""0.7500""},{""id"":""128049671"",""name"":""Resonating Separators"",""cost_min"":5892,""cost_max"":6797,""cost_mean"":""5958.00"",""homebuy"":""91"",""homesell"":""90"",""consumebuy"":""1"",""baseCreationQty"":9.6,""baseConsumptionQty"":59.6,""capacity"":203558,""buyPrice"":0,""sellPrice"":6690,""meanPrice"":5958,""demandBracket"":2,""stockBracket"":0,""creationQty"":8439,""consumptionQty"":69852,""targetStock"":25902,""stock"":0,""demand"":139331,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[""powerplay""],""categoryname"":""Technology"",""volumescale"":""1.1400"",""sec_illegal_min"":""1.02"",""sec_illegal_max"":""1.18"",""stolenmod"":""0.7500""},{""id"":""128049227"",""name"":""Robotics"",""cost_min"":1817.55,""cost_max"":2353.05,""cost_mean"":""1856.00"",""homebuy"":""82"",""homesell"":""80"",""consumebuy"":""2"",""baseCreationQty"":12,""baseConsumptionQty"":0,""capacity"":54852,""buyPrice"":1941,""sellPrice"":1883,""meanPrice"":1856,""demandBracket"":0,""stockBracket"":1,""creationQty"":10548,""consumptionQty"":0,""targetStock"":10548,""stock"":3838.5,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[""powerplay""],""categoryname"":""Technology"",""volumescale"":""1.4500"",""sec_illegal_min"":""1.08"",""sec_illegal_max"":""1.69"",""stolenmod"":""0.7500""},{""id"":""128672311"",""name"":""Structural Regulators"",""cost_min"":1730.316,""cost_max"":2240.7,""cost_mean"":""1791.00"",""homebuy"":""82"",""homesell"":""80"",""consumebuy"":""2"",""baseCreationQty"":2.4,""baseConsumptionQty"":0,""capacity"":8440,""buyPrice"":1848,""sellPrice"":1793,""meanPrice"":1791,""demandBracket"":0,""stockBracket"":1,""creationQty"":2110,""consumptionQty"":0,""targetStock"":2110,""stock"":1055,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.4500"",""sec_illegal_min"":""1.08"",""sec_illegal_max"":""1.69"",""stolenmod"":""0.7500""},{""id"":""128049232"",""name"":""Terrain Enrichment Systems"",""cost_min"":4813,""cost_max"":5623,""cost_mean"":""4887.00"",""homebuy"":""90"",""homesell"":""89"",""consumebuy"":""1"",""baseCreationQty"":139.2,""baseConsumptionQty"":0,""capacity"":318126,""buyPrice"":5049,""sellPrice"":4966,""meanPrice"":4887,""demandBracket"":0,""stockBracket"":1,""creationQty"":122356,""consumptionQty"":0,""targetStock"":122356,""stock"":89074,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[""powerplay""],""categoryname"":""Technology"",""volumescale"":""1.2000"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.22"",""stolenmod"":""0.7500""},{""id"":""128682044"",""name"":""Conductive Fabrics"",""cost_min"":528.64,""cost_max"":801.45739130435,""cost_mean"":""507.00"",""homebuy"":""67"",""homesell"":""64"",""consumebuy"":""3"",""baseCreationQty"":46.4,""baseConsumptionQty"":371.2,""capacity"":1903344,""buyPrice"":0,""sellPrice"":802,""meanPrice"":507,""demandBracket"":3,""stockBracket"":0,""creationQty"":40786,""consumptionQty"":435050,""targetStock"":149548,""stock"":0,""demand"":1821198,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Textiles"",""volumescale"":""1.2300"",""sec_illegal_min"":""1.16"",""sec_illegal_max"":""2.33"",""stolenmod"":""0.7500""},{""id"":""128682045"",""name"":""Military Grade Fabrics"",""cost_min"":749.39465217391,""cost_max"":1170.2653478261,""cost_mean"":""708.00"",""homebuy"":""74"",""homesell"":""71"",""consumebuy"":""3"",""baseCreationQty"":29.2,""baseConsumptionQty"":0,""capacity"":102668,""buyPrice"":871,""sellPrice"":831,""meanPrice"":708,""demandBracket"":0,""stockBracket"":1,""creationQty"":25667,""consumptionQty"":0,""targetStock"":25667,""stock"":12833.5,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Textiles"",""volumescale"":""1.3200"",""sec_illegal_min"":""1.13"",""sec_illegal_max"":""2.04"",""stolenmod"":""0.7500""},{""id"":""128049193"",""name"":""Synthetic Fabrics"",""cost_min"":271.01227826087,""cost_max"":690.12772173913,""cost_mean"":""211.00"",""homebuy"":""45"",""homesell"":""40"",""consumebuy"":""6"",""baseCreationQty"":136.2,""baseConsumptionQty"":0,""capacity"":478876,""buyPrice"":313,""sellPrice"":277,""meanPrice"":211,""demandBracket"":0,""stockBracket"":1,""creationQty"":119719,""consumptionQty"":0,""targetStock"":119719,""stock"":33520,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Textiles"",""volumescale"":""1.0200"",""sec_illegal_min"":""1.29"",""sec_illegal_max"":""3.34"",""stolenmod"":""0.7500""},{""id"":""128049244"",""name"":""Biowaste"",""cost_min"":32.834782608696,""cost_max"":""97.00"",""cost_mean"":""63.00"",""homebuy"":""27"",""homesell"":""20"",""consumebuy"":""7"",""baseCreationQty"":162,""baseConsumptionQty"":0,""capacity"":27690,""buyPrice"":26,""sellPrice"":19,""meanPrice"":63,""demandBracket"":0,""stockBracket"":1,""creationQty"":13845,""consumptionQty"":0,""targetStock"":13845,""stock"":7753,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Waste "",""volumescale"":""1.0000"",""sec_illegal_min"":""1.44"",""sec_illegal_max"":""4.47"",""stolenmod"":""0.7500""},{""id"":""128049246"",""name"":""Chemical Waste"",""cost_min"":36.121739130435,""cost_max"":120.87826086957,""cost_mean"":""131.00"",""homebuy"":""18"",""homesell"":""10"",""consumebuy"":""8"",""baseCreationQty"":0,""baseConsumptionQty"":72.2,""capacity"":42426,""buyPrice"":0,""sellPrice"":121,""meanPrice"":131,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":21213,""targetStock"":5303,""stock"":0,""demand"":37123,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Waste "",""volumescale"":""1.0000"",""sec_illegal_min"":""1.48"",""sec_illegal_max"":""4.78"",""stolenmod"":""0.7500""},{""id"":""128049248"",""name"":""Scrap"",""cost_min"":51.826086956522,""cost_max"":""120.00"",""cost_mean"":""48.00"",""homebuy"":""42"",""homesell"":""36"",""consumebuy"":""6"",""baseCreationQty"":120.8,""baseConsumptionQty"":30.2,""capacity"":153376,""buyPrice"":51,""sellPrice"":44,""meanPrice"":48,""demandBracket"":0,""stockBracket"":1,""creationQty"":41293,""consumptionQty"":35395,""targetStock"":50141,""stock"":31969,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Waste "",""volumescale"":""1.0000"",""sec_illegal_min"":""1.31"",""sec_illegal_max"":""3.48"",""stolenmod"":""0.7500""},{""id"":""128049236"",""name"":""Non Lethal Weapons"",""cost_min"":1647.92,""cost_max"":""2134.00"",""cost_mean"":""1837.00"",""homebuy"":""82"",""homesell"":""80"",""consumebuy"":""2"",""baseCreationQty"":120,""baseConsumptionQty"":15.8,""capacity"":71858,""buyPrice"":1741,""sellPrice"":1689,""meanPrice"":1837,""demandBracket"":0,""stockBracket"":1,""creationQty"":35161,""consumptionQty"":768,""targetStock"":35352,""stock"":19875,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Weapons"",""volumescale"":""1.4500"",""sec_illegal_min"":""1.08"",""sec_illegal_max"":""1.69"",""stolenmod"":""0.7500""},{""id"":""128049235"",""name"":""Reactive Armour"",""cost_min"":1978,""cost_max"":2525,""cost_mean"":""2113.00"",""homebuy"":""84"",""homesell"":""82"",""consumebuy"":""2"",""baseCreationQty"":54.4,""baseConsumptionQty"":13.6,""capacity"":44308,""buyPrice"":2116,""sellPrice"":2055,""meanPrice"":2113,""demandBracket"":0,""stockBracket"":1,""creationQty"":15940,""consumptionQty"":1101,""targetStock"":16215,""stock"":11957,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[""powerplay""],""categoryname"":""Weapons"",""volumescale"":""1.4600"",""sec_illegal_min"":""1.08"",""sec_illegal_max"":""1.64"",""stolenmod"":""0.7500""}],""ships"":{""shipyard_list"":{""Orca"":{""id"":128049327,""name"":""Orca"",""basevalue"":46143231,""sku"":""""},""Cutter"":{""id"":128049375,""name"":""Cutter"",""basevalue"":198651585,""sku"":""""},""Eagle"":{""id"":128049255,""name"":""Eagle"",""basevalue"":42588,""sku"":""""},""Empire_Trader"":{""id"":128049315,""name"":""Empire_Trader"",""basevalue"":21195002,""sku"":""""},""Viper"":{""id"":128049273,""name"":""Viper"",""basevalue"":135874,""sku"":""""},""CobraMkIII"":{""id"":128049279,""name"":""CobraMkIII"",""basevalue"":332451,""sku"":""""},""SideWinder"":{""id"":128049249,""name"":""SideWinder"",""basevalue"":30420,""sku"":""""},""Viper_MkIV"":{""id"":128672255,""name"":""Viper_MkIV"",""basevalue"":416309,""sku"":""""},""Type9"":{""id"":128049333,""name"":""Type9"",""basevalue"":72775898,""sku"":""""},""CobraMkIV"":{""id"":128672262,""name"":""CobraMkIV"",""basevalue"":726961,""sku"":""ELITE_HORIZONS_V_COBRA_MK_IV_1000""},""Adder"":{""id"":128049267,""name"":""Adder"",""basevalue"":83473,""sku"":""""},""Type6"":{""id"":128049285,""name"":""Type6"",""basevalue"":994302,""sku"":""""},""Independant_Trader"":{""id"":128672269,""name"":""Independant_Trader"",""basevalue"":2971801,""sku"":""""}},""unavailable_list"":[]},""modules"":{""128049509"":{""id"":128049509,""category"":""weapon"",""name"":""Hpt_AdvancedTorpPylon_Fixed_Small"",""cost"":10647,""sku"":null},""128666725"":{""id"":128666725,""category"":""weapon"",""name"":""Hpt_DumbfireMissileRack_Fixed_Medium"",""cost"":228531,""sku"":null},""128666724"":{""id"":128666724,""category"":""weapon"",""name"":""Hpt_DumbfireMissileRack_Fixed_Small"",""cost"":30587,""sku"":null},""128671448"":{""id"":128671448,""category"":""weapon"",""name"":""Hpt_MineLauncher_Fixed_Small_Impulse"",""cost"":34594,""sku"":null},""128049500"":{""id"":128049500,""category"":""weapon"",""name"":""Hpt_MineLauncher_Fixed_Small"",""cost"":23063,""sku"":null},""128049501"":{""id"":128049501,""category"":""weapon"",""name"":""Hpt_MineLauncher_Fixed_Medium"",""cost"":279560,""sku"":null},""128049493"":{""id"":128049493,""category"":""weapon"",""name"":""Hpt_BasicMissileRack_Fixed_Medium"",""cost"":487101,""sku"":null},""128049492"":{""id"":128049492,""category"":""weapon"",""name"":""Hpt_BasicMissileRack_Fixed_Small"",""cost"":69016,""sku"":null},""128049466"":{""id"":128049466,""category"":""weapon"",""name"":""Hpt_PlasmaAccelerator_Fixed_Large"",""cost"":2900547,""sku"":null},""128049465"":{""id"":128049465,""category"":""weapon"",""name"":""Hpt_PlasmaAccelerator_Fixed_Medium"",""cost"":793012,""sku"":null},""128681995"":{""id"":128681995,""category"":""weapon"",""name"":""Hpt_PulseLaser_Gimbal_Huge"",""cost"":834269,""sku"":null},""128049384"":{""id"":128049384,""category"":""weapon"",""name"":""Hpt_PulseLaser_Fixed_Huge"",""cost"":168831,""sku"":null},""128049390"":{""id"":128049390,""category"":""weapon"",""name"":""Hpt_PulseLaser_Turret_Large"",""cost"":380631,""sku"":null},""128049387"":{""id"":128049387,""category"":""weapon"",""name"":""Hpt_PulseLaser_Gimbal_Large"",""cost"":133658,""sku"":null},""128049385"":{""id"":128049385,""category"":""weapon"",""name"":""Hpt_PulseLaser_Gimbal_Small"",""cost"":6275,""sku"":null},""128049388"":{""id"":128049388,""category"":""weapon"",""name"":""Hpt_PulseLaser_Turret_Small"",""cost"":24717,""sku"":null},""128049386"":{""id"":128049386,""category"":""weapon"",""name"":""Hpt_PulseLaser_Gimbal_Medium"",""cost"":33653,""sku"":null},""128049382"":{""id"":128049382,""category"":""weapon"",""name"":""Hpt_PulseLaser_Fixed_Medium"",""cost"":16731,""sku"":null},""128049381"":{""id"":128049381,""category"":""weapon"",""name"":""Hpt_PulseLaser_Fixed_Small"",""cost"":2092,""sku"":null},""128049409"":{""id"":128049409,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Turret_Large"",""cost"":760881,""sku"":null},""128049406"":{""id"":128049406,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Gimbal_Large"",""cost"":267696,""sku"":null},""128049404"":{""id"":128049404,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Gimbal_Small"",""cost"":8176,""sku"":null},""128049405"":{""id"":128049405,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Gimbal_Medium"",""cost"":46106,""sku"":null},""128049401"":{""id"":128049401,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Fixed_Medium"",""cost"":21865,""sku"":null},""128049402"":{""id"":128049402,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Fixed_Large"",""cost"":133468,""sku"":null},""128049400"":{""id"":128049400,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Fixed_Small"",""cost"":4183,""sku"":null},""128681994"":{""id"":128681994,""category"":""weapon"",""name"":""Hpt_BeamLaser_Gimbal_Huge"",""cost"":8314319,""sku"":null},""128049434"":{""id"":128049434,""category"":""weapon"",""name"":""Hpt_BeamLaser_Gimbal_Large"",""cost"":2277850,""sku"":null},""128049428"":{""id"":128049428,""category"":""weapon"",""name"":""Hpt_BeamLaser_Fixed_Small"",""cost"":35582,""sku"":null},""128049430"":{""id"":128049430,""category"":""weapon"",""name"":""Hpt_BeamLaser_Fixed_Large"",""cost"":1119456,""sku"":null},""128049432"":{""id"":128049432,""category"":""weapon"",""name"":""Hpt_BeamLaser_Gimbal_Small"",""cost"":70965,""sku"":null},""128049436"":{""id"":128049436,""category"":""weapon"",""name"":""Hpt_BeamLaser_Turret_Medium"",""cost"":1996218,""sku"":null},""128049435"":{""id"":128049435,""category"":""weapon"",""name"":""Hpt_BeamLaser_Turret_Small"",""cost"":475313,""sku"":null},""128049429"":{""id"":128049429,""category"":""weapon"",""name"":""Hpt_BeamLaser_Fixed_Medium"",""cost"":284732,""sku"":null},""128049444"":{""id"":128049444,""category"":""weapon"",""name"":""Hpt_Cannon_Gimbal_Huge"",""cost"":5134896,""sku"":null},""128049441"":{""id"":128049441,""category"":""weapon"",""name"":""Hpt_Cannon_Fixed_Huge"",""cost"":2567448,""sku"":null},""128049445"":{""id"":128049445,""category"":""weapon"",""name"":""Hpt_Cannon_Turret_Small"",""cost"":481397,""sku"":null},""128671120"":{""id"":128671120,""category"":""weapon"",""name"":""Hpt_Cannon_Gimbal_Large"",""cost"":1283724,""sku"":null},""128049447"":{""id"":128049447,""category"":""weapon"",""name"":""Hpt_Cannon_Turret_Large"",""cost"":15404688,""sku"":null},""128049446"":{""id"":128049446,""category"":""weapon"",""name"":""Hpt_Cannon_Turret_Medium"",""cost"":3851172,""sku"":null},""128049442"":{""id"":128049442,""category"":""weapon"",""name"":""Hpt_Cannon_Gimbal_Small"",""cost"":40117,""sku"":null},""128671321"":{""id"":128671321,""category"":""weapon"",""name"":""Hpt_Slugshot_Gimbal_Large"",""cost"":1664583,""sku"":null},""128671322"":{""id"":128671322,""category"":""weapon"",""name"":""Hpt_Slugshot_Turret_Large"",""cost"":5548608,""sku"":null},""128049454"":{""id"":128049454,""category"":""weapon"",""name"":""Hpt_Slugshot_Turret_Medium"",""cost"":1387152,""sku"":null},""128049451"":{""id"":128049451,""category"":""weapon"",""name"":""Hpt_Slugshot_Gimbal_Small"",""cost"":52019,""sku"":null},""128049452"":{""id"":128049452,""category"":""weapon"",""name"":""Hpt_Slugshot_Gimbal_Medium"",""cost"":416146,""sku"":null},""128049450"":{""id"":128049450,""category"":""weapon"",""name"":""Hpt_Slugshot_Fixed_Large"",""cost"":1109722,""sku"":null},""128049458"":{""id"":128049458,""category"":""weapon"",""name"":""Hpt_MultiCannon_Fixed_Huge"",""cost"":1119456,""sku"":null},""128049462"":{""id"":128049462,""category"":""weapon"",""name"":""Hpt_MultiCannon_Turret_Small"",""cost"":77571,""sku"":null},""128049463"":{""id"":128049463,""category"":""weapon"",""name"":""Hpt_MultiCannon_Turret_Medium"",""cost"":1228968,""sku"":null},""128049459"":{""id"":128049459,""category"":""weapon"",""name"":""Hpt_MultiCannon_Gimbal_Small"",""cost"":13547,""sku"":null},""128049460"":{""id"":128049460,""category"":""weapon"",""name"":""Hpt_MultiCannon_Gimbal_Medium"",""cost"":54186,""sku"":null},""128049457"":{""id"":128049457,""category"":""weapon"",""name"":""Hpt_MultiCannon_Fixed_Large"",""cost"":133468,""sku"":null},""128049489"":{""id"":128049489,""category"":""weapon"",""name"":""Hpt_Railgun_Fixed_Medium"",""cost"":392418,""sku"":null},""128049488"":{""id"":128049488,""category"":""weapon"",""name"":""Hpt_Railgun_Fixed_Small"",""cost"":49053,""sku"":null},""128049440"":{""id"":128049440,""category"":""weapon"",""name"":""Hpt_Cannon_Fixed_Large"",""cost"":641862,""sku"":null},""128049439"":{""id"":128049439,""category"":""weapon"",""name"":""Hpt_Cannon_Fixed_Medium"",""cost"":160114,""sku"":null},""128049443"":{""id"":128049443,""category"":""weapon"",""name"":""Hpt_Cannon_Gimbal_Medium"",""cost"":320931,""sku"":null},""128049438"":{""id"":128049438,""category"":""weapon"",""name"":""Hpt_Cannon_Fixed_Small"",""cost"":20059,""sku"":null},""128049431"":{""id"":128049431,""category"":""weapon"",""name"":""Hpt_BeamLaser_Fixed_Huge"",""cost"":2277850,""sku"":null},""128049437"":{""id"":128049437,""category"":""weapon"",""name"":""Hpt_BeamLaser_Turret_Large"",""cost"":18441745,""sku"":null},""128049433"":{""id"":128049433,""category"":""weapon"",""name"":""Hpt_BeamLaser_Gimbal_Medium"",""cost"":475883,""sku"":null},""128049461"":{""id"":128049461,""category"":""weapon"",""name"":""Hpt_MultiCannon_Gimbal_Large"",""cost"":549876,""sku"":null},""128681996"":{""id"":128681996,""category"":""weapon"",""name"":""Hpt_MultiCannon_Gimbal_Huge"",""cost"":6062706,""sku"":null},""128049456"":{""id"":128049456,""category"":""weapon"",""name"":""Hpt_MultiCannon_Fixed_Medium"",""cost"":36124,""sku"":null},""128049455"":{""id"":128049455,""category"":""weapon"",""name"":""Hpt_MultiCannon_Fixed_Small"",""cost"":9031,""sku"":null},""128049453"":{""id"":128049453,""category"":""weapon"",""name"":""Hpt_Slugshot_Turret_Small"",""cost"":173394,""sku"":null},""128049448"":{""id"":128049448,""category"":""weapon"",""name"":""Hpt_Slugshot_Fixed_Small"",""cost"":34223,""sku"":null},""128049449"":{""id"":128049449,""category"":""weapon"",""name"":""Hpt_Slugshot_Fixed_Medium"",""cost"":277431,""sku"":null},""128049389"":{""id"":128049389,""category"":""weapon"",""name"":""Hpt_PulseLaser_Turret_Medium"",""cost"":126243,""sku"":null},""128049510"":{""id"":128049510,""category"":""weapon"",""name"":""Hpt_AdvancedTorpPylon_Fixed_Medium"",""cost"":42588,""sku"":null},""128049467"":{""id"":128049467,""category"":""weapon"",""name"":""Hpt_PlasmaAccelerator_Fixed_Huge"",""cost"":13112541,""sku"":null},""128662524"":{""id"":128662524,""category"":""utility"",""name"":""Hpt_CargoScanner_Size0_Class5"",""cost"":1042926,""sku"":null},""128662523"":{""id"":128662523,""category"":""utility"",""name"":""Hpt_CargoScanner_Size0_Class4"",""cost"":347642,""sku"":null},""128662522"":{""id"":128662522,""category"":""utility"",""name"":""Hpt_CargoScanner_Size0_Class3"",""cost"":115881,""sku"":null},""128662521"":{""id"":128662521,""category"":""utility"",""name"":""Hpt_CargoScanner_Size0_Class2"",""cost"":38627,""sku"":null},""128662520"":{""id"":128662520,""category"":""utility"",""name"":""Hpt_CargoScanner_Size0_Class1"",""cost"":12876,""sku"":null},""128662533"":{""id"":128662533,""category"":""utility"",""name"":""Hpt_CrimeScanner_Size0_Class4"",""cost"":347642,""sku"":null},""128662532"":{""id"":128662532,""category"":""utility"",""name"":""Hpt_CrimeScanner_Size0_Class3"",""cost"":115881,""sku"":null},""128662531"":{""id"":128662531,""category"":""utility"",""name"":""Hpt_CrimeScanner_Size0_Class2"",""cost"":38627,""sku"":null},""128662530"":{""id"":128662530,""category"":""utility"",""name"":""Hpt_CrimeScanner_Size0_Class1"",""cost"":12876,""sku"":null},""128662526"":{""id"":128662526,""category"":""utility"",""name"":""Hpt_CloudScanner_Size0_Class2"",""cost"":38627,""sku"":null},""128662529"":{""id"":128662529,""category"":""utility"",""name"":""Hpt_CloudScanner_Size0_Class5"",""cost"":1042926,""sku"":null},""128049549"":{""id"":128049549,""category"":""utility"",""name"":""Int_DockingComputer_Standard"",""cost"":4278,""sku"":null},""128049519"":{""id"":128049519,""category"":""utility"",""name"":""Hpt_HeatSinkLauncher_Turret_Tiny"",""cost"":3328,""sku"":null},""128049522"":{""id"":128049522,""category"":""utility"",""name"":""Hpt_PlasmaPointDefence_Turret_Tiny"",""cost"":17631,""sku"":null},""128049526"":{""id"":128049526,""category"":""utility"",""name"":""Hpt_MiningLaser_Fixed_Medium"",""cost"":21462,""sku"":null},""128049516"":{""id"":128049516,""category"":""utility"",""name"":""Hpt_ElectronicCountermeasure_Tiny"",""cost"":11883,""sku"":null},""128049513"":{""id"":128049513,""category"":""utility"",""name"":""Hpt_ChaffLauncher_Tiny"",""cost"":8081,""sku"":null},""128662528"":{""id"":128662528,""category"":""utility"",""name"":""Hpt_CloudScanner_Size0_Class4"",""cost"":347642,""sku"":null},""128662527"":{""id"":128662527,""category"":""utility"",""name"":""Hpt_CloudScanner_Size0_Class3"",""cost"":115881,""sku"":null},""128049330"":{""id"":128049330,""category"":""module"",""name"":""Orca_Armour_Grade3"",""cost"":41528907,""sku"":null},""128049329"":{""id"":128049329,""category"":""module"",""name"":""Orca_Armour_Grade2"",""cost"":18457292,""sku"":null},""128049328"":{""id"":128049328,""category"":""module"",""name"":""Orca_Armour_Grade1"",""cost"":0,""sku"":null},""128049331"":{""id"":128049331,""category"":""module"",""name"":""Orca_Armour_Mirrored"",""cost"":98146650,""sku"":null},""128049332"":{""id"":128049332,""category"":""module"",""name"":""Orca_Armour_Reactive"",""cost"":108759593,""sku"":null},""128049378"":{""id"":128049378,""category"":""module"",""name"":""Cutter_Armour_Grade3"",""cost"":178786426,""sku"":null},""128049377"":{""id"":128049377,""category"":""module"",""name"":""Cutter_Armour_Grade2"",""cost"":79460634,""sku"":null},""128049376"":{""id"":128049376,""category"":""module"",""name"":""Cutter_Armour_Grade1"",""cost"":0,""sku"":null},""128049380"":{""id"":128049380,""category"":""module"",""name"":""Cutter_Armour_Reactive"",""cost"":468221784,""sku"":null},""128049379"":{""id"":128049379,""category"":""module"",""name"":""Cutter_Armour_Mirrored"",""cost"":422531919,""sku"":null},""128049258"":{""id"":128049258,""category"":""module"",""name"":""Eagle_Armour_Grade3"",""cost"":85602,""sku"":null},""128049257"":{""id"":128049257,""category"":""module"",""name"":""Eagle_Armour_Grade2"",""cost"":25553,""sku"":null},""128049256"":{""id"":128049256,""category"":""module"",""name"":""Eagle_Armour_Grade1"",""cost"":0,""sku"":null},""128049259"":{""id"":128049259,""category"":""module"",""name"":""Eagle_Armour_Mirrored"",""cost"":133173,""sku"":null},""128049260"":{""id"":128049260,""category"":""module"",""name"":""Eagle_Armour_Reactive"",""cost"":142968,""sku"":null},""128049318"":{""id"":128049318,""category"":""module"",""name"":""Empire_Trader_Armour_Grade3"",""cost"":19075502,""sku"":null},""128049317"":{""id"":128049317,""category"":""module"",""name"":""Empire_Trader_Armour_Grade2"",""cost"":8478001,""sku"":null},""128049316"":{""id"":128049316,""category"":""module"",""name"":""Empire_Trader_Armour_Grade1"",""cost"":0,""sku"":null},""128049319"":{""id"":128049319,""category"":""module"",""name"":""Empire_Trader_Armour_Mirrored"",""cost"":45081769,""sku"":null},""128049320"":{""id"":128049320,""category"":""module"",""name"":""Empire_Trader_Armour_Reactive"",""cost"":49956620,""sku"":null},""128049276"":{""id"":128049276,""category"":""module"",""name"":""Viper_Armour_Grade3"",""cost"":122286,""sku"":null},""128049275"":{""id"":128049275,""category"":""module"",""name"":""Viper_Armour_Grade2"",""cost"":54350,""sku"":null},""128049274"":{""id"":128049274,""category"":""module"",""name"":""Viper_Armour_Grade1"",""cost"":0,""sku"":null},""128049277"":{""id"":128049277,""category"":""module"",""name"":""Viper_Armour_Mirrored"",""cost"":289004,""sku"":null},""128049278"":{""id"":128049278,""category"":""module"",""name"":""Viper_Armour_Reactive"",""cost"":320255,""sku"":null},""128049282"":{""id"":128049282,""category"":""module"",""name"":""CobraMkIII_Armour_Grade3"",""cost"":299206,""sku"":null},""128049281"":{""id"":128049281,""category"":""module"",""name"":""CobraMkIII_Armour_Grade2"",""cost"":132981,""sku"":null},""128049280"":{""id"":128049280,""category"":""module"",""name"":""CobraMkIII_Armour_Grade1"",""cost"":0,""sku"":null},""128049283"":{""id"":128049283,""category"":""module"",""name"":""CobraMkIII_Armour_Mirrored"",""cost"":698146,""sku"":null},""128049284"":{""id"":128049284,""category"":""module"",""name"":""CobraMkIII_Armour_Reactive"",""cost"":783586,""sku"":null},""128049252"":{""id"":128049252,""category"":""module"",""name"":""SideWinder_Armour_Grade3"",""cost"":76355,""sku"":null},""128049251"":{""id"":128049251,""category"":""module"",""name"":""SideWinder_Armour_Grade2"",""cost"":24336,""sku"":null},""128049250"":{""id"":128049250,""category"":""module"",""name"":""SideWinder_Armour_Grade1"",""cost"":0,""sku"":null},""128049253"":{""id"":128049253,""category"":""module"",""name"":""SideWinder_Armour_Mirrored"",""cost"":125544,""sku"":null},""128049254"":{""id"":128049254,""category"":""module"",""name"":""SideWinder_Armour_Reactive"",""cost"":132540,""sku"":null},""128672259"":{""id"":128672259,""category"":""module"",""name"":""Viper_MkIV_Armour_Grade3"",""cost"":374677,""sku"":null},""128672258"":{""id"":128672258,""category"":""module"",""name"":""Viper_MkIV_Armour_Grade2"",""cost"":166523,""sku"":null},""128672257"":{""id"":128672257,""category"":""module"",""name"":""Viper_MkIV_Armour_Grade1"",""cost"":0,""sku"":null},""128672260"":{""id"":128672260,""category"":""module"",""name"":""Viper_MkIV_Armour_Mirrored"",""cost"":885488,""sku"":null},""128672261"":{""id"":128672261,""category"":""module"",""name"":""Viper_MkIV_Armour_Reactive"",""cost"":981238,""sku"":null},""128049336"":{""id"":128049336,""category"":""module"",""name"":""Type9_Armour_Grade3"",""cost"":65498307,""sku"":null},""128049335"":{""id"":128049335,""category"":""module"",""name"":""Type9_Armour_Grade2"",""cost"":29110359,""sku"":null},""128049334"":{""id"":128049334,""category"":""module"",""name"":""Type9_Armour_Grade1"",""cost"":0,""sku"":null},""128049337"":{""id"":128049337,""category"":""module"",""name"":""Type9_Armour_Mirrored"",""cost"":154794333,""sku"":null},""128049338"":{""id"":128049338,""category"":""module"",""name"":""Type9_Armour_Reactive"",""cost"":171532790,""sku"":null},""128672266"":{""id"":128672266,""category"":""module"",""name"":""CobraMkIV_Armour_Grade3"",""cost"":654264,""sku"":null},""128672265"":{""id"":128672265,""category"":""module"",""name"":""CobraMkIV_Armour_Grade2"",""cost"":290784,""sku"":null},""128672264"":{""id"":128672264,""category"":""module"",""name"":""CobraMkIV_Armour_Grade1"",""cost"":0,""sku"":null},""128672267"":{""id"":128672267,""category"":""module"",""name"":""CobraMkIV_Armour_Mirrored"",""cost"":1526616,""sku"":null},""128672268"":{""id"":128672268,""category"":""module"",""name"":""CobraMkIV_Armour_Reactive"",""cost"":1713445,""sku"":null},""128662535"":{""id"":128662535,""category"":""module"",""name"":""Int_StellarBodyDiscoveryScanner_Standard"",""cost"":951,""sku"":null},""128064338"":{""id"":128064338,""category"":""module"",""name"":""Int_CargoRack_Size1_Class1"",""cost"":951,""sku"":null},""128666684"":{""id"":128666684,""category"":""module"",""name"":""Int_Refinery_Size1_Class1"",""cost"":5704,""sku"":null},""128666644"":{""id"":128666644,""category"":""module"",""name"":""Int_FuelScoop_Size1_Class1"",""cost"":294,""sku"":null},""128672317"":{""id"":128672317,""category"":""module"",""name"":""Int_PlanetApproachSuite"",""cost"":476,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128666704"":{""id"":128666704,""category"":""module"",""name"":""Int_FSDInterdictor_Size1_Class1"",""cost"":11408,""sku"":null},""128066532"":{""id"":128066532,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size1_Class1"",""cost"":571,""sku"":null},""128064263"":{""id"":128064263,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class1"",""cost"":1881,""sku"":null},""128666721"":{""id"":128666721,""category"":""module"",""name"":""Int_FSDInterdictor_Size2_Class5"",""cost"":2587221,""sku"":null},""128666717"":{""id"":128666717,""category"":""module"",""name"":""Int_FSDInterdictor_Size2_Class4"",""cost"":862407,""sku"":null},""128666713"":{""id"":128666713,""category"":""module"",""name"":""Int_FSDInterdictor_Size2_Class3"",""cost"":287469,""sku"":null},""128666720"":{""id"":128666720,""category"":""module"",""name"":""Int_FSDInterdictor_Size1_Class5"",""cost"":924008,""sku"":null},""128666712"":{""id"":128666712,""category"":""module"",""name"":""Int_FSDInterdictor_Size1_Class3"",""cost"":102668,""sku"":null},""128666709"":{""id"":128666709,""category"":""module"",""name"":""Int_FSDInterdictor_Size2_Class2"",""cost"":95823,""sku"":null},""128666708"":{""id"":128666708,""category"":""module"",""name"":""Int_FSDInterdictor_Size1_Class2"",""cost"":34223,""sku"":null},""128666705"":{""id"":128666705,""category"":""module"",""name"":""Int_FSDInterdictor_Size2_Class1"",""cost"":31941,""sku"":null},""128064252"":{""id"":128064252,""category"":""module"",""name"":""Int_Sensors_Size7_Class5"",""cost"":9251412,""sku"":null},""128064256"":{""id"":128064256,""category"":""module"",""name"":""Int_Sensors_Size8_Class4"",""cost"":10361581,""sku"":null},""128064251"":{""id"":128064251,""category"":""module"",""name"":""Int_Sensors_Size7_Class4"",""cost"":3700565,""sku"":null},""128064255"":{""id"":128064255,""category"":""module"",""name"":""Int_Sensors_Size8_Class3"",""cost"":4144633,""sku"":null},""128064250"":{""id"":128064250,""category"":""module"",""name"":""Int_Sensors_Size7_Class3"",""cost"":1480226,""sku"":null},""128064254"":{""id"":128064254,""category"":""module"",""name"":""Int_Sensors_Size8_Class2"",""cost"":1657853,""sku"":null},""128064249"":{""id"":128064249,""category"":""module"",""name"":""Int_Sensors_Size7_Class2"",""cost"":592091,""sku"":null},""128064253"":{""id"":128064253,""category"":""module"",""name"":""Int_Sensors_Size8_Class1"",""cost"":663141,""sku"":null},""128064248"":{""id"":128064248,""category"":""module"",""name"":""Int_Sensors_Size7_Class1"",""cost"":236836,""sku"":null},""128672293"":{""id"":128672293,""category"":""module"",""name"":""Int_BuggyBay_Size6_Class2"",""cost"":657072,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128672291"":{""id"":128672291,""category"":""module"",""name"":""Int_BuggyBay_Size4_Class2"",""cost"":82134,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128672292"":{""id"":128672292,""category"":""module"",""name"":""Int_BuggyBay_Size6_Class1"",""cost"":547560,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128672289"":{""id"":128672289,""category"":""module"",""name"":""Int_BuggyBay_Size2_Class2"",""cost"":20534,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128672290"":{""id"":128672290,""category"":""module"",""name"":""Int_BuggyBay_Size4_Class1"",""cost"":68445,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128672288"":{""id"":128672288,""category"":""module"",""name"":""Int_BuggyBay_Size2_Class1"",""cost"":17112,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128064272"":{""id"":128064272,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class5"",""cost"":482834,""sku"":null},""128064267"":{""id"":128064267,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class5"",""cost"":152313,""sku"":null},""128064271"":{""id"":128064271,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class4"",""cost"":160945,""sku"":null},""128064266"":{""id"":128064266,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class4"",""cost"":50771,""sku"":null},""128671333"":{""id"":128671333,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class3_Fast"",""cost"":80474,""sku"":null},""128064270"":{""id"":128064270,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class3"",""cost"":53649,""sku"":null},""128671332"":{""id"":128671332,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class3_Fast"",""cost"":25387,""sku"":null},""128671331"":{""id"":128671331,""category"":""module"",""name"":""Int_ShieldGenerator_Size1_Class3_Fast"",""cost"":7333,""sku"":null},""128064265"":{""id"":128064265,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class3"",""cost"":16924,""sku"":null},""128064269"":{""id"":128064269,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class2"",""cost"":17884,""sku"":null},""128064264"":{""id"":128064264,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class2"",""cost"":5642,""sku"":null},""128064268"":{""id"":128064268,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class1"",""cost"":5962,""sku"":null},""128064296"":{""id"":128064296,""category"":""module"",""name"":""Int_ShieldGenerator_Size8_Class4"",""cost"":51519593,""sku"":null},""128064291"":{""id"":128064291,""category"":""module"",""name"":""Int_ShieldGenerator_Size7_Class4"",""cost"":16252238,""sku"":null},""128671338"":{""id"":128671338,""category"":""module"",""name"":""Int_ShieldGenerator_Size8_Class3_Fast"",""cost"":25759797,""sku"":null},""128064295"":{""id"":128064295,""category"":""module"",""name"":""Int_ShieldGenerator_Size8_Class3"",""cost"":17173198,""sku"":null},""128671337"":{""id"":128671337,""category"":""module"",""name"":""Int_ShieldGenerator_Size7_Class3_Fast"",""cost"":8126119,""sku"":null},""128064290"":{""id"":128064290,""category"":""module"",""name"":""Int_ShieldGenerator_Size7_Class3"",""cost"":5417413,""sku"":null},""128064294"":{""id"":128064294,""category"":""module"",""name"":""Int_ShieldGenerator_Size8_Class2"",""cost"":5724400,""sku"":null},""128064293"":{""id"":128064293,""category"":""module"",""name"":""Int_ShieldGenerator_Size8_Class1"",""cost"":1908134,""sku"":null},""128064288"":{""id"":128064288,""category"":""module"",""name"":""Int_ShieldGenerator_Size7_Class1"",""cost"":601935,""sku"":null},""128064232"":{""id"":128064232,""category"":""module"",""name"":""Int_Sensors_Size3_Class5"",""cost"":150514,""sku"":null},""128064227"":{""id"":128064227,""category"":""module"",""name"":""Int_Sensors_Size2_Class5"",""cost"":53755,""sku"":null},""128064222"":{""id"":128064222,""category"":""module"",""name"":""Int_Sensors_Size1_Class5"",""cost"":19198,""sku"":null},""128064221"":{""id"":128064221,""category"":""module"",""name"":""Int_Sensors_Size1_Class4"",""cost"":7680,""sku"":null},""128064226"":{""id"":128064226,""category"":""module"",""name"":""Int_Sensors_Size2_Class4"",""cost"":21503,""sku"":null},""128064230"":{""id"":128064230,""category"":""module"",""name"":""Int_Sensors_Size3_Class3"",""cost"":24083,""sku"":null},""128064225"":{""id"":128064225,""category"":""module"",""name"":""Int_Sensors_Size2_Class3"",""cost"":8602,""sku"":null},""128064220"":{""id"":128064220,""category"":""module"",""name"":""Int_Sensors_Size1_Class3"",""cost"":3072,""sku"":null},""128064229"":{""id"":128064229,""category"":""module"",""name"":""Int_Sensors_Size3_Class2"",""cost"":9633,""sku"":null},""128064219"":{""id"":128064219,""category"":""module"",""name"":""Int_Sensors_Size1_Class2"",""cost"":1230,""sku"":null},""128064224"":{""id"":128064224,""category"":""module"",""name"":""Int_Sensors_Size2_Class2"",""cost"":3441,""sku"":null},""128064228"":{""id"":128064228,""category"":""module"",""name"":""Int_Sensors_Size3_Class1"",""cost"":3853,""sku"":null},""128064223"":{""id"":128064223,""category"":""module"",""name"":""Int_Sensors_Size2_Class1"",""cost"":1377,""sku"":null},""128064218"":{""id"":128064218,""category"":""module"",""name"":""Int_Sensors_Size1_Class1"",""cost"":492,""sku"":null},""128064112"":{""id"":128064112,""category"":""module"",""name"":""Int_Hyperdrive_Size3_Class5"",""cost"":482834,""sku"":null},""128064107"":{""id"":128064107,""category"":""module"",""name"":""Int_Hyperdrive_Size2_Class5"",""cost"":152313,""sku"":null},""128064111"":{""id"":128064111,""category"":""module"",""name"":""Int_Hyperdrive_Size3_Class4"",""cost"":160945,""sku"":null},""128064106"":{""id"":128064106,""category"":""module"",""name"":""Int_Hyperdrive_Size2_Class4"",""cost"":50771,""sku"":null},""128064110"":{""id"":128064110,""category"":""module"",""name"":""Int_Hyperdrive_Size3_Class3"",""cost"":53649,""sku"":null},""128064105"":{""id"":128064105,""category"":""module"",""name"":""Int_Hyperdrive_Size2_Class3"",""cost"":16924,""sku"":null},""128064109"":{""id"":128064109,""category"":""module"",""name"":""Int_Hyperdrive_Size3_Class2"",""cost"":17884,""sku"":null},""128064104"":{""id"":128064104,""category"":""module"",""name"":""Int_Hyperdrive_Size2_Class2"",""cost"":5642,""sku"":null},""128064108"":{""id"":128064108,""category"":""module"",""name"":""Int_Hyperdrive_Size3_Class1"",""cost"":5962,""sku"":null},""128064103"":{""id"":128064103,""category"":""module"",""name"":""Int_Hyperdrive_Size2_Class1"",""cost"":1881,""sku"":null},""128064192"":{""id"":128064192,""category"":""module"",""name"":""Int_PowerDistributor_Size3_Class5"",""cost"":150514,""sku"":null},""128064187"":{""id"":128064187,""category"":""module"",""name"":""Int_PowerDistributor_Size2_Class5"",""cost"":53755,""sku"":null},""128064182"":{""id"":128064182,""category"":""module"",""name"":""Int_PowerDistributor_Size1_Class5"",""cost"":19198,""sku"":null},""128064191"":{""id"":128064191,""category"":""module"",""name"":""Int_PowerDistributor_Size3_Class4"",""cost"":60206,""sku"":null},""128064190"":{""id"":128064190,""category"":""module"",""name"":""Int_PowerDistributor_Size3_Class3"",""cost"":24083,""sku"":null},""128064185"":{""id"":128064185,""category"":""module"",""name"":""Int_PowerDistributor_Size2_Class3"",""cost"":8602,""sku"":null},""128064180"":{""id"":128064180,""category"":""module"",""name"":""Int_PowerDistributor_Size1_Class3"",""cost"":3072,""sku"":null},""128064189"":{""id"":128064189,""category"":""module"",""name"":""Int_PowerDistributor_Size3_Class2"",""cost"":9633,""sku"":null},""128064179"":{""id"":128064179,""category"":""module"",""name"":""Int_PowerDistributor_Size1_Class2"",""cost"":1230,""sku"":null},""128064184"":{""id"":128064184,""category"":""module"",""name"":""Int_PowerDistributor_Size2_Class2"",""cost"":3441,""sku"":null},""128064188"":{""id"":128064188,""category"":""module"",""name"":""Int_PowerDistributor_Size3_Class1"",""cost"":3853,""sku"":null},""128064183"":{""id"":128064183,""category"":""module"",""name"":""Int_PowerDistributor_Size2_Class1"",""cost"":1377,""sku"":null},""128064178"":{""id"":128064178,""category"":""module"",""name"":""Int_PowerDistributor_Size1_Class1"",""cost"":492,""sku"":null},""128671272"":{""id"":128671272,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size1_Class4"",""cost"":4563,""sku"":null},""128671284"":{""id"":128671284,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size7_Class1"",""cost"":415804,""sku"":null},""128671280"":{""id"":128671280,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size5_Class2"",""cost"":92401,""sku"":null},""128671276"":{""id"":128671276,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size3_Class3"",""cost"":20534,""sku"":null},""128671271"":{""id"":128671271,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size1_Class3"",""cost"":2282,""sku"":null},""128671279"":{""id"":128671279,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size5_Class1"",""cost"":46201,""sku"":null},""128671275"":{""id"":128671275,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size3_Class2"",""cost"":10267,""sku"":null},""128671270"":{""id"":128671270,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size1_Class2"",""cost"":1141,""sku"":null},""128671274"":{""id"":128671274,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size3_Class1"",""cost"":5134,""sku"":null},""128671269"":{""id"":128671269,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size1_Class1"",""cost"":571,""sku"":null},""128064345"":{""id"":128064345,""category"":""module"",""name"":""Int_CargoRack_Size8_Class1"",""cost"":3640767,""sku"":null},""128064344"":{""id"":128064344,""category"":""module"",""name"":""Int_CargoRack_Size7_Class1"",""cost"":1120236,""sku"":null},""128064343"":{""id"":128064343,""category"":""module"",""name"":""Int_CargoRack_Size6_Class1"",""cost"":344689,""sku"":null},""128064342"":{""id"":128064342,""category"":""module"",""name"":""Int_CargoRack_Size5_Class1"",""cost"":106058,""sku"":null},""128064341"":{""id"":128064341,""category"":""module"",""name"":""Int_CargoRack_Size4_Class1"",""cost"":32634,""sku"":null},""128064340"":{""id"":128064340,""category"":""module"",""name"":""Int_CargoRack_Size3_Class1"",""cost"":10042,""sku"":null},""128064339"":{""id"":128064339,""category"":""module"",""name"":""Int_CargoRack_Size2_Class1"",""cost"":3090,""sku"":null},""128671288"":{""id"":128671288,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size7_Class5"",""cost"":6652854,""sku"":null},""128671283"":{""id"":128671283,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size5_Class5"",""cost"":739206,""sku"":null},""128671278"":{""id"":128671278,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size3_Class5"",""cost"":82134,""sku"":null},""128671286"":{""id"":128671286,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size7_Class3"",""cost"":1663214,""sku"":null},""128671277"":{""id"":128671277,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size3_Class4"",""cost"":41067,""sku"":null},""128671285"":{""id"":128671285,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size7_Class2"",""cost"":831607,""sku"":null},""128671281"":{""id"":128671281,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size5_Class3"",""cost"":184802,""sku"":null},""128064247"":{""id"":128064247,""category"":""module"",""name"":""Int_Sensors_Size6_Class5"",""cost"":3304076,""sku"":null},""128064237"":{""id"":128064237,""category"":""module"",""name"":""Int_Sensors_Size4_Class5"",""cost"":421439,""sku"":null},""128064246"":{""id"":128064246,""category"":""module"",""name"":""Int_Sensors_Size6_Class4"",""cost"":1321631,""sku"":null},""128064241"":{""id"":128064241,""category"":""module"",""name"":""Int_Sensors_Size5_Class4"",""cost"":472011,""sku"":null},""128064245"":{""id"":128064245,""category"":""module"",""name"":""Int_Sensors_Size6_Class3"",""cost"":528653,""sku"":null},""128064244"":{""id"":128064244,""category"":""module"",""name"":""Int_Sensors_Size6_Class2"",""cost"":211461,""sku"":null},""128064240"":{""id"":128064240,""category"":""module"",""name"":""Int_Sensors_Size5_Class3"",""cost"":188805,""sku"":null},""128064235"":{""id"":128064235,""category"":""module"",""name"":""Int_Sensors_Size4_Class3"",""cost"":67430,""sku"":null},""128064239"":{""id"":128064239,""category"":""module"",""name"":""Int_Sensors_Size5_Class2"",""cost"":75522,""sku"":null},""128064243"":{""id"":128064243,""category"":""module"",""name"":""Int_Sensors_Size6_Class1"",""cost"":84585,""sku"":null},""128064234"":{""id"":128064234,""category"":""module"",""name"":""Int_Sensors_Size4_Class2"",""cost"":26973,""sku"":null},""128064238"":{""id"":128064238,""category"":""module"",""name"":""Int_Sensors_Size5_Class1"",""cost"":30209,""sku"":null},""128064233"":{""id"":128064233,""category"":""module"",""name"":""Int_Sensors_Size4_Class1"",""cost"":10789,""sku"":null},""128064077"":{""id"":128064077,""category"":""module"",""name"":""Int_Engine_Size3_Class5"",""cost"":482834,""sku"":null},""128064072"":{""id"":128064072,""category"":""module"",""name"":""Int_Engine_Size2_Class5"",""cost"":152313,""sku"":null},""128064071"":{""id"":128064071,""category"":""module"",""name"":""Int_Engine_Size2_Class4"",""cost"":50771,""sku"":null},""128064075"":{""id"":128064075,""category"":""module"",""name"":""Int_Engine_Size3_Class3"",""cost"":53649,""sku"":null},""128064070"":{""id"":128064070,""category"":""module"",""name"":""Int_Engine_Size2_Class3"",""cost"":16924,""sku"":null},""128064074"":{""id"":128064074,""category"":""module"",""name"":""Int_Engine_Size3_Class2"",""cost"":17884,""sku"":null},""128064069"":{""id"":128064069,""category"":""module"",""name"":""Int_Engine_Size2_Class2"",""cost"":5642,""sku"":null},""128064073"":{""id"":128064073,""category"":""module"",""name"":""Int_Engine_Size3_Class1"",""cost"":5962,""sku"":null},""128064068"":{""id"":128064068,""category"":""module"",""name"":""Int_Engine_Size2_Class1"",""cost"":1881,""sku"":null},""128064042"":{""id"":128064042,""category"":""module"",""name"":""Int_Powerplant_Size3_Class5"",""cost"":482834,""sku"":null},""128064041"":{""id"":128064041,""category"":""module"",""name"":""Int_Powerplant_Size3_Class4"",""cost"":160945,""sku"":null},""128064040"":{""id"":128064040,""category"":""module"",""name"":""Int_Powerplant_Size3_Class3"",""cost"":53649,""sku"":null},""128064035"":{""id"":128064035,""category"":""module"",""name"":""Int_Powerplant_Size2_Class3"",""cost"":16924,""sku"":null},""128064039"":{""id"":128064039,""category"":""module"",""name"":""Int_Powerplant_Size3_Class2"",""cost"":17884,""sku"":null},""128064034"":{""id"":128064034,""category"":""module"",""name"":""Int_Powerplant_Size2_Class2"",""cost"":5642,""sku"":null},""128064038"":{""id"":128064038,""category"":""module"",""name"":""Int_Powerplant_Size3_Class1"",""cost"":5962,""sku"":null},""128064033"":{""id"":128064033,""category"":""module"",""name"":""Int_Powerplant_Size2_Class1"",""cost"":1881,""sku"":null},""128064197"":{""id"":128064197,""category"":""module"",""name"":""Int_PowerDistributor_Size4_Class5"",""cost"":421439,""sku"":null},""128064206"":{""id"":128064206,""category"":""module"",""name"":""Int_PowerDistributor_Size6_Class4"",""cost"":1321631,""sku"":null},""128064201"":{""id"":128064201,""category"":""module"",""name"":""Int_PowerDistributor_Size5_Class4"",""cost"":472011,""sku"":null},""128064205"":{""id"":128064205,""category"":""module"",""name"":""Int_PowerDistributor_Size6_Class3"",""cost"":528653,""sku"":null},""128064204"":{""id"":128064204,""category"":""module"",""name"":""Int_PowerDistributor_Size6_Class2"",""cost"":211461,""sku"":null},""128064200"":{""id"":128064200,""category"":""module"",""name"":""Int_PowerDistributor_Size5_Class3"",""cost"":188805,""sku"":null},""128064195"":{""id"":128064195,""category"":""module"",""name"":""Int_PowerDistributor_Size4_Class3"",""cost"":67430,""sku"":null},""128064199"":{""id"":128064199,""category"":""module"",""name"":""Int_PowerDistributor_Size5_Class2"",""cost"":75522,""sku"":null},""128064203"":{""id"":128064203,""category"":""module"",""name"":""Int_PowerDistributor_Size6_Class1"",""cost"":84585,""sku"":null},""128064194"":{""id"":128064194,""category"":""module"",""name"":""Int_PowerDistributor_Size4_Class2"",""cost"":26973,""sku"":null},""128064198"":{""id"":128064198,""category"":""module"",""name"":""Int_PowerDistributor_Size5_Class1"",""cost"":30209,""sku"":null},""128064193"":{""id"":128064193,""category"":""module"",""name"":""Int_PowerDistributor_Size4_Class1"",""cost"":10789,""sku"":null},""128666680"":{""id"":128666680,""category"":""module"",""name"":""Int_FuelScoop_Size5_Class5"",""cost"":8625681,""sku"":null},""128666665"":{""id"":128666665,""category"":""module"",""name"":""Int_FuelScoop_Size6_Class3"",""cost"":1708964,""sku"":null},""128666672"":{""id"":128666672,""category"":""module"",""name"":""Int_FuelScoop_Size5_Class4"",""cost"":2156421,""sku"":null},""128666671"":{""id"":128666671,""category"":""module"",""name"":""Int_FuelScoop_Size4_Class4"",""cost"":680259,""sku"":null},""128666664"":{""id"":128666664,""category"":""module"",""name"":""Int_FuelScoop_Size5_Class3"",""cost"":539106,""sku"":null},""128666663"":{""id"":128666663,""category"":""module"",""name"":""Int_FuelScoop_Size4_Class3"",""cost"":170065,""sku"":null},""128666657"":{""id"":128666657,""category"":""module"",""name"":""Int_FuelScoop_Size6_Class2"",""cost"":427241,""sku"":null},""128666656"":{""id"":128666656,""category"":""module"",""name"":""Int_FuelScoop_Size5_Class2"",""cost"":134776,""sku"":null},""128666649"":{""id"":128666649,""category"":""module"",""name"":""Int_FuelScoop_Size6_Class1"",""cost"":102539,""sku"":null},""128666655"":{""id"":128666655,""category"":""module"",""name"":""Int_FuelScoop_Size4_Class2"",""cost"":42516,""sku"":null},""128666648"":{""id"":128666648,""category"":""module"",""name"":""Int_FuelScoop_Size5_Class1"",""cost"":32346,""sku"":null},""128666647"":{""id"":128666647,""category"":""module"",""name"":""Int_FuelScoop_Size4_Class1"",""cost"":10205,""sku"":null},""128668546"":{""id"":128668546,""category"":""module"",""name"":""Int_HullReinforcement_Size5_Class2"",""cost"":427782,""sku"":null},""128668544"":{""id"":128668544,""category"":""module"",""name"":""Int_HullReinforcement_Size4_Class2"",""cost"":185372,""sku"":null},""128668540"":{""id"":128668540,""category"":""module"",""name"":""Int_HullReinforcement_Size2_Class2"",""cost"":34223,""sku"":null},""128064147"":{""id"":128064147,""category"":""module"",""name"":""Int_LifeSupport_Size2_Class5"",""cost"":53755,""sku"":null},""128064142"":{""id"":128064142,""category"":""module"",""name"":""Int_LifeSupport_Size1_Class5"",""cost"":19198,""sku"":null},""128064141"":{""id"":128064141,""category"":""module"",""name"":""Int_LifeSupport_Size1_Class4"",""cost"":7680,""sku"":null},""128064146"":{""id"":128064146,""category"":""module"",""name"":""Int_LifeSupport_Size2_Class4"",""cost"":21503,""sku"":null},""128064150"":{""id"":128064150,""category"":""module"",""name"":""Int_LifeSupport_Size3_Class3"",""cost"":24083,""sku"":null},""128064145"":{""id"":128064145,""category"":""module"",""name"":""Int_LifeSupport_Size2_Class3"",""cost"":8602,""sku"":null},""128064140"":{""id"":128064140,""category"":""module"",""name"":""Int_LifeSupport_Size1_Class3"",""cost"":3072,""sku"":null},""128064149"":{""id"":128064149,""category"":""module"",""name"":""Int_LifeSupport_Size3_Class2"",""cost"":9633,""sku"":null},""128064139"":{""id"":128064139,""category"":""module"",""name"":""Int_LifeSupport_Size1_Class2"",""cost"":1230,""sku"":null},""128064144"":{""id"":128064144,""category"":""module"",""name"":""Int_LifeSupport_Size2_Class2"",""cost"":3441,""sku"":null},""128064148"":{""id"":128064148,""category"":""module"",""name"":""Int_LifeSupport_Size3_Class1"",""cost"":3853,""sku"":null},""128064143"":{""id"":128064143,""category"":""module"",""name"":""Int_LifeSupport_Size2_Class1"",""cost"":1377,""sku"":null},""128064138"":{""id"":128064138,""category"":""module"",""name"":""Int_LifeSupport_Size1_Class1"",""cost"":492,""sku"":null},""128663561"":{""id"":128663561,""category"":""module"",""name"":""Int_StellarBodyDiscoveryScanner_Advanced"",""cost"":1468716,""sku"":null},""128663560"":{""id"":128663560,""category"":""module"",""name"":""Int_StellarBodyDiscoveryScanner_Intermediate"",""cost"":480066,""sku"":null},""128666634"":{""id"":128666634,""category"":""module"",""name"":""Int_DetailedSurfaceScanner_Tiny"",""cost"":237657,""sku"":null},""128064327"":{""id"":128064327,""category"":""module"",""name"":""Int_ShieldCellBank_Size6_Class5"",""cost"":3304076,""sku"":null},""128064322"":{""id"":128064322,""category"":""module"",""name"":""Int_ShieldCellBank_Size5_Class5"",""cost"":1180027,""sku"":null},""128064317"":{""id"":128064317,""category"":""module"",""name"":""Int_ShieldCellBank_Size4_Class5"",""cost"":421439,""sku"":null},""128064326"":{""id"":128064326,""category"":""module"",""name"":""Int_ShieldCellBank_Size6_Class4"",""cost"":1321631,""sku"":null},""128064321"":{""id"":128064321,""category"":""module"",""name"":""Int_ShieldCellBank_Size5_Class4"",""cost"":472011,""sku"":null},""128064316"":{""id"":128064316,""category"":""module"",""name"":""Int_ShieldCellBank_Size4_Class4"",""cost"":168576,""sku"":null},""128064324"":{""id"":128064324,""category"":""module"",""name"":""Int_ShieldCellBank_Size6_Class2"",""cost"":211461,""sku"":null},""128064320"":{""id"":128064320,""category"":""module"",""name"":""Int_ShieldCellBank_Size5_Class3"",""cost"":188805,""sku"":null},""128064315"":{""id"":128064315,""category"":""module"",""name"":""Int_ShieldCellBank_Size4_Class3"",""cost"":67430,""sku"":null},""128064319"":{""id"":128064319,""category"":""module"",""name"":""Int_ShieldCellBank_Size5_Class2"",""cost"":75522,""sku"":null},""128064323"":{""id"":128064323,""category"":""module"",""name"":""Int_ShieldCellBank_Size6_Class1"",""cost"":84585,""sku"":null},""128064314"":{""id"":128064314,""category"":""module"",""name"":""Int_ShieldCellBank_Size4_Class2"",""cost"":26973,""sku"":null},""128064318"":{""id"":128064318,""category"":""module"",""name"":""Int_ShieldCellBank_Size5_Class1"",""cost"":30209,""sku"":null},""128064313"":{""id"":128064313,""category"":""module"",""name"":""Int_ShieldCellBank_Size4_Class1"",""cost"":10789,""sku"":null},""128668545"":{""id"":128668545,""category"":""module"",""name"":""Int_HullReinforcement_Size5_Class1"",""cost"":142594,""sku"":null},""128668543"":{""id"":128668543,""category"":""module"",""name"":""Int_HullReinforcement_Size4_Class1"",""cost"":61791,""sku"":null},""128668541"":{""id"":128668541,""category"":""module"",""name"":""Int_HullReinforcement_Size3_Class1"",""cost"":26618,""sku"":null},""128668539"":{""id"":128668539,""category"":""module"",""name"":""Int_HullReinforcement_Size2_Class1"",""cost"":11408,""sku"":null},""128668537"":{""id"":128668537,""category"":""module"",""name"":""Int_HullReinforcement_Size1_Class1"",""cost"":4754,""sku"":null},""128671232"":{""id"":128671232,""category"":""module"",""name"":""Int_DroneControl_Collection_Size1_Class4"",""cost"":4563,""sku"":null},""128671244"":{""id"":128671244,""category"":""module"",""name"":""Int_DroneControl_Collection_Size7_Class1"",""cost"":415804,""sku"":null},""128671240"":{""id"":128671240,""category"":""module"",""name"":""Int_DroneControl_Collection_Size5_Class2"",""cost"":92401,""sku"":null},""128671236"":{""id"":128671236,""category"":""module"",""name"":""Int_DroneControl_Collection_Size3_Class3"",""cost"":20534,""sku"":null},""128671231"":{""id"":128671231,""category"":""module"",""name"":""Int_DroneControl_Collection_Size1_Class3"",""cost"":2282,""sku"":null},""128671239"":{""id"":128671239,""category"":""module"",""name"":""Int_DroneControl_Collection_Size5_Class1"",""cost"":46201,""sku"":null},""128671235"":{""id"":128671235,""category"":""module"",""name"":""Int_DroneControl_Collection_Size3_Class2"",""cost"":10267,""sku"":null},""128671230"":{""id"":128671230,""category"":""module"",""name"":""Int_DroneControl_Collection_Size1_Class2"",""cost"":1141,""sku"":null},""128671234"":{""id"":128671234,""category"":""module"",""name"":""Int_DroneControl_Collection_Size3_Class1"",""cost"":5134,""sku"":null},""128671229"":{""id"":128671229,""category"":""module"",""name"":""Int_DroneControl_Collection_Size1_Class1"",""cost"":571,""sku"":null},""128064162"":{""id"":128064162,""category"":""module"",""name"":""Int_LifeSupport_Size5_Class5"",""cost"":1180027,""sku"":null},""128064157"":{""id"":128064157,""category"":""module"",""name"":""Int_LifeSupport_Size4_Class5"",""cost"":421439,""sku"":null},""128064166"":{""id"":128064166,""category"":""module"",""name"":""Int_LifeSupport_Size6_Class4"",""cost"":1321631,""sku"":null},""128064156"":{""id"":128064156,""category"":""module"",""name"":""Int_LifeSupport_Size4_Class4"",""cost"":168576,""sku"":null},""128064164"":{""id"":128064164,""category"":""module"",""name"":""Int_LifeSupport_Size6_Class2"",""cost"":211461,""sku"":null},""128064160"":{""id"":128064160,""category"":""module"",""name"":""Int_LifeSupport_Size5_Class3"",""cost"":188805,""sku"":null},""128064155"":{""id"":128064155,""category"":""module"",""name"":""Int_LifeSupport_Size4_Class3"",""cost"":67430,""sku"":null},""128064159"":{""id"":128064159,""category"":""module"",""name"":""Int_LifeSupport_Size5_Class2"",""cost"":75522,""sku"":null},""128064163"":{""id"":128064163,""category"":""module"",""name"":""Int_LifeSupport_Size6_Class1"",""cost"":84585,""sku"":null},""128064154"":{""id"":128064154,""category"":""module"",""name"":""Int_LifeSupport_Size4_Class2"",""cost"":26973,""sku"":null},""128064158"":{""id"":128064158,""category"":""module"",""name"":""Int_LifeSupport_Size5_Class1"",""cost"":30209,""sku"":null},""128064153"":{""id"":128064153,""category"":""module"",""name"":""Int_LifeSupport_Size4_Class1"",""cost"":10789,""sku"":null},""128667605"":{""id"":128667605,""category"":""module"",""name"":""Int_Repairer_Size8_Class1"",""cost"":581992,""sku"":null},""128667611"":{""id"":128667611,""category"":""module"",""name"":""Int_Repairer_Size6_Class2"",""cost"":538881,""sku"":null},""128667604"":{""id"":128667604,""category"":""module"",""name"":""Int_Repairer_Size7_Class1"",""cost"":323329,""sku"":null},""128667610"":{""id"":128667610,""category"":""module"",""name"":""Int_Repairer_Size5_Class2"",""cost"":299379,""sku"":null},""128667616"":{""id"":128667616,""category"":""module"",""name"":""Int_Repairer_Size3_Class3"",""cost"":277203,""sku"":null},""128667623"":{""id"":128667623,""category"":""module"",""name"":""Int_Repairer_Size2_Class4"",""cost"":462004,""sku"":null},""128667615"":{""id"":128667615,""category"":""module"",""name"":""Int_Repairer_Size2_Class3"",""cost"":154002,""sku"":null},""128667609"":{""id"":128667609,""category"":""module"",""name"":""Int_Repairer_Size4_Class2"",""cost"":166322,""sku"":null},""128667603"":{""id"":128667603,""category"":""module"",""name"":""Int_Repairer_Size6_Class1"",""cost"":179628,""sku"":null},""128667602"":{""id"":128667602,""category"":""module"",""name"":""Int_Repairer_Size5_Class1"",""cost"":99793,""sku"":null},""128667608"":{""id"":128667608,""category"":""module"",""name"":""Int_Repairer_Size3_Class2"",""cost"":92401,""sku"":null},""128667614"":{""id"":128667614,""category"":""module"",""name"":""Int_Repairer_Size1_Class3"",""cost"":85557,""sku"":null},""128667601"":{""id"":128667601,""category"":""module"",""name"":""Int_Repairer_Size4_Class1"",""cost"":55441,""sku"":null},""128667607"":{""id"":128667607,""category"":""module"",""name"":""Int_Repairer_Size2_Class2"",""cost"":51334,""sku"":null},""128667600"":{""id"":128667600,""category"":""module"",""name"":""Int_Repairer_Size3_Class1"",""cost"":30801,""sku"":null},""128668536"":{""id"":128668536,""category"":""module"",""name"":""Hpt_ShieldBooster_Size0_Class5"",""cost"":267126,""sku"":null},""128668535"":{""id"":128668535,""category"":""module"",""name"":""Hpt_ShieldBooster_Size0_Class4"",""cost"":115977,""sku"":null},""128668534"":{""id"":128668534,""category"":""module"",""name"":""Hpt_ShieldBooster_Size0_Class3"",""cost"":50384,""sku"":null},""128668533"":{""id"":128668533,""category"":""module"",""name"":""Hpt_ShieldBooster_Size0_Class2"",""cost"":21865,""sku"":null},""128668532"":{""id"":128668532,""category"":""module"",""name"":""Hpt_ShieldBooster_Size0_Class1"",""cost"":9507,""sku"":null},""128064057"":{""id"":128064057,""category"":""module"",""name"":""Int_Powerplant_Size6_Class5"",""cost"":15380667,""sku"":null},""128064052"":{""id"":128064052,""category"":""module"",""name"":""Int_Powerplant_Size5_Class5"",""cost"":4851946,""sku"":null},""128064047"":{""id"":128064047,""category"":""module"",""name"":""Int_Powerplant_Size4_Class5"",""cost"":1530583,""sku"":null},""128064056"":{""id"":128064056,""category"":""module"",""name"":""Int_Powerplant_Size6_Class4"",""cost"":5126889,""sku"":null},""128064051"":{""id"":128064051,""category"":""module"",""name"":""Int_Powerplant_Size5_Class4"",""cost"":1617316,""sku"":null},""128064046"":{""id"":128064046,""category"":""module"",""name"":""Int_Powerplant_Size4_Class4"",""cost"":510194,""sku"":null},""128064054"":{""id"":128064054,""category"":""module"",""name"":""Int_Powerplant_Size6_Class2"",""cost"":569655,""sku"":null},""128064050"":{""id"":128064050,""category"":""module"",""name"":""Int_Powerplant_Size5_Class3"",""cost"":539106,""sku"":null},""128064045"":{""id"":128064045,""category"":""module"",""name"":""Int_Powerplant_Size4_Class3"",""cost"":170065,""sku"":null},""128064049"":{""id"":128064049,""category"":""module"",""name"":""Int_Powerplant_Size5_Class2"",""cost"":179702,""sku"":null},""128064053"":{""id"":128064053,""category"":""module"",""name"":""Int_Powerplant_Size6_Class1"",""cost"":189885,""sku"":null},""128064044"":{""id"":128064044,""category"":""module"",""name"":""Int_Powerplant_Size4_Class2"",""cost"":56689,""sku"":null},""128064048"":{""id"":128064048,""category"":""module"",""name"":""Int_Powerplant_Size5_Class1"",""cost"":59901,""sku"":null},""128064043"":{""id"":128064043,""category"":""module"",""name"":""Int_Powerplant_Size4_Class1"",""cost"":18897,""sku"":null},""128064092"":{""id"":128064092,""category"":""module"",""name"":""Int_Engine_Size6_Class5"",""cost"":15380667,""sku"":null},""128064087"":{""id"":128064087,""category"":""module"",""name"":""Int_Engine_Size5_Class5"",""cost"":4851946,""sku"":null},""128064091"":{""id"":128064091,""category"":""module"",""name"":""Int_Engine_Size6_Class4"",""cost"":5126889,""sku"":null},""128064086"":{""id"":128064086,""category"":""module"",""name"":""Int_Engine_Size5_Class4"",""cost"":1617316,""sku"":null},""128064081"":{""id"":128064081,""category"":""module"",""name"":""Int_Engine_Size4_Class4"",""cost"":510194,""sku"":null},""128064090"":{""id"":128064090,""category"":""module"",""name"":""Int_Engine_Size6_Class3"",""cost"":1708964,""sku"":null},""128064089"":{""id"":128064089,""category"":""module"",""name"":""Int_Engine_Size6_Class2"",""cost"":569655,""sku"":null},""128064085"":{""id"":128064085,""category"":""module"",""name"":""Int_Engine_Size5_Class3"",""cost"":539106,""sku"":null},""128064080"":{""id"":128064080,""category"":""module"",""name"":""Int_Engine_Size4_Class3"",""cost"":170065,""sku"":null},""128064084"":{""id"":128064084,""category"":""module"",""name"":""Int_Engine_Size5_Class2"",""cost"":179702,""sku"":null},""128064088"":{""id"":128064088,""category"":""module"",""name"":""Int_Engine_Size6_Class1"",""cost"":189885,""sku"":null},""128064079"":{""id"":128064079,""category"":""module"",""name"":""Int_Engine_Size4_Class2"",""cost"":56689,""sku"":null},""128064083"":{""id"":128064083,""category"":""module"",""name"":""Int_Engine_Size5_Class1"",""cost"":59901,""sku"":null},""128064078"":{""id"":128064078,""category"":""module"",""name"":""Int_Engine_Size4_Class1"",""cost"":18897,""sku"":null},""128666697"":{""id"":128666697,""category"":""module"",""name"":""Int_Refinery_Size2_Class4"",""cost"":323403,""sku"":null},""128666700"":{""id"":128666700,""category"":""module"",""name"":""Int_Refinery_Size1_Class5"",""cost"":462004,""sku"":null},""128666693"":{""id"":128666693,""category"":""module"",""name"":""Int_Refinery_Size2_Class3"",""cost"":107801,""sku"":null},""128666696"":{""id"":128666696,""category"":""module"",""name"":""Int_Refinery_Size1_Class4"",""cost"":154002,""sku"":null},""128666689"":{""id"":128666689,""category"":""module"",""name"":""Int_Refinery_Size2_Class2"",""cost"":35934,""sku"":null},""128666692"":{""id"":128666692,""category"":""module"",""name"":""Int_Refinery_Size1_Class3"",""cost"":51334,""sku"":null},""128666688"":{""id"":128666688,""category"":""module"",""name"":""Int_Refinery_Size1_Class2"",""cost"":17112,""sku"":null},""128666685"":{""id"":128666685,""category"":""module"",""name"":""Int_Refinery_Size2_Class1"",""cost"":11978,""sku"":null},""128064122"":{""id"":128064122,""category"":""module"",""name"":""Int_Hyperdrive_Size5_Class5"",""cost"":4851946,""sku"":null},""128064117"":{""id"":128064117,""category"":""module"",""name"":""Int_Hyperdrive_Size4_Class5"",""cost"":1530583,""sku"":null},""128064126"":{""id"":128064126,""category"":""module"",""name"":""Int_Hyperdrive_Size6_Class4"",""cost"":5126889,""sku"":null},""128064121"":{""id"":128064121,""category"":""module"",""name"":""Int_Hyperdrive_Size5_Class4"",""cost"":1617316,""sku"":null},""128064116"":{""id"":128064116,""category"":""module"",""name"":""Int_Hyperdrive_Size4_Class4"",""cost"":510194,""sku"":null},""128064125"":{""id"":128064125,""category"":""module"",""name"":""Int_Hyperdrive_Size6_Class3"",""cost"":1708964,""sku"":null},""128064124"":{""id"":128064124,""category"":""module"",""name"":""Int_Hyperdrive_Size6_Class2"",""cost"":569655,""sku"":null},""128064120"":{""id"":128064120,""category"":""module"",""name"":""Int_Hyperdrive_Size5_Class3"",""cost"":539106,""sku"":null},""128064115"":{""id"":128064115,""category"":""module"",""name"":""Int_Hyperdrive_Size4_Class3"",""cost"":170065,""sku"":null},""128064119"":{""id"":128064119,""category"":""module"",""name"":""Int_Hyperdrive_Size5_Class2"",""cost"":179702,""sku"":null},""128064123"":{""id"":128064123,""category"":""module"",""name"":""Int_Hyperdrive_Size6_Class1"",""cost"":189885,""sku"":null},""128064114"":{""id"":128064114,""category"":""module"",""name"":""Int_Hyperdrive_Size4_Class2"",""cost"":56689,""sku"":null},""128064118"":{""id"":128064118,""category"":""module"",""name"":""Int_Hyperdrive_Size5_Class1"",""cost"":59901,""sku"":null},""128064282"":{""id"":128064282,""category"":""module"",""name"":""Int_ShieldGenerator_Size5_Class5"",""cost"":4851946,""sku"":null},""128064277"":{""id"":128064277,""category"":""module"",""name"":""Int_ShieldGenerator_Size4_Class5"",""cost"":1530583,""sku"":null},""128064286"":{""id"":128064286,""category"":""module"",""name"":""Int_ShieldGenerator_Size6_Class4"",""cost"":5126889,""sku"":null},""128064281"":{""id"":128064281,""category"":""module"",""name"":""Int_ShieldGenerator_Size5_Class4"",""cost"":1617316,""sku"":null},""128064276"":{""id"":128064276,""category"":""module"",""name"":""Int_ShieldGenerator_Size4_Class4"",""cost"":510194,""sku"":null},""128671336"":{""id"":128671336,""category"":""module"",""name"":""Int_ShieldGenerator_Size6_Class3_Fast"",""cost"":2563445,""sku"":null},""128064285"":{""id"":128064285,""category"":""module"",""name"":""Int_ShieldGenerator_Size6_Class3"",""cost"":1708964,""sku"":null},""128671335"":{""id"":128671335,""category"":""module"",""name"":""Int_ShieldGenerator_Size5_Class3_Fast"",""cost"":808658,""sku"":null},""128064284"":{""id"":128064284,""category"":""module"",""name"":""Int_ShieldGenerator_Size6_Class2"",""cost"":569655,""sku"":null},""128064280"":{""id"":128064280,""category"":""module"",""name"":""Int_ShieldGenerator_Size5_Class3"",""cost"":539106,""sku"":null},""128671334"":{""id"":128671334,""category"":""module"",""name"":""Int_ShieldGenerator_Size4_Class3_Fast"",""cost"":255098,""sku"":null},""128064275"":{""id"":128064275,""category"":""module"",""name"":""Int_ShieldGenerator_Size4_Class3"",""cost"":170065,""sku"":null},""128064279"":{""id"":128064279,""category"":""module"",""name"":""Int_ShieldGenerator_Size5_Class2"",""cost"":179702,""sku"":null},""128671248"":{""id"":128671248,""category"":""module"",""name"":""Int_DroneControl_Collection_Size7_Class5"",""cost"":6652854,""sku"":null},""128671243"":{""id"":128671243,""category"":""module"",""name"":""Int_DroneControl_Collection_Size5_Class5"",""cost"":739206,""sku"":null},""128671238"":{""id"":128671238,""category"":""module"",""name"":""Int_DroneControl_Collection_Size3_Class5"",""cost"":82134,""sku"":null},""128671247"":{""id"":128671247,""category"":""module"",""name"":""Int_DroneControl_Collection_Size7_Class4"",""cost"":3326427,""sku"":null},""128671242"":{""id"":128671242,""category"":""module"",""name"":""Int_DroneControl_Collection_Size5_Class4"",""cost"":369603,""sku"":null},""128671246"":{""id"":128671246,""category"":""module"",""name"":""Int_DroneControl_Collection_Size7_Class3"",""cost"":1663214,""sku"":null},""128671237"":{""id"":128671237,""category"":""module"",""name"":""Int_DroneControl_Collection_Size3_Class4"",""cost"":41067,""sku"":null},""128671245"":{""id"":128671245,""category"":""module"",""name"":""Int_DroneControl_Collection_Size7_Class2"",""cost"":831607,""sku"":null},""128671241"":{""id"":128671241,""category"":""module"",""name"":""Int_DroneControl_Collection_Size5_Class3"",""cost"":184802,""sku"":null},""128064353"":{""id"":128064353,""category"":""module"",""name"":""Int_FuelTank_Size8_Class3"",""cost"":5160401,""sku"":null},""128064352"":{""id"":128064352,""category"":""module"",""name"":""Int_FuelTank_Size7_Class3"",""cost"":1692982,""sku"":null},""128064351"":{""id"":128064351,""category"":""module"",""name"":""Int_FuelTank_Size6_Class3"",""cost"":324712,""sku"":null},""128064350"":{""id"":128064350,""category"":""module"",""name"":""Int_FuelTank_Size5_Class3"",""cost"":92928,""sku"":null},""128064349"":{""id"":128064349,""category"":""module"",""name"":""Int_FuelTank_Size4_Class3"",""cost"":23513,""sku"":null},""128064348"":{""id"":128064348,""category"":""module"",""name"":""Int_FuelTank_Size3_Class3"",""cost"":6715,""sku"":null},""128064347"":{""id"":128064347,""category"":""module"",""name"":""Int_FuelTank_Size2_Class3"",""cost"":3565,""sku"":null},""128064346"":{""id"":128064346,""category"":""module"",""name"":""Int_FuelTank_Size1_Class3"",""cost"":951,""sku"":null},""128064217"":{""id"":128064217,""category"":""module"",""name"":""Int_PowerDistributor_Size8_Class5"",""cost"":25903953,""sku"":null},""128064216"":{""id"":128064216,""category"":""module"",""name"":""Int_PowerDistributor_Size8_Class4"",""cost"":10361581,""sku"":null},""128064211"":{""id"":128064211,""category"":""module"",""name"":""Int_PowerDistributor_Size7_Class4"",""cost"":3700565,""sku"":null},""128064210"":{""id"":128064210,""category"":""module"",""name"":""Int_PowerDistributor_Size7_Class3"",""cost"":1480226,""sku"":null},""128064214"":{""id"":128064214,""category"":""module"",""name"":""Int_PowerDistributor_Size8_Class2"",""cost"":1657853,""sku"":null},""128064209"":{""id"":128064209,""category"":""module"",""name"":""Int_PowerDistributor_Size7_Class2"",""cost"":592091,""sku"":null},""128064213"":{""id"":128064213,""category"":""module"",""name"":""Int_PowerDistributor_Size8_Class1"",""cost"":663141,""sku"":null},""128064208"":{""id"":128064208,""category"":""module"",""name"":""Int_PowerDistributor_Size7_Class1"",""cost"":236836,""sku"":null},""128666678"":{""id"":128666678,""category"":""module"",""name"":""Int_FuelScoop_Size3_Class5"",""cost"":858371,""sku"":null},""128666677"":{""id"":128666677,""category"":""module"",""name"":""Int_FuelScoop_Size2_Class5"",""cost"":270780,""sku"":null},""128666670"":{""id"":128666670,""category"":""module"",""name"":""Int_FuelScoop_Size3_Class4"",""cost"":214593,""sku"":null},""128666662"":{""id"":128666662,""category"":""module"",""name"":""Int_FuelScoop_Size3_Class3"",""cost"":53649,""sku"":null},""128666669"":{""id"":128666669,""category"":""module"",""name"":""Int_FuelScoop_Size2_Class4"",""cost"":67695,""sku"":null},""128666676"":{""id"":128666676,""category"":""module"",""name"":""Int_FuelScoop_Size1_Class5"",""cost"":78208,""sku"":null},""128666668"":{""id"":128666668,""category"":""module"",""name"":""Int_FuelScoop_Size1_Class4"",""cost"":19553,""sku"":null},""128666661"":{""id"":128666661,""category"":""module"",""name"":""Int_FuelScoop_Size2_Class3"",""cost"":16924,""sku"":null},""128666660"":{""id"":128666660,""category"":""module"",""name"":""Int_FuelScoop_Size1_Class3"",""cost"":4889,""sku"":null},""128666654"":{""id"":128666654,""category"":""module"",""name"":""Int_FuelScoop_Size3_Class2"",""cost"":13413,""sku"":null},""128666653"":{""id"":128666653,""category"":""module"",""name"":""Int_FuelScoop_Size2_Class2"",""cost"":4232,""sku"":null},""128666695"":{""id"":128666695,""category"":""module"",""name"":""Int_Refinery_Size4_Class3"",""cost"":475402,""sku"":null},""128666702"":{""id"":128666702,""category"":""module"",""name"":""Int_Refinery_Size3_Class5"",""cost"":2037437,""sku"":null},""128666698"":{""id"":128666698,""category"":""module"",""name"":""Int_Refinery_Size3_Class4"",""cost"":679146,""sku"":null},""128666694"":{""id"":128666694,""category"":""module"",""name"":""Int_Refinery_Size3_Class3"",""cost"":226382,""sku"":null},""128666691"":{""id"":128666691,""category"":""module"",""name"":""Int_Refinery_Size4_Class2"",""cost"":158468,""sku"":null},""128666687"":{""id"":128666687,""category"":""module"",""name"":""Int_Refinery_Size4_Class1"",""cost"":52823,""sku"":null},""128666690"":{""id"":128666690,""category"":""module"",""name"":""Int_Refinery_Size3_Class2"",""cost"":75461,""sku"":null},""128666686"":{""id"":128666686,""category"":""module"",""name"":""Int_Refinery_Size3_Class1"",""cost"":25154,""sku"":null},""128667629"":{""id"":128667629,""category"":""module"",""name"":""Int_Repairer_Size8_Class4"",""cost"":15713776,""sku"":null},""128667621"":{""id"":128667621,""category"":""module"",""name"":""Int_Repairer_Size8_Class3"",""cost"":5237925,""sku"":null},""128667628"":{""id"":128667628,""category"":""module"",""name"":""Int_Repairer_Size7_Class4"",""cost"":8729875,""sku"":null},""128667635"":{""id"":128667635,""category"":""module"",""name"":""Int_Repairer_Size6_Class5"",""cost"":14549792,""sku"":null},""128667627"":{""id"":128667627,""category"":""module"",""name"":""Int_Repairer_Size6_Class4"",""cost"":4849931,""sku"":null},""128667634"":{""id"":128667634,""category"":""module"",""name"":""Int_Repairer_Size5_Class5"",""cost"":8083218,""sku"":null},""128667633"":{""id"":128667633,""category"":""module"",""name"":""Int_Repairer_Size4_Class5"",""cost"":4490677,""sku"":null},""128667626"":{""id"":128667626,""category"":""module"",""name"":""Int_Repairer_Size5_Class4"",""cost"":2694406,""sku"":null},""128667613"":{""id"":128667613,""category"":""module"",""name"":""Int_Repairer_Size8_Class2"",""cost"":1745975,""sku"":null},""128667619"":{""id"":128667619,""category"":""module"",""name"":""Int_Repairer_Size6_Class3"",""cost"":1616644,""sku"":null},""128666719"":{""id"":128666719,""category"":""module"",""name"":""Int_FSDInterdictor_Size4_Class4"",""cost"":6761271,""sku"":null},""128666715"":{""id"":128666715,""category"":""module"",""name"":""Int_FSDInterdictor_Size4_Class3"",""cost"":2253757,""sku"":null},""128666722"":{""id"":128666722,""category"":""module"",""name"":""Int_FSDInterdictor_Size3_Class5"",""cost"":7244219,""sku"":null},""128666718"":{""id"":128666718,""category"":""module"",""name"":""Int_FSDInterdictor_Size3_Class4"",""cost"":2414740,""sku"":null},""128666711"":{""id"":128666711,""category"":""module"",""name"":""Int_FSDInterdictor_Size4_Class2"",""cost"":751253,""sku"":null},""128666707"":{""id"":128666707,""category"":""module"",""name"":""Int_FSDInterdictor_Size4_Class1"",""cost"":250418,""sku"":null},""128666710"":{""id"":128666710,""category"":""module"",""name"":""Int_FSDInterdictor_Size3_Class2"",""cost"":268305,""sku"":null},""128666706"":{""id"":128666706,""category"":""module"",""name"":""Int_FSDInterdictor_Size3_Class1"",""cost"":89435,""sku"":null},""128671268"":{""id"":128671268,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size7_Class5"",""cost"":6652854,""sku"":null},""128671263"":{""id"":128671263,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size5_Class5"",""cost"":739206,""sku"":null},""128671258"":{""id"":128671258,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size3_Class5"",""cost"":82134,""sku"":null},""128671253"":{""id"":128671253,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size1_Class5"",""cost"":9126,""sku"":null},""128671267"":{""id"":128671267,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size7_Class4"",""cost"":3326427,""sku"":null},""128671262"":{""id"":128671262,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size5_Class4"",""cost"":369603,""sku"":null},""128671266"":{""id"":128671266,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size7_Class3"",""cost"":1663214,""sku"":null},""128671257"":{""id"":128671257,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size3_Class4"",""cost"":41067,""sku"":null},""128671265"":{""id"":128671265,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size7_Class2"",""cost"":831607,""sku"":null},""128064311"":{""id"":128064311,""category"":""module"",""name"":""Int_ShieldCellBank_Size3_Class4"",""cost"":60206,""sku"":null},""128064301"":{""id"":128064301,""category"":""module"",""name"":""Int_ShieldCellBank_Size1_Class4"",""cost"":7680,""sku"":null},""128064306"":{""id"":128064306,""category"":""module"",""name"":""Int_ShieldCellBank_Size2_Class4"",""cost"":21503,""sku"":null},""128064310"":{""id"":128064310,""category"":""module"",""name"":""Int_ShieldCellBank_Size3_Class3"",""cost"":24083,""sku"":null},""128064305"":{""id"":128064305,""category"":""module"",""name"":""Int_ShieldCellBank_Size2_Class3"",""cost"":8602,""sku"":null},""128064300"":{""id"":128064300,""category"":""module"",""name"":""Int_ShieldCellBank_Size1_Class3"",""cost"":3072,""sku"":null},""128064309"":{""id"":128064309,""category"":""module"",""name"":""Int_ShieldCellBank_Size3_Class2"",""cost"":9633,""sku"":null},""128064299"":{""id"":128064299,""category"":""module"",""name"":""Int_ShieldCellBank_Size1_Class2"",""cost"":1230,""sku"":null},""128064304"":{""id"":128064304,""category"":""module"",""name"":""Int_ShieldCellBank_Size2_Class2"",""cost"":3441,""sku"":null},""128671252"":{""id"":128671252,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size1_Class4"",""cost"":4563,""sku"":null},""128671264"":{""id"":128671264,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size7_Class1"",""cost"":415804,""sku"":null},""128671260"":{""id"":128671260,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size5_Class2"",""cost"":92401,""sku"":null},""128671256"":{""id"":128671256,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size3_Class3"",""cost"":20534,""sku"":null},""128671251"":{""id"":128671251,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size1_Class3"",""cost"":2282,""sku"":null},""128671259"":{""id"":128671259,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size5_Class1"",""cost"":46201,""sku"":null},""128671255"":{""id"":128671255,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size3_Class2"",""cost"":10267,""sku"":null},""128671250"":{""id"":128671250,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size1_Class2"",""cost"":1141,""sku"":null},""128666683"":{""id"":128666683,""category"":""module"",""name"":""Int_FuelScoop_Size8_Class5"",""cost"":274771161,""sku"":null},""128666675"":{""id"":128666675,""category"":""module"",""name"":""Int_FuelScoop_Size8_Class4"",""cost"":68692790,""sku"":null},""128666682"":{""id"":128666682,""category"":""module"",""name"":""Int_FuelScoop_Size7_Class5"",""cost"":86678600,""sku"":null},""128666674"":{""id"":128666674,""category"":""module"",""name"":""Int_FuelScoop_Size7_Class4"",""cost"":21669650,""sku"":null},""128666667"":{""id"":128666667,""category"":""module"",""name"":""Int_FuelScoop_Size8_Class3"",""cost"":17173198,""sku"":null},""128666666"":{""id"":128666666,""category"":""module"",""name"":""Int_FuelScoop_Size7_Class3"",""cost"":5417413,""sku"":null},""128666659"":{""id"":128666659,""category"":""module"",""name"":""Int_FuelScoop_Size8_Class2"",""cost"":4293300,""sku"":null},""128666658"":{""id"":128666658,""category"":""module"",""name"":""Int_FuelScoop_Size7_Class2"",""cost"":1354354,""sku"":null},""128066541"":{""id"":128066541,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size3_Class5"",""cost"":82134,""sku"":null},""128066540"":{""id"":128066540,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size3_Class4"",""cost"":41067,""sku"":null},""128066539"":{""id"":128066539,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size3_Class3"",""cost"":20534,""sku"":null},""128066534"":{""id"":128066534,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size1_Class3"",""cost"":2282,""sku"":null},""128066538"":{""id"":128066538,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size3_Class2"",""cost"":10267,""sku"":null},""128066533"":{""id"":128066533,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size1_Class2"",""cost"":1141,""sku"":null},""128066537"":{""id"":128066537,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size3_Class1"",""cost"":5134,""sku"":null},""128064337"":{""id"":128064337,""category"":""module"",""name"":""Int_ShieldCellBank_Size8_Class5"",""cost"":25903953,""sku"":null},""128064332"":{""id"":128064332,""category"":""module"",""name"":""Int_ShieldCellBank_Size7_Class5"",""cost"":9251412,""sku"":null},""128064336"":{""id"":128064336,""category"":""module"",""name"":""Int_ShieldCellBank_Size8_Class4"",""cost"":10361581,""sku"":null},""128064331"":{""id"":128064331,""category"":""module"",""name"":""Int_ShieldCellBank_Size7_Class4"",""cost"":3700565,""sku"":null},""128064335"":{""id"":128064335,""category"":""module"",""name"":""Int_ShieldCellBank_Size8_Class3"",""cost"":4144633,""sku"":null},""128064334"":{""id"":128064334,""category"":""module"",""name"":""Int_ShieldCellBank_Size8_Class2"",""cost"":1657853,""sku"":null},""128064329"":{""id"":128064329,""category"":""module"",""name"":""Int_ShieldCellBank_Size7_Class2"",""cost"":592091,""sku"":null},""128066550"":{""id"":128066550,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size7_Class4"",""cost"":3326427,""sku"":null},""128066548"":{""id"":128066548,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size7_Class2"",""cost"":831607,""sku"":null},""128066544"":{""id"":128066544,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size5_Class3"",""cost"":184802,""sku"":null},""128066547"":{""id"":128066547,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size7_Class1"",""cost"":415804,""sku"":null},""128066543"":{""id"":128066543,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size5_Class2"",""cost"":92401,""sku"":null},""128066542"":{""id"":128066542,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size5_Class1"",""cost"":46201,""sku"":null},""128064172"":{""id"":128064172,""category"":""module"",""name"":""Int_LifeSupport_Size7_Class5"",""cost"":9251412,""sku"":null},""128064170"":{""id"":128064170,""category"":""module"",""name"":""Int_LifeSupport_Size7_Class3"",""cost"":1480226,""sku"":null},""128064174"":{""id"":128064174,""category"":""module"",""name"":""Int_LifeSupport_Size8_Class2"",""cost"":1657853,""sku"":null},""128064169"":{""id"":128064169,""category"":""module"",""name"":""Int_LifeSupport_Size7_Class2"",""cost"":592091,""sku"":null},""128064173"":{""id"":128064173,""category"":""module"",""name"":""Int_LifeSupport_Size8_Class1"",""cost"":663141,""sku"":null},""128064168"":{""id"":128064168,""category"":""module"",""name"":""Int_LifeSupport_Size7_Class1"",""cost"":236836,""sku"":null},""128064066"":{""id"":128064066,""category"":""module"",""name"":""Int_Powerplant_Size8_Class4"",""cost"":51519593,""sku"":null},""128064061"":{""id"":128064061,""category"":""module"",""name"":""Int_Powerplant_Size7_Class4"",""cost"":16252238,""sku"":null},""128064065"":{""id"":128064065,""category"":""module"",""name"":""Int_Powerplant_Size8_Class3"",""cost"":17173198,""sku"":null},""128064060"":{""id"":128064060,""category"":""module"",""name"":""Int_Powerplant_Size7_Class3"",""cost"":5417413,""sku"":null},""128064059"":{""id"":128064059,""category"":""module"",""name"":""Int_Powerplant_Size7_Class2"",""cost"":1805805,""sku"":null},""128064137"":{""id"":128064137,""category"":""module"",""name"":""Int_Hyperdrive_Size8_Class5"",""cost"":154558779,""sku"":null},""128064131"":{""id"":128064131,""category"":""module"",""name"":""Int_Hyperdrive_Size7_Class4"",""cost"":16252238,""sku"":null},""128064135"":{""id"":128064135,""category"":""module"",""name"":""Int_Hyperdrive_Size8_Class3"",""cost"":17173198,""sku"":null},""128064134"":{""id"":128064134,""category"":""module"",""name"":""Int_Hyperdrive_Size8_Class2"",""cost"":5724400,""sku"":null},""128064100"":{""id"":128064100,""category"":""module"",""name"":""Int_Engine_Size8_Class3"",""cost"":17173198,""sku"":null},""128064095"":{""id"":128064095,""category"":""module"",""name"":""Int_Engine_Size7_Class3"",""cost"":5417413,""sku"":null},""128064099"":{""id"":128064099,""category"":""module"",""name"":""Int_Engine_Size8_Class2"",""cost"":5724400,""sku"":null},""128064094"":{""id"":128064094,""category"":""module"",""name"":""Int_Engine_Size7_Class2"",""cost"":1805805,""sku"":null},""128049270"":{""id"":128049270,""category"":""module"",""name"":""Adder_Armour_Grade3"",""cost"":75126,""sku"":null},""128049269"":{""id"":128049269,""category"":""module"",""name"":""Adder_Armour_Grade2"",""cost"":33389,""sku"":null},""128049268"":{""id"":128049268,""category"":""module"",""name"":""Adder_Armour_Grade1"",""cost"":0,""sku"":null},""128049271"":{""id"":128049271,""category"":""module"",""name"":""Adder_Armour_Mirrored"",""cost"":177546,""sku"":null},""128049272"":{""id"":128049272,""category"":""module"",""name"":""Adder_Armour_Reactive"",""cost"":196745,""sku"":null},""128049288"":{""id"":128049288,""category"":""module"",""name"":""Type6_Armour_Grade3"",""cost"":894871,""sku"":null},""128049287"":{""id"":128049287,""category"":""module"",""name"":""Type6_Armour_Grade2"",""cost"":397721,""sku"":null},""128049286"":{""id"":128049286,""category"":""module"",""name"":""Type6_Armour_Grade1"",""cost"":0,""sku"":null},""128049289"":{""id"":128049289,""category"":""module"",""name"":""Type6_Armour_Mirrored"",""cost"":2114880,""sku"":null},""128049290"":{""id"":128049290,""category"":""module"",""name"":""Type6_Armour_Reactive"",""cost"":2343569,""sku"":null},""128672273"":{""id"":128672273,""category"":""module"",""name"":""Independant_Trader_Armour_Grade3"",""cost"":2674620,""sku"":null},""128672272"":{""id"":128672272,""category"":""module"",""name"":""Independant_Trader_Armour_Grade2"",""cost"":1188720,""sku"":null},""128672271"":{""id"":128672271,""category"":""module"",""name"":""Independant_Trader_Armour_Grade1"",""cost"":0,""sku"":null},""128672274"":{""id"":128672274,""category"":""module"",""name"":""Independant_Trader_Armour_Mirrored"",""cost"":6321019,""sku"":null},""128672275"":{""id"":128672275,""category"":""module"",""name"":""Independant_Trader_Armour_Reactive"",""cost"":7004533,""sku"":null},""128666701"":{""id"":128666701,""category"":""module"",""name"":""Int_Refinery_Size2_Class5"",""cost"":970208,""sku"":null},""128064181"":{""id"":128064181,""category"":""module"",""name"":""Int_PowerDistributor_Size1_Class4"",""cost"":7680,""sku"":null},""128064186"":{""id"":128064186,""category"":""module"",""name"":""Int_PowerDistributor_Size2_Class4"",""cost"":21503,""sku"":null},""128064036"":{""id"":128064036,""category"":""module"",""name"":""Int_Powerplant_Size2_Class4"",""cost"":50771,""sku"":null},""128064212"":{""id"":128064212,""category"":""module"",""name"":""Int_PowerDistributor_Size7_Class5"",""cost"":9251412,""sku"":null},""128064215"":{""id"":128064215,""category"":""module"",""name"":""Int_PowerDistributor_Size8_Class3"",""cost"":4144633,""sku"":null},""128666646"":{""id"":128666646,""category"":""module"",""name"":""Int_FuelScoop_Size3_Class1"",""cost"":3219,""sku"":null},""128666652"":{""id"":128666652,""category"":""module"",""name"":""Int_FuelScoop_Size1_Class2"",""cost"":1222,""sku"":null},""128666645"":{""id"":128666645,""category"":""module"",""name"":""Int_FuelScoop_Size2_Class1"",""cost"":1016,""sku"":null},""128064076"":{""id"":128064076,""category"":""module"",""name"":""Int_Engine_Size3_Class4"",""cost"":160945,""sku"":null},""128671254"":{""id"":128671254,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size3_Class1"",""cost"":5134,""sku"":null},""128671249"":{""id"":128671249,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size1_Class1"",""cost"":571,""sku"":null},""128666703"":{""id"":128666703,""category"":""module"",""name"":""Int_Refinery_Size4_Class5"",""cost"":4278617,""sku"":null},""128666699"":{""id"":128666699,""category"":""module"",""name"":""Int_Refinery_Size4_Class4"",""cost"":1426206,""sku"":null},""128671261"":{""id"":128671261,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size5_Class3"",""cost"":184802,""sku"":null},""128667620"":{""id"":128667620,""category"":""module"",""name"":""Int_Repairer_Size7_Class3"",""cost"":2909959,""sku"":null},""128667632"":{""id"":128667632,""category"":""module"",""name"":""Int_Repairer_Size3_Class5"",""cost"":2494821,""sku"":null},""128667618"":{""id"":128667618,""category"":""module"",""name"":""Int_Repairer_Size5_Class3"",""cost"":898136,""sku"":null},""128667612"":{""id"":128667612,""category"":""module"",""name"":""Int_Repairer_Size7_Class2"",""cost"":969987,""sku"":null},""128667631"":{""id"":128667631,""category"":""module"",""name"":""Int_Repairer_Size2_Class5"",""cost"":1386012,""sku"":null},""128667617"":{""id"":128667617,""category"":""module"",""name"":""Int_Repairer_Size4_Class3"",""cost"":498965,""sku"":null},""128667624"":{""id"":128667624,""category"":""module"",""name"":""Int_Repairer_Size3_Class4"",""cost"":831607,""sku"":null},""128667630"":{""id"":128667630,""category"":""module"",""name"":""Int_Repairer_Size1_Class5"",""cost"":770007,""sku"":null},""128666714"":{""id"":128666714,""category"":""module"",""name"":""Int_FSDInterdictor_Size3_Class3"",""cost"":804914,""sku"":null},""128064312"":{""id"":128064312,""category"":""module"",""name"":""Int_ShieldCellBank_Size3_Class5"",""cost"":150514,""sku"":null},""128064302"":{""id"":128064302,""category"":""module"",""name"":""Int_ShieldCellBank_Size1_Class5"",""cost"":19198,""sku"":null},""128064307"":{""id"":128064307,""category"":""module"",""name"":""Int_ShieldCellBank_Size2_Class5"",""cost"":53755,""sku"":null},""128064308"":{""id"":128064308,""category"":""module"",""name"":""Int_ShieldCellBank_Size3_Class1"",""cost"":3853,""sku"":null},""128064298"":{""id"":128064298,""category"":""module"",""name"":""Int_ShieldCellBank_Size1_Class1"",""cost"":492,""sku"":null},""128064303"":{""id"":128064303,""category"":""module"",""name"":""Int_ShieldCellBank_Size2_Class1"",""cost"":1377,""sku"":null},""128066536"":{""id"":128066536,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size1_Class5"",""cost"":9126,""sku"":null},""128066535"":{""id"":128066535,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size1_Class4"",""cost"":4563,""sku"":null},""128666651"":{""id"":128666651,""category"":""module"",""name"":""Int_FuelScoop_Size8_Class1"",""cost"":1030392,""sku"":null},""128666650"":{""id"":128666650,""category"":""module"",""name"":""Int_FuelScoop_Size7_Class1"",""cost"":325045,""sku"":null},""128064330"":{""id"":128064330,""category"":""module"",""name"":""Int_ShieldCellBank_Size7_Class3"",""cost"":1480226,""sku"":null},""128064333"":{""id"":128064333,""category"":""module"",""name"":""Int_ShieldCellBank_Size8_Class1"",""cost"":663141,""sku"":null},""128064328"":{""id"":128064328,""category"":""module"",""name"":""Int_ShieldCellBank_Size7_Class1"",""cost"":236836,""sku"":null},""128667622"":{""id"":128667622,""category"":""module"",""name"":""Int_Repairer_Size1_Class4"",""cost"":256669,""sku"":null},""128667606"":{""id"":128667606,""category"":""module"",""name"":""Int_Repairer_Size1_Class2"",""cost"":28519,""sku"":null},""128667599"":{""id"":128667599,""category"":""module"",""name"":""Int_Repairer_Size2_Class1"",""cost"":17112,""sku"":null},""128667598"":{""id"":128667598,""category"":""module"",""name"":""Int_Repairer_Size1_Class1"",""cost"":9507,""sku"":null},""128066551"":{""id"":128066551,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size7_Class5"",""cost"":6652854,""sku"":null},""128066545"":{""id"":128066545,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size5_Class4"",""cost"":369603,""sku"":null},""128066549"":{""id"":128066549,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size7_Class3"",""cost"":1663214,""sku"":null},""128064152"":{""id"":128064152,""category"":""module"",""name"":""Int_LifeSupport_Size3_Class5"",""cost"":150514,""sku"":null},""128064151"":{""id"":128064151,""category"":""module"",""name"":""Int_LifeSupport_Size3_Class4"",""cost"":60206,""sku"":null},""128064171"":{""id"":128064171,""category"":""module"",""name"":""Int_LifeSupport_Size7_Class4"",""cost"":3700565,""sku"":null},""128064175"":{""id"":128064175,""category"":""module"",""name"":""Int_LifeSupport_Size8_Class3"",""cost"":4144633,""sku"":null},""128064062"":{""id"":128064062,""category"":""module"",""name"":""Int_Powerplant_Size7_Class5"",""cost"":48756713,""sku"":null},""128064064"":{""id"":128064064,""category"":""module"",""name"":""Int_Powerplant_Size8_Class2"",""cost"":5724400,""sku"":null},""128064063"":{""id"":128064063,""category"":""module"",""name"":""Int_Powerplant_Size8_Class1"",""cost"":1908134,""sku"":null},""128064058"":{""id"":128064058,""category"":""module"",""name"":""Int_Powerplant_Size7_Class1"",""cost"":601935,""sku"":null},""128671273"":{""id"":128671273,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size1_Class5"",""cost"":9126,""sku"":null},""128671282"":{""id"":128671282,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size5_Class4"",""cost"":369603,""sku"":null},""128064097"":{""id"":128064097,""category"":""module"",""name"":""Int_Engine_Size7_Class5"",""cost"":48756713,""sku"":null},""128064098"":{""id"":128064098,""category"":""module"",""name"":""Int_Engine_Size8_Class1"",""cost"":1908134,""sku"":null},""128064093"":{""id"":128064093,""category"":""module"",""name"":""Int_Engine_Size7_Class1"",""cost"":601935,""sku"":null},""128064132"":{""id"":128064132,""category"":""module"",""name"":""Int_Hyperdrive_Size7_Class5"",""cost"":48756713,""sku"":null},""128064136"":{""id"":128064136,""category"":""module"",""name"":""Int_Hyperdrive_Size8_Class4"",""cost"":51519593,""sku"":null},""128064130"":{""id"":128064130,""category"":""module"",""name"":""Int_Hyperdrive_Size7_Class3"",""cost"":5417413,""sku"":null},""128064129"":{""id"":128064129,""category"":""module"",""name"":""Int_Hyperdrive_Size7_Class2"",""cost"":1805805,""sku"":null},""128064133"":{""id"":128064133,""category"":""module"",""name"":""Int_Hyperdrive_Size8_Class1"",""cost"":1908134,""sku"":null},""128064128"":{""id"":128064128,""category"":""module"",""name"":""Int_Hyperdrive_Size7_Class1"",""cost"":601935,""sku"":null},""128064297"":{""id"":128064297,""category"":""module"",""name"":""Int_ShieldGenerator_Size8_Class5"",""cost"":154558779,""sku"":null},""128064292"":{""id"":128064292,""category"":""module"",""name"":""Int_ShieldGenerator_Size7_Class5"",""cost"":48756713,""sku"":null},""128064289"":{""id"":128064289,""category"":""module"",""name"":""Int_ShieldGenerator_Size7_Class2"",""cost"":1805805,""sku"":null},""128064231"":{""id"":128064231,""category"":""module"",""name"":""Int_Sensors_Size3_Class4"",""cost"":60206,""sku"":null},""128064242"":{""id"":128064242,""category"":""module"",""name"":""Int_Sensors_Size5_Class5"",""cost"":1180027,""sku"":null},""128671233"":{""id"":128671233,""category"":""module"",""name"":""Int_DroneControl_Collection_Size1_Class5"",""cost"":9126,""sku"":null},""128064196"":{""id"":128064196,""category"":""module"",""name"":""Int_PowerDistributor_Size4_Class4"",""cost"":168576,""sku"":null},""128666673"":{""id"":128666673,""category"":""module"",""name"":""Int_FuelScoop_Size6_Class4"",""cost"":6835853,""sku"":null},""128666679"":{""id"":128666679,""category"":""module"",""name"":""Int_FuelScoop_Size4_Class5"",""cost"":2721035,""sku"":null},""128668542"":{""id"":128668542,""category"":""module"",""name"":""Int_HullReinforcement_Size3_Class2"",""cost"":79853,""sku"":null},""128668538"":{""id"":128668538,""category"":""module"",""name"":""Int_HullReinforcement_Size1_Class2"",""cost"":14260,""sku"":null},""128064161"":{""id"":128064161,""category"":""module"",""name"":""Int_LifeSupport_Size5_Class4"",""cost"":472011,""sku"":null},""128064055"":{""id"":128064055,""category"":""module"",""name"":""Int_Powerplant_Size6_Class3"",""cost"":1708964,""sku"":null},""128064283"":{""id"":128064283,""category"":""module"",""name"":""Int_ShieldGenerator_Size6_Class1"",""cost"":189885,""sku"":null},""128064278"":{""id"":128064278,""category"":""module"",""name"":""Int_ShieldGenerator_Size5_Class1"",""cost"":59901,""sku"":null},""128064274"":{""id"":128064274,""category"":""module"",""name"":""Int_ShieldGenerator_Size4_Class2"",""cost"":56689,""sku"":null},""128064273"":{""id"":128064273,""category"":""module"",""name"":""Int_ShieldGenerator_Size4_Class1"",""cost"":18897,""sku"":null},""128064127"":{""id"":128064127,""category"":""module"",""name"":""Int_Hyperdrive_Size6_Class5"",""cost"":15380667,""sku"":null},""128064113"":{""id"":128064113,""category"":""module"",""name"":""Int_Hyperdrive_Size4_Class1"",""cost"":18897,""sku"":null},""128682043"":{""id"":128682043,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Metallic"",""cost"":0,""sku"":null},""128672343"":{""id"":128672343,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Horizons_Polar"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1058""},""128672344"":{""id"":128672344,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Horizons_Desert"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1062""},""128672345"":{""id"":128672345,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Horizons_Lunar"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1063""},""128667727"":{""id"":128667727,""category"":""paintjob"",""name"":""paintjob_CobraMkiii_Default_52"",""cost"":0,""sku"":null},""128066428"":{""id"":128066428,""category"":""paintjob"",""name"":""paintjob_cobramkiii_wireframe_01"",""cost"":0,""sku"":null},""128670861"":{""id"":128670861,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Onionhead1_01"",""cost"":0,""sku"":null},""128671133"":{""id"":128671133,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_green"",""cost"":0,""sku"":null},""128671134"":{""id"":128671134,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_blue"",""cost"":0,""sku"":null},""128671135"":{""id"":128671135,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_orange"",""cost"":0,""sku"":null},""128671136"":{""id"":128671136,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_red"",""cost"":0,""sku"":null},""128671137"":{""id"":128671137,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_purple"",""cost"":0,""sku"":null},""128671138"":{""id"":128671138,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_yellow"",""cost"":0,""sku"":null},""128667638"":{""id"":128667638,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Merc"",""cost"":0,""sku"":null},""128667639"":{""id"":128667639,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Merc"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_MERCENARY_PAINTJOB_1001""},""128672779"":{""id"":128672779,""category"":""paintjob"",""name"":""PaintJob_Eagle_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_EAGLE_1033""},""128066405"":{""id"":128066405,""category"":""paintjob"",""name"":""paintjob_eagle_stripe1_02"",""cost"":0,""sku"":null},""128066406"":{""id"":128066406,""category"":""paintjob"",""name"":""paintjob_eagle_doublestripe_01"",""cost"":0,""sku"":null},""128066416"":{""id"":128066416,""category"":""paintjob"",""name"":""paintjob_eagle_thirds_01"",""cost"":0,""sku"":null},""128066419"":{""id"":128066419,""category"":""paintjob"",""name"":""paintjob_eagle_stripe1_03"",""cost"":0,""sku"":null},""128668019"":{""id"":128668019,""category"":""paintjob"",""name"":""PaintJob_Eagle_Crimson"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_MERCENARY_PAINTJOB_1003""},""128066420"":{""id"":128066420,""category"":""paintjob"",""name"":""paintjob_eagle_thirds_02"",""cost"":0,""sku"":null},""128066430"":{""id"":128066430,""category"":""paintjob"",""name"":""paintjob_eagle_stripe1_01"",""cost"":0,""sku"":null},""128066436"":{""id"":128066436,""category"":""paintjob"",""name"":""paintjob_eagle_camo_03"",""cost"":0,""sku"":null},""128066437"":{""id"":128066437,""category"":""paintjob"",""name"":""paintjob_eagle_thirds_03"",""cost"":0,""sku"":null},""128066441"":{""id"":128066441,""category"":""paintjob"",""name"":""paintjob_eagle_camo_02"",""cost"":0,""sku"":null},""128066449"":{""id"":128066449,""category"":""paintjob"",""name"":""paintjob_eagle_doublestripe_02"",""cost"":0,""sku"":null},""128066453"":{""id"":128066453,""category"":""paintjob"",""name"":""paintjob_eagle_doublestripe_03"",""cost"":0,""sku"":null},""128066456"":{""id"":128066456,""category"":""paintjob"",""name"":""paintjob_eagle_camo_01"",""cost"":0,""sku"":null},""128671139"":{""id"":128671139,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_green"",""cost"":0,""sku"":null},""128671140"":{""id"":128671140,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_blue"",""cost"":0,""sku"":null},""128671141"":{""id"":128671141,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_orange"",""cost"":0,""sku"":null},""128671142"":{""id"":128671142,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_red"",""cost"":0,""sku"":null},""128671143"":{""id"":128671143,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_purple"",""cost"":0,""sku"":null},""128671144"":{""id"":128671144,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_yellow"",""cost"":0,""sku"":null},""128671777"":{""id"":128671777,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Desert_Sand"",""cost"":0,""sku"":null},""128671778"":{""id"":128671778,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Earth_Yellow"",""cost"":0,""sku"":null},""128672802"":{""id"":128672802,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_BlackFriday_01"",""cost"":0,""sku"":null},""128671779"":{""id"":128671779,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Dark_Green"",""cost"":0,""sku"":null},""128671780"":{""id"":128671780,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Forest_Green"",""cost"":0,""sku"":null},""128671781"":{""id"":128671781,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Sand"",""cost"":0,""sku"":null},""128671782"":{""id"":128671782,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Earth_Red"",""cost"":0,""sku"":null},""128672426"":{""id"":128672426,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_SpecialEffect_01"",""cost"":0,""sku"":null},""128066404"":{""id"":128066404,""category"":""paintjob"",""name"":""paintjob_sidewinder_default_02"",""cost"":0,""sku"":null},""128066408"":{""id"":128066408,""category"":""paintjob"",""name"":""paintjob_sidewinder_default_03"",""cost"":0,""sku"":null},""128066414"":{""id"":128066414,""category"":""paintjob"",""name"":""paintjob_sidewinder_doublestripe_08"",""cost"":0,""sku"":null},""128066423"":{""id"":128066423,""category"":""paintjob"",""name"":""paintjob_sidewinder_doublestripe_05"",""cost"":0,""sku"":null},""128066431"":{""id"":128066431,""category"":""paintjob"",""name"":""paintjob_sidewinder_thirds_07"",""cost"":0,""sku"":null},""128066432"":{""id"":128066432,""category"":""paintjob"",""name"":""paintjob_sidewinder_thirds_01"",""cost"":0,""sku"":null},""128066433"":{""id"":128066433,""category"":""paintjob"",""name"":""paintjob_sidewinder_doublestripe_07"",""cost"":0,""sku"":null},""128066440"":{""id"":128066440,""category"":""paintjob"",""name"":""paintjob_sidewinder_camo_01"",""cost"":0,""sku"":null},""128066444"":{""id"":128066444,""category"":""paintjob"",""name"":""paintjob_sidewinder_thirds_06"",""cost"":0,""sku"":null},""128066447"":{""id"":128066447,""category"":""paintjob"",""name"":""paintjob_sidewinder_camo_03"",""cost"":0,""sku"":null},""128066448"":{""id"":128066448,""category"":""paintjob"",""name"":""paintjob_sidewinder_default_04"",""cost"":0,""sku"":null},""128066454"":{""id"":128066454,""category"":""paintjob"",""name"":""paintjob_sidewinder_camo_02"",""cost"":0,""sku"":null},""128671181"":{""id"":128671181,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_green"",""cost"":0,""sku"":null},""128671182"":{""id"":128671182,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_blue"",""cost"":0,""sku"":null},""128671183"":{""id"":128671183,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_orange"",""cost"":0,""sku"":null},""128671184"":{""id"":128671184,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_red"",""cost"":0,""sku"":null},""128671185"":{""id"":128671185,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_purple"",""cost"":0,""sku"":null},""128671186"":{""id"":128671186,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_yellow"",""cost"":0,""sku"":null},""128672796"":{""id"":128672796,""category"":""paintjob"",""name"":""PaintJob_Viper_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_VIPER_1049""},""128667667"":{""id"":128667667,""category"":""paintjob"",""name"":""PaintJob_Viper_Merc"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_MERCENARY_PAINTJOB_1002""},""128066407"":{""id"":128066407,""category"":""paintjob"",""name"":""paintjob_viper_flag_switzerland_01"",""cost"":0,""sku"":null},""128066409"":{""id"":128066409,""category"":""paintjob"",""name"":""paintjob_viper_flag_belgium_01"",""cost"":0,""sku"":null},""128066410"":{""id"":128066410,""category"":""paintjob"",""name"":""paintjob_viper_flag_australia_01"",""cost"":0,""sku"":null},""128066411"":{""id"":128066411,""category"":""paintjob"",""name"":""paintjob_viper_default_01"",""cost"":0,""sku"":null},""128066412"":{""id"":128066412,""category"":""paintjob"",""name"":""paintjob_viper_stripe2_02"",""cost"":0,""sku"":null},""128066413"":{""id"":128066413,""category"":""paintjob"",""name"":""paintjob_viper_flag_austria_01"",""cost"":0,""sku"":null},""128066415"":{""id"":128066415,""category"":""paintjob"",""name"":""paintjob_viper_stripe1_01"",""cost"":0,""sku"":null},""128066417"":{""id"":128066417,""category"":""paintjob"",""name"":""paintjob_viper_flag_spain_01"",""cost"":0,""sku"":null},""128066418"":{""id"":128066418,""category"":""paintjob"",""name"":""paintjob_viper_stripe1_02"",""cost"":0,""sku"":null},""128066421"":{""id"":128066421,""category"":""paintjob"",""name"":""paintjob_viper_flag_denmark_01"",""cost"":0,""sku"":null},""128066422"":{""id"":128066422,""category"":""paintjob"",""name"":""paintjob_viper_police_federation_01"",""cost"":0,""sku"":null},""128666742"":{""id"":128666742,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Hotrod_01"",""cost"":0,""sku"":null},""128666743"":{""id"":128666743,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Hotrod_03"",""cost"":0,""sku"":null},""128066424"":{""id"":128066424,""category"":""paintjob"",""name"":""paintjob_viper_flag_newzealand_01"",""cost"":0,""sku"":null},""128066425"":{""id"":128066425,""category"":""paintjob"",""name"":""paintjob_viper_flag_italy_01"",""cost"":0,""sku"":null},""128066426"":{""id"":128066426,""category"":""paintjob"",""name"":""paintjob_viper_stripe2_04"",""cost"":0,""sku"":null},""128066427"":{""id"":128066427,""category"":""paintjob"",""name"":""paintjob_viper_police_independent_01"",""cost"":0,""sku"":null},""128066429"":{""id"":128066429,""category"":""paintjob"",""name"":""paintjob_viper_default_03"",""cost"":0,""sku"":null},""128066434"":{""id"":128066434,""category"":""paintjob"",""name"":""paintjob_viper_flag_uk_01"",""cost"":0,""sku"":null},""128066435"":{""id"":128066435,""category"":""paintjob"",""name"":""paintjob_viper_flag_germany_01"",""cost"":0,""sku"":null},""128066438"":{""id"":128066438,""category"":""paintjob"",""name"":""paintjob_viper_flag_netherlands_01"",""cost"":0,""sku"":null},""128066439"":{""id"":128066439,""category"":""paintjob"",""name"":""paintjob_viper_flag_usa_01"",""cost"":0,""sku"":null},""128066442"":{""id"":128066442,""category"":""paintjob"",""name"":""paintjob_viper_flag_russia_01"",""cost"":0,""sku"":null},""128066443"":{""id"":128066443,""category"":""paintjob"",""name"":""paintjob_viper_flag_canada_01"",""cost"":0,""sku"":null},""128066445"":{""id"":128066445,""category"":""paintjob"",""name"":""paintjob_viper_flag_sweden_01"",""cost"":0,""sku"":null},""128066446"":{""id"":128066446,""category"":""paintjob"",""name"":""paintjob_viper_flag_poland_01"",""cost"":0,""sku"":null},""128066450"":{""id"":128066450,""category"":""paintjob"",""name"":""paintjob_viper_flag_finland_01"",""cost"":0,""sku"":null},""128066451"":{""id"":128066451,""category"":""paintjob"",""name"":""paintjob_viper_flag_france_01"",""cost"":0,""sku"":null},""128066452"":{""id"":128066452,""category"":""paintjob"",""name"":""paintjob_viper_police_empire_01"",""cost"":0,""sku"":null},""128066455"":{""id"":128066455,""category"":""paintjob"",""name"":""paintjob_viper_flag_norway_01"",""cost"":0,""sku"":null},""128671205"":{""id"":128671205,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_green"",""cost"":0,""sku"":null},""128671206"":{""id"":128671206,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_blue"",""cost"":0,""sku"":null},""128671207"":{""id"":128671207,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_orange"",""cost"":0,""sku"":null},""128671208"":{""id"":128671208,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_red"",""cost"":0,""sku"":null},""128671209"":{""id"":128671209,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_purple"",""cost"":0,""sku"":null},""128671210"":{""id"":128671210,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_yellow"",""cost"":0,""sku"":null},""128672806"":{""id"":128672806,""category"":""paintjob"",""name"":""PaintJob_Asp_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_ASP_1043""},""128672419"":{""id"":128672419,""category"":""paintjob"",""name"":""PaintJob_Asp_Metallic_Gold"",""cost"":0,""sku"":""FORC_FDEV_V_ASP_1038""},""128667720"":{""id"":128667720,""category"":""paintjob"",""name"":""PaintJob_Asp_Default_02"",""cost"":0,""sku"":""FORC_FDEV_V_ASP_1001""},""128667721"":{""id"":128667721,""category"":""paintjob"",""name"":""PaintJob_Asp_Default_03"",""cost"":0,""sku"":""FORC_FDEV_V_ASP_1002""},""128667722"":{""id"":128667722,""category"":""paintjob"",""name"":""PaintJob_Asp_Default_04"",""cost"":0,""sku"":""FORC_FDEV_V_ASP_1003""},""128667724"":{""id"":128667724,""category"":""paintjob"",""name"":""PaintJob_Asp_Stripe1_02"",""cost"":0,""sku"":""FORC_FDEV_V_ASP_1004""},""128667725"":{""id"":128667725,""category"":""paintjob"",""name"":""PaintJob_Asp_Stripe1_03"",""cost"":0,""sku"":""FORC_FDEV_V_ASP_1005""},""128667726"":{""id"":128667726,""category"":""paintjob"",""name"":""PaintJob_Asp_Stripe1_04"",""cost"":0,""sku"":""FORC_FDEV_V_ASP_1006""},""128671127"":{""id"":128671127,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_green"",""cost"":0,""sku"":null},""128671128"":{""id"":128671128,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_blue"",""cost"":0,""sku"":null},""128671129"":{""id"":128671129,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_orange"",""cost"":0,""sku"":null},""128671130"":{""id"":128671130,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_red"",""cost"":0,""sku"":null},""128671131"":{""id"":128671131,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_purple"",""cost"":0,""sku"":null},""128671132"":{""id"":128671132,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_yellow"",""cost"":0,""sku"":null},""128672783"":{""id"":128672783,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1076""},""128672805"":{""id"":128672805,""category"":""paintjob"",""name"":""PaintJob_FedDropship_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_FEDDROP_1019""},""128671151"":{""id"":128671151,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_green"",""cost"":0,""sku"":null},""128671152"":{""id"":128671152,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_blue"",""cost"":0,""sku"":null},""128671153"":{""id"":128671153,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_orange"",""cost"":0,""sku"":null},""128671154"":{""id"":128671154,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_red"",""cost"":0,""sku"":null},""128671155"":{""id"":128671155,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_purple"",""cost"":0,""sku"":null},""128671156"":{""id"":128671156,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_yellow"",""cost"":0,""sku"":null},""128672788"":{""id"":128672788,""category"":""paintjob"",""name"":""PaintJob_Python_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_PYTHON_1020""},""128671175"":{""id"":128671175,""category"":""paintjob"",""name"":""paintjob_python_vibrant_green"",""cost"":0,""sku"":null},""128671176"":{""id"":128671176,""category"":""paintjob"",""name"":""paintjob_python_vibrant_blue"",""cost"":0,""sku"":null},""128671177"":{""id"":128671177,""category"":""paintjob"",""name"":""paintjob_python_vibrant_orange"",""cost"":0,""sku"":null},""128671178"":{""id"":128671178,""category"":""paintjob"",""name"":""paintjob_python_vibrant_red"",""cost"":0,""sku"":null},""128671179"":{""id"":128671179,""category"":""paintjob"",""name"":""paintjob_python_vibrant_purple"",""cost"":0,""sku"":null},""128671180"":{""id"":128671180,""category"":""paintjob"",""name"":""paintjob_python_vibrant_yellow"",""cost"":0,""sku"":null},""128672807"":{""id"":128672807,""category"":""paintjob"",""name"":""PaintJob_Adder_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_ADDER_1019""},""128671121"":{""id"":128671121,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_green"",""cost"":0,""sku"":null},""128671122"":{""id"":128671122,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_blue"",""cost"":0,""sku"":null},""128671123"":{""id"":128671123,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_orange"",""cost"":0,""sku"":null},""128671124"":{""id"":128671124,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_red"",""cost"":0,""sku"":null},""128671125"":{""id"":128671125,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_purple"",""cost"":0,""sku"":null},""128671126"":{""id"":128671126,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_yellow"",""cost"":0,""sku"":null},""128671145"":{""id"":128671145,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_green"",""cost"":0,""sku"":null},""128671146"":{""id"":128671146,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_blue"",""cost"":0,""sku"":null},""128671147"":{""id"":128671147,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_orange"",""cost"":0,""sku"":null},""128671148"":{""id"":128671148,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_red"",""cost"":0,""sku"":null},""128671149"":{""id"":128671149,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_purple"",""cost"":0,""sku"":null},""128671150"":{""id"":128671150,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_yellow"",""cost"":0,""sku"":null},""128671749"":{""id"":128671749,""category"":""paintjob"",""name"":""PaintJob_FerDeLance_Militaire_desert_Sand"",""cost"":0,""sku"":null},""128672795"":{""id"":128672795,""category"":""paintjob"",""name"":""PaintJob_FerDeLance_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_FERDELANCE_1019""},""128671157"":{""id"":128671157,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_green"",""cost"":0,""sku"":null},""128671158"":{""id"":128671158,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_blue"",""cost"":0,""sku"":null},""128671159"":{""id"":128671159,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_orange"",""cost"":0,""sku"":null},""128671160"":{""id"":128671160,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_red"",""cost"":0,""sku"":null},""128671161"":{""id"":128671161,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_purple"",""cost"":0,""sku"":null},""128671162"":{""id"":128671162,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_yellow"",""cost"":0,""sku"":null},""128672789"":{""id"":128672789,""category"":""paintjob"",""name"":""PaintJob_Hauler_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_HAULER_1024""},""128671163"":{""id"":128671163,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_green"",""cost"":0,""sku"":null},""128671164"":{""id"":128671164,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_blue"",""cost"":0,""sku"":null},""128671165"":{""id"":128671165,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_orange"",""cost"":0,""sku"":null},""128671166"":{""id"":128671166,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_red"",""cost"":0,""sku"":null},""128671167"":{""id"":128671167,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_purple"",""cost"":0,""sku"":null},""128671168"":{""id"":128671168,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_yellow"",""cost"":0,""sku"":null},""128672797"":{""id"":128672797,""category"":""paintjob"",""name"":""PaintJob_Orca_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_ORCA_1018""},""128671169"":{""id"":128671169,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_green"",""cost"":0,""sku"":null},""128671170"":{""id"":128671170,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_blue"",""cost"":0,""sku"":null},""128671171"":{""id"":128671171,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_orange"",""cost"":0,""sku"":null},""128671172"":{""id"":128671172,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_red"",""cost"":0,""sku"":null},""128671173"":{""id"":128671173,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_purple"",""cost"":0,""sku"":null},""128671174"":{""id"":128671174,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_yellow"",""cost"":0,""sku"":null},""128672800"":{""id"":128672800,""category"":""paintjob"",""name"":""PaintJob_Type6_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_TYPE6_1024""},""128671187"":{""id"":128671187,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_green"",""cost"":0,""sku"":null},""128671188"":{""id"":128671188,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_blue"",""cost"":0,""sku"":null},""128671189"":{""id"":128671189,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_orange"",""cost"":0,""sku"":null},""128671190"":{""id"":128671190,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_red"",""cost"":0,""sku"":null},""128671191"":{""id"":128671191,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_purple"",""cost"":0,""sku"":null},""128671192"":{""id"":128671192,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_yellow"",""cost"":0,""sku"":null},""128672799"":{""id"":128672799,""category"":""paintjob"",""name"":""PaintJob_Type7_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_TYPE7_1018""},""128671193"":{""id"":128671193,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_green"",""cost"":0,""sku"":null},""128671194"":{""id"":128671194,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_blue"",""cost"":0,""sku"":null},""128671195"":{""id"":128671195,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_orange"",""cost"":0,""sku"":null},""128671196"":{""id"":128671196,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_red"",""cost"":0,""sku"":null},""128671197"":{""id"":128671197,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_purple"",""cost"":0,""sku"":null},""128671198"":{""id"":128671198,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_yellow"",""cost"":0,""sku"":null},""128672793"":{""id"":128672793,""category"":""paintjob"",""name"":""PaintJob_Type9_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_TYPE9_1018""},""128671199"":{""id"":128671199,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_green"",""cost"":0,""sku"":null},""128671200"":{""id"":128671200,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_blue"",""cost"":0,""sku"":null},""128671201"":{""id"":128671201,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_orange"",""cost"":0,""sku"":null},""128671202"":{""id"":128671202,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_red"",""cost"":0,""sku"":null},""128671203"":{""id"":128671203,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_purple"",""cost"":0,""sku"":null},""128671204"":{""id"":128671204,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_yellow"",""cost"":0,""sku"":null},""128671211"":{""id"":128671211,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_green"",""cost"":0,""sku"":null},""128671212"":{""id"":128671212,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_blue"",""cost"":0,""sku"":null},""128671213"":{""id"":128671213,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_orange"",""cost"":0,""sku"":null},""128671214"":{""id"":128671214,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_red"",""cost"":0,""sku"":null},""128671215"":{""id"":128671215,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_purple"",""cost"":0,""sku"":null},""128671216"":{""id"":128671216,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_yellow"",""cost"":0,""sku"":null},""128672801"":{""id"":128672801,""category"":""paintjob"",""name"":""PaintJob_Vulture_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_VULTURE_1030""},""128672782"":{""id"":128672782,""category"":""paintjob"",""name"":""PaintJob_Anaconda_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_ANACONDA_1027""},""128672804"":{""id"":128672804,""category"":""paintjob"",""name"":""PaintJob_DiamondBack_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_DIAMOND_SCOUT_1018""},""128672784"":{""id"":128672784,""category"":""paintjob"",""name"":""PaintJob_DiamondBackXL_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_DIAMOND_EXPLORER_1020""},""128672786"":{""id"":128672786,""category"":""paintjob"",""name"":""PaintJob_Federation_Corvette_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_FEDERAL_CORVETTE_1000""},""128672781"":{""id"":128672781,""category"":""paintjob"",""name"":""PaintJob_Cutter_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_IMPERIAL_CUTTER_1000""},""128672792"":{""id"":128672792,""category"":""paintjob"",""name"":""PaintJob_Empire_Courier_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_IMPERIAL_COURIER_1018""},""128672791"":{""id"":128672791,""category"":""paintjob"",""name"":""PaintJob_FedGunship_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_FEDERAL_GUNSHIP_1000""},""128672794"":{""id"":128672794,""category"":""paintjob"",""name"":""PaintJob_FedDropshipMkII_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_FEDERAL_ASSAULT_1000""},""128672778"":{""id"":128672778,""category"":""paintjob"",""name"":""PaintJob_Empire_Eagle_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_IMPERIAL_EAGLE_1000""},""128672780"":{""id"":128672780,""category"":""paintjob"",""name"":""PaintJob_ViperMkIV_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_VIPER_1050""},""128672790"":{""id"":128672790,""category"":""paintjob"",""name"":""PaintJob_CobraMkIV_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_COBRA_1075""},""128672785"":{""id"":128672785,""category"":""paintjob"",""name"":""PaintJob_Independant_Trader_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_KEELBACK_1000""},""128672803"":{""id"":128672803,""category"":""paintjob"",""name"":""PaintJob_Asp_Scout_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_ASP_SCOUT_1000""},""128672798"":{""id"":128672798,""category"":""paintjob"",""name"":""PaintJob_EmpireTrader_BlackFriday_01"",""cost"":0,""sku"":""FORC_FDEV_V_CLIPPER_1019""},""128667650"":{""id"":128667650,""category"":""decal"",""name"":""Decal_Planet2"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_BACKERS_DECAL_1000""},""128667655"":{""id"":128667655,""category"":""decal"",""name"":""Decal_Skull3"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_MERCENARY_DECAL_1000""},""128667735"":{""id"":128667735,""category"":""decal"",""name"":""Decal_Pilot_Fed1"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_FOUNDER_DECAL_1000""},""128667736"":{""id"":128667736,""category"":""decal"",""name"":""Decal_Combat_Mostly_Harmless"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_COMBAT_DECAL_1001""},""128667737"":{""id"":128667737,""category"":""decal"",""name"":""Decal_Combat_Novice"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_COMBAT_DECAL_1002""},""128667738"":{""id"":128667738,""category"":""decal"",""name"":""Decal_Combat_Competent"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_COMBAT_DECAL_1003""},""128667739"":{""id"":128667739,""category"":""decal"",""name"":""Decal_Combat_Expert"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_COMBAT_DECAL_1004""},""128667740"":{""id"":128667740,""category"":""decal"",""name"":""Decal_Combat_Master"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_COMBAT_DECAL_1005""},""128667741"":{""id"":128667741,""category"":""decal"",""name"":""Decal_Combat_Dangerous"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_COMBAT_DECAL_1006""},""128671838"":{""id"":128671838,""category"":""decal"",""name"":""Decal_Founders_Reversed"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_FOUNDER_DECAL_1000""},""128667744"":{""id"":128667744,""category"":""decal"",""name"":""Decal_Trade_Mostly_Penniless"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1001""},""128667745"":{""id"":128667745,""category"":""decal"",""name"":""Decal_Trade_Peddler"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1002""},""128667746"":{""id"":128667746,""category"":""decal"",""name"":""Decal_Trade_Dealer"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1003""},""128667747"":{""id"":128667747,""category"":""decal"",""name"":""Decal_Trade_Merchant"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1004""},""128667748"":{""id"":128667748,""category"":""decal"",""name"":""Decal_Trade_Broker"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1005""},""128667749"":{""id"":128667749,""category"":""decal"",""name"":""Decal_Trade_Entrepeneur"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1006""},""128667750"":{""id"":128667750,""category"":""decal"",""name"":""Decal_Trade_Tycoon"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1007""},""128667751"":{""id"":128667751,""category"":""decal"",""name"":""Decal_Trade_Elite"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1008""},""128667752"":{""id"":128667752,""category"":""decal"",""name"":""Decal_Explorer_Mostly_Aimless"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_EXPLORE_DECAL_1001""},""128667753"":{""id"":128667753,""category"":""decal"",""name"":""Decal_Explorer_Scout"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_EXPLORE_DECAL_1002""},""128667754"":{""id"":128667754,""category"":""decal"",""name"":""Decal_Explorer_Surveyor"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_EXPLORE_DECAL_1003""},""128667755"":{""id"":128667755,""category"":""decal"",""name"":""Decal_Explorer_Trailblazer"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_EXPLORE_DECAL_1004""}}},""ship"":{""name"":""Cutter"",""modules"":{""MediumHardpoint2"":{""module"":{""id"":128049501,""name"":""Hpt_MineLauncher_Fixed_Medium"",""value"":279560,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":3,""hopper"":72}}},""MediumHardpoint1"":{""module"":{""id"":128049501,""name"":""Hpt_MineLauncher_Fixed_Medium"",""value"":279560,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":3,""hopper"":72}}},""MediumHardpoint4"":{""module"":{""id"":128049389,""name"":""Hpt_PulseLaser_Turret_Medium"",""value"":113619,""unloaned"":113619,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":1,""hopper"":1}}},""MediumHardpoint3"":{""module"":{""id"":128049389,""name"":""Hpt_PulseLaser_Turret_Medium"",""value"":113619,""unloaned"":113619,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":1,""hopper"":1}}},""LargeHardpoint2"":{""module"":{""id"":128049461,""name"":""Hpt_MultiCannon_Gimbal_Large"",""value"":549876,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":90,""hopper"":2100}}},""LargeHardpoint1"":{""module"":{""id"":128049461,""name"":""Hpt_MultiCannon_Gimbal_Large"",""value"":549876,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":90,""hopper"":2100}}},""HugeHardpoint1"":{""module"":{""id"":128049458,""name"":""Hpt_MultiCannon_Fixed_Huge"",""value"":1119456,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":90,""hopper"":2100}}},""TinyHardpoint1"":{""module"":{""id"":128049522,""name"":""Hpt_PlasmaPointDefence_Turret_Tiny"",""value"":17631,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":12,""hopper"":10000}}},""TinyHardpoint2"":{""module"":{""id"":128668536,""name"":""Hpt_ShieldBooster_Size0_Class5"",""value"":240414,""unloaned"":240414,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":null,""hopper"":null}}},""TinyHardpoint3"":{""module"":{""id"":128049522,""name"":""Hpt_PlasmaPointDefence_Turret_Tiny"",""value"":17631,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":12,""hopper"":10000}}},""TinyHardpoint4"":{""module"":{""id"":128668536,""name"":""Hpt_ShieldBooster_Size0_Class5"",""value"":240414,""unloaned"":240414,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":null,""hopper"":null}}},""TinyHardpoint5"":{""module"":{""id"":128668536,""name"":""Hpt_ShieldBooster_Size0_Class5"",""value"":240414,""unloaned"":240414,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":null,""hopper"":null}}},""TinyHardpoint6"":{""module"":{""id"":128668536,""name"":""Hpt_ShieldBooster_Size0_Class5"",""value"":240414,""unloaned"":240414,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":null,""hopper"":null}}},""TinyHardpoint7"":{""module"":{""id"":128668536,""name"":""Hpt_ShieldBooster_Size0_Class5"",""value"":240414,""unloaned"":240414,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":null,""hopper"":null}}},""TinyHardpoint8"":{""module"":{""id"":128049519,""name"":""Hpt_HeatSinkLauncher_Turret_Tiny"",""value"":3328,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":1,""hopper"":2}}},""Decal1"":[],""Decal2"":[],""Decal3"":[],""PaintJob"":{""module"":{""id"":128672781,""name"":""PaintJob_Cutter_BlackFriday_01"",""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":null,""hopper"":null}}},""Armour"":{""module"":{""name"":""Cutter_Armour_Grade1"",""id"":128049376,""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1}},""PowerPlant"":{""module"":{""id"":128064067,""name"":""Int_Powerplant_Size8_Class5"",""value"":139102901,""unloaned"":139102901,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":null,""hopper"":null}}},""MainEngines"":{""module"":{""id"":128064102,""name"":""Int_Engine_Size8_Class5"",""value"":139102901,""unloaned"":139102901,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":null,""hopper"":null}}},""FrameShiftDrive"":{""module"":{""id"":128064132,""name"":""Int_Hyperdrive_Size7_Class5"",""value"":43881042,""unloaned"":43881042,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":null,""hopper"":null}}},""LifeSupport"":{""module"":{""id"":128064169,""name"":""Int_LifeSupport_Size7_Class2"",""value"":532882,""unloaned"":532882,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":null,""hopper"":null}}},""PowerDistributor"":{""module"":{""id"":128064212,""name"":""Int_PowerDistributor_Size7_Class5"",""value"":8326271,""unloaned"":8326271,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":null,""hopper"":null}}},""Radar"":{""module"":{""id"":128064249,""name"":""Int_Sensors_Size7_Class2"",""value"":532882,""unloaned"":532882,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":null,""hopper"":null}}},""FuelTank"":{""module"":{""name"":""Int_FuelTank_Size6_Class3"",""id"":128064351,""value"":292240,""unloaned"":292240,""free"":false,""health"":1000000,""on"":true,""priority"":1}},""Slot01_Size8"":{""module"":{""id"":128064345,""name"":""Int_CargoRack_Size8_Class1"",""value"":3276691,""unloaned"":3276691,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":null,""hopper"":null}}},""Slot02_Size8"":{""module"":{""id"":128064345,""name"":""Int_CargoRack_Size8_Class1"",""value"":3276691,""unloaned"":3276691,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":null,""hopper"":null}}},""Slot03_Size6"":{""module"":{""id"":128064287,""name"":""Int_ShieldGenerator_Size6_Class5"",""value"":13842601,""unloaned"":13842601,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":null,""hopper"":null}}},""Slot04_Size6"":{""module"":{""id"":128064343,""name"":""Int_CargoRack_Size6_Class1"",""value"":310220,""unloaned"":310220,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":null,""hopper"":null}}},""Slot05_Size6"":{""module"":{""id"":128666681,""name"":""Int_FuelScoop_Size6_Class5"",""value"":27343407,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""Slot06_Size5"":{""module"":{""id"":128064342,""name"":""Int_CargoRack_Size5_Class1"",""value"":106058,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Slot07_Size5"":{""module"":{""id"":128064342,""name"":""Int_CargoRack_Size5_Class1"",""value"":95453,""unloaned"":95453,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":null,""hopper"":null}}},""Slot08_Size4"":{""module"":{""id"":128663561,""name"":""Int_StellarBodyDiscoveryScanner_Advanced"",""value"":1468716,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Slot09_Size3"":{""module"":{""id"":128049549,""name"":""Int_DockingComputer_Standard"",""value"":3851,""unloaned"":3851,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":null,""hopper"":null}}},""PlanetaryApproachSuite"":{""module"":{""name"":""Int_PlanetApproachSuite"",""id"":128672317,""value"":427,""unloaned"":427,""free"":false,""health"":1000000,""on"":true,""priority"":1}},""Bobble01"":[],""Bobble02"":[],""Bobble03"":[],""Bobble04"":[],""Bobble05"":[],""Bobble06"":[],""Bobble07"":[],""Bobble08"":[],""Bobble09"":[],""Bobble10"":[]},""value"":{""hull"":171049542,""modules"":385741460,""cargo"":0,""total"":556791002,""unloaned"":354006361},""free"":false,""alive"":true,""health"":{""hull"":1000000,""shield"":1000000,""shieldup"":true,""integrity"":0,""paintwork"":0},""wear"":{""dirt"":0,""fade"":0,""tear"":0,""game"":0},""cockpitBreached"":false,""oxygenRemaining"":450000,""fuel"":{""main"":{""capacity"":64,""level"":62.84},""reserve"":{""capacity"":1.16,""level"":1.03964304}},""cargo"":{""capacity"":640,""qty"":569,""items"":[{""commodity"":""explosives"",""origin"":128071672,""powerplayOrigin"":null,""masq"":null,""owner"":830153,""mission"":3693089,""qty"":108,""value"":0,""xyz"":null,""marked"":0},{""commodity"":""surfacestabilisers"",""origin"":128071672,""powerplayOrigin"":null,""masq"":null,""owner"":830153,""mission"":3693420,""qty"":144,""value"":0,""xyz"":null,""marked"":0},{""commodity"":""surfacestabilisers"",""origin"":128071672,""powerplayOrigin"":null,""masq"":null,""owner"":830153,""mission"":3693648,""qty"":66,""value"":0,""xyz"":null,""marked"":0},{""commodity"":""surfacestabilisers"",""origin"":128071672,""powerplayOrigin"":null,""masq"":null,""owner"":830153,""mission"":3693891,""qty"":135,""value"":0,""xyz"":null,""marked"":0},{""commodity"":""semiconductors"",""origin"":128071672,""powerplayOrigin"":null,""masq"":null,""owner"":830153,""mission"":3694187,""qty"":108,""value"":0,""xyz"":null,""marked"":0},{""commodity"":""superconductors"",""origin"":128071672,""powerplayOrigin"":null,""masq"":null,""owner"":830153,""mission"":3694301,""qty"":8,""value"":0,""xyz"":null,""marked"":0}],""lock"":1974701932,""ts"":{""sec"":1465938486,""usec"":496000}},""passengers"":[],""refinery"":null,""id"":5},""ships"":{""5"":{""name"":""Cutter"",""alive"":true,""station"":{""id"":3222794496,""name"":""Davies City""},""starsystem"":{""id"":""5031721865930"",""name"":""Devane"",""systemaddress"":""5031721865930""},""id"":5},""23"":{""name"":""Adder"",""alive"":true,""station"":{""id"":128666762,""name"":""Jameson Memorial""},""starsystem"":{""id"":""69268"",""name"":""Shinrarta Dezhra"",""systemaddress"":""3932277478106""},""id"":23},""27"":{""name"":""Type6"",""alive"":true,""station"":{""id"":128666762,""name"":""Jameson Memorial""},""starsystem"":{""id"":""69268"",""name"":""Shinrarta Dezhra"",""systemaddress"":""3932277478106""},""id"":27},""30"":{""name"":""Type7"",""alive"":true,""station"":{""id"":128666762,""name"":""Jameson Memorial""},""starsystem"":{""id"":""69268"",""name"":""Shinrarta Dezhra"",""systemaddress"":""3932277478106""},""id"":30},""32"":{""name"":""FerDeLance"",""alive"":true,""station"":{""id"":128666762,""name"":""Jameson Memorial""},""starsystem"":{""id"":""69268"",""name"":""Shinrarta Dezhra"",""systemaddress"":""3932277478106""},""id"":32},""37"":{""name"":""Empire_Courier"",""alive"":true,""station"":{""id"":128666762,""name"":""Jameson Memorial""},""starsystem"":{""id"":""69268"",""name"":""Shinrarta Dezhra"",""systemaddress"":""3932277478106""},""id"":37},""38"":{""name"":""Federation_Dropship"",""alive"":true,""station"":{""id"":128666762,""name"":""Jameson Memorial""},""starsystem"":{""id"":""69268"",""name"":""Shinrarta Dezhra"",""systemaddress"":""3932277478106""},""id"":38},""39"":{""name"":""Empire_Trader"",""alive"":true,""station"":{""id"":128666762,""name"":""Jameson Memorial""},""starsystem"":{""id"":""69268"",""name"":""Shinrarta Dezhra"",""systemaddress"":""3932277478106""},""id"":39},""40"":{""name"":""Federation_Dropship_MkII"",""alive"":true,""station"":{""id"":128666762,""name"":""Jameson Memorial""},""starsystem"":{""id"":""69268"",""name"":""Shinrarta Dezhra"",""systemaddress"":""3932277478106""},""id"":40},""41"":{""name"":""Federation_Gunship"",""alive"":true,""station"":{""id"":128666762,""name"":""Jameson Memorial""},""starsystem"":{""id"":""69268"",""name"":""Shinrarta Dezhra"",""systemaddress"":""3932277478106""},""id"":41},""46"":{""name"":""Python"",""alive"":true,""station"":{""id"":3226081024,""name"":""Bessel Gateway""},""starsystem"":{""id"":""9468120933825"",""name"":""Orang"",""systemaddress"":""9468120933825""},""id"":46},""48"":{""name"":""CobraMkIV"",""alive"":true,""station"":{""id"":128666762,""name"":""Jameson Memorial""},""starsystem"":{""id"":""69268"",""name"":""Shinrarta Dezhra"",""systemaddress"":""3932277478106""},""id"":48},""49"":{""name"":""Independant_Trader"",""alive"":true,""station"":{""id"":128666762,""name"":""Jameson Memorial""},""starsystem"":{""id"":""69268"",""name"":""Shinrarta Dezhra"",""systemaddress"":""3932277478106""},""id"":49},""53"":{""name"":""SideWinder"",""alive"":true,""station"":{""id"":3223343616,""name"":""Ray Gateway""},""starsystem"":{""id"":""670417429889"",""name"":""Diaguandri"",""systemaddress"":""670417429889""},""id"":53},""54"":{""name"":""Eagle"",""alive"":true,""station"":{""id"":3223343616,""name"":""Ray Gateway""},""starsystem"":{""id"":""670417429889"",""name"":""Diaguandri"",""systemaddress"":""670417429889""},""id"":54},""55"":{""name"":""Hauler"",""alive"":true,""station"":{""id"":3223343616,""name"":""Ray Gateway""},""starsystem"":{""id"":""670417429889"",""name"":""Diaguandri"",""systemaddress"":""670417429889""},""id"":55},""56"":{""name"":""Viper"",""alive"":true,""station"":{""id"":3223343616,""name"":""Ray Gateway""},""starsystem"":{""id"":""670417429889"",""name"":""Diaguandri"",""systemaddress"":""670417429889""},""id"":56},""57"":{""name"":""CobraMkIII"",""alive"":true,""station"":{""id"":3223343616,""name"":""Ray Gateway""},""starsystem"":{""id"":""670417429889"",""name"":""Diaguandri"",""systemaddress"":""670417429889""},""id"":57},""58"":{""name"":""Viper_MkIV"",""alive"":true,""station"":{""id"":3223343616,""name"":""Ray Gateway""},""starsystem"":{""id"":""670417429889"",""name"":""Diaguandri"",""systemaddress"":""670417429889""},""id"":58},""59"":{""name"":""DiamondBack"",""alive"":true,""station"":{""id"":3223343616,""name"":""Ray Gateway""},""starsystem"":{""id"":""670417429889"",""name"":""Diaguandri"",""systemaddress"":""670417429889""},""id"":59},""60"":{""name"":""DiamondBackXL"",""alive"":true,""station"":{""id"":3223343616,""name"":""Ray Gateway""},""starsystem"":{""id"":""670417429889"",""name"":""Diaguandri"",""systemaddress"":""670417429889""},""id"":60},""61"":{""name"":""Asp_Scout"",""alive"":true,""station"":{""id"":3223343616,""name"":""Ray Gateway""},""starsystem"":{""id"":""670417429889"",""name"":""Diaguandri"",""systemaddress"":""670417429889""},""id"":61},""62"":{""name"":""Vulture"",""alive"":true,""station"":{""id"":3223343616,""name"":""Ray Gateway""},""starsystem"":{""id"":""670417429889"",""name"":""Diaguandri"",""systemaddress"":""670417429889""},""id"":62},""63"":{""name"":""Asp"",""alive"":true,""station"":{""id"":3223343616,""name"":""Ray Gateway""},""starsystem"":{""id"":""670417429889"",""name"":""Diaguandri"",""systemaddress"":""670417429889""},""id"":63},""64"":{""name"":""Orca"",""alive"":true,""station"":{""id"":3223343616,""name"":""Ray Gateway""},""starsystem"":{""id"":""670417429889"",""name"":""Diaguandri"",""systemaddress"":""670417429889""},""id"":64},""65"":{""name"":""Type9"",""alive"":true,""station"":{""id"":3223343616,""name"":""Ray Gateway""},""starsystem"":{""id"":""670417429889"",""name"":""Diaguandri"",""systemaddress"":""670417429889""},""id"":65},""66"":{""name"":""Anaconda"",""alive"":true,""station"":{""id"":3223343616,""name"":""Ray Gateway""},""starsystem"":{""id"":""670417429889"",""name"":""Diaguandri"",""systemaddress"":""670417429889""},""id"":66},""67"":{""name"":""Asp"",""alive"":true,""station"":{""id"":3226081024,""name"":""Bessel Gateway""},""starsystem"":{""id"":""9468120933825"",""name"":""Orang"",""systemaddress"":""9468120933825""},""id"":67},""69"":{""name"":""Federation_Corvette"",""alive"":true,""station"":{""id"":128666762,""name"":""Jameson Memorial""},""starsystem"":{""id"":""69268"",""name"":""Shinrarta Dezhra"",""systemaddress"":""3932277478106""},""id"":69}}}";
            JObject json = JObject.Parse(data);

            Ship ship = FrontierApi.ShipFromJson((JObject)json["ship"]);
        }

        [TestMethod]
        public void TestCommanderFromProfile5()
        {
            string data = @"{""commander"":{""id"":1,""name"":""TestForInternals"",""credits"":2,""debt"":0,""currentShipId"":7,""alive"":true,""docked"":false,""rank"":{""combat"":3,""trade"":7,""explore"":5,""crime"":0,""service"":0,""empire"":14,""federation"":14,""power"":4,""cqc"":0}},""lastSystem"":{""id"":""5379983297336"",""name"":""Col 285 Sector AF-A a30-0""},""lastStarport"":{""id"":""3229756160"",""name"":""Garay Terminal"",""faction"":""Federation"",""commodities"":[{""id"":""128049204"",""name"":""Explosives"",""cost_min"":247.91608695652,""cost_max"":498.08391304348,""cost_mean"":""261.00"",""homebuy"":""59"",""homesell"":""55"",""consumebuy"":""4"",""baseCreationQty"":7.4,""baseConsumptionQty"":0,""capacity"":9856.99,""buyPrice"":183,""sellPrice"":169,""meanPrice"":261,""demandBracket"":0,""stockBracket"":2,""creationQty"":6710,""consumptionQty"":0,""targetStock"":6710,""stock"":6616.539,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.1400"",""sec_illegal_min"":""1.21"",""sec_illegal_max"":""2.69"",""stolenmod"":""0.7500""},{""id"":""128049202"",""name"":""Hydrogen Fuel"",""cost_min"":110.82434782609,""cost_max"":""164.00"",""cost_mean"":""110.00"",""homebuy"":""73"",""homesell"":""70"",""consumebuy"":""3"",""baseCreationQty"":200,""baseConsumptionQty"":200,""capacity"":677318.764,""buyPrice"":97,""sellPrice"":92,""meanPrice"":110,""demandBracket"":0,""stockBracket"":2,""creationQty"":362688,""consumptionQty"":91279,""targetStock"":385507,""stock"":404403.96,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.3000"",""sec_illegal_min"":""1.13"",""sec_illegal_max"":""2.10"",""stolenmod"":""0.7500""},{""id"":""128673850"",""name"":""Hydrogen Peroxide"",""cost_min"":""589.00"",""cost_max"":860.65586956522,""cost_mean"":""917.00"",""homebuy"":""70"",""homesell"":""67"",""consumebuy"":""3"",""baseCreationQty"":0,""baseConsumptionQty"":812.6,""capacity"":2210431.5,""buyPrice"":0,""sellPrice"":836,""meanPrice"":917,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":1473621,""targetStock"":368404,""stock"":0,""demand"":1547304.3,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.2700"",""sec_illegal_min"":""1.14"",""sec_illegal_max"":""2.19"",""stolenmod"":""0.7500""},{""id"":""128673851"",""name"":""Liquid Oxygen"",""cost_min"":""223.00"",""cost_max"":400.96065217391,""cost_mean"":""263.00"",""homebuy"":""51"",""homesell"":""46"",""consumebuy"":""5"",""baseCreationQty"":10.6,""baseConsumptionQty"":421.6,""capacity"":1159890.797,""buyPrice"":0,""sellPrice"":378,""meanPrice"":263,""demandBracket"":2,""stockBracket"":0,""creationQty"":14417,""consumptionQty"":764556,""targetStock"":205556,""stock"":0,""demand"":787276.279,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.0700"",""sec_illegal_min"":""1.25"",""sec_illegal_max"":""3.04"",""stolenmod"":""0.7500""},{""id"":""128049203"",""name"":""Mineral Oil"",""cost_min"":146.05913043478,""cost_max"":364.94086956522,""cost_mean"":""181.00"",""homebuy"":""46"",""homesell"":""41"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":194.2,""capacity"":509262.402,""buyPrice"":0,""sellPrice"":147,""meanPrice"":181,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":352187,""targetStock"":88046,""stock"":0,""demand"":12789.11575,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.0300"",""sec_illegal_min"":""1.21"",""sec_illegal_max"":""2.69"",""stolenmod"":""0.7500""},{""id"":""128672305"",""name"":""Surface Stabilisers"",""cost_min"":460.23369565217,""cost_max"":782.76630434783,""cost_mean"":""467.00"",""homebuy"":""69"",""homesell"":""66"",""consumebuy"":""3"",""baseCreationQty"":41.8,""baseConsumptionQty"":0,""capacity"":84766.332,""buyPrice"":321,""sellPrice"":304,""meanPrice"":467,""demandBracket"":0,""stockBracket"":3,""creationQty"":56852,""consumptionQty"":0,""targetStock"":56852,""stock"":96170.642998,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.2500"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128049166"",""name"":""Water"",""cost_min"":""124.00"",""cost_max"":287.06956521739,""cost_mean"":""120.00"",""homebuy"":""18"",""homesell"":""10"",""consumebuy"":""8"",""baseCreationQty"":0,""baseConsumptionQty"":163,""capacity"":419449.305,""buyPrice"":0,""sellPrice"":124,""meanPrice"":120,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":295595,""targetStock"":73898,""stock"":0,""demand"":10678.6885,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128049241"",""name"":""Clothing"",""cost_min"":262.20260869565,""cost_max"":""463.00"",""cost_mean"":""285.00"",""homebuy"":""60"",""homesell"":""56"",""consumebuy"":""4"",""baseCreationQty"":28,""baseConsumptionQty"":70,""capacity"":66294.855,""buyPrice"":0,""sellPrice"":263,""meanPrice"":285,""demandBracket"":1,""stockBracket"":0,""creationQty"":12695,""consumptionQty"":31948,""targetStock"":20682,""stock"":0,""demand"":0,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Consumer Items"",""volumescale"":""1.1500"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.65"",""stolenmod"":""0.7500""},{""id"":""128049240"",""name"":""Consumer Technology"",""cost_min"":""6561.00"",""cost_max"":7364.7391304348,""cost_mean"":""6769.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":27,""capacity"":32570.386,""buyPrice"":0,""sellPrice"":7292,""meanPrice"":6769,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":21874,""targetStock"":5467,""stock"":0,""demand"":22800.857,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Consumer Items"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.11"",""stolenmod"":""0.7500""},{""id"":""128049238"",""name"":""Domestic Appliances"",""cost_min"":460.23369565217,""cost_max"":""716.00"",""cost_mean"":""487.00"",""homebuy"":""69"",""homesell"":""66"",""consumebuy"":""3"",""baseCreationQty"":16.8,""baseConsumptionQty"":41.8,""capacity"":26764.941,""buyPrice"":0,""sellPrice"":461,""meanPrice"":487,""demandBracket"":1,""stockBracket"":0,""creationQty"":7617,""consumptionQty"":10334,""targetStock"":10200,""stock"":0,""demand"":0,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Consumer Items"",""volumescale"":""1.2500"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128682048"",""name"":""Survival Equipment"",""cost_min"":920.46739130435,""cost_max"":1432,""cost_mean"":""485.00"",""homebuy"":""69"",""homesell"":""66"",""consumebuy"":""3"",""baseCreationQty"":9.6,""baseConsumptionQty"":0,""capacity"":6507.735,""buyPrice"":642,""sellPrice"":608,""meanPrice"":485,""demandBracket"":0,""stockBracket"":3,""creationQty"":4353,""consumptionQty"":0,""targetStock"":4353,""stock"":7770.9832395,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Consumer Items"",""volumescale"":""1.2500"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128049177"",""name"":""Algae"",""cost_min"":""135.00"",""cost_max"":308.16173913043,""cost_mean"":""137.00"",""homebuy"":""27"",""homesell"":""20"",""consumebuy"":""7"",""baseCreationQty"":0,""baseConsumptionQty"":1008,""capacity"":645001.588,""buyPrice"":0,""sellPrice"":135,""meanPrice"":137,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":440876,""targetStock"":110219,""stock"":0,""demand"":15981.755,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.06"",""sec_illegal_max"":""1.48"",""stolenmod"":""0.7500""},{""id"":""128049182"",""name"":""Animalmeat"",""cost_min"":""1286.00"",""cost_max"":1695.1776086957,""cost_mean"":""1292.00"",""homebuy"":""80"",""homesell"":""78"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":97,""capacity"":62245.84,""buyPrice"":0,""sellPrice"":1286,""meanPrice"":1292,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":42058,""targetStock"":10514,""stock"":0,""demand"":1556.368,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.4000"",""sec_illegal_min"":""1.10"",""sec_illegal_max"":""1.81"",""stolenmod"":""0.7500""},{""id"":""128049189"",""name"":""Coffee"",""cost_min"":""1286.00"",""cost_max"":1695.1776086957,""cost_mean"":""1279.00"",""homebuy"":""80"",""homesell"":""78"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":97,""capacity"":17745.2,""buyPrice"":0,""sellPrice"":1286,""meanPrice"":1279,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":11990,""targetStock"":2997,""stock"":0,""demand"":443.852,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.4000"",""sec_illegal_min"":""1.10"",""sec_illegal_max"":""1.81"",""stolenmod"":""0.7500""},{""id"":""128049183"",""name"":""Fish"",""cost_min"":""403.00"",""cost_max"":627.93,""cost_mean"":""406.00"",""homebuy"":""64"",""homesell"":""60"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":271,""capacity"":178114.384,""buyPrice"":0,""sellPrice"":403,""meanPrice"":406,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":120592,""targetStock"":30147,""stock"":0,""demand"":4394.51425,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.2000"",""sec_illegal_min"":""1.18"",""sec_illegal_max"":""2.44"",""stolenmod"":""0.7500""},{""id"":""128049184"",""name"":""Food Cartridges"",""cost_min"":95.201739130435,""cost_max"":""267.00"",""cost_mean"":""105.00"",""homebuy"":""30"",""homesell"":""23"",""consumebuy"":""7"",""baseCreationQty"":36,""baseConsumptionQty"":90.4,""capacity"":99945.285,""buyPrice"":29,""sellPrice"":22,""meanPrice"":105,""demandBracket"":0,""stockBracket"":3,""creationQty"":65284,""consumptionQty"":2201,""targetStock"":65834,""stock"":107699.90587661,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128049178"",""name"":""Fruit And Vegetables"",""cost_min"":""315.00"",""cost_max"":515.79739130435,""cost_mean"":""312.00"",""homebuy"":""60"",""homesell"":""56"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":350,""capacity"":63337.032,""buyPrice"":0,""sellPrice"":315,""meanPrice"":312,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":43263,""targetStock"":10815,""stock"":0,""demand"":1611.876,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.1500"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.65"",""stolenmod"":""0.7500""},{""id"":""128049180"",""name"":""Grain"",""cost_min"":""207.00"",""cost_max"":380.8852173913,""cost_mean"":""210.00"",""homebuy"":""48"",""homesell"":""43"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":584,""capacity"":390205.776,""buyPrice"":0,""sellPrice"":207,""meanPrice"":210,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":266534,""targetStock"":66632,""stock"":0,""demand"":9929.05,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.0500"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.65"",""stolenmod"":""0.7500""},{""id"":""128049185"",""name"":""Synthetic Meat"",""cost_min"":""252.00"",""cost_max"":436.51652173913,""cost_mean"":""271.00"",""homebuy"":""54"",""homesell"":""49"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":226,""capacity"":41654.067,""buyPrice"":0,""sellPrice"":420,""meanPrice"":271,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":27937,""targetStock"":6984,""stock"":0,""demand"":29159.691,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.23"",""sec_illegal_max"":""2.88"",""stolenmod"":""0.7500""},{""id"":""128049188"",""name"":""Tea"",""cost_min"":""1459.00"",""cost_max"":1896.6086956522,""cost_mean"":""1467.00"",""homebuy"":""81"",""homesell"":""79"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":88,""capacity"":57042.18,""buyPrice"":0,""sellPrice"":1459,""meanPrice"":1467,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":38490,""targetStock"":9622,""stock"":0,""demand"":1438.786,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.4200"",""sec_illegal_min"":""1.09"",""sec_illegal_max"":""1.76"",""stolenmod"":""0.7500""},{""id"":""128673856"",""name"":""C M M Composite"",""cost_min"":""2966.00"",""cost_max"":3605.947826087,""cost_mean"":""3132.00"",""homebuy"":""87"",""homesell"":""86"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":60,""capacity"":163218,""buyPrice"":0,""sellPrice"":3548,""meanPrice"":3132,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":108812,""targetStock"":27203,""stock"":0,""demand"":114252.6,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.3400"",""sec_illegal_min"":""1.05"",""sec_illegal_max"":""1.37"",""stolenmod"":""0.7500""},{""id"":""128672302"",""name"":""Ceramic Composites"",""cost_min"":""192.00"",""cost_max"":364.94086956522,""cost_mean"":""232.00"",""homebuy"":""46"",""homesell"":""41"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":776.8,""capacity"":2056769.16,""buyPrice"":0,""sellPrice"":350,""meanPrice"":232,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":1408746,""targetStock"":352186,""stock"":0,""demand"":1440091.474,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.0300"",""sec_illegal_min"":""1.28"",""sec_illegal_max"":""3.28"",""stolenmod"":""0.7500""},{""id"":""128673855"",""name"":""Insulating Membrane"",""cost_min"":""7498.00"",""cost_max"":8450.2097826087,""cost_mean"":""7837.00"",""homebuy"":""93"",""homesell"":""92"",""consumebuy"":""1"",""baseCreationQty"":0.4,""baseConsumptionQty"":28.8,""capacity"":79162.5,""buyPrice"":0,""sellPrice"":8344,""meanPrice"":7837,""demandBracket"":2,""stockBracket"":0,""creationQty"":545,""consumptionQty"":52230,""targetStock"":13602,""stock"":0,""demand"":54482.7,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.0600"",""sec_illegal_min"":""1.02"",""sec_illegal_max"":""1.15"",""stolenmod"":""0.7500""},{""id"":""128049197"",""name"":""Polymers"",""cost_min"":""152.00"",""cost_max"":320.85565217391,""cost_mean"":""171.00"",""homebuy"":""35"",""homesell"":""29"",""consumebuy"":""7"",""baseCreationQty"":19.6,""baseConsumptionQty"":1170.4,""capacity"":3135693.013,""buyPrice"":0,""sellPrice"":294,""meanPrice"":171,""demandBracket"":2,""stockBracket"":0,""creationQty"":26658,""consumptionQty"":2122549,""targetStock"":557295,""stock"":0,""demand"":2067920.956,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.23"",""sec_illegal_max"":""2.88"",""stolenmod"":""0.7500""},{""id"":""128049199"",""name"":""Semiconductors"",""cost_min"":""889.00"",""cost_max"":1280.847826087,""cost_mean"":""967.00"",""homebuy"":""76"",""homesell"":""74"",""consumebuy"":""2"",""baseCreationQty"":26.4,""baseConsumptionQty"":158.4,""capacity"":483139.15,""buyPrice"":0,""sellPrice"":1155,""meanPrice"":967,""demandBracket"":2,""stockBracket"":0,""creationQty"":35907,""consumptionQty"":287263,""targetStock"":107722,""stock"":0,""demand"":276772.022,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.3400"",""sec_illegal_min"":""1.12"",""sec_illegal_max"":""1.98"",""stolenmod"":""0.7500""},{""id"":""128049200"",""name"":""Superconductors"",""cost_min"":""6561.00"",""cost_max"":7507.6260869565,""cost_mean"":""6609.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":27,""baseConsumptionQty"":32,""capacity"":141091.684,""buyPrice"":0,""sellPrice"":6561,""meanPrice"":6609,""demandBracket"":1,""stockBracket"":0,""creationQty"":36723,""consumptionQty"":58033,""targetStock"":51231,""stock"":0,""demand"":23219.377,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.02"",""sec_illegal_max"":""1.18"",""stolenmod"":""0.7500""},{""id"":""128064028"",""name"":""Atmospheric Extractors"",""cost_min"":344.07,""cost_max"":""569.00"",""cost_mean"":""357.00"",""homebuy"":""64"",""homesell"":""60"",""consumebuy"":""4"",""baseCreationQty"":432.8,""baseConsumptionQty"":0,""capacity"":289822.848,""buyPrice"":257,""sellPrice"":238,""meanPrice"":357,""demandBracket"":0,""stockBracket"":2,""creationQty"":196224,""consumptionQty"":0,""targetStock"":196224,""stock"":194826.105,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.2000"",""sec_illegal_min"":""1.18"",""sec_illegal_max"":""2.44"",""stolenmod"":""0.7500""},{""id"":""128672309"",""name"":""Building Fabricators"",""cost_min"":975.18,""cost_max"":""1344.00"",""cost_mean"":""980.00"",""homebuy"":""78"",""homesell"":""76"",""consumebuy"":""2"",""baseCreationQty"":452.8,""baseConsumptionQty"":0,""capacity"":908988.696,""buyPrice"":769,""sellPrice"":742,""meanPrice"":980,""demandBracket"":0,""stockBracket"":3,""creationQty"":615846,""consumptionQty"":0,""targetStock"":615846,""stock"":1090987.287522,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.3700"",""sec_illegal_min"":""1.11"",""sec_illegal_max"":""1.89"",""stolenmod"":""0.7500""},{""id"":""128049222"",""name"":""Crop Harvesters"",""cost_min"":2233.0903558696,""cost_max"":2831.277,""cost_mean"":""2021.00"",""homebuy"":""84"",""homesell"":""82"",""consumebuy"":""2"",""baseCreationQty"":514.4,""baseConsumptionQty"":0,""capacity"":1030550.571,""buyPrice"":2013,""sellPrice"":1946,""meanPrice"":2021,""demandBracket"":0,""stockBracket"":2,""creationQty"":699627,""consumptionQty"":0,""targetStock"":699627,""stock"":692684.72,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.4400"",""sec_illegal_min"":""1.06"",""sec_illegal_max"":""1.46"",""stolenmod"":""0.7500""},{""id"":""128673861"",""name"":""Emergency Power Cells"",""cost_min"":""889.00"",""cost_max"":1224.7608695652,""cost_mean"":""1011.00"",""homebuy"":""76"",""homesell"":""74"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":52.8,""capacity"":107367.91,""buyPrice"":0,""sellPrice"":1195,""meanPrice"":1011,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":71818,""targetStock"":17954,""stock"":0,""demand"":75176.388,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.3400"",""sec_illegal_min"":""1.12"",""sec_illegal_max"":""1.98"",""stolenmod"":""0.7500""},{""id"":""128673866"",""name"":""Exhaust Manifold"",""cost_min"":""383.00"",""cost_max"":602.51,""cost_mean"":""479.00"",""homebuy"":""63"",""homesell"":""59"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":113.6,""capacity"":229919.808,""buyPrice"":0,""sellPrice"":583,""meanPrice"":479,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":154516,""targetStock"":38629,""stock"":0,""demand"":160928.414,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.1900"",""sec_illegal_min"":""1.18"",""sec_illegal_max"":""2.48"",""stolenmod"":""0.7500""},{""id"":""128672307"",""name"":""Geological Equipment"",""cost_min"":1647.92,""cost_max"":""2134.00"",""cost_mean"":""1661.00"",""homebuy"":""82"",""homesell"":""80"",""consumebuy"":""2"",""baseCreationQty"":6.4,""baseConsumptionQty"":0,""capacity"":12918.22,""buyPrice"":1365,""sellPrice"":1319,""meanPrice"":1661,""demandBracket"":0,""stockBracket"":3,""creationQty"":8705,""consumptionQty"":0,""targetStock"":8705,""stock"":15289.59977285,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.4500"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.28"",""stolenmod"":""0.7500""},{""id"":""128673860"",""name"":""H N Shock Mount"",""cost_min"":434,""cost_max"":""683.00"",""cost_mean"":""406.00"",""homebuy"":""68"",""homesell"":""65"",""consumebuy"":""3"",""baseCreationQty"":176,""baseConsumptionQty"":0,""capacity"":356908.125,""buyPrice"":299,""sellPrice"":283,""meanPrice"":406,""demandBracket"":0,""stockBracket"":3,""creationQty"":239375,""consumptionQty"":0,""targetStock"":239375,""stock"":428241.875,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.2400"",""sec_illegal_min"":""1.16"",""sec_illegal_max"":""2.30"",""stolenmod"":""0.7500""},{""id"":""128049220"",""name"":""Heliostatic Furnaces"",""cost_min"":152.69739130435,""cost_max"":373.30260869565,""cost_mean"":""236.00"",""homebuy"":""47"",""homesell"":""42"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":61.4,""capacity"":165578.937,""buyPrice"":0,""sellPrice"":354,""meanPrice"":236,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":111351,""targetStock"":27837,""stock"":0,""demand"":115889.892,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.0400"",""sec_illegal_min"":""1.22"",""sec_illegal_max"":""2.78"",""stolenmod"":""0.7500""},{""id"":""128049223"",""name"":""Marine Supplies"",""cost_min"":3964.9560869565,""cost_max"":""4723.00"",""cost_mean"":""3916.00"",""homebuy"":""89"",""homesell"":""88"",""consumebuy"":""1"",""baseCreationQty"":308,""baseConsumptionQty"":0,""capacity"":621656.504,""buyPrice"":3723,""sellPrice"":3645,""meanPrice"":3916,""demandBracket"":0,""stockBracket"":2,""creationQty"":418906,""consumptionQty"":0,""targetStock"":418906,""stock"":417797.666,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.2400"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.28"",""stolenmod"":""0.7500""},{""id"":""128049221"",""name"":""Mineral Extractors"",""cost_min"":659.17294782609,""cost_max"":1131.28,""cost_mean"":""443.00"",""homebuy"":""70"",""homesell"":""67"",""consumebuy"":""3"",""baseCreationQty"":302.4,""baseConsumptionQty"":0,""capacity"":610352.876,""buyPrice"":470,""sellPrice"":445,""meanPrice"":443,""demandBracket"":0,""stockBracket"":3,""creationQty"":411289,""consumptionQty"":0,""targetStock"":411289,""stock"":484948.4529178,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.2700"",""sec_illegal_min"":""1.14"",""sec_illegal_max"":""2.19"",""stolenmod"":""0.7500""},{""id"":""128049217"",""name"":""Power Generators"",""cost_min"":422.43369565217,""cost_max"":""716.00"",""cost_mean"":""458.00"",""homebuy"":""69"",""homesell"":""66"",""consumebuy"":""3"",""baseCreationQty"":16.8,""baseConsumptionQty"":41.8,""capacity"":145813.568,""buyPrice"":0,""sellPrice"":423,""meanPrice"":458,""demandBracket"":1,""stockBracket"":0,""creationQty"":22850,""consumptionQty"":75806,""targetStock"":41801,""stock"":0,""demand"":0,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.2500"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128672313"",""name"":""Skimer Components"",""cost_min"":815.06304347826,""cost_max"":""1203.00"",""cost_mean"":""859.00"",""homebuy"":""76"",""homesell"":""74"",""consumebuy"":""2"",""baseCreationQty"":40,""baseConsumptionQty"":0,""capacity"":27167.728,""buyPrice"":626,""sellPrice"":604,""meanPrice"":859,""demandBracket"":0,""stockBracket"":3,""creationQty"":18136,""consumptionQty"":0,""targetStock"":18136,""stock"":32456.0880761,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.3500"",""sec_illegal_min"":""1.11"",""sec_illegal_max"":""1.95"",""stolenmod"":""0.7500""},{""id"":""128672308"",""name"":""Thermal Cooling Units"",""cost_min"":247.91608695652,""cost_max"":""446.00"",""cost_mean"":""256.00"",""homebuy"":""59"",""homesell"":""55"",""consumebuy"":""4"",""baseCreationQty"":147.2,""baseConsumptionQty"":0,""capacity"":292699.71,""buyPrice"":148,""sellPrice"":137,""meanPrice"":256,""demandBracket"":0,""stockBracket"":3,""creationQty"":200205,""consumptionQty"":0,""targetStock"":200205,""stock"":350188.1441116,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.1400"",""sec_illegal_min"":""1.21"",""sec_illegal_max"":""2.69"",""stolenmod"":""0.7500""},{""id"":""128049218"",""name"":""Water Purifiers"",""cost_min"":247.91608695652,""cost_max"":""446.00"",""cost_mean"":""258.00"",""homebuy"":""59"",""homesell"":""55"",""consumebuy"":""4"",""baseCreationQty"":736,""baseConsumptionQty"":7.4,""capacity"":1483114.204,""buyPrice"":148,""sellPrice"":137,""meanPrice"":258,""demandBracket"":0,""stockBracket"":3,""creationQty"":1001021,""consumptionQty"":13421,""targetStock"":1004376,""stock"":1463539.2027526,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.1400"",""sec_illegal_min"":""1.21"",""sec_illegal_max"":""2.69"",""stolenmod"":""0.7500""},{""id"":""128682046"",""name"":""Advanced Medicines"",""cost_min"":2272,""cost_max"":3067.2043478261,""cost_mean"":""1259.00"",""homebuy"":""78"",""homesell"":""76"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":107,""capacity"":73057.16,""buyPrice"":0,""sellPrice"":2995,""meanPrice"":1259,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":48835,""targetStock"":12208,""stock"":0,""demand"":51143.8,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Medicines"",""volumescale"":""1.3800"",""sec_illegal_min"":""1.10"",""sec_illegal_max"":""1.87"",""stolenmod"":""0.7500""},{""id"":""128049210"",""name"":""Basic Medicines"",""cost_min"":256.28260869565,""cost_max"":""463.00"",""cost_mean"":""279.00"",""homebuy"":""60"",""homesell"":""56"",""consumebuy"":""4"",""baseCreationQty"":56,""baseConsumptionQty"":98,""capacity"":43673.85,""buyPrice"":199,""sellPrice"":184,""meanPrice"":279,""demandBracket"":0,""stockBracket"":2,""creationQty"":25390,""consumptionQty"":4020,""targetStock"":26394,""stock"":26659.416,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Medicines"",""volumescale"":""1.1500"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.65"",""stolenmod"":""0.7500""},{""id"":""128049209"",""name"":""Performance Enhancers"",""cost_min"":""6561.00"",""cost_max"":7488.3913043478,""cost_mean"":""6816.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":27,""capacity"":11850.951,""buyPrice"":0,""sellPrice"":7405,""meanPrice"":6816,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":7959,""targetStock"":1989,""stock"":0,""demand"":8296.608,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Medicines"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.26"",""stolenmod"":""0.7500""},{""id"":""128049669"",""name"":""Progenitor Cells"",""cost_min"":""6561.00"",""cost_max"":7448.547826087,""cost_mean"":""6779.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":27,""capacity"":17126.478,""buyPrice"":0,""sellPrice"":7368,""meanPrice"":6779,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":11502,""targetStock"":2875,""stock"":0,""demand"":11988.853,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Medicines"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.26"",""stolenmod"":""0.7500""},{""id"":""128049176"",""name"":""Aluminium"",""cost_min"":""330.00"",""cost_max"":536.22434782609,""cost_mean"":""340.00"",""homebuy"":""61"",""homesell"":""57"",""consumebuy"":""4"",""baseCreationQty"":66.4,""baseConsumptionQty"":2258.4,""capacity"":5872726.505,""buyPrice"":0,""sellPrice"":505,""meanPrice"":340,""demandBracket"":2,""stockBracket"":0,""creationQty"":90310,""consumptionQty"":4095525,""targetStock"":1114191,""stock"":0,""demand"":3903345.761,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1600"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.60"",""stolenmod"":""0.7500""},{""id"":""128049168"",""name"":""Beryllium"",""cost_min"":""8017.00"",""cost_max"":8987.1695652174,""cost_mean"":""8288.00"",""homebuy"":""93"",""homesell"":""92"",""consumebuy"":""1"",""baseCreationQty"":4.6,""baseConsumptionQty"":92,""capacity"":258778.52,""buyPrice"":0,""sellPrice"":8826,""meanPrice"":8288,""demandBracket"":2,""stockBracket"":0,""creationQty"":6257,""consumptionQty"":166839,""targetStock"":47966,""stock"":0,""demand"":170293.058,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.0400"",""sec_illegal_min"":""1.02"",""sec_illegal_max"":""1.15"",""stolenmod"":""0.7500""},{""id"":""128049162"",""name"":""Cobalt"",""cost_min"":""701.00"",""cost_max"":997.2347826087,""cost_mean"":""647.00"",""homebuy"":""73"",""homesell"":""70"",""consumebuy"":""3"",""baseCreationQty"":32.4,""baseConsumptionQty"":129.6,""capacity"":399938.836,""buyPrice"":0,""sellPrice"":701,""meanPrice"":647,""demandBracket"":1,""stockBracket"":0,""creationQty"":44067,""consumptionQty"":235025,""targetStock"":102823,""stock"":0,""demand"":28968.322,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.3000"",""sec_illegal_min"":""1.13"",""sec_illegal_max"":""2.10"",""stolenmod"":""0.7500""},{""id"":""128049175"",""name"":""Copper"",""cost_min"":""472.00"",""cost_max"":715.58695652174,""cost_mean"":""481.00"",""homebuy"":""67"",""homesell"":""64"",""consumebuy"":""3"",""baseCreationQty"":46.4,""baseConsumptionQty"":742.4,""capacity"":2046482.196,""buyPrice"":0,""sellPrice"":654,""meanPrice"":481,""demandBracket"":2,""stockBracket"":0,""creationQty"":63108,""consumptionQty"":1346315,""targetStock"":399686,""stock"":0,""demand"":1249273.316,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.2300"",""sec_illegal_min"":""1.16"",""sec_illegal_max"":""2.33"",""stolenmod"":""0.7500""},{""id"":""128049170"",""name"":""Gallium"",""cost_min"":""5028.00"",""cost_max"":5863.447826087,""cost_mean"":""5135.00"",""homebuy"":""90"",""homesell"":""89"",""consumebuy"":""1"",""baseCreationQty"":6.6,""baseConsumptionQty"":264,""capacity"":724769.752,""buyPrice"":0,""sellPrice"":5707,""meanPrice"":5135,""demandBracket"":2,""stockBracket"":0,""creationQty"":8977,""consumptionQty"":478755,""targetStock"":128665,""stock"":0,""demand"":468608.76,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1800"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.24"",""stolenmod"":""0.7500""},{""id"":""128049154"",""name"":""Gold"",""cost_min"":""9164.00"",""cost_max"":10172.009782609,""cost_mean"":""9401.00"",""homebuy"":""94"",""homesell"":""93"",""consumebuy"":""1"",""baseCreationQty"":2,""baseConsumptionQty"":166.4,""capacity"":456860.448,""buyPrice"":0,""sellPrice"":10027,""meanPrice"":9401,""demandBracket"":2,""stockBracket"":0,""creationQty"":3627,""consumptionQty"":301761,""targetStock"":79067,""stock"":0,""demand"":306373.033,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.09"",""stolenmod"":""0.7500""},{""id"":""128049169"",""name"":""Indium"",""cost_min"":""5743.00"",""cost_max"":6617.4130434783,""cost_mean"":""5727.00"",""homebuy"":""91"",""homesell"":""90"",""consumebuy"":""1"",""baseCreationQty"":59.6,""baseConsumptionQty"":48,""capacity"":250144.704,""buyPrice"":0,""sellPrice"":5743,""meanPrice"":5727,""demandBracket"":1,""stockBracket"":0,""creationQty"":81061,""consumptionQty"":87047,""targetStock"":102822,""stock"":0,""demand"":36890.754,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1400"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.22"",""stolenmod"":""0.7500""},{""id"":""128049173"",""name"":""Lithium"",""cost_min"":""1555.00"",""cost_max"":2006.6026086957,""cost_mean"":""1596.00"",""homebuy"":""81"",""homesell"":""79"",""consumebuy"":""2"",""baseCreationQty"":16.6,""baseConsumptionQty"":266.4,""capacity"":740828.525,""buyPrice"":0,""sellPrice"":1908,""meanPrice"":1596,""demandBracket"":2,""stockBracket"":0,""creationQty"":22578,""consumptionQty"":483107,""targetStock"":143354,""stock"":0,""demand"":465935.339,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.4300"",""sec_illegal_min"":""1.09"",""sec_illegal_max"":""1.74"",""stolenmod"":""0.7500""},{""id"":""128671118"",""name"":""Osmium"",""cost_min"":8529.3,""cost_max"":9759.9139130435,""cost_mean"":""7591.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":215.2,""capacity"":585387,""buyPrice"":0,""sellPrice"":9649,""meanPrice"":7591,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":390258,""targetStock"":97564,""stock"":0,""demand"":409771.8,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.22"",""stolenmod"":""0.7500""},{""id"":""128049153"",""name"":""Palladium"",""cost_min"":""12815.00"",""cost_max"":13960.415652174,""cost_mean"":""13298.00"",""homebuy"":""96"",""homesell"":""96"",""consumebuy"":""0"",""baseCreationQty"":0,""baseConsumptionQty"":128.8,""capacity"":349895.35,""buyPrice"":0,""sellPrice"":13829,""meanPrice"":13298,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":233575,""targetStock"":58393,""stock"":0,""demand"":240327.028,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.08"",""stolenmod"":""0.7500""},{""id"":""128049152"",""name"":""Platinum"",""cost_min"":21523.2,""cost_max"":23082.087652174,""cost_mean"":""19279.00"",""homebuy"":""97"",""homesell"":""97"",""consumebuy"":""0"",""baseCreationQty"":0,""baseConsumptionQty"":9.6,""capacity"":26115,""buyPrice"":0,""sellPrice"":22941,""meanPrice"":19279,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":17410,""targetStock"":4352,""stock"":0,""demand"":18281.4,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":""0"",""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.04"",""stolenmod"":""0.7500""},{""id"":""128673845"",""name"":""Praseodymium"",""cost_min"":""6138.00"",""cost_max"":7057.7652173913,""cost_mean"":""7156.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":113.6,""capacity"":309015,""buyPrice"":0,""sellPrice"":6975,""meanPrice"":7156,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":206010,""targetStock"":51502,""stock"":0,""demand"":216311.4,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1200"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.28"",""stolenmod"":""0.7500""},{""id"":""128673847"",""name"":""Samarium"",""cost_min"":""5373.00"",""cost_max"":6236.9258695652,""cost_mean"":""6330.00"",""homebuy"":""91"",""homesell"":""90"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":125.6,""capacity"":341656.5,""buyPrice"":0,""sellPrice"":6159,""meanPrice"":6330,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":227771,""targetStock"":56942,""stock"":0,""demand"":239160.9,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1600"",""sec_illegal_min"":""1.04"",""sec_illegal_max"":""1.31"",""stolenmod"":""0.7500""},{""id"":""128049155"",""name"":""Silver"",""cost_min"":""4705.00"",""cost_max"":5513.8260869565,""cost_mean"":""4775.00"",""homebuy"":""90"",""homesell"":""89"",""consumebuy"":""1"",""baseCreationQty"":7,""baseConsumptionQty"":278.4,""capacity"":760268.42,""buyPrice"":0,""sellPrice"":5380,""meanPrice"":4775,""demandBracket"":2,""stockBracket"":0,""creationQty"":9521,""consumptionQty"":504869,""targetStock"":135738,""stock"":0,""demand"":500549.498,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.2000"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.24"",""stolenmod"":""0.7500""},{""id"":""128049171"",""name"":""Tantalum"",""cost_min"":""3858.00"",""cost_max"":4590.3091304348,""cost_mean"":""3962.00"",""homebuy"":""89"",""homesell"":""88"",""consumebuy"":""1"",""baseCreationQty"":81.2,""baseConsumptionQty"":162.4,""capacity"":600534.918,""buyPrice"":0,""sellPrice"":4169,""meanPrice"":3962,""demandBracket"":2,""stockBracket"":0,""creationQty"":110439,""consumptionQty"":294507,""targetStock"":184065,""stock"":0,""demand"":260101.018,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.2600"",""sec_illegal_min"":""1.04"",""sec_illegal_max"":""1.29"",""stolenmod"":""0.7500""},{""id"":""128049174"",""name"":""Titanium"",""cost_min"":""1004.00"",""cost_max"":1361.2576086957,""cost_mean"":""1006.00"",""homebuy"":""77"",""homesell"":""75"",""consumebuy"":""2"",""baseCreationQty"":59.6,""baseConsumptionQty"":952.8,""capacity"":2622947.05,""buyPrice"":0,""sellPrice"":1294,""meanPrice"":1006,""demandBracket"":2,""stockBracket"":0,""creationQty"":81061,""consumptionQty"":1727868,""targetStock"":513028,""stock"":0,""demand"":1693874.05,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.3600"",""sec_illegal_min"":""1.11"",""sec_illegal_max"":""1.92"",""stolenmod"":""0.7500""},{""id"":""128049172"",""name"":""Uranium"",""cost_min"":""2603.00"",""cost_max"":3199.7,""cost_mean"":""2705.00"",""homebuy"":""86"",""homesell"":""85"",""consumebuy"":""1"",""baseCreationQty"":11,""baseConsumptionQty"":662.4,""capacity"":1796325.923,""buyPrice"":0,""sellPrice"":3107,""meanPrice"":2705,""demandBracket"":2,""stockBracket"":0,""creationQty"":14961,""consumptionQty"":1201238,""targetStock"":315270,""stock"":0,""demand"":1193434.819,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.3800"",""sec_illegal_min"":""1.05"",""sec_illegal_max"":""1.39"",""stolenmod"":""0.7500""},{""id"":""128049165"",""name"":""Bauxite"",""cost_min"":107.14434782609,""cost_max"":320.85565217391,""cost_mean"":""120.00"",""homebuy"":""35"",""homesell"":""29"",""consumebuy"":""7"",""baseCreationQty"":0,""baseConsumptionQty"":487.6,""capacity"":1236174.51,""buyPrice"":0,""sellPrice"":108,""meanPrice"":120,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":884245,""targetStock"":221061,""stock"":0,""demand"":30617.088,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.24"",""sec_illegal_max"":""2.93"",""stolenmod"":""0.7500""},{""id"":""128049156"",""name"":""Bertrandite"",""cost_min"":2304.6704347826,""cost_max"":3015.3295652174,""cost_mean"":""2374.00"",""homebuy"":""85"",""homesell"":""84"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":116.2,""capacity"":300704.575,""buyPrice"":0,""sellPrice"":2507,""meanPrice"":2374,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":210725,""targetStock"":52681,""stock"":0,""demand"":107101.315,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.4000"",""sec_illegal_min"":""1.07"",""sec_illegal_max"":""1.57"",""stolenmod"":""0.7500""},{""id"":""128049159"",""name"":""Coltan"",""cost_min"":1264.7143478261,""cost_max"":1793.2856521739,""cost_mean"":""1319.00"",""homebuy"":""80"",""homesell"":""78"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":27.6,""capacity"":74127.012,""buyPrice"":0,""sellPrice"":1265,""meanPrice"":1319,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":50052,""targetStock"":12513,""stock"":0,""demand"":1861.30875,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.4100"",""sec_illegal_min"":""1.10"",""sec_illegal_max"":""1.79"",""stolenmod"":""0.7500""},{""id"":""128672294"",""name"":""Cryolite"",""cost_min"":2013.607173913,""cost_max"":2681.392826087,""cost_mean"":""2266.00"",""homebuy"":""84"",""homesell"":""82"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":128.6,""capacity"":344920.548,""buyPrice"":0,""sellPrice"":2621,""meanPrice"":2266,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":233212,""targetStock"":58303,""stock"":0,""demand"":241491.026,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.4400"",""sec_illegal_min"":""1.07"",""sec_illegal_max"":""1.61"",""stolenmod"":""0.7500""},{""id"":""128049158"",""name"":""Gallite"",""cost_min"":1762.6536956522,""cost_max"":2384.3463043478,""cost_mean"":""1819.00"",""homebuy"":""83"",""homesell"":""81"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":142.6,""capacity"":381693.6,""buyPrice"":0,""sellPrice"":2026,""meanPrice"":1819,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":258600,""targetStock"":64650,""stock"":0,""demand"":165135.708,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.4600"",""sec_illegal_min"":""1.08"",""sec_illegal_max"":""1.66"",""stolenmod"":""0.7500""},{""id"":""128672295"",""name"":""Goslarite"",""cost_min"":753.56260869565,""cost_max"":1162.4373913043,""cost_mean"":""916.00"",""homebuy"":""75"",""homesell"":""73"",""consumebuy"":""3"",""baseCreationQty"":0,""baseConsumptionQty"":41.6,""capacity"":112256.208,""buyPrice"":0,""sellPrice"":1126,""meanPrice"":916,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":75441,""targetStock"":18860,""stock"":0,""demand"":78572.248,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.3300"",""sec_illegal_min"":""1.12"",""sec_illegal_max"":""2.01"",""stolenmod"":""0.7500""},{""id"":""128049157"",""name"":""Indite"",""cost_min"":2013.607173913,""cost_max"":2681.392826087,""cost_mean"":""2088.00"",""homebuy"":""84"",""homesell"":""82"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":19.4,""capacity"":51823.086,""buyPrice"":0,""sellPrice"":2014,""meanPrice"":2088,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":35182,""targetStock"":8795,""stock"":0,""demand"":1284.364,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.4400"",""sec_illegal_min"":""1.07"",""sec_illegal_max"":""1.61"",""stolenmod"":""0.7500""},{""id"":""128049161"",""name"":""Lepidolite"",""cost_min"":518.34413043478,""cost_max"":860.65586956522,""cost_mean"":""544.00"",""homebuy"":""70"",""homesell"":""67"",""consumebuy"":""3"",""baseCreationQty"":0,""baseConsumptionQty"":56.6,""capacity"":150782.567,""buyPrice"":0,""sellPrice"":519,""meanPrice"":544,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":102643,""targetStock"":25660,""stock"":0,""demand"":3836.6125,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.2700"",""sec_illegal_min"":""1.14"",""sec_illegal_max"":""2.19"",""stolenmod"":""0.7500""},{""id"":""128673853"",""name"":""Lithium Hydroxide"",""cost_min"":4546.1739130435,""cost_max"":5513.8260869565,""cost_mean"":""5646.00"",""homebuy"":""90"",""homesell"":""89"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":34.8,""capacity"":94663.5,""buyPrice"":0,""sellPrice"":5426,""meanPrice"":5646,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":63109,""targetStock"":15777,""stock"":0,""demand"":66264.9,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.2000"",""sec_illegal_min"":""1.04"",""sec_illegal_max"":""1.35"",""stolenmod"":""0.7500""},{""id"":""128673848"",""name"":""Low Temperature Diamond"",""cost_min"":64800,""cost_max"":69221.762608696,""cost_mean"":""57445.00"",""homebuy"":""97"",""homesell"":""97"",""consumebuy"":""0"",""baseCreationQty"":0,""baseConsumptionQty"":5,""capacity"":13602,""buyPrice"":0,""sellPrice"":68821,""meanPrice"":57445,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":9068,""targetStock"":2266,""stock"":0,""demand"":9523.2,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.00"",""sec_illegal_max"":""1.03"",""stolenmod"":""0.7500""},{""id"":""128673854"",""name"":""Methane Clathrate"",""cost_min"":307.555,""cost_max"":579.445,""cost_mean"":""629.00"",""homebuy"":""63"",""homesell"":""59"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":90,""capacity"":244818,""buyPrice"":0,""sellPrice"":555,""meanPrice"":629,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":163212,""targetStock"":40803,""stock"":0,""demand"":171372.6,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.1800"",""sec_illegal_min"":""1.19"",""sec_illegal_max"":""2.52"",""stolenmod"":""0.7500""},{""id"":""128668550"",""name"":""Painite"",""cost_min"":63000,""cost_max"":73764.782608696,""cost_mean"":""40508.00"",""homebuy"":""100"",""homesell"":""100"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":5,""capacity"":2268,""buyPrice"":0,""sellPrice"":73765,""meanPrice"":40508,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":2268,""targetStock"":566,""stock"":0,""demand"":1985,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.04"",""stolenmod"":""0.7500""},{""id"":""128672297"",""name"":""Pyrophyllite"",""cost_min"":1351.3913043478,""cost_max"":1896.6086956522,""cost_mean"":""1565.00"",""homebuy"":""81"",""homesell"":""79"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":35.8,""capacity"":96733.78,""buyPrice"":0,""sellPrice"":1848,""meanPrice"":1565,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":64922,""targetStock"":16230,""stock"":0,""demand"":67714.54,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.4200"",""sec_illegal_min"":""1.09"",""sec_illegal_max"":""1.76"",""stolenmod"":""0.7500""},{""id"":""128049163"",""name"":""Rutile"",""cost_min"":275.77565217391,""cost_max"":536.22434782609,""cost_mean"":""299.00"",""homebuy"":""61"",""homesell"":""57"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":166,""capacity"":430781.085,""buyPrice"":0,""sellPrice"":276,""meanPrice"":299,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":301035,""targetStock"":75258,""stock"":0,""demand"":10818.76725,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.1600"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.60"",""stolenmod"":""0.7500""},{""id"":""128049160"",""name"":""Uraninite"",""cost_min"":803.23913043478,""cost_max"":1224.7608695652,""cost_mean"":""836.00"",""homebuy"":""76"",""homesell"":""74"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":263.8,""capacity"":704671.416,""buyPrice"":0,""sellPrice"":1096,""meanPrice"":836,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":478392,""targetStock"":119598,""stock"":0,""demand"":409354.473,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.3400"",""sec_illegal_min"":""1.12"",""sec_illegal_max"":""1.98"",""stolenmod"":""0.7500""},{""id"":""128049214"",""name"":""Beer"",""cost_min"":""175.00"",""cost_max"":343.85565217391,""cost_mean"":""186.00"",""homebuy"":""42"",""homesell"":""36"",""consumebuy"":""6"",""baseCreationQty"":0,""baseConsumptionQty"":755,""capacity"":341123.835,""buyPrice"":0,""sellPrice"":175,""meanPrice"":186,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":232215,""targetStock"":58053,""stock"":0,""demand"":8679.366,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Narcotics"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.25"",""sec_illegal_max"":""3.04"",""stolenmod"":""0.7500""},{""id"":""128049216"",""name"":""Liquor"",""cost_min"":551.91,""cost_max"":""831.00"",""cost_mean"":""587.00"",""homebuy"":""71"",""homesell"":""68"",""consumebuy"":""3"",""baseCreationQty"":14.4,""baseConsumptionQty"":35.8,""capacity"":4668.611,""buyPrice"":0,""sellPrice"":552,""meanPrice"":587,""demandBracket"":1,""stockBracket"":0,""creationQty"":144,""consumptionQty"":2983,""targetStock"":889,""stock"":0,""demand"":70.2185,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Narcotics"",""volumescale"":""1.2800"",""sec_illegal_min"":""1.14"",""sec_illegal_max"":""2.16"",""stolenmod"":""0.7500""},{""id"":""128049215"",""name"":""Wine"",""cost_min"":""252.00"",""cost_max"":436.51652173913,""cost_mean"":""260.00"",""homebuy"":""54"",""homesell"":""49"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":452,""capacity"":241866.553,""buyPrice"":0,""sellPrice"":252,""meanPrice"":260,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":163313,""targetStock"":40827,""stock"":0,""demand"":6073.7575,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Narcotics"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.23"",""sec_illegal_max"":""2.88"",""stolenmod"":""0.7500""},{""id"":""128066403"",""name"":""Drones"",""cost_min"":""100.00"",""cost_max"":""100.00"",""cost_mean"":""101.00"",""homebuy"":""100"",""homesell"":""100"",""consumebuy"":""1"",""baseCreationQty"":200,""baseConsumptionQty"":0,""capacity"":91279,""buyPrice"":101,""sellPrice"":100,""meanPrice"":101,""demandBracket"":0,""stockBracket"":3,""creationQty"":91279,""consumptionQty"":0,""targetStock"":91279,""stock"":109534.8,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""NonMarketable"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.00"",""sec_illegal_max"":""0.99"",""stolenmod"":""0.7500""},{""id"":""128049231"",""name"":""Advanced Catalysers"",""cost_min"":2637.6134782609,""cost_max"":3396.3865217391,""cost_mean"":""2947.00"",""homebuy"":""86"",""homesell"":""85"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":104.8,""capacity"":282996.362,""buyPrice"":0,""sellPrice"":3328,""meanPrice"":2947,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":190058,""targetStock"":47514,""stock"":0,""demand"":198088.844,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.3600"",""sec_illegal_min"":""1.05"",""sec_illegal_max"":""1.39"",""stolenmod"":""0.7500""},{""id"":""128049228"",""name"":""Auto Fabricators"",""cost_min"":3843.168,""cost_max"":4597.7635078261,""cost_mean"":""3734.00"",""homebuy"":""88"",""homesell"":""87"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":34.4,""capacity"":124772,""buyPrice"":0,""sellPrice"":4530,""meanPrice"":3734,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":62386,""targetStock"":15596,""stock"":0,""demand"":87341.6,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.2800"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.28"",""stolenmod"":""0.7500""},{""id"":""128049225"",""name"":""Computer Components"",""cost_min"":460.23369565217,""cost_max"":""716.00"",""cost_mean"":""513.00"",""homebuy"":""69"",""homesell"":""66"",""consumebuy"":""3"",""baseCreationQty"":167.2,""baseConsumptionQty"":0,""capacity"":112723.522,""buyPrice"":321,""sellPrice"":304,""meanPrice"":513,""demandBracket"":0,""stockBracket"":3,""creationQty"":75806,""consumptionQty"":0,""targetStock"":75806,""stock"":135237.904,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.2500"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128049226"",""name"":""Hazardous Environment Suits"",""cost_min"":""274.00"",""cost_max"":465.3002173913,""cost_mean"":""340.00"",""homebuy"":""56"",""homesell"":""52"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":612,""capacity"":1661487.366,""buyPrice"":0,""sellPrice"":448,""meanPrice"":340,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":1109878,""targetStock"":277469,""stock"":0,""demand"":1163153.042,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.1200"",""sec_illegal_min"":""1.22"",""sec_illegal_max"":""2.78"",""stolenmod"":""0.7500""},{""id"":""128673873"",""name"":""Micro Controllers"",""cost_min"":""3167.00"",""cost_max"":3827.9913043478,""cost_mean"":""3274.00"",""homebuy"":""87"",""homesell"":""86"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":19.2,""capacity"":52195.18,""buyPrice"":0,""sellPrice"":3769,""meanPrice"":3274,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":34820,""targetStock"":8705,""stock"":0,""demand"":36543.59,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.3200"",""sec_illegal_min"":""1.05"",""sec_illegal_max"":""1.37"",""stolenmod"":""0.7500""},{""id"":""128049671"",""name"":""Resonating Separators"",""cost_min"":5818.9000652174,""cost_max"":6849.0919347826,""cost_mean"":""5958.00"",""homebuy"":""91"",""homesell"":""90"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":59.6,""capacity"":216174,""buyPrice"":0,""sellPrice"":6756,""meanPrice"":5958,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":108087,""targetStock"":27021,""stock"":0,""demand"":151323.6,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.1400"",""sec_illegal_min"":""1.02"",""sec_illegal_max"":""1.18"",""stolenmod"":""0.7500""},{""id"":""128049227"",""name"":""Robotics"",""cost_min"":1999.112,""cost_max"":2549.35456,""cost_mean"":""1856.00"",""homebuy"":""82"",""homesell"":""80"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":60,""capacity"":312834,""buyPrice"":0,""sellPrice"":2500,""meanPrice"":1856,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":156417,""targetStock"":39104,""stock"":0,""demand"":218984.4,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.4500"",""sec_illegal_min"":""1.08"",""sec_illegal_max"":""1.69"",""stolenmod"":""0.7500""},{""id"":""128682044"",""name"":""Conductive Fabrics"",""cost_min"":944,""cost_max"":1431.1739130435,""cost_mean"":""507.00"",""homebuy"":""67"",""homesell"":""64"",""consumebuy"":""3"",""baseCreationQty"":46.4,""baseConsumptionQty"":278.4,""capacity"":846311.06,""buyPrice"":0,""sellPrice"":1276,""meanPrice"":507,""demandBracket"":2,""stockBracket"":0,""creationQty"":63108,""consumptionQty"":504886,""targetStock"":189329,""stock"":0,""demand"":485719.736,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Textiles"",""volumescale"":""1.2300"",""sec_illegal_min"":""1.16"",""sec_illegal_max"":""2.33"",""stolenmod"":""0.7500""},{""id"":""128049190"",""name"":""Leather"",""cost_min"":""175.00"",""cost_max"":343.85565217391,""cost_mean"":""205.00"",""homebuy"":""42"",""homesell"":""36"",""consumebuy"":""6"",""baseCreationQty"":0,""baseConsumptionQty"":1207.2,""capacity"":3248900.424,""buyPrice"":0,""sellPrice"":310,""meanPrice"":205,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":2189286,""targetStock"":547321,""stock"":0,""demand"":2075513.508,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Textiles"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.23"",""sec_illegal_max"":""2.88"",""stolenmod"":""0.7500""},{""id"":""128682045"",""name"":""Military Grade Fabrics"",""cost_min"":2120.9282608696,""cost_max"":3312.0717391304,""cost_mean"":""708.00"",""homebuy"":""74"",""homesell"":""71"",""consumebuy"":""3"",""baseCreationQty"":29.2,""baseConsumptionQty"":0,""capacity"":59215.065,""buyPrice"":1586,""sellPrice"":1506,""meanPrice"":708,""demandBracket"":0,""stockBracket"":3,""creationQty"":39715,""consumptionQty"":0,""targetStock"":39715,""stock"":71050.135,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Textiles"",""volumescale"":""1.3200"",""sec_illegal_min"":""1.13"",""sec_illegal_max"":""2.04"",""stolenmod"":""0.7500""},{""id"":""128049191"",""name"":""Natural Fabrics"",""cost_min"":""403.00"",""cost_max"":627.93,""cost_mean"":""439.00"",""homebuy"":""64"",""homesell"":""60"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":216.8,""capacity"":585433.108,""buyPrice"":0,""sellPrice"":403,""meanPrice"":439,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":393172,""targetStock"":98293,""stock"":0,""demand"":81891.38544514,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Textiles"",""volumescale"":""1.2000"",""sec_illegal_min"":""1.18"",""sec_illegal_max"":""2.44"",""stolenmod"":""0.7500""},{""id"":""128049193"",""name"":""Synthetic Fabrics"",""cost_min"":""186.00"",""cost_max"":357.57913043478,""cost_mean"":""211.00"",""homebuy"":""45"",""homesell"":""40"",""consumebuy"":""6"",""baseCreationQty"":136.2,""baseConsumptionQty"":817.6,""capacity"":2455269.504,""buyPrice"":0,""sellPrice"":303,""meanPrice"":211,""demandBracket"":2,""stockBracket"":0,""creationQty"":185244,""consumptionQty"":1482738,""targetStock"":555928,""stock"":0,""demand"":1406306.122,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Textiles"",""volumescale"":""1.0200"",""sec_illegal_min"":""1.29"",""sec_illegal_max"":""3.34"",""stolenmod"":""0.7500""},{""id"":""128049244"",""name"":""Biowaste"",""cost_min"":32.834782608696,""cost_max"":""97.00"",""cost_mean"":""63.00"",""homebuy"":""27"",""homesell"":""20"",""consumebuy"":""7"",""baseCreationQty"":162,""baseConsumptionQty"":0,""capacity"":29700.249,""buyPrice"":14,""sellPrice"":10,""meanPrice"":63,""demandBracket"":0,""stockBracket"":2,""creationQty"":19893,""consumptionQty"":0,""targetStock"":19893,""stock"":19962.88,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Waste "",""volumescale"":""1.0000"",""sec_illegal_min"":""1.44"",""sec_illegal_max"":""4.47"",""stolenmod"":""0.7500""},{""id"":""128049246"",""name"":""Chemical Waste"",""cost_min"":36.121739130435,""cost_max"":120.87826086957,""cost_mean"":""131.00"",""homebuy"":""18"",""homesell"":""10"",""consumebuy"":""8"",""baseCreationQty"":0,""baseConsumptionQty"":72.2,""capacity"":49428,""buyPrice"":0,""sellPrice"":114,""meanPrice"":131,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":32952,""targetStock"":8238,""stock"":0,""demand"":34599.6,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Waste "",""volumescale"":""1.0000"",""sec_illegal_min"":""1.48"",""sec_illegal_max"":""4.78"",""stolenmod"":""0.7500""},{""id"":""128049248"",""name"":""Scrap"",""cost_min"":51.826086956522,""cost_max"":""120.00"",""cost_mean"":""48.00"",""homebuy"":""42"",""homesell"":""36"",""consumebuy"":""6"",""baseCreationQty"":362.4,""baseConsumptionQty"":30.2,""capacity"":330749.118,""buyPrice"":22,""sellPrice"":19,""meanPrice"":48,""demandBracket"":0,""stockBracket"":3,""creationQty"":177989,""consumptionQty"":54769,""targetStock"":191681,""stock"":306165.25249861,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Waste "",""volumescale"":""1.0000"",""sec_illegal_min"":""1.31"",""sec_illegal_max"":""3.48"",""stolenmod"":""0.7500""},{""id"":""128049236"",""name"":""Non Lethal Weapons"",""cost_min"":""1766.00"",""cost_max"":2252.08,""cost_mean"":""1837.00"",""homebuy"":""82"",""homesell"":""80"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":75,""capacity"":6296.612,""buyPrice"":0,""sellPrice"":2209,""meanPrice"":1837,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":4243,""targetStock"":1060,""stock"":0,""demand"":4408.752,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Weapons"",""volumescale"":""1.4500"",""sec_illegal_min"":""1.08"",""sec_illegal_max"":""1.69"",""stolenmod"":""0.7500""},{""id"":""128049235"",""name"":""Reactive Armour"",""cost_min"":""2008.00"",""cost_max"":2528.3956521739,""cost_mean"":""2113.00"",""homebuy"":""84"",""homesell"":""82"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":68,""capacity"":12064.834,""buyPrice"":0,""sellPrice"":2482,""meanPrice"":2113,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":8119,""targetStock"":2029,""stock"":0,""demand"":8447.127,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Weapons"",""volumescale"":""1.4600"",""sec_illegal_min"":""1.08"",""sec_illegal_max"":""1.64"",""stolenmod"":""0.7500""}],""ships"":{""shipyard_list"":{""SideWinder"":{""id"":128049249,""name"":""SideWinder"",""basevalue"":32000,""sku"":""""},""Eagle"":{""id"":128049255,""name"":""Eagle"",""basevalue"":44800,""sku"":""""},""Hauler"":{""id"":128049261,""name"":""Hauler"",""basevalue"":52720,""sku"":""""},""Type7"":{""id"":128049297,""name"":""Type7"",""basevalue"":17472252,""sku"":""""},""Asp_Scout"":{""id"":128672276,""name"":""Asp_Scout"",""basevalue"":3961154,""sku"":""""},""Asp"":{""id"":128049303,""name"":""Asp"",""basevalue"":6661154,""sku"":""""},""Vulture"":{""id"":128049309,""name"":""Vulture"",""basevalue"":4925615,""sku"":""""},""Adder"":{""id"":128049267,""name"":""Adder"",""basevalue"":87808,""sku"":""""},""Federation_Dropship"":{""id"":128049321,""name"":""Federation_Dropship"",""basevalue"":14314205,""sku"":""""},""CobraMkIII"":{""id"":128049279,""name"":""CobraMkIII"",""basevalue"":349718,""sku"":""""},""Type6"":{""id"":128049285,""name"":""Type6"",""basevalue"":1045945,""sku"":""""}},""unavailable_list"":[]},""modules"":{""128049434"":{""id"":128049434,""category"":""weapon"",""name"":""Hpt_BeamLaser_Gimbal_Large"",""cost"":2396160,""sku"":null},""128049437"":{""id"":128049437,""category"":""weapon"",""name"":""Hpt_BeamLaser_Turret_Large"",""cost"":19399600,""sku"":null},""128049430"":{""id"":128049430,""category"":""weapon"",""name"":""Hpt_BeamLaser_Fixed_Large"",""cost"":1177600,""sku"":null},""128049433"":{""id"":128049433,""category"":""weapon"",""name"":""Hpt_BeamLaser_Gimbal_Medium"",""cost"":500600,""sku"":null},""128049436"":{""id"":128049436,""category"":""weapon"",""name"":""Hpt_BeamLaser_Turret_Medium"",""cost"":2099900,""sku"":null},""128049432"":{""id"":128049432,""category"":""weapon"",""name"":""Hpt_BeamLaser_Gimbal_Small"",""cost"":74650,""sku"":null},""128049435"":{""id"":128049435,""category"":""weapon"",""name"":""Hpt_BeamLaser_Turret_Small"",""cost"":500000,""sku"":null},""128049428"":{""id"":128049428,""category"":""weapon"",""name"":""Hpt_BeamLaser_Fixed_Small"",""cost"":37430,""sku"":null},""128681995"":{""id"":128681995,""category"":""weapon"",""name"":""Hpt_PulseLaser_Gimbal_Huge"",""cost"":877600,""sku"":null},""128049390"":{""id"":128049390,""category"":""weapon"",""name"":""Hpt_PulseLaser_Turret_Large"",""cost"":400400,""sku"":null},""128049387"":{""id"":128049387,""category"":""weapon"",""name"":""Hpt_PulseLaser_Gimbal_Large"",""cost"":140600,""sku"":null},""128049388"":{""id"":128049388,""category"":""weapon"",""name"":""Hpt_PulseLaser_Turret_Small"",""cost"":26000,""sku"":null},""128049386"":{""id"":128049386,""category"":""weapon"",""name"":""Hpt_PulseLaser_Gimbal_Medium"",""cost"":35400,""sku"":null},""128049385"":{""id"":128049385,""category"":""weapon"",""name"":""Hpt_PulseLaser_Gimbal_Small"",""cost"":6600,""sku"":null},""128049383"":{""id"":128049383,""category"":""weapon"",""name"":""Hpt_PulseLaser_Fixed_Large"",""cost"":70400,""sku"":null},""128049382"":{""id"":128049382,""category"":""weapon"",""name"":""Hpt_PulseLaser_Fixed_Medium"",""cost"":17600,""sku"":null},""128049409"":{""id"":128049409,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Turret_Large"",""cost"":800400,""sku"":null},""128049406"":{""id"":128049406,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Gimbal_Large"",""cost"":281600,""sku"":null},""128049404"":{""id"":128049404,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Gimbal_Small"",""cost"":8600,""sku"":null},""128049405"":{""id"":128049405,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Gimbal_Medium"",""cost"":48500,""sku"":null},""128049402"":{""id"":128049402,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Fixed_Large"",""cost"":140400,""sku"":null},""128049401"":{""id"":128049401,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Fixed_Medium"",""cost"":23000,""sku"":null},""128049400"":{""id"":128049400,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Fixed_Small"",""cost"":4400,""sku"":null},""128049467"":{""id"":128049467,""category"":""weapon"",""name"":""Hpt_PlasmaAccelerator_Fixed_Huge"",""cost"":13793600,""sku"":null},""128049466"":{""id"":128049466,""category"":""weapon"",""name"":""Hpt_PlasmaAccelerator_Fixed_Large"",""cost"":3051200,""sku"":null},""128049465"":{""id"":128049465,""category"":""weapon"",""name"":""Hpt_PlasmaAccelerator_Fixed_Medium"",""cost"":834200,""sku"":null},""128671321"":{""id"":128671321,""category"":""weapon"",""name"":""Hpt_Slugshot_Gimbal_Large"",""cost"":1751040,""sku"":null},""128671322"":{""id"":128671322,""category"":""weapon"",""name"":""Hpt_Slugshot_Turret_Large"",""cost"":5836800,""sku"":null},""128049454"":{""id"":128049454,""category"":""weapon"",""name"":""Hpt_Slugshot_Turret_Medium"",""cost"":1459200,""sku"":null},""128049450"":{""id"":128049450,""category"":""weapon"",""name"":""Hpt_Slugshot_Fixed_Large"",""cost"":1167360,""sku"":null},""128049453"":{""id"":128049453,""category"":""weapon"",""name"":""Hpt_Slugshot_Turret_Small"",""cost"":182400,""sku"":null},""128049451"":{""id"":128049451,""category"":""weapon"",""name"":""Hpt_Slugshot_Gimbal_Small"",""cost"":54720,""sku"":null},""128049461"":{""id"":128049461,""category"":""weapon"",""name"":""Hpt_MultiCannon_Gimbal_Large"",""cost"":578436,""sku"":null},""128049457"":{""id"":128049457,""category"":""weapon"",""name"":""Hpt_MultiCannon_Fixed_Large"",""cost"":140400,""sku"":null},""128049462"":{""id"":128049462,""category"":""weapon"",""name"":""Hpt_MultiCannon_Turret_Small"",""cost"":81600,""sku"":null},""128049459"":{""id"":128049459,""category"":""weapon"",""name"":""Hpt_MultiCannon_Gimbal_Small"",""cost"":14250,""sku"":null},""128049460"":{""id"":128049460,""category"":""weapon"",""name"":""Hpt_MultiCannon_Gimbal_Medium"",""cost"":57000,""sku"":null},""128049456"":{""id"":128049456,""category"":""weapon"",""name"":""Hpt_MultiCannon_Fixed_Medium"",""cost"":38000,""sku"":null},""128049510"":{""id"":128049510,""category"":""weapon"",""name"":""Hpt_AdvancedTorpPylon_Fixed_Medium"",""cost"":44800,""sku"":null},""128049509"":{""id"":128049509,""category"":""weapon"",""name"":""Hpt_AdvancedTorpPylon_Fixed_Small"",""cost"":11200,""sku"":null},""128666725"":{""id"":128666725,""category"":""weapon"",""name"":""Hpt_DumbfireMissileRack_Fixed_Medium"",""cost"":240400,""sku"":null},""128666724"":{""id"":128666724,""category"":""weapon"",""name"":""Hpt_DumbfireMissileRack_Fixed_Small"",""cost"":32175,""sku"":null},""128671448"":{""id"":128671448,""category"":""weapon"",""name"":""Hpt_MineLauncher_Fixed_Small_Impulse"",""cost"":36390,""sku"":null},""128049500"":{""id"":128049500,""category"":""weapon"",""name"":""Hpt_MineLauncher_Fixed_Small"",""cost"":24260,""sku"":null},""128049488"":{""id"":128049488,""category"":""weapon"",""name"":""Hpt_Railgun_Fixed_Small"",""cost"":51600,""sku"":null},""128049493"":{""id"":128049493,""category"":""weapon"",""name"":""Hpt_BasicMissileRack_Fixed_Medium"",""cost"":512400,""sku"":null},""128049492"":{""id"":128049492,""category"":""weapon"",""name"":""Hpt_BasicMissileRack_Fixed_Small"",""cost"":72600,""sku"":null},""128049441"":{""id"":128049441,""category"":""weapon"",""name"":""Hpt_Cannon_Fixed_Huge"",""cost"":2700800,""sku"":null},""128049444"":{""id"":128049444,""category"":""weapon"",""name"":""Hpt_Cannon_Gimbal_Huge"",""cost"":5401600,""sku"":null},""128671120"":{""id"":128671120,""category"":""weapon"",""name"":""Hpt_Cannon_Gimbal_Large"",""cost"":1350400,""sku"":null},""128049445"":{""id"":128049445,""category"":""weapon"",""name"":""Hpt_Cannon_Turret_Small"",""cost"":506400,""sku"":null},""128049458"":{""id"":128049458,""category"":""weapon"",""name"":""Hpt_MultiCannon_Fixed_Huge"",""cost"":1177600,""sku"":null},""128049455"":{""id"":128049455,""category"":""weapon"",""name"":""Hpt_MultiCannon_Fixed_Small"",""cost"":9500,""sku"":null},""128681994"":{""id"":128681994,""category"":""weapon"",""name"":""Hpt_BeamLaser_Gimbal_Huge"",""cost"":8746160,""sku"":null},""128049429"":{""id"":128049429,""category"":""weapon"",""name"":""Hpt_BeamLaser_Fixed_Medium"",""cost"":299520,""sku"":null},""128049489"":{""id"":128049489,""category"":""weapon"",""name"":""Hpt_Railgun_Fixed_Medium"",""cost"":412800,""sku"":null},""128049501"":{""id"":128049501,""category"":""weapon"",""name"":""Hpt_MineLauncher_Fixed_Medium"",""cost"":294080,""sku"":null},""128049389"":{""id"":128049389,""category"":""weapon"",""name"":""Hpt_PulseLaser_Turret_Medium"",""cost"":132800,""sku"":null},""128049381"":{""id"":128049381,""category"":""weapon"",""name"":""Hpt_PulseLaser_Fixed_Small"",""cost"":2200,""sku"":null},""128049452"":{""id"":128049452,""category"":""weapon"",""name"":""Hpt_Slugshot_Gimbal_Medium"",""cost"":437760,""sku"":null},""128049448"":{""id"":128049448,""category"":""weapon"",""name"":""Hpt_Slugshot_Fixed_Small"",""cost"":36000,""sku"":null},""128049408"":{""id"":128049408,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Turret_Medium"",""cost"":162800,""sku"":null},""128049407"":{""id"":128049407,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Turret_Small"",""cost"":52800,""sku"":null},""128049519"":{""id"":128049519,""category"":""utility"",""name"":""Hpt_HeatSinkLauncher_Turret_Tiny"",""cost"":3500,""sku"":null},""128049526"":{""id"":128049526,""category"":""utility"",""name"":""Hpt_MiningLaser_Fixed_Medium"",""cost"":22576,""sku"":null},""128049525"":{""id"":128049525,""category"":""utility"",""name"":""Hpt_MiningLaser_Fixed_Small"",""cost"":6800,""sku"":null},""128049513"":{""id"":128049513,""category"":""utility"",""name"":""Hpt_ChaffLauncher_Tiny"",""cost"":8500,""sku"":null},""128049549"":{""id"":128049549,""category"":""utility"",""name"":""Int_DockingComputer_Standard"",""cost"":4500,""sku"":null},""128662532"":{""id"":128662532,""category"":""utility"",""name"":""Hpt_CrimeScanner_Size0_Class3"",""cost"":121899,""sku"":null},""128662531"":{""id"":128662531,""category"":""utility"",""name"":""Hpt_CrimeScanner_Size0_Class2"",""cost"":40633,""sku"":null},""128662530"":{""id"":128662530,""category"":""utility"",""name"":""Hpt_CrimeScanner_Size0_Class1"",""cost"":13544,""sku"":null},""128662524"":{""id"":128662524,""category"":""utility"",""name"":""Hpt_CargoScanner_Size0_Class5"",""cost"":1097095,""sku"":null},""128662523"":{""id"":128662523,""category"":""utility"",""name"":""Hpt_CargoScanner_Size0_Class4"",""cost"":365698,""sku"":null},""128662520"":{""id"":128662520,""category"":""utility"",""name"":""Hpt_CargoScanner_Size0_Class1"",""cost"":13544,""sku"":null},""128662527"":{""id"":128662527,""category"":""utility"",""name"":""Hpt_CloudScanner_Size0_Class3"",""cost"":121899,""sku"":null},""128662526"":{""id"":128662526,""category"":""utility"",""name"":""Hpt_CloudScanner_Size0_Class2"",""cost"":40633,""sku"":null},""128662525"":{""id"":128662525,""category"":""utility"",""name"":""Hpt_CloudScanner_Size0_Class1"",""cost"":13544,""sku"":null},""128662529"":{""id"":128662529,""category"":""utility"",""name"":""Hpt_CloudScanner_Size0_Class5"",""cost"":1097095,""sku"":null},""128662528"":{""id"":128662528,""category"":""utility"",""name"":""Hpt_CloudScanner_Size0_Class4"",""cost"":365698,""sku"":null},""128049516"":{""id"":128049516,""category"":""utility"",""name"":""Hpt_ElectronicCountermeasure_Tiny"",""cost"":12500,""sku"":null},""128049522"":{""id"":128049522,""category"":""utility"",""name"":""Hpt_PlasmaPointDefence_Turret_Tiny"",""cost"":18546,""sku"":null},""128662533"":{""id"":128662533,""category"":""utility"",""name"":""Hpt_CrimeScanner_Size0_Class4"",""cost"":365698,""sku"":null},""128662534"":{""id"":128662534,""category"":""utility"",""name"":""Hpt_CrimeScanner_Size0_Class5"",""cost"":1097095,""sku"":null},""128049252"":{""id"":128049252,""category"":""module"",""name"":""SideWinder_Armour_Grade3"",""cost"":80320,""sku"":null},""128049251"":{""id"":128049251,""category"":""module"",""name"":""SideWinder_Armour_Grade2"",""cost"":25600,""sku"":null},""128049250"":{""id"":128049250,""category"":""module"",""name"":""SideWinder_Armour_Grade1"",""cost"":0,""sku"":null},""128049253"":{""id"":128049253,""category"":""module"",""name"":""SideWinder_Armour_Mirrored"",""cost"":132064,""sku"":null},""128049254"":{""id"":128049254,""category"":""module"",""name"":""SideWinder_Armour_Reactive"",""cost"":139424,""sku"":null},""128049258"":{""id"":128049258,""category"":""module"",""name"":""Eagle_Armour_Grade3"",""cost"":90048,""sku"":null},""128049257"":{""id"":128049257,""category"":""module"",""name"":""Eagle_Armour_Grade2"",""cost"":26880,""sku"":null},""128049256"":{""id"":128049256,""category"":""module"",""name"":""Eagle_Armour_Grade1"",""cost"":0,""sku"":null},""128049259"":{""id"":128049259,""category"":""module"",""name"":""Eagle_Armour_Mirrored"",""cost"":140089,""sku"":null},""128049260"":{""id"":128049260,""category"":""module"",""name"":""Eagle_Armour_Reactive"",""cost"":150393,""sku"":null},""128049264"":{""id"":128049264,""category"":""module"",""name"":""Hauler_Armour_Grade3"",""cost"":185047,""sku"":null},""128049263"":{""id"":128049263,""category"":""module"",""name"":""Hauler_Armour_Grade2"",""cost"":42176,""sku"":null},""128049262"":{""id"":128049262,""category"":""module"",""name"":""Hauler_Armour_Grade1"",""cost"":0,""sku"":null},""128049265"":{""id"":128049265,""category"":""module"",""name"":""Hauler_Armour_Mirrored"",""cost"":270295,""sku"":null},""128049266"":{""id"":128049266,""category"":""module"",""name"":""Hauler_Armour_Reactive"",""cost"":282421,""sku"":null},""128049300"":{""id"":128049300,""category"":""module"",""name"":""Type7_Armour_Grade3"",""cost"":15725026,""sku"":null},""128049299"":{""id"":128049299,""category"":""module"",""name"":""Type7_Armour_Grade2"",""cost"":6988900,""sku"":null},""128049298"":{""id"":128049298,""category"":""module"",""name"":""Type7_Armour_Grade1"",""cost"":0,""sku"":null},""128049301"":{""id"":128049301,""category"":""module"",""name"":""Type7_Armour_Mirrored"",""cost"":37163480,""sku"":null},""128049302"":{""id"":128049302,""category"":""module"",""name"":""Type7_Armour_Reactive"",""cost"":41182097,""sku"":null},""128672280"":{""id"":128672280,""category"":""module"",""name"":""Asp_Scout_Armour_Grade3"",""cost"":3565038,""sku"":null},""128672279"":{""id"":128672279,""category"":""module"",""name"":""Asp_Scout_Armour_Grade2"",""cost"":1584461,""sku"":null},""128672278"":{""id"":128672278,""category"":""module"",""name"":""Asp_Scout_Armour_Grade1"",""cost"":0,""sku"":null},""128672281"":{""id"":128672281,""category"":""module"",""name"":""Asp_Scout_Armour_Mirrored"",""cost"":8425374,""sku"":null},""128672282"":{""id"":128672282,""category"":""module"",""name"":""Asp_Scout_Armour_Reactive"",""cost"":9336439,""sku"":null},""128049306"":{""id"":128049306,""category"":""module"",""name"":""Asp_Armour_Grade3"",""cost"":5995038,""sku"":null},""128049305"":{""id"":128049305,""category"":""module"",""name"":""Asp_Armour_Grade2"",""cost"":2664461,""sku"":null},""128049304"":{""id"":128049304,""category"":""module"",""name"":""Asp_Armour_Grade1"",""cost"":0,""sku"":null},""128049307"":{""id"":128049307,""category"":""module"",""name"":""Asp_Armour_Mirrored"",""cost"":14168274,""sku"":null},""128049308"":{""id"":128049308,""category"":""module"",""name"":""Asp_Armour_Reactive"",""cost"":15700339,""sku"":null},""128049312"":{""id"":128049312,""category"":""module"",""name"":""Vulture_Armour_Grade3"",""cost"":4433053,""sku"":null},""128049311"":{""id"":128049311,""category"":""module"",""name"":""Vulture_Armour_Grade2"",""cost"":1970246,""sku"":null},""128049310"":{""id"":128049310,""category"":""module"",""name"":""Vulture_Armour_Grade1"",""cost"":0,""sku"":null},""128049313"":{""id"":128049313,""category"":""module"",""name"":""Vulture_Armour_Mirrored"",""cost"":10476783,""sku"":null},""128049314"":{""id"":128049314,""category"":""module"",""name"":""Vulture_Armour_Reactive"",""cost"":11609674,""sku"":null},""128049270"":{""id"":128049270,""category"":""module"",""name"":""Adder_Armour_Grade3"",""cost"":79027,""sku"":null},""128049269"":{""id"":128049269,""category"":""module"",""name"":""Adder_Armour_Grade2"",""cost"":35123,""sku"":null},""128049268"":{""id"":128049268,""category"":""module"",""name"":""Adder_Armour_Grade1"",""cost"":0,""sku"":null},""128049271"":{""id"":128049271,""category"":""module"",""name"":""Adder_Armour_Mirrored"",""cost"":186767,""sku"":null},""128049272"":{""id"":128049272,""category"":""module"",""name"":""Adder_Armour_Reactive"",""cost"":206963,""sku"":null},""128049324"":{""id"":128049324,""category"":""module"",""name"":""Federation_Dropship_Armour_Grade3"",""cost"":12882784,""sku"":null},""128049323"":{""id"":128049323,""category"":""module"",""name"":""Federation_Dropship_Armour_Grade2"",""cost"":5725682,""sku"":null},""128049322"":{""id"":128049322,""category"":""module"",""name"":""Federation_Dropship_Armour_Grade1"",""cost"":0,""sku"":null},""128049325"":{""id"":128049325,""category"":""module"",""name"":""Federation_Dropship_Armour_Mirrored"",""cost"":30446314,""sku"":null},""128049326"":{""id"":128049326,""category"":""module"",""name"":""Federation_Dropship_Armour_Reactive"",""cost"":33738581,""sku"":null},""128662535"":{""id"":128662535,""category"":""module"",""name"":""Int_StellarBodyDiscoveryScanner_Standard"",""cost"":1000,""sku"":null},""128064338"":{""id"":128064338,""category"":""module"",""name"":""Int_CargoRack_Size1_Class1"",""cost"":1000,""sku"":null},""128666684"":{""id"":128666684,""category"":""module"",""name"":""Int_Refinery_Size1_Class1"",""cost"":6000,""sku"":null},""128666644"":{""id"":128666644,""category"":""module"",""name"":""Int_FuelScoop_Size1_Class1"",""cost"":309,""sku"":null},""128672317"":{""id"":128672317,""category"":""module"",""name"":""Int_PlanetApproachSuite"",""cost"":500,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128666704"":{""id"":128666704,""category"":""module"",""name"":""Int_FSDInterdictor_Size1_Class1"",""cost"":12000,""sku"":null},""128066532"":{""id"":128066532,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size1_Class1"",""cost"":600,""sku"":null},""128064263"":{""id"":128064263,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class1"",""cost"":1978,""sku"":null},""128064112"":{""id"":128064112,""category"":""module"",""name"":""Int_Hyperdrive_Size3_Class5"",""cost"":507912,""sku"":null},""128064107"":{""id"":128064107,""category"":""module"",""name"":""Int_Hyperdrive_Size2_Class5"",""cost"":160224,""sku"":null},""128064111"":{""id"":128064111,""category"":""module"",""name"":""Int_Hyperdrive_Size3_Class4"",""cost"":169304,""sku"":null},""128064106"":{""id"":128064106,""category"":""module"",""name"":""Int_Hyperdrive_Size2_Class4"",""cost"":53408,""sku"":null},""128064110"":{""id"":128064110,""category"":""module"",""name"":""Int_Hyperdrive_Size3_Class3"",""cost"":56435,""sku"":null},""128064105"":{""id"":128064105,""category"":""module"",""name"":""Int_Hyperdrive_Size2_Class3"",""cost"":17803,""sku"":null},""128064109"":{""id"":128064109,""category"":""module"",""name"":""Int_Hyperdrive_Size3_Class2"",""cost"":18812,""sku"":null},""128064104"":{""id"":128064104,""category"":""module"",""name"":""Int_Hyperdrive_Size2_Class2"",""cost"":5934,""sku"":null},""128064108"":{""id"":128064108,""category"":""module"",""name"":""Int_Hyperdrive_Size3_Class1"",""cost"":6271,""sku"":null},""128064103"":{""id"":128064103,""category"":""module"",""name"":""Int_Hyperdrive_Size2_Class1"",""cost"":1978,""sku"":null},""128064272"":{""id"":128064272,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class5"",""cost"":507912,""sku"":null},""128064267"":{""id"":128064267,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class5"",""cost"":160224,""sku"":null},""128064271"":{""id"":128064271,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class4"",""cost"":169304,""sku"":null},""128064266"":{""id"":128064266,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class4"",""cost"":53408,""sku"":null},""128671333"":{""id"":128671333,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class3_Fast"",""cost"":84653,""sku"":null},""128064270"":{""id"":128064270,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class3"",""cost"":56435,""sku"":null},""128671332"":{""id"":128671332,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class3_Fast"",""cost"":26705,""sku"":null},""128671331"":{""id"":128671331,""category"":""module"",""name"":""Int_ShieldGenerator_Size1_Class3_Fast"",""cost"":7713,""sku"":null},""128064265"":{""id"":128064265,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class3"",""cost"":17803,""sku"":null},""128064269"":{""id"":128064269,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class2"",""cost"":18812,""sku"":null},""128064264"":{""id"":128064264,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class2"",""cost"":5934,""sku"":null},""128064268"":{""id"":128064268,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class1"",""cost"":6271,""sku"":null},""128672293"":{""id"":128672293,""category"":""module"",""name"":""Int_BuggyBay_Size6_Class2"",""cost"":691200,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128672291"":{""id"":128672291,""category"":""module"",""name"":""Int_BuggyBay_Size4_Class2"",""cost"":86400,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128672292"":{""id"":128672292,""category"":""module"",""name"":""Int_BuggyBay_Size6_Class1"",""cost"":576000,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128672289"":{""id"":128672289,""category"":""module"",""name"":""Int_BuggyBay_Size2_Class2"",""cost"":21600,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128672290"":{""id"":128672290,""category"":""module"",""name"":""Int_BuggyBay_Size4_Class1"",""cost"":72000,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128672288"":{""id"":128672288,""category"":""module"",""name"":""Int_BuggyBay_Size2_Class1"",""cost"":18000,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128064297"":{""id"":128064297,""category"":""module"",""name"":""Int_ShieldGenerator_Size8_Class5"",""cost"":162586486,""sku"":null},""128064292"":{""id"":128064292,""category"":""module"",""name"":""Int_ShieldGenerator_Size7_Class5"",""cost"":51289112,""sku"":null},""128064291"":{""id"":128064291,""category"":""module"",""name"":""Int_ShieldGenerator_Size7_Class4"",""cost"":17096371,""sku"":null},""128671338"":{""id"":128671338,""category"":""module"",""name"":""Int_ShieldGenerator_Size8_Class3_Fast"",""cost"":27097748,""sku"":null},""128064295"":{""id"":128064295,""category"":""module"",""name"":""Int_ShieldGenerator_Size8_Class3"",""cost"":18065165,""sku"":null},""128671337"":{""id"":128671337,""category"":""module"",""name"":""Int_ShieldGenerator_Size7_Class3_Fast"",""cost"":8548185,""sku"":null},""128064290"":{""id"":128064290,""category"":""module"",""name"":""Int_ShieldGenerator_Size7_Class3"",""cost"":5698790,""sku"":null},""128064294"":{""id"":128064294,""category"":""module"",""name"":""Int_ShieldGenerator_Size8_Class2"",""cost"":6021722,""sku"":null},""128064289"":{""id"":128064289,""category"":""module"",""name"":""Int_ShieldGenerator_Size7_Class2"",""cost"":1899597,""sku"":null},""128064293"":{""id"":128064293,""category"":""module"",""name"":""Int_ShieldGenerator_Size8_Class1"",""cost"":2007241,""sku"":null},""128064288"":{""id"":128064288,""category"":""module"",""name"":""Int_ShieldGenerator_Size7_Class1"",""cost"":633199,""sku"":null},""128666717"":{""id"":128666717,""category"":""module"",""name"":""Int_FSDInterdictor_Size2_Class4"",""cost"":907200,""sku"":null},""128666713"":{""id"":128666713,""category"":""module"",""name"":""Int_FSDInterdictor_Size2_Class3"",""cost"":302400,""sku"":null},""128666716"":{""id"":128666716,""category"":""module"",""name"":""Int_FSDInterdictor_Size1_Class4"",""cost"":324000,""sku"":null},""128666712"":{""id"":128666712,""category"":""module"",""name"":""Int_FSDInterdictor_Size1_Class3"",""cost"":108000,""sku"":null},""128666709"":{""id"":128666709,""category"":""module"",""name"":""Int_FSDInterdictor_Size2_Class2"",""cost"":100800,""sku"":null},""128666708"":{""id"":128666708,""category"":""module"",""name"":""Int_FSDInterdictor_Size1_Class2"",""cost"":36000,""sku"":null},""128666705"":{""id"":128666705,""category"":""module"",""name"":""Int_FSDInterdictor_Size2_Class1"",""cost"":33600,""sku"":null},""128064042"":{""id"":128064042,""category"":""module"",""name"":""Int_Powerplant_Size3_Class5"",""cost"":507912,""sku"":null},""128064037"":{""id"":128064037,""category"":""module"",""name"":""Int_Powerplant_Size2_Class5"",""cost"":160224,""sku"":null},""128064041"":{""id"":128064041,""category"":""module"",""name"":""Int_Powerplant_Size3_Class4"",""cost"":169304,""sku"":null},""128064036"":{""id"":128064036,""category"":""module"",""name"":""Int_Powerplant_Size2_Class4"",""cost"":53408,""sku"":null},""128064040"":{""id"":128064040,""category"":""module"",""name"":""Int_Powerplant_Size3_Class3"",""cost"":56435,""sku"":null},""128064035"":{""id"":128064035,""category"":""module"",""name"":""Int_Powerplant_Size2_Class3"",""cost"":17803,""sku"":null},""128064039"":{""id"":128064039,""category"":""module"",""name"":""Int_Powerplant_Size3_Class2"",""cost"":18812,""sku"":null},""128064034"":{""id"":128064034,""category"":""module"",""name"":""Int_Powerplant_Size2_Class2"",""cost"":5934,""sku"":null},""128064038"":{""id"":128064038,""category"":""module"",""name"":""Int_Powerplant_Size3_Class1"",""cost"":6271,""sku"":null},""128064033"":{""id"":128064033,""category"":""module"",""name"":""Int_Powerplant_Size2_Class1"",""cost"":1978,""sku"":null},""128064345"":{""id"":128064345,""category"":""module"",""name"":""Int_CargoRack_Size8_Class1"",""cost"":3829866,""sku"":null},""128064344"":{""id"":128064344,""category"":""module"",""name"":""Int_CargoRack_Size7_Class1"",""cost"":1178420,""sku"":null},""128064343"":{""id"":128064343,""category"":""module"",""name"":""Int_CargoRack_Size6_Class1"",""cost"":362591,""sku"":null},""128064342"":{""id"":128064342,""category"":""module"",""name"":""Int_CargoRack_Size5_Class1"",""cost"":111566,""sku"":null},""128064341"":{""id"":128064341,""category"":""module"",""name"":""Int_CargoRack_Size4_Class1"",""cost"":34328,""sku"":null},""128064340"":{""id"":128064340,""category"":""module"",""name"":""Int_CargoRack_Size3_Class1"",""cost"":10563,""sku"":null},""128064339"":{""id"":128064339,""category"":""module"",""name"":""Int_CargoRack_Size2_Class1"",""cost"":3250,""sku"":null},""128064353"":{""id"":128064353,""category"":""module"",""name"":""Int_FuelTank_Size8_Class3"",""cost"":5428429,""sku"":null},""128064352"":{""id"":128064352,""category"":""module"",""name"":""Int_FuelTank_Size7_Class3"",""cost"":1780914,""sku"":null},""128064351"":{""id"":128064351,""category"":""module"",""name"":""Int_FuelTank_Size6_Class3"",""cost"":341577,""sku"":null},""128064350"":{""id"":128064350,""category"":""module"",""name"":""Int_FuelTank_Size5_Class3"",""cost"":97754,""sku"":null},""128064349"":{""id"":128064349,""category"":""module"",""name"":""Int_FuelTank_Size4_Class3"",""cost"":24734,""sku"":null},""128064348"":{""id"":128064348,""category"":""module"",""name"":""Int_FuelTank_Size3_Class3"",""cost"":7063,""sku"":null},""128064347"":{""id"":128064347,""category"":""module"",""name"":""Int_FuelTank_Size2_Class3"",""cost"":3750,""sku"":null},""128064346"":{""id"":128064346,""category"":""module"",""name"":""Int_FuelTank_Size1_Class3"",""cost"":1000,""sku"":null},""128671252"":{""id"":128671252,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size1_Class4"",""cost"":4800,""sku"":null},""128671264"":{""id"":128671264,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size7_Class1"",""cost"":437400,""sku"":null},""128671260"":{""id"":128671260,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size5_Class2"",""cost"":97200,""sku"":null},""128671256"":{""id"":128671256,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size3_Class3"",""cost"":21600,""sku"":null},""128671251"":{""id"":128671251,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size1_Class3"",""cost"":2400,""sku"":null},""128671259"":{""id"":128671259,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size5_Class1"",""cost"":48600,""sku"":null},""128671255"":{""id"":128671255,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size3_Class2"",""cost"":10800,""sku"":null},""128671250"":{""id"":128671250,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size1_Class2"",""cost"":1200,""sku"":null},""128671254"":{""id"":128671254,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size3_Class1"",""cost"":5400,""sku"":null},""128671249"":{""id"":128671249,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size1_Class1"",""cost"":600,""sku"":null},""128666697"":{""id"":128666697,""category"":""module"",""name"":""Int_Refinery_Size2_Class4"",""cost"":340200,""sku"":null},""128666693"":{""id"":128666693,""category"":""module"",""name"":""Int_Refinery_Size2_Class3"",""cost"":113400,""sku"":null},""128666696"":{""id"":128666696,""category"":""module"",""name"":""Int_Refinery_Size1_Class4"",""cost"":162000,""sku"":null},""128666689"":{""id"":128666689,""category"":""module"",""name"":""Int_Refinery_Size2_Class2"",""cost"":37800,""sku"":null},""128666692"":{""id"":128666692,""category"":""module"",""name"":""Int_Refinery_Size1_Class3"",""cost"":54000,""sku"":null},""128666688"":{""id"":128666688,""category"":""module"",""name"":""Int_Refinery_Size1_Class2"",""cost"":18000,""sku"":null},""128666685"":{""id"":128666685,""category"":""module"",""name"":""Int_Refinery_Size2_Class1"",""cost"":12600,""sku"":null},""128064257"":{""id"":128064257,""category"":""module"",""name"":""Int_Sensors_Size8_Class5"",""cost"":27249391,""sku"":null},""128064256"":{""id"":128064256,""category"":""module"",""name"":""Int_Sensors_Size8_Class4"",""cost"":10899756,""sku"":null},""128064255"":{""id"":128064255,""category"":""module"",""name"":""Int_Sensors_Size8_Class3"",""cost"":4359903,""sku"":null},""128064254"":{""id"":128064254,""category"":""module"",""name"":""Int_Sensors_Size8_Class2"",""cost"":1743961,""sku"":null},""128064249"":{""id"":128064249,""category"":""module"",""name"":""Int_Sensors_Size7_Class2"",""cost"":622843,""sku"":null},""128064253"":{""id"":128064253,""category"":""module"",""name"":""Int_Sensors_Size8_Class1"",""cost"":697584,""sku"":null},""128064248"":{""id"":128064248,""category"":""module"",""name"":""Int_Sensors_Size7_Class1"",""cost"":249137,""sku"":null},""128064192"":{""id"":128064192,""category"":""module"",""name"":""Int_PowerDistributor_Size3_Class5"",""cost"":158331,""sku"":null},""128064187"":{""id"":128064187,""category"":""module"",""name"":""Int_PowerDistributor_Size2_Class5"",""cost"":56547,""sku"":null},""128064182"":{""id"":128064182,""category"":""module"",""name"":""Int_PowerDistributor_Size1_Class5"",""cost"":20195,""sku"":null},""128064191"":{""id"":128064191,""category"":""module"",""name"":""Int_PowerDistributor_Size3_Class4"",""cost"":63333,""sku"":null},""128064181"":{""id"":128064181,""category"":""module"",""name"":""Int_PowerDistributor_Size1_Class4"",""cost"":8078,""sku"":null},""128064190"":{""id"":128064190,""category"":""module"",""name"":""Int_PowerDistributor_Size3_Class3"",""cost"":25333,""sku"":null},""128064185"":{""id"":128064185,""category"":""module"",""name"":""Int_PowerDistributor_Size2_Class3"",""cost"":9048,""sku"":null},""128064180"":{""id"":128064180,""category"":""module"",""name"":""Int_PowerDistributor_Size1_Class3"",""cost"":3231,""sku"":null},""128064189"":{""id"":128064189,""category"":""module"",""name"":""Int_PowerDistributor_Size3_Class2"",""cost"":10133,""sku"":null},""128064179"":{""id"":128064179,""category"":""module"",""name"":""Int_PowerDistributor_Size1_Class2"",""cost"":1293,""sku"":null},""128064184"":{""id"":128064184,""category"":""module"",""name"":""Int_PowerDistributor_Size2_Class2"",""cost"":3619,""sku"":null},""128064188"":{""id"":128064188,""category"":""module"",""name"":""Int_PowerDistributor_Size3_Class1"",""cost"":4053,""sku"":null},""128064183"":{""id"":128064183,""category"":""module"",""name"":""Int_PowerDistributor_Size2_Class1"",""cost"":1448,""sku"":null},""128064178"":{""id"":128064178,""category"":""module"",""name"":""Int_PowerDistributor_Size1_Class1"",""cost"":517,""sku"":null},""128064217"":{""id"":128064217,""category"":""module"",""name"":""Int_PowerDistributor_Size8_Class5"",""cost"":27249391,""sku"":null},""128064212"":{""id"":128064212,""category"":""module"",""name"":""Int_PowerDistributor_Size7_Class5"",""cost"":9731925,""sku"":null},""128064210"":{""id"":128064210,""category"":""module"",""name"":""Int_PowerDistributor_Size7_Class3"",""cost"":1557108,""sku"":null},""128064214"":{""id"":128064214,""category"":""module"",""name"":""Int_PowerDistributor_Size8_Class2"",""cost"":1743961,""sku"":null},""128064213"":{""id"":128064213,""category"":""module"",""name"":""Int_PowerDistributor_Size8_Class1"",""cost"":697584,""sku"":null},""128064208"":{""id"":128064208,""category"":""module"",""name"":""Int_PowerDistributor_Size7_Class1"",""cost"":249137,""sku"":null},""128064077"":{""id"":128064077,""category"":""module"",""name"":""Int_Engine_Size3_Class5"",""cost"":507912,""sku"":null},""128064072"":{""id"":128064072,""category"":""module"",""name"":""Int_Engine_Size2_Class5"",""cost"":160224,""sku"":null},""128064076"":{""id"":128064076,""category"":""module"",""name"":""Int_Engine_Size3_Class4"",""cost"":169304,""sku"":null},""128064071"":{""id"":128064071,""category"":""module"",""name"":""Int_Engine_Size2_Class4"",""cost"":53408,""sku"":null},""128064075"":{""id"":128064075,""category"":""module"",""name"":""Int_Engine_Size3_Class3"",""cost"":56435,""sku"":null},""128064070"":{""id"":128064070,""category"":""module"",""name"":""Int_Engine_Size2_Class3"",""cost"":17803,""sku"":null},""128064074"":{""id"":128064074,""category"":""module"",""name"":""Int_Engine_Size3_Class2"",""cost"":18812,""sku"":null},""128064069"":{""id"":128064069,""category"":""module"",""name"":""Int_Engine_Size2_Class2"",""cost"":5934,""sku"":null},""128064073"":{""id"":128064073,""category"":""module"",""name"":""Int_Engine_Size3_Class1"",""cost"":6271,""sku"":null},""128064068"":{""id"":128064068,""category"":""module"",""name"":""Int_Engine_Size2_Class1"",""cost"":1978,""sku"":null},""128666670"":{""id"":128666670,""category"":""module"",""name"":""Int_FuelScoop_Size3_Class4"",""cost"":225738,""sku"":null},""128666662"":{""id"":128666662,""category"":""module"",""name"":""Int_FuelScoop_Size3_Class3"",""cost"":56435,""sku"":null},""128666676"":{""id"":128666676,""category"":""module"",""name"":""Int_FuelScoop_Size1_Class5"",""cost"":82270,""sku"":null},""128666668"":{""id"":128666668,""category"":""module"",""name"":""Int_FuelScoop_Size1_Class4"",""cost"":20568,""sku"":null},""128666661"":{""id"":128666661,""category"":""module"",""name"":""Int_FuelScoop_Size2_Class3"",""cost"":17803,""sku"":null},""128666660"":{""id"":128666660,""category"":""module"",""name"":""Int_FuelScoop_Size1_Class3"",""cost"":5142,""sku"":null},""128666654"":{""id"":128666654,""category"":""module"",""name"":""Int_FuelScoop_Size3_Class2"",""cost"":14109,""sku"":null},""128666653"":{""id"":128666653,""category"":""module"",""name"":""Int_FuelScoop_Size2_Class2"",""cost"":4451,""sku"":null},""128666646"":{""id"":128666646,""category"":""module"",""name"":""Int_FuelScoop_Size3_Class1"",""cost"":3386,""sku"":null},""128666652"":{""id"":128666652,""category"":""module"",""name"":""Int_FuelScoop_Size1_Class2"",""cost"":1285,""sku"":null},""128666645"":{""id"":128666645,""category"":""module"",""name"":""Int_FuelScoop_Size2_Class1"",""cost"":1068,""sku"":null},""128666699"":{""id"":128666699,""category"":""module"",""name"":""Int_Refinery_Size4_Class4"",""cost"":1500282,""sku"":null},""128666695"":{""id"":128666695,""category"":""module"",""name"":""Int_Refinery_Size4_Class3"",""cost"":500094,""sku"":null},""128666694"":{""id"":128666694,""category"":""module"",""name"":""Int_Refinery_Size3_Class3"",""cost"":238140,""sku"":null},""128666691"":{""id"":128666691,""category"":""module"",""name"":""Int_Refinery_Size4_Class2"",""cost"":166698,""sku"":null},""128666687"":{""id"":128666687,""category"":""module"",""name"":""Int_Refinery_Size4_Class1"",""cost"":55566,""sku"":null},""128666690"":{""id"":128666690,""category"":""module"",""name"":""Int_Refinery_Size3_Class2"",""cost"":79380,""sku"":null},""128666686"":{""id"":128666686,""category"":""module"",""name"":""Int_Refinery_Size3_Class1"",""cost"":26460,""sku"":null},""128671272"":{""id"":128671272,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size1_Class4"",""cost"":4800,""sku"":null},""128671284"":{""id"":128671284,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size7_Class1"",""cost"":437400,""sku"":null},""128671280"":{""id"":128671280,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size5_Class2"",""cost"":97200,""sku"":null},""128671276"":{""id"":128671276,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size3_Class3"",""cost"":21600,""sku"":null},""128671271"":{""id"":128671271,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size1_Class3"",""cost"":2400,""sku"":null},""128671279"":{""id"":128671279,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size5_Class1"",""cost"":48600,""sku"":null},""128671275"":{""id"":128671275,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size3_Class2"",""cost"":10800,""sku"":null},""128671270"":{""id"":128671270,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size1_Class2"",""cost"":1200,""sku"":null},""128671274"":{""id"":128671274,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size3_Class1"",""cost"":5400,""sku"":null},""128671269"":{""id"":128671269,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size1_Class1"",""cost"":600,""sku"":null},""128667629"":{""id"":128667629,""category"":""module"",""name"":""Int_Repairer_Size8_Class4"",""cost"":16529941,""sku"":null},""128667621"":{""id"":128667621,""category"":""module"",""name"":""Int_Repairer_Size8_Class3"",""cost"":5509980,""sku"":null},""128667620"":{""id"":128667620,""category"":""module"",""name"":""Int_Repairer_Size7_Class3"",""cost"":3061100,""sku"":null},""128667627"":{""id"":128667627,""category"":""module"",""name"":""Int_Repairer_Size6_Class4"",""cost"":5101834,""sku"":null},""128667626"":{""id"":128667626,""category"":""module"",""name"":""Int_Repairer_Size5_Class4"",""cost"":2834352,""sku"":null},""128667613"":{""id"":128667613,""category"":""module"",""name"":""Int_Repairer_Size8_Class2"",""cost"":1836660,""sku"":null},""128667619"":{""id"":128667619,""category"":""module"",""name"":""Int_Repairer_Size6_Class3"",""cost"":1700611,""sku"":null},""128667612"":{""id"":128667612,""category"":""module"",""name"":""Int_Repairer_Size7_Class2"",""cost"":1020367,""sku"":null},""128667618"":{""id"":128667618,""category"":""module"",""name"":""Int_Repairer_Size5_Class3"",""cost"":944784,""sku"":null},""128667632"":{""id"":128667632,""category"":""module"",""name"":""Int_Repairer_Size3_Class5"",""cost"":2624400,""sku"":null},""128667617"":{""id"":128667617,""category"":""module"",""name"":""Int_Repairer_Size4_Class3"",""cost"":524880,""sku"":null},""128667624"":{""id"":128667624,""category"":""module"",""name"":""Int_Repairer_Size3_Class4"",""cost"":874800,""sku"":null},""128667631"":{""id"":128667631,""category"":""module"",""name"":""Int_Repairer_Size2_Class5"",""cost"":1458000,""sku"":null},""128667630"":{""id"":128667630,""category"":""module"",""name"":""Int_Repairer_Size1_Class5"",""cost"":810000,""sku"":null},""128663561"":{""id"":128663561,""category"":""module"",""name"":""Int_StellarBodyDiscoveryScanner_Advanced"",""cost"":1545000,""sku"":null},""128663560"":{""id"":128663560,""category"":""module"",""name"":""Int_StellarBodyDiscoveryScanner_Intermediate"",""cost"":505000,""sku"":null},""128666634"":{""id"":128666634,""category"":""module"",""name"":""Int_DetailedSurfaceScanner_Tiny"",""cost"":250000,""sku"":null},""128064152"":{""id"":128064152,""category"":""module"",""name"":""Int_LifeSupport_Size3_Class5"",""cost"":158331,""sku"":null},""128064151"":{""id"":128064151,""category"":""module"",""name"":""Int_LifeSupport_Size3_Class4"",""cost"":63333,""sku"":null},""128064141"":{""id"":128064141,""category"":""module"",""name"":""Int_LifeSupport_Size1_Class4"",""cost"":8078,""sku"":null},""128064146"":{""id"":128064146,""category"":""module"",""name"":""Int_LifeSupport_Size2_Class4"",""cost"":22619,""sku"":null},""128064150"":{""id"":128064150,""category"":""module"",""name"":""Int_LifeSupport_Size3_Class3"",""cost"":25333,""sku"":null},""128064145"":{""id"":128064145,""category"":""module"",""name"":""Int_LifeSupport_Size2_Class3"",""cost"":9048,""sku"":null},""128064140"":{""id"":128064140,""category"":""module"",""name"":""Int_LifeSupport_Size1_Class3"",""cost"":3231,""sku"":null},""128064149"":{""id"":128064149,""category"":""module"",""name"":""Int_LifeSupport_Size3_Class2"",""cost"":10133,""sku"":null},""128064139"":{""id"":128064139,""category"":""module"",""name"":""Int_LifeSupport_Size1_Class2"",""cost"":1293,""sku"":null},""128064144"":{""id"":128064144,""category"":""module"",""name"":""Int_LifeSupport_Size2_Class2"",""cost"":3619,""sku"":null},""128064148"":{""id"":128064148,""category"":""module"",""name"":""Int_LifeSupport_Size3_Class1"",""cost"":4053,""sku"":null},""128064143"":{""id"":128064143,""category"":""module"",""name"":""Int_LifeSupport_Size2_Class1"",""cost"":1448,""sku"":null},""128064138"":{""id"":128064138,""category"":""module"",""name"":""Int_LifeSupport_Size1_Class1"",""cost"":517,""sku"":null},""128671268"":{""id"":128671268,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size7_Class5"",""cost"":6998400,""sku"":null},""128671258"":{""id"":128671258,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size3_Class5"",""cost"":86400,""sku"":null},""128671253"":{""id"":128671253,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size1_Class5"",""cost"":9600,""sku"":null},""128671267"":{""id"":128671267,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size7_Class4"",""cost"":3499200,""sku"":null},""128671262"":{""id"":128671262,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size5_Class4"",""cost"":388800,""sku"":null},""128671266"":{""id"":128671266,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size7_Class3"",""cost"":1749600,""sku"":null},""128671257"":{""id"":128671257,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size3_Class4"",""cost"":43200,""sku"":null},""128671265"":{""id"":128671265,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size7_Class2"",""cost"":874800,""sku"":null},""128671261"":{""id"":128671261,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size5_Class3"",""cost"":194400,""sku"":null},""128064232"":{""id"":128064232,""category"":""module"",""name"":""Int_Sensors_Size3_Class5"",""cost"":158331,""sku"":null},""128064227"":{""id"":128064227,""category"":""module"",""name"":""Int_Sensors_Size2_Class5"",""cost"":56547,""sku"":null},""128064222"":{""id"":128064222,""category"":""module"",""name"":""Int_Sensors_Size1_Class5"",""cost"":20195,""sku"":null},""128064231"":{""id"":128064231,""category"":""module"",""name"":""Int_Sensors_Size3_Class4"",""cost"":63333,""sku"":null},""128064221"":{""id"":128064221,""category"":""module"",""name"":""Int_Sensors_Size1_Class4"",""cost"":8078,""sku"":null},""128064226"":{""id"":128064226,""category"":""module"",""name"":""Int_Sensors_Size2_Class4"",""cost"":22619,""sku"":null},""128064230"":{""id"":128064230,""category"":""module"",""name"":""Int_Sensors_Size3_Class3"",""cost"":25333,""sku"":null},""128064225"":{""id"":128064225,""category"":""module"",""name"":""Int_Sensors_Size2_Class3"",""cost"":9048,""sku"":null},""128064220"":{""id"":128064220,""category"":""module"",""name"":""Int_Sensors_Size1_Class3"",""cost"":3231,""sku"":null},""128064229"":{""id"":128064229,""category"":""module"",""name"":""Int_Sensors_Size3_Class2"",""cost"":10133,""sku"":null},""128064219"":{""id"":128064219,""category"":""module"",""name"":""Int_Sensors_Size1_Class2"",""cost"":1293,""sku"":null},""128064224"":{""id"":128064224,""category"":""module"",""name"":""Int_Sensors_Size2_Class2"",""cost"":3619,""sku"":null},""128064228"":{""id"":128064228,""category"":""module"",""name"":""Int_Sensors_Size3_Class1"",""cost"":4053,""sku"":null},""128064223"":{""id"":128064223,""category"":""module"",""name"":""Int_Sensors_Size2_Class1"",""cost"":1448,""sku"":null},""128064307"":{""id"":128064307,""category"":""module"",""name"":""Int_ShieldCellBank_Size2_Class5"",""cost"":56547,""sku"":null},""128064302"":{""id"":128064302,""category"":""module"",""name"":""Int_ShieldCellBank_Size1_Class5"",""cost"":20195,""sku"":null},""128064301"":{""id"":128064301,""category"":""module"",""name"":""Int_ShieldCellBank_Size1_Class4"",""cost"":8078,""sku"":null},""128064306"":{""id"":128064306,""category"":""module"",""name"":""Int_ShieldCellBank_Size2_Class4"",""cost"":22619,""sku"":null},""128064310"":{""id"":128064310,""category"":""module"",""name"":""Int_ShieldCellBank_Size3_Class3"",""cost"":25333,""sku"":null},""128064305"":{""id"":128064305,""category"":""module"",""name"":""Int_ShieldCellBank_Size2_Class3"",""cost"":9048,""sku"":null},""128064300"":{""id"":128064300,""category"":""module"",""name"":""Int_ShieldCellBank_Size1_Class3"",""cost"":3231,""sku"":null},""128064309"":{""id"":128064309,""category"":""module"",""name"":""Int_ShieldCellBank_Size3_Class2"",""cost"":10133,""sku"":null},""128064299"":{""id"":128064299,""category"":""module"",""name"":""Int_ShieldCellBank_Size1_Class2"",""cost"":1293,""sku"":null},""128064304"":{""id"":128064304,""category"":""module"",""name"":""Int_ShieldCellBank_Size2_Class2"",""cost"":3619,""sku"":null},""128064308"":{""id"":128064308,""category"":""module"",""name"":""Int_ShieldCellBank_Size3_Class1"",""cost"":4053,""sku"":null},""128064303"":{""id"":128064303,""category"":""module"",""name"":""Int_ShieldCellBank_Size2_Class1"",""cost"":1448,""sku"":null},""128064298"":{""id"":128064298,""category"":""module"",""name"":""Int_ShieldCellBank_Size1_Class1"",""cost"":517,""sku"":null},""128666723"":{""id"":128666723,""category"":""module"",""name"":""Int_FSDInterdictor_Size4_Class5"",""cost"":21337344,""sku"":null},""128666719"":{""id"":128666719,""category"":""module"",""name"":""Int_FSDInterdictor_Size4_Class4"",""cost"":7112448,""sku"":null},""128666715"":{""id"":128666715,""category"":""module"",""name"":""Int_FSDInterdictor_Size4_Class3"",""cost"":2370816,""sku"":null},""128666722"":{""id"":128666722,""category"":""module"",""name"":""Int_FSDInterdictor_Size3_Class5"",""cost"":7620480,""sku"":null},""128666718"":{""id"":128666718,""category"":""module"",""name"":""Int_FSDInterdictor_Size3_Class4"",""cost"":2540160,""sku"":null},""128666711"":{""id"":128666711,""category"":""module"",""name"":""Int_FSDInterdictor_Size4_Class2"",""cost"":790272,""sku"":null},""128666707"":{""id"":128666707,""category"":""module"",""name"":""Int_FSDInterdictor_Size4_Class1"",""cost"":263424,""sku"":null},""128666710"":{""id"":128666710,""category"":""module"",""name"":""Int_FSDInterdictor_Size3_Class2"",""cost"":282240,""sku"":null},""128666706"":{""id"":128666706,""category"":""module"",""name"":""Int_FSDInterdictor_Size3_Class1"",""cost"":94080,""sku"":null},""128066541"":{""id"":128066541,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size3_Class5"",""cost"":86400,""sku"":null},""128066536"":{""id"":128066536,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size1_Class5"",""cost"":9600,""sku"":null},""128066540"":{""id"":128066540,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size3_Class4"",""cost"":43200,""sku"":null},""128066535"":{""id"":128066535,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size1_Class4"",""cost"":4800,""sku"":null},""128066539"":{""id"":128066539,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size3_Class3"",""cost"":21600,""sku"":null},""128066534"":{""id"":128066534,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size1_Class3"",""cost"":2400,""sku"":null},""128066538"":{""id"":128066538,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size3_Class2"",""cost"":10800,""sku"":null},""128066533"":{""id"":128066533,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size1_Class2"",""cost"":1200,""sku"":null},""128066537"":{""id"":128066537,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size3_Class1"",""cost"":5400,""sku"":null},""128666682"":{""id"":128666682,""category"":""module"",""name"":""Int_FuelScoop_Size7_Class5"",""cost"":91180644,""sku"":null},""128666674"":{""id"":128666674,""category"":""module"",""name"":""Int_FuelScoop_Size7_Class4"",""cost"":22795161,""sku"":null},""128666667"":{""id"":128666667,""category"":""module"",""name"":""Int_FuelScoop_Size8_Class3"",""cost"":18065165,""sku"":null},""128666659"":{""id"":128666659,""category"":""module"",""name"":""Int_FuelScoop_Size8_Class2"",""cost"":4516291,""sku"":null},""128666658"":{""id"":128666658,""category"":""module"",""name"":""Int_FuelScoop_Size7_Class2"",""cost"":1424698,""sku"":null},""128666651"":{""id"":128666651,""category"":""module"",""name"":""Int_FuelScoop_Size8_Class1"",""cost"":1083910,""sku"":null},""128666650"":{""id"":128666650,""category"":""module"",""name"":""Int_FuelScoop_Size7_Class1"",""cost"":341927,""sku"":null},""128667605"":{""id"":128667605,""category"":""module"",""name"":""Int_Repairer_Size8_Class1"",""cost"":612220,""sku"":null},""128667611"":{""id"":128667611,""category"":""module"",""name"":""Int_Repairer_Size6_Class2"",""cost"":566870,""sku"":null},""128667604"":{""id"":128667604,""category"":""module"",""name"":""Int_Repairer_Size7_Class1"",""cost"":340122,""sku"":null},""128667610"":{""id"":128667610,""category"":""module"",""name"":""Int_Repairer_Size5_Class2"",""cost"":314928,""sku"":null},""128667616"":{""id"":128667616,""category"":""module"",""name"":""Int_Repairer_Size3_Class3"",""cost"":291600,""sku"":null},""128667623"":{""id"":128667623,""category"":""module"",""name"":""Int_Repairer_Size2_Class4"",""cost"":486000,""sku"":null},""128667615"":{""id"":128667615,""category"":""module"",""name"":""Int_Repairer_Size2_Class3"",""cost"":162000,""sku"":null},""128667622"":{""id"":128667622,""category"":""module"",""name"":""Int_Repairer_Size1_Class4"",""cost"":270000,""sku"":null},""128667609"":{""id"":128667609,""category"":""module"",""name"":""Int_Repairer_Size4_Class2"",""cost"":174960,""sku"":null},""128667603"":{""id"":128667603,""category"":""module"",""name"":""Int_Repairer_Size6_Class1"",""cost"":188957,""sku"":null},""128667602"":{""id"":128667602,""category"":""module"",""name"":""Int_Repairer_Size5_Class1"",""cost"":104976,""sku"":null},""128671278"":{""id"":128671278,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size3_Class5"",""cost"":86400,""sku"":null},""128671273"":{""id"":128671273,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size1_Class5"",""cost"":9600,""sku"":null},""128671287"":{""id"":128671287,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size7_Class4"",""cost"":3499200,""sku"":null},""128671282"":{""id"":128671282,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size5_Class4"",""cost"":388800,""sku"":null},""128671286"":{""id"":128671286,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size7_Class3"",""cost"":1749600,""sku"":null},""128671277"":{""id"":128671277,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size3_Class4"",""cost"":43200,""sku"":null},""128671285"":{""id"":128671285,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size7_Class2"",""cost"":874800,""sku"":null},""128064337"":{""id"":128064337,""category"":""module"",""name"":""Int_ShieldCellBank_Size8_Class5"",""cost"":27249391,""sku"":null},""128064336"":{""id"":128064336,""category"":""module"",""name"":""Int_ShieldCellBank_Size8_Class4"",""cost"":10899756,""sku"":null},""128064331"":{""id"":128064331,""category"":""module"",""name"":""Int_ShieldCellBank_Size7_Class4"",""cost"":3892770,""sku"":null},""128064330"":{""id"":128064330,""category"":""module"",""name"":""Int_ShieldCellBank_Size7_Class3"",""cost"":1557108,""sku"":null},""128064334"":{""id"":128064334,""category"":""module"",""name"":""Int_ShieldCellBank_Size8_Class2"",""cost"":1743961,""sku"":null},""128064329"":{""id"":128064329,""category"":""module"",""name"":""Int_ShieldCellBank_Size7_Class2"",""cost"":622843,""sku"":null},""128064333"":{""id"":128064333,""category"":""module"",""name"":""Int_ShieldCellBank_Size8_Class1"",""cost"":697584,""sku"":null},""128064328"":{""id"":128064328,""category"":""module"",""name"":""Int_ShieldCellBank_Size7_Class1"",""cost"":249137,""sku"":null},""128066550"":{""id"":128066550,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size7_Class4"",""cost"":3499200,""sku"":null},""128066549"":{""id"":128066549,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size7_Class3"",""cost"":1749600,""sku"":null},""128066548"":{""id"":128066548,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size7_Class2"",""cost"":874800,""sku"":null},""128066547"":{""id"":128066547,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size7_Class1"",""cost"":437400,""sku"":null},""128066543"":{""id"":128066543,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size5_Class2"",""cost"":97200,""sku"":null},""128066542"":{""id"":128066542,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size5_Class1"",""cost"":48600,""sku"":null},""128064177"":{""id"":128064177,""category"":""module"",""name"":""Int_LifeSupport_Size8_Class5"",""cost"":27249391,""sku"":null},""128064172"":{""id"":128064172,""category"":""module"",""name"":""Int_LifeSupport_Size7_Class5"",""cost"":9731925,""sku"":null},""128064171"":{""id"":128064171,""category"":""module"",""name"":""Int_LifeSupport_Size7_Class4"",""cost"":3892770,""sku"":null},""128064170"":{""id"":128064170,""category"":""module"",""name"":""Int_LifeSupport_Size7_Class3"",""cost"":1557108,""sku"":null},""128064174"":{""id"":128064174,""category"":""module"",""name"":""Int_LifeSupport_Size8_Class2"",""cost"":1743961,""sku"":null},""128064173"":{""id"":128064173,""category"":""module"",""name"":""Int_LifeSupport_Size8_Class1"",""cost"":697584,""sku"":null},""128064168"":{""id"":128064168,""category"":""module"",""name"":""Int_LifeSupport_Size7_Class1"",""cost"":249137,""sku"":null},""128064061"":{""id"":128064061,""category"":""module"",""name"":""Int_Powerplant_Size7_Class4"",""cost"":17096371,""sku"":null},""128064065"":{""id"":128064065,""category"":""module"",""name"":""Int_Powerplant_Size8_Class3"",""cost"":18065165,""sku"":null},""128064060"":{""id"":128064060,""category"":""module"",""name"":""Int_Powerplant_Size7_Class3"",""cost"":5698790,""sku"":null},""128064064"":{""id"":128064064,""category"":""module"",""name"":""Int_Powerplant_Size8_Class2"",""cost"":6021722,""sku"":null},""128064059"":{""id"":128064059,""category"":""module"",""name"":""Int_Powerplant_Size7_Class2"",""cost"":1899597,""sku"":null},""128064063"":{""id"":128064063,""category"":""module"",""name"":""Int_Powerplant_Size8_Class1"",""cost"":2007241,""sku"":null},""128064058"":{""id"":128064058,""category"":""module"",""name"":""Int_Powerplant_Size7_Class1"",""cost"":633199,""sku"":null},""128064097"":{""id"":128064097,""category"":""module"",""name"":""Int_Engine_Size7_Class5"",""cost"":51289112,""sku"":null},""128064101"":{""id"":128064101,""category"":""module"",""name"":""Int_Engine_Size8_Class4"",""cost"":54195495,""sku"":null},""128064096"":{""id"":128064096,""category"":""module"",""name"":""Int_Engine_Size7_Class4"",""cost"":17096371,""sku"":null},""128064094"":{""id"":128064094,""category"":""module"",""name"":""Int_Engine_Size7_Class2"",""cost"":1899597,""sku"":null},""128064098"":{""id"":128064098,""category"":""module"",""name"":""Int_Engine_Size8_Class1"",""cost"":2007241,""sku"":null},""128064093"":{""id"":128064093,""category"":""module"",""name"":""Int_Engine_Size7_Class1"",""cost"":633199,""sku"":null},""128671232"":{""id"":128671232,""category"":""module"",""name"":""Int_DroneControl_Collection_Size1_Class4"",""cost"":4800,""sku"":null},""128671244"":{""id"":128671244,""category"":""module"",""name"":""Int_DroneControl_Collection_Size7_Class1"",""cost"":437400,""sku"":null},""128671240"":{""id"":128671240,""category"":""module"",""name"":""Int_DroneControl_Collection_Size5_Class2"",""cost"":97200,""sku"":null},""128671236"":{""id"":128671236,""category"":""module"",""name"":""Int_DroneControl_Collection_Size3_Class3"",""cost"":21600,""sku"":null},""128671231"":{""id"":128671231,""category"":""module"",""name"":""Int_DroneControl_Collection_Size1_Class3"",""cost"":2400,""sku"":null},""128671239"":{""id"":128671239,""category"":""module"",""name"":""Int_DroneControl_Collection_Size5_Class1"",""cost"":48600,""sku"":null},""128671235"":{""id"":128671235,""category"":""module"",""name"":""Int_DroneControl_Collection_Size3_Class2"",""cost"":10800,""sku"":null},""128671230"":{""id"":128671230,""category"":""module"",""name"":""Int_DroneControl_Collection_Size1_Class2"",""cost"":1200,""sku"":null},""128671234"":{""id"":128671234,""category"":""module"",""name"":""Int_DroneControl_Collection_Size3_Class1"",""cost"":5400,""sku"":null},""128064136"":{""id"":128064136,""category"":""module"",""name"":""Int_Hyperdrive_Size8_Class4"",""cost"":54195495,""sku"":null},""128064131"":{""id"":128064131,""category"":""module"",""name"":""Int_Hyperdrive_Size7_Class4"",""cost"":17096371,""sku"":null},""128064135"":{""id"":128064135,""category"":""module"",""name"":""Int_Hyperdrive_Size8_Class3"",""cost"":18065165,""sku"":null},""128064130"":{""id"":128064130,""category"":""module"",""name"":""Int_Hyperdrive_Size7_Class3"",""cost"":5698790,""sku"":null},""128064134"":{""id"":128064134,""category"":""module"",""name"":""Int_Hyperdrive_Size8_Class2"",""cost"":6021722,""sku"":null},""128064129"":{""id"":128064129,""category"":""module"",""name"":""Int_Hyperdrive_Size7_Class2"",""cost"":1899597,""sku"":null},""128064133"":{""id"":128064133,""category"":""module"",""name"":""Int_Hyperdrive_Size8_Class1"",""cost"":2007241,""sku"":null},""128064128"":{""id"":128064128,""category"":""module"",""name"":""Int_Hyperdrive_Size7_Class1"",""cost"":633199,""sku"":null},""128064237"":{""id"":128064237,""category"":""module"",""name"":""Int_Sensors_Size4_Class5"",""cost"":443328,""sku"":null},""128064236"":{""id"":128064236,""category"":""module"",""name"":""Int_Sensors_Size4_Class4"",""cost"":177331,""sku"":null},""128064244"":{""id"":128064244,""category"":""module"",""name"":""Int_Sensors_Size6_Class2"",""cost"":222444,""sku"":null},""128064240"":{""id"":128064240,""category"":""module"",""name"":""Int_Sensors_Size5_Class3"",""cost"":198611,""sku"":null},""128064235"":{""id"":128064235,""category"":""module"",""name"":""Int_Sensors_Size4_Class3"",""cost"":70932,""sku"":null},""128064239"":{""id"":128064239,""category"":""module"",""name"":""Int_Sensors_Size5_Class2"",""cost"":79444,""sku"":null},""128064243"":{""id"":128064243,""category"":""module"",""name"":""Int_Sensors_Size6_Class1"",""cost"":88978,""sku"":null},""128064207"":{""id"":128064207,""category"":""module"",""name"":""Int_PowerDistributor_Size6_Class5"",""cost"":3475688,""sku"":null},""128064197"":{""id"":128064197,""category"":""module"",""name"":""Int_PowerDistributor_Size4_Class5"",""cost"":443328,""sku"":null},""128064206"":{""id"":128064206,""category"":""module"",""name"":""Int_PowerDistributor_Size6_Class4"",""cost"":1390275,""sku"":null},""128064196"":{""id"":128064196,""category"":""module"",""name"":""Int_PowerDistributor_Size4_Class4"",""cost"":177331,""sku"":null},""128064204"":{""id"":128064204,""category"":""module"",""name"":""Int_PowerDistributor_Size6_Class2"",""cost"":222444,""sku"":null},""128064195"":{""id"":128064195,""category"":""module"",""name"":""Int_PowerDistributor_Size4_Class3"",""cost"":70932,""sku"":null},""128064199"":{""id"":128064199,""category"":""module"",""name"":""Int_PowerDistributor_Size5_Class2"",""cost"":79444,""sku"":null},""128668546"":{""id"":128668546,""category"":""module"",""name"":""Int_HullReinforcement_Size5_Class2"",""cost"":450000,""sku"":null},""128668544"":{""id"":128668544,""category"":""module"",""name"":""Int_HullReinforcement_Size4_Class2"",""cost"":195000,""sku"":null},""128668542"":{""id"":128668542,""category"":""module"",""name"":""Int_HullReinforcement_Size3_Class2"",""cost"":84000,""sku"":null},""128668540"":{""id"":128668540,""category"":""module"",""name"":""Int_HullReinforcement_Size2_Class2"",""cost"":36000,""sku"":null},""128668538"":{""id"":128668538,""category"":""module"",""name"":""Int_HullReinforcement_Size1_Class2"",""cost"":15000,""sku"":null},""128666680"":{""id"":128666680,""category"":""module"",""name"":""Int_FuelScoop_Size5_Class5"",""cost"":9073694,""sku"":null},""128666665"":{""id"":128666665,""category"":""module"",""name"":""Int_FuelScoop_Size6_Class3"",""cost"":1797726,""sku"":null},""128666672"":{""id"":128666672,""category"":""module"",""name"":""Int_FuelScoop_Size5_Class4"",""cost"":2268424,""sku"":null},""128666671"":{""id"":128666671,""category"":""module"",""name"":""Int_FuelScoop_Size4_Class4"",""cost"":715591,""sku"":null},""128666664"":{""id"":128666664,""category"":""module"",""name"":""Int_FuelScoop_Size5_Class3"",""cost"":567106,""sku"":null},""128666663"":{""id"":128666663,""category"":""module"",""name"":""Int_FuelScoop_Size4_Class3"",""cost"":178898,""sku"":null},""128668545"":{""id"":128668545,""category"":""module"",""name"":""Int_HullReinforcement_Size5_Class1"",""cost"":150000,""sku"":null},""128668543"":{""id"":128668543,""category"":""module"",""name"":""Int_HullReinforcement_Size4_Class1"",""cost"":65000,""sku"":null},""128668541"":{""id"":128668541,""category"":""module"",""name"":""Int_HullReinforcement_Size3_Class1"",""cost"":28000,""sku"":null},""128668539"":{""id"":128668539,""category"":""module"",""name"":""Int_HullReinforcement_Size2_Class1"",""cost"":12000,""sku"":null},""128668537"":{""id"":128668537,""category"":""module"",""name"":""Int_HullReinforcement_Size1_Class1"",""cost"":5000,""sku"":null},""128064317"":{""id"":128064317,""category"":""module"",""name"":""Int_ShieldCellBank_Size4_Class5"",""cost"":443328,""sku"":null},""128064326"":{""id"":128064326,""category"":""module"",""name"":""Int_ShieldCellBank_Size6_Class4"",""cost"":1390275,""sku"":null},""128064321"":{""id"":128064321,""category"":""module"",""name"":""Int_ShieldCellBank_Size5_Class4"",""cost"":496527,""sku"":null},""128064316"":{""id"":128064316,""category"":""module"",""name"":""Int_ShieldCellBank_Size4_Class4"",""cost"":177331,""sku"":null},""128064325"":{""id"":128064325,""category"":""module"",""name"":""Int_ShieldCellBank_Size6_Class3"",""cost"":556110,""sku"":null},""128671248"":{""id"":128671248,""category"":""module"",""name"":""Int_DroneControl_Collection_Size7_Class5"",""cost"":6998400,""sku"":null},""128671243"":{""id"":128671243,""category"":""module"",""name"":""Int_DroneControl_Collection_Size5_Class5"",""cost"":777600,""sku"":null},""128671233"":{""id"":128671233,""category"":""module"",""name"":""Int_DroneControl_Collection_Size1_Class5"",""cost"":9600,""sku"":null},""128671247"":{""id"":128671247,""category"":""module"",""name"":""Int_DroneControl_Collection_Size7_Class4"",""cost"":3499200,""sku"":null},""128668536"":{""id"":128668536,""category"":""module"",""name"":""Hpt_ShieldBooster_Size0_Class5"",""cost"":281000,""sku"":null},""128668535"":{""id"":128668535,""category"":""module"",""name"":""Hpt_ShieldBooster_Size0_Class4"",""cost"":122000,""sku"":null},""128668534"":{""id"":128668534,""category"":""module"",""name"":""Hpt_ShieldBooster_Size0_Class3"",""cost"":53000,""sku"":null},""128668533"":{""id"":128668533,""category"":""module"",""name"":""Hpt_ShieldBooster_Size0_Class2"",""cost"":23000,""sku"":null},""128064167"":{""id"":128064167,""category"":""module"",""name"":""Int_LifeSupport_Size6_Class5"",""cost"":3475688,""sku"":null},""128064157"":{""id"":128064157,""category"":""module"",""name"":""Int_LifeSupport_Size4_Class5"",""cost"":443328,""sku"":null},""128064166"":{""id"":128064166,""category"":""module"",""name"":""Int_LifeSupport_Size6_Class4"",""cost"":1390275,""sku"":null},""128064052"":{""id"":128064052,""category"":""module"",""name"":""Int_Powerplant_Size5_Class5"",""cost"":5103953,""sku"":null},""128064047"":{""id"":128064047,""category"":""module"",""name"":""Int_Powerplant_Size4_Class5"",""cost"":1610080,""sku"":null},""128064287"":{""id"":128064287,""category"":""module"",""name"":""Int_ShieldGenerator_Size6_Class5"",""cost"":16179531,""sku"":null},""128049282"":{""id"":128049282,""category"":""module"",""name"":""CobraMkIII_Armour_Grade3"",""cost"":314746,""sku"":null},""128049281"":{""id"":128049281,""category"":""module"",""name"":""CobraMkIII_Armour_Grade2"",""cost"":139887,""sku"":null},""128049280"":{""id"":128049280,""category"":""module"",""name"":""CobraMkIII_Armour_Grade1"",""cost"":0,""sku"":null},""128049283"":{""id"":128049283,""category"":""module"",""name"":""CobraMkIII_Armour_Mirrored"",""cost"":734407,""sku"":null},""128049284"":{""id"":128049284,""category"":""module"",""name"":""CobraMkIII_Armour_Reactive"",""cost"":824285,""sku"":null},""128672266"":{""id"":128672266,""category"":""module"",""name"":""CobraMkIV_Armour_Grade3"",""cost"":688246,""sku"":null},""128672265"":{""id"":128672265,""category"":""module"",""name"":""CobraMkIV_Armour_Grade2"",""cost"":305887,""sku"":null},""128672264"":{""id"":128672264,""category"":""module"",""name"":""CobraMkIV_Armour_Grade1"",""cost"":0,""sku"":null},""128672267"":{""id"":128672267,""category"":""module"",""name"":""CobraMkIV_Armour_Mirrored"",""cost"":1605907,""sku"":null},""128672268"":{""id"":128672268,""category"":""module"",""name"":""CobraMkIV_Armour_Reactive"",""cost"":1802440,""sku"":null},""128049288"":{""id"":128049288,""category"":""module"",""name"":""Type6_Armour_Grade3"",""cost"":941350,""sku"":null},""128049287"":{""id"":128049287,""category"":""module"",""name"":""Type6_Armour_Grade2"",""cost"":418378,""sku"":null},""128049286"":{""id"":128049286,""category"":""module"",""name"":""Type6_Armour_Grade1"",""cost"":0,""sku"":null},""128049289"":{""id"":128049289,""category"":""module"",""name"":""Type6_Armour_Mirrored"",""cost"":2224725,""sku"":null},""128049290"":{""id"":128049290,""category"":""module"",""name"":""Type6_Armour_Reactive"",""cost"":2465292,""sku"":null},""128666700"":{""id"":128666700,""category"":""module"",""name"":""Int_Refinery_Size1_Class5"",""cost"":486000,""sku"":null},""128064251"":{""id"":128064251,""category"":""module"",""name"":""Int_Sensors_Size7_Class4"",""cost"":3892770,""sku"":null},""128064250"":{""id"":128064250,""category"":""module"",""name"":""Int_Sensors_Size7_Class3"",""cost"":1557108,""sku"":null},""128666677"":{""id"":128666677,""category"":""module"",""name"":""Int_FuelScoop_Size2_Class5"",""cost"":284844,""sku"":null},""128666669"":{""id"":128666669,""category"":""module"",""name"":""Int_FuelScoop_Size2_Class4"",""cost"":71211,""sku"":null},""128064218"":{""id"":128064218,""category"":""module"",""name"":""Int_Sensors_Size1_Class1"",""cost"":517,""sku"":null},""128064246"":{""id"":128064246,""category"":""module"",""name"":""Int_Sensors_Size6_Class4"",""cost"":1390275,""sku"":null},""128064238"":{""id"":128064238,""category"":""module"",""name"":""Int_Sensors_Size5_Class1"",""cost"":31778,""sku"":null},""128064234"":{""id"":128064234,""category"":""module"",""name"":""Int_Sensors_Size4_Class2"",""cost"":28373,""sku"":null},""128064233"":{""id"":128064233,""category"":""module"",""name"":""Int_Sensors_Size4_Class1"",""cost"":11349,""sku"":null},""128064202"":{""id"":128064202,""category"":""module"",""name"":""Int_PowerDistributor_Size5_Class5"",""cost"":1241317,""sku"":null},""128064201"":{""id"":128064201,""category"":""module"",""name"":""Int_PowerDistributor_Size5_Class4"",""cost"":496527,""sku"":null},""128064205"":{""id"":128064205,""category"":""module"",""name"":""Int_PowerDistributor_Size6_Class3"",""cost"":556110,""sku"":null},""128064200"":{""id"":128064200,""category"":""module"",""name"":""Int_PowerDistributor_Size5_Class3"",""cost"":198611,""sku"":null},""128064203"":{""id"":128064203,""category"":""module"",""name"":""Int_PowerDistributor_Size6_Class1"",""cost"":88978,""sku"":null},""128064198"":{""id"":128064198,""category"":""module"",""name"":""Int_PowerDistributor_Size5_Class1"",""cost"":31778,""sku"":null},""128064194"":{""id"":128064194,""category"":""module"",""name"":""Int_PowerDistributor_Size4_Class2"",""cost"":28373,""sku"":null},""128064193"":{""id"":128064193,""category"":""module"",""name"":""Int_PowerDistributor_Size4_Class1"",""cost"":11349,""sku"":null},""128666681"":{""id"":128666681,""category"":""module"",""name"":""Int_FuelScoop_Size6_Class5"",""cost"":28763610,""sku"":null},""128666673"":{""id"":128666673,""category"":""module"",""name"":""Int_FuelScoop_Size6_Class4"",""cost"":7190903,""sku"":null},""128666657"":{""id"":128666657,""category"":""module"",""name"":""Int_FuelScoop_Size6_Class2"",""cost"":449431,""sku"":null},""128666656"":{""id"":128666656,""category"":""module"",""name"":""Int_FuelScoop_Size5_Class2"",""cost"":141776,""sku"":null},""128666649"":{""id"":128666649,""category"":""module"",""name"":""Int_FuelScoop_Size6_Class1"",""cost"":107864,""sku"":null},""128666655"":{""id"":128666655,""category"":""module"",""name"":""Int_FuelScoop_Size4_Class2"",""cost"":44724,""sku"":null},""128666648"":{""id"":128666648,""category"":""module"",""name"":""Int_FuelScoop_Size5_Class1"",""cost"":34026,""sku"":null},""128666647"":{""id"":128666647,""category"":""module"",""name"":""Int_FuelScoop_Size4_Class1"",""cost"":10734,""sku"":null},""128064142"":{""id"":128064142,""category"":""module"",""name"":""Int_LifeSupport_Size1_Class5"",""cost"":20195,""sku"":null},""128671263"":{""id"":128671263,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size5_Class5"",""cost"":777600,""sku"":null},""128064324"":{""id"":128064324,""category"":""module"",""name"":""Int_ShieldCellBank_Size6_Class2"",""cost"":222444,""sku"":null},""128064315"":{""id"":128064315,""category"":""module"",""name"":""Int_ShieldCellBank_Size4_Class3"",""cost"":70932,""sku"":null},""128064323"":{""id"":128064323,""category"":""module"",""name"":""Int_ShieldCellBank_Size6_Class1"",""cost"":88978,""sku"":null},""128064319"":{""id"":128064319,""category"":""module"",""name"":""Int_ShieldCellBank_Size5_Class2"",""cost"":79444,""sku"":null},""128064318"":{""id"":128064318,""category"":""module"",""name"":""Int_ShieldCellBank_Size5_Class1"",""cost"":31778,""sku"":null},""128064314"":{""id"":128064314,""category"":""module"",""name"":""Int_ShieldCellBank_Size4_Class2"",""cost"":28373,""sku"":null},""128064313"":{""id"":128064313,""category"":""module"",""name"":""Int_ShieldCellBank_Size4_Class1"",""cost"":11349,""sku"":null},""128064161"":{""id"":128064161,""category"":""module"",""name"":""Int_LifeSupport_Size5_Class4"",""cost"":496527,""sku"":null},""128064156"":{""id"":128064156,""category"":""module"",""name"":""Int_LifeSupport_Size4_Class4"",""cost"":177331,""sku"":null},""128064160"":{""id"":128064160,""category"":""module"",""name"":""Int_LifeSupport_Size5_Class3"",""cost"":198611,""sku"":null},""128064164"":{""id"":128064164,""category"":""module"",""name"":""Int_LifeSupport_Size6_Class2"",""cost"":222444,""sku"":null},""128064155"":{""id"":128064155,""category"":""module"",""name"":""Int_LifeSupport_Size4_Class3"",""cost"":70932,""sku"":null},""128064163"":{""id"":128064163,""category"":""module"",""name"":""Int_LifeSupport_Size6_Class1"",""cost"":88978,""sku"":null},""128064159"":{""id"":128064159,""category"":""module"",""name"":""Int_LifeSupport_Size5_Class2"",""cost"":79444,""sku"":null},""128064158"":{""id"":128064158,""category"":""module"",""name"":""Int_LifeSupport_Size5_Class1"",""cost"":31778,""sku"":null},""128668532"":{""id"":128668532,""category"":""module"",""name"":""Hpt_ShieldBooster_Size0_Class1"",""cost"":10000,""sku"":null},""128064057"":{""id"":128064057,""category"":""module"",""name"":""Int_Powerplant_Size6_Class5"",""cost"":16179531,""sku"":null},""128064051"":{""id"":128064051,""category"":""module"",""name"":""Int_Powerplant_Size5_Class4"",""cost"":1701318,""sku"":null},""128064046"":{""id"":128064046,""category"":""module"",""name"":""Int_Powerplant_Size4_Class4"",""cost"":536693,""sku"":null},""128064055"":{""id"":128064055,""category"":""module"",""name"":""Int_Powerplant_Size6_Class3"",""cost"":1797726,""sku"":null},""128064050"":{""id"":128064050,""category"":""module"",""name"":""Int_Powerplant_Size5_Class3"",""cost"":567106,""sku"":null},""128064054"":{""id"":128064054,""category"":""module"",""name"":""Int_Powerplant_Size6_Class2"",""cost"":599242,""sku"":null},""128064045"":{""id"":128064045,""category"":""module"",""name"":""Int_Powerplant_Size4_Class3"",""cost"":178898,""sku"":null},""128064087"":{""id"":128064087,""category"":""module"",""name"":""Int_Engine_Size5_Class5"",""cost"":5103953,""sku"":null},""128064082"":{""id"":128064082,""category"":""module"",""name"":""Int_Engine_Size4_Class5"",""cost"":1610080,""sku"":null},""128064091"":{""id"":128064091,""category"":""module"",""name"":""Int_Engine_Size6_Class4"",""cost"":5393177,""sku"":null},""128064086"":{""id"":128064086,""category"":""module"",""name"":""Int_Engine_Size5_Class4"",""cost"":1701318,""sku"":null},""128064081"":{""id"":128064081,""category"":""module"",""name"":""Int_Engine_Size4_Class4"",""cost"":536693,""sku"":null},""128064090"":{""id"":128064090,""category"":""module"",""name"":""Int_Engine_Size6_Class3"",""cost"":1797726,""sku"":null},""128064089"":{""id"":128064089,""category"":""module"",""name"":""Int_Engine_Size6_Class2"",""cost"":599242,""sku"":null},""128064122"":{""id"":128064122,""category"":""module"",""name"":""Int_Hyperdrive_Size5_Class5"",""cost"":5103953,""sku"":null},""128064117"":{""id"":128064117,""category"":""module"",""name"":""Int_Hyperdrive_Size4_Class5"",""cost"":1610080,""sku"":null},""128064121"":{""id"":128064121,""category"":""module"",""name"":""Int_Hyperdrive_Size5_Class4"",""cost"":1701318,""sku"":null},""128064116"":{""id"":128064116,""category"":""module"",""name"":""Int_Hyperdrive_Size4_Class4"",""cost"":536693,""sku"":null},""128064125"":{""id"":128064125,""category"":""module"",""name"":""Int_Hyperdrive_Size6_Class3"",""cost"":1797726,""sku"":null},""128064124"":{""id"":128064124,""category"":""module"",""name"":""Int_Hyperdrive_Size6_Class2"",""cost"":599242,""sku"":null},""128666721"":{""id"":128666721,""category"":""module"",""name"":""Int_FSDInterdictor_Size2_Class5"",""cost"":2721600,""sku"":null},""128064277"":{""id"":128064277,""category"":""module"",""name"":""Int_ShieldGenerator_Size4_Class5"",""cost"":1610080,""sku"":null},""128064286"":{""id"":128064286,""category"":""module"",""name"":""Int_ShieldGenerator_Size6_Class4"",""cost"":5393177,""sku"":null},""128064281"":{""id"":128064281,""category"":""module"",""name"":""Int_ShieldGenerator_Size5_Class4"",""cost"":1701318,""sku"":null},""128064276"":{""id"":128064276,""category"":""module"",""name"":""Int_ShieldGenerator_Size4_Class4"",""cost"":536693,""sku"":null},""128671336"":{""id"":128671336,""category"":""module"",""name"":""Int_ShieldGenerator_Size6_Class3_Fast"",""cost"":2696589,""sku"":null},""128064285"":{""id"":128064285,""category"":""module"",""name"":""Int_ShieldGenerator_Size6_Class3"",""cost"":1797726,""sku"":null},""128671229"":{""id"":128671229,""category"":""module"",""name"":""Int_DroneControl_Collection_Size1_Class1"",""cost"":600,""sku"":null},""128064216"":{""id"":128064216,""category"":""module"",""name"":""Int_PowerDistributor_Size8_Class4"",""cost"":10899756,""sku"":null},""128064211"":{""id"":128064211,""category"":""module"",""name"":""Int_PowerDistributor_Size7_Class4"",""cost"":3892770,""sku"":null},""128064215"":{""id"":128064215,""category"":""module"",""name"":""Int_PowerDistributor_Size8_Class3"",""cost"":4359903,""sku"":null},""128064209"":{""id"":128064209,""category"":""module"",""name"":""Int_PowerDistributor_Size7_Class2"",""cost"":622843,""sku"":null},""128666703"":{""id"":128666703,""category"":""module"",""name"":""Int_Refinery_Size4_Class5"",""cost"":4500846,""sku"":null},""128666702"":{""id"":128666702,""category"":""module"",""name"":""Int_Refinery_Size3_Class5"",""cost"":2143260,""sku"":null},""128666698"":{""id"":128666698,""category"":""module"",""name"":""Int_Refinery_Size3_Class4"",""cost"":714420,""sku"":null},""128667637"":{""id"":128667637,""category"":""module"",""name"":""Int_Repairer_Size8_Class5"",""cost"":49589823,""sku"":null},""128667628"":{""id"":128667628,""category"":""module"",""name"":""Int_Repairer_Size7_Class4"",""cost"":9183300,""sku"":null},""128667635"":{""id"":128667635,""category"":""module"",""name"":""Int_Repairer_Size6_Class5"",""cost"":15305501,""sku"":null},""128667634"":{""id"":128667634,""category"":""module"",""name"":""Int_Repairer_Size5_Class5"",""cost"":8503056,""sku"":null},""128671288"":{""id"":128671288,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size7_Class5"",""cost"":6998400,""sku"":null},""128671281"":{""id"":128671281,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size5_Class3"",""cost"":194400,""sku"":null},""128064312"":{""id"":128064312,""category"":""module"",""name"":""Int_ShieldCellBank_Size3_Class5"",""cost"":158331,""sku"":null},""128064311"":{""id"":128064311,""category"":""module"",""name"":""Int_ShieldCellBank_Size3_Class4"",""cost"":63333,""sku"":null},""128666675"":{""id"":128666675,""category"":""module"",""name"":""Int_FuelScoop_Size8_Class4"",""cost"":72260660,""sku"":null},""128671238"":{""id"":128671238,""category"":""module"",""name"":""Int_DroneControl_Collection_Size3_Class5"",""cost"":86400,""sku"":null},""128671242"":{""id"":128671242,""category"":""module"",""name"":""Int_DroneControl_Collection_Size5_Class4"",""cost"":388800,""sku"":null},""128671246"":{""id"":128671246,""category"":""module"",""name"":""Int_DroneControl_Collection_Size7_Class3"",""cost"":1749600,""sku"":null},""128066551"":{""id"":128066551,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size7_Class5"",""cost"":6998400,""sku"":null},""128066546"":{""id"":128066546,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size5_Class5"",""cost"":777600,""sku"":null},""128682043"":{""id"":128682043,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Metallic"",""cost"":0,""sku"":null},""128667727"":{""id"":128667727,""category"":""paintjob"",""name"":""paintjob_CobraMkiii_Default_52"",""cost"":0,""sku"":null},""128066428"":{""id"":128066428,""category"":""paintjob"",""name"":""paintjob_cobramkiii_wireframe_01"",""cost"":0,""sku"":null},""128670861"":{""id"":128670861,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Onionhead1_01"",""cost"":0,""sku"":null},""128671133"":{""id"":128671133,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_green"",""cost"":0,""sku"":null},""128671134"":{""id"":128671134,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_blue"",""cost"":0,""sku"":null},""128671135"":{""id"":128671135,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_orange"",""cost"":0,""sku"":null},""128671136"":{""id"":128671136,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_red"",""cost"":0,""sku"":null},""128671137"":{""id"":128671137,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_purple"",""cost"":0,""sku"":null},""128671138"":{""id"":128671138,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_yellow"",""cost"":0,""sku"":null},""128667638"":{""id"":128667638,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Merc"",""cost"":0,""sku"":null},""128066405"":{""id"":128066405,""category"":""paintjob"",""name"":""paintjob_eagle_stripe1_02"",""cost"":0,""sku"":null},""128066406"":{""id"":128066406,""category"":""paintjob"",""name"":""paintjob_eagle_doublestripe_01"",""cost"":0,""sku"":null},""128066416"":{""id"":128066416,""category"":""paintjob"",""name"":""paintjob_eagle_thirds_01"",""cost"":0,""sku"":null},""128066419"":{""id"":128066419,""category"":""paintjob"",""name"":""paintjob_eagle_stripe1_03"",""cost"":0,""sku"":null},""128066420"":{""id"":128066420,""category"":""paintjob"",""name"":""paintjob_eagle_thirds_02"",""cost"":0,""sku"":null},""128066430"":{""id"":128066430,""category"":""paintjob"",""name"":""paintjob_eagle_stripe1_01"",""cost"":0,""sku"":null},""128066436"":{""id"":128066436,""category"":""paintjob"",""name"":""paintjob_eagle_camo_03"",""cost"":0,""sku"":null},""128066437"":{""id"":128066437,""category"":""paintjob"",""name"":""paintjob_eagle_thirds_03"",""cost"":0,""sku"":null},""128066441"":{""id"":128066441,""category"":""paintjob"",""name"":""paintjob_eagle_camo_02"",""cost"":0,""sku"":null},""128066449"":{""id"":128066449,""category"":""paintjob"",""name"":""paintjob_eagle_doublestripe_02"",""cost"":0,""sku"":null},""128066453"":{""id"":128066453,""category"":""paintjob"",""name"":""paintjob_eagle_doublestripe_03"",""cost"":0,""sku"":null},""128066456"":{""id"":128066456,""category"":""paintjob"",""name"":""paintjob_eagle_camo_01"",""cost"":0,""sku"":null},""128671139"":{""id"":128671139,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_green"",""cost"":0,""sku"":null},""128671140"":{""id"":128671140,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_blue"",""cost"":0,""sku"":null},""128671141"":{""id"":128671141,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_orange"",""cost"":0,""sku"":null},""128671142"":{""id"":128671142,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_red"",""cost"":0,""sku"":null},""128671143"":{""id"":128671143,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_purple"",""cost"":0,""sku"":null},""128671144"":{""id"":128671144,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_yellow"",""cost"":0,""sku"":null},""128671777"":{""id"":128671777,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Desert_Sand"",""cost"":0,""sku"":null},""128671778"":{""id"":128671778,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Earth_Yellow"",""cost"":0,""sku"":null},""128672802"":{""id"":128672802,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_BlackFriday_01"",""cost"":0,""sku"":null},""128671779"":{""id"":128671779,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Dark_Green"",""cost"":0,""sku"":null},""128671780"":{""id"":128671780,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Forest_Green"",""cost"":0,""sku"":null},""128671781"":{""id"":128671781,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Sand"",""cost"":0,""sku"":null},""128671782"":{""id"":128671782,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Earth_Red"",""cost"":0,""sku"":null},""128672426"":{""id"":128672426,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_SpecialEffect_01"",""cost"":0,""sku"":null},""128066404"":{""id"":128066404,""category"":""paintjob"",""name"":""paintjob_sidewinder_default_02"",""cost"":0,""sku"":null},""128066408"":{""id"":128066408,""category"":""paintjob"",""name"":""paintjob_sidewinder_default_03"",""cost"":0,""sku"":null},""128066414"":{""id"":128066414,""category"":""paintjob"",""name"":""paintjob_sidewinder_doublestripe_08"",""cost"":0,""sku"":null},""128066423"":{""id"":128066423,""category"":""paintjob"",""name"":""paintjob_sidewinder_doublestripe_05"",""cost"":0,""sku"":null},""128066431"":{""id"":128066431,""category"":""paintjob"",""name"":""paintjob_sidewinder_thirds_07"",""cost"":0,""sku"":null},""128066432"":{""id"":128066432,""category"":""paintjob"",""name"":""paintjob_sidewinder_thirds_01"",""cost"":0,""sku"":null},""128066433"":{""id"":128066433,""category"":""paintjob"",""name"":""paintjob_sidewinder_doublestripe_07"",""cost"":0,""sku"":null},""128066440"":{""id"":128066440,""category"":""paintjob"",""name"":""paintjob_sidewinder_camo_01"",""cost"":0,""sku"":null},""128066444"":{""id"":128066444,""category"":""paintjob"",""name"":""paintjob_sidewinder_thirds_06"",""cost"":0,""sku"":null},""128066447"":{""id"":128066447,""category"":""paintjob"",""name"":""paintjob_sidewinder_camo_03"",""cost"":0,""sku"":null},""128066448"":{""id"":128066448,""category"":""paintjob"",""name"":""paintjob_sidewinder_default_04"",""cost"":0,""sku"":null},""128066454"":{""id"":128066454,""category"":""paintjob"",""name"":""paintjob_sidewinder_camo_02"",""cost"":0,""sku"":null},""128671181"":{""id"":128671181,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_green"",""cost"":0,""sku"":null},""128671182"":{""id"":128671182,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_blue"",""cost"":0,""sku"":null},""128671183"":{""id"":128671183,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_orange"",""cost"":0,""sku"":null},""128671184"":{""id"":128671184,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_red"",""cost"":0,""sku"":null},""128671185"":{""id"":128671185,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_purple"",""cost"":0,""sku"":null},""128671186"":{""id"":128671186,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_yellow"",""cost"":0,""sku"":null},""128066407"":{""id"":128066407,""category"":""paintjob"",""name"":""paintjob_viper_flag_switzerland_01"",""cost"":0,""sku"":null},""128066409"":{""id"":128066409,""category"":""paintjob"",""name"":""paintjob_viper_flag_belgium_01"",""cost"":0,""sku"":null},""128066410"":{""id"":128066410,""category"":""paintjob"",""name"":""paintjob_viper_flag_australia_01"",""cost"":0,""sku"":null},""128066411"":{""id"":128066411,""category"":""paintjob"",""name"":""paintjob_viper_default_01"",""cost"":0,""sku"":null},""128066412"":{""id"":128066412,""category"":""paintjob"",""name"":""paintjob_viper_stripe2_02"",""cost"":0,""sku"":null},""128066413"":{""id"":128066413,""category"":""paintjob"",""name"":""paintjob_viper_flag_austria_01"",""cost"":0,""sku"":null},""128066415"":{""id"":128066415,""category"":""paintjob"",""name"":""paintjob_viper_stripe1_01"",""cost"":0,""sku"":null},""128066417"":{""id"":128066417,""category"":""paintjob"",""name"":""paintjob_viper_flag_spain_01"",""cost"":0,""sku"":null},""128066418"":{""id"":128066418,""category"":""paintjob"",""name"":""paintjob_viper_stripe1_02"",""cost"":0,""sku"":null},""128066421"":{""id"":128066421,""category"":""paintjob"",""name"":""paintjob_viper_flag_denmark_01"",""cost"":0,""sku"":null},""128066422"":{""id"":128066422,""category"":""paintjob"",""name"":""paintjob_viper_police_federation_01"",""cost"":0,""sku"":null},""128666742"":{""id"":128666742,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Hotrod_01"",""cost"":0,""sku"":null},""128666743"":{""id"":128666743,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Hotrod_03"",""cost"":0,""sku"":null},""128066424"":{""id"":128066424,""category"":""paintjob"",""name"":""paintjob_viper_flag_newzealand_01"",""cost"":0,""sku"":null},""128066425"":{""id"":128066425,""category"":""paintjob"",""name"":""paintjob_viper_flag_italy_01"",""cost"":0,""sku"":null},""128066426"":{""id"":128066426,""category"":""paintjob"",""name"":""paintjob_viper_stripe2_04"",""cost"":0,""sku"":null},""128066427"":{""id"":128066427,""category"":""paintjob"",""name"":""paintjob_viper_police_independent_01"",""cost"":0,""sku"":null},""128066429"":{""id"":128066429,""category"":""paintjob"",""name"":""paintjob_viper_default_03"",""cost"":0,""sku"":null},""128066434"":{""id"":128066434,""category"":""paintjob"",""name"":""paintjob_viper_flag_uk_01"",""cost"":0,""sku"":null},""128066435"":{""id"":128066435,""category"":""paintjob"",""name"":""paintjob_viper_flag_germany_01"",""cost"":0,""sku"":null},""128066438"":{""id"":128066438,""category"":""paintjob"",""name"":""paintjob_viper_flag_netherlands_01"",""cost"":0,""sku"":null},""128066439"":{""id"":128066439,""category"":""paintjob"",""name"":""paintjob_viper_flag_usa_01"",""cost"":0,""sku"":null},""128066442"":{""id"":128066442,""category"":""paintjob"",""name"":""paintjob_viper_flag_russia_01"",""cost"":0,""sku"":null},""128066443"":{""id"":128066443,""category"":""paintjob"",""name"":""paintjob_viper_flag_canada_01"",""cost"":0,""sku"":null},""128066445"":{""id"":128066445,""category"":""paintjob"",""name"":""paintjob_viper_flag_sweden_01"",""cost"":0,""sku"":null},""128066446"":{""id"":128066446,""category"":""paintjob"",""name"":""paintjob_viper_flag_poland_01"",""cost"":0,""sku"":null},""128066450"":{""id"":128066450,""category"":""paintjob"",""name"":""paintjob_viper_flag_finland_01"",""cost"":0,""sku"":null},""128066451"":{""id"":128066451,""category"":""paintjob"",""name"":""paintjob_viper_flag_france_01"",""cost"":0,""sku"":null},""128066452"":{""id"":128066452,""category"":""paintjob"",""name"":""paintjob_viper_police_empire_01"",""cost"":0,""sku"":null},""128066455"":{""id"":128066455,""category"":""paintjob"",""name"":""paintjob_viper_flag_norway_01"",""cost"":0,""sku"":null},""128671205"":{""id"":128671205,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_green"",""cost"":0,""sku"":null},""128671206"":{""id"":128671206,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_blue"",""cost"":0,""sku"":null},""128671207"":{""id"":128671207,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_orange"",""cost"":0,""sku"":null},""128671208"":{""id"":128671208,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_red"",""cost"":0,""sku"":null},""128671209"":{""id"":128671209,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_purple"",""cost"":0,""sku"":null},""128671210"":{""id"":128671210,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_yellow"",""cost"":0,""sku"":null},""128671127"":{""id"":128671127,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_green"",""cost"":0,""sku"":null},""128671128"":{""id"":128671128,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_blue"",""cost"":0,""sku"":null},""128671129"":{""id"":128671129,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_orange"",""cost"":0,""sku"":null},""128671130"":{""id"":128671130,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_red"",""cost"":0,""sku"":null},""128671131"":{""id"":128671131,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_purple"",""cost"":0,""sku"":null},""128671132"":{""id"":128671132,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_yellow"",""cost"":0,""sku"":null},""128671151"":{""id"":128671151,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_green"",""cost"":0,""sku"":null},""128671152"":{""id"":128671152,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_blue"",""cost"":0,""sku"":null},""128671153"":{""id"":128671153,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_orange"",""cost"":0,""sku"":null},""128671154"":{""id"":128671154,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_red"",""cost"":0,""sku"":null},""128671155"":{""id"":128671155,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_purple"",""cost"":0,""sku"":null},""128671156"":{""id"":128671156,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_yellow"",""cost"":0,""sku"":null},""128671175"":{""id"":128671175,""category"":""paintjob"",""name"":""paintjob_python_vibrant_green"",""cost"":0,""sku"":null},""128671176"":{""id"":128671176,""category"":""paintjob"",""name"":""paintjob_python_vibrant_blue"",""cost"":0,""sku"":null},""128671177"":{""id"":128671177,""category"":""paintjob"",""name"":""paintjob_python_vibrant_orange"",""cost"":0,""sku"":null},""128671178"":{""id"":128671178,""category"":""paintjob"",""name"":""paintjob_python_vibrant_red"",""cost"":0,""sku"":null},""128671179"":{""id"":128671179,""category"":""paintjob"",""name"":""paintjob_python_vibrant_purple"",""cost"":0,""sku"":null},""128671180"":{""id"":128671180,""category"":""paintjob"",""name"":""paintjob_python_vibrant_yellow"",""cost"":0,""sku"":null},""128671121"":{""id"":128671121,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_green"",""cost"":0,""sku"":null},""128671122"":{""id"":128671122,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_blue"",""cost"":0,""sku"":null},""128671123"":{""id"":128671123,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_orange"",""cost"":0,""sku"":null},""128671124"":{""id"":128671124,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_red"",""cost"":0,""sku"":null},""128671125"":{""id"":128671125,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_purple"",""cost"":0,""sku"":null},""128671126"":{""id"":128671126,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_yellow"",""cost"":0,""sku"":null},""128671145"":{""id"":128671145,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_green"",""cost"":0,""sku"":null},""128671146"":{""id"":128671146,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_blue"",""cost"":0,""sku"":null},""128671147"":{""id"":128671147,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_orange"",""cost"":0,""sku"":null},""128671148"":{""id"":128671148,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_red"",""cost"":0,""sku"":null},""128671149"":{""id"":128671149,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_purple"",""cost"":0,""sku"":null},""128671150"":{""id"":128671150,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_yellow"",""cost"":0,""sku"":null},""128671749"":{""id"":128671749,""category"":""paintjob"",""name"":""PaintJob_FerDeLance_Militaire_desert_Sand"",""cost"":0,""sku"":null},""128671157"":{""id"":128671157,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_green"",""cost"":0,""sku"":null},""128671158"":{""id"":128671158,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_blue"",""cost"":0,""sku"":null},""128671159"":{""id"":128671159,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_orange"",""cost"":0,""sku"":null},""128671160"":{""id"":128671160,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_red"",""cost"":0,""sku"":null},""128671161"":{""id"":128671161,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_purple"",""cost"":0,""sku"":null},""128671162"":{""id"":128671162,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_yellow"",""cost"":0,""sku"":null},""128671163"":{""id"":128671163,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_green"",""cost"":0,""sku"":null},""128671164"":{""id"":128671164,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_blue"",""cost"":0,""sku"":null},""128671165"":{""id"":128671165,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_orange"",""cost"":0,""sku"":null},""128671166"":{""id"":128671166,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_red"",""cost"":0,""sku"":null},""128671167"":{""id"":128671167,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_purple"",""cost"":0,""sku"":null},""128671168"":{""id"":128671168,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_yellow"",""cost"":0,""sku"":null},""128671169"":{""id"":128671169,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_green"",""cost"":0,""sku"":null},""128671170"":{""id"":128671170,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_blue"",""cost"":0,""sku"":null},""128671171"":{""id"":128671171,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_orange"",""cost"":0,""sku"":null},""128671172"":{""id"":128671172,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_red"",""cost"":0,""sku"":null},""128671173"":{""id"":128671173,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_purple"",""cost"":0,""sku"":null},""128671174"":{""id"":128671174,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_yellow"",""cost"":0,""sku"":null},""128671187"":{""id"":128671187,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_green"",""cost"":0,""sku"":null},""128671188"":{""id"":128671188,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_blue"",""cost"":0,""sku"":null},""128671189"":{""id"":128671189,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_orange"",""cost"":0,""sku"":null},""128671190"":{""id"":128671190,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_red"",""cost"":0,""sku"":null},""128671191"":{""id"":128671191,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_purple"",""cost"":0,""sku"":null},""128671192"":{""id"":128671192,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_yellow"",""cost"":0,""sku"":null},""128671193"":{""id"":128671193,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_green"",""cost"":0,""sku"":null},""128671194"":{""id"":128671194,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_blue"",""cost"":0,""sku"":null},""128671195"":{""id"":128671195,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_orange"",""cost"":0,""sku"":null},""128671196"":{""id"":128671196,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_red"",""cost"":0,""sku"":null},""128671197"":{""id"":128671197,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_purple"",""cost"":0,""sku"":null},""128671198"":{""id"":128671198,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_yellow"",""cost"":0,""sku"":null},""128671199"":{""id"":128671199,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_green"",""cost"":0,""sku"":null},""128671200"":{""id"":128671200,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_blue"",""cost"":0,""sku"":null},""128671201"":{""id"":128671201,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_orange"",""cost"":0,""sku"":null},""128671202"":{""id"":128671202,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_red"",""cost"":0,""sku"":null},""128671203"":{""id"":128671203,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_purple"",""cost"":0,""sku"":null},""128671204"":{""id"":128671204,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_yellow"",""cost"":0,""sku"":null},""128671211"":{""id"":128671211,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_green"",""cost"":0,""sku"":null},""128671212"":{""id"":128671212,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_blue"",""cost"":0,""sku"":null},""128671213"":{""id"":128671213,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_orange"",""cost"":0,""sku"":null},""128671214"":{""id"":128671214,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_red"",""cost"":0,""sku"":null},""128671215"":{""id"":128671215,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_purple"",""cost"":0,""sku"":null},""128671216"":{""id"":128671216,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_yellow"",""cost"":0,""sku"":null},""128667736"":{""id"":128667736,""category"":""decal"",""name"":""Decal_Combat_Mostly_Harmless"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_COMBAT_DECAL_1001""},""128667737"":{""id"":128667737,""category"":""decal"",""name"":""Decal_Combat_Novice"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_COMBAT_DECAL_1002""},""128667738"":{""id"":128667738,""category"":""decal"",""name"":""Decal_Combat_Competent"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_COMBAT_DECAL_1003""},""128667744"":{""id"":128667744,""category"":""decal"",""name"":""Decal_Trade_Mostly_Penniless"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1001""},""128667745"":{""id"":128667745,""category"":""decal"",""name"":""Decal_Trade_Peddler"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1002""},""128667746"":{""id"":128667746,""category"":""decal"",""name"":""Decal_Trade_Dealer"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1003""},""128667747"":{""id"":128667747,""category"":""decal"",""name"":""Decal_Trade_Merchant"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1004""},""128667748"":{""id"":128667748,""category"":""decal"",""name"":""Decal_Trade_Broker"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1005""},""128667749"":{""id"":128667749,""category"":""decal"",""name"":""Decal_Trade_Entrepeneur"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1006""},""128667750"":{""id"":128667750,""category"":""decal"",""name"":""Decal_Trade_Tycoon"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1007""},""128667752"":{""id"":128667752,""category"":""decal"",""name"":""Decal_Explorer_Mostly_Aimless"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_EXPLORE_DECAL_1001""},""128667753"":{""id"":128667753,""category"":""decal"",""name"":""Decal_Explorer_Scout"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_EXPLORE_DECAL_1002""},""128667754"":{""id"":128667754,""category"":""decal"",""name"":""Decal_Explorer_Surveyor"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_EXPLORE_DECAL_1003""},""128667755"":{""id"":128667755,""category"":""decal"",""name"":""Decal_Explorer_Trailblazer"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_EXPLORE_DECAL_1004""},""128667756"":{""id"":128667756,""category"":""decal"",""name"":""Decal_Explorer_Pathfinder"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_EXPLORE_DECAL_1005""},""128671323"":{""id"":128671323,""category"":""powerplay"",""name"":""Int_ShieldGenerator_Size1_Class5_Strong"",""cost"":132195,""sku"":""ELITE_SPECIFIC_V_POWER_100000""},""128671324"":{""id"":128671324,""category"":""powerplay"",""name"":""Int_ShieldGenerator_Size2_Class5_Strong"",""cost"":240336,""sku"":""ELITE_SPECIFIC_V_POWER_100000""},""128671325"":{""id"":128671325,""category"":""powerplay"",""name"":""Int_ShieldGenerator_Size3_Class5_Strong"",""cost"":761868,""sku"":""ELITE_SPECIFIC_V_POWER_100000""},""128671326"":{""id"":128671326,""category"":""powerplay"",""name"":""Int_ShieldGenerator_Size4_Class5_Strong"",""cost"":2415120,""sku"":""ELITE_SPECIFIC_V_POWER_100000""},""128671327"":{""id"":128671327,""category"":""powerplay"",""name"":""Int_ShieldGenerator_Size5_Class5_Strong"",""cost"":7655930,""sku"":""ELITE_SPECIFIC_V_POWER_100000""},""128671328"":{""id"":128671328,""category"":""powerplay"",""name"":""Int_ShieldGenerator_Size6_Class5_Strong"",""cost"":24269297,""sku"":""ELITE_SPECIFIC_V_POWER_100000""},""128671329"":{""id"":128671329,""category"":""powerplay"",""name"":""Int_ShieldGenerator_Size7_Class5_Strong"",""cost"":76933668,""sku"":""ELITE_SPECIFIC_V_POWER_100000""},""128671330"":{""id"":128671330,""category"":""powerplay"",""name"":""Int_ShieldGenerator_Size8_Class5_Strong"",""cost"":243879729,""sku"":""ELITE_SPECIFIC_V_POWER_100000""}}},""ship"":{""name"":""Cutter"",""modules"":{""MediumHardpoint2"":{""module"":{""id"":128049389,""name"":""Hpt_PulseLaser_Turret_Medium"",""value"":112880,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":1,""hopper"":1}}},""MediumHardpoint1"":{""module"":{""id"":128049389,""name"":""Hpt_PulseLaser_Turret_Medium"",""value"":112880,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":1,""hopper"":1}}},""MediumHardpoint4"":{""module"":{""id"":128049463,""name"":""Hpt_MultiCannon_Turret_Medium"",""value"":1098880,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":90,""hopper"":2100}}},""MediumHardpoint3"":{""module"":{""id"":128049463,""name"":""Hpt_MultiCannon_Turret_Medium"",""value"":1098880,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":90,""hopper"":2100}}},""LargeHardpoint2"":{""module"":{""id"":128049390,""name"":""Hpt_PulseLaser_Turret_Large"",""value"":340340,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":1,""hopper"":1}}},""LargeHardpoint1"":{""module"":{""id"":128049390,""name"":""Hpt_PulseLaser_Turret_Large"",""value"":340340,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":1,""hopper"":1}}},""HugeHardpoint1"":{""module"":{""id"":128681994,""name"":""Hpt_BeamLaser_Gimbal_Huge"",""value"":7434236,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""TinyHardpoint1"":{""module"":{""id"":128668536,""name"":""Hpt_ShieldBooster_Size0_Class5"",""value"":238850,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0},""recipeValue"":0,""recipeName"":""ShieldBooster_Thermic"",""recipeLevel"":1,""modifiers"":{""id"":2839,""engineerID"":300100,""recipeID"":128673795,""slotIndex"":35,""moduleTags"":[22],""modifiers"":[{""name"":""mod_defencemodifier_shield_thermic_mult"",""value"":-0.049490001052618,""type"":1},{""name"":""mod_defencemodifier_shield_explosive_mult"",""value"":0.020628247410059,""type"":1},{""name"":""mod_defencemodifier_shield_kinetic_mult"",""value"":0.021059958264232,""type"":1}]}}},""TinyHardpoint2"":{""module"":{""id"":128668536,""name"":""Hpt_ShieldBooster_Size0_Class5"",""value"":238850,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0},""recipeValue"":0,""recipeName"":""ShieldBooster_Resistive"",""recipeLevel"":1,""modifiers"":{""id"":2841,""engineerID"":300100,""recipeID"":128673790,""slotIndex"":36,""moduleTags"":[22],""modifiers"":[{""name"":""mod_passive_power"",""value"":0.048395585268736,""type"":1},{""name"":""mod_defencemodifier_global_shield_mult"",""value"":-0.013773267157376,""type"":1},{""name"":""mod_health"",""value"":-0.02138064801693,""type"":1}]}}},""TinyHardpoint3"":{""module"":{""id"":128668536,""name"":""Hpt_ShieldBooster_Size0_Class5"",""value"":238850,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0},""recipeValue"":0,""recipeName"":""ShieldBooster_Resistive"",""recipeLevel"":1,""modifiers"":{""id"":2843,""engineerID"":300100,""recipeID"":128673790,""slotIndex"":37,""moduleTags"":[22],""modifiers"":[{""name"":""mod_passive_power"",""value"":0.016008185222745,""type"":1},{""name"":""mod_defencemodifier_global_shield_mult"",""value"":-0.010448658838868,""type"":1},{""name"":""mod_health"",""value"":-0.014675753191113,""type"":1}]}}},""TinyHardpoint4"":{""module"":{""id"":128662526,""name"":""Hpt_CloudScanner_Size0_Class2"",""value"":34539,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":2,""ammo"":{""clip"":0,""hopper"":0}}},""TinyHardpoint5"":{""module"":{""id"":128049516,""name"":""Hpt_ElectronicCountermeasure_Tiny"",""value"":10625,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""TinyHardpoint6"":{""module"":{""id"":128049519,""name"":""Hpt_HeatSinkLauncher_Turret_Tiny"",""value"":2975,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":1,""hopper"":2}}},""TinyHardpoint7"":{""module"":{""id"":128049513,""name"":""Hpt_ChaffLauncher_Tiny"",""value"":7225,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":1,""hopper"":10}}},""TinyHardpoint8"":{""module"":{""id"":128049513,""name"":""Hpt_ChaffLauncher_Tiny"",""value"":7225,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":1,""hopper"":10}}},""Decal1"":{""module"":{""id"":128667750,""name"":""Decal_Trade_Tycoon"",""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Decal2"":{""module"":{""id"":128667750,""name"":""Decal_Trade_Tycoon"",""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Decal3"":{""module"":{""id"":128667750,""name"":""Decal_Trade_Tycoon"",""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""PaintJob"":[],""Armour"":{""module"":{""name"":""Cutter_Armour_Grade1"",""id"":128049376,""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1}},""PowerPlant"":{""module"":{""id"":128064067,""name"":""Int_Powerplant_Size8_Class5"",""value"":138198514,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0},""recipeValue"":0,""recipeName"":""PowerPlant_Armoured"",""recipeLevel"":1,""modifiers"":{""id"":2838,""engineerID"":300100,""recipeID"":128673760,""slotIndex"":50,""moduleTags"":[18],""modifiers"":[{""name"":""mod_health"",""value"":0.12365217506886,""type"":1},{""name"":""mod_mass"",""value"":0.0067569278180599,""type"":1},{""name"":""mod_powerplant_heat"",""value"":-0.022506788372993,""type"":1}]}}},""MainEngines"":{""module"":{""id"":128064102,""name"":""Int_Engine_Size8_Class5"",""value"":138198514,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0},""recipeValue"":0,""recipeName"":""Engine_Reinforced"",""recipeLevel"":1,""modifiers"":{""id"":2826,""engineerID"":300100,""recipeID"":128673660,""slotIndex"":51,""moduleTags"":[17],""modifiers"":[{""name"":""mod_mass"",""value"":0,""type"":1},{""name"":""mod_health"",""value"":0.18770506978035,""type"":1},{""name"":""mod_engine_heat"",""value"":-0.089846849441528,""type"":1},{""name"":""mod_engine_mass_curve_multiplier"",""value"":-0.016663840040565,""type"":1}]}}},""FrameShiftDrive"":{""module"":{""id"":128064132,""name"":""Int_Hyperdrive_Size7_Class5"",""value"":43595746,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0},""recipeValue"":0,""recipeName"":""FSD_LongRange"",""recipeLevel"":1,""modifiers"":{""id"":2829,""engineerID"":300100,""recipeID"":128673690,""slotIndex"":52,""moduleTags"":[16],""modifiers"":[{""name"":""mod_mass"",""value"":0.075771875679493,""type"":1},{""name"":""mod_health"",""value"":-0.028150219470263,""type"":1},{""name"":""mod_passive_power"",""value"":0.0045042973943055,""type"":1},{""name"":""mod_fsd_optimised_mass"",""value"":0.034270532429218,""type"":1}]}}},""LifeSupport"":{""module"":{""id"":128064169,""name"":""Int_LifeSupport_Size7_Class2"",""value"":529417,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""PowerDistributor"":{""module"":{""id"":128064212,""name"":""Int_PowerDistributor_Size7_Class5"",""value"":9731925,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""Radar"":{""module"":{""id"":128064249,""name"":""Int_Sensors_Size7_Class2"",""value"":529417,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""FuelTank"":{""module"":{""id"":128064351,""name"":""Int_FuelTank_Size6_Class3"",""value"":341577,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Slot01_Size8"":{""module"":{""id"":128064345,""name"":""Int_CargoRack_Size8_Class1"",""value"":3255387,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Slot02_Size8"":{""module"":{""id"":128064345,""name"":""Int_CargoRack_Size8_Class1"",""value"":3829866,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Slot03_Size6"":{""module"":{""id"":128671328,""name"":""Int_ShieldGenerator_Size6_Class5_Strong"",""value"":20628903,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""Slot04_Size6"":{""module"":{""id"":128064343,""name"":""Int_CargoRack_Size6_Class1"",""value"":308203,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Slot05_Size6"":{""module"":{""id"":128666681,""name"":""Int_FuelScoop_Size6_Class5"",""value"":28763610,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":2,""ammo"":{""clip"":0,""hopper"":0}}},""Slot06_Size5"":{""module"":{""id"":128671243,""name"":""Int_DroneControl_Collection_Size5_Class5"",""value"":660960,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Slot07_Size5"":{""module"":{""id"":128663561,""name"":""Int_StellarBodyDiscoveryScanner_Advanced"",""value"":1545000,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Slot08_Size4"":{""module"":{""id"":128666634,""name"":""Int_DetailedSurfaceScanner_Tiny"",""value"":250000,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Slot09_Size3"":{""module"":{""id"":128049549,""name"":""Int_DockingComputer_Standard"",""value"":4500,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":2,""ammo"":{""clip"":0,""hopper"":0}}},""PlanetaryApproachSuite"":{""module"":{""name"":""Int_PlanetApproachSuite"",""id"":128672317,""value"":449,""unloaned"":449,""free"":false,""health"":1000000,""on"":true,""priority"":1}},""Bobble01"":[],""Bobble02"":[],""Bobble03"":[],""Bobble04"":[],""Bobble05"":[],""Bobble06"":[],""Bobble07"":[],""Bobble08"":[],""Bobble09"":[],""Bobble10"":[]},""value"":{""hull"":179933774,""modules"":401689563,""cargo"":111552,""total"":581734889,""unloaned"":449},""free"":false,""alive"":true,""health"":{""hull"":1000000,""shield"":1000000,""shieldup"":true,""integrity"":0,""paintwork"":0},""wear"":{""dirt"":621,""fade"":380,""tear"":23,""game"":1024},""cockpitBreached"":false,""oxygenRemaining"":450000,""fuel"":{""main"":{""capacity"":64,""level"":45.479351},""reserve"":{""capacity"":1.16,""level"":0.24177764}},""cargo"":{""capacity"":576,""qty"":576,""items"":[{""commodity"":""drones"",""origin"":3223470848,""powerplayOrigin"":null,""masq"":null,""owner"":1943695,""mission"":null,""qty"":20,""value"":2020,""xyz"":{""x"":50000.53125,""y"":40884.8125,""z"":24017.71875},""marked"":0},{""commodity"":""basicmedicines"",""origin"":3229756160,""powerplayOrigin"":null,""masq"":null,""owner"":1943695,""mission"":null,""qty"":556,""value"":109532,""xyz"":{""x"":50107.625,""y"":40984.1875,""z"":24057.71875},""marked"":0}],""lock"":533553633,""ts"":{""sec"":1466355486,""usec"":507000}},""passengers"":[],""refinery"":null,""id"":7},""ships"":{""1"":{""name"":""Asp"",""alive"":true,""station"":{""id"":3223470848,""name"":""Hiyya Orbital""},""starsystem"":{""id"":""9467315627393"",""name"":""Arjung"",""systemaddress"":""9467315627393""},""id"":1},""2"":{""name"":""SideWinder"",""alive"":true,""station"":{""id"":3223474944,""name"":""Struzan Enterprise""},""starsystem"":{""id"":""671222605185"",""name"":""Zorya Nong"",""systemaddress"":""671222605185""},""id"":2},""4"":{""name"":""Empire_Trader"",""alive"":true,""station"":{""id"":3223470848,""name"":""Hiyya Orbital""},""starsystem"":{""id"":""9467315627393"",""name"":""Arjung"",""systemaddress"":""9467315627393""},""id"":4},""7"":{""name"":""Cutter"",""alive"":true,""id"":7},""8"":{""name"":""Federation_Corvette"",""alive"":true,""station"":{""id"":3223470848,""name"":""Hiyya Orbital""},""starsystem"":{""id"":""9467315627393"",""name"":""Arjung"",""systemaddress"":""9467315627393""},""id"":8},""10"":{""name"":""SideWinder"",""alive"":true,""station"":{""id"":3222981888,""name"":""Lee Dock""},""starsystem"":{""id"":""9468120802745"",""name"":""Urvane"",""systemaddress"":""9468120802745""},""id"":10},""11"":{""name"":""Vulture"",""alive"":true,""station"":{""id"":3223470848,""name"":""Hiyya Orbital""},""starsystem"":{""id"":""9467315627393"",""name"":""Arjung"",""systemaddress"":""9467315627393""},""id"":11}}}";
            Profile profile = CompanionAppService.ProfileFromJson(data);
            Assert.IsNotNull(profile);
        }

        [TestMethod]
        public void TestCommanderFromProfile6()
        {
            string data = @"{""commander"":{""id"":1,""name"":""TestForShipyard"",""credits"":2,""debt"":0,""currentShipId"":7,""alive"":true,""docked"":false,""rank"":{""combat"":3,""trade"":7,""explore"":5,""crime"":0,""service"":0,""empire"":14,""federation"":14,""power"":4,""cqc"":0}},""lastSystem"":{""id"":""5379983297336"",""name"":""Col 285 Sector AF-A a30-0""},""lastStarport"":{""id"":""3229756160"",""name"":""Garay Terminal"",""faction"":""Federation"",""commodities"":[{""id"":""128049204"",""name"":""Explosives"",""cost_min"":247.91608695652,""cost_max"":498.08391304348,""cost_mean"":""261.00"",""homebuy"":""59"",""homesell"":""55"",""consumebuy"":""4"",""baseCreationQty"":7.4,""baseConsumptionQty"":0,""capacity"":9856.99,""buyPrice"":183,""sellPrice"":169,""meanPrice"":261,""demandBracket"":0,""stockBracket"":2,""creationQty"":6710,""consumptionQty"":0,""targetStock"":6710,""stock"":6616.539,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.1400"",""sec_illegal_min"":""1.21"",""sec_illegal_max"":""2.69"",""stolenmod"":""0.7500""},{""id"":""128049202"",""name"":""Hydrogen Fuel"",""cost_min"":110.82434782609,""cost_max"":""164.00"",""cost_mean"":""110.00"",""homebuy"":""73"",""homesell"":""70"",""consumebuy"":""3"",""baseCreationQty"":200,""baseConsumptionQty"":200,""capacity"":677318.764,""buyPrice"":97,""sellPrice"":92,""meanPrice"":110,""demandBracket"":0,""stockBracket"":2,""creationQty"":362688,""consumptionQty"":91279,""targetStock"":385507,""stock"":404403.96,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.3000"",""sec_illegal_min"":""1.13"",""sec_illegal_max"":""2.10"",""stolenmod"":""0.7500""},{""id"":""128673850"",""name"":""Hydrogen Peroxide"",""cost_min"":""589.00"",""cost_max"":860.65586956522,""cost_mean"":""917.00"",""homebuy"":""70"",""homesell"":""67"",""consumebuy"":""3"",""baseCreationQty"":0,""baseConsumptionQty"":812.6,""capacity"":2210431.5,""buyPrice"":0,""sellPrice"":836,""meanPrice"":917,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":1473621,""targetStock"":368404,""stock"":0,""demand"":1547304.3,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.2700"",""sec_illegal_min"":""1.14"",""sec_illegal_max"":""2.19"",""stolenmod"":""0.7500""},{""id"":""128673851"",""name"":""Liquid Oxygen"",""cost_min"":""223.00"",""cost_max"":400.96065217391,""cost_mean"":""263.00"",""homebuy"":""51"",""homesell"":""46"",""consumebuy"":""5"",""baseCreationQty"":10.6,""baseConsumptionQty"":421.6,""capacity"":1159890.797,""buyPrice"":0,""sellPrice"":378,""meanPrice"":263,""demandBracket"":2,""stockBracket"":0,""creationQty"":14417,""consumptionQty"":764556,""targetStock"":205556,""stock"":0,""demand"":787276.279,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.0700"",""sec_illegal_min"":""1.25"",""sec_illegal_max"":""3.04"",""stolenmod"":""0.7500""},{""id"":""128049203"",""name"":""Mineral Oil"",""cost_min"":146.05913043478,""cost_max"":364.94086956522,""cost_mean"":""181.00"",""homebuy"":""46"",""homesell"":""41"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":194.2,""capacity"":509262.402,""buyPrice"":0,""sellPrice"":147,""meanPrice"":181,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":352187,""targetStock"":88046,""stock"":0,""demand"":12789.11575,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.0300"",""sec_illegal_min"":""1.21"",""sec_illegal_max"":""2.69"",""stolenmod"":""0.7500""},{""id"":""128672305"",""name"":""Surface Stabilisers"",""cost_min"":460.23369565217,""cost_max"":782.76630434783,""cost_mean"":""467.00"",""homebuy"":""69"",""homesell"":""66"",""consumebuy"":""3"",""baseCreationQty"":41.8,""baseConsumptionQty"":0,""capacity"":84766.332,""buyPrice"":321,""sellPrice"":304,""meanPrice"":467,""demandBracket"":0,""stockBracket"":3,""creationQty"":56852,""consumptionQty"":0,""targetStock"":56852,""stock"":96170.642998,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.2500"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128049166"",""name"":""Water"",""cost_min"":""124.00"",""cost_max"":287.06956521739,""cost_mean"":""120.00"",""homebuy"":""18"",""homesell"":""10"",""consumebuy"":""8"",""baseCreationQty"":0,""baseConsumptionQty"":163,""capacity"":419449.305,""buyPrice"":0,""sellPrice"":124,""meanPrice"":120,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":295595,""targetStock"":73898,""stock"":0,""demand"":10678.6885,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Chemicals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128049241"",""name"":""Clothing"",""cost_min"":262.20260869565,""cost_max"":""463.00"",""cost_mean"":""285.00"",""homebuy"":""60"",""homesell"":""56"",""consumebuy"":""4"",""baseCreationQty"":28,""baseConsumptionQty"":70,""capacity"":66294.855,""buyPrice"":0,""sellPrice"":263,""meanPrice"":285,""demandBracket"":1,""stockBracket"":0,""creationQty"":12695,""consumptionQty"":31948,""targetStock"":20682,""stock"":0,""demand"":0,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Consumer Items"",""volumescale"":""1.1500"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.65"",""stolenmod"":""0.7500""},{""id"":""128049240"",""name"":""Consumer Technology"",""cost_min"":""6561.00"",""cost_max"":7364.7391304348,""cost_mean"":""6769.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":27,""capacity"":32570.386,""buyPrice"":0,""sellPrice"":7292,""meanPrice"":6769,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":21874,""targetStock"":5467,""stock"":0,""demand"":22800.857,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Consumer Items"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.11"",""stolenmod"":""0.7500""},{""id"":""128049238"",""name"":""Domestic Appliances"",""cost_min"":460.23369565217,""cost_max"":""716.00"",""cost_mean"":""487.00"",""homebuy"":""69"",""homesell"":""66"",""consumebuy"":""3"",""baseCreationQty"":16.8,""baseConsumptionQty"":41.8,""capacity"":26764.941,""buyPrice"":0,""sellPrice"":461,""meanPrice"":487,""demandBracket"":1,""stockBracket"":0,""creationQty"":7617,""consumptionQty"":10334,""targetStock"":10200,""stock"":0,""demand"":0,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Consumer Items"",""volumescale"":""1.2500"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128682048"",""name"":""Survival Equipment"",""cost_min"":920.46739130435,""cost_max"":1432,""cost_mean"":""485.00"",""homebuy"":""69"",""homesell"":""66"",""consumebuy"":""3"",""baseCreationQty"":9.6,""baseConsumptionQty"":0,""capacity"":6507.735,""buyPrice"":642,""sellPrice"":608,""meanPrice"":485,""demandBracket"":0,""stockBracket"":3,""creationQty"":4353,""consumptionQty"":0,""targetStock"":4353,""stock"":7770.9832395,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Consumer Items"",""volumescale"":""1.2500"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128049177"",""name"":""Algae"",""cost_min"":""135.00"",""cost_max"":308.16173913043,""cost_mean"":""137.00"",""homebuy"":""27"",""homesell"":""20"",""consumebuy"":""7"",""baseCreationQty"":0,""baseConsumptionQty"":1008,""capacity"":645001.588,""buyPrice"":0,""sellPrice"":135,""meanPrice"":137,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":440876,""targetStock"":110219,""stock"":0,""demand"":15981.755,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.06"",""sec_illegal_max"":""1.48"",""stolenmod"":""0.7500""},{""id"":""128049182"",""name"":""Animalmeat"",""cost_min"":""1286.00"",""cost_max"":1695.1776086957,""cost_mean"":""1292.00"",""homebuy"":""80"",""homesell"":""78"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":97,""capacity"":62245.84,""buyPrice"":0,""sellPrice"":1286,""meanPrice"":1292,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":42058,""targetStock"":10514,""stock"":0,""demand"":1556.368,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.4000"",""sec_illegal_min"":""1.10"",""sec_illegal_max"":""1.81"",""stolenmod"":""0.7500""},{""id"":""128049189"",""name"":""Coffee"",""cost_min"":""1286.00"",""cost_max"":1695.1776086957,""cost_mean"":""1279.00"",""homebuy"":""80"",""homesell"":""78"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":97,""capacity"":17745.2,""buyPrice"":0,""sellPrice"":1286,""meanPrice"":1279,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":11990,""targetStock"":2997,""stock"":0,""demand"":443.852,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.4000"",""sec_illegal_min"":""1.10"",""sec_illegal_max"":""1.81"",""stolenmod"":""0.7500""},{""id"":""128049183"",""name"":""Fish"",""cost_min"":""403.00"",""cost_max"":627.93,""cost_mean"":""406.00"",""homebuy"":""64"",""homesell"":""60"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":271,""capacity"":178114.384,""buyPrice"":0,""sellPrice"":403,""meanPrice"":406,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":120592,""targetStock"":30147,""stock"":0,""demand"":4394.51425,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.2000"",""sec_illegal_min"":""1.18"",""sec_illegal_max"":""2.44"",""stolenmod"":""0.7500""},{""id"":""128049184"",""name"":""Food Cartridges"",""cost_min"":95.201739130435,""cost_max"":""267.00"",""cost_mean"":""105.00"",""homebuy"":""30"",""homesell"":""23"",""consumebuy"":""7"",""baseCreationQty"":36,""baseConsumptionQty"":90.4,""capacity"":99945.285,""buyPrice"":29,""sellPrice"":22,""meanPrice"":105,""demandBracket"":0,""stockBracket"":3,""creationQty"":65284,""consumptionQty"":2201,""targetStock"":65834,""stock"":107699.90587661,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128049178"",""name"":""Fruit And Vegetables"",""cost_min"":""315.00"",""cost_max"":515.79739130435,""cost_mean"":""312.00"",""homebuy"":""60"",""homesell"":""56"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":350,""capacity"":63337.032,""buyPrice"":0,""sellPrice"":315,""meanPrice"":312,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":43263,""targetStock"":10815,""stock"":0,""demand"":1611.876,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.1500"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.65"",""stolenmod"":""0.7500""},{""id"":""128049180"",""name"":""Grain"",""cost_min"":""207.00"",""cost_max"":380.8852173913,""cost_mean"":""210.00"",""homebuy"":""48"",""homesell"":""43"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":584,""capacity"":390205.776,""buyPrice"":0,""sellPrice"":207,""meanPrice"":210,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":266534,""targetStock"":66632,""stock"":0,""demand"":9929.05,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.0500"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.65"",""stolenmod"":""0.7500""},{""id"":""128049185"",""name"":""Synthetic Meat"",""cost_min"":""252.00"",""cost_max"":436.51652173913,""cost_mean"":""271.00"",""homebuy"":""54"",""homesell"":""49"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":226,""capacity"":41654.067,""buyPrice"":0,""sellPrice"":420,""meanPrice"":271,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":27937,""targetStock"":6984,""stock"":0,""demand"":29159.691,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.23"",""sec_illegal_max"":""2.88"",""stolenmod"":""0.7500""},{""id"":""128049188"",""name"":""Tea"",""cost_min"":""1459.00"",""cost_max"":1896.6086956522,""cost_mean"":""1467.00"",""homebuy"":""81"",""homesell"":""79"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":88,""capacity"":57042.18,""buyPrice"":0,""sellPrice"":1459,""meanPrice"":1467,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":38490,""targetStock"":9622,""stock"":0,""demand"":1438.786,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Foods"",""volumescale"":""1.4200"",""sec_illegal_min"":""1.09"",""sec_illegal_max"":""1.76"",""stolenmod"":""0.7500""},{""id"":""128673856"",""name"":""C M M Composite"",""cost_min"":""2966.00"",""cost_max"":3605.947826087,""cost_mean"":""3132.00"",""homebuy"":""87"",""homesell"":""86"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":60,""capacity"":163218,""buyPrice"":0,""sellPrice"":3548,""meanPrice"":3132,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":108812,""targetStock"":27203,""stock"":0,""demand"":114252.6,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.3400"",""sec_illegal_min"":""1.05"",""sec_illegal_max"":""1.37"",""stolenmod"":""0.7500""},{""id"":""128672302"",""name"":""Ceramic Composites"",""cost_min"":""192.00"",""cost_max"":364.94086956522,""cost_mean"":""232.00"",""homebuy"":""46"",""homesell"":""41"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":776.8,""capacity"":2056769.16,""buyPrice"":0,""sellPrice"":350,""meanPrice"":232,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":1408746,""targetStock"":352186,""stock"":0,""demand"":1440091.474,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.0300"",""sec_illegal_min"":""1.28"",""sec_illegal_max"":""3.28"",""stolenmod"":""0.7500""},{""id"":""128673855"",""name"":""Insulating Membrane"",""cost_min"":""7498.00"",""cost_max"":8450.2097826087,""cost_mean"":""7837.00"",""homebuy"":""93"",""homesell"":""92"",""consumebuy"":""1"",""baseCreationQty"":0.4,""baseConsumptionQty"":28.8,""capacity"":79162.5,""buyPrice"":0,""sellPrice"":8344,""meanPrice"":7837,""demandBracket"":2,""stockBracket"":0,""creationQty"":545,""consumptionQty"":52230,""targetStock"":13602,""stock"":0,""demand"":54482.7,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.0600"",""sec_illegal_min"":""1.02"",""sec_illegal_max"":""1.15"",""stolenmod"":""0.7500""},{""id"":""128049197"",""name"":""Polymers"",""cost_min"":""152.00"",""cost_max"":320.85565217391,""cost_mean"":""171.00"",""homebuy"":""35"",""homesell"":""29"",""consumebuy"":""7"",""baseCreationQty"":19.6,""baseConsumptionQty"":1170.4,""capacity"":3135693.013,""buyPrice"":0,""sellPrice"":294,""meanPrice"":171,""demandBracket"":2,""stockBracket"":0,""creationQty"":26658,""consumptionQty"":2122549,""targetStock"":557295,""stock"":0,""demand"":2067920.956,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.23"",""sec_illegal_max"":""2.88"",""stolenmod"":""0.7500""},{""id"":""128049199"",""name"":""Semiconductors"",""cost_min"":""889.00"",""cost_max"":1280.847826087,""cost_mean"":""967.00"",""homebuy"":""76"",""homesell"":""74"",""consumebuy"":""2"",""baseCreationQty"":26.4,""baseConsumptionQty"":158.4,""capacity"":483139.15,""buyPrice"":0,""sellPrice"":1155,""meanPrice"":967,""demandBracket"":2,""stockBracket"":0,""creationQty"":35907,""consumptionQty"":287263,""targetStock"":107722,""stock"":0,""demand"":276772.022,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.3400"",""sec_illegal_min"":""1.12"",""sec_illegal_max"":""1.98"",""stolenmod"":""0.7500""},{""id"":""128049200"",""name"":""Superconductors"",""cost_min"":""6561.00"",""cost_max"":7507.6260869565,""cost_mean"":""6609.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":27,""baseConsumptionQty"":32,""capacity"":141091.684,""buyPrice"":0,""sellPrice"":6561,""meanPrice"":6609,""demandBracket"":1,""stockBracket"":0,""creationQty"":36723,""consumptionQty"":58033,""targetStock"":51231,""stock"":0,""demand"":23219.377,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Industrial Materials"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.02"",""sec_illegal_max"":""1.18"",""stolenmod"":""0.7500""},{""id"":""128064028"",""name"":""Atmospheric Extractors"",""cost_min"":344.07,""cost_max"":""569.00"",""cost_mean"":""357.00"",""homebuy"":""64"",""homesell"":""60"",""consumebuy"":""4"",""baseCreationQty"":432.8,""baseConsumptionQty"":0,""capacity"":289822.848,""buyPrice"":257,""sellPrice"":238,""meanPrice"":357,""demandBracket"":0,""stockBracket"":2,""creationQty"":196224,""consumptionQty"":0,""targetStock"":196224,""stock"":194826.105,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.2000"",""sec_illegal_min"":""1.18"",""sec_illegal_max"":""2.44"",""stolenmod"":""0.7500""},{""id"":""128672309"",""name"":""Building Fabricators"",""cost_min"":975.18,""cost_max"":""1344.00"",""cost_mean"":""980.00"",""homebuy"":""78"",""homesell"":""76"",""consumebuy"":""2"",""baseCreationQty"":452.8,""baseConsumptionQty"":0,""capacity"":908988.696,""buyPrice"":769,""sellPrice"":742,""meanPrice"":980,""demandBracket"":0,""stockBracket"":3,""creationQty"":615846,""consumptionQty"":0,""targetStock"":615846,""stock"":1090987.287522,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.3700"",""sec_illegal_min"":""1.11"",""sec_illegal_max"":""1.89"",""stolenmod"":""0.7500""},{""id"":""128049222"",""name"":""Crop Harvesters"",""cost_min"":2233.0903558696,""cost_max"":2831.277,""cost_mean"":""2021.00"",""homebuy"":""84"",""homesell"":""82"",""consumebuy"":""2"",""baseCreationQty"":514.4,""baseConsumptionQty"":0,""capacity"":1030550.571,""buyPrice"":2013,""sellPrice"":1946,""meanPrice"":2021,""demandBracket"":0,""stockBracket"":2,""creationQty"":699627,""consumptionQty"":0,""targetStock"":699627,""stock"":692684.72,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.4400"",""sec_illegal_min"":""1.06"",""sec_illegal_max"":""1.46"",""stolenmod"":""0.7500""},{""id"":""128673861"",""name"":""Emergency Power Cells"",""cost_min"":""889.00"",""cost_max"":1224.7608695652,""cost_mean"":""1011.00"",""homebuy"":""76"",""homesell"":""74"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":52.8,""capacity"":107367.91,""buyPrice"":0,""sellPrice"":1195,""meanPrice"":1011,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":71818,""targetStock"":17954,""stock"":0,""demand"":75176.388,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.3400"",""sec_illegal_min"":""1.12"",""sec_illegal_max"":""1.98"",""stolenmod"":""0.7500""},{""id"":""128673866"",""name"":""Exhaust Manifold"",""cost_min"":""383.00"",""cost_max"":602.51,""cost_mean"":""479.00"",""homebuy"":""63"",""homesell"":""59"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":113.6,""capacity"":229919.808,""buyPrice"":0,""sellPrice"":583,""meanPrice"":479,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":154516,""targetStock"":38629,""stock"":0,""demand"":160928.414,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.1900"",""sec_illegal_min"":""1.18"",""sec_illegal_max"":""2.48"",""stolenmod"":""0.7500""},{""id"":""128672307"",""name"":""Geological Equipment"",""cost_min"":1647.92,""cost_max"":""2134.00"",""cost_mean"":""1661.00"",""homebuy"":""82"",""homesell"":""80"",""consumebuy"":""2"",""baseCreationQty"":6.4,""baseConsumptionQty"":0,""capacity"":12918.22,""buyPrice"":1365,""sellPrice"":1319,""meanPrice"":1661,""demandBracket"":0,""stockBracket"":3,""creationQty"":8705,""consumptionQty"":0,""targetStock"":8705,""stock"":15289.59977285,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.4500"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.28"",""stolenmod"":""0.7500""},{""id"":""128673860"",""name"":""H N Shock Mount"",""cost_min"":434,""cost_max"":""683.00"",""cost_mean"":""406.00"",""homebuy"":""68"",""homesell"":""65"",""consumebuy"":""3"",""baseCreationQty"":176,""baseConsumptionQty"":0,""capacity"":356908.125,""buyPrice"":299,""sellPrice"":283,""meanPrice"":406,""demandBracket"":0,""stockBracket"":3,""creationQty"":239375,""consumptionQty"":0,""targetStock"":239375,""stock"":428241.875,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.2400"",""sec_illegal_min"":""1.16"",""sec_illegal_max"":""2.30"",""stolenmod"":""0.7500""},{""id"":""128049220"",""name"":""Heliostatic Furnaces"",""cost_min"":152.69739130435,""cost_max"":373.30260869565,""cost_mean"":""236.00"",""homebuy"":""47"",""homesell"":""42"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":61.4,""capacity"":165578.937,""buyPrice"":0,""sellPrice"":354,""meanPrice"":236,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":111351,""targetStock"":27837,""stock"":0,""demand"":115889.892,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.0400"",""sec_illegal_min"":""1.22"",""sec_illegal_max"":""2.78"",""stolenmod"":""0.7500""},{""id"":""128049223"",""name"":""Marine Supplies"",""cost_min"":3964.9560869565,""cost_max"":""4723.00"",""cost_mean"":""3916.00"",""homebuy"":""89"",""homesell"":""88"",""consumebuy"":""1"",""baseCreationQty"":308,""baseConsumptionQty"":0,""capacity"":621656.504,""buyPrice"":3723,""sellPrice"":3645,""meanPrice"":3916,""demandBracket"":0,""stockBracket"":2,""creationQty"":418906,""consumptionQty"":0,""targetStock"":418906,""stock"":417797.666,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.2400"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.28"",""stolenmod"":""0.7500""},{""id"":""128049221"",""name"":""Mineral Extractors"",""cost_min"":659.17294782609,""cost_max"":1131.28,""cost_mean"":""443.00"",""homebuy"":""70"",""homesell"":""67"",""consumebuy"":""3"",""baseCreationQty"":302.4,""baseConsumptionQty"":0,""capacity"":610352.876,""buyPrice"":470,""sellPrice"":445,""meanPrice"":443,""demandBracket"":0,""stockBracket"":3,""creationQty"":411289,""consumptionQty"":0,""targetStock"":411289,""stock"":484948.4529178,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.2700"",""sec_illegal_min"":""1.14"",""sec_illegal_max"":""2.19"",""stolenmod"":""0.7500""},{""id"":""128049217"",""name"":""Power Generators"",""cost_min"":422.43369565217,""cost_max"":""716.00"",""cost_mean"":""458.00"",""homebuy"":""69"",""homesell"":""66"",""consumebuy"":""3"",""baseCreationQty"":16.8,""baseConsumptionQty"":41.8,""capacity"":145813.568,""buyPrice"":0,""sellPrice"":423,""meanPrice"":458,""demandBracket"":1,""stockBracket"":0,""creationQty"":22850,""consumptionQty"":75806,""targetStock"":41801,""stock"":0,""demand"":0,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.2500"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128672313"",""name"":""Skimer Components"",""cost_min"":815.06304347826,""cost_max"":""1203.00"",""cost_mean"":""859.00"",""homebuy"":""76"",""homesell"":""74"",""consumebuy"":""2"",""baseCreationQty"":40,""baseConsumptionQty"":0,""capacity"":27167.728,""buyPrice"":626,""sellPrice"":604,""meanPrice"":859,""demandBracket"":0,""stockBracket"":3,""creationQty"":18136,""consumptionQty"":0,""targetStock"":18136,""stock"":32456.0880761,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.3500"",""sec_illegal_min"":""1.11"",""sec_illegal_max"":""1.95"",""stolenmod"":""0.7500""},{""id"":""128672308"",""name"":""Thermal Cooling Units"",""cost_min"":247.91608695652,""cost_max"":""446.00"",""cost_mean"":""256.00"",""homebuy"":""59"",""homesell"":""55"",""consumebuy"":""4"",""baseCreationQty"":147.2,""baseConsumptionQty"":0,""capacity"":292699.71,""buyPrice"":148,""sellPrice"":137,""meanPrice"":256,""demandBracket"":0,""stockBracket"":3,""creationQty"":200205,""consumptionQty"":0,""targetStock"":200205,""stock"":350188.1441116,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.1400"",""sec_illegal_min"":""1.21"",""sec_illegal_max"":""2.69"",""stolenmod"":""0.7500""},{""id"":""128049218"",""name"":""Water Purifiers"",""cost_min"":247.91608695652,""cost_max"":""446.00"",""cost_mean"":""258.00"",""homebuy"":""59"",""homesell"":""55"",""consumebuy"":""4"",""baseCreationQty"":736,""baseConsumptionQty"":7.4,""capacity"":1483114.204,""buyPrice"":148,""sellPrice"":137,""meanPrice"":258,""demandBracket"":0,""stockBracket"":3,""creationQty"":1001021,""consumptionQty"":13421,""targetStock"":1004376,""stock"":1463539.2027526,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Machinery"",""volumescale"":""1.1400"",""sec_illegal_min"":""1.21"",""sec_illegal_max"":""2.69"",""stolenmod"":""0.7500""},{""id"":""128682046"",""name"":""Advanced Medicines"",""cost_min"":2272,""cost_max"":3067.2043478261,""cost_mean"":""1259.00"",""homebuy"":""78"",""homesell"":""76"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":107,""capacity"":73057.16,""buyPrice"":0,""sellPrice"":2995,""meanPrice"":1259,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":48835,""targetStock"":12208,""stock"":0,""demand"":51143.8,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Medicines"",""volumescale"":""1.3800"",""sec_illegal_min"":""1.10"",""sec_illegal_max"":""1.87"",""stolenmod"":""0.7500""},{""id"":""128049210"",""name"":""Basic Medicines"",""cost_min"":256.28260869565,""cost_max"":""463.00"",""cost_mean"":""279.00"",""homebuy"":""60"",""homesell"":""56"",""consumebuy"":""4"",""baseCreationQty"":56,""baseConsumptionQty"":98,""capacity"":43673.85,""buyPrice"":199,""sellPrice"":184,""meanPrice"":279,""demandBracket"":0,""stockBracket"":2,""creationQty"":25390,""consumptionQty"":4020,""targetStock"":26394,""stock"":26659.416,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Medicines"",""volumescale"":""1.1500"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.65"",""stolenmod"":""0.7500""},{""id"":""128049209"",""name"":""Performance Enhancers"",""cost_min"":""6561.00"",""cost_max"":7488.3913043478,""cost_mean"":""6816.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":27,""capacity"":11850.951,""buyPrice"":0,""sellPrice"":7405,""meanPrice"":6816,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":7959,""targetStock"":1989,""stock"":0,""demand"":8296.608,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Medicines"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.26"",""stolenmod"":""0.7500""},{""id"":""128049669"",""name"":""Progenitor Cells"",""cost_min"":""6561.00"",""cost_max"":7448.547826087,""cost_mean"":""6779.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":27,""capacity"":17126.478,""buyPrice"":0,""sellPrice"":7368,""meanPrice"":6779,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":11502,""targetStock"":2875,""stock"":0,""demand"":11988.853,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Medicines"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.26"",""stolenmod"":""0.7500""},{""id"":""128049176"",""name"":""Aluminium"",""cost_min"":""330.00"",""cost_max"":536.22434782609,""cost_mean"":""340.00"",""homebuy"":""61"",""homesell"":""57"",""consumebuy"":""4"",""baseCreationQty"":66.4,""baseConsumptionQty"":2258.4,""capacity"":5872726.505,""buyPrice"":0,""sellPrice"":505,""meanPrice"":340,""demandBracket"":2,""stockBracket"":0,""creationQty"":90310,""consumptionQty"":4095525,""targetStock"":1114191,""stock"":0,""demand"":3903345.761,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1600"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.60"",""stolenmod"":""0.7500""},{""id"":""128049168"",""name"":""Beryllium"",""cost_min"":""8017.00"",""cost_max"":8987.1695652174,""cost_mean"":""8288.00"",""homebuy"":""93"",""homesell"":""92"",""consumebuy"":""1"",""baseCreationQty"":4.6,""baseConsumptionQty"":92,""capacity"":258778.52,""buyPrice"":0,""sellPrice"":8826,""meanPrice"":8288,""demandBracket"":2,""stockBracket"":0,""creationQty"":6257,""consumptionQty"":166839,""targetStock"":47966,""stock"":0,""demand"":170293.058,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.0400"",""sec_illegal_min"":""1.02"",""sec_illegal_max"":""1.15"",""stolenmod"":""0.7500""},{""id"":""128049162"",""name"":""Cobalt"",""cost_min"":""701.00"",""cost_max"":997.2347826087,""cost_mean"":""647.00"",""homebuy"":""73"",""homesell"":""70"",""consumebuy"":""3"",""baseCreationQty"":32.4,""baseConsumptionQty"":129.6,""capacity"":399938.836,""buyPrice"":0,""sellPrice"":701,""meanPrice"":647,""demandBracket"":1,""stockBracket"":0,""creationQty"":44067,""consumptionQty"":235025,""targetStock"":102823,""stock"":0,""demand"":28968.322,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.3000"",""sec_illegal_min"":""1.13"",""sec_illegal_max"":""2.10"",""stolenmod"":""0.7500""},{""id"":""128049175"",""name"":""Copper"",""cost_min"":""472.00"",""cost_max"":715.58695652174,""cost_mean"":""481.00"",""homebuy"":""67"",""homesell"":""64"",""consumebuy"":""3"",""baseCreationQty"":46.4,""baseConsumptionQty"":742.4,""capacity"":2046482.196,""buyPrice"":0,""sellPrice"":654,""meanPrice"":481,""demandBracket"":2,""stockBracket"":0,""creationQty"":63108,""consumptionQty"":1346315,""targetStock"":399686,""stock"":0,""demand"":1249273.316,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.2300"",""sec_illegal_min"":""1.16"",""sec_illegal_max"":""2.33"",""stolenmod"":""0.7500""},{""id"":""128049170"",""name"":""Gallium"",""cost_min"":""5028.00"",""cost_max"":5863.447826087,""cost_mean"":""5135.00"",""homebuy"":""90"",""homesell"":""89"",""consumebuy"":""1"",""baseCreationQty"":6.6,""baseConsumptionQty"":264,""capacity"":724769.752,""buyPrice"":0,""sellPrice"":5707,""meanPrice"":5135,""demandBracket"":2,""stockBracket"":0,""creationQty"":8977,""consumptionQty"":478755,""targetStock"":128665,""stock"":0,""demand"":468608.76,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1800"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.24"",""stolenmod"":""0.7500""},{""id"":""128049154"",""name"":""Gold"",""cost_min"":""9164.00"",""cost_max"":10172.009782609,""cost_mean"":""9401.00"",""homebuy"":""94"",""homesell"":""93"",""consumebuy"":""1"",""baseCreationQty"":2,""baseConsumptionQty"":166.4,""capacity"":456860.448,""buyPrice"":0,""sellPrice"":10027,""meanPrice"":9401,""demandBracket"":2,""stockBracket"":0,""creationQty"":3627,""consumptionQty"":301761,""targetStock"":79067,""stock"":0,""demand"":306373.033,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.09"",""stolenmod"":""0.7500""},{""id"":""128049169"",""name"":""Indium"",""cost_min"":""5743.00"",""cost_max"":6617.4130434783,""cost_mean"":""5727.00"",""homebuy"":""91"",""homesell"":""90"",""consumebuy"":""1"",""baseCreationQty"":59.6,""baseConsumptionQty"":48,""capacity"":250144.704,""buyPrice"":0,""sellPrice"":5743,""meanPrice"":5727,""demandBracket"":1,""stockBracket"":0,""creationQty"":81061,""consumptionQty"":87047,""targetStock"":102822,""stock"":0,""demand"":36890.754,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1400"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.22"",""stolenmod"":""0.7500""},{""id"":""128049173"",""name"":""Lithium"",""cost_min"":""1555.00"",""cost_max"":2006.6026086957,""cost_mean"":""1596.00"",""homebuy"":""81"",""homesell"":""79"",""consumebuy"":""2"",""baseCreationQty"":16.6,""baseConsumptionQty"":266.4,""capacity"":740828.525,""buyPrice"":0,""sellPrice"":1908,""meanPrice"":1596,""demandBracket"":2,""stockBracket"":0,""creationQty"":22578,""consumptionQty"":483107,""targetStock"":143354,""stock"":0,""demand"":465935.339,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.4300"",""sec_illegal_min"":""1.09"",""sec_illegal_max"":""1.74"",""stolenmod"":""0.7500""},{""id"":""128671118"",""name"":""Osmium"",""cost_min"":8529.3,""cost_max"":9759.9139130435,""cost_mean"":""7591.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":215.2,""capacity"":585387,""buyPrice"":0,""sellPrice"":9649,""meanPrice"":7591,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":390258,""targetStock"":97564,""stock"":0,""demand"":409771.8,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.22"",""stolenmod"":""0.7500""},{""id"":""128049153"",""name"":""Palladium"",""cost_min"":""12815.00"",""cost_max"":13960.415652174,""cost_mean"":""13298.00"",""homebuy"":""96"",""homesell"":""96"",""consumebuy"":""0"",""baseCreationQty"":0,""baseConsumptionQty"":128.8,""capacity"":349895.35,""buyPrice"":0,""sellPrice"":13829,""meanPrice"":13298,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":233575,""targetStock"":58393,""stock"":0,""demand"":240327.028,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.08"",""stolenmod"":""0.7500""},{""id"":""128049152"",""name"":""Platinum"",""cost_min"":21523.2,""cost_max"":23082.087652174,""cost_mean"":""19279.00"",""homebuy"":""97"",""homesell"":""97"",""consumebuy"":""0"",""baseCreationQty"":0,""baseConsumptionQty"":9.6,""capacity"":26115,""buyPrice"":0,""sellPrice"":22941,""meanPrice"":19279,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":17410,""targetStock"":4352,""stock"":0,""demand"":18281.4,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":""0"",""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.04"",""stolenmod"":""0.7500""},{""id"":""128673845"",""name"":""Praseodymium"",""cost_min"":""6138.00"",""cost_max"":7057.7652173913,""cost_mean"":""7156.00"",""homebuy"":""92"",""homesell"":""91"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":113.6,""capacity"":309015,""buyPrice"":0,""sellPrice"":6975,""meanPrice"":7156,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":206010,""targetStock"":51502,""stock"":0,""demand"":216311.4,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1200"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.28"",""stolenmod"":""0.7500""},{""id"":""128673847"",""name"":""Samarium"",""cost_min"":""5373.00"",""cost_max"":6236.9258695652,""cost_mean"":""6330.00"",""homebuy"":""91"",""homesell"":""90"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":125.6,""capacity"":341656.5,""buyPrice"":0,""sellPrice"":6159,""meanPrice"":6330,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":227771,""targetStock"":56942,""stock"":0,""demand"":239160.9,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.1600"",""sec_illegal_min"":""1.04"",""sec_illegal_max"":""1.31"",""stolenmod"":""0.7500""},{""id"":""128049155"",""name"":""Silver"",""cost_min"":""4705.00"",""cost_max"":5513.8260869565,""cost_mean"":""4775.00"",""homebuy"":""90"",""homesell"":""89"",""consumebuy"":""1"",""baseCreationQty"":7,""baseConsumptionQty"":278.4,""capacity"":760268.42,""buyPrice"":0,""sellPrice"":5380,""meanPrice"":4775,""demandBracket"":2,""stockBracket"":0,""creationQty"":9521,""consumptionQty"":504869,""targetStock"":135738,""stock"":0,""demand"":500549.498,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.2000"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.24"",""stolenmod"":""0.7500""},{""id"":""128049171"",""name"":""Tantalum"",""cost_min"":""3858.00"",""cost_max"":4590.3091304348,""cost_mean"":""3962.00"",""homebuy"":""89"",""homesell"":""88"",""consumebuy"":""1"",""baseCreationQty"":81.2,""baseConsumptionQty"":162.4,""capacity"":600534.918,""buyPrice"":0,""sellPrice"":4169,""meanPrice"":3962,""demandBracket"":2,""stockBracket"":0,""creationQty"":110439,""consumptionQty"":294507,""targetStock"":184065,""stock"":0,""demand"":260101.018,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.2600"",""sec_illegal_min"":""1.04"",""sec_illegal_max"":""1.29"",""stolenmod"":""0.7500""},{""id"":""128049174"",""name"":""Titanium"",""cost_min"":""1004.00"",""cost_max"":1361.2576086957,""cost_mean"":""1006.00"",""homebuy"":""77"",""homesell"":""75"",""consumebuy"":""2"",""baseCreationQty"":59.6,""baseConsumptionQty"":952.8,""capacity"":2622947.05,""buyPrice"":0,""sellPrice"":1294,""meanPrice"":1006,""demandBracket"":2,""stockBracket"":0,""creationQty"":81061,""consumptionQty"":1727868,""targetStock"":513028,""stock"":0,""demand"":1693874.05,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.3600"",""sec_illegal_min"":""1.11"",""sec_illegal_max"":""1.92"",""stolenmod"":""0.7500""},{""id"":""128049172"",""name"":""Uranium"",""cost_min"":""2603.00"",""cost_max"":3199.7,""cost_mean"":""2705.00"",""homebuy"":""86"",""homesell"":""85"",""consumebuy"":""1"",""baseCreationQty"":11,""baseConsumptionQty"":662.4,""capacity"":1796325.923,""buyPrice"":0,""sellPrice"":3107,""meanPrice"":2705,""demandBracket"":2,""stockBracket"":0,""creationQty"":14961,""consumptionQty"":1201238,""targetStock"":315270,""stock"":0,""demand"":1193434.819,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Metals"",""volumescale"":""1.3800"",""sec_illegal_min"":""1.05"",""sec_illegal_max"":""1.39"",""stolenmod"":""0.7500""},{""id"":""128049165"",""name"":""Bauxite"",""cost_min"":107.14434782609,""cost_max"":320.85565217391,""cost_mean"":""120.00"",""homebuy"":""35"",""homesell"":""29"",""consumebuy"":""7"",""baseCreationQty"":0,""baseConsumptionQty"":487.6,""capacity"":1236174.51,""buyPrice"":0,""sellPrice"":108,""meanPrice"":120,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":884245,""targetStock"":221061,""stock"":0,""demand"":30617.088,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.24"",""sec_illegal_max"":""2.93"",""stolenmod"":""0.7500""},{""id"":""128049156"",""name"":""Bertrandite"",""cost_min"":2304.6704347826,""cost_max"":3015.3295652174,""cost_mean"":""2374.00"",""homebuy"":""85"",""homesell"":""84"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":116.2,""capacity"":300704.575,""buyPrice"":0,""sellPrice"":2507,""meanPrice"":2374,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":210725,""targetStock"":52681,""stock"":0,""demand"":107101.315,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.4000"",""sec_illegal_min"":""1.07"",""sec_illegal_max"":""1.57"",""stolenmod"":""0.7500""},{""id"":""128049159"",""name"":""Coltan"",""cost_min"":1264.7143478261,""cost_max"":1793.2856521739,""cost_mean"":""1319.00"",""homebuy"":""80"",""homesell"":""78"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":27.6,""capacity"":74127.012,""buyPrice"":0,""sellPrice"":1265,""meanPrice"":1319,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":50052,""targetStock"":12513,""stock"":0,""demand"":1861.30875,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.4100"",""sec_illegal_min"":""1.10"",""sec_illegal_max"":""1.79"",""stolenmod"":""0.7500""},{""id"":""128672294"",""name"":""Cryolite"",""cost_min"":2013.607173913,""cost_max"":2681.392826087,""cost_mean"":""2266.00"",""homebuy"":""84"",""homesell"":""82"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":128.6,""capacity"":344920.548,""buyPrice"":0,""sellPrice"":2621,""meanPrice"":2266,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":233212,""targetStock"":58303,""stock"":0,""demand"":241491.026,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.4400"",""sec_illegal_min"":""1.07"",""sec_illegal_max"":""1.61"",""stolenmod"":""0.7500""},{""id"":""128049158"",""name"":""Gallite"",""cost_min"":1762.6536956522,""cost_max"":2384.3463043478,""cost_mean"":""1819.00"",""homebuy"":""83"",""homesell"":""81"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":142.6,""capacity"":381693.6,""buyPrice"":0,""sellPrice"":2026,""meanPrice"":1819,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":258600,""targetStock"":64650,""stock"":0,""demand"":165135.708,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.4600"",""sec_illegal_min"":""1.08"",""sec_illegal_max"":""1.66"",""stolenmod"":""0.7500""},{""id"":""128672295"",""name"":""Goslarite"",""cost_min"":753.56260869565,""cost_max"":1162.4373913043,""cost_mean"":""916.00"",""homebuy"":""75"",""homesell"":""73"",""consumebuy"":""3"",""baseCreationQty"":0,""baseConsumptionQty"":41.6,""capacity"":112256.208,""buyPrice"":0,""sellPrice"":1126,""meanPrice"":916,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":75441,""targetStock"":18860,""stock"":0,""demand"":78572.248,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.3300"",""sec_illegal_min"":""1.12"",""sec_illegal_max"":""2.01"",""stolenmod"":""0.7500""},{""id"":""128049157"",""name"":""Indite"",""cost_min"":2013.607173913,""cost_max"":2681.392826087,""cost_mean"":""2088.00"",""homebuy"":""84"",""homesell"":""82"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":19.4,""capacity"":51823.086,""buyPrice"":0,""sellPrice"":2014,""meanPrice"":2088,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":35182,""targetStock"":8795,""stock"":0,""demand"":1284.364,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.4400"",""sec_illegal_min"":""1.07"",""sec_illegal_max"":""1.61"",""stolenmod"":""0.7500""},{""id"":""128049161"",""name"":""Lepidolite"",""cost_min"":518.34413043478,""cost_max"":860.65586956522,""cost_mean"":""544.00"",""homebuy"":""70"",""homesell"":""67"",""consumebuy"":""3"",""baseCreationQty"":0,""baseConsumptionQty"":56.6,""capacity"":150782.567,""buyPrice"":0,""sellPrice"":519,""meanPrice"":544,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":102643,""targetStock"":25660,""stock"":0,""demand"":3836.6125,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.2700"",""sec_illegal_min"":""1.14"",""sec_illegal_max"":""2.19"",""stolenmod"":""0.7500""},{""id"":""128673853"",""name"":""Lithium Hydroxide"",""cost_min"":4546.1739130435,""cost_max"":5513.8260869565,""cost_mean"":""5646.00"",""homebuy"":""90"",""homesell"":""89"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":34.8,""capacity"":94663.5,""buyPrice"":0,""sellPrice"":5426,""meanPrice"":5646,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":63109,""targetStock"":15777,""stock"":0,""demand"":66264.9,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.2000"",""sec_illegal_min"":""1.04"",""sec_illegal_max"":""1.35"",""stolenmod"":""0.7500""},{""id"":""128673848"",""name"":""Low Temperature Diamond"",""cost_min"":64800,""cost_max"":69221.762608696,""cost_mean"":""57445.00"",""homebuy"":""97"",""homesell"":""97"",""consumebuy"":""0"",""baseCreationQty"":0,""baseConsumptionQty"":5,""capacity"":13602,""buyPrice"":0,""sellPrice"":68821,""meanPrice"":57445,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":9068,""targetStock"":2266,""stock"":0,""demand"":9523.2,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.00"",""sec_illegal_max"":""1.03"",""stolenmod"":""0.7500""},{""id"":""128673854"",""name"":""Methane Clathrate"",""cost_min"":307.555,""cost_max"":579.445,""cost_mean"":""629.00"",""homebuy"":""63"",""homesell"":""59"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":90,""capacity"":244818,""buyPrice"":0,""sellPrice"":555,""meanPrice"":629,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":163212,""targetStock"":40803,""stock"":0,""demand"":171372.6,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.1800"",""sec_illegal_min"":""1.19"",""sec_illegal_max"":""2.52"",""stolenmod"":""0.7500""},{""id"":""128668550"",""name"":""Painite"",""cost_min"":63000,""cost_max"":73764.782608696,""cost_mean"":""40508.00"",""homebuy"":""100"",""homesell"":""100"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":5,""capacity"":2268,""buyPrice"":0,""sellPrice"":73765,""meanPrice"":40508,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":2268,""targetStock"":566,""stock"":0,""demand"":1985,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.01"",""sec_illegal_max"":""1.04"",""stolenmod"":""0.7500""},{""id"":""128672297"",""name"":""Pyrophyllite"",""cost_min"":1351.3913043478,""cost_max"":1896.6086956522,""cost_mean"":""1565.00"",""homebuy"":""81"",""homesell"":""79"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":35.8,""capacity"":96733.78,""buyPrice"":0,""sellPrice"":1848,""meanPrice"":1565,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":64922,""targetStock"":16230,""stock"":0,""demand"":67714.54,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.4200"",""sec_illegal_min"":""1.09"",""sec_illegal_max"":""1.76"",""stolenmod"":""0.7500""},{""id"":""128049163"",""name"":""Rutile"",""cost_min"":275.77565217391,""cost_max"":536.22434782609,""cost_mean"":""299.00"",""homebuy"":""61"",""homesell"":""57"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":166,""capacity"":430781.085,""buyPrice"":0,""sellPrice"":276,""meanPrice"":299,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":301035,""targetStock"":75258,""stock"":0,""demand"":10818.76725,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.1600"",""sec_illegal_min"":""1.20"",""sec_illegal_max"":""2.60"",""stolenmod"":""0.7500""},{""id"":""128049160"",""name"":""Uraninite"",""cost_min"":803.23913043478,""cost_max"":1224.7608695652,""cost_mean"":""836.00"",""homebuy"":""76"",""homesell"":""74"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":263.8,""capacity"":704671.416,""buyPrice"":0,""sellPrice"":1096,""meanPrice"":836,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":478392,""targetStock"":119598,""stock"":0,""demand"":409354.473,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Minerals"",""volumescale"":""1.3400"",""sec_illegal_min"":""1.12"",""sec_illegal_max"":""1.98"",""stolenmod"":""0.7500""},{""id"":""128049214"",""name"":""Beer"",""cost_min"":""175.00"",""cost_max"":343.85565217391,""cost_mean"":""186.00"",""homebuy"":""42"",""homesell"":""36"",""consumebuy"":""6"",""baseCreationQty"":0,""baseConsumptionQty"":755,""capacity"":341123.835,""buyPrice"":0,""sellPrice"":175,""meanPrice"":186,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":232215,""targetStock"":58053,""stock"":0,""demand"":8679.366,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Narcotics"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.25"",""sec_illegal_max"":""3.04"",""stolenmod"":""0.7500""},{""id"":""128049216"",""name"":""Liquor"",""cost_min"":551.91,""cost_max"":""831.00"",""cost_mean"":""587.00"",""homebuy"":""71"",""homesell"":""68"",""consumebuy"":""3"",""baseCreationQty"":14.4,""baseConsumptionQty"":35.8,""capacity"":4668.611,""buyPrice"":0,""sellPrice"":552,""meanPrice"":587,""demandBracket"":1,""stockBracket"":0,""creationQty"":144,""consumptionQty"":2983,""targetStock"":889,""stock"":0,""demand"":70.2185,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Narcotics"",""volumescale"":""1.2800"",""sec_illegal_min"":""1.14"",""sec_illegal_max"":""2.16"",""stolenmod"":""0.7500""},{""id"":""128049215"",""name"":""Wine"",""cost_min"":""252.00"",""cost_max"":436.51652173913,""cost_mean"":""260.00"",""homebuy"":""54"",""homesell"":""49"",""consumebuy"":""5"",""baseCreationQty"":0,""baseConsumptionQty"":452,""capacity"":241866.553,""buyPrice"":0,""sellPrice"":252,""meanPrice"":260,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":163313,""targetStock"":40827,""stock"":0,""demand"":6073.7575,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Narcotics"",""volumescale"":""1.1000"",""sec_illegal_min"":""1.23"",""sec_illegal_max"":""2.88"",""stolenmod"":""0.7500""},{""id"":""128066403"",""name"":""Drones"",""cost_min"":""100.00"",""cost_max"":""100.00"",""cost_mean"":""101.00"",""homebuy"":""100"",""homesell"":""100"",""consumebuy"":""1"",""baseCreationQty"":200,""baseConsumptionQty"":0,""capacity"":91279,""buyPrice"":101,""sellPrice"":100,""meanPrice"":101,""demandBracket"":0,""stockBracket"":3,""creationQty"":91279,""consumptionQty"":0,""targetStock"":91279,""stock"":109534.8,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""NonMarketable"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.00"",""sec_illegal_max"":""0.99"",""stolenmod"":""0.7500""},{""id"":""128049231"",""name"":""Advanced Catalysers"",""cost_min"":2637.6134782609,""cost_max"":3396.3865217391,""cost_mean"":""2947.00"",""homebuy"":""86"",""homesell"":""85"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":104.8,""capacity"":282996.362,""buyPrice"":0,""sellPrice"":3328,""meanPrice"":2947,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":190058,""targetStock"":47514,""stock"":0,""demand"":198088.844,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.3600"",""sec_illegal_min"":""1.05"",""sec_illegal_max"":""1.39"",""stolenmod"":""0.7500""},{""id"":""128049228"",""name"":""Auto Fabricators"",""cost_min"":3843.168,""cost_max"":4597.7635078261,""cost_mean"":""3734.00"",""homebuy"":""88"",""homesell"":""87"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":34.4,""capacity"":124772,""buyPrice"":0,""sellPrice"":4530,""meanPrice"":3734,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":62386,""targetStock"":15596,""stock"":0,""demand"":87341.6,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.2800"",""sec_illegal_min"":""1.03"",""sec_illegal_max"":""1.28"",""stolenmod"":""0.7500""},{""id"":""128049225"",""name"":""Computer Components"",""cost_min"":460.23369565217,""cost_max"":""716.00"",""cost_mean"":""513.00"",""homebuy"":""69"",""homesell"":""66"",""consumebuy"":""3"",""baseCreationQty"":167.2,""baseConsumptionQty"":0,""capacity"":112723.522,""buyPrice"":321,""sellPrice"":304,""meanPrice"":513,""demandBracket"":0,""stockBracket"":3,""creationQty"":75806,""consumptionQty"":0,""targetStock"":75806,""stock"":135237.904,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.2500"",""sec_illegal_min"":""1.15"",""sec_illegal_max"":""2.26"",""stolenmod"":""0.7500""},{""id"":""128049226"",""name"":""Hazardous Environment Suits"",""cost_min"":""274.00"",""cost_max"":465.3002173913,""cost_mean"":""340.00"",""homebuy"":""56"",""homesell"":""52"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":612,""capacity"":1661487.366,""buyPrice"":0,""sellPrice"":448,""meanPrice"":340,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":1109878,""targetStock"":277469,""stock"":0,""demand"":1163153.042,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.1200"",""sec_illegal_min"":""1.22"",""sec_illegal_max"":""2.78"",""stolenmod"":""0.7500""},{""id"":""128673873"",""name"":""Micro Controllers"",""cost_min"":""3167.00"",""cost_max"":3827.9913043478,""cost_mean"":""3274.00"",""homebuy"":""87"",""homesell"":""86"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":19.2,""capacity"":52195.18,""buyPrice"":0,""sellPrice"":3769,""meanPrice"":3274,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":34820,""targetStock"":8705,""stock"":0,""demand"":36543.59,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.3200"",""sec_illegal_min"":""1.05"",""sec_illegal_max"":""1.37"",""stolenmod"":""0.7500""},{""id"":""128049671"",""name"":""Resonating Separators"",""cost_min"":5818.9000652174,""cost_max"":6849.0919347826,""cost_mean"":""5958.00"",""homebuy"":""91"",""homesell"":""90"",""consumebuy"":""1"",""baseCreationQty"":0,""baseConsumptionQty"":59.6,""capacity"":216174,""buyPrice"":0,""sellPrice"":6756,""meanPrice"":5958,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":108087,""targetStock"":27021,""stock"":0,""demand"":151323.6,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.1400"",""sec_illegal_min"":""1.02"",""sec_illegal_max"":""1.18"",""stolenmod"":""0.7500""},{""id"":""128049227"",""name"":""Robotics"",""cost_min"":1999.112,""cost_max"":2549.35456,""cost_mean"":""1856.00"",""homebuy"":""82"",""homesell"":""80"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":60,""capacity"":312834,""buyPrice"":0,""sellPrice"":2500,""meanPrice"":1856,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":156417,""targetStock"":39104,""stock"":0,""demand"":218984.4,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Technology"",""volumescale"":""1.4500"",""sec_illegal_min"":""1.08"",""sec_illegal_max"":""1.69"",""stolenmod"":""0.7500""},{""id"":""128682044"",""name"":""Conductive Fabrics"",""cost_min"":944,""cost_max"":1431.1739130435,""cost_mean"":""507.00"",""homebuy"":""67"",""homesell"":""64"",""consumebuy"":""3"",""baseCreationQty"":46.4,""baseConsumptionQty"":278.4,""capacity"":846311.06,""buyPrice"":0,""sellPrice"":1276,""meanPrice"":507,""demandBracket"":2,""stockBracket"":0,""creationQty"":63108,""consumptionQty"":504886,""targetStock"":189329,""stock"":0,""demand"":485719.736,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Textiles"",""volumescale"":""1.2300"",""sec_illegal_min"":""1.16"",""sec_illegal_max"":""2.33"",""stolenmod"":""0.7500""},{""id"":""128049190"",""name"":""Leather"",""cost_min"":""175.00"",""cost_max"":343.85565217391,""cost_mean"":""205.00"",""homebuy"":""42"",""homesell"":""36"",""consumebuy"":""6"",""baseCreationQty"":0,""baseConsumptionQty"":1207.2,""capacity"":3248900.424,""buyPrice"":0,""sellPrice"":310,""meanPrice"":205,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":2189286,""targetStock"":547321,""stock"":0,""demand"":2075513.508,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Textiles"",""volumescale"":""1.0000"",""sec_illegal_min"":""1.23"",""sec_illegal_max"":""2.88"",""stolenmod"":""0.7500""},{""id"":""128682045"",""name"":""Military Grade Fabrics"",""cost_min"":2120.9282608696,""cost_max"":3312.0717391304,""cost_mean"":""708.00"",""homebuy"":""74"",""homesell"":""71"",""consumebuy"":""3"",""baseCreationQty"":29.2,""baseConsumptionQty"":0,""capacity"":59215.065,""buyPrice"":1586,""sellPrice"":1506,""meanPrice"":708,""demandBracket"":0,""stockBracket"":3,""creationQty"":39715,""consumptionQty"":0,""targetStock"":39715,""stock"":71050.135,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Textiles"",""volumescale"":""1.3200"",""sec_illegal_min"":""1.13"",""sec_illegal_max"":""2.04"",""stolenmod"":""0.7500""},{""id"":""128049191"",""name"":""Natural Fabrics"",""cost_min"":""403.00"",""cost_max"":627.93,""cost_mean"":""439.00"",""homebuy"":""64"",""homesell"":""60"",""consumebuy"":""4"",""baseCreationQty"":0,""baseConsumptionQty"":216.8,""capacity"":585433.108,""buyPrice"":0,""sellPrice"":403,""meanPrice"":439,""demandBracket"":1,""stockBracket"":0,""creationQty"":0,""consumptionQty"":393172,""targetStock"":98293,""stock"":0,""demand"":81891.38544514,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Textiles"",""volumescale"":""1.2000"",""sec_illegal_min"":""1.18"",""sec_illegal_max"":""2.44"",""stolenmod"":""0.7500""},{""id"":""128049193"",""name"":""Synthetic Fabrics"",""cost_min"":""186.00"",""cost_max"":357.57913043478,""cost_mean"":""211.00"",""homebuy"":""45"",""homesell"":""40"",""consumebuy"":""6"",""baseCreationQty"":136.2,""baseConsumptionQty"":817.6,""capacity"":2455269.504,""buyPrice"":0,""sellPrice"":303,""meanPrice"":211,""demandBracket"":2,""stockBracket"":0,""creationQty"":185244,""consumptionQty"":1482738,""targetStock"":555928,""stock"":0,""demand"":1406306.122,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Textiles"",""volumescale"":""1.0200"",""sec_illegal_min"":""1.29"",""sec_illegal_max"":""3.34"",""stolenmod"":""0.7500""},{""id"":""128049244"",""name"":""Biowaste"",""cost_min"":32.834782608696,""cost_max"":""97.00"",""cost_mean"":""63.00"",""homebuy"":""27"",""homesell"":""20"",""consumebuy"":""7"",""baseCreationQty"":162,""baseConsumptionQty"":0,""capacity"":29700.249,""buyPrice"":14,""sellPrice"":10,""meanPrice"":63,""demandBracket"":0,""stockBracket"":2,""creationQty"":19893,""consumptionQty"":0,""targetStock"":19893,""stock"":19962.88,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Waste "",""volumescale"":""1.0000"",""sec_illegal_min"":""1.44"",""sec_illegal_max"":""4.47"",""stolenmod"":""0.7500""},{""id"":""128049246"",""name"":""Chemical Waste"",""cost_min"":36.121739130435,""cost_max"":120.87826086957,""cost_mean"":""131.00"",""homebuy"":""18"",""homesell"":""10"",""consumebuy"":""8"",""baseCreationQty"":0,""baseConsumptionQty"":72.2,""capacity"":49428,""buyPrice"":0,""sellPrice"":114,""meanPrice"":131,""demandBracket"":2,""stockBracket"":0,""creationQty"":0,""consumptionQty"":32952,""targetStock"":8238,""stock"":0,""demand"":34599.6,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Waste "",""volumescale"":""1.0000"",""sec_illegal_min"":""1.48"",""sec_illegal_max"":""4.78"",""stolenmod"":""0.7500""},{""id"":""128049248"",""name"":""Scrap"",""cost_min"":51.826086956522,""cost_max"":""120.00"",""cost_mean"":""48.00"",""homebuy"":""42"",""homesell"":""36"",""consumebuy"":""6"",""baseCreationQty"":362.4,""baseConsumptionQty"":30.2,""capacity"":330749.118,""buyPrice"":22,""sellPrice"":19,""meanPrice"":48,""demandBracket"":0,""stockBracket"":3,""creationQty"":177989,""consumptionQty"":54769,""targetStock"":191681,""stock"":306165.25249861,""demand"":1,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Waste "",""volumescale"":""1.0000"",""sec_illegal_min"":""1.31"",""sec_illegal_max"":""3.48"",""stolenmod"":""0.7500""},{""id"":""128049236"",""name"":""Non Lethal Weapons"",""cost_min"":""1766.00"",""cost_max"":2252.08,""cost_mean"":""1837.00"",""homebuy"":""82"",""homesell"":""80"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":75,""capacity"":6296.612,""buyPrice"":0,""sellPrice"":2209,""meanPrice"":1837,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":4243,""targetStock"":1060,""stock"":0,""demand"":4408.752,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Weapons"",""volumescale"":""1.4500"",""sec_illegal_min"":""1.08"",""sec_illegal_max"":""1.69"",""stolenmod"":""0.7500""},{""id"":""128049235"",""name"":""Reactive Armour"",""cost_min"":""2008.00"",""cost_max"":2528.3956521739,""cost_mean"":""2113.00"",""homebuy"":""84"",""homesell"":""82"",""consumebuy"":""2"",""baseCreationQty"":0,""baseConsumptionQty"":68,""capacity"":12064.834,""buyPrice"":0,""sellPrice"":2482,""meanPrice"":2113,""demandBracket"":3,""stockBracket"":0,""creationQty"":0,""consumptionQty"":8119,""targetStock"":2029,""stock"":0,""demand"":8447.127,""rare_min_stock"":""0"",""rare_max_stock"":""0"",""market_id"":null,""parent_id"":null,""statusFlags"":[],""categoryname"":""Weapons"",""volumescale"":""1.4600"",""sec_illegal_min"":""1.08"",""sec_illegal_max"":""1.64"",""stolenmod"":""0.7500""}],""ships"":{""shipyard_list"":{""SideWinder"":{""id"":128049249,""name"":""SideWinder"",""basevalue"":32000,""sku"":""""},""Eagle"":{""id"":128049255,""name"":""Eagle"",""basevalue"":44800,""sku"":""""},""Hauler"":{""id"":128049261,""name"":""Hauler"",""basevalue"":52720,""sku"":""""},""Type7"":{""id"":128049297,""name"":""Type7"",""basevalue"":17472252,""sku"":""""},""Asp_Scout"":{""id"":128672276,""name"":""Asp_Scout"",""basevalue"":3961154,""sku"":""""},""Asp"":{""id"":128049303,""name"":""Asp"",""basevalue"":6661154,""sku"":""""},""Vulture"":{""id"":128049309,""name"":""Vulture"",""basevalue"":4925615,""sku"":""""},""Adder"":{""id"":128049267,""name"":""Adder"",""basevalue"":87808,""sku"":""""},""Federation_Dropship"":{""id"":128049321,""name"":""Federation_Dropship"",""basevalue"":14314205,""sku"":""""},""CobraMkIII"":{""id"":128049279,""name"":""CobraMkIII"",""basevalue"":349718,""sku"":""""},""Type6"":{""id"":128049285,""name"":""Type6"",""basevalue"":1045945,""sku"":""""}},""unavailable_list"":[]},""modules"":{""128049434"":{""id"":128049434,""category"":""weapon"",""name"":""Hpt_BeamLaser_Gimbal_Large"",""cost"":2396160,""sku"":null},""128049437"":{""id"":128049437,""category"":""weapon"",""name"":""Hpt_BeamLaser_Turret_Large"",""cost"":19399600,""sku"":null},""128049430"":{""id"":128049430,""category"":""weapon"",""name"":""Hpt_BeamLaser_Fixed_Large"",""cost"":1177600,""sku"":null},""128049433"":{""id"":128049433,""category"":""weapon"",""name"":""Hpt_BeamLaser_Gimbal_Medium"",""cost"":500600,""sku"":null},""128049436"":{""id"":128049436,""category"":""weapon"",""name"":""Hpt_BeamLaser_Turret_Medium"",""cost"":2099900,""sku"":null},""128049432"":{""id"":128049432,""category"":""weapon"",""name"":""Hpt_BeamLaser_Gimbal_Small"",""cost"":74650,""sku"":null},""128049435"":{""id"":128049435,""category"":""weapon"",""name"":""Hpt_BeamLaser_Turret_Small"",""cost"":500000,""sku"":null},""128049428"":{""id"":128049428,""category"":""weapon"",""name"":""Hpt_BeamLaser_Fixed_Small"",""cost"":37430,""sku"":null},""128681995"":{""id"":128681995,""category"":""weapon"",""name"":""Hpt_PulseLaser_Gimbal_Huge"",""cost"":877600,""sku"":null},""128049390"":{""id"":128049390,""category"":""weapon"",""name"":""Hpt_PulseLaser_Turret_Large"",""cost"":400400,""sku"":null},""128049387"":{""id"":128049387,""category"":""weapon"",""name"":""Hpt_PulseLaser_Gimbal_Large"",""cost"":140600,""sku"":null},""128049388"":{""id"":128049388,""category"":""weapon"",""name"":""Hpt_PulseLaser_Turret_Small"",""cost"":26000,""sku"":null},""128049386"":{""id"":128049386,""category"":""weapon"",""name"":""Hpt_PulseLaser_Gimbal_Medium"",""cost"":35400,""sku"":null},""128049385"":{""id"":128049385,""category"":""weapon"",""name"":""Hpt_PulseLaser_Gimbal_Small"",""cost"":6600,""sku"":null},""128049383"":{""id"":128049383,""category"":""weapon"",""name"":""Hpt_PulseLaser_Fixed_Large"",""cost"":70400,""sku"":null},""128049382"":{""id"":128049382,""category"":""weapon"",""name"":""Hpt_PulseLaser_Fixed_Medium"",""cost"":17600,""sku"":null},""128049409"":{""id"":128049409,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Turret_Large"",""cost"":800400,""sku"":null},""128049406"":{""id"":128049406,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Gimbal_Large"",""cost"":281600,""sku"":null},""128049404"":{""id"":128049404,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Gimbal_Small"",""cost"":8600,""sku"":null},""128049405"":{""id"":128049405,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Gimbal_Medium"",""cost"":48500,""sku"":null},""128049402"":{""id"":128049402,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Fixed_Large"",""cost"":140400,""sku"":null},""128049401"":{""id"":128049401,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Fixed_Medium"",""cost"":23000,""sku"":null},""128049400"":{""id"":128049400,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Fixed_Small"",""cost"":4400,""sku"":null},""128049467"":{""id"":128049467,""category"":""weapon"",""name"":""Hpt_PlasmaAccelerator_Fixed_Huge"",""cost"":13793600,""sku"":null},""128049466"":{""id"":128049466,""category"":""weapon"",""name"":""Hpt_PlasmaAccelerator_Fixed_Large"",""cost"":3051200,""sku"":null},""128049465"":{""id"":128049465,""category"":""weapon"",""name"":""Hpt_PlasmaAccelerator_Fixed_Medium"",""cost"":834200,""sku"":null},""128671321"":{""id"":128671321,""category"":""weapon"",""name"":""Hpt_Slugshot_Gimbal_Large"",""cost"":1751040,""sku"":null},""128671322"":{""id"":128671322,""category"":""weapon"",""name"":""Hpt_Slugshot_Turret_Large"",""cost"":5836800,""sku"":null},""128049454"":{""id"":128049454,""category"":""weapon"",""name"":""Hpt_Slugshot_Turret_Medium"",""cost"":1459200,""sku"":null},""128049450"":{""id"":128049450,""category"":""weapon"",""name"":""Hpt_Slugshot_Fixed_Large"",""cost"":1167360,""sku"":null},""128049453"":{""id"":128049453,""category"":""weapon"",""name"":""Hpt_Slugshot_Turret_Small"",""cost"":182400,""sku"":null},""128049451"":{""id"":128049451,""category"":""weapon"",""name"":""Hpt_Slugshot_Gimbal_Small"",""cost"":54720,""sku"":null},""128049461"":{""id"":128049461,""category"":""weapon"",""name"":""Hpt_MultiCannon_Gimbal_Large"",""cost"":578436,""sku"":null},""128049457"":{""id"":128049457,""category"":""weapon"",""name"":""Hpt_MultiCannon_Fixed_Large"",""cost"":140400,""sku"":null},""128049462"":{""id"":128049462,""category"":""weapon"",""name"":""Hpt_MultiCannon_Turret_Small"",""cost"":81600,""sku"":null},""128049459"":{""id"":128049459,""category"":""weapon"",""name"":""Hpt_MultiCannon_Gimbal_Small"",""cost"":14250,""sku"":null},""128049460"":{""id"":128049460,""category"":""weapon"",""name"":""Hpt_MultiCannon_Gimbal_Medium"",""cost"":57000,""sku"":null},""128049456"":{""id"":128049456,""category"":""weapon"",""name"":""Hpt_MultiCannon_Fixed_Medium"",""cost"":38000,""sku"":null},""128049510"":{""id"":128049510,""category"":""weapon"",""name"":""Hpt_AdvancedTorpPylon_Fixed_Medium"",""cost"":44800,""sku"":null},""128049509"":{""id"":128049509,""category"":""weapon"",""name"":""Hpt_AdvancedTorpPylon_Fixed_Small"",""cost"":11200,""sku"":null},""128666725"":{""id"":128666725,""category"":""weapon"",""name"":""Hpt_DumbfireMissileRack_Fixed_Medium"",""cost"":240400,""sku"":null},""128666724"":{""id"":128666724,""category"":""weapon"",""name"":""Hpt_DumbfireMissileRack_Fixed_Small"",""cost"":32175,""sku"":null},""128671448"":{""id"":128671448,""category"":""weapon"",""name"":""Hpt_MineLauncher_Fixed_Small_Impulse"",""cost"":36390,""sku"":null},""128049500"":{""id"":128049500,""category"":""weapon"",""name"":""Hpt_MineLauncher_Fixed_Small"",""cost"":24260,""sku"":null},""128049488"":{""id"":128049488,""category"":""weapon"",""name"":""Hpt_Railgun_Fixed_Small"",""cost"":51600,""sku"":null},""128049493"":{""id"":128049493,""category"":""weapon"",""name"":""Hpt_BasicMissileRack_Fixed_Medium"",""cost"":512400,""sku"":null},""128049492"":{""id"":128049492,""category"":""weapon"",""name"":""Hpt_BasicMissileRack_Fixed_Small"",""cost"":72600,""sku"":null},""128049441"":{""id"":128049441,""category"":""weapon"",""name"":""Hpt_Cannon_Fixed_Huge"",""cost"":2700800,""sku"":null},""128049444"":{""id"":128049444,""category"":""weapon"",""name"":""Hpt_Cannon_Gimbal_Huge"",""cost"":5401600,""sku"":null},""128671120"":{""id"":128671120,""category"":""weapon"",""name"":""Hpt_Cannon_Gimbal_Large"",""cost"":1350400,""sku"":null},""128049445"":{""id"":128049445,""category"":""weapon"",""name"":""Hpt_Cannon_Turret_Small"",""cost"":506400,""sku"":null},""128049458"":{""id"":128049458,""category"":""weapon"",""name"":""Hpt_MultiCannon_Fixed_Huge"",""cost"":1177600,""sku"":null},""128049455"":{""id"":128049455,""category"":""weapon"",""name"":""Hpt_MultiCannon_Fixed_Small"",""cost"":9500,""sku"":null},""128681994"":{""id"":128681994,""category"":""weapon"",""name"":""Hpt_BeamLaser_Gimbal_Huge"",""cost"":8746160,""sku"":null},""128049429"":{""id"":128049429,""category"":""weapon"",""name"":""Hpt_BeamLaser_Fixed_Medium"",""cost"":299520,""sku"":null},""128049489"":{""id"":128049489,""category"":""weapon"",""name"":""Hpt_Railgun_Fixed_Medium"",""cost"":412800,""sku"":null},""128049501"":{""id"":128049501,""category"":""weapon"",""name"":""Hpt_MineLauncher_Fixed_Medium"",""cost"":294080,""sku"":null},""128049389"":{""id"":128049389,""category"":""weapon"",""name"":""Hpt_PulseLaser_Turret_Medium"",""cost"":132800,""sku"":null},""128049381"":{""id"":128049381,""category"":""weapon"",""name"":""Hpt_PulseLaser_Fixed_Small"",""cost"":2200,""sku"":null},""128049452"":{""id"":128049452,""category"":""weapon"",""name"":""Hpt_Slugshot_Gimbal_Medium"",""cost"":437760,""sku"":null},""128049448"":{""id"":128049448,""category"":""weapon"",""name"":""Hpt_Slugshot_Fixed_Small"",""cost"":36000,""sku"":null},""128049408"":{""id"":128049408,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Turret_Medium"",""cost"":162800,""sku"":null},""128049407"":{""id"":128049407,""category"":""weapon"",""name"":""Hpt_PulseLaserBurst_Turret_Small"",""cost"":52800,""sku"":null},""128049519"":{""id"":128049519,""category"":""utility"",""name"":""Hpt_HeatSinkLauncher_Turret_Tiny"",""cost"":3500,""sku"":null},""128049526"":{""id"":128049526,""category"":""utility"",""name"":""Hpt_MiningLaser_Fixed_Medium"",""cost"":22576,""sku"":null},""128049525"":{""id"":128049525,""category"":""utility"",""name"":""Hpt_MiningLaser_Fixed_Small"",""cost"":6800,""sku"":null},""128049513"":{""id"":128049513,""category"":""utility"",""name"":""Hpt_ChaffLauncher_Tiny"",""cost"":8500,""sku"":null},""128049549"":{""id"":128049549,""category"":""utility"",""name"":""Int_DockingComputer_Standard"",""cost"":4500,""sku"":null},""128662532"":{""id"":128662532,""category"":""utility"",""name"":""Hpt_CrimeScanner_Size0_Class3"",""cost"":121899,""sku"":null},""128662531"":{""id"":128662531,""category"":""utility"",""name"":""Hpt_CrimeScanner_Size0_Class2"",""cost"":40633,""sku"":null},""128662530"":{""id"":128662530,""category"":""utility"",""name"":""Hpt_CrimeScanner_Size0_Class1"",""cost"":13544,""sku"":null},""128662524"":{""id"":128662524,""category"":""utility"",""name"":""Hpt_CargoScanner_Size0_Class5"",""cost"":1097095,""sku"":null},""128662523"":{""id"":128662523,""category"":""utility"",""name"":""Hpt_CargoScanner_Size0_Class4"",""cost"":365698,""sku"":null},""128662520"":{""id"":128662520,""category"":""utility"",""name"":""Hpt_CargoScanner_Size0_Class1"",""cost"":13544,""sku"":null},""128662527"":{""id"":128662527,""category"":""utility"",""name"":""Hpt_CloudScanner_Size0_Class3"",""cost"":121899,""sku"":null},""128662526"":{""id"":128662526,""category"":""utility"",""name"":""Hpt_CloudScanner_Size0_Class2"",""cost"":40633,""sku"":null},""128662525"":{""id"":128662525,""category"":""utility"",""name"":""Hpt_CloudScanner_Size0_Class1"",""cost"":13544,""sku"":null},""128662529"":{""id"":128662529,""category"":""utility"",""name"":""Hpt_CloudScanner_Size0_Class5"",""cost"":1097095,""sku"":null},""128662528"":{""id"":128662528,""category"":""utility"",""name"":""Hpt_CloudScanner_Size0_Class4"",""cost"":365698,""sku"":null},""128049516"":{""id"":128049516,""category"":""utility"",""name"":""Hpt_ElectronicCountermeasure_Tiny"",""cost"":12500,""sku"":null},""128049522"":{""id"":128049522,""category"":""utility"",""name"":""Hpt_PlasmaPointDefence_Turret_Tiny"",""cost"":18546,""sku"":null},""128662533"":{""id"":128662533,""category"":""utility"",""name"":""Hpt_CrimeScanner_Size0_Class4"",""cost"":365698,""sku"":null},""128662534"":{""id"":128662534,""category"":""utility"",""name"":""Hpt_CrimeScanner_Size0_Class5"",""cost"":1097095,""sku"":null},""128049252"":{""id"":128049252,""category"":""module"",""name"":""SideWinder_Armour_Grade3"",""cost"":80320,""sku"":null},""128049251"":{""id"":128049251,""category"":""module"",""name"":""SideWinder_Armour_Grade2"",""cost"":25600,""sku"":null},""128049250"":{""id"":128049250,""category"":""module"",""name"":""SideWinder_Armour_Grade1"",""cost"":0,""sku"":null},""128049253"":{""id"":128049253,""category"":""module"",""name"":""SideWinder_Armour_Mirrored"",""cost"":132064,""sku"":null},""128049254"":{""id"":128049254,""category"":""module"",""name"":""SideWinder_Armour_Reactive"",""cost"":139424,""sku"":null},""128049258"":{""id"":128049258,""category"":""module"",""name"":""Eagle_Armour_Grade3"",""cost"":90048,""sku"":null},""128049257"":{""id"":128049257,""category"":""module"",""name"":""Eagle_Armour_Grade2"",""cost"":26880,""sku"":null},""128049256"":{""id"":128049256,""category"":""module"",""name"":""Eagle_Armour_Grade1"",""cost"":0,""sku"":null},""128049259"":{""id"":128049259,""category"":""module"",""name"":""Eagle_Armour_Mirrored"",""cost"":140089,""sku"":null},""128049260"":{""id"":128049260,""category"":""module"",""name"":""Eagle_Armour_Reactive"",""cost"":150393,""sku"":null},""128049264"":{""id"":128049264,""category"":""module"",""name"":""Hauler_Armour_Grade3"",""cost"":185047,""sku"":null},""128049263"":{""id"":128049263,""category"":""module"",""name"":""Hauler_Armour_Grade2"",""cost"":42176,""sku"":null},""128049262"":{""id"":128049262,""category"":""module"",""name"":""Hauler_Armour_Grade1"",""cost"":0,""sku"":null},""128049265"":{""id"":128049265,""category"":""module"",""name"":""Hauler_Armour_Mirrored"",""cost"":270295,""sku"":null},""128049266"":{""id"":128049266,""category"":""module"",""name"":""Hauler_Armour_Reactive"",""cost"":282421,""sku"":null},""128049300"":{""id"":128049300,""category"":""module"",""name"":""Type7_Armour_Grade3"",""cost"":15725026,""sku"":null},""128049299"":{""id"":128049299,""category"":""module"",""name"":""Type7_Armour_Grade2"",""cost"":6988900,""sku"":null},""128049298"":{""id"":128049298,""category"":""module"",""name"":""Type7_Armour_Grade1"",""cost"":0,""sku"":null},""128049301"":{""id"":128049301,""category"":""module"",""name"":""Type7_Armour_Mirrored"",""cost"":37163480,""sku"":null},""128049302"":{""id"":128049302,""category"":""module"",""name"":""Type7_Armour_Reactive"",""cost"":41182097,""sku"":null},""128672280"":{""id"":128672280,""category"":""module"",""name"":""Asp_Scout_Armour_Grade3"",""cost"":3565038,""sku"":null},""128672279"":{""id"":128672279,""category"":""module"",""name"":""Asp_Scout_Armour_Grade2"",""cost"":1584461,""sku"":null},""128672278"":{""id"":128672278,""category"":""module"",""name"":""Asp_Scout_Armour_Grade1"",""cost"":0,""sku"":null},""128672281"":{""id"":128672281,""category"":""module"",""name"":""Asp_Scout_Armour_Mirrored"",""cost"":8425374,""sku"":null},""128672282"":{""id"":128672282,""category"":""module"",""name"":""Asp_Scout_Armour_Reactive"",""cost"":9336439,""sku"":null},""128049306"":{""id"":128049306,""category"":""module"",""name"":""Asp_Armour_Grade3"",""cost"":5995038,""sku"":null},""128049305"":{""id"":128049305,""category"":""module"",""name"":""Asp_Armour_Grade2"",""cost"":2664461,""sku"":null},""128049304"":{""id"":128049304,""category"":""module"",""name"":""Asp_Armour_Grade1"",""cost"":0,""sku"":null},""128049307"":{""id"":128049307,""category"":""module"",""name"":""Asp_Armour_Mirrored"",""cost"":14168274,""sku"":null},""128049308"":{""id"":128049308,""category"":""module"",""name"":""Asp_Armour_Reactive"",""cost"":15700339,""sku"":null},""128049312"":{""id"":128049312,""category"":""module"",""name"":""Vulture_Armour_Grade3"",""cost"":4433053,""sku"":null},""128049311"":{""id"":128049311,""category"":""module"",""name"":""Vulture_Armour_Grade2"",""cost"":1970246,""sku"":null},""128049310"":{""id"":128049310,""category"":""module"",""name"":""Vulture_Armour_Grade1"",""cost"":0,""sku"":null},""128049313"":{""id"":128049313,""category"":""module"",""name"":""Vulture_Armour_Mirrored"",""cost"":10476783,""sku"":null},""128049314"":{""id"":128049314,""category"":""module"",""name"":""Vulture_Armour_Reactive"",""cost"":11609674,""sku"":null},""128049270"":{""id"":128049270,""category"":""module"",""name"":""Adder_Armour_Grade3"",""cost"":79027,""sku"":null},""128049269"":{""id"":128049269,""category"":""module"",""name"":""Adder_Armour_Grade2"",""cost"":35123,""sku"":null},""128049268"":{""id"":128049268,""category"":""module"",""name"":""Adder_Armour_Grade1"",""cost"":0,""sku"":null},""128049271"":{""id"":128049271,""category"":""module"",""name"":""Adder_Armour_Mirrored"",""cost"":186767,""sku"":null},""128049272"":{""id"":128049272,""category"":""module"",""name"":""Adder_Armour_Reactive"",""cost"":206963,""sku"":null},""128049324"":{""id"":128049324,""category"":""module"",""name"":""Federation_Dropship_Armour_Grade3"",""cost"":12882784,""sku"":null},""128049323"":{""id"":128049323,""category"":""module"",""name"":""Federation_Dropship_Armour_Grade2"",""cost"":5725682,""sku"":null},""128049322"":{""id"":128049322,""category"":""module"",""name"":""Federation_Dropship_Armour_Grade1"",""cost"":0,""sku"":null},""128049325"":{""id"":128049325,""category"":""module"",""name"":""Federation_Dropship_Armour_Mirrored"",""cost"":30446314,""sku"":null},""128049326"":{""id"":128049326,""category"":""module"",""name"":""Federation_Dropship_Armour_Reactive"",""cost"":33738581,""sku"":null},""128662535"":{""id"":128662535,""category"":""module"",""name"":""Int_StellarBodyDiscoveryScanner_Standard"",""cost"":1000,""sku"":null},""128064338"":{""id"":128064338,""category"":""module"",""name"":""Int_CargoRack_Size1_Class1"",""cost"":1000,""sku"":null},""128666684"":{""id"":128666684,""category"":""module"",""name"":""Int_Refinery_Size1_Class1"",""cost"":6000,""sku"":null},""128666644"":{""id"":128666644,""category"":""module"",""name"":""Int_FuelScoop_Size1_Class1"",""cost"":309,""sku"":null},""128672317"":{""id"":128672317,""category"":""module"",""name"":""Int_PlanetApproachSuite"",""cost"":500,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128666704"":{""id"":128666704,""category"":""module"",""name"":""Int_FSDInterdictor_Size1_Class1"",""cost"":12000,""sku"":null},""128066532"":{""id"":128066532,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size1_Class1"",""cost"":600,""sku"":null},""128064263"":{""id"":128064263,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class1"",""cost"":1978,""sku"":null},""128064112"":{""id"":128064112,""category"":""module"",""name"":""Int_Hyperdrive_Size3_Class5"",""cost"":507912,""sku"":null},""128064107"":{""id"":128064107,""category"":""module"",""name"":""Int_Hyperdrive_Size2_Class5"",""cost"":160224,""sku"":null},""128064111"":{""id"":128064111,""category"":""module"",""name"":""Int_Hyperdrive_Size3_Class4"",""cost"":169304,""sku"":null},""128064106"":{""id"":128064106,""category"":""module"",""name"":""Int_Hyperdrive_Size2_Class4"",""cost"":53408,""sku"":null},""128064110"":{""id"":128064110,""category"":""module"",""name"":""Int_Hyperdrive_Size3_Class3"",""cost"":56435,""sku"":null},""128064105"":{""id"":128064105,""category"":""module"",""name"":""Int_Hyperdrive_Size2_Class3"",""cost"":17803,""sku"":null},""128064109"":{""id"":128064109,""category"":""module"",""name"":""Int_Hyperdrive_Size3_Class2"",""cost"":18812,""sku"":null},""128064104"":{""id"":128064104,""category"":""module"",""name"":""Int_Hyperdrive_Size2_Class2"",""cost"":5934,""sku"":null},""128064108"":{""id"":128064108,""category"":""module"",""name"":""Int_Hyperdrive_Size3_Class1"",""cost"":6271,""sku"":null},""128064103"":{""id"":128064103,""category"":""module"",""name"":""Int_Hyperdrive_Size2_Class1"",""cost"":1978,""sku"":null},""128064272"":{""id"":128064272,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class5"",""cost"":507912,""sku"":null},""128064267"":{""id"":128064267,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class5"",""cost"":160224,""sku"":null},""128064271"":{""id"":128064271,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class4"",""cost"":169304,""sku"":null},""128064266"":{""id"":128064266,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class4"",""cost"":53408,""sku"":null},""128671333"":{""id"":128671333,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class3_Fast"",""cost"":84653,""sku"":null},""128064270"":{""id"":128064270,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class3"",""cost"":56435,""sku"":null},""128671332"":{""id"":128671332,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class3_Fast"",""cost"":26705,""sku"":null},""128671331"":{""id"":128671331,""category"":""module"",""name"":""Int_ShieldGenerator_Size1_Class3_Fast"",""cost"":7713,""sku"":null},""128064265"":{""id"":128064265,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class3"",""cost"":17803,""sku"":null},""128064269"":{""id"":128064269,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class2"",""cost"":18812,""sku"":null},""128064264"":{""id"":128064264,""category"":""module"",""name"":""Int_ShieldGenerator_Size2_Class2"",""cost"":5934,""sku"":null},""128064268"":{""id"":128064268,""category"":""module"",""name"":""Int_ShieldGenerator_Size3_Class1"",""cost"":6271,""sku"":null},""128672293"":{""id"":128672293,""category"":""module"",""name"":""Int_BuggyBay_Size6_Class2"",""cost"":691200,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128672291"":{""id"":128672291,""category"":""module"",""name"":""Int_BuggyBay_Size4_Class2"",""cost"":86400,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128672292"":{""id"":128672292,""category"":""module"",""name"":""Int_BuggyBay_Size6_Class1"",""cost"":576000,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128672289"":{""id"":128672289,""category"":""module"",""name"":""Int_BuggyBay_Size2_Class2"",""cost"":21600,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128672290"":{""id"":128672290,""category"":""module"",""name"":""Int_BuggyBay_Size4_Class1"",""cost"":72000,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128672288"":{""id"":128672288,""category"":""module"",""name"":""Int_BuggyBay_Size2_Class1"",""cost"":18000,""sku"":""ELITE_HORIZONS_V_PLANETARY_LANDINGS""},""128064297"":{""id"":128064297,""category"":""module"",""name"":""Int_ShieldGenerator_Size8_Class5"",""cost"":162586486,""sku"":null},""128064292"":{""id"":128064292,""category"":""module"",""name"":""Int_ShieldGenerator_Size7_Class5"",""cost"":51289112,""sku"":null},""128064291"":{""id"":128064291,""category"":""module"",""name"":""Int_ShieldGenerator_Size7_Class4"",""cost"":17096371,""sku"":null},""128671338"":{""id"":128671338,""category"":""module"",""name"":""Int_ShieldGenerator_Size8_Class3_Fast"",""cost"":27097748,""sku"":null},""128064295"":{""id"":128064295,""category"":""module"",""name"":""Int_ShieldGenerator_Size8_Class3"",""cost"":18065165,""sku"":null},""128671337"":{""id"":128671337,""category"":""module"",""name"":""Int_ShieldGenerator_Size7_Class3_Fast"",""cost"":8548185,""sku"":null},""128064290"":{""id"":128064290,""category"":""module"",""name"":""Int_ShieldGenerator_Size7_Class3"",""cost"":5698790,""sku"":null},""128064294"":{""id"":128064294,""category"":""module"",""name"":""Int_ShieldGenerator_Size8_Class2"",""cost"":6021722,""sku"":null},""128064289"":{""id"":128064289,""category"":""module"",""name"":""Int_ShieldGenerator_Size7_Class2"",""cost"":1899597,""sku"":null},""128064293"":{""id"":128064293,""category"":""module"",""name"":""Int_ShieldGenerator_Size8_Class1"",""cost"":2007241,""sku"":null},""128064288"":{""id"":128064288,""category"":""module"",""name"":""Int_ShieldGenerator_Size7_Class1"",""cost"":633199,""sku"":null},""128666717"":{""id"":128666717,""category"":""module"",""name"":""Int_FSDInterdictor_Size2_Class4"",""cost"":907200,""sku"":null},""128666713"":{""id"":128666713,""category"":""module"",""name"":""Int_FSDInterdictor_Size2_Class3"",""cost"":302400,""sku"":null},""128666716"":{""id"":128666716,""category"":""module"",""name"":""Int_FSDInterdictor_Size1_Class4"",""cost"":324000,""sku"":null},""128666712"":{""id"":128666712,""category"":""module"",""name"":""Int_FSDInterdictor_Size1_Class3"",""cost"":108000,""sku"":null},""128666709"":{""id"":128666709,""category"":""module"",""name"":""Int_FSDInterdictor_Size2_Class2"",""cost"":100800,""sku"":null},""128666708"":{""id"":128666708,""category"":""module"",""name"":""Int_FSDInterdictor_Size1_Class2"",""cost"":36000,""sku"":null},""128666705"":{""id"":128666705,""category"":""module"",""name"":""Int_FSDInterdictor_Size2_Class1"",""cost"":33600,""sku"":null},""128064042"":{""id"":128064042,""category"":""module"",""name"":""Int_Powerplant_Size3_Class5"",""cost"":507912,""sku"":null},""128064037"":{""id"":128064037,""category"":""module"",""name"":""Int_Powerplant_Size2_Class5"",""cost"":160224,""sku"":null},""128064041"":{""id"":128064041,""category"":""module"",""name"":""Int_Powerplant_Size3_Class4"",""cost"":169304,""sku"":null},""128064036"":{""id"":128064036,""category"":""module"",""name"":""Int_Powerplant_Size2_Class4"",""cost"":53408,""sku"":null},""128064040"":{""id"":128064040,""category"":""module"",""name"":""Int_Powerplant_Size3_Class3"",""cost"":56435,""sku"":null},""128064035"":{""id"":128064035,""category"":""module"",""name"":""Int_Powerplant_Size2_Class3"",""cost"":17803,""sku"":null},""128064039"":{""id"":128064039,""category"":""module"",""name"":""Int_Powerplant_Size3_Class2"",""cost"":18812,""sku"":null},""128064034"":{""id"":128064034,""category"":""module"",""name"":""Int_Powerplant_Size2_Class2"",""cost"":5934,""sku"":null},""128064038"":{""id"":128064038,""category"":""module"",""name"":""Int_Powerplant_Size3_Class1"",""cost"":6271,""sku"":null},""128064033"":{""id"":128064033,""category"":""module"",""name"":""Int_Powerplant_Size2_Class1"",""cost"":1978,""sku"":null},""128064345"":{""id"":128064345,""category"":""module"",""name"":""Int_CargoRack_Size8_Class1"",""cost"":3829866,""sku"":null},""128064344"":{""id"":128064344,""category"":""module"",""name"":""Int_CargoRack_Size7_Class1"",""cost"":1178420,""sku"":null},""128064343"":{""id"":128064343,""category"":""module"",""name"":""Int_CargoRack_Size6_Class1"",""cost"":362591,""sku"":null},""128064342"":{""id"":128064342,""category"":""module"",""name"":""Int_CargoRack_Size5_Class1"",""cost"":111566,""sku"":null},""128064341"":{""id"":128064341,""category"":""module"",""name"":""Int_CargoRack_Size4_Class1"",""cost"":34328,""sku"":null},""128064340"":{""id"":128064340,""category"":""module"",""name"":""Int_CargoRack_Size3_Class1"",""cost"":10563,""sku"":null},""128064339"":{""id"":128064339,""category"":""module"",""name"":""Int_CargoRack_Size2_Class1"",""cost"":3250,""sku"":null},""128064353"":{""id"":128064353,""category"":""module"",""name"":""Int_FuelTank_Size8_Class3"",""cost"":5428429,""sku"":null},""128064352"":{""id"":128064352,""category"":""module"",""name"":""Int_FuelTank_Size7_Class3"",""cost"":1780914,""sku"":null},""128064351"":{""id"":128064351,""category"":""module"",""name"":""Int_FuelTank_Size6_Class3"",""cost"":341577,""sku"":null},""128064350"":{""id"":128064350,""category"":""module"",""name"":""Int_FuelTank_Size5_Class3"",""cost"":97754,""sku"":null},""128064349"":{""id"":128064349,""category"":""module"",""name"":""Int_FuelTank_Size4_Class3"",""cost"":24734,""sku"":null},""128064348"":{""id"":128064348,""category"":""module"",""name"":""Int_FuelTank_Size3_Class3"",""cost"":7063,""sku"":null},""128064347"":{""id"":128064347,""category"":""module"",""name"":""Int_FuelTank_Size2_Class3"",""cost"":3750,""sku"":null},""128064346"":{""id"":128064346,""category"":""module"",""name"":""Int_FuelTank_Size1_Class3"",""cost"":1000,""sku"":null},""128671252"":{""id"":128671252,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size1_Class4"",""cost"":4800,""sku"":null},""128671264"":{""id"":128671264,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size7_Class1"",""cost"":437400,""sku"":null},""128671260"":{""id"":128671260,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size5_Class2"",""cost"":97200,""sku"":null},""128671256"":{""id"":128671256,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size3_Class3"",""cost"":21600,""sku"":null},""128671251"":{""id"":128671251,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size1_Class3"",""cost"":2400,""sku"":null},""128671259"":{""id"":128671259,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size5_Class1"",""cost"":48600,""sku"":null},""128671255"":{""id"":128671255,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size3_Class2"",""cost"":10800,""sku"":null},""128671250"":{""id"":128671250,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size1_Class2"",""cost"":1200,""sku"":null},""128671254"":{""id"":128671254,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size3_Class1"",""cost"":5400,""sku"":null},""128671249"":{""id"":128671249,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size1_Class1"",""cost"":600,""sku"":null},""128666697"":{""id"":128666697,""category"":""module"",""name"":""Int_Refinery_Size2_Class4"",""cost"":340200,""sku"":null},""128666693"":{""id"":128666693,""category"":""module"",""name"":""Int_Refinery_Size2_Class3"",""cost"":113400,""sku"":null},""128666696"":{""id"":128666696,""category"":""module"",""name"":""Int_Refinery_Size1_Class4"",""cost"":162000,""sku"":null},""128666689"":{""id"":128666689,""category"":""module"",""name"":""Int_Refinery_Size2_Class2"",""cost"":37800,""sku"":null},""128666692"":{""id"":128666692,""category"":""module"",""name"":""Int_Refinery_Size1_Class3"",""cost"":54000,""sku"":null},""128666688"":{""id"":128666688,""category"":""module"",""name"":""Int_Refinery_Size1_Class2"",""cost"":18000,""sku"":null},""128666685"":{""id"":128666685,""category"":""module"",""name"":""Int_Refinery_Size2_Class1"",""cost"":12600,""sku"":null},""128064257"":{""id"":128064257,""category"":""module"",""name"":""Int_Sensors_Size8_Class5"",""cost"":27249391,""sku"":null},""128064256"":{""id"":128064256,""category"":""module"",""name"":""Int_Sensors_Size8_Class4"",""cost"":10899756,""sku"":null},""128064255"":{""id"":128064255,""category"":""module"",""name"":""Int_Sensors_Size8_Class3"",""cost"":4359903,""sku"":null},""128064254"":{""id"":128064254,""category"":""module"",""name"":""Int_Sensors_Size8_Class2"",""cost"":1743961,""sku"":null},""128064249"":{""id"":128064249,""category"":""module"",""name"":""Int_Sensors_Size7_Class2"",""cost"":622843,""sku"":null},""128064253"":{""id"":128064253,""category"":""module"",""name"":""Int_Sensors_Size8_Class1"",""cost"":697584,""sku"":null},""128064248"":{""id"":128064248,""category"":""module"",""name"":""Int_Sensors_Size7_Class1"",""cost"":249137,""sku"":null},""128064192"":{""id"":128064192,""category"":""module"",""name"":""Int_PowerDistributor_Size3_Class5"",""cost"":158331,""sku"":null},""128064187"":{""id"":128064187,""category"":""module"",""name"":""Int_PowerDistributor_Size2_Class5"",""cost"":56547,""sku"":null},""128064182"":{""id"":128064182,""category"":""module"",""name"":""Int_PowerDistributor_Size1_Class5"",""cost"":20195,""sku"":null},""128064191"":{""id"":128064191,""category"":""module"",""name"":""Int_PowerDistributor_Size3_Class4"",""cost"":63333,""sku"":null},""128064181"":{""id"":128064181,""category"":""module"",""name"":""Int_PowerDistributor_Size1_Class4"",""cost"":8078,""sku"":null},""128064190"":{""id"":128064190,""category"":""module"",""name"":""Int_PowerDistributor_Size3_Class3"",""cost"":25333,""sku"":null},""128064185"":{""id"":128064185,""category"":""module"",""name"":""Int_PowerDistributor_Size2_Class3"",""cost"":9048,""sku"":null},""128064180"":{""id"":128064180,""category"":""module"",""name"":""Int_PowerDistributor_Size1_Class3"",""cost"":3231,""sku"":null},""128064189"":{""id"":128064189,""category"":""module"",""name"":""Int_PowerDistributor_Size3_Class2"",""cost"":10133,""sku"":null},""128064179"":{""id"":128064179,""category"":""module"",""name"":""Int_PowerDistributor_Size1_Class2"",""cost"":1293,""sku"":null},""128064184"":{""id"":128064184,""category"":""module"",""name"":""Int_PowerDistributor_Size2_Class2"",""cost"":3619,""sku"":null},""128064188"":{""id"":128064188,""category"":""module"",""name"":""Int_PowerDistributor_Size3_Class1"",""cost"":4053,""sku"":null},""128064183"":{""id"":128064183,""category"":""module"",""name"":""Int_PowerDistributor_Size2_Class1"",""cost"":1448,""sku"":null},""128064178"":{""id"":128064178,""category"":""module"",""name"":""Int_PowerDistributor_Size1_Class1"",""cost"":517,""sku"":null},""128064217"":{""id"":128064217,""category"":""module"",""name"":""Int_PowerDistributor_Size8_Class5"",""cost"":27249391,""sku"":null},""128064212"":{""id"":128064212,""category"":""module"",""name"":""Int_PowerDistributor_Size7_Class5"",""cost"":9731925,""sku"":null},""128064210"":{""id"":128064210,""category"":""module"",""name"":""Int_PowerDistributor_Size7_Class3"",""cost"":1557108,""sku"":null},""128064214"":{""id"":128064214,""category"":""module"",""name"":""Int_PowerDistributor_Size8_Class2"",""cost"":1743961,""sku"":null},""128064213"":{""id"":128064213,""category"":""module"",""name"":""Int_PowerDistributor_Size8_Class1"",""cost"":697584,""sku"":null},""128064208"":{""id"":128064208,""category"":""module"",""name"":""Int_PowerDistributor_Size7_Class1"",""cost"":249137,""sku"":null},""128064077"":{""id"":128064077,""category"":""module"",""name"":""Int_Engine_Size3_Class5"",""cost"":507912,""sku"":null},""128064072"":{""id"":128064072,""category"":""module"",""name"":""Int_Engine_Size2_Class5"",""cost"":160224,""sku"":null},""128064076"":{""id"":128064076,""category"":""module"",""name"":""Int_Engine_Size3_Class4"",""cost"":169304,""sku"":null},""128064071"":{""id"":128064071,""category"":""module"",""name"":""Int_Engine_Size2_Class4"",""cost"":53408,""sku"":null},""128064075"":{""id"":128064075,""category"":""module"",""name"":""Int_Engine_Size3_Class3"",""cost"":56435,""sku"":null},""128064070"":{""id"":128064070,""category"":""module"",""name"":""Int_Engine_Size2_Class3"",""cost"":17803,""sku"":null},""128064074"":{""id"":128064074,""category"":""module"",""name"":""Int_Engine_Size3_Class2"",""cost"":18812,""sku"":null},""128064069"":{""id"":128064069,""category"":""module"",""name"":""Int_Engine_Size2_Class2"",""cost"":5934,""sku"":null},""128064073"":{""id"":128064073,""category"":""module"",""name"":""Int_Engine_Size3_Class1"",""cost"":6271,""sku"":null},""128064068"":{""id"":128064068,""category"":""module"",""name"":""Int_Engine_Size2_Class1"",""cost"":1978,""sku"":null},""128666670"":{""id"":128666670,""category"":""module"",""name"":""Int_FuelScoop_Size3_Class4"",""cost"":225738,""sku"":null},""128666662"":{""id"":128666662,""category"":""module"",""name"":""Int_FuelScoop_Size3_Class3"",""cost"":56435,""sku"":null},""128666676"":{""id"":128666676,""category"":""module"",""name"":""Int_FuelScoop_Size1_Class5"",""cost"":82270,""sku"":null},""128666668"":{""id"":128666668,""category"":""module"",""name"":""Int_FuelScoop_Size1_Class4"",""cost"":20568,""sku"":null},""128666661"":{""id"":128666661,""category"":""module"",""name"":""Int_FuelScoop_Size2_Class3"",""cost"":17803,""sku"":null},""128666660"":{""id"":128666660,""category"":""module"",""name"":""Int_FuelScoop_Size1_Class3"",""cost"":5142,""sku"":null},""128666654"":{""id"":128666654,""category"":""module"",""name"":""Int_FuelScoop_Size3_Class2"",""cost"":14109,""sku"":null},""128666653"":{""id"":128666653,""category"":""module"",""name"":""Int_FuelScoop_Size2_Class2"",""cost"":4451,""sku"":null},""128666646"":{""id"":128666646,""category"":""module"",""name"":""Int_FuelScoop_Size3_Class1"",""cost"":3386,""sku"":null},""128666652"":{""id"":128666652,""category"":""module"",""name"":""Int_FuelScoop_Size1_Class2"",""cost"":1285,""sku"":null},""128666645"":{""id"":128666645,""category"":""module"",""name"":""Int_FuelScoop_Size2_Class1"",""cost"":1068,""sku"":null},""128666699"":{""id"":128666699,""category"":""module"",""name"":""Int_Refinery_Size4_Class4"",""cost"":1500282,""sku"":null},""128666695"":{""id"":128666695,""category"":""module"",""name"":""Int_Refinery_Size4_Class3"",""cost"":500094,""sku"":null},""128666694"":{""id"":128666694,""category"":""module"",""name"":""Int_Refinery_Size3_Class3"",""cost"":238140,""sku"":null},""128666691"":{""id"":128666691,""category"":""module"",""name"":""Int_Refinery_Size4_Class2"",""cost"":166698,""sku"":null},""128666687"":{""id"":128666687,""category"":""module"",""name"":""Int_Refinery_Size4_Class1"",""cost"":55566,""sku"":null},""128666690"":{""id"":128666690,""category"":""module"",""name"":""Int_Refinery_Size3_Class2"",""cost"":79380,""sku"":null},""128666686"":{""id"":128666686,""category"":""module"",""name"":""Int_Refinery_Size3_Class1"",""cost"":26460,""sku"":null},""128671272"":{""id"":128671272,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size1_Class4"",""cost"":4800,""sku"":null},""128671284"":{""id"":128671284,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size7_Class1"",""cost"":437400,""sku"":null},""128671280"":{""id"":128671280,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size5_Class2"",""cost"":97200,""sku"":null},""128671276"":{""id"":128671276,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size3_Class3"",""cost"":21600,""sku"":null},""128671271"":{""id"":128671271,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size1_Class3"",""cost"":2400,""sku"":null},""128671279"":{""id"":128671279,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size5_Class1"",""cost"":48600,""sku"":null},""128671275"":{""id"":128671275,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size3_Class2"",""cost"":10800,""sku"":null},""128671270"":{""id"":128671270,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size1_Class2"",""cost"":1200,""sku"":null},""128671274"":{""id"":128671274,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size3_Class1"",""cost"":5400,""sku"":null},""128671269"":{""id"":128671269,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size1_Class1"",""cost"":600,""sku"":null},""128667629"":{""id"":128667629,""category"":""module"",""name"":""Int_Repairer_Size8_Class4"",""cost"":16529941,""sku"":null},""128667621"":{""id"":128667621,""category"":""module"",""name"":""Int_Repairer_Size8_Class3"",""cost"":5509980,""sku"":null},""128667620"":{""id"":128667620,""category"":""module"",""name"":""Int_Repairer_Size7_Class3"",""cost"":3061100,""sku"":null},""128667627"":{""id"":128667627,""category"":""module"",""name"":""Int_Repairer_Size6_Class4"",""cost"":5101834,""sku"":null},""128667626"":{""id"":128667626,""category"":""module"",""name"":""Int_Repairer_Size5_Class4"",""cost"":2834352,""sku"":null},""128667613"":{""id"":128667613,""category"":""module"",""name"":""Int_Repairer_Size8_Class2"",""cost"":1836660,""sku"":null},""128667619"":{""id"":128667619,""category"":""module"",""name"":""Int_Repairer_Size6_Class3"",""cost"":1700611,""sku"":null},""128667612"":{""id"":128667612,""category"":""module"",""name"":""Int_Repairer_Size7_Class2"",""cost"":1020367,""sku"":null},""128667618"":{""id"":128667618,""category"":""module"",""name"":""Int_Repairer_Size5_Class3"",""cost"":944784,""sku"":null},""128667632"":{""id"":128667632,""category"":""module"",""name"":""Int_Repairer_Size3_Class5"",""cost"":2624400,""sku"":null},""128667617"":{""id"":128667617,""category"":""module"",""name"":""Int_Repairer_Size4_Class3"",""cost"":524880,""sku"":null},""128667624"":{""id"":128667624,""category"":""module"",""name"":""Int_Repairer_Size3_Class4"",""cost"":874800,""sku"":null},""128667631"":{""id"":128667631,""category"":""module"",""name"":""Int_Repairer_Size2_Class5"",""cost"":1458000,""sku"":null},""128667630"":{""id"":128667630,""category"":""module"",""name"":""Int_Repairer_Size1_Class5"",""cost"":810000,""sku"":null},""128663561"":{""id"":128663561,""category"":""module"",""name"":""Int_StellarBodyDiscoveryScanner_Advanced"",""cost"":1545000,""sku"":null},""128663560"":{""id"":128663560,""category"":""module"",""name"":""Int_StellarBodyDiscoveryScanner_Intermediate"",""cost"":505000,""sku"":null},""128666634"":{""id"":128666634,""category"":""module"",""name"":""Int_DetailedSurfaceScanner_Tiny"",""cost"":250000,""sku"":null},""128064152"":{""id"":128064152,""category"":""module"",""name"":""Int_LifeSupport_Size3_Class5"",""cost"":158331,""sku"":null},""128064151"":{""id"":128064151,""category"":""module"",""name"":""Int_LifeSupport_Size3_Class4"",""cost"":63333,""sku"":null},""128064141"":{""id"":128064141,""category"":""module"",""name"":""Int_LifeSupport_Size1_Class4"",""cost"":8078,""sku"":null},""128064146"":{""id"":128064146,""category"":""module"",""name"":""Int_LifeSupport_Size2_Class4"",""cost"":22619,""sku"":null},""128064150"":{""id"":128064150,""category"":""module"",""name"":""Int_LifeSupport_Size3_Class3"",""cost"":25333,""sku"":null},""128064145"":{""id"":128064145,""category"":""module"",""name"":""Int_LifeSupport_Size2_Class3"",""cost"":9048,""sku"":null},""128064140"":{""id"":128064140,""category"":""module"",""name"":""Int_LifeSupport_Size1_Class3"",""cost"":3231,""sku"":null},""128064149"":{""id"":128064149,""category"":""module"",""name"":""Int_LifeSupport_Size3_Class2"",""cost"":10133,""sku"":null},""128064139"":{""id"":128064139,""category"":""module"",""name"":""Int_LifeSupport_Size1_Class2"",""cost"":1293,""sku"":null},""128064144"":{""id"":128064144,""category"":""module"",""name"":""Int_LifeSupport_Size2_Class2"",""cost"":3619,""sku"":null},""128064148"":{""id"":128064148,""category"":""module"",""name"":""Int_LifeSupport_Size3_Class1"",""cost"":4053,""sku"":null},""128064143"":{""id"":128064143,""category"":""module"",""name"":""Int_LifeSupport_Size2_Class1"",""cost"":1448,""sku"":null},""128064138"":{""id"":128064138,""category"":""module"",""name"":""Int_LifeSupport_Size1_Class1"",""cost"":517,""sku"":null},""128671268"":{""id"":128671268,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size7_Class5"",""cost"":6998400,""sku"":null},""128671258"":{""id"":128671258,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size3_Class5"",""cost"":86400,""sku"":null},""128671253"":{""id"":128671253,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size1_Class5"",""cost"":9600,""sku"":null},""128671267"":{""id"":128671267,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size7_Class4"",""cost"":3499200,""sku"":null},""128671262"":{""id"":128671262,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size5_Class4"",""cost"":388800,""sku"":null},""128671266"":{""id"":128671266,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size7_Class3"",""cost"":1749600,""sku"":null},""128671257"":{""id"":128671257,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size3_Class4"",""cost"":43200,""sku"":null},""128671265"":{""id"":128671265,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size7_Class2"",""cost"":874800,""sku"":null},""128671261"":{""id"":128671261,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size5_Class3"",""cost"":194400,""sku"":null},""128064232"":{""id"":128064232,""category"":""module"",""name"":""Int_Sensors_Size3_Class5"",""cost"":158331,""sku"":null},""128064227"":{""id"":128064227,""category"":""module"",""name"":""Int_Sensors_Size2_Class5"",""cost"":56547,""sku"":null},""128064222"":{""id"":128064222,""category"":""module"",""name"":""Int_Sensors_Size1_Class5"",""cost"":20195,""sku"":null},""128064231"":{""id"":128064231,""category"":""module"",""name"":""Int_Sensors_Size3_Class4"",""cost"":63333,""sku"":null},""128064221"":{""id"":128064221,""category"":""module"",""name"":""Int_Sensors_Size1_Class4"",""cost"":8078,""sku"":null},""128064226"":{""id"":128064226,""category"":""module"",""name"":""Int_Sensors_Size2_Class4"",""cost"":22619,""sku"":null},""128064230"":{""id"":128064230,""category"":""module"",""name"":""Int_Sensors_Size3_Class3"",""cost"":25333,""sku"":null},""128064225"":{""id"":128064225,""category"":""module"",""name"":""Int_Sensors_Size2_Class3"",""cost"":9048,""sku"":null},""128064220"":{""id"":128064220,""category"":""module"",""name"":""Int_Sensors_Size1_Class3"",""cost"":3231,""sku"":null},""128064229"":{""id"":128064229,""category"":""module"",""name"":""Int_Sensors_Size3_Class2"",""cost"":10133,""sku"":null},""128064219"":{""id"":128064219,""category"":""module"",""name"":""Int_Sensors_Size1_Class2"",""cost"":1293,""sku"":null},""128064224"":{""id"":128064224,""category"":""module"",""name"":""Int_Sensors_Size2_Class2"",""cost"":3619,""sku"":null},""128064228"":{""id"":128064228,""category"":""module"",""name"":""Int_Sensors_Size3_Class1"",""cost"":4053,""sku"":null},""128064223"":{""id"":128064223,""category"":""module"",""name"":""Int_Sensors_Size2_Class1"",""cost"":1448,""sku"":null},""128064307"":{""id"":128064307,""category"":""module"",""name"":""Int_ShieldCellBank_Size2_Class5"",""cost"":56547,""sku"":null},""128064302"":{""id"":128064302,""category"":""module"",""name"":""Int_ShieldCellBank_Size1_Class5"",""cost"":20195,""sku"":null},""128064301"":{""id"":128064301,""category"":""module"",""name"":""Int_ShieldCellBank_Size1_Class4"",""cost"":8078,""sku"":null},""128064306"":{""id"":128064306,""category"":""module"",""name"":""Int_ShieldCellBank_Size2_Class4"",""cost"":22619,""sku"":null},""128064310"":{""id"":128064310,""category"":""module"",""name"":""Int_ShieldCellBank_Size3_Class3"",""cost"":25333,""sku"":null},""128064305"":{""id"":128064305,""category"":""module"",""name"":""Int_ShieldCellBank_Size2_Class3"",""cost"":9048,""sku"":null},""128064300"":{""id"":128064300,""category"":""module"",""name"":""Int_ShieldCellBank_Size1_Class3"",""cost"":3231,""sku"":null},""128064309"":{""id"":128064309,""category"":""module"",""name"":""Int_ShieldCellBank_Size3_Class2"",""cost"":10133,""sku"":null},""128064299"":{""id"":128064299,""category"":""module"",""name"":""Int_ShieldCellBank_Size1_Class2"",""cost"":1293,""sku"":null},""128064304"":{""id"":128064304,""category"":""module"",""name"":""Int_ShieldCellBank_Size2_Class2"",""cost"":3619,""sku"":null},""128064308"":{""id"":128064308,""category"":""module"",""name"":""Int_ShieldCellBank_Size3_Class1"",""cost"":4053,""sku"":null},""128064303"":{""id"":128064303,""category"":""module"",""name"":""Int_ShieldCellBank_Size2_Class1"",""cost"":1448,""sku"":null},""128064298"":{""id"":128064298,""category"":""module"",""name"":""Int_ShieldCellBank_Size1_Class1"",""cost"":517,""sku"":null},""128666723"":{""id"":128666723,""category"":""module"",""name"":""Int_FSDInterdictor_Size4_Class5"",""cost"":21337344,""sku"":null},""128666719"":{""id"":128666719,""category"":""module"",""name"":""Int_FSDInterdictor_Size4_Class4"",""cost"":7112448,""sku"":null},""128666715"":{""id"":128666715,""category"":""module"",""name"":""Int_FSDInterdictor_Size4_Class3"",""cost"":2370816,""sku"":null},""128666722"":{""id"":128666722,""category"":""module"",""name"":""Int_FSDInterdictor_Size3_Class5"",""cost"":7620480,""sku"":null},""128666718"":{""id"":128666718,""category"":""module"",""name"":""Int_FSDInterdictor_Size3_Class4"",""cost"":2540160,""sku"":null},""128666711"":{""id"":128666711,""category"":""module"",""name"":""Int_FSDInterdictor_Size4_Class2"",""cost"":790272,""sku"":null},""128666707"":{""id"":128666707,""category"":""module"",""name"":""Int_FSDInterdictor_Size4_Class1"",""cost"":263424,""sku"":null},""128666710"":{""id"":128666710,""category"":""module"",""name"":""Int_FSDInterdictor_Size3_Class2"",""cost"":282240,""sku"":null},""128666706"":{""id"":128666706,""category"":""module"",""name"":""Int_FSDInterdictor_Size3_Class1"",""cost"":94080,""sku"":null},""128066541"":{""id"":128066541,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size3_Class5"",""cost"":86400,""sku"":null},""128066536"":{""id"":128066536,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size1_Class5"",""cost"":9600,""sku"":null},""128066540"":{""id"":128066540,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size3_Class4"",""cost"":43200,""sku"":null},""128066535"":{""id"":128066535,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size1_Class4"",""cost"":4800,""sku"":null},""128066539"":{""id"":128066539,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size3_Class3"",""cost"":21600,""sku"":null},""128066534"":{""id"":128066534,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size1_Class3"",""cost"":2400,""sku"":null},""128066538"":{""id"":128066538,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size3_Class2"",""cost"":10800,""sku"":null},""128066533"":{""id"":128066533,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size1_Class2"",""cost"":1200,""sku"":null},""128066537"":{""id"":128066537,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size3_Class1"",""cost"":5400,""sku"":null},""128666682"":{""id"":128666682,""category"":""module"",""name"":""Int_FuelScoop_Size7_Class5"",""cost"":91180644,""sku"":null},""128666674"":{""id"":128666674,""category"":""module"",""name"":""Int_FuelScoop_Size7_Class4"",""cost"":22795161,""sku"":null},""128666667"":{""id"":128666667,""category"":""module"",""name"":""Int_FuelScoop_Size8_Class3"",""cost"":18065165,""sku"":null},""128666659"":{""id"":128666659,""category"":""module"",""name"":""Int_FuelScoop_Size8_Class2"",""cost"":4516291,""sku"":null},""128666658"":{""id"":128666658,""category"":""module"",""name"":""Int_FuelScoop_Size7_Class2"",""cost"":1424698,""sku"":null},""128666651"":{""id"":128666651,""category"":""module"",""name"":""Int_FuelScoop_Size8_Class1"",""cost"":1083910,""sku"":null},""128666650"":{""id"":128666650,""category"":""module"",""name"":""Int_FuelScoop_Size7_Class1"",""cost"":341927,""sku"":null},""128667605"":{""id"":128667605,""category"":""module"",""name"":""Int_Repairer_Size8_Class1"",""cost"":612220,""sku"":null},""128667611"":{""id"":128667611,""category"":""module"",""name"":""Int_Repairer_Size6_Class2"",""cost"":566870,""sku"":null},""128667604"":{""id"":128667604,""category"":""module"",""name"":""Int_Repairer_Size7_Class1"",""cost"":340122,""sku"":null},""128667610"":{""id"":128667610,""category"":""module"",""name"":""Int_Repairer_Size5_Class2"",""cost"":314928,""sku"":null},""128667616"":{""id"":128667616,""category"":""module"",""name"":""Int_Repairer_Size3_Class3"",""cost"":291600,""sku"":null},""128667623"":{""id"":128667623,""category"":""module"",""name"":""Int_Repairer_Size2_Class4"",""cost"":486000,""sku"":null},""128667615"":{""id"":128667615,""category"":""module"",""name"":""Int_Repairer_Size2_Class3"",""cost"":162000,""sku"":null},""128667622"":{""id"":128667622,""category"":""module"",""name"":""Int_Repairer_Size1_Class4"",""cost"":270000,""sku"":null},""128667609"":{""id"":128667609,""category"":""module"",""name"":""Int_Repairer_Size4_Class2"",""cost"":174960,""sku"":null},""128667603"":{""id"":128667603,""category"":""module"",""name"":""Int_Repairer_Size6_Class1"",""cost"":188957,""sku"":null},""128667602"":{""id"":128667602,""category"":""module"",""name"":""Int_Repairer_Size5_Class1"",""cost"":104976,""sku"":null},""128671278"":{""id"":128671278,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size3_Class5"",""cost"":86400,""sku"":null},""128671273"":{""id"":128671273,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size1_Class5"",""cost"":9600,""sku"":null},""128671287"":{""id"":128671287,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size7_Class4"",""cost"":3499200,""sku"":null},""128671282"":{""id"":128671282,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size5_Class4"",""cost"":388800,""sku"":null},""128671286"":{""id"":128671286,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size7_Class3"",""cost"":1749600,""sku"":null},""128671277"":{""id"":128671277,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size3_Class4"",""cost"":43200,""sku"":null},""128671285"":{""id"":128671285,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size7_Class2"",""cost"":874800,""sku"":null},""128064337"":{""id"":128064337,""category"":""module"",""name"":""Int_ShieldCellBank_Size8_Class5"",""cost"":27249391,""sku"":null},""128064336"":{""id"":128064336,""category"":""module"",""name"":""Int_ShieldCellBank_Size8_Class4"",""cost"":10899756,""sku"":null},""128064331"":{""id"":128064331,""category"":""module"",""name"":""Int_ShieldCellBank_Size7_Class4"",""cost"":3892770,""sku"":null},""128064330"":{""id"":128064330,""category"":""module"",""name"":""Int_ShieldCellBank_Size7_Class3"",""cost"":1557108,""sku"":null},""128064334"":{""id"":128064334,""category"":""module"",""name"":""Int_ShieldCellBank_Size8_Class2"",""cost"":1743961,""sku"":null},""128064329"":{""id"":128064329,""category"":""module"",""name"":""Int_ShieldCellBank_Size7_Class2"",""cost"":622843,""sku"":null},""128064333"":{""id"":128064333,""category"":""module"",""name"":""Int_ShieldCellBank_Size8_Class1"",""cost"":697584,""sku"":null},""128064328"":{""id"":128064328,""category"":""module"",""name"":""Int_ShieldCellBank_Size7_Class1"",""cost"":249137,""sku"":null},""128066550"":{""id"":128066550,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size7_Class4"",""cost"":3499200,""sku"":null},""128066549"":{""id"":128066549,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size7_Class3"",""cost"":1749600,""sku"":null},""128066548"":{""id"":128066548,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size7_Class2"",""cost"":874800,""sku"":null},""128066547"":{""id"":128066547,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size7_Class1"",""cost"":437400,""sku"":null},""128066543"":{""id"":128066543,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size5_Class2"",""cost"":97200,""sku"":null},""128066542"":{""id"":128066542,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size5_Class1"",""cost"":48600,""sku"":null},""128064177"":{""id"":128064177,""category"":""module"",""name"":""Int_LifeSupport_Size8_Class5"",""cost"":27249391,""sku"":null},""128064172"":{""id"":128064172,""category"":""module"",""name"":""Int_LifeSupport_Size7_Class5"",""cost"":9731925,""sku"":null},""128064171"":{""id"":128064171,""category"":""module"",""name"":""Int_LifeSupport_Size7_Class4"",""cost"":3892770,""sku"":null},""128064170"":{""id"":128064170,""category"":""module"",""name"":""Int_LifeSupport_Size7_Class3"",""cost"":1557108,""sku"":null},""128064174"":{""id"":128064174,""category"":""module"",""name"":""Int_LifeSupport_Size8_Class2"",""cost"":1743961,""sku"":null},""128064173"":{""id"":128064173,""category"":""module"",""name"":""Int_LifeSupport_Size8_Class1"",""cost"":697584,""sku"":null},""128064168"":{""id"":128064168,""category"":""module"",""name"":""Int_LifeSupport_Size7_Class1"",""cost"":249137,""sku"":null},""128064061"":{""id"":128064061,""category"":""module"",""name"":""Int_Powerplant_Size7_Class4"",""cost"":17096371,""sku"":null},""128064065"":{""id"":128064065,""category"":""module"",""name"":""Int_Powerplant_Size8_Class3"",""cost"":18065165,""sku"":null},""128064060"":{""id"":128064060,""category"":""module"",""name"":""Int_Powerplant_Size7_Class3"",""cost"":5698790,""sku"":null},""128064064"":{""id"":128064064,""category"":""module"",""name"":""Int_Powerplant_Size8_Class2"",""cost"":6021722,""sku"":null},""128064059"":{""id"":128064059,""category"":""module"",""name"":""Int_Powerplant_Size7_Class2"",""cost"":1899597,""sku"":null},""128064063"":{""id"":128064063,""category"":""module"",""name"":""Int_Powerplant_Size8_Class1"",""cost"":2007241,""sku"":null},""128064058"":{""id"":128064058,""category"":""module"",""name"":""Int_Powerplant_Size7_Class1"",""cost"":633199,""sku"":null},""128064097"":{""id"":128064097,""category"":""module"",""name"":""Int_Engine_Size7_Class5"",""cost"":51289112,""sku"":null},""128064101"":{""id"":128064101,""category"":""module"",""name"":""Int_Engine_Size8_Class4"",""cost"":54195495,""sku"":null},""128064096"":{""id"":128064096,""category"":""module"",""name"":""Int_Engine_Size7_Class4"",""cost"":17096371,""sku"":null},""128064094"":{""id"":128064094,""category"":""module"",""name"":""Int_Engine_Size7_Class2"",""cost"":1899597,""sku"":null},""128064098"":{""id"":128064098,""category"":""module"",""name"":""Int_Engine_Size8_Class1"",""cost"":2007241,""sku"":null},""128064093"":{""id"":128064093,""category"":""module"",""name"":""Int_Engine_Size7_Class1"",""cost"":633199,""sku"":null},""128671232"":{""id"":128671232,""category"":""module"",""name"":""Int_DroneControl_Collection_Size1_Class4"",""cost"":4800,""sku"":null},""128671244"":{""id"":128671244,""category"":""module"",""name"":""Int_DroneControl_Collection_Size7_Class1"",""cost"":437400,""sku"":null},""128671240"":{""id"":128671240,""category"":""module"",""name"":""Int_DroneControl_Collection_Size5_Class2"",""cost"":97200,""sku"":null},""128671236"":{""id"":128671236,""category"":""module"",""name"":""Int_DroneControl_Collection_Size3_Class3"",""cost"":21600,""sku"":null},""128671231"":{""id"":128671231,""category"":""module"",""name"":""Int_DroneControl_Collection_Size1_Class3"",""cost"":2400,""sku"":null},""128671239"":{""id"":128671239,""category"":""module"",""name"":""Int_DroneControl_Collection_Size5_Class1"",""cost"":48600,""sku"":null},""128671235"":{""id"":128671235,""category"":""module"",""name"":""Int_DroneControl_Collection_Size3_Class2"",""cost"":10800,""sku"":null},""128671230"":{""id"":128671230,""category"":""module"",""name"":""Int_DroneControl_Collection_Size1_Class2"",""cost"":1200,""sku"":null},""128671234"":{""id"":128671234,""category"":""module"",""name"":""Int_DroneControl_Collection_Size3_Class1"",""cost"":5400,""sku"":null},""128064136"":{""id"":128064136,""category"":""module"",""name"":""Int_Hyperdrive_Size8_Class4"",""cost"":54195495,""sku"":null},""128064131"":{""id"":128064131,""category"":""module"",""name"":""Int_Hyperdrive_Size7_Class4"",""cost"":17096371,""sku"":null},""128064135"":{""id"":128064135,""category"":""module"",""name"":""Int_Hyperdrive_Size8_Class3"",""cost"":18065165,""sku"":null},""128064130"":{""id"":128064130,""category"":""module"",""name"":""Int_Hyperdrive_Size7_Class3"",""cost"":5698790,""sku"":null},""128064134"":{""id"":128064134,""category"":""module"",""name"":""Int_Hyperdrive_Size8_Class2"",""cost"":6021722,""sku"":null},""128064129"":{""id"":128064129,""category"":""module"",""name"":""Int_Hyperdrive_Size7_Class2"",""cost"":1899597,""sku"":null},""128064133"":{""id"":128064133,""category"":""module"",""name"":""Int_Hyperdrive_Size8_Class1"",""cost"":2007241,""sku"":null},""128064128"":{""id"":128064128,""category"":""module"",""name"":""Int_Hyperdrive_Size7_Class1"",""cost"":633199,""sku"":null},""128064237"":{""id"":128064237,""category"":""module"",""name"":""Int_Sensors_Size4_Class5"",""cost"":443328,""sku"":null},""128064236"":{""id"":128064236,""category"":""module"",""name"":""Int_Sensors_Size4_Class4"",""cost"":177331,""sku"":null},""128064244"":{""id"":128064244,""category"":""module"",""name"":""Int_Sensors_Size6_Class2"",""cost"":222444,""sku"":null},""128064240"":{""id"":128064240,""category"":""module"",""name"":""Int_Sensors_Size5_Class3"",""cost"":198611,""sku"":null},""128064235"":{""id"":128064235,""category"":""module"",""name"":""Int_Sensors_Size4_Class3"",""cost"":70932,""sku"":null},""128064239"":{""id"":128064239,""category"":""module"",""name"":""Int_Sensors_Size5_Class2"",""cost"":79444,""sku"":null},""128064243"":{""id"":128064243,""category"":""module"",""name"":""Int_Sensors_Size6_Class1"",""cost"":88978,""sku"":null},""128064207"":{""id"":128064207,""category"":""module"",""name"":""Int_PowerDistributor_Size6_Class5"",""cost"":3475688,""sku"":null},""128064197"":{""id"":128064197,""category"":""module"",""name"":""Int_PowerDistributor_Size4_Class5"",""cost"":443328,""sku"":null},""128064206"":{""id"":128064206,""category"":""module"",""name"":""Int_PowerDistributor_Size6_Class4"",""cost"":1390275,""sku"":null},""128064196"":{""id"":128064196,""category"":""module"",""name"":""Int_PowerDistributor_Size4_Class4"",""cost"":177331,""sku"":null},""128064204"":{""id"":128064204,""category"":""module"",""name"":""Int_PowerDistributor_Size6_Class2"",""cost"":222444,""sku"":null},""128064195"":{""id"":128064195,""category"":""module"",""name"":""Int_PowerDistributor_Size4_Class3"",""cost"":70932,""sku"":null},""128064199"":{""id"":128064199,""category"":""module"",""name"":""Int_PowerDistributor_Size5_Class2"",""cost"":79444,""sku"":null},""128668546"":{""id"":128668546,""category"":""module"",""name"":""Int_HullReinforcement_Size5_Class2"",""cost"":450000,""sku"":null},""128668544"":{""id"":128668544,""category"":""module"",""name"":""Int_HullReinforcement_Size4_Class2"",""cost"":195000,""sku"":null},""128668542"":{""id"":128668542,""category"":""module"",""name"":""Int_HullReinforcement_Size3_Class2"",""cost"":84000,""sku"":null},""128668540"":{""id"":128668540,""category"":""module"",""name"":""Int_HullReinforcement_Size2_Class2"",""cost"":36000,""sku"":null},""128668538"":{""id"":128668538,""category"":""module"",""name"":""Int_HullReinforcement_Size1_Class2"",""cost"":15000,""sku"":null},""128666680"":{""id"":128666680,""category"":""module"",""name"":""Int_FuelScoop_Size5_Class5"",""cost"":9073694,""sku"":null},""128666665"":{""id"":128666665,""category"":""module"",""name"":""Int_FuelScoop_Size6_Class3"",""cost"":1797726,""sku"":null},""128666672"":{""id"":128666672,""category"":""module"",""name"":""Int_FuelScoop_Size5_Class4"",""cost"":2268424,""sku"":null},""128666671"":{""id"":128666671,""category"":""module"",""name"":""Int_FuelScoop_Size4_Class4"",""cost"":715591,""sku"":null},""128666664"":{""id"":128666664,""category"":""module"",""name"":""Int_FuelScoop_Size5_Class3"",""cost"":567106,""sku"":null},""128666663"":{""id"":128666663,""category"":""module"",""name"":""Int_FuelScoop_Size4_Class3"",""cost"":178898,""sku"":null},""128668545"":{""id"":128668545,""category"":""module"",""name"":""Int_HullReinforcement_Size5_Class1"",""cost"":150000,""sku"":null},""128668543"":{""id"":128668543,""category"":""module"",""name"":""Int_HullReinforcement_Size4_Class1"",""cost"":65000,""sku"":null},""128668541"":{""id"":128668541,""category"":""module"",""name"":""Int_HullReinforcement_Size3_Class1"",""cost"":28000,""sku"":null},""128668539"":{""id"":128668539,""category"":""module"",""name"":""Int_HullReinforcement_Size2_Class1"",""cost"":12000,""sku"":null},""128668537"":{""id"":128668537,""category"":""module"",""name"":""Int_HullReinforcement_Size1_Class1"",""cost"":5000,""sku"":null},""128064317"":{""id"":128064317,""category"":""module"",""name"":""Int_ShieldCellBank_Size4_Class5"",""cost"":443328,""sku"":null},""128064326"":{""id"":128064326,""category"":""module"",""name"":""Int_ShieldCellBank_Size6_Class4"",""cost"":1390275,""sku"":null},""128064321"":{""id"":128064321,""category"":""module"",""name"":""Int_ShieldCellBank_Size5_Class4"",""cost"":496527,""sku"":null},""128064316"":{""id"":128064316,""category"":""module"",""name"":""Int_ShieldCellBank_Size4_Class4"",""cost"":177331,""sku"":null},""128064325"":{""id"":128064325,""category"":""module"",""name"":""Int_ShieldCellBank_Size6_Class3"",""cost"":556110,""sku"":null},""128671248"":{""id"":128671248,""category"":""module"",""name"":""Int_DroneControl_Collection_Size7_Class5"",""cost"":6998400,""sku"":null},""128671243"":{""id"":128671243,""category"":""module"",""name"":""Int_DroneControl_Collection_Size5_Class5"",""cost"":777600,""sku"":null},""128671233"":{""id"":128671233,""category"":""module"",""name"":""Int_DroneControl_Collection_Size1_Class5"",""cost"":9600,""sku"":null},""128671247"":{""id"":128671247,""category"":""module"",""name"":""Int_DroneControl_Collection_Size7_Class4"",""cost"":3499200,""sku"":null},""128668536"":{""id"":128668536,""category"":""module"",""name"":""Hpt_ShieldBooster_Size0_Class5"",""cost"":281000,""sku"":null},""128668535"":{""id"":128668535,""category"":""module"",""name"":""Hpt_ShieldBooster_Size0_Class4"",""cost"":122000,""sku"":null},""128668534"":{""id"":128668534,""category"":""module"",""name"":""Hpt_ShieldBooster_Size0_Class3"",""cost"":53000,""sku"":null},""128668533"":{""id"":128668533,""category"":""module"",""name"":""Hpt_ShieldBooster_Size0_Class2"",""cost"":23000,""sku"":null},""128064167"":{""id"":128064167,""category"":""module"",""name"":""Int_LifeSupport_Size6_Class5"",""cost"":3475688,""sku"":null},""128064157"":{""id"":128064157,""category"":""module"",""name"":""Int_LifeSupport_Size4_Class5"",""cost"":443328,""sku"":null},""128064166"":{""id"":128064166,""category"":""module"",""name"":""Int_LifeSupport_Size6_Class4"",""cost"":1390275,""sku"":null},""128064052"":{""id"":128064052,""category"":""module"",""name"":""Int_Powerplant_Size5_Class5"",""cost"":5103953,""sku"":null},""128064047"":{""id"":128064047,""category"":""module"",""name"":""Int_Powerplant_Size4_Class5"",""cost"":1610080,""sku"":null},""128064287"":{""id"":128064287,""category"":""module"",""name"":""Int_ShieldGenerator_Size6_Class5"",""cost"":16179531,""sku"":null},""128049282"":{""id"":128049282,""category"":""module"",""name"":""CobraMkIII_Armour_Grade3"",""cost"":314746,""sku"":null},""128049281"":{""id"":128049281,""category"":""module"",""name"":""CobraMkIII_Armour_Grade2"",""cost"":139887,""sku"":null},""128049280"":{""id"":128049280,""category"":""module"",""name"":""CobraMkIII_Armour_Grade1"",""cost"":0,""sku"":null},""128049283"":{""id"":128049283,""category"":""module"",""name"":""CobraMkIII_Armour_Mirrored"",""cost"":734407,""sku"":null},""128049284"":{""id"":128049284,""category"":""module"",""name"":""CobraMkIII_Armour_Reactive"",""cost"":824285,""sku"":null},""128672266"":{""id"":128672266,""category"":""module"",""name"":""CobraMkIV_Armour_Grade3"",""cost"":688246,""sku"":null},""128672265"":{""id"":128672265,""category"":""module"",""name"":""CobraMkIV_Armour_Grade2"",""cost"":305887,""sku"":null},""128672264"":{""id"":128672264,""category"":""module"",""name"":""CobraMkIV_Armour_Grade1"",""cost"":0,""sku"":null},""128672267"":{""id"":128672267,""category"":""module"",""name"":""CobraMkIV_Armour_Mirrored"",""cost"":1605907,""sku"":null},""128672268"":{""id"":128672268,""category"":""module"",""name"":""CobraMkIV_Armour_Reactive"",""cost"":1802440,""sku"":null},""128049288"":{""id"":128049288,""category"":""module"",""name"":""Type6_Armour_Grade3"",""cost"":941350,""sku"":null},""128049287"":{""id"":128049287,""category"":""module"",""name"":""Type6_Armour_Grade2"",""cost"":418378,""sku"":null},""128049286"":{""id"":128049286,""category"":""module"",""name"":""Type6_Armour_Grade1"",""cost"":0,""sku"":null},""128049289"":{""id"":128049289,""category"":""module"",""name"":""Type6_Armour_Mirrored"",""cost"":2224725,""sku"":null},""128049290"":{""id"":128049290,""category"":""module"",""name"":""Type6_Armour_Reactive"",""cost"":2465292,""sku"":null},""128666700"":{""id"":128666700,""category"":""module"",""name"":""Int_Refinery_Size1_Class5"",""cost"":486000,""sku"":null},""128064251"":{""id"":128064251,""category"":""module"",""name"":""Int_Sensors_Size7_Class4"",""cost"":3892770,""sku"":null},""128064250"":{""id"":128064250,""category"":""module"",""name"":""Int_Sensors_Size7_Class3"",""cost"":1557108,""sku"":null},""128666677"":{""id"":128666677,""category"":""module"",""name"":""Int_FuelScoop_Size2_Class5"",""cost"":284844,""sku"":null},""128666669"":{""id"":128666669,""category"":""module"",""name"":""Int_FuelScoop_Size2_Class4"",""cost"":71211,""sku"":null},""128064218"":{""id"":128064218,""category"":""module"",""name"":""Int_Sensors_Size1_Class1"",""cost"":517,""sku"":null},""128064246"":{""id"":128064246,""category"":""module"",""name"":""Int_Sensors_Size6_Class4"",""cost"":1390275,""sku"":null},""128064238"":{""id"":128064238,""category"":""module"",""name"":""Int_Sensors_Size5_Class1"",""cost"":31778,""sku"":null},""128064234"":{""id"":128064234,""category"":""module"",""name"":""Int_Sensors_Size4_Class2"",""cost"":28373,""sku"":null},""128064233"":{""id"":128064233,""category"":""module"",""name"":""Int_Sensors_Size4_Class1"",""cost"":11349,""sku"":null},""128064202"":{""id"":128064202,""category"":""module"",""name"":""Int_PowerDistributor_Size5_Class5"",""cost"":1241317,""sku"":null},""128064201"":{""id"":128064201,""category"":""module"",""name"":""Int_PowerDistributor_Size5_Class4"",""cost"":496527,""sku"":null},""128064205"":{""id"":128064205,""category"":""module"",""name"":""Int_PowerDistributor_Size6_Class3"",""cost"":556110,""sku"":null},""128064200"":{""id"":128064200,""category"":""module"",""name"":""Int_PowerDistributor_Size5_Class3"",""cost"":198611,""sku"":null},""128064203"":{""id"":128064203,""category"":""module"",""name"":""Int_PowerDistributor_Size6_Class1"",""cost"":88978,""sku"":null},""128064198"":{""id"":128064198,""category"":""module"",""name"":""Int_PowerDistributor_Size5_Class1"",""cost"":31778,""sku"":null},""128064194"":{""id"":128064194,""category"":""module"",""name"":""Int_PowerDistributor_Size4_Class2"",""cost"":28373,""sku"":null},""128064193"":{""id"":128064193,""category"":""module"",""name"":""Int_PowerDistributor_Size4_Class1"",""cost"":11349,""sku"":null},""128666681"":{""id"":128666681,""category"":""module"",""name"":""Int_FuelScoop_Size6_Class5"",""cost"":28763610,""sku"":null},""128666673"":{""id"":128666673,""category"":""module"",""name"":""Int_FuelScoop_Size6_Class4"",""cost"":7190903,""sku"":null},""128666657"":{""id"":128666657,""category"":""module"",""name"":""Int_FuelScoop_Size6_Class2"",""cost"":449431,""sku"":null},""128666656"":{""id"":128666656,""category"":""module"",""name"":""Int_FuelScoop_Size5_Class2"",""cost"":141776,""sku"":null},""128666649"":{""id"":128666649,""category"":""module"",""name"":""Int_FuelScoop_Size6_Class1"",""cost"":107864,""sku"":null},""128666655"":{""id"":128666655,""category"":""module"",""name"":""Int_FuelScoop_Size4_Class2"",""cost"":44724,""sku"":null},""128666648"":{""id"":128666648,""category"":""module"",""name"":""Int_FuelScoop_Size5_Class1"",""cost"":34026,""sku"":null},""128666647"":{""id"":128666647,""category"":""module"",""name"":""Int_FuelScoop_Size4_Class1"",""cost"":10734,""sku"":null},""128064142"":{""id"":128064142,""category"":""module"",""name"":""Int_LifeSupport_Size1_Class5"",""cost"":20195,""sku"":null},""128671263"":{""id"":128671263,""category"":""module"",""name"":""Int_DroneControl_FuelTransfer_Size5_Class5"",""cost"":777600,""sku"":null},""128064324"":{""id"":128064324,""category"":""module"",""name"":""Int_ShieldCellBank_Size6_Class2"",""cost"":222444,""sku"":null},""128064315"":{""id"":128064315,""category"":""module"",""name"":""Int_ShieldCellBank_Size4_Class3"",""cost"":70932,""sku"":null},""128064323"":{""id"":128064323,""category"":""module"",""name"":""Int_ShieldCellBank_Size6_Class1"",""cost"":88978,""sku"":null},""128064319"":{""id"":128064319,""category"":""module"",""name"":""Int_ShieldCellBank_Size5_Class2"",""cost"":79444,""sku"":null},""128064318"":{""id"":128064318,""category"":""module"",""name"":""Int_ShieldCellBank_Size5_Class1"",""cost"":31778,""sku"":null},""128064314"":{""id"":128064314,""category"":""module"",""name"":""Int_ShieldCellBank_Size4_Class2"",""cost"":28373,""sku"":null},""128064313"":{""id"":128064313,""category"":""module"",""name"":""Int_ShieldCellBank_Size4_Class1"",""cost"":11349,""sku"":null},""128064161"":{""id"":128064161,""category"":""module"",""name"":""Int_LifeSupport_Size5_Class4"",""cost"":496527,""sku"":null},""128064156"":{""id"":128064156,""category"":""module"",""name"":""Int_LifeSupport_Size4_Class4"",""cost"":177331,""sku"":null},""128064160"":{""id"":128064160,""category"":""module"",""name"":""Int_LifeSupport_Size5_Class3"",""cost"":198611,""sku"":null},""128064164"":{""id"":128064164,""category"":""module"",""name"":""Int_LifeSupport_Size6_Class2"",""cost"":222444,""sku"":null},""128064155"":{""id"":128064155,""category"":""module"",""name"":""Int_LifeSupport_Size4_Class3"",""cost"":70932,""sku"":null},""128064163"":{""id"":128064163,""category"":""module"",""name"":""Int_LifeSupport_Size6_Class1"",""cost"":88978,""sku"":null},""128064159"":{""id"":128064159,""category"":""module"",""name"":""Int_LifeSupport_Size5_Class2"",""cost"":79444,""sku"":null},""128064158"":{""id"":128064158,""category"":""module"",""name"":""Int_LifeSupport_Size5_Class1"",""cost"":31778,""sku"":null},""128668532"":{""id"":128668532,""category"":""module"",""name"":""Hpt_ShieldBooster_Size0_Class1"",""cost"":10000,""sku"":null},""128064057"":{""id"":128064057,""category"":""module"",""name"":""Int_Powerplant_Size6_Class5"",""cost"":16179531,""sku"":null},""128064051"":{""id"":128064051,""category"":""module"",""name"":""Int_Powerplant_Size5_Class4"",""cost"":1701318,""sku"":null},""128064046"":{""id"":128064046,""category"":""module"",""name"":""Int_Powerplant_Size4_Class4"",""cost"":536693,""sku"":null},""128064055"":{""id"":128064055,""category"":""module"",""name"":""Int_Powerplant_Size6_Class3"",""cost"":1797726,""sku"":null},""128064050"":{""id"":128064050,""category"":""module"",""name"":""Int_Powerplant_Size5_Class3"",""cost"":567106,""sku"":null},""128064054"":{""id"":128064054,""category"":""module"",""name"":""Int_Powerplant_Size6_Class2"",""cost"":599242,""sku"":null},""128064045"":{""id"":128064045,""category"":""module"",""name"":""Int_Powerplant_Size4_Class3"",""cost"":178898,""sku"":null},""128064087"":{""id"":128064087,""category"":""module"",""name"":""Int_Engine_Size5_Class5"",""cost"":5103953,""sku"":null},""128064082"":{""id"":128064082,""category"":""module"",""name"":""Int_Engine_Size4_Class5"",""cost"":1610080,""sku"":null},""128064091"":{""id"":128064091,""category"":""module"",""name"":""Int_Engine_Size6_Class4"",""cost"":5393177,""sku"":null},""128064086"":{""id"":128064086,""category"":""module"",""name"":""Int_Engine_Size5_Class4"",""cost"":1701318,""sku"":null},""128064081"":{""id"":128064081,""category"":""module"",""name"":""Int_Engine_Size4_Class4"",""cost"":536693,""sku"":null},""128064090"":{""id"":128064090,""category"":""module"",""name"":""Int_Engine_Size6_Class3"",""cost"":1797726,""sku"":null},""128064089"":{""id"":128064089,""category"":""module"",""name"":""Int_Engine_Size6_Class2"",""cost"":599242,""sku"":null},""128064122"":{""id"":128064122,""category"":""module"",""name"":""Int_Hyperdrive_Size5_Class5"",""cost"":5103953,""sku"":null},""128064117"":{""id"":128064117,""category"":""module"",""name"":""Int_Hyperdrive_Size4_Class5"",""cost"":1610080,""sku"":null},""128064121"":{""id"":128064121,""category"":""module"",""name"":""Int_Hyperdrive_Size5_Class4"",""cost"":1701318,""sku"":null},""128064116"":{""id"":128064116,""category"":""module"",""name"":""Int_Hyperdrive_Size4_Class4"",""cost"":536693,""sku"":null},""128064125"":{""id"":128064125,""category"":""module"",""name"":""Int_Hyperdrive_Size6_Class3"",""cost"":1797726,""sku"":null},""128064124"":{""id"":128064124,""category"":""module"",""name"":""Int_Hyperdrive_Size6_Class2"",""cost"":599242,""sku"":null},""128666721"":{""id"":128666721,""category"":""module"",""name"":""Int_FSDInterdictor_Size2_Class5"",""cost"":2721600,""sku"":null},""128064277"":{""id"":128064277,""category"":""module"",""name"":""Int_ShieldGenerator_Size4_Class5"",""cost"":1610080,""sku"":null},""128064286"":{""id"":128064286,""category"":""module"",""name"":""Int_ShieldGenerator_Size6_Class4"",""cost"":5393177,""sku"":null},""128064281"":{""id"":128064281,""category"":""module"",""name"":""Int_ShieldGenerator_Size5_Class4"",""cost"":1701318,""sku"":null},""128064276"":{""id"":128064276,""category"":""module"",""name"":""Int_ShieldGenerator_Size4_Class4"",""cost"":536693,""sku"":null},""128671336"":{""id"":128671336,""category"":""module"",""name"":""Int_ShieldGenerator_Size6_Class3_Fast"",""cost"":2696589,""sku"":null},""128064285"":{""id"":128064285,""category"":""module"",""name"":""Int_ShieldGenerator_Size6_Class3"",""cost"":1797726,""sku"":null},""128671229"":{""id"":128671229,""category"":""module"",""name"":""Int_DroneControl_Collection_Size1_Class1"",""cost"":600,""sku"":null},""128064216"":{""id"":128064216,""category"":""module"",""name"":""Int_PowerDistributor_Size8_Class4"",""cost"":10899756,""sku"":null},""128064211"":{""id"":128064211,""category"":""module"",""name"":""Int_PowerDistributor_Size7_Class4"",""cost"":3892770,""sku"":null},""128064215"":{""id"":128064215,""category"":""module"",""name"":""Int_PowerDistributor_Size8_Class3"",""cost"":4359903,""sku"":null},""128064209"":{""id"":128064209,""category"":""module"",""name"":""Int_PowerDistributor_Size7_Class2"",""cost"":622843,""sku"":null},""128666703"":{""id"":128666703,""category"":""module"",""name"":""Int_Refinery_Size4_Class5"",""cost"":4500846,""sku"":null},""128666702"":{""id"":128666702,""category"":""module"",""name"":""Int_Refinery_Size3_Class5"",""cost"":2143260,""sku"":null},""128666698"":{""id"":128666698,""category"":""module"",""name"":""Int_Refinery_Size3_Class4"",""cost"":714420,""sku"":null},""128667637"":{""id"":128667637,""category"":""module"",""name"":""Int_Repairer_Size8_Class5"",""cost"":49589823,""sku"":null},""128667628"":{""id"":128667628,""category"":""module"",""name"":""Int_Repairer_Size7_Class4"",""cost"":9183300,""sku"":null},""128667635"":{""id"":128667635,""category"":""module"",""name"":""Int_Repairer_Size6_Class5"",""cost"":15305501,""sku"":null},""128667634"":{""id"":128667634,""category"":""module"",""name"":""Int_Repairer_Size5_Class5"",""cost"":8503056,""sku"":null},""128671288"":{""id"":128671288,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size7_Class5"",""cost"":6998400,""sku"":null},""128671281"":{""id"":128671281,""category"":""module"",""name"":""Int_DroneControl_Prospector_Size5_Class3"",""cost"":194400,""sku"":null},""128064312"":{""id"":128064312,""category"":""module"",""name"":""Int_ShieldCellBank_Size3_Class5"",""cost"":158331,""sku"":null},""128064311"":{""id"":128064311,""category"":""module"",""name"":""Int_ShieldCellBank_Size3_Class4"",""cost"":63333,""sku"":null},""128666675"":{""id"":128666675,""category"":""module"",""name"":""Int_FuelScoop_Size8_Class4"",""cost"":72260660,""sku"":null},""128671238"":{""id"":128671238,""category"":""module"",""name"":""Int_DroneControl_Collection_Size3_Class5"",""cost"":86400,""sku"":null},""128671242"":{""id"":128671242,""category"":""module"",""name"":""Int_DroneControl_Collection_Size5_Class4"",""cost"":388800,""sku"":null},""128671246"":{""id"":128671246,""category"":""module"",""name"":""Int_DroneControl_Collection_Size7_Class3"",""cost"":1749600,""sku"":null},""128066551"":{""id"":128066551,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size7_Class5"",""cost"":6998400,""sku"":null},""128066546"":{""id"":128066546,""category"":""module"",""name"":""Int_DroneControl_ResourceSiphon_Size5_Class5"",""cost"":777600,""sku"":null},""128682043"":{""id"":128682043,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Metallic"",""cost"":0,""sku"":null},""128667727"":{""id"":128667727,""category"":""paintjob"",""name"":""paintjob_CobraMkiii_Default_52"",""cost"":0,""sku"":null},""128066428"":{""id"":128066428,""category"":""paintjob"",""name"":""paintjob_cobramkiii_wireframe_01"",""cost"":0,""sku"":null},""128670861"":{""id"":128670861,""category"":""paintjob"",""name"":""PaintJob_CobraMkIII_Onionhead1_01"",""cost"":0,""sku"":null},""128671133"":{""id"":128671133,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_green"",""cost"":0,""sku"":null},""128671134"":{""id"":128671134,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_blue"",""cost"":0,""sku"":null},""128671135"":{""id"":128671135,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_orange"",""cost"":0,""sku"":null},""128671136"":{""id"":128671136,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_red"",""cost"":0,""sku"":null},""128671137"":{""id"":128671137,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_purple"",""cost"":0,""sku"":null},""128671138"":{""id"":128671138,""category"":""paintjob"",""name"":""paintjob_cobramkiii_vibrant_yellow"",""cost"":0,""sku"":null},""128667638"":{""id"":128667638,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Merc"",""cost"":0,""sku"":null},""128066405"":{""id"":128066405,""category"":""paintjob"",""name"":""paintjob_eagle_stripe1_02"",""cost"":0,""sku"":null},""128066406"":{""id"":128066406,""category"":""paintjob"",""name"":""paintjob_eagle_doublestripe_01"",""cost"":0,""sku"":null},""128066416"":{""id"":128066416,""category"":""paintjob"",""name"":""paintjob_eagle_thirds_01"",""cost"":0,""sku"":null},""128066419"":{""id"":128066419,""category"":""paintjob"",""name"":""paintjob_eagle_stripe1_03"",""cost"":0,""sku"":null},""128066420"":{""id"":128066420,""category"":""paintjob"",""name"":""paintjob_eagle_thirds_02"",""cost"":0,""sku"":null},""128066430"":{""id"":128066430,""category"":""paintjob"",""name"":""paintjob_eagle_stripe1_01"",""cost"":0,""sku"":null},""128066436"":{""id"":128066436,""category"":""paintjob"",""name"":""paintjob_eagle_camo_03"",""cost"":0,""sku"":null},""128066437"":{""id"":128066437,""category"":""paintjob"",""name"":""paintjob_eagle_thirds_03"",""cost"":0,""sku"":null},""128066441"":{""id"":128066441,""category"":""paintjob"",""name"":""paintjob_eagle_camo_02"",""cost"":0,""sku"":null},""128066449"":{""id"":128066449,""category"":""paintjob"",""name"":""paintjob_eagle_doublestripe_02"",""cost"":0,""sku"":null},""128066453"":{""id"":128066453,""category"":""paintjob"",""name"":""paintjob_eagle_doublestripe_03"",""cost"":0,""sku"":null},""128066456"":{""id"":128066456,""category"":""paintjob"",""name"":""paintjob_eagle_camo_01"",""cost"":0,""sku"":null},""128671139"":{""id"":128671139,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_green"",""cost"":0,""sku"":null},""128671140"":{""id"":128671140,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_blue"",""cost"":0,""sku"":null},""128671141"":{""id"":128671141,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_orange"",""cost"":0,""sku"":null},""128671142"":{""id"":128671142,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_red"",""cost"":0,""sku"":null},""128671143"":{""id"":128671143,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_purple"",""cost"":0,""sku"":null},""128671144"":{""id"":128671144,""category"":""paintjob"",""name"":""paintjob_eagle_vibrant_yellow"",""cost"":0,""sku"":null},""128671777"":{""id"":128671777,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Desert_Sand"",""cost"":0,""sku"":null},""128671778"":{""id"":128671778,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Earth_Yellow"",""cost"":0,""sku"":null},""128672802"":{""id"":128672802,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_BlackFriday_01"",""cost"":0,""sku"":null},""128671779"":{""id"":128671779,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Dark_Green"",""cost"":0,""sku"":null},""128671780"":{""id"":128671780,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Forest_Green"",""cost"":0,""sku"":null},""128671781"":{""id"":128671781,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Sand"",""cost"":0,""sku"":null},""128671782"":{""id"":128671782,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Militaire_Earth_Red"",""cost"":0,""sku"":null},""128672426"":{""id"":128672426,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_SpecialEffect_01"",""cost"":0,""sku"":null},""128066404"":{""id"":128066404,""category"":""paintjob"",""name"":""paintjob_sidewinder_default_02"",""cost"":0,""sku"":null},""128066408"":{""id"":128066408,""category"":""paintjob"",""name"":""paintjob_sidewinder_default_03"",""cost"":0,""sku"":null},""128066414"":{""id"":128066414,""category"":""paintjob"",""name"":""paintjob_sidewinder_doublestripe_08"",""cost"":0,""sku"":null},""128066423"":{""id"":128066423,""category"":""paintjob"",""name"":""paintjob_sidewinder_doublestripe_05"",""cost"":0,""sku"":null},""128066431"":{""id"":128066431,""category"":""paintjob"",""name"":""paintjob_sidewinder_thirds_07"",""cost"":0,""sku"":null},""128066432"":{""id"":128066432,""category"":""paintjob"",""name"":""paintjob_sidewinder_thirds_01"",""cost"":0,""sku"":null},""128066433"":{""id"":128066433,""category"":""paintjob"",""name"":""paintjob_sidewinder_doublestripe_07"",""cost"":0,""sku"":null},""128066440"":{""id"":128066440,""category"":""paintjob"",""name"":""paintjob_sidewinder_camo_01"",""cost"":0,""sku"":null},""128066444"":{""id"":128066444,""category"":""paintjob"",""name"":""paintjob_sidewinder_thirds_06"",""cost"":0,""sku"":null},""128066447"":{""id"":128066447,""category"":""paintjob"",""name"":""paintjob_sidewinder_camo_03"",""cost"":0,""sku"":null},""128066448"":{""id"":128066448,""category"":""paintjob"",""name"":""paintjob_sidewinder_default_04"",""cost"":0,""sku"":null},""128066454"":{""id"":128066454,""category"":""paintjob"",""name"":""paintjob_sidewinder_camo_02"",""cost"":0,""sku"":null},""128671181"":{""id"":128671181,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_green"",""cost"":0,""sku"":null},""128671182"":{""id"":128671182,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_blue"",""cost"":0,""sku"":null},""128671183"":{""id"":128671183,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_orange"",""cost"":0,""sku"":null},""128671184"":{""id"":128671184,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_red"",""cost"":0,""sku"":null},""128671185"":{""id"":128671185,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_purple"",""cost"":0,""sku"":null},""128671186"":{""id"":128671186,""category"":""paintjob"",""name"":""paintjob_sidewinder_vibrant_yellow"",""cost"":0,""sku"":null},""128066407"":{""id"":128066407,""category"":""paintjob"",""name"":""paintjob_viper_flag_switzerland_01"",""cost"":0,""sku"":null},""128066409"":{""id"":128066409,""category"":""paintjob"",""name"":""paintjob_viper_flag_belgium_01"",""cost"":0,""sku"":null},""128066410"":{""id"":128066410,""category"":""paintjob"",""name"":""paintjob_viper_flag_australia_01"",""cost"":0,""sku"":null},""128066411"":{""id"":128066411,""category"":""paintjob"",""name"":""paintjob_viper_default_01"",""cost"":0,""sku"":null},""128066412"":{""id"":128066412,""category"":""paintjob"",""name"":""paintjob_viper_stripe2_02"",""cost"":0,""sku"":null},""128066413"":{""id"":128066413,""category"":""paintjob"",""name"":""paintjob_viper_flag_austria_01"",""cost"":0,""sku"":null},""128066415"":{""id"":128066415,""category"":""paintjob"",""name"":""paintjob_viper_stripe1_01"",""cost"":0,""sku"":null},""128066417"":{""id"":128066417,""category"":""paintjob"",""name"":""paintjob_viper_flag_spain_01"",""cost"":0,""sku"":null},""128066418"":{""id"":128066418,""category"":""paintjob"",""name"":""paintjob_viper_stripe1_02"",""cost"":0,""sku"":null},""128066421"":{""id"":128066421,""category"":""paintjob"",""name"":""paintjob_viper_flag_denmark_01"",""cost"":0,""sku"":null},""128066422"":{""id"":128066422,""category"":""paintjob"",""name"":""paintjob_viper_police_federation_01"",""cost"":0,""sku"":null},""128666742"":{""id"":128666742,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Hotrod_01"",""cost"":0,""sku"":null},""128666743"":{""id"":128666743,""category"":""paintjob"",""name"":""PaintJob_Sidewinder_Hotrod_03"",""cost"":0,""sku"":null},""128066424"":{""id"":128066424,""category"":""paintjob"",""name"":""paintjob_viper_flag_newzealand_01"",""cost"":0,""sku"":null},""128066425"":{""id"":128066425,""category"":""paintjob"",""name"":""paintjob_viper_flag_italy_01"",""cost"":0,""sku"":null},""128066426"":{""id"":128066426,""category"":""paintjob"",""name"":""paintjob_viper_stripe2_04"",""cost"":0,""sku"":null},""128066427"":{""id"":128066427,""category"":""paintjob"",""name"":""paintjob_viper_police_independent_01"",""cost"":0,""sku"":null},""128066429"":{""id"":128066429,""category"":""paintjob"",""name"":""paintjob_viper_default_03"",""cost"":0,""sku"":null},""128066434"":{""id"":128066434,""category"":""paintjob"",""name"":""paintjob_viper_flag_uk_01"",""cost"":0,""sku"":null},""128066435"":{""id"":128066435,""category"":""paintjob"",""name"":""paintjob_viper_flag_germany_01"",""cost"":0,""sku"":null},""128066438"":{""id"":128066438,""category"":""paintjob"",""name"":""paintjob_viper_flag_netherlands_01"",""cost"":0,""sku"":null},""128066439"":{""id"":128066439,""category"":""paintjob"",""name"":""paintjob_viper_flag_usa_01"",""cost"":0,""sku"":null},""128066442"":{""id"":128066442,""category"":""paintjob"",""name"":""paintjob_viper_flag_russia_01"",""cost"":0,""sku"":null},""128066443"":{""id"":128066443,""category"":""paintjob"",""name"":""paintjob_viper_flag_canada_01"",""cost"":0,""sku"":null},""128066445"":{""id"":128066445,""category"":""paintjob"",""name"":""paintjob_viper_flag_sweden_01"",""cost"":0,""sku"":null},""128066446"":{""id"":128066446,""category"":""paintjob"",""name"":""paintjob_viper_flag_poland_01"",""cost"":0,""sku"":null},""128066450"":{""id"":128066450,""category"":""paintjob"",""name"":""paintjob_viper_flag_finland_01"",""cost"":0,""sku"":null},""128066451"":{""id"":128066451,""category"":""paintjob"",""name"":""paintjob_viper_flag_france_01"",""cost"":0,""sku"":null},""128066452"":{""id"":128066452,""category"":""paintjob"",""name"":""paintjob_viper_police_empire_01"",""cost"":0,""sku"":null},""128066455"":{""id"":128066455,""category"":""paintjob"",""name"":""paintjob_viper_flag_norway_01"",""cost"":0,""sku"":null},""128671205"":{""id"":128671205,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_green"",""cost"":0,""sku"":null},""128671206"":{""id"":128671206,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_blue"",""cost"":0,""sku"":null},""128671207"":{""id"":128671207,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_orange"",""cost"":0,""sku"":null},""128671208"":{""id"":128671208,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_red"",""cost"":0,""sku"":null},""128671209"":{""id"":128671209,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_purple"",""cost"":0,""sku"":null},""128671210"":{""id"":128671210,""category"":""paintjob"",""name"":""paintjob_viper_vibrant_yellow"",""cost"":0,""sku"":null},""128671127"":{""id"":128671127,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_green"",""cost"":0,""sku"":null},""128671128"":{""id"":128671128,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_blue"",""cost"":0,""sku"":null},""128671129"":{""id"":128671129,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_orange"",""cost"":0,""sku"":null},""128671130"":{""id"":128671130,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_red"",""cost"":0,""sku"":null},""128671131"":{""id"":128671131,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_purple"",""cost"":0,""sku"":null},""128671132"":{""id"":128671132,""category"":""paintjob"",""name"":""paintjob_asp_vibrant_yellow"",""cost"":0,""sku"":null},""128671151"":{""id"":128671151,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_green"",""cost"":0,""sku"":null},""128671152"":{""id"":128671152,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_blue"",""cost"":0,""sku"":null},""128671153"":{""id"":128671153,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_orange"",""cost"":0,""sku"":null},""128671154"":{""id"":128671154,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_red"",""cost"":0,""sku"":null},""128671155"":{""id"":128671155,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_purple"",""cost"":0,""sku"":null},""128671156"":{""id"":128671156,""category"":""paintjob"",""name"":""paintjob_feddropship_vibrant_yellow"",""cost"":0,""sku"":null},""128671175"":{""id"":128671175,""category"":""paintjob"",""name"":""paintjob_python_vibrant_green"",""cost"":0,""sku"":null},""128671176"":{""id"":128671176,""category"":""paintjob"",""name"":""paintjob_python_vibrant_blue"",""cost"":0,""sku"":null},""128671177"":{""id"":128671177,""category"":""paintjob"",""name"":""paintjob_python_vibrant_orange"",""cost"":0,""sku"":null},""128671178"":{""id"":128671178,""category"":""paintjob"",""name"":""paintjob_python_vibrant_red"",""cost"":0,""sku"":null},""128671179"":{""id"":128671179,""category"":""paintjob"",""name"":""paintjob_python_vibrant_purple"",""cost"":0,""sku"":null},""128671180"":{""id"":128671180,""category"":""paintjob"",""name"":""paintjob_python_vibrant_yellow"",""cost"":0,""sku"":null},""128671121"":{""id"":128671121,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_green"",""cost"":0,""sku"":null},""128671122"":{""id"":128671122,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_blue"",""cost"":0,""sku"":null},""128671123"":{""id"":128671123,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_orange"",""cost"":0,""sku"":null},""128671124"":{""id"":128671124,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_red"",""cost"":0,""sku"":null},""128671125"":{""id"":128671125,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_purple"",""cost"":0,""sku"":null},""128671126"":{""id"":128671126,""category"":""paintjob"",""name"":""paintjob_adder_vibrant_yellow"",""cost"":0,""sku"":null},""128671145"":{""id"":128671145,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_green"",""cost"":0,""sku"":null},""128671146"":{""id"":128671146,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_blue"",""cost"":0,""sku"":null},""128671147"":{""id"":128671147,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_orange"",""cost"":0,""sku"":null},""128671148"":{""id"":128671148,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_red"",""cost"":0,""sku"":null},""128671149"":{""id"":128671149,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_purple"",""cost"":0,""sku"":null},""128671150"":{""id"":128671150,""category"":""paintjob"",""name"":""paintjob_empiretrader_vibrant_yellow"",""cost"":0,""sku"":null},""128671749"":{""id"":128671749,""category"":""paintjob"",""name"":""PaintJob_FerDeLance_Militaire_desert_Sand"",""cost"":0,""sku"":null},""128671157"":{""id"":128671157,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_green"",""cost"":0,""sku"":null},""128671158"":{""id"":128671158,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_blue"",""cost"":0,""sku"":null},""128671159"":{""id"":128671159,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_orange"",""cost"":0,""sku"":null},""128671160"":{""id"":128671160,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_red"",""cost"":0,""sku"":null},""128671161"":{""id"":128671161,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_purple"",""cost"":0,""sku"":null},""128671162"":{""id"":128671162,""category"":""paintjob"",""name"":""paintjob_ferdelance_vibrant_yellow"",""cost"":0,""sku"":null},""128671163"":{""id"":128671163,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_green"",""cost"":0,""sku"":null},""128671164"":{""id"":128671164,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_blue"",""cost"":0,""sku"":null},""128671165"":{""id"":128671165,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_orange"",""cost"":0,""sku"":null},""128671166"":{""id"":128671166,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_red"",""cost"":0,""sku"":null},""128671167"":{""id"":128671167,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_purple"",""cost"":0,""sku"":null},""128671168"":{""id"":128671168,""category"":""paintjob"",""name"":""paintjob_hauler_vibrant_yellow"",""cost"":0,""sku"":null},""128671169"":{""id"":128671169,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_green"",""cost"":0,""sku"":null},""128671170"":{""id"":128671170,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_blue"",""cost"":0,""sku"":null},""128671171"":{""id"":128671171,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_orange"",""cost"":0,""sku"":null},""128671172"":{""id"":128671172,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_red"",""cost"":0,""sku"":null},""128671173"":{""id"":128671173,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_purple"",""cost"":0,""sku"":null},""128671174"":{""id"":128671174,""category"":""paintjob"",""name"":""paintjob_orca_vibrant_yellow"",""cost"":0,""sku"":null},""128671187"":{""id"":128671187,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_green"",""cost"":0,""sku"":null},""128671188"":{""id"":128671188,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_blue"",""cost"":0,""sku"":null},""128671189"":{""id"":128671189,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_orange"",""cost"":0,""sku"":null},""128671190"":{""id"":128671190,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_red"",""cost"":0,""sku"":null},""128671191"":{""id"":128671191,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_purple"",""cost"":0,""sku"":null},""128671192"":{""id"":128671192,""category"":""paintjob"",""name"":""paintjob_type6_vibrant_yellow"",""cost"":0,""sku"":null},""128671193"":{""id"":128671193,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_green"",""cost"":0,""sku"":null},""128671194"":{""id"":128671194,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_blue"",""cost"":0,""sku"":null},""128671195"":{""id"":128671195,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_orange"",""cost"":0,""sku"":null},""128671196"":{""id"":128671196,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_red"",""cost"":0,""sku"":null},""128671197"":{""id"":128671197,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_purple"",""cost"":0,""sku"":null},""128671198"":{""id"":128671198,""category"":""paintjob"",""name"":""paintjob_type7_vibrant_yellow"",""cost"":0,""sku"":null},""128671199"":{""id"":128671199,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_green"",""cost"":0,""sku"":null},""128671200"":{""id"":128671200,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_blue"",""cost"":0,""sku"":null},""128671201"":{""id"":128671201,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_orange"",""cost"":0,""sku"":null},""128671202"":{""id"":128671202,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_red"",""cost"":0,""sku"":null},""128671203"":{""id"":128671203,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_purple"",""cost"":0,""sku"":null},""128671204"":{""id"":128671204,""category"":""paintjob"",""name"":""paintjob_type9_vibrant_yellow"",""cost"":0,""sku"":null},""128671211"":{""id"":128671211,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_green"",""cost"":0,""sku"":null},""128671212"":{""id"":128671212,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_blue"",""cost"":0,""sku"":null},""128671213"":{""id"":128671213,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_orange"",""cost"":0,""sku"":null},""128671214"":{""id"":128671214,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_red"",""cost"":0,""sku"":null},""128671215"":{""id"":128671215,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_purple"",""cost"":0,""sku"":null},""128671216"":{""id"":128671216,""category"":""paintjob"",""name"":""paintjob_vulture_vibrant_yellow"",""cost"":0,""sku"":null},""128667736"":{""id"":128667736,""category"":""decal"",""name"":""Decal_Combat_Mostly_Harmless"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_COMBAT_DECAL_1001""},""128667737"":{""id"":128667737,""category"":""decal"",""name"":""Decal_Combat_Novice"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_COMBAT_DECAL_1002""},""128667738"":{""id"":128667738,""category"":""decal"",""name"":""Decal_Combat_Competent"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_COMBAT_DECAL_1003""},""128667744"":{""id"":128667744,""category"":""decal"",""name"":""Decal_Trade_Mostly_Penniless"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1001""},""128667745"":{""id"":128667745,""category"":""decal"",""name"":""Decal_Trade_Peddler"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1002""},""128667746"":{""id"":128667746,""category"":""decal"",""name"":""Decal_Trade_Dealer"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1003""},""128667747"":{""id"":128667747,""category"":""decal"",""name"":""Decal_Trade_Merchant"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1004""},""128667748"":{""id"":128667748,""category"":""decal"",""name"":""Decal_Trade_Broker"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1005""},""128667749"":{""id"":128667749,""category"":""decal"",""name"":""Decal_Trade_Entrepeneur"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1006""},""128667750"":{""id"":128667750,""category"":""decal"",""name"":""Decal_Trade_Tycoon"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_TRADE_DECAL_1007""},""128667752"":{""id"":128667752,""category"":""decal"",""name"":""Decal_Explorer_Mostly_Aimless"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_EXPLORE_DECAL_1001""},""128667753"":{""id"":128667753,""category"":""decal"",""name"":""Decal_Explorer_Scout"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_EXPLORE_DECAL_1002""},""128667754"":{""id"":128667754,""category"":""decal"",""name"":""Decal_Explorer_Surveyor"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_EXPLORE_DECAL_1003""},""128667755"":{""id"":128667755,""category"":""decal"",""name"":""Decal_Explorer_Trailblazer"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_EXPLORE_DECAL_1004""},""128667756"":{""id"":128667756,""category"":""decal"",""name"":""Decal_Explorer_Pathfinder"",""cost"":0,""sku"":""ELITE_SPECIFIC_V_EXPLORE_DECAL_1005""},""128671323"":{""id"":128671323,""category"":""powerplay"",""name"":""Int_ShieldGenerator_Size1_Class5_Strong"",""cost"":132195,""sku"":""ELITE_SPECIFIC_V_POWER_100000""},""128671324"":{""id"":128671324,""category"":""powerplay"",""name"":""Int_ShieldGenerator_Size2_Class5_Strong"",""cost"":240336,""sku"":""ELITE_SPECIFIC_V_POWER_100000""},""128671325"":{""id"":128671325,""category"":""powerplay"",""name"":""Int_ShieldGenerator_Size3_Class5_Strong"",""cost"":761868,""sku"":""ELITE_SPECIFIC_V_POWER_100000""},""128671326"":{""id"":128671326,""category"":""powerplay"",""name"":""Int_ShieldGenerator_Size4_Class5_Strong"",""cost"":2415120,""sku"":""ELITE_SPECIFIC_V_POWER_100000""},""128671327"":{""id"":128671327,""category"":""powerplay"",""name"":""Int_ShieldGenerator_Size5_Class5_Strong"",""cost"":7655930,""sku"":""ELITE_SPECIFIC_V_POWER_100000""},""128671328"":{""id"":128671328,""category"":""powerplay"",""name"":""Int_ShieldGenerator_Size6_Class5_Strong"",""cost"":24269297,""sku"":""ELITE_SPECIFIC_V_POWER_100000""},""128671329"":{""id"":128671329,""category"":""powerplay"",""name"":""Int_ShieldGenerator_Size7_Class5_Strong"",""cost"":76933668,""sku"":""ELITE_SPECIFIC_V_POWER_100000""},""128671330"":{""id"":128671330,""category"":""powerplay"",""name"":""Int_ShieldGenerator_Size8_Class5_Strong"",""cost"":243879729,""sku"":""ELITE_SPECIFIC_V_POWER_100000""}}},""ship"":{""name"":""Cutter"",""modules"":{""MediumHardpoint2"":{""module"":{""id"":128049389,""name"":""Hpt_PulseLaser_Turret_Medium"",""value"":112880,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":1,""hopper"":1}}},""MediumHardpoint1"":{""module"":{""id"":128049389,""name"":""Hpt_PulseLaser_Turret_Medium"",""value"":112880,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":1,""hopper"":1}}},""MediumHardpoint4"":{""module"":{""id"":128049463,""name"":""Hpt_MultiCannon_Turret_Medium"",""value"":1098880,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":90,""hopper"":2100}}},""MediumHardpoint3"":{""module"":{""id"":128049463,""name"":""Hpt_MultiCannon_Turret_Medium"",""value"":1098880,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":90,""hopper"":2100}}},""LargeHardpoint2"":{""module"":{""id"":128049390,""name"":""Hpt_PulseLaser_Turret_Large"",""value"":340340,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":1,""hopper"":1}}},""LargeHardpoint1"":{""module"":{""id"":128049390,""name"":""Hpt_PulseLaser_Turret_Large"",""value"":340340,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":1,""hopper"":1}}},""HugeHardpoint1"":{""module"":{""id"":128681994,""name"":""Hpt_BeamLaser_Gimbal_Huge"",""value"":7434236,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""TinyHardpoint1"":{""module"":{""id"":128668536,""name"":""Hpt_ShieldBooster_Size0_Class5"",""value"":238850,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0},""recipeValue"":0,""recipeName"":""ShieldBooster_Thermic"",""recipeLevel"":1,""modifiers"":{""id"":2839,""engineerID"":300100,""recipeID"":128673795,""slotIndex"":35,""moduleTags"":[22],""modifiers"":[{""name"":""mod_defencemodifier_shield_thermic_mult"",""value"":-0.049490001052618,""type"":1},{""name"":""mod_defencemodifier_shield_explosive_mult"",""value"":0.020628247410059,""type"":1},{""name"":""mod_defencemodifier_shield_kinetic_mult"",""value"":0.021059958264232,""type"":1}]}}},""TinyHardpoint2"":{""module"":{""id"":128668536,""name"":""Hpt_ShieldBooster_Size0_Class5"",""value"":238850,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0},""recipeValue"":0,""recipeName"":""ShieldBooster_Resistive"",""recipeLevel"":1,""modifiers"":{""id"":2841,""engineerID"":300100,""recipeID"":128673790,""slotIndex"":36,""moduleTags"":[22],""modifiers"":[{""name"":""mod_passive_power"",""value"":0.048395585268736,""type"":1},{""name"":""mod_defencemodifier_global_shield_mult"",""value"":-0.013773267157376,""type"":1},{""name"":""mod_health"",""value"":-0.02138064801693,""type"":1}]}}},""TinyHardpoint3"":{""module"":{""id"":128668536,""name"":""Hpt_ShieldBooster_Size0_Class5"",""value"":238850,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0},""recipeValue"":0,""recipeName"":""ShieldBooster_Resistive"",""recipeLevel"":1,""modifiers"":{""id"":2843,""engineerID"":300100,""recipeID"":128673790,""slotIndex"":37,""moduleTags"":[22],""modifiers"":[{""name"":""mod_passive_power"",""value"":0.016008185222745,""type"":1},{""name"":""mod_defencemodifier_global_shield_mult"",""value"":-0.010448658838868,""type"":1},{""name"":""mod_health"",""value"":-0.014675753191113,""type"":1}]}}},""TinyHardpoint4"":{""module"":{""id"":128662526,""name"":""Hpt_CloudScanner_Size0_Class2"",""value"":34539,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":2,""ammo"":{""clip"":0,""hopper"":0}}},""TinyHardpoint5"":{""module"":{""id"":128049516,""name"":""Hpt_ElectronicCountermeasure_Tiny"",""value"":10625,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""TinyHardpoint6"":{""module"":{""id"":128049519,""name"":""Hpt_HeatSinkLauncher_Turret_Tiny"",""value"":2975,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":1,""hopper"":2}}},""TinyHardpoint7"":{""module"":{""id"":128049513,""name"":""Hpt_ChaffLauncher_Tiny"",""value"":7225,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":1,""hopper"":10}}},""TinyHardpoint8"":{""module"":{""id"":128049513,""name"":""Hpt_ChaffLauncher_Tiny"",""value"":7225,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":1,""hopper"":10}}},""Decal1"":{""module"":{""id"":128667750,""name"":""Decal_Trade_Tycoon"",""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Decal2"":{""module"":{""id"":128667750,""name"":""Decal_Trade_Tycoon"",""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Decal3"":{""module"":{""id"":128667750,""name"":""Decal_Trade_Tycoon"",""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""PaintJob"":[],""Armour"":{""module"":{""name"":""Cutter_Armour_Grade1"",""id"":128049376,""value"":0,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1}},""PowerPlant"":{""module"":{""id"":128064067,""name"":""Int_Powerplant_Size8_Class5"",""value"":138198514,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0},""recipeValue"":0,""recipeName"":""PowerPlant_Armoured"",""recipeLevel"":1,""modifiers"":{""id"":2838,""engineerID"":300100,""recipeID"":128673760,""slotIndex"":50,""moduleTags"":[18],""modifiers"":[{""name"":""mod_health"",""value"":0.12365217506886,""type"":1},{""name"":""mod_mass"",""value"":0.0067569278180599,""type"":1},{""name"":""mod_powerplant_heat"",""value"":-0.022506788372993,""type"":1}]}}},""MainEngines"":{""module"":{""id"":128064102,""name"":""Int_Engine_Size8_Class5"",""value"":138198514,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0},""recipeValue"":0,""recipeName"":""Engine_Reinforced"",""recipeLevel"":1,""modifiers"":{""id"":2826,""engineerID"":300100,""recipeID"":128673660,""slotIndex"":51,""moduleTags"":[17],""modifiers"":[{""name"":""mod_mass"",""value"":0,""type"":1},{""name"":""mod_health"",""value"":0.18770506978035,""type"":1},{""name"":""mod_engine_heat"",""value"":-0.089846849441528,""type"":1},{""name"":""mod_engine_mass_curve_multiplier"",""value"":-0.016663840040565,""type"":1}]}}},""FrameShiftDrive"":{""module"":{""id"":128064132,""name"":""Int_Hyperdrive_Size7_Class5"",""value"":43595746,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0},""recipeValue"":0,""recipeName"":""FSD_LongRange"",""recipeLevel"":1,""modifiers"":{""id"":2829,""engineerID"":300100,""recipeID"":128673690,""slotIndex"":52,""moduleTags"":[16],""modifiers"":[{""name"":""mod_mass"",""value"":0.075771875679493,""type"":1},{""name"":""mod_health"",""value"":-0.028150219470263,""type"":1},{""name"":""mod_passive_power"",""value"":0.0045042973943055,""type"":1},{""name"":""mod_fsd_optimised_mass"",""value"":0.034270532429218,""type"":1}]}}},""LifeSupport"":{""module"":{""id"":128064169,""name"":""Int_LifeSupport_Size7_Class2"",""value"":529417,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""PowerDistributor"":{""module"":{""id"":128064212,""name"":""Int_PowerDistributor_Size7_Class5"",""value"":9731925,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""Radar"":{""module"":{""id"":128064249,""name"":""Int_Sensors_Size7_Class2"",""value"":529417,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""FuelTank"":{""module"":{""id"":128064351,""name"":""Int_FuelTank_Size6_Class3"",""value"":341577,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Slot01_Size8"":{""module"":{""id"":128064345,""name"":""Int_CargoRack_Size8_Class1"",""value"":3255387,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Slot02_Size8"":{""module"":{""id"":128064345,""name"":""Int_CargoRack_Size8_Class1"",""value"":3829866,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Slot03_Size6"":{""module"":{""id"":128671328,""name"":""Int_ShieldGenerator_Size6_Class5_Strong"",""value"":20628903,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":0,""ammo"":{""clip"":0,""hopper"":0}}},""Slot04_Size6"":{""module"":{""id"":128064343,""name"":""Int_CargoRack_Size6_Class1"",""value"":308203,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Slot05_Size6"":{""module"":{""id"":128666681,""name"":""Int_FuelScoop_Size6_Class5"",""value"":28763610,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":2,""ammo"":{""clip"":0,""hopper"":0}}},""Slot06_Size5"":{""module"":{""id"":128671243,""name"":""Int_DroneControl_Collection_Size5_Class5"",""value"":660960,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Slot07_Size5"":{""module"":{""id"":128663561,""name"":""Int_StellarBodyDiscoveryScanner_Advanced"",""value"":1545000,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Slot08_Size4"":{""module"":{""id"":128666634,""name"":""Int_DetailedSurfaceScanner_Tiny"",""value"":250000,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":1,""ammo"":{""clip"":0,""hopper"":0}}},""Slot09_Size3"":{""module"":{""id"":128049549,""name"":""Int_DockingComputer_Standard"",""value"":4500,""unloaned"":0,""free"":false,""health"":1000000,""on"":true,""priority"":2,""ammo"":{""clip"":0,""hopper"":0}}},""PlanetaryApproachSuite"":{""module"":{""name"":""Int_PlanetApproachSuite"",""id"":128672317,""value"":449,""unloaned"":449,""free"":false,""health"":1000000,""on"":true,""priority"":1}},""Bobble01"":[],""Bobble02"":[],""Bobble03"":[],""Bobble04"":[],""Bobble05"":[],""Bobble06"":[],""Bobble07"":[],""Bobble08"":[],""Bobble09"":[],""Bobble10"":[]},""value"":{""hull"":179933774,""modules"":401689563,""cargo"":111552,""total"":581734889,""unloaned"":449},""free"":false,""alive"":true,""health"":{""hull"":1000000,""shield"":1000000,""shieldup"":true,""integrity"":0,""paintwork"":0},""wear"":{""dirt"":621,""fade"":380,""tear"":23,""game"":1024},""cockpitBreached"":false,""oxygenRemaining"":450000,""fuel"":{""main"":{""capacity"":64,""level"":45.479351},""reserve"":{""capacity"":1.16,""level"":0.24177764}},""cargo"":{""capacity"":576,""qty"":576,""items"":[{""commodity"":""drones"",""origin"":3223470848,""powerplayOrigin"":null,""masq"":null,""owner"":1943695,""mission"":null,""qty"":20,""value"":2020,""xyz"":{""x"":50000.53125,""y"":40884.8125,""z"":24017.71875},""marked"":0},{""commodity"":""basicmedicines"",""origin"":3229756160,""powerplayOrigin"":null,""masq"":null,""owner"":1943695,""mission"":null,""qty"":556,""value"":109532,""xyz"":{""x"":50107.625,""y"":40984.1875,""z"":24057.71875},""marked"":0}],""lock"":533553633,""ts"":{""sec"":1466355486,""usec"":507000}},""passengers"":[],""refinery"":null,""id"":7},""ships"":[{""name"":""Asp"",""alive"":true,""station"":{""id"":3223470848,""name"":""Hiyya Orbital""},""starsystem"":{""id"":""9467315627393"",""name"":""Arjung"",""systemaddress"":""9467315627393""},""id"":1},{""name"":""SideWinder"",""alive"":true,""station"":{""id"":3223474944,""name"":""Struzan Enterprise""},""starsystem"":{""id"":""671222605185"",""name"":""Zorya Nong"",""systemaddress"":""671222605185""},""id"":2},{""name"":""Empire_Trader"",""alive"":true,""station"":{""id"":3223470848,""name"":""Hiyya Orbital""},""starsystem"":{""id"":""9467315627393"",""name"":""Arjung"",""systemaddress"":""9467315627393""},""id"":4}]}";
            JObject json = JObject.Parse(data);

            List<Ship> shipyard = FrontierApi.ShipyardFromJson(null, json);

            Assert.AreEqual(3, shipyard.Count);
        }
    }
}
