using Utilities;

namespace EddiDataDefinitions
{
    public class DestinationSystem
    {
        [PublicAPI]
        public string name { get; set; }

        [PublicAPI]
        public bool visited;

        public DestinationSystem() { }

        public DestinationSystem(DestinationSystem DestinationSystem)
        {
            this.name = DestinationSystem.name;
        }

        public DestinationSystem(string Name)
        {
            this.name = Name;
            this.visited = false;
        }
    }
}
