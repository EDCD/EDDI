using System;
using System.Globalization;
using Utilities;

namespace EddiSpeechService.SpeechConversions
{
    public static partial class SpeechConversions
    {
        /// <summary>
        /// Present a number's approximate value in a format suitable for text-to-speech
        /// </summary>
        /// <param name="rawValue">The value to be formatted. If null, returns null.</param>
        /// <param name="forceIntegerMantissa">true: always express the mantissa as an integer; false: allow the mantissa to be a short decimal; null: lookup behavior from localized resource</param>
        /// <returns>A string being the number's approximate value in a format suitable for text-to-speech</returns>
        public static string Humanize(decimal? rawValue, bool? forceIntegerMantissa = null)
        {
            if (rawValue == null) {return null;}
            var value = (decimal)rawValue;
            if ( value == 0 )
            {
                var result = Properties.Phrases.zero;
                Logging.Debug( $"Converted raw value '{rawValue}' to '{result}'" );
                return result;
            }

            var wantIntegerMantissa = forceIntegerMantissa ?? Properties.FormatOverrides.forceIntegerMantissa.Equals("true");

            var isNegative = value < 0;
            if (isNegative)
            {
                value = -value;
            }

            if (value < 10)
            {
                // Work out how many 0s to begin with
                var numzeros = -1;
                while (value < 1)
                {
                    value *= 10;
                    numzeros++;
                }

                // Now round it to 2sf
                var result = ( isNegative ? Properties.Phrases.minus + " " : "" ) +
                             ( Math.Round( value * 10 ) / (decimal)Math.Pow( 10, numzeros + 2 ) );
                Logging.Debug($"Converted raw value '{rawValue}' to '{result}'");
                return result;
            }

            var magnitude = Math.Log10((double) value);
            var orderMultiplier = (long) Math.Pow(10, Math.Floor(magnitude / 3) * 3);
            var (number, nextDigit) = Normalize(value, orderMultiplier);

            // See if we have a whole number that is fully described within the largest order
            if (number * orderMultiplier == Math.Abs(value))
            {
                // Some languages render these differently than others. "1000" in English is "one thousand" but in Italian is simply "mille".
                // Consequently, we leave the interpretation to the culture-specific voice.
                var result = FormatVerbatim(number, isNegative, orderMultiplier);
                Logging.Debug( $"Converted raw value '{rawValue}' to '{result}'" );
                return result;
            }

            if (number < 100)
            {
                var result = FormatWith2SignificantDigits(number, isNegative, orderMultiplier, nextDigit, value, wantIntegerMantissa);
                Logging.Debug( $"Converted raw value '{rawValue}' to '{result}'" );
                return result;
            }
            else // Describe (less precisely) values for numbers where the largest order number exceeds one hundred
            {
                var result = FormatWith3SignificantDigits(number, isNegative, orderMultiplier, nextDigit, value);
                Logging.Debug( $"Converted raw value '{rawValue}' to '{result}'" );
                return result;
            }
        }

        private static (int number, int nextDigit) Normalize(decimal inputValue, decimal orderMultiplierVal)
        {
            return (number: (int) (inputValue / orderMultiplierVal), nextDigit: (int) (inputValue % orderMultiplierVal / (orderMultiplierVal / 10)));
        }

