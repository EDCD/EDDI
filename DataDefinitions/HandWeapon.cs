
using System;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public class HandWeapon ( string edname, HandWeaponType type, Manufacturer manufacturer, int price ) : ResourceBasedLocalizedEDName<HandWeapon>( edname, edname )
    {
        static HandWeapon ()
        {
            resourceManager = Properties.HandWeapon.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = edname => new HandWeapon( edname, null, null, 0 );
        }

        // dummy used to ensure that the static constructor has run
        public HandWeapon () : this( "", null, null, 0 )
        { }

        // Kinetic Weapons
        public static readonly HandWeapon KarmaAR50 = new( "Wpn_M_AssaultRifle_Kinetic_FAuto", HandWeaponType.AssaultRifle, Manufacturer.KinematicArmaments, 100000 );
        public static readonly HandWeapon KarmaC44 = new( "Wpn_M_SubMachineGun_Kinetic_FAuto", HandWeaponType.SubMachineGun, Manufacturer.KinematicArmaments, 50000 );
        public static readonly HandWeapon KarmaP15 = new( "Wpn_S_Pistol_Kinetic_SAuto", HandWeaponType.Pistol, Manufacturer.KinematicArmaments, 75000 );

        // Thermal Weapons
        public static readonly HandWeapon TKAphelion = new( "Wpn_M_AssaultRifle_Laser_FAuto", HandWeaponType.AssaultRifle, Manufacturer.Takada, 100000 );
        public static readonly HandWeapon TKEclipse = new( "Wpn_M_SubMachineGun_Laser_FAuto", HandWeaponType.SubMachineGun, Manufacturer.Takada, 50000 );
        public static readonly HandWeapon TKZenith = new( "Wpn_S_Pistol_Laser_SAuto", HandWeaponType.Pistol, Manufacturer.Takada, 75000 );

        // Plasma Weapons
        public static readonly HandWeapon ManticoreExecutioner = new( "Wpn_M_Sniper_Plasma_Charged", HandWeaponType.SniperRifle, Manufacturer.Manticore, 175000 );
        public static readonly HandWeapon ManticoreIntimidator = new( "Wpn_M_Shotgun_Plasma_DoubleBarrel", HandWeaponType.Shotgun, Manufacturer.Manticore, 100000 );
        public static readonly HandWeapon ManticoreOppressor = new( "Wpn_M_AssaultRifle_Plasma_FAuto", HandWeaponType.AssaultRifle, Manufacturer.Manticore, 125000 );
        public static readonly HandWeapon ManticoreTormentor = new( "Wpn_S_Pistol_Plasma_Charged", HandWeaponType.Pistol, Manufacturer.Manticore, 50000 );

        // Other Weapons
        public static readonly HandWeapon KarmaL6 = new( "Wpn_M_Launcher_Rocket_SAuto", HandWeaponType.RocketLauncher, Manufacturer.KinematicArmaments, 175000 );

        [PublicAPI( "The weapon's type, as an object" )]
        public HandWeaponType type { get; } = type;

        [PublicAPI( "The weapon's manufacturer, as an object" )]
        public Manufacturer manufacturer { get; } = manufacturer;

        [PublicAPI( "The weapon's standard grade 1 price" )]
        public int price { get; } = price;

        public static HandWeapon FromNameOrEdName ( string edname, string name )
        {
            var handweapon = AllOfThem.FirstOrDefault(handweapon => handweapon.edname.Equals(edname, StringComparison.OrdinalIgnoreCase));
            if ( handweapon is null )
            {
                Logging.Warn( $"Unknown hand weapon with edname '{edname}' and localized name '{name}'" );
                handweapon = new HandWeapon( edname, null, null, 0 );
            }
            return handweapon;
        }
    }
}
