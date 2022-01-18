using EddiDataDefinitions;
using System;
using System.Linq;
using Utilities;

namespace EddiNavigationService
{
    public class JumpCalcs
    {
        public class JumpDetail
        {
            [PublicAPI]
            public decimal distance { get; private set; }

            [PublicAPI]
            public int jumps { get; private set; }

            public JumpDetail() { }

            public JumpDetail(decimal distance, int jumps)
            {
                this.distance = distance;
                this.jumps = jumps;
            }
        }

        public static JumpDetail JumpDetails(string type, Ship ship, decimal? fuelInTanks, int cargoCarried)
        {
            if (string.IsNullOrEmpty(type) || ship is null || fuelInTanks is null) { return null; }

            ship.maxfuelperjump = MaxFuelPerJump(ship);

            decimal maxFuel = ship.fueltanktotalcapacity ?? 0;

            if (!string.IsNullOrEmpty(type))
            {
                switch (type)
                {
                    case "next":
                        {
                            decimal distance = JumpRange(ship, fuelInTanks ?? 0, cargoCarried);
                            return new JumpDetail(distance, 1);
                        }
                    case "max":
                        {
                            decimal distance = JumpRange(ship, ship.maxfuelperjump, cargoCarried);
                            return new JumpDetail(distance, 1);
                        }
                    case "total":
                        {
                            decimal total = 0;
                            int jumps = 0;
                            while (fuelInTanks > 0)
                            {
                                total += JumpRange(ship, fuelInTanks ?? 0, cargoCarried);
                                jumps++;
                                fuelInTanks -= Math.Min(fuelInTanks ?? 0, ship.maxfuelperjump);
                            }
                            return new JumpDetail(total, jumps);
                        }
                    case "full":
                        {
                            decimal total = 0;
                            int jumps = 0;
                            while (maxFuel > 0)
                            {
                                total += JumpRange(ship, maxFuel, cargoCarried);
                                jumps++;
                                maxFuel -= Math.Min(maxFuel, ship.maxfuelperjump);
                            }
                            return new JumpDetail(total, jumps);
                        }
                }
            }
            return null;
        }

        private static decimal JumpRange(Ship ship, decimal currentFuel, int cargoCarried)
        {
            decimal boostConstant = 0;
            Module module = ship.compartments.FirstOrDefault(c => c.module.edname.Contains("Int_GuardianFSDBooster"))?.module;
            if (module != null)
            {
                Constants.guardianBoostFSD.TryGetValue(module.@class, out boostConstant);
            }

            Constants.ratingConstantFSD.TryGetValue(ship.frameshiftdrive.grade, out decimal ratingConstant);
            Constants.powerConstantFSD.TryGetValue(ship.frameshiftdrive.@class, out decimal powerConstant);
            decimal massRatio = ship.optimalmass / (ship.unladenmass + currentFuel + cargoCarried);
            decimal fuel = Math.Min(currentFuel, ship.maxfuelperjump);

            return ((decimal)Math.Pow((double)(1000 * fuel / ratingConstant), (double)(1 / powerConstant)) * massRatio) + boostConstant;
        }

        public static decimal MaxFuelPerJump(Ship ship)
        {
            // Max fuel per jump calculated using unladen mass and max jump range w/ just enough fuel to complete max jump
            decimal boostConstant = 0;
            Module module = ship.compartments.FirstOrDefault(c => c?.module?.edname != null && c.module.edname.Contains("Int_GuardianFSDBooster"))?.module;
            if (module != null)
            {
                Constants.guardianBoostFSD.TryGetValue(module.@class, out boostConstant);
            }
            Constants.ratingConstantFSD.TryGetValue(ship.frameshiftdrive.grade, out decimal ratingConstant);
            Constants.powerConstantFSD.TryGetValue(ship.frameshiftdrive.@class, out decimal powerConstant);
            decimal maxJumpRange = Math.Max(ship.maxjumprange - boostConstant, 0);
            decimal massRatio = (ship.unladenmass + ship.maxfuelperjump) / ship.optimalmass;

            return ratingConstant * (decimal)Math.Pow((double)(maxJumpRange * massRatio), (double)powerConstant) / 1000;
        }
    }
}
