using JetBrains.Annotations;

namespace EddiEddnResponder.Sender
{
    public class EDDNHeader
    {
        [UsedImplicitly]
        public string uploaderID;
        
        [UsedImplicitly]
        public string softwareVersion;
        
        [UsedImplicitly]
        public string softwareName;

        [UsedImplicitly]
        public string gameversion;

        [UsedImplicitly]
        public string gamebuild;
    }
}
