using EddiDataDefinitions;

namespace EddiSpeechService
{
    public class EddiSpeech
    {
        public string message { get; private set; }
        public bool wait { get; private set; }
        public Ship ship { get; private set; }
        public int priority { get; private set; }
        public string voice { get; private set; }
        public bool radio { get; private set; }
        public string eventType { get; private set; }

        public EddiSpeech(string message, bool wait, Ship ship = null, int priority = 3, string voice = null, bool radio = false, string eventType = null)
        {
            this.message = message;
            this.wait = wait;
            this.ship = ship;
            this.priority = priority;
            this.voice = voice;
            this.radio = radio;
            this.eventType = eventType;
        }
    }
}
