using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiInaraService
{
    // Documentation: https://inara.cz/inara-api-docs/
    // Inara API events related to manipulating the commander's material inventory

        /*
    public class addCommanderInventoryMaterialsItem : InaraAPIEvent
    {
        public addCommanderInventoryMaterialsItem(DateTime timestamp, Material material, int itemCount)
        {
            // Adds a specified count of the individual material to the commander's inventory.
            // Note: Don't forget to correctly remove/add cargo commodities or materials also on journal events like
            // MiningRefined, ScientificResearch, SearchAndRescue, Synthesis, EngineerCraft, EngineerContribution, etc.
            if (material is null || itemCount < 0)
            {
                return;
            }
            eventName = "addCommanderInventoryMaterialsItem";
            eventTimeStamp = timestamp;
            eventData = new Dictionary<string, object>()
            {
                { "itemName", material.edname },
                { "itemCount", itemCount }
            };
        }
    }

    public class delCommanderInventoryMaterialsItem : InaraAPIEvent
    {
        public delCommanderInventoryMaterialsItem(DateTime timestamp, Material material, int itemCount)
        {
            // Removes a specified count of the individual material from the commander's inventory.
            // Note: Don't forget to correctly remove/add cargo commodities or materials also on journal events like
            // MiningRefined, ScientificResearch, SearchAndRescue, Synthesis, EngineerCraft, EngineerContribution, etc.
            if (material is null || itemCount < 0)
            {
                return;
            }
            eventName = "delCommanderInventoryMaterialsItem";
            eventTimeStamp = timestamp;
            eventData = new Dictionary<string, object>()
            {
                { "itemName", material.edname },
                { "itemCount", itemCount }
            };
        }
    }

    public class setCommanderInventoryMaterials : InaraAPIEvent
    {
        public setCommanderInventoryMaterials(DateTime timestamp, List<MaterialAmount> materials)
        {
            // Sets a specified count of the individual item in the commander's cargo. 
            // If no item is present in the cargo, it is added. When count is set to zero, the item is removed.
            if (materials is null || materials?.Count == 0)
            {
                return;
            }
            eventName = "setCommanderInventoryMaterials";
            eventTimeStamp = timestamp;
            eventData = new List<Dictionary<string, object>>();
            foreach (MaterialAmount materialAmount in materials)
            {
                Dictionary<string, object> inaraMaterialAmount = new Dictionary<string, object>()
                {
                    { "itemName", materialAmount.edname },
                    { "itemCount", materialAmount.amount }
                };
                ((List<Dictionary<string, object>>)eventData).Add(inaraMaterialAmount);
            }
        }
    }

    public class setCommanderInventoryMaterialsItem : InaraAPIEvent
    {
        public setCommanderInventoryMaterialsItem(DateTime timestamp, MaterialAmount materialAmount)
        {
            // Sets a specified count of the individual item in the commander's cargo. 
            // If no item is present in the cargo, it is added. When count is set to zero, the item is removed.
            if (materialAmount is null || materialAmount?.amount < 0)
            {
                return;
            }
            eventName = "setCommanderInventoryMaterialsItem";
            eventTimeStamp = timestamp;
            eventData = new Dictionary<string, object>()
            {
                { "itemName", materialAmount.edname },
                { "itemCount", materialAmount.amount }
            };
        }
    }
    */
}
