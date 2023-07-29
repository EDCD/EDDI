using Utilities;

namespace EddiDataDefinitions
{
    public class BioList
    {
        [PublicAPI]
        //public string source => signalSource.localizedName;
        public string source;

        // Not intended to be user facing

        //public SignalSource signalSource { get; }

        //public string edname => signalSource.edname;
        
        public BioList ( string signalSource)
        {
            this.source = signalSource;
        }
    }
}
