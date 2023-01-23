using System.Collections.Generic;
using EddiDataDefinitions;

namespace EddiSpanshService
{
    public interface ISpanshService
    {
        NavWaypointCollection GetCarrierRoute(string currentSystem, string[] targetSystems, long usedCarrierCapacity,
            bool calculateTotalFuelRequired = true, string[] refuelDestinations = null, bool fromUIquery = false);

        NavWaypointCollection GetGalaxyRoute(string currentSystem, string targetSystem, Ship ship,
            int? cargoCarriedTons = null, bool isSupercharged = false, bool useSupercharge = true,
            bool useInjections = false, bool excludeSecondary = false, bool fromUIquery = false);

        List<string> GetTypeAheadStarSystems(string partialSystemName);
    }
}
