using System.Collections.Generic;
using Utilities;

namespace EddiDataDefinitions
{
    /**
     * A modification to a module
     */
    public class Modification
    {
        public const int NONE = -1;

        public const int AMMO = 0;
        public const int BOOT = 1;
        public const int BROKENREGEN = 2;
        public const int BURST = 3;
        public const int CLIP = 4;
        public const int DAMAGE = 5;
        public const int DISTDRAW = 6;
        public const int DURATION = 7;
        public const int EFF = 8;
        public const int ENGCAP = 9;
        public const int ENGRATE = 10;
        public const int EXPLRES = 11;
        public const int FACINGLIMIT = 12;
        public const int HULLBOOST = 13;
        public const int HULLREINFORCEMENT = 14;
        public const int INTEGRITY = 15;
        public const int JITTER = 16;
        public const int KINRES = 17;
        public const int MASS = 18;
        public const int MAXFUEL = 19;
        public const int OPTMASS = 20;
        public const int OPTMUL = 21;
        public const int PGEN = 22;
        public const int PIERCING = 23;
        public const int POWER = 24;
        public const int RANGE = 25;
        public const int RANGET = 26;
        public const int REGEN = 27;
        public const int RELOAD = 28;
        public const int ROF = 29;
        public const int SHIELD = 30;
        public const int SHIELDBOOST = 31;
        public const int SPINUP = 32;
        public const int SYSCAP = 33;
        public const int SYSRATE = 34;
        public const int THERMLOAD = 35;
        public const int THERMRES = 36;
        public const int WEPCAP = 37;
        public const int WEPRATE = 38;

        public int id { get; private set; }

        public decimal value { get; private set; }

        public Modification(int id)
        {
            this.id = id;
            value = 0;
        }

        public void Modify(decimal value)
        {
            this.value = (1 + this.value) * (1 + value) - 1;
        }

