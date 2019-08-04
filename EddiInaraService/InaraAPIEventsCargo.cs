using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiInaraService
{
    // Documentation: https://inara.cz/inara-api-docs/
    // Inara API events related to manipulating the commander's cargo

        /*
    public class addCommanderInventoryCargoItem : InaraAPIEvent
    {
        public addCommanderInventoryCargoItem(DateTime timestamp, CommodityDefinition commodityDefinition, int itemCount, bool isStolen, long? missionGameID = null)
        {
            // Adds a specified count of the individual item to the commander's cargo.
            // Note: Don't forget to correctly remove/add cargo commodities or materials also on journal events like
            // MiningRefined, ScientificResearch, SearchAndRescue, Synthesis, EngineerCraft, EngineerContribution, etc.
            if (commodityDefinition is null || itemCount < 0)
            {
                return;
            }
            eventName = "addCommanderInventoryCargoItem";
            eventTimeStamp = timestamp;
            eventData = new Dictionary<string, object>()
            {
                { "itemName", commodityDefinition.edname },
                { "itemCount", itemCount },
                { "isStolen", isStolen }
            };
            if (!(missionGameID is null))
            {
                ((Dictionary<string, object>)eventData).Add("missionGameID", missionGameID);
            }
        }
    }

    public class delCommanderInventoryCargoItem : InaraAPIEvent
    {
        public delCommanderInventoryCargoItem(DateTime timestamp, CommodityDefinition commodityDefinition, int itemCount, bool isStolen, long? missionGameID = null)
        {
            // Removes a specified count of the individual item from the commander's cargo.
            // Note: Don't forget to correctly remove/add cargo commodities or materials also on journal events like
            // MiningRefined, ScientificResearch, SearchAndRescue, Synthesis, EngineerCraft, EngineerContribution, etc.
            if (commodityDefinition is null || itemCount < 0)
            {
                return;
            }
            eventName = "delCommanderInventoryCargoItem";
            eventTimeStamp = timestamp;
            eventData = new Dictionary<string, object>()
            {
                { "itemName", commodityDefinition.edname },
                { "itemCount", itemCount },
                { "isStolen", isStolen }
            };
            if (!(missionGameID is null))
            {
                ((Dictionary<string, object>)eventData).Add("missionGameID", missionGameID);
            }
        }
    }

    public class setCommanderInventoryCargo : InaraAPIEvent
    {
        public setCommanderInventoryCargo(DateTime timestamp, List<CargoInfo> inventory)
        {
            // Sets a specified count of the individual item in the commander's cargo. 
            // If no item is present in the cargo, it is added. When count is set to zero, the item is removed.
            if (inventory is null || inventory?.Count == 0)
            {
                return;
            }
            eventName = "setCommanderInventoryCargo";
            eventTimeStamp = timestamp;
            eventData = new List<Dictionary<string, object>>();
            foreach (CargoInfo cargoInfo in inventory)
            {
                CommodityDefinition commodityDefinition = CommodityDefinition.FromEDName(cargoInfo.name);
                Dictionary<string, object> inaraCargo = new Dictionary<string, object>()
                {
                    { "itemName", commodityDefinition.edname },
                    { "itemCount", cargoInfo.count },
                    { "isStolen", cargoInfo.stolen > 0 },
                };
                if (!(cargoInfo.missionid is null))
                {
                    inaraCargo.Add("missionGameID", cargoInfo.missionid);
                }
                ((List<Dictionary<string, object>>)eventData).Add(inaraCargo);
            }
        }
    }

    public class setCommanderInventoryCargoItem : InaraAPIEvent
    {
        public setCommanderInventoryCargoItem(DateTime timestamp, CommodityDefinition commodityDefinition, int itemCount, bool isStolen, long? missionGameID = null)
        {
            // Sets a specified count of the individual item in the commander's cargo. 
            // If no item is present in the cargo, it is added. When count is set to zero, the item is removed.
            if (commodityDefinition is null || itemCount < 0)
            {
                return;
            }
            eventName = "setCommanderInventoryCargoItem";
            eventTimeStamp = timestamp;
            eventData = new Dictionary<string, object>()
            {
                { "itemName", commodityDefinition.edname },
                { "itemCount", itemCount },
                { "isStolen", isStolen }
            };
            if (!(missionGameID is null))
            {
                ((Dictionary<string, object>)eventData).Add("missionGameID", missionGameID);
            }
        }
    }
    */
}
