namespace EddiDataDefinitions
{
    /// <summary>
    /// Cargo defines a number of commodities carried along with some additional data
    /// </summary>
    public class Cargo
    {
        public Commodity commodity; // The commodity
        public int amount; // The number of items
        public long price; // How much we actually paid for it (per unit)
        public bool stolen; // If the cargo is stolen
        public long? missionid; // The mission ID to which the cargo relates
    }
}
