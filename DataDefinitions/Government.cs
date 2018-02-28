using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Government types
    /// </summary>
    public class Government
    {
        private static readonly List<Government> GOVERNMENTS = new List<Government>();

        public string name { get; private set; }

        public string edname { get; private set; }

        private Government(string edname, string name)
        {
            this.edname = edname;
            this.name = name;

            GOVERNMENTS.Add(this);
        }

        public static readonly Government None = new Government("$government_None", "None");
        public static readonly Government Anarchy = new Government("$government_Anarchy", "Anarchy");
        public static readonly Government Colony = new Government("$government_Colony", "Colony");
        public static readonly Government Communism = new Government("$government_Communism", "Communism");
        public static readonly Government Confederacy = new Government("$government_Confederacy", "Confederacy");
        public static readonly Government Cooperative = new Government("$government_Cooperative", "Cooperative");
        public static readonly Government Corporate = new Government("$government_Corporate", "Corporate");
        public static readonly Government Democracy = new Government("$government_Democracy", "Democracy");
        public static readonly Government Dictatorship = new Government("$government_Dictatorship", "Dictatorship");
        public static readonly Government Feudal = new Government("$government_Feudal", "Feudal");
        public static readonly Government Imperial = new Government("$government_Imperial", "Imperial");
        public static readonly Government Patronage = new Government("$government_Patronage", "Patronage");
        public static readonly Government Prison = new Government("$government_Prison", "Prison");
        public static readonly Government PrisonColony = new Government("$government_PrisonColony", "Prison Colony");
        public static readonly Government Theocracy = new Government("$government_Theocracy", "Theocracy");
        public static readonly Government Workshop = new Government("$government_Workshop", "Workshop");
        public static readonly Government Engineer = new Government("$government_engineer", "Engineer");

        public static Government FromName(string from)
        {
            if (from == null)
            {
                return null;
            }

            Government result = GOVERNMENTS.FirstOrDefault(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown Government name " + from);
            }
            return result;
        }

        public static Government FromEDName(string from)
        {
            if (from == null)
            {
                return null;
            }

            string tidiedFrom = from == null ? null : from.Replace(";", "").ToLowerInvariant();
            Government result = GOVERNMENTS.FirstOrDefault(v => v.edname.ToLowerInvariant() == tidiedFrom);
            if (result == null)
            {
                Logging.Report("Unknown Government ED name " + from);
                result = new Government(from, tidiedFrom);
            }
            return result;
        }
    }
}
