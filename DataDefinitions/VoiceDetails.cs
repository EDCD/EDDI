using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    [PublicAPI]
    public class VoiceDetails : IEquatable<VoiceDetails>
    {
        [PublicAPI( "the name of the voice" )]
        public string name { get; }

        [PublicAPI( "the friendly name of the voice" )]
        public string friendlyName { get; }

        [PublicAPI( "the gender of the voice" )]
        public string gender { get; }

        [PublicAPI( "the two letter language code and two letter region code of the voice culture" )]
        public string culturecode { get; }

        [PublicAPI( "the invariant name of the culture" )]
        public string cultureinvariantname { get; }

        [PublicAPI( "the localized name of the culture (as recognized by a native speaker)" )]
        public string culturename { get; }

        [PublicAPI( "the list of locales supported by the voice" )]
        public IReadOnlyList<string> supportedLocales { get; }

        [PublicAPI( "true if the voice is multilingual" )]
        public bool isMultilingual { get; }

        // Not intended to be user facing

        public string synthType { get; }

        public bool hideVoice { get; set; }

        public string voiceKey { get; }

        public string providerProfileId { get; }

        public string providerDisplayName { get; }

        public string providerVoiceId { get; }

        public string cultureTwoLetterISOLanguageName;
        public string cultureIetfLanguageTag;

        public VoiceDetails (
            string displayName,
            string gender,
            CultureInfo Culture,
            string synthType,
            string providerProfileId = null,
            string providerDisplayName = null,
            bool isMultilingual = false,
            IEnumerable<string> supportedLocales = null,
            string providerVoiceId = null,
            string friendlyName = null )
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
            this.providerVoiceId = providerVoiceId;
            this.friendlyName = friendlyName;
            this.isMultilingual = isMultilingual;

            culturecode = BestGuessCulture( Culture );
            this.supportedLocales = ( supportedLocales ?? [ culturecode ] )
                .Where( locale => !string.IsNullOrWhiteSpace( locale ) )
                .Distinct( StringComparer.InvariantCultureIgnoreCase )
                .ToList();
            var providerVoiceKey = string.IsNullOrWhiteSpace( providerVoiceId ) ? name : providerVoiceId;
            voiceKey = string.IsNullOrWhiteSpace( providerProfileId )
                ? name
                : $"{synthType}:{providerProfileId}:{providerVoiceKey}";
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
