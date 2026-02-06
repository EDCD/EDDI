using Newtonsoft.Json;
using System.Windows;

namespace EddiConfigService.Configurations
{
    /// <summary>Configuration for the speech responder</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\speechresponder.json")]
    public class SpeechResponderConfiguration : Config
    {
        private Rect _editScriptWindowPosition = new Rect( 300, 200, 800, 600 );
        private bool _subtitlesOnly;
        private bool _subtitles;
        private string _personality = "EDDI";

        [ JsonProperty( "personality" ) ]
        public string Personality
        {
            get => _personality;
            set
            {
                if ( value == _personality )
                {
                    return;
                }

                _personality = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "subtitles" ) ]
        public bool Subtitles
        {
            get => _subtitles;
            set
            {
                if ( value == _subtitles )
                {
                    return;
                }

                _subtitles = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "subtitlesonly" ) ]
        public bool SubtitlesOnly
        {
            get => _subtitlesOnly;
            set
            {
                if ( value == _subtitlesOnly )
                {
                    return;
                }

                _subtitlesOnly = value;
                OnPropertyChanged();
            }
        }

        // Window Properties

        [ JsonProperty( "EditScriptWindowPosition" ) ]
        public Rect EditScriptWindowPosition
        {
            get => _editScriptWindowPosition;
            set
            {
                if ( value.Equals( _editScriptWindowPosition ) )
                {
                    return;
                }

                _editScriptWindowPosition = value;
                OnPropertyChanged();
            }
        }
    }
}
