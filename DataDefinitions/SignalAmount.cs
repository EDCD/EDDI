namespace EddiDataDefinitions
{
    public class SignalAmount
    {
        public SignalSource signalSource { get; }
        public string source => signalSource.localizedName;
        public string edname => signalSource.edname;
        public int amount { get; }

        public SignalAmount(SignalSource signalSource, int amount)
        {
            this.signalSource = signalSource;
            this.amount = amount;
        }
    }
}