        public static void Modify(string name, decimal value, ref Dictionary<int, Modification> modifications)
        {
            switch (name)
            {
                case "mod_boot_time":
                    modify(BOOT, value, modifications);
                    break;
                case "mod_defencemodifier_explosive_mult":
                    modify(EXPLRES, value, modifications);
                    break;
                case "mod_defencemodifier_global_hull_mult":
                    modify(HULLBOOST, value, modifications);
                    break;
                case "mod_defencemodifier_global_shield_mult":
                    modify(EXPLRES, value, modifications);
                    modify(KINRES, value, modifications);
                    modify(THERMRES, value, modifications);
                    break;
                case "mod_defencemodifier_health_add":
                    modify(HULLREINFORCEMENT, value, modifications);
                    break;
                case "mod_defencemodifier_health_mult":
                    modify(HULLBOOST, value, modifications);
                    break;
                case "mod_defencemodifier_kinetic_mult":
                    modify(KINRES, value, modifications);
                    break;
                case "mod_defencemodifier_shield_explosive_mult":
                    modify(EXPLRES, value, modifications);
                    break;
                case "mod_defencemodifier_shield_kinetic_mult":
                    modify(KINRES, value, modifications);
                    break;
                case "mod_defencemodifier_shield_mult":
                    modify(SHIELDBOOST, value, modifications);
                    break;
                case "mod_defencemodifier_shield_thermic_mult":
                    modify(THERMRES, value, modifications);
                    break;
                case "mod_defencemodifier_thermic_mult":
                    modify(THERMRES, value, modifications);
                    break;
                case "mod_engine_heat":
                    modify(THERMLOAD, value, modifications);
                    break;
                case "mod_engine_mass_curve":
                    modify(OPTMASS, value, modifications);
                    break;
                case "mod_engine_mass_curve_multiplier":
                    modify(OPTMUL, value, modifications);
                    break;
                case "mod_fsd_heat_rate":
                    modify(THERMLOAD, value, modifications);
                    break;
                case "mod_fsd_max_fuel_per_jump":
                    modify(MAXFUEL, value, modifications);
                    break;
                case "mod_fsd_optimised_mass":
                    modify(OPTMASS, value, modifications);
                    break;
                case "mod_fsdinterdictor_facing_limit":
                    modify(FACINGLIMIT, value, modifications);
                    break;
                case "mod_fsdinterdictor_range":
                    modify(RANGET, value, modifications);
                    break;
                case "mod_health":
                    modify(INTEGRITY, value, modifications);
                    break;
                case "mod_mass":
                    modify(MASS, value, modifications);
                    break;
                case "mod_passive_power":
                    modify(POWER, value, modifications);
                    break;
                case "mod_powerdistributor_engine_charge":
                    modify(ENGCAP, value, modifications);
                    break;
                case "mod_powerdistributor_engine_rate":
                    modify(ENGRATE, value, modifications);
                    break;
                case "mod_powerdistributor_global_charge":
                    modify(SYSCAP, value, modifications);
                    modify(ENGCAP, value, modifications);
                    modify(WEPCAP, value, modifications);
                    break;
                case "mod_powerdistributor_global_rate":
                    modify(SYSRATE, value, modifications);
                    modify(ENGRATE, value, modifications);
                    modify(WEPRATE, value, modifications);
                    break;
                case "mod_powerdistributor_system_charge":
                    modify(SYSCAP, value, modifications);
                    break;
                case "mod_powerdistributor_system_rate":
                    modify(SYSRATE, value, modifications);
                    break;
                case "mod_powerdistributor_weapon_charge":
                    modify(WEPCAP, value, modifications);
                    break;
                case "mod_powerdistributor_weapon_rate":
                    modify(WEPRATE, value, modifications);
                    break;
                case "mod_powerplant_heat":
                    modify(EFF, value, modifications);
                    break;
                case "mod_powerplant_power":
                    modify(PGEN, value, modifications);
                    break;
                case "mod_shield_broken_regen":
                    modify(BROKENREGEN, value, modifications);
                    break;
                case "mod_shield_energy_per_regen":
                    modify(DISTDRAW, value, modifications);
                    break;
                case "mod_shield_explosive_mult":
                    modify(EXPLRES, value, modifications);
                    break;
                case "mod_shield_global_mult":
                    modify(EXPLRES, value, modifications);
                    modify(KINRES, value, modifications);
                    modify(THERMRES, value, modifications);
                    break;
                case "mod_shield_kinetic_mult":
                    modify(KINRES, value, modifications);
                    break;
                case "mod_shield_mass_curve":
                    modify(OPTMASS, value, modifications);
                    break;
                case "mod_shield_mass_curve_multiplier":
                    modify(OPTMUL, value, modifications);
                    break;
                case "mod_shield_normal_regen":
                    modify(REGEN, value, modifications);
                    break;
                case "mod_shield_thermal_mult":
                    modify(THERMRES, value, modifications);
                    break;
                case "mod_shieldcell_charge_heat":
                    modify(THERMLOAD, value, modifications);
                    break;
                case "mod_shieldcell_duration":
                    modify(DURATION, value, modifications);
                    break;
                case "mod_shieldcell_shield_units":
                    modify(SHIELD, value, modifications);
                    break;
                case "mod_shieldcell_spin_up":
                    modify(SPINUP, value, modifications);
                    break;
                case "mod_weapon_active_heat":
                    modify(THERMLOAD, value, modifications);
                    break;
                case "mod_weapon_active_power":
                    modify(DISTDRAW, value, modifications);
                    break;
                case "mod_weapon_ammo_capacity":
                    modify(AMMO, value, modifications);
                    break;
                case "mod_weapon_burst_interval":
                    modify(ROF, value, modifications);
                    break;
                case "mod_weapon_burst_rof":
                    // TODO
                    break;
                case "mod_weapon_burst_size":
                    modify(BURST, value, modifications);
                    break;
                case "mod_weapon_clip_size":
                    modify(CLIP, value, modifications);
                    break;
                case "mod_weapon_damage":
                    modify(DAMAGE, value, modifications);
                    break;
                case "mod_weapon_hardness_piercing":
                    modify(PIERCING, value, modifications);
                    break;
                case "mod_weapon_jitter_radius":
                    modify(JITTER, value, modifications); // TODO this is in degrees not %
                    break;
                case "mod_weapon_range":
                    modify(RANGE, value, modifications);
                    break;
                case "mod_weapon_reload_time":
                    modify(RELOAD, value, modifications);
                    break;
                case "special_auto_loader":
                    // TODO
                    break;
                case "special_corrosive_shell":
                    modify(AMMO, -0.2M, modifications);
                    break;
                case "special_distortion_field":
                    // TODO
                    break;
                case "special_drag_munitions":
                    // TODO
                    break;
                case "special_emissive_munitions":
                    // TODO
                    break;
                case "special_feedback_cascade":
                    // TODO
                    break;
                case "special_high_yield_shell":
                    // TODO
                    break;
                case "special_incendiary_rounds":
                    modify(THERMLOAD, 2M, modifications);
                    modify(ROF, (1M / 0.95M) - 1, modifications);
                    modify(DAMAGE, 0.5M, modifications);
                    break;
                case "special_phasing_sequence":
                    // TODO
                    break;
                case "special_regeneration_sequence":
                    // TODO
                    break;
                case "special_scramble_spectrum":
                    modify(ROF, (1M / 0.9M) - 1, modifications);
                    break;
                case "special_thermal_cascade":
                    // TODO
                    break;
                case "special_thermal_conduit":
                    // TODO
                    break;
                case "special_thermal_vent":
                    modify(THERMLOAD, 0.25M, modifications);
                    break;
                case "special_thermalshock":
                    modify(DAMAGE, -0.25M, modifications);
                    break;
                case "trade_cell_heat_cell_units":
                    // TODO
                    break;
                case "trade_defence_health_add_defence_global_mult":
                    // TODO
                    break;
                case "trade_distributor_engine_charge_system_charge":
                    // TODO
                    break;
                case "trade_distributor_global_charge_mass":
                    modify(MASS, value, modifications);
                    modify(SYSCAP, value * 0.75M, modifications);
                    modify(ENGCAP, value * 0.75M, modifications);
                    modify(WEPCAP, value * 0.75M, modifications);
                    break;
                case "trade_engine_curve_mult_engine_heat":
                    modify(OPTMUL, value * 0.4M, modifications);
                    modify(THERMLOAD, value * 1M, modifications);
                    break;
                case "trade_fsd_fuel_per_jump_fsd_heat":
                    modify(MAXFUEL, value * 0.5M, modifications);
                    modify(THERMLOAD, value * 1M, modifications);
                    break;
                case "trade_interdictor_range_facing_limit":
                    // TODO
                    break;
                case "trade_mass_defence_health_add":
                    // TODO
                    break;
                case "trade_mass_health":
                    modify(MASS, value * 0.4M, modifications);
                    modify(INTEGRITY, value, modifications);
                    break;
                case "trade_passive_power_booster_global_mult":
                    modify(POWER, value * -1M, modifications);
                    modify(EXPLRES, value * 0.4M, modifications);
                    modify(KINRES, value * 0.4M, modifications);
                    modify(THERMRES, value * 0.4M, modifications);
                    break;
                case "trade_passive_power_boot_time":
                    modify(POWER, value * -0.5M, modifications);
                    modify(BOOT, value, modifications);
                    break;
                case "trade_passive_power_cell_spin_up":
                    // TODO
                    break;
                case "trade_passive_power_distributor_global_rate":
                    modify(POWER, value * -1M, modifications);
                    modify(SYSCAP, value * -1M, modifications);
                    modify(ENGCAP, value * -1M, modifications);
                    modify(WEPCAP, value * -1M, modifications);
                    break;
                case "trade_passive_power_engine_curve":
                    // TODO
                    break;
                case "trade_passive_power_shield_global_mult":
                    modify(POWER, value * -1M, modifications);
                    modify(SYSCAP, value * 0.5M, modifications);
                    modify(ENGCAP, value * 0.5M, modifications);
                    modify(WEPCAP, value * 0.5M, modifications);
                    break;
                case "trade_passive_power_weapon_active":
                    modify(POWER, value, modifications);
                    modify(DISTDRAW, value * -0.6M, modifications);
                    break;
                case "trade_shield_curve_shield_curve_mult":
                    modify(OPTMASS, value * -1M, modifications);
                    modify(OPTMUL, value * -0.8M, modifications);
                    break;
                case "trade_shield_global_mult_shield_broken_regen":
                    // TODO
                    break;
                case "trade_shield_kinetic_shield_thermic":
                    // TODO
                    break;
                case "trade_weapon_active_passive_power":
                    modify(DISTDRAW, value * -0.67M, modifications);
                    modify(POWER, value, modifications);
                    break;
                case "trade_weapon_damage_weapon_active_power":
                    modify(DAMAGE, value * 0.5M, modifications);
                    modify(DISTDRAW, value, modifications);
                    break;
                case "trade_weapon_hardness_weapon_heat":
                    modify(PIERCING, value * 0.4M, modifications);
                    modify(THERMLOAD, value, modifications);
                    break;
                default:
                    Logging.Warn("Unhandled modification " + name);
                    break;
            }
        }

