using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class EngineerProgressedEvent ( DateTime timestamp, Engineer engineer, string type )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Engineer progressed";
        public const string DESCRIPTION = "Triggered at startup and when you reach a new rank with an engineer";
        public const string SAMPLE = @"{ ""timestamp"":""2018-01-16T09:34:36Z"", ""event"":""EngineerProgress"", ""Engineer"":""Zacariah Nemo"", ""EngineerID"":300050, ""Rank"":4, ""RankProgress"":0 }";

        [PublicAPI("The name of the engineer with whom you have progressed (not written at startup)")]
        public string engineer => Engineer?.name;

        [PublicAPI("The rank of your relationship with the engineer (not written at startup)")]
        public int? rank => Engineer?.rank;

        [PublicAPI("The current stage of your relations with the engineer (Invited/Known/Unlocked/Barred) (not written at startup)")]
        public string stage => Engineer?.stage;

        [PublicAPI("The type of progress that is applicable (Rank/Stage) (not written at startup)")]
        public string progresstype { get; private set; } = type;

        // Not intended to be user facing
        
        public Engineer Engineer { get; private set; } = engineer;

        // Omitted, only written at startup and for an array of engineers rather than for a single engineer.
        public int? rankprogress => Engineer?.rankprogress;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            data.TryGetValue( "Engineers", out var val );
            if ( val != null )
            {
                // This is a startup entry. 
                // Update engineer progress / status data
                var engineers = (List<object>)val;
                foreach ( var engineerData in engineers.Cast<IDictionary<string, object>>() )
                {
                    var engineer = EventParsing.Engineer(engineerData);
                    if ( !string.IsNullOrEmpty( engineer?.name ) )
                    {
                        Engineer.AddOrUpdate( engineer );
                    }
                }
                // Generate an event to pass the data
                events.Add( new EngineerProgressedEvent( timestamp, null, null ) { raw = line, fromLoad = fromLogLoad } );
            }
            else
            {
                // This is a progress entry.
                var engineer = EventParsing.Engineer(data);
                var lastEngineer = Engineer.FromNameOrId(engineer.name, engineer.id).Copy();
                Engineer.AddOrUpdate( engineer );
                if ( engineer.stage != null && engineer.stage != lastEngineer?.stage )
                {
                    events.Add( new EngineerProgressedEvent( timestamp, engineer, "Stage" ) { raw = line, fromLoad = fromLogLoad } );
                }
                if ( engineer.rank != null && engineer.rank != lastEngineer?.rank )
                {
                    events.Add( new EngineerProgressedEvent( timestamp, engineer, "Rank" ) { raw = line, fromLoad = fromLogLoad } );
                }
            }

            return true;
        }
    }
}
