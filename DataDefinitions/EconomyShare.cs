using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    public class EconomyShare
    {
        public Economy economy;

        /// <summary> On a 0 to 1 scale </summary>
        public decimal proportion;

        public EconomyShare(string name, decimal proportion)
        {
            this.economy = Economy.FromName(name) ?? Economy.FromEDName(name);
            this.proportion = proportion;
        }

        [JsonConstructor]
        public EconomyShare(Economy economy, decimal proportion)
        {
            this.economy = economy;
            this.proportion = proportion;
        }
    }
}
