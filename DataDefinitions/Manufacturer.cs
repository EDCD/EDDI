using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public class Manufacturer
    {
        public static readonly Manufacturer CoreDynamics = new( "Core Dynamics", [ new Translation( "Core", "kɔɹ" ), new Translation( "Dynamics", "dəˈnamɪks" ) ] );
        public static readonly Manufacturer FaulconDeLacy = new( "Faulcon DeLacy", [ new Translation( "Falcon", "ˈfælkən" ), new Translation( "Delacy", "dᵻlˈæ.si" ) ] );
        public static readonly Manufacturer Gutamaya = new( "Gutamaya", [ new Translation( "Gutamaya", "guːtəˈmaɪə" ) ] );
        public static readonly Manufacturer KinematicArmaments = new( "Kinematic Armaments" );
        public static readonly Manufacturer LakonSpaceways = new( "Lakon Spaceways", [ new Translation( "Lakon", "leɪkɒn" ), new Translation( "Spaceways", "speɪsweɪz" ) ] );
        public static readonly Manufacturer Manticore = new( "Manticore" );
        public static readonly Manufacturer Remlok = new( "Remlok" );
        public static readonly Manufacturer SaudKruger = new( "Saud Kruger", [ new Translation( "Saud", "saʊd" ), new Translation( "Kruger", "ˈkruːɡə" ) ] );
        public static readonly Manufacturer Supratech = new( "Supratech", [ new Translation( "Supratech", "su.pɹətɛk" ) ] );
        public static readonly Manufacturer Takada = new( "Takada", [ new Translation( "Takada", "t.ækɑːdə" ) ] );
        public static readonly Manufacturer ZorgonPeterson = new( "Zorgon Peterson" );

        public static readonly List<Manufacturer> AllOfThem =
        [
            CoreDynamics,
            FaulconDeLacy,
            Gutamaya,
            KinematicArmaments,
            LakonSpaceways,
            Manticore,
            Remlok,
            SaudKruger,
            Supratech,
            Takada,
            ZorgonPeterson
        ];

        [Utilities.PublicAPI( "The manufacturer name" )]
        public string name { get; }

        [Utilities.PublicAPI( "The phonetic name of the manufacturer, if it is known" ), UsedImplicitly]
        public string phoneticname => SpokenManufacturer( name ) ?? name;

        // Not intended to be user facing

        public List<Translation> phoneticName { get; }

        private Manufacturer ( string name, List<Translation> phoneticName = null )
        {
            this.name = name;
            this.phoneticName = phoneticName;
        }

        public static string SpokenManufacturer(string manufacturer)
        {
            var phoneticmanufacturer = AllOfThem.FirstOrDefault(m => m.name == manufacturer)?.phoneticName;
            if (phoneticmanufacturer != null)
            {
                var result = "";
                foreach (var item in phoneticmanufacturer)
                {
                    result += "<phoneme alphabet=\"ipa\" ph=\"" + item.to + "\">" + item.from + "</phoneme> ";
                }
                return result;
            }
            // Model isn't in the dictionary
            return null;
        }
    }
}
