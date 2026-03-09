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

        public static readonly PassengerType AidWorker = new("AidWorker");
        public static readonly PassengerType Business = new("Business");
        public static readonly PassengerType Celebrity = new("MinorCelebrity");
        public static readonly PassengerType CEO = new("CEO");
        public static readonly PassengerType Criminal = new("Criminal");
        public static readonly PassengerType Doctor = new("Doctor");
        public static readonly PassengerType Explorer = new("Explorer");
        public static readonly PassengerType General = new("General");
        public static readonly PassengerType HeadOfState = new("HeadOfState");
        public static readonly PassengerType Medical = new("Medical");
        public static readonly PassengerType Politician = new("Politician");
        public static readonly PassengerType PrisonerOfWar = new("POW");
        public static readonly PassengerType Protestor = new("Protestor");
        public static readonly PassengerType Rebel = new("Rebel");
        public static readonly PassengerType Refugee = new("Refugee");
        public static readonly PassengerType Scientist = new("Scientist");
        public static readonly PassengerType Security = new("Security");
        public static readonly PassengerType Terrorist = new("Terrorist");
        public static readonly PassengerType Tourist = new("Tourist");
        public static readonly PassengerType Whistleblower = new("Whistleblower");

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
