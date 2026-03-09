namespace EddiDataDefinitions
{
    public class MessageChannel : ResourceBasedLocalizedEDName<MessageChannel>
    {
        static MessageChannel()
        {
            resourceManager = Properties.MessageChannels.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new MessageChannel(edname);
        }

        public static readonly MessageChannel Friend = new("Friend");
        public static readonly MessageChannel Local = new("Local");
        public static readonly MessageChannel MultiCrew = new("MultiCrew");
        public static readonly MessageChannel NPC = new("NPC");
        public static readonly MessageChannel Player = new("Player");
        public static readonly MessageChannel Squadron = new("Squadron");
        public static readonly MessageChannel StarSystem = new("StarSystem");
        public static readonly MessageChannel VoiceChat = new("VoiceChat");
        public static readonly MessageChannel Wing = new("Wing");
        
        // dummy used to ensure that the static constructor has run
        public MessageChannel() : this("")
        { }

        private MessageChannel(string edname) : base(edname, edname)
        { }
    }
}
