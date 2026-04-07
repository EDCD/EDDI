
namespace EddiDataDefinitions
{
    public class HandWeaponMod : ResourceBasedLocalizedEDName<HandWeaponMod>
    {
        static HandWeaponMod ()
        {
            resourceManager = Properties.HandWeaponMod.ResourceManager;
            resourceManager.IgnoreCase = false;
            missingEDNameHandler = edname => new HandWeaponMod( edname );
        }

        public static readonly HandWeaponMod AudioMasking = new( "weapon_suppression_unpressurised");
        public static readonly HandWeaponMod FasterHandling = new( "weapon_handling");
        public static readonly HandWeaponMod GreaterRange = new( "weapon_range");
        public static readonly HandWeaponMod HeadshotDamage = new( "weapon_headshotdamage");
        public static readonly HandWeaponMod HipFireAccuracy = new( "weapon_accuracy");
        public static readonly HandWeaponMod MagazineSize = new( "weapon_clipsize");
        public static readonly HandWeaponMod NoiseSuppressor = new( "weapon_suppression_pressurised");
        public static readonly HandWeaponMod ReloadSpeed = new( "weapon_reloadspeed");
        public static readonly HandWeaponMod Scope = new( "weapon_scope");
        public static readonly HandWeaponMod Stability = new( "weapon_stability");
        public static readonly HandWeaponMod StowedReloading = new( "weapon_backpackreloading");

        // dummy used to ensure that the static constructor has run
        public HandWeaponMod () : this("")
        { }

        private HandWeaponMod ( string edname ) : base( edname, edname )
        { }

        public new static HandWeaponMod FromEDName ( string edname )
        { 
            var tidiedEdName = edname.Replace("_kinetic", "").Replace( "_laser", "" ).Replace( "_plasma", "" );
            return ResourceBasedLocalizedEDName<HandWeaponMod>.FromEDName( tidiedEdName );
        }
    }
}
