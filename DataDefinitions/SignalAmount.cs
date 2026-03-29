using Utilities;

namespace EddiDataDefinitions
{
    public class SignalAmount ( SignalSource signalSource, int amount )
    {
        [PublicAPI]
        public string source => signalSource.localizedName;
        
        [PublicAPI]
        public int amount { get; } = amount;

        // Not intended to be user facing

        public SignalSource signalSource { get; } = signalSource;

        public string edname => signalSource.edname;
    }
}
