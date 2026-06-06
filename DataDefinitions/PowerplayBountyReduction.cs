using System.Collections.Generic;
using System.Linq;

namespace EddiDataDefinitions
{
    public static class PowerplayBountyReduction
    {
        private static readonly Dictionary<string, List<(int rank, decimal reduction)>> reductionByPower = new()
        {
            [ Power.ArchonDelaine.edname ] =
            [
                ( 5, 0.10M ), ( 14, 0.20M ), ( 22, 0.33M ), ( 32, 0.40M ), ( 48, 0.50M ),
                ( 55, 0.60M ), ( 67, 0.70M ), ( 73, 0.80M ), ( 86, 0.90M ), ( 100, 1.00M )
            ]
        };

        public static bool TryGetReduction ( Power power, int rank, out decimal reduction )
        {
            reduction = 0;
            if ( power is null || !reductionByPower.TryGetValue( power.edname, out var reductions ) )
            {
                return false;
            }

            var bestReduction = reductions
                .Where( r => rank >= r.rank )
                .OrderByDescending( r => r.rank )
                .Select( r => (decimal?)r.reduction )
                .FirstOrDefault();
            if ( bestReduction is null )
            {
                return false;
            }

            reduction = bestReduction.Value;
            return true;
        }
    }
}
