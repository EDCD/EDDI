using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    public class ModuleInfo
    {
        [JsonProperty]
        public string slot { get; set; }

        [JsonProperty]
        public string item { get; set; }

        [JsonProperty]
        public decimal power { get; set; }

        [JsonProperty]
        public int priority { get; set; }

        public ModuleInfo()
        { }

        public ModuleInfo(ModuleInfo ModuleInfo)
        {
            this.slot = ModuleInfo.slot;
            this.item = ModuleInfo.item;
            this.power = ModuleInfo.power;
            this.priority = ModuleInfo.priority;
        }

        public ModuleInfo(string Slot, string Item, decimal Power, int Priority)
        {
            this.slot = Slot;
            this.item = Item;
            this.power = Power;
            this.priority = Priority;

        }
    }
}
