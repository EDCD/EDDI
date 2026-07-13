using Utilities;

namespace EddiDataDefinitions
{
    public class JumpDetail
    {
        [PublicAPI( "distance of jump range" )]
        public decimal distance { get; private set; }

        [PublicAPI( "number of jumps for given range" )]
        public int jumps { get; private set; }

        public JumpDetail() { }

        public JumpDetail(decimal distance, int jumps)
        {
            this.distance = distance;
            this.jumps = jumps;
        }
    }
}