        private static string FormatWith2SignificantDigits(int number, bool isNegative, long orderMultiplier, int nextDigit, decimal value, bool wantIntegerMantissa)
        {
            // See if we have a number whose value can be expressed with a short decimal (i.e 1.3 million)
            var shortDecimal = (number + ((decimal) nextDigit / 10));
            if (shortDecimal == Math.Round(value / orderMultiplier, 2))
            {
                switch (wantIntegerMantissa)
                {
                case true when orderMultiplier >= 1000:
                    // borrow a factor of 1000 from orderMultiplier and multiply the mantissa by it
                    number = (number * 1000) + (nextDigit * 100);
                    orderMultiplier /= 1000;
                    return FormatVerbatim(number, isNegative, orderMultiplier);
                default:
                    if (nextDigit == 0)
                    {
                        return FormatVerbatim(number, isNegative, orderMultiplier);
                    }
                    return FormatAsShortDecimal(shortDecimal, isNegative, orderMultiplier);
                }
            }

            // Describe values for numbers where the mantissa does not exceed one hundred
            return nextDigit switch
            {
                1 => FormatAsJustOver( number, isNegative, orderMultiplier, value ),
                2 => FormatAsOver( number, isNegative, orderMultiplier, value ),
                3 => FormatAsWellOver( number, isNegative, orderMultiplier, value ),
                4 => FormatAsNearlyOneAndAHalf( number, isNegative, orderMultiplier, value ),
                5 => FormatAsAroundOneAndAHalf( number, isNegative, orderMultiplier, value ),
                6 or 7 => FormatAsOverOneAndAHalf( number, isNegative, orderMultiplier, value ),
                8 => FormatAsWellOverOneAndAHalf( number, isNegative, orderMultiplier, value ),
                9 => FormatAsNearly( number + 1, isNegative, orderMultiplier, value ),
                _ => FormatVerbatim( number, isNegative, orderMultiplier )
            };
        }

        private static string FormatWith3SignificantDigits(int number, bool isNegative, long orderMultiplier, int nextDigit,
            decimal value)
        {
            // Round mantissas in the hundreds to the nearest 10, except where the number after the hundreds place is 20 or less
            if ((number - ((int)((decimal)number / 100) * 100)) >= 20)
            {
                (number, nextDigit) = Normalize(number, 10);
                number *= 10;
            }

            return nextDigit switch
            {
                1 => FormatAsJustOver( number, isNegative, orderMultiplier, value ),
                2 or 3 or 4 or 5 or 6 => FormatAsOver( number, isNegative, orderMultiplier, value ),
                7 or 8 or 9 => FormatAsNearly( number + 1, isNegative, orderMultiplier, value ),
                _ => FormatVerbatim( number, isNegative, orderMultiplier )
            };
        }

        private static CultureInfo _formattingCultureInfo;
        private static CultureInfo formattingCultureInfo
        {
            get
            {
                if (_formattingCultureInfo == null)
                {
                    _formattingCultureInfo = (CultureInfo)CultureInfo.CurrentUICulture.Clone();
                    if (Properties.FormatOverrides.overrideThousandsSeparator.Equals("true"))
                    {
                        _formattingCultureInfo.NumberFormat.NumberGroupSeparator = Properties.FormatOverrides.thousandsSeparator;
                    }
                }

                return _formattingCultureInfo;
            }
        }

        private static string FormatVerbatim(int number, bool isNegative, long orderMultiplier)
        {
            var value = number * orderMultiplier;
            // some TTS voices need the thousands separators, so use format string "N0" (numeric, zero decimal places)
            return (isNegative ? Properties.Phrases.minus + " " : "") + value.ToString("N0", formattingCultureInfo);
        }

        private static string FormatAsShortDecimal(decimal shortDecimal, bool isNegative, long orderMultiplier)
        {
            return (decimal)orderMultiplier switch
            {
                1 => ( isNegative ? Properties.Phrases.minus + " " : "" ) + shortDecimal,
                1E3M => ( isNegative ? Properties.Phrases.minus + " " : "" ) +
                        string.Format( Properties.Phrases.shortDecimalThousand, shortDecimal ),
                1E6M => ( isNegative ? Properties.Phrases.minus + " " : "" ) +
                        string.Format( Properties.Phrases.shortDecimalMillion, shortDecimal ),
                1E9M => ( isNegative ? Properties.Phrases.minus + " " : "" ) +
                        string.Format( Properties.Phrases.shortDecimalBillion, shortDecimal ),
                1E12M => ( isNegative ? Properties.Phrases.minus + " " : "" ) +
                         string.Format( Properties.Phrases.shortDecimalTrillion, shortDecimal ),
                1E15M => ( isNegative ? Properties.Phrases.minus + " " : "" ) +
                         string.Format( Properties.Phrases.shortDecimalQuadrillion, shortDecimal ),
                1E18M => ( isNegative ? Properties.Phrases.minus + " " : "" ) +
                         string.Format( Properties.Phrases.shortDecimalQuintillion, shortDecimal ),
                _ => ( isNegative ? Properties.Phrases.minus + " " : "" ) + ( shortDecimal * orderMultiplier )
            };
        }

