using System.Collections.Generic;
using System.Linq;

namespace EddiDataDefinitions
{
    public static class PowerplayBountyVoucherBonus
    {
        private static readonly Dictionary<string, List<(int rank, decimal bonus)>> bonusByPower = new()
        {
            [ Power.ALavignyDuval.edname ] =
            [
                ( 5, 0.10M ), ( 14, 0.20M ), ( 22, 0.30M ), ( 32, 0.40M ), ( 48, 0.50M ),
                ( 55, 0.60M ), ( 67, 0.70M ), ( 73, 0.80M ), ( 86, 0.90M ), ( 100, 1.00M )
            ],
            [ Power.JeromeArcher.edname ] =
            [
                ( 5, 0.10M ), ( 14, 0.20M ), ( 22, 0.30M ), ( 32, 0.40M ), ( 48, 0.50M ),
                ( 55, 0.60M ), ( 67, 0.70M ), ( 73, 0.80M ), ( 86, 0.90M ), ( 100, 1.00M )
            ],
            [ Power.DentonPatreus.edname ] =
            [
                ( 24, 0.20M ), ( 42, 0.35M ), ( 52, 0.50M ), ( 78, 0.65M ), ( 94, 0.80M )
            ],
            [ Power.YuriGrom.edname ] =
            [
                ( 5, 0.02M ), ( 14, 0.05M ), ( 22, 0.07M ), ( 32, 0.10M ), ( 48, 0.13M ),
                ( 55, 0.15M ), ( 67, 0.20M ), ( 73, 0.30M ), ( 86, 0.40M ), ( 100, 0.60M )
            ]
        };

        public static bool TryGetBonus ( Power power, int rank, out decimal bonus )
        {
            bonus = 0;
            if ( power is null || !bonusByPower.TryGetValue( power.edname, out var bonuses ) )
            {
                return false;
            }

            var bestBonus = bonuses
                .Where( b => rank >= b.rank )
                .OrderByDescending( b => b.rank )
                .Select( b => (decimal?)b.bonus )
                .FirstOrDefault();
            if ( bestBonus is null )
            {
                return false;
            }

            bonus = bestBonus.Value;
            return true;
        }

        public static long ApplyBonus ( long amount, decimal bonus )
        {
            return (long)decimal.Round( amount * ( 1 + bonus ), 0, System.MidpointRounding.AwayFromZero );
        }
    }
}
