using EddiDataDefinitions;

namespace EddiSpanshService
{
    public interface ISpanshService
    {
        NavWaypointCollection GetCarrierRoute(string currentSystem, string[] targetSystems, long usedCarrierCapacity,
            bool calculateTotalFuelRequired = true, string[] refuelDestinations = null);

        NavWaypointCollection GetGalaxyRoute(string currentSystem, string targetSystem, Ship ship,
            int? cargoCarriedTons = null, bool isSupercharged = false, bool useSupercharge = true,
            bool useInjections = false, bool excludeSecondary = false);
    }
}
