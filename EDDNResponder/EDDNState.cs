using EddiDataProviderService;
using EddiEddnResponder.Toolkit;

namespace EddiEddnResponder
{
    public class EDDNState
    {
        public readonly GameVersionAugmenter GameVersion;

        public readonly LocationAugmenter Location;

        public readonly PersonalDataStripper PersonalData;

        public EDDNState(IStarSystemRepository starSystemRepository)
        {
            GameVersion = new GameVersionAugmenter();
            Location = new LocationAugmenter(starSystemRepository);
            PersonalData = new PersonalDataStripper();
        }
    }
}