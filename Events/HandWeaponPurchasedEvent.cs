using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class HandWeaponPurchasedEvent ( DateTime timestamp, HandWeapon weapon, int grade, int price, ulong? suitModuleId, List<HandWeaponMod> mods )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Hand weapon purchased";
        public const string DESCRIPTION = "Triggered when purchasing a new hand weapon";
        public static readonly string[] SAMPLES = {
            @"{ ""timestamp"":""2021-06-05T07:44:12Z"", ""event"":""BuyWeapon"", ""Name"":""Wpn_M_AssaultRifle_Laser_FAuto"", ""Name_Localised"":""TK Aphelion"", ""Class"":3, ""Price"":15625000, ""SuitModuleID"":1701712024520913, ""WeaponMods"":[ ""weapon_headshotdamage"", ""weapon_suppression_pressurised"" ] }",
            @"{ ""timestamp"":""2021-06-04T08:11:17Z"", ""event"":""BuyWeapon"", ""Name"":""Wpn_M_SubMachineGun_Laser_FAuto"", ""Name_Localised"":""TK Eclipse"", ""Class"":3, ""Price"":5625000, ""SuitModuleID"":1701623131984176, ""WeaponMods"":[ ""weapon_stability"" ] }",
            @"{ ""timestamp"":""2021-06-05T12:46:47Z"", ""event"":""BuyWeapon"", ""Name"":""Wpn_S_Pistol_Laser_SAuto"", ""Name_Localised"":""TK Zenith"", ""Class"":3, ""Price"":3750000, ""SuitModuleID"":1701731061807438, ""WeaponMods"":[ ""weapon_suppression_unpressurised"" ] }",
            @"{ ""timestamp"":""2021-05-21T22:21:12Z"", ""event"":""BuyWeapon"", ""Name"":""Wpn_S_Pistol_Plasma_Charged"", ""Name_Localised"":""Manticore Tormentor"", ""Price"":50000, ""SuitModuleID"":1700408246310245, ""WeaponMods"":[  ] }",
            @"{ ""timestamp"":""2021-06-05T11:19:25Z"", ""event"":""BuyWeapon"", ""Name"":""Wpn_M_AssaultRifle_Plasma_FAuto"", ""Name_Localised"":""Manticore Oppressor"", ""Class"":3, ""Price"":9375000, ""SuitModuleID"":1701725564827288, ""WeaponMods"":[ ""weapon_clipsize"" ] }",
            @"{ ""timestamp"":""2021-06-05T05:27:29Z"", ""event"":""BuyWeapon"", ""Name"":""Wpn_M_Sniper_Plasma_Charged"", ""Name_Localised"":""Manticore Executioner"", ""Class"":3, ""Price"":13125000, ""SuitModuleID"":1701703423078722, ""WeaponMods"":[ ""weapon_reloadspeed"" ] }",
            @"{ ""timestamp"":""2021-06-05T08:59:58Z"", ""event"":""BuyWeapon"", ""Name"":""Wpn_M_Shotgun_Plasma_DoubleBarrel"", ""Name_Localised"":""Manticore Intimidator"", ""Class"":3, ""Price"":7500000, ""SuitModuleID"":1701716787574379, ""WeaponMods"":[ ""weapon_suppression_unpressurised"" ] }",
            @"{ ""timestamp"":""2021-05-31T00:58:49Z"", ""event"":""BuyWeapon"", ""Name"":""Wpn_M_AssaultRifle_Kinetic_FAuto"", ""Name_Localised"":""Karma AR-50"", ""Class"":3, ""Price"":9375000, ""SuitModuleID"":1701233535979007, ""WeaponMods"":[ ""weapon_suppression_pressurised"" ] }",
            @"{ ""timestamp"":""2021-05-21T22:55:57Z"", ""event"":""BuyWeapon"", ""Name"":""Wpn_M_SubMachineGun_Kinetic_FAuto"", ""Name_Localised"":""Karma C-44"", ""Price"":75000, ""SuitModuleID"":1700410432911892, ""WeaponMods"":[  ] }",
            @"{ ""timestamp"":""2021-06-06T00:50:35Z"", ""event"":""BuyWeapon"", ""Name"":""Wpn_M_Launcher_Rocket_SAuto"", ""Name_Localised"":""Karma L-6"", ""Class"":3, ""Price"":13125000, ""SuitModuleID"":1701776598877722, ""WeaponMods"":[ ""weapon_backpackreloading"" ] }",
            @"{ ""timestamp"":""2021-05-22T12:57:56Z"", ""event"":""BuyWeapon"", ""Name"":""Wpn_S_Pistol_Kinetic_SAuto"", ""Name_Localised"":""Karma P-15"", ""Price"":250000, ""SuitModuleID"":1700463405600816, ""WeaponMods"":[  ] }"
        };

        [PublicAPI(@"The weapon, as an object")]
        public HandWeapon weapon { get; } = weapon;

        [PublicAPI( @"The weapon's grade" )]
        public int grade { get; } = grade;

        [PublicAPI( @"The weapon's purchase price" )]
        public int price { get; } = price;

        [PublicAPI( @"The weapon's modifications (as objects)" )]
        public List<HandWeaponMod> mods { get; } = mods ?? [ ];

        // Not intended to be user facing

        public ulong? suitModuleId { get; } = suitModuleId;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var edname = JsonParsing.getString( data, "Name" );
            var name = JsonParsing.getString( data, "Name_Localised" );
            var handweapon = HandWeapon.FromEDName( edname );
            if ( handweapon != null )
            {
                handweapon.fallbackLocalizedName = name;
                var grade = JsonParsing.getOptionalInt( data, "Class" ) ?? 1;
                var price = JsonParsing.getInt( data, "Price" );
                var suitModuleId = JsonParsing.getOptionalULong( data, "SuitModuleID" );
                var mods = new List<HandWeaponMod>();
                if ( data.TryGetValue( "WeaponMods", out var weaponModsVal ) )
                {
                    var weaponMods = ( weaponModsVal as List<object> )?.Cast<string>()?.ToList() ?? [ ];
                    foreach ( var modEdName in weaponMods )
                    {
                        mods.Add( HandWeaponMod.FromEDName( modEdName ) );
                    }
                }

                events.Add( new HandWeaponPurchasedEvent( timestamp, handweapon, grade, price, suitModuleId, mods ) { raw = line, fromLoad = fromLogLoad } );
                return true;
            }

            return false;
        }
    }
}