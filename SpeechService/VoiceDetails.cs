using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Utilities;

namespace EddiSpeechService
{
    [PublicAPI]
    public class VoiceDetails : IEquatable<VoiceDetails>
    {
        [PublicAPI]
        public string name { get; }

        [PublicAPI]
        public string gender { get; }

        [PublicAPI]
        public string culturecode { get; }

        public string synthType { get; }

        [PublicAPI]
        public string cultureinvariantname { get; }

        [PublicAPI]
        public string culturename { get; }

        public bool hideVoice { get; set; }

        public string voiceKey { get; }

        public string providerProfileId { get; }

        public string providerDisplayName { get; }

        public IReadOnlyList<string> supportedLocales { get; }

        public bool isMultilingual { get; }

        internal string cultureTwoLetterISOLanguageName;
        internal string cultureIetfLanguageTag;

        internal VoiceDetails (
            string displayName,
            string gender,
            CultureInfo Culture,
            string synthType,
            string providerProfileId = null,
            string providerDisplayName = null,
            bool isMultilingual = false,
            IEnumerable<string> supportedLocales = null )
        {
            name = displayName;
            this.gender = gender;
            cultureinvariantname = Culture.EnglishName;
            culturename = Culture.NativeName;
            cultureTwoLetterISOLanguageName = Culture.TwoLetterISOLanguageName;
            cultureIetfLanguageTag = Culture.IetfLanguageTag;
            this.synthType = synthType;
            this.providerProfileId = providerProfileId;
            this.providerDisplayName = providerDisplayName;
            this.isMultilingual = isMultilingual;

            culturecode = BestGuessCulture( Culture );
            this.supportedLocales = ( supportedLocales ?? [ culturecode ] )
                .Where( locale => !string.IsNullOrWhiteSpace( locale ) )
                .Distinct( StringComparer.InvariantCultureIgnoreCase )
                .ToList();
            voiceKey = string.IsNullOrWhiteSpace( providerProfileId )
                ? name
                : $"{synthType}:{providerProfileId}:{name}";
        }

        private string BestGuessCulture ( CultureInfo Culture )
        {
            // Cereproc voices do not support the normal xml:lang attribute country/region codes (like en-GB)
            // (see https://www.cereproc.com/files/CereVoiceCloudGuide.pdf),
            // but it does support two letter country codes so we will use those instead.
            // For other voices, we will trust the voice's information (using the complete country/region code).
            var guess = name.Contains( "CereVoice" )
                ? Culture.Parent.Name
                : Culture.Name;
            Logging.Debug( $"Best guess culture for {name} is {guess}" );
            return guess;
        }

        // Implement IEquatable
        public bool Equals ( VoiceDetails other ) => voiceKey == other?.voiceKey;

        public override bool Equals ( object obj ) => obj is VoiceDetails other && voiceKey == other.voiceKey;

        public override int GetHashCode() => voiceKey.GetHashCode();
    }
}