        private static string FormatAsJustOver(int number, bool isNegative, long orderMultiplier, decimal value)
        {
            return (decimal)orderMultiplier switch
            {
                1 => isNegative
                    ? string.Format( Properties.Phrases.justOverMinus, number )
                    : string.Format( Properties.Phrases.justOver, number ),
                1E3M => isNegative
                    ? string.Format( Properties.Phrases.justOverMinusThousand, number )
                    : string.Format( Properties.Phrases.justOverThousand, number ),
                1E6M => isNegative
                    ? string.Format( Properties.Phrases.justOverMinusMillion, number )
                    : string.Format( Properties.Phrases.justOverMillion, number ),
                1E9M => isNegative
                    ? string.Format( Properties.Phrases.justOverMinusBillion, number )
                    : string.Format( Properties.Phrases.justOverBillion, number ),
                1E12M => isNegative
                    ? string.Format( Properties.Phrases.justOverMinusTrillion, number )
                    : string.Format( Properties.Phrases.justOverTrillion, number ),
                1E15M => isNegative
                    ? string.Format( Properties.Phrases.justOverMinusQuadrillion, number )
                    : string.Format( Properties.Phrases.justOverQuadrillion, number ),
                1E18M => isNegative
                    ? string.Format( Properties.Phrases.justOverMinusQuintillion, number )
                    : string.Format( Properties.Phrases.justOverQuintillion, number ),
                _ => $"{value}"
            };
        }

        private static string FormatAsOver(int number, bool isNegative, long orderMultiplier, decimal value)
        {
            return (decimal)orderMultiplier switch
            {
                1 => isNegative
                    ? string.Format( Properties.Phrases.overMinus, number )
                    : string.Format( Properties.Phrases.over, number ),
                1E3M => isNegative
                    ? string.Format( Properties.Phrases.overMinusThousand, number )
                    : string.Format( Properties.Phrases.overThousand, number ),
                1E6M => isNegative
                    ? string.Format( Properties.Phrases.overMinusMillion, number )
                    : string.Format( Properties.Phrases.overMillion, number ),
                1E9M => isNegative
                    ? string.Format( Properties.Phrases.overMinusBillion, number )
                    : string.Format( Properties.Phrases.overBillion, number ),
                1E12M => isNegative
                    ? string.Format( Properties.Phrases.overMinusTrillion, number )
                    : string.Format( Properties.Phrases.overTrillion, number ),
                1E15M => isNegative
                    ? string.Format( Properties.Phrases.overMinusQuadrillion, number )
                    : string.Format( Properties.Phrases.overQuadrillion, number ),
                1E18M => isNegative
                    ? string.Format( Properties.Phrases.overMinusQuintillion, number )
                    : string.Format( Properties.Phrases.overQuintillion, number ),
                _ => $"{value}"
            };
        }

        private static string FormatAsWellOver(int number, bool isNegative, long orderMultiplier, decimal value)
        {
            return (decimal)orderMultiplier switch
            {
                1 => isNegative
                    ? string.Format( Properties.Phrases.wellOverMinus, number )
                    : string.Format( Properties.Phrases.wellOver, number ),
                1E3M => isNegative
                    ? string.Format( Properties.Phrases.wellOverMinusThousand, number )
                    : string.Format( Properties.Phrases.wellOverThousand, number ),
                1E6M => isNegative
                    ? string.Format( Properties.Phrases.wellOverMinusMillion, number )
                    : string.Format( Properties.Phrases.wellOverMillion, number ),
                1E9M => isNegative
                    ? string.Format( Properties.Phrases.wellOverMinusBillion, number )
                    : string.Format( Properties.Phrases.wellOverBillion, number ),
                1E12M => isNegative
                    ? string.Format( Properties.Phrases.wellOverMinusTrillion, number )
                    : string.Format( Properties.Phrases.wellOverTrillion, number ),
                1E15M => isNegative
                    ? string.Format( Properties.Phrases.wellOverMinusQuadrillion, number )
                    : string.Format( Properties.Phrases.wellOverQuadrillion, number ),
                1E18M => isNegative
                    ? string.Format( Properties.Phrases.wellOverMinusQuintillion, number )
                    : string.Format( Properties.Phrases.wellOverQuintillion, number ),
                _ => $"{value}"
            };
        }