        private static void modify(int id, decimal value, Dictionary<int, Modification> modifications)
        {
            Modification modification;
            if (!modifications.TryGetValue(id, out modification))
            {
                modification = new Modification(id);
                modifications.Add(id, modification);
            }
            modification.Modify(value);
        }

        /// <summary>
        /// There are a number of wrinkles with modifications that we need to fix up when all of the modification
        /// information has been gathered.  See in-code comments for each case
        /// </summary>
        public static void FixUpModifications(Module module, Dictionary<int, Modification> modifications)
        {
            if (module.EDName.StartsWith("Hpt_ShieldBooster_"))
            {
                // Shield boosters are treated internally as straight modifiers, so rather than (for example)
                // being a 4% boost they are a 104% multiplier.  Unfortunately this means that our % modification
                // is incorrect so we fix it

                Modification sbModification;
                if (modifications.TryGetValue(SHIELDBOOST, out sbModification))
                {
                    // We do have a boost modification
                    decimal boost;
                    if (module.grade == "E")
                    {
                        boost = 1.04M;
                    }
                    else if (module.grade == "D")
                    {
                        boost = 1.08M;
                    }
                    else if (module.grade == "C")
                    {
                        boost = 1.12M;
                    }
                    else if (module.grade == "B")
                    {
                        boost = 1.16M;
                    }
                    else
                    {
                        boost = 1.2M;
                    }

                    decimal alteredBoost = boost * (1 + sbModification.value) - boost;
                    decimal alteredValue = alteredBoost / (boost - 1);
                    sbModification = new Modification(SHIELDBOOST);
                    sbModification.Modify(alteredValue);
                    modifications.Remove(SHIELDBOOST);
                    modifications.Add(SHIELDBOOST, sbModification);
                }
            }

            Modification jitterModification;
            if (modifications.TryGetValue(JITTER, out jitterModification))
            {
                // Jitter is in degrees rather than being a percentage, so needs to be /100
                decimal value = jitterModification.value / 100;
                jitterModification = new Modification(JITTER);
                jitterModification.Modify(value);
                modifications.Remove(JITTER);
                modifications.Add(JITTER, jitterModification);
            }

            Modification rofModification;
            if (modifications.TryGetValue(ROF, out rofModification))
            {
                // Although Elite talks about rate of fire, it is internally modelled as burst interval
                // i.e. the interval between bursts of fire.  We've been happily modifying ROF with interval modifiers
                // until now, so flip it here to provide the right number
                decimal value = (1.0M / (1 + rofModification.value)) - 1;
                rofModification = new Modification(ROF);
                rofModification.Modify(value);
                modifications.Remove(ROF);
                modifications.Add(ROF, rofModification);
            }
        }

    }
}
