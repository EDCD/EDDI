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

            var AidWorker = new PassengerType("AidWorker");
            var Business = new PassengerType("Business");
            var Celebrity = new PassengerType("Celebrity");
            var CEO = new PassengerType("CEO");
            var Criminal = new PassengerType("Criminal");
            var Doctor = new PassengerType("Doctor");
            var Explorer = new PassengerType("Explorer");
            var General = new PassengerType("General");
            var HeadOfState = new PassengerType("HeadOfState");
            var Medical = new PassengerType("Medical");
            var Politician = new PassengerType("Politician");
            var PrisonerOfWar = new PassengerType("PrisonerOfWar");
            var Protestor = new PassengerType("Protestor");
            var Rebel = new PassengerType("Rebel");
            var Refugee = new PassengerType("Refugee");
            var Scientist = new PassengerType("Scientist");
            var Security = new PassengerType("Security");
            var Terrorist = new PassengerType("Terrorist");
            var Tourist = new PassengerType("Tourist");
            var Whistleblower = new PassengerType("Whistleblower");
        }

        // dummy used to ensure that the static constructor has run
        public PassengerType() : this("")
        { }

        private PassengerType(string edname) : base(edname, edname)
        { }
    }
}
