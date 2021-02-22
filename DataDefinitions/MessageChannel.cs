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

        public static readonly MessageChannel Friend = new MessageChannel("Friend");
        public static readonly MessageChannel Local = new MessageChannel("Local");
        public static readonly MessageChannel MultiCrew = new MessageChannel("MultiCrew");
        public static readonly MessageChannel NPC = new MessageChannel("NPC");
        public static readonly MessageChannel Player = new MessageChannel("Player");
        public static readonly MessageChannel Squadron = new MessageChannel("Squadron");
        public static readonly MessageChannel StarSystem = new MessageChannel("StarSystem");
        public static readonly MessageChannel VoiceChat = new MessageChannel("VoiceChat");
        public static readonly MessageChannel Wing = new MessageChannel("Wing");
        
        // dummy used to ensure that the static constructor has run
        public MessageChannel() : this("")
        { }

        private MessageChannel(string edname) : base(edname, edname)
        { }
    }
}
