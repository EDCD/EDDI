namespace EddiDataDefinitions
{
    /// <summary>
    /// Passenger types
    /// </summary>
    public class PassengerType : ResourceBasedLocalizedEDName<PassengerType>
    {
        static PassengerType()
        {
            resourceManager = Properties.PassengerType.ResourceManager;
            resourceManager.IgnoreCase = false;
            missingEDNameHandler = (edname) => new PassengerType(edname);
        }

        public static readonly PassengerType AidWorker = new PassengerType("AidWorker");
        public static readonly PassengerType Business = new PassengerType("Business");
        public static readonly PassengerType Celebrity = new PassengerType("MinorCelebrity");
        public static readonly PassengerType CEO = new PassengerType("CEO");
        public static readonly PassengerType Criminal = new PassengerType("Criminal");
        public static readonly PassengerType Doctor = new PassengerType("Doctor");
        public static readonly PassengerType Explorer = new PassengerType("Explorer");
        public static readonly PassengerType General = new PassengerType("General");
        public static readonly PassengerType HeadOfState = new PassengerType("HeadOfState");
        public static readonly PassengerType Medical = new PassengerType("Medical");
        public static readonly PassengerType Politician = new PassengerType("Politician");
        public static readonly PassengerType PrisonerOfWar = new PassengerType("POW");
        public static readonly PassengerType Protestor = new PassengerType("Protestor");
        public static readonly PassengerType Rebel = new PassengerType("Rebel");
        public static readonly PassengerType Refugee = new PassengerType("Refugee");
        public static readonly PassengerType Scientist = new PassengerType("Scientist");
        public static readonly PassengerType Security = new PassengerType("Security");
        public static readonly PassengerType Terrorist = new PassengerType("Terrorist");
        public static readonly PassengerType Tourist = new PassengerType("Tourist");
        public static readonly PassengerType Whistleblower = new PassengerType("Whistleblower");

        // dummy used to ensure that the static constructor has run
        public PassengerType() : this("")
        { }

        private PassengerType(string edname) : base(edname, edname
            .Replace( "POW", "PrisonerOfWar" )
            .Replace("MinorCelebrity", "Celebrity" ) 
        )
        { }
    }
}