        private static string FormatAsNearlyOneAndAHalf(int number, bool isNegative, long orderMultiplier, decimal value)
        {
            return (decimal)orderMultiplier switch
            {
                1 => isNegative
                    ? string.Format( Properties.Phrases.nearlyMinusAndAHalf, number )
                    : string.Format( Properties.Phrases.nearlyAndAHalf, number ),
                1E3M => isNegative
                    ? string.Format( Properties.Phrases.nearlyMinusThousandAndAHalf, number )
                    : string.Format( Properties.Phrases.nearlyThousandAndAHalf, number ),
                1E6M => isNegative
                    ? string.Format( Properties.Phrases.nearlyMinusMillionAndAHalf, number )
                    : string.Format( Properties.Phrases.nearlyMillionAndAHalf, number ),
                1E9M => isNegative
                    ? string.Format( Properties.Phrases.nearlyMinusBillionAndAHalf, number )
                    : string.Format( Properties.Phrases.nearlyBillionAndAHalf, number ),
                1E12M => isNegative
                    ? string.Format( Properties.Phrases.nearlyMinusTrillionAndAHalf, number )
                    : string.Format( Properties.Phrases.nearlyTrillionAndAHalf, number ),
                1E15M => isNegative
                    ? string.Format( Properties.Phrases.nearlyMinusQuadrillionAndAHalf, number )
                    : string.Format( Properties.Phrases.nearlyQuadrillionAndAHalf, number ),
                1E18M => isNegative
                    ? string.Format( Properties.Phrases.nearlyMinusQuintillionAndAHalf, number )
                    : string.Format( Properties.Phrases.nearlyQuintillionAndAHalf, number ),
                _ => $"{value}"
            };
        }

        private static string FormatAsAroundOneAndAHalf(int number, bool isNegative, long orderMultiplier, decimal value)
        {
            return (decimal)orderMultiplier switch
            {
                1 => isNegative
                    ? string.Format( Properties.Phrases.aroundMinusAndAHalf, number )
                    : string.Format( Properties.Phrases.aroundAndAHalf, number ),
                1E3M => isNegative
                    ? string.Format( Properties.Phrases.aroundMinusAndAHalfThousand, number )
                    : string.Format( Properties.Phrases.aroundAndAHalfThousand, number ),
                1E6M => isNegative
                    ? string.Format( Properties.Phrases.aroundMinusAndAHalfMillion, number )
                    : string.Format( Properties.Phrases.aroundAndAHalfMillion, number ),
                1E9M => isNegative
                    ? string.Format( Properties.Phrases.aroundMinusAndAHalfBillion, number )
                    : string.Format( Properties.Phrases.aroundAndAHalfBillion, number ),
                1E12M => isNegative
                    ? string.Format( Properties.Phrases.aroundMinusAndAHalfTrillion, number )
                    : string.Format( Properties.Phrases.aroundAndAHalfTrillion, number ),
                1E15M => isNegative
                    ? string.Format( Properties.Phrases.aroundMinusAndAHalfQuadrillion, number )
                    : string.Format( Properties.Phrases.aroundAndAHalfQuadrillion, number ),
                1E18M => isNegative
                    ? string.Format( Properties.Phrases.aroundMinusAndAHalfQuintillion, number )
                    : string.Format( Properties.Phrases.aroundAndAHalfQuintillion, number ),
                _ => $"{value}"
            };
        }

