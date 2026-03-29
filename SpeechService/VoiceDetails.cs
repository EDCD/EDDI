using System;
using System.Globalization;
using Utilities;

namespace EddiSpeechService
{
    [Utilities.PublicAPI]
    public class VoiceDetails : IEquatable<VoiceDetails>
    {
        [Utilities.PublicAPI]
        public string name { get; }

        [Utilities.PublicAPI]
        public string gender { get; }

        [Utilities.PublicAPI]
        public string culturecode { get; }

        public string synthType { get; }

        [Utilities.PublicAPI]
        public string cultureinvariantname { get; }

        [Utilities.PublicAPI]
        public string culturename { get; }

        public bool hideVoice { get; set; }

        internal string cultureTwoLetterISOLanguageName;
        internal string cultureIetfLanguageTag;

        internal VoiceDetails( string displayName, string gender, CultureInfo Culture, string synthType )
        {
            name = displayName;
            this.gender = gender;
            cultureinvariantname = Culture.EnglishName;
            culturename = Culture.NativeName;
            cultureTwoLetterISOLanguageName = Culture.TwoLetterISOLanguageName;
            cultureIetfLanguageTag = Culture.IetfLanguageTag;
            this.synthType = synthType;

            culturecode = BestGuessCulture(Culture);
        }

        private string BestGuessCulture(CultureInfo Culture)
        {
            // Cereproc voices do not support the normal xml:lang attribute country/region codes (like en-GB) 
            // (see https://www.cereproc.com/files/CereVoiceCloudGuide.pdf), 
            // but it does support two letter country codes so we will use those instead.
            // For other voices, we will trust the voice's information (using the complete country/region code).
            var guess = name.Contains("CereVoice") 
                ? Culture.Parent.Name
                : Culture.Name; 
            Logging.Debug($"Best guess culture for {name} is {guess}"); return guess;
        }

        // Implement IEquatable
        public bool Equals ( VoiceDetails other ) => name == other?.name;

        public override bool Equals ( object obj ) => obj is VoiceDetails other && name == other.name;

        public override int GetHashCode() => name.GetHashCode();
    }
}