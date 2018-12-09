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
        internal static List<Engineer> ENGINEERS = new List<Engineer>();

        public long id { get; set; }

        public string name { get; set; }

        public string stage { get; set; }

        public int? rankprogress { get; set; }

        public int? rank { get; set; }

        private Engineer(long engineerId, string engineerName)
        {
            this.id = engineerId;
            this.name = engineerName;

            ENGINEERS.Add(this);
        }

        public static readonly Engineer DidiVatermann = new Engineer(300000, "Didi Vatermann");
        public static readonly Engineer BillTurner = new Engineer(300010, "Bill Turner");
        public static readonly Engineer BrooTarquin = new Engineer(300030, "Broo Tarquin");
        public static readonly Engineer TheSarge = new Engineer(300040, "The Sarge");
        public static readonly Engineer ZacariahNemo = new Engineer(300050, "Zacariah Nemo");
        public static readonly Engineer LizRyder = new Engineer(300080, "Liz Ryder");
        public static readonly Engineer HeraTani = new Engineer(300090, "Hera Tani");
        public static readonly Engineer FelicityFarseer = new Engineer(300100, "Felicity Farseer");
        public static readonly Engineer RamTah = new Engineer(300110, "Ram Tah");
        public static readonly Engineer LeiCheung = new Engineer(300120, "Lei Cheung");
        public static readonly Engineer PetraOlmanova = new Engineer(300130, "Petra Olmanova");
        public static readonly Engineer ColBrisDekker = new Engineer(300140, "Colonel Bris Dekker");
        public static readonly Engineer MarshaHicks = new Engineer(300150, "Marsha Hicks");
        public static readonly Engineer ElviraMartuuk = new Engineer(300160, "Elvira Martuuk");
        public static readonly Engineer TheDweller = new Engineer(300180, "The Dweller");
        public static readonly Engineer MarcoQwent = new Engineer(300200, "Marco Qwent");
        public static readonly Engineer SeleneJean = new Engineer(300210, "Selene Jean");
        public static readonly Engineer ProfessorPalin = new Engineer(300220, "Professor Palin");
        public static readonly Engineer LoriJameson = new Engineer(300230, "Lori Jameson");
        public static readonly Engineer JuriIshmaak = new Engineer(300250, "Juri Ishmaak");
        public static readonly Engineer TodMcQuinn = new Engineer(300260, "Tod 'The Blaster' McQuinn");
        public static readonly Engineer TianaFortune = new Engineer(300270, "Tiana Fortune");
        public static readonly Engineer MelBrandon = new Engineer(300280, "Mel Brandon");
        public static readonly Engineer EtienneDorn = new Engineer(300290, "Etienne Dorn");

        public static Engineer FromName(string from)
        {
            Engineer result = ENGINEERS.FirstOrDefault(v => v.name == from);
            if (result == null)
            {
                Logging.Info("Unknown Engineer name " + from);
            }
            return result;
        }

        public static Engineer FromNameOrId(string from, long id)
        {
            Engineer result = ENGINEERS.FirstOrDefault(v => v.id == id);
            if (result == null)
            {
                result = ENGINEERS.FirstOrDefault(v => v.name == from);
                if (result == null)
                {
                    Logging.Error("Unknown Engineer name " + from + " EngineerID: " + id);
                }
            }
            return result;
        }

        public Engineer(string name, long engineerId, string progressStage, int? rankProgress, int? rank)
        {
            this.name = name;
            this.id = engineerId;
            this.stage = progressStage;
            this.rankprogress = rankProgress;
            this.rank = rank;
        }

        public static void AddOrUpdate(Engineer engineer)
        {
            Engineer result = Engineer.FromNameOrId(engineer.name, engineer.id);
            if (result == null)
            {
                ENGINEERS.Add(engineer);
            }
            else
            {
                int? index = ENGINEERS.FindIndex(eng => eng.id == engineer.id);
                if (index != null) { ENGINEERS.RemoveAt((int)index); }
                ENGINEERS.Add(engineer);
            }
        }
    }
}
