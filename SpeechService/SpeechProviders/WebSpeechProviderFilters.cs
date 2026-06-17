using System;
using System.Collections.Generic;
using System.Linq;

namespace EddiSpeechService.SpeechProviders
{
    internal static class WebSpeechProviderFilters
    {
        internal static bool IsVoiceAllowed ( VoiceDetails voice, IEnumerable<string> localeFilters )
        {
            var filters = localeFilters?
                .Where( filter => !string.IsNullOrWhiteSpace( filter ) )
                .Select( filter => filter.Trim() )
                .ToList() ?? [];

            if ( filters.Count == 0 )
            {
                return true;
            }

            if ( voice.isMultilingual )
            {
                return true;
            }

            var locales = voice.supportedLocales?.Count > 0
                ? voice.supportedLocales
                : [ voice.culturecode ];

            return locales.Any( locale => filters.Any( filter => LocaleMatches( locale, filter ) ) );
        }

        private static bool LocaleMatches ( string locale, string filter )
        {
            if ( string.IsNullOrWhiteSpace( locale ) || string.IsNullOrWhiteSpace( filter ) )
            {
                return false;
            }

            return locale.Equals( filter, StringComparison.InvariantCultureIgnoreCase ) ||
                   locale.StartsWith( $"{filter}-", StringComparison.InvariantCultureIgnoreCase );
        }
    }
}
