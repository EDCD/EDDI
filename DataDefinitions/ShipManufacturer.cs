using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public class ShipManufacturer
    {
        public static readonly ShipManufacturer CoreDynamics = new( "Core Dynamics" );
        public static readonly ShipManufacturer FaulconDeLacy = new( "Faulcon DeLacy" );
        public static readonly ShipManufacturer Gutamaya = new( "Gutamaya",
            [ new Translation( "Gutamaya", "guːtəˈmaɪə" ) ] );
        public static readonly ShipManufacturer LakonSpaceways = new( "Lakon Spaceways",
            [ new Translation( "Lakon", "leɪkɒn" ), new Translation( "Spaceways", "speɪsweɪz" ) ] );
        public static readonly ShipManufacturer SaudKruger = new( "Saud Kruger",
            [ new Translation( "Saud", "saʊd" ), new Translation( "Kruger", "ˈkruːɡə" ) ] );
        public static readonly ShipManufacturer ZorgonPeterson = new( "Zorgon Peterson" );

        public static readonly List<ShipManufacturer> AllOfThem =
        [
            CoreDynamics,
            FaulconDeLacy,
            Gutamaya,
            LakonSpaceways,
            SaudKruger,
            ZorgonPeterson
        ];

        public string name { get; }

        public List<Translation> phoneticName { get; }

        private ShipManufacturer ( string name, List<Translation> phoneticName = null )
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
