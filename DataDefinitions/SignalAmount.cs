using Utilities;

namespace EddiDataDefinitions
{
    public class SignalAmount
    {
        [PublicAPI]
        public string source => signalSource.localizedName;
        
        [PublicAPI]
        public int amount { get; }      
        
        // Not intended to be user facing

        public SignalSource signalSource { get; }

        public string edname => signalSource.edname;
        
        public SignalAmount(SignalSource signalSource, int amount)
        {
            this.signalSource = signalSource;
            this.amount = amount;
        }
    }
}
