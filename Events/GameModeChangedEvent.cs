using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class GameModeChangedEvent ( DateTime timestamp, string gameMode ) : Event( timestamp, NAME )
    {
        public const string NAME = "Game mode changed";
        public const string DESCRIPTION = "Triggered when joining or leaving an Operation or CQC combat";
        public static readonly string[] SAMPLES =
        {
            @"{ ""timestamp"" : ""2026-09-02T11:37:37Z"", ""event"" : ""GameModeChange"", ""GameMode"" : ""MainGame"" }",
            @"{ ""timestamp"" : ""2026-09-02T11:37:37Z"", ""event"" : ""GameModeChange"", ""GameMode"" : ""Operation"" }",
            @"{ ""timestamp"":""2026-09-06T08:27:10Z"", ""event"":""GameModeChange"", ""GameMode"":""ProvingGrounds"" }" // CQC
        };

        [PublicAPI("The game mode that you are entering. Expected values are 'MainGame', 'Operation', and 'ProvingGrounds' (i.e. CQC)")]
        public string gameMode { get; set; } = gameMode;

        // Not intended to be user facing
        private static readonly List<string> known_game_modes = [ "MainGame", "Operation", "ProvingGrounds" ];

        public static bool Handle ( DateTime timestamp, IDictionary<string, object> data, ref List<Event> events )
        {
            var gameMode = JsonParsing.getString(data, "GameMode");
            if (!known_game_modes.Contains(gameMode))
            {
                Logging.Error( $"Unknown game mode '{gameMode}' in GameModeChangedEvent" );
            }
            events.Add( new GameModeChangedEvent( timestamp, gameMode ) );
            return true;
        }
    }
}
