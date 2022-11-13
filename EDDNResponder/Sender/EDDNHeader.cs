using JetBrains.Annotations;

namespace EddiEddnResponder.Sender
{
    public class EDDNHeader
    {
        [UsedImplicitly]
        public string softwareVersion;
        
        [UsedImplicitly]
        public string softwareName;

        [UsedImplicitly]
        public string uploaderID;
    }
}
