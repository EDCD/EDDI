using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EddiSpeechService.SpeechProviders
{
    internal static class WebSpeechProviderFilters
    {
        /// <summary>
        /// The voice is allowed if any of its supported locales match any of the locale filters, if no locale filters are specified, or if the voice has no supported locales.
        /// </summary>
        /// <param name="voice"></param>
        /// <param name="localeFilters"></param>
        /// <returns></returns>
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

            var locales = voice.supportedLocales?.Count > 0
                ? voice.supportedLocales
                : voice.culturecode != null
                    ? [ voice.culturecode ]
                    : [];

            return locales.Count == 0 || locales.Any( locale => filters.Any( filter => LocaleMatches( locale, filter ) ) );
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
