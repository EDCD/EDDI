using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class HandWeaponUpgraded (
        DateTime timestamp, HandWeapon weapon, int grade, int cost, ulong? suitModuleId )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Hand weapon upgraded";
        public const string DESCRIPTION = "Triggered when upgrading a hand weapon to a new grade";
        public static readonly string[] SAMPLES = {
            @"{ ""timestamp"":""2023-01-22T07:36:39Z"", ""event"":""UpgradeWeapon"", ""Name"":""wpn_m_sniper_plasma_charged"", ""Name_Localised"":""Manticore Executioner"", ""Class"":3, ""SuitModuleID"":1701230606725635, ""Cost"":2625000, ""Resources"":[ { ""Name"":""weaponschematic"", ""Name_Localised"":""Weapon Schematic"", ""Count"":5 }, { ""Name"":""ionisedgas"", ""Name_Localised"":""Ionised Gas"", ""Count"":5 }, { ""Name"":""manufacturinginstructions"", ""Name_Localised"":""Manufacturing Instructions"", ""Count"":5 }, { ""Name"":""chemicalsuperbase"", ""Name_Localised"":""Chemical Superbase"", ""Count"":15 }, { ""Name"":""microelectrode"", ""Count"":15 } ] }",
            @"{ ""timestamp"":""2022-08-21T08:34:15Z"", ""event"":""UpgradeWeapon"", ""Name"":""wpn_s_pistol_kinetic_sauto"", ""Name_Localised"":""Karma P-15"", ""Class"":5, ""SuitModuleID"":1700520378813110, ""Cost"":2500000 }",
            @"{ ""timestamp"":""2023-01-09T05:24:50Z"", ""event"":""UpgradeWeapon"", ""Name"":""wpn_s_pistol_laser_sauto"", ""Name_Localised"":""TK Zenith"", ""Class"":5, ""SuitModuleID"":1701136125680965, ""Cost"":2500000, ""Resources"":[ { ""Name"":""weaponschematic"", ""Name_Localised"":""Weapon Schematic"", ""Count"":15 }, { ""Name"":""ionisedgas"", ""Name_Localised"":""Ionised Gas"", ""Count"":15 }, { ""Name"":""manufacturinginstructions"", ""Name_Localised"":""Manufacturing Instructions"", ""Count"":15 }, { ""Name"":""microelectrode"", ""Count"":35 }, { ""Name"":""opticalfibre"", ""Name_Localised"":""Optical Fibre"", ""Count"":35 } ] }",
            @"{ ""timestamp"":""2024-09-23T04:13:09Z"", ""event"":""UpgradeWeapon"", ""Name"":""wpn_m_submachinegun_kinetic_fauto"", ""Name_Localised"":""Karma C-44"", ""Class"":5, ""SuitModuleID"":1701137241732532, ""Cost"":3750000, ""Resources"":[ { ""Name"":""weaponschematic"", ""Name_Localised"":""Weapon Schematic"", ""Count"":5 }, { ""Name"":""compressionliquefiedgas"", ""Name_Localised"":""Compression-Liquefied Gas"", ""Count"":5 }, { ""Name"":""manufacturinginstructions"", ""Name_Localised"":""Manufacturing Instructions"", ""Count"":5 }, { ""Name"":""tungstencarbide"", ""Name_Localised"":""Tungsten Carbide"", ""Count"":12 }, { ""Name"":""weaponcomponent"", ""Name_Localised"":""Weapon Component"", ""Count"":12 } ] }",
            @"{ ""timestamp"":""2024-09-23T04:14:01Z"", ""event"":""UpgradeWeapon"", ""Name"":""wpn_m_submachinegun_laser_fauto"", ""Name_Localised"":""TK Eclipse"", ""Class"":4, ""SuitModuleID"":1701623826231187, ""Cost"":2250000, ""Resources"":[ { ""Name"":""weaponschematic"", ""Name_Localised"":""Weapon Schematic"", ""Count"":4 }, { ""Name"":""ionisedgas"", ""Name_Localised"":""Ionised Gas"", ""Count"":4 }, { ""Name"":""manufacturinginstructions"", ""Name_Localised"":""Manufacturing Instructions"", ""Count"":4 }, { ""Name"":""microelectrode"", ""Count"":9 }, { ""Name"":""opticalfibre"", ""Name_Localised"":""Optical Fibre"", ""Count"":9 } ] }"
        };

        [PublicAPI(@"The weapon, as an object")]
        public HandWeapon weapon { get; } = weapon;

        [PublicAPI( @"The weapon's new grade" )]
        public int grade { get; } = grade;

        [PublicAPI( @"The weapon's upgrade cost" )]
        public int cost { get; } = cost;

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
                var cost = JsonParsing.getInt( data, "Cost" );
                var suitModuleId = JsonParsing.getOptionalULong( data, "SuitModuleID" );
                // No need to worry about spent resources, a separate event will keep our microresource inventory up to date

                events.Add( new HandWeaponUpgraded( timestamp, handweapon, grade, cost, suitModuleId ) { raw = line, fromLoad = fromLogLoad } );
                return true;
            }

            return false;
        }
    }
}