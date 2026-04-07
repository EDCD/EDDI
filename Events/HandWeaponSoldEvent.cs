using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class HandWeaponSoldEvent (
        DateTime timestamp, HandWeapon weapon, int grade, int price, ulong? suitModuleId, List<HandWeaponMod> mods )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Hand weapon sold";
        public const string DESCRIPTION = "Triggered when selling a hand weapon";
        public static readonly string[] SAMPLES = {
            @"{ ""timestamp"":""2023-01-22T07:12:29Z"", ""event"":""SellWeapon"", ""Name"":""wpn_m_assaultrifle_laser_fauto"", ""Name_Localised"":""TK Aphelion"", ""Class"":4, ""WeaponMods"":[ ""weapon_headshotdamage"", ""weapon_suppression_pressurised"" ], ""Price"":1200000, ""SuitModuleID"":1701712024520913 }",
            @"{ ""timestamp"":""2021-06-04T08:11:07Z"", ""event"":""SellWeapon"", ""Name"":""wpn_m_submachinegun_laser_fauto"", ""Name_Localised"":""TK Eclipse"", ""Class"":2, ""WeaponMods"":[  ], ""Price"":135000, ""SuitModuleID"":1700531590909303 }",
            @"{ ""timestamp"":""2021-05-25T03:11:40Z"", ""event"":""SellWeapon"", ""Name"":""wpn_s_pistol_laser_sauto"", ""Name_Localised"":""TK Zenith"", ""Price"":30000, ""SuitModuleID"":1700408238948652 }",
            @"{ ""timestamp"":""2021-05-25T03:11:08Z"", ""event"":""SellWeapon"", ""Name"":""wpn_s_pistol_plasma_charged"", ""Name_Localised"":""Manticore Tormentor"", ""Price"":30000, ""SuitModuleID"":1700408246310245 }",
            @"{ ""timestamp"":""2021-05-25T05:30:53Z"", ""event"":""SellWeapon"", ""Name"":""wpn_m_assaultrifle_plasma_fauto"", ""Name_Localised"":""Manticore Oppressor"", ""Price"":75000, ""SuitModuleID"":1700408252867861 }",
            @"{ ""timestamp"":""2021-05-27T08:31:12Z"", ""event"":""SellWeapon"", ""Name"":""wpn_m_sniper_plasma_charged"", ""Name_Localised"":""Manticore Executioner"", ""Class"":1, ""WeaponMods"":[  ], ""Price"":105000, ""SuitModuleID"":1700408257595207 }",
            @"{ ""timestamp"":""2021-06-05T09:00:23Z"", ""event"":""SellWeapon"", ""Name"":""wpn_m_shotgun_plasma_doublebarrel"", ""Name_Localised"":""Manticore Intimidator"", ""Class"":2, ""WeaponMods"":[  ], ""Price"":180000, ""SuitModuleID"":1700465272724996 }",
            @"{ ""timestamp"":""2021-06-13T10:13:35Z"", ""event"":""SellWeapon"", ""Name"":""wpn_m_assaultrifle_kinetic_fauto"", ""Name_Localised"":""Karma AR-50"", ""Class"":2, ""WeaponMods"":[  ], ""Price"":225000, ""SuitModuleID"":1701224948271833 }",
            @"{ ""timestamp"":""2021-05-29T20:17:44Z"", ""event"":""SellWeapon"", ""Name"":""wpn_m_submachinegun_kinetic_fauto"", ""Name_Localised"":""Karma C-44"", ""Class"":1, ""WeaponMods"":[  ], ""Price"":45000, ""SuitModuleID"":1700410432911892 }",
            @"{ ""timestamp"":""2023-01-22T07:13:08Z"", ""event"":""SellWeapon"", ""Name"":""wpn_m_launcher_rocket_sauto"", ""Name_Localised"":""Karma L-6"", ""Class"":3, ""WeaponMods"":[  ], ""Price"":525000, ""SuitModuleID"":1700537820479008 }",
            @"{ ""timestamp"":""2023-01-22T07:14:13Z"", ""event"":""SellWeapon"", ""Name"":""wpn_s_pistol_kinetic_sauto"", ""Name_Localised"":""Karma P-15"", ""Class"":3, ""WeaponMods"":[ ""weapon_suppression_pressurised"" ], ""Price"":275000, ""SuitModuleID"":1742399366073452 }"
        };

        [PublicAPI(@"The weapon, as an object")]
        public HandWeapon weapon { get; } = weapon;

        [PublicAPI( @"The weapon's grade" )]
        public int grade { get; } = grade;

        [PublicAPI( @"The weapon's sell price" )]
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

                events.Add( new HandWeaponSoldEvent( timestamp, handweapon, grade, price, suitModuleId, mods ) { raw = line, fromLoad = fromLogLoad } );
                return true;
            }

            return false;
        }
    }
}