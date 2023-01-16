using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    public class ModuleInfoItem
    {
        [JsonProperty]
        public string slot { get; set; }

        [JsonProperty]
        public string item { get; set; }

        [JsonProperty]
        public decimal power { get; set; }

        [JsonProperty]
        public int priority { get; set; }

        public ModuleInfoItem()
        { }

        public ModuleInfoItem(ModuleInfoItem moduleInfoItem)
        {
            this.slot = moduleInfoItem.slot;
            this.item = moduleInfoItem.item;
            this.power = moduleInfoItem.power;
            this.priority = moduleInfoItem.priority;
        }

        public ModuleInfoItem(string Slot, string Item, decimal Power, int Priority)
        {
            this.slot = Slot;
            this.item = Item;
            this.power = Power;
            this.priority = Priority;

        }
    }
}
