using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Engineers
    /// </summary>
    public class Engineer
    {
        private static readonly List<Engineer> ENGINEERS = new List<Engineer>();

        public long engineerId { get; private set; }

        public string engineerName { get; private set; }

        private Engineer(long engineerId, string engineerName)
        {
            this.engineerId = engineerId;
            this.engineerName = engineerName;

            ENGINEERS.Add(this);
        }

        public static readonly Engineer DidiVatermann = new Engineer(300000, "Didi Vatermann");
        public static readonly Engineer BillTurner = new Engineer(300010, "Didi Vatermann");
        public static readonly Engineer BrooTarquin = new Engineer(300030, "Broo Tarquin");
        public static readonly Engineer TheSarge = new Engineer(300040, "The Sarge");
        public static readonly Engineer ZachariahNemo = new Engineer(300040, "Zachariah Nemo");
        public static readonly Engineer LizRyder = new Engineer(300080, "Liz Ryder");
        public static readonly Engineer HeraTani = new Engineer(300090, "Hera Tani");
        public static readonly Engineer FelicityFarseer = new Engineer(300100, "Felicity Farseer");
        public static readonly Engineer RamTah = new Engineer(300110, "Ram Tah");
        public static readonly Engineer LeiCheung = new Engineer(300120, "Lei Cheung");
        public static readonly Engineer ColBrisDekker = new Engineer(300140, "Colonel Bris Dekker");
        public static readonly Engineer ElivraMartuuk = new Engineer(300160, "Elivra Martuuk");
        public static readonly Engineer TheDweller = new Engineer(300180, "The Dweller");
        public static readonly Engineer MarcoQuent = new Engineer(300200, "Marco Quent");
        public static readonly Engineer SeleneJean = new Engineer(300210, "Selene Jean");
        public static readonly Engineer ProfessorPalin = new Engineer(300220, "Professor Palin");
        public static readonly Engineer LoriJameson = new Engineer(300230, "Lori Jameson");
        public static readonly Engineer JuriIshmaak = new Engineer(300250, "Juri Ishmaak");
        public static readonly Engineer TodMcQuinn = new Engineer(300260, "Tod 'The Blaster' McQuinn");
        public static readonly Engineer TianaFortune = new Engineer(300270, "Tiana Fortune");

        public static Engineer FromId(long from)
        {

            Engineer result = ENGINEERS.FirstOrDefault(v => v.engineerId == from);
            if (result == null)
            {
                Logging.Report("Unknown Engineer ID " + from);
            }
            return result;
        }

        public static Engineer FromName(string from)
        {

            Engineer result = ENGINEERS.FirstOrDefault(v => v.engineerName == from);
            if (result == null)
            {
                Logging.Report("Unknown Engineer name " + from);
            }
            return result;
        }
    }
}
