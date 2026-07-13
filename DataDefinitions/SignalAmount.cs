using Utilities;

namespace EddiDataDefinitions
{
    public class SignalAmount ( SignalSource signalSource, int amount )
    {
        [PublicAPI( "the localized name of the source of the signal" )]
        public string source => signalSource.localizedName;
        
        [PublicAPI( "the amount of the signal" )]
        public int amount { get; } = amount;

        // Not intended to be user facing

        public SignalSource signalSource { get; } = signalSource;

        public string edname => signalSource.edname;
    }
}
