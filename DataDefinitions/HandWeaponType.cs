
namespace EddiDataDefinitions
{
    public class HandWeaponType : ResourceBasedLocalizedEDName<HandWeaponType>
    {
        static HandWeaponType ()
        {
            resourceManager = Properties.HandWeaponType.ResourceManager;
            resourceManager.IgnoreCase = false;
            missingEDNameHandler = (edname) => new HandWeaponType( edname );
        }

        public static readonly HandWeaponType AssaultRifle = new( "AssaultRifle");
        public static readonly HandWeaponType Pistol = new( "Pistol");
        public static readonly HandWeaponType RocketLauncher = new( "RocketLauncher");
        public static readonly HandWeaponType Shotgun = new( "Shotgun");
        public static readonly HandWeaponType SniperRifle = new( "SniperRifle");
        public static readonly HandWeaponType SubMachineGun = new( "SubMachineGun");

        // dummy used to ensure that the static constructor has run
        public HandWeaponType () : this("")
        { }

        private HandWeaponType ( string edname ) : base( edname, edname )
        { }
    }
}
