using EddiDataDefinitions;
using EddiEvents;
using System.Threading.Tasks;

namespace EddiCore
{
    public interface ISpeechResponderController
    {
        Task SayAsync ( Ship ship, string scriptName, Event theEvent = null, int? priority = null, string voice = null, bool sayOutLoud = true, bool invokedFromVA = false );

        string ApproximateNumber ( decimal? number );

        bool TrySetPersonality ( string newPersonalityName );
    }
}
