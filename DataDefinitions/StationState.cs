using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    /// <summary> Station's state (UnderRepairs, Damaged, Abandoned, UnderAttack) </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StationState : ResourceBasedLocalizedEDName<StationState>
    {
        static StationState ()
        {
            resourceManager = Properties.StationState.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( edname ) => new StationState( edname );
        }

        public static readonly StationState Abandoned = new StationState ( "Abandoned" );
        public static readonly StationState Damaged = new StationState ( "Damaged" );
        public static readonly StationState NormalOperation = new StationState ( "NormalOperation" );
        public static readonly StationState UnderAttack = new StationState ( "UnderAttack" );
        public static readonly StationState UnderRepairs = new StationState ( "UnderRepairs" );

        // dummy used to ensure that the static constructor has run
        public StationState () : this("")
        { }

        private StationState ( string edname) : base(edname, edname)
        { }
    }
}