        private static string FormatAsOverOneAndAHalf(int number, bool isNegative, long orderMultiplier, decimal value)
        {
            return (decimal)orderMultiplier switch
            {
                1 => isNegative
                    ? string.Format( Properties.Phrases.overMinusAndAHalf, number )
                    : string.Format( Properties.Phrases.overAndAHalf, number ),
                1E3M => isNegative
                    ? string.Format( Properties.Phrases.overMinusAndAHalfThousand, number )
                    : string.Format( Properties.Phrases.overAndAHalfThousand, number ),
                1E6M => isNegative
                    ? string.Format( Properties.Phrases.overMinusAndAHalfMillion, number )
                    : string.Format( Properties.Phrases.overAndAHalfMillion, number ),
                1E9M => isNegative
                    ? string.Format( Properties.Phrases.overMinusAndAHalfBillion, number )
                    : string.Format( Properties.Phrases.overAndAHalfBillion, number ),
                1E12M => isNegative
                    ? string.Format( Properties.Phrases.overMinusAndAHalfTrillion, number )
                    : string.Format( Properties.Phrases.overAndAHalfTrillion, number ),
                1E15M => isNegative
                    ? string.Format( Properties.Phrases.overMinusAndAHalfQuadrillion, number )
                    : string.Format( Properties.Phrases.overAndAHalfQuadrillion, number ),
                1E18M => isNegative
                    ? string.Format( Properties.Phrases.overMinusAndAHalfQuintillion, number )
                    : string.Format( Properties.Phrases.overAndAHalfQuintillion, number ),
                _ => $"{value}"
            };
        }

        private static string FormatAsWellOverOneAndAHalf(int number, bool isNegative, long orderMultiplier, decimal value)
        {
            return (decimal)orderMultiplier switch
            {
                1 => isNegative
                    ? string.Format( Properties.Phrases.wellOverMinusAndAHalf, number )
                    : string.Format( Properties.Phrases.wellOverAndAHalf, number ),
                1E3M => isNegative
                    ? string.Format( Properties.Phrases.wellOverMinusAndAHalfThousand, number )
                    : string.Format( Properties.Phrases.wellOverAndAHalfThousand, number ),
                1E6M => isNegative
                    ? string.Format( Properties.Phrases.wellOverMinusAndAHalfMillion, number )
                    : string.Format( Properties.Phrases.wellOverAndAHalfMillion, number ),
                1E9M => isNegative
                    ? string.Format( Properties.Phrases.wellOverMinusAndAHalfBillion, number )
                    : string.Format( Properties.Phrases.wellOverAndAHalfBillion, number ),
                1E12M => isNegative
                    ? string.Format( Properties.Phrases.wellOverMinusAndAHalfTrillion, number )
                    : string.Format( Properties.Phrases.wellOverAndAHalfTrillion, number ),
                1E15M => isNegative
                    ? string.Format( Properties.Phrases.wellOverMinusAndAHalfQuadrillion, number )
                    : string.Format( Properties.Phrases.wellOverAndAHalfQuadrillion, number ),
                1E18M => isNegative
                    ? string.Format( Properties.Phrases.wellOverMinusAndAHalfQuintillion, number )
                    : string.Format( Properties.Phrases.wellOverAndAHalfQuintillion, number ),
                _ => $"{value}"
            };
        }

        private static string FormatAsNearly(int number, bool isNegative, long orderMultiplier, decimal value)
        {
            return (decimal)orderMultiplier switch
            {
                1 => isNegative
                    ? string.Format( Properties.Phrases.nearlyMinus, number )
                    : string.Format( Properties.Phrases.nearly, number ),
                1E3M => isNegative
                    ? string.Format( Properties.Phrases.nearlyMinusThousand, number )
                    : string.Format( Properties.Phrases.nearlyThousand, number ),
                1E6M => isNegative
                    ? string.Format( Properties.Phrases.nearlyMinusMillion, number )
                    : string.Format( Properties.Phrases.nearlyMillion, number ),
                1E9M => isNegative
                    ? string.Format( Properties.Phrases.nearlyMinusBillion, number )
                    : string.Format( Properties.Phrases.nearlyBillion, number ),
                1E12M => isNegative
                    ? string.Format( Properties.Phrases.nearlyMinusTrillion, number )
                    : string.Format( Properties.Phrases.nearlyTrillion, number ),
                1E15M => isNegative
                    ? string.Format( Properties.Phrases.nearlyMinusQuadrillion, number )
                    : string.Format( Properties.Phrases.nearlyQuadrillion, number ),
                1E18M => isNegative
                    ? string.Format( Properties.Phrases.nearlyMinusQuintillion, number )
                    : string.Format( Properties.Phrases.nearlyQuintillion, number ),
                _ => $"{value}"
            };
        }
    }
}