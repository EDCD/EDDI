using System;
using System.Globalization;
using Utilities;

namespace EddiSpeechService.SpeechConversions
{
    public static partial class SpeechConversions
    {
        public sealed class HumanizedNumber
        {
            [PublicAPI( "The original number passed to NumberDetails()." )]
            public decimal rawvalue { get; init; }

            [PublicAPI( "The absolute value of the original number." )]
            public decimal absolutevalue { get; init; }

            [PublicAPI( "True if the original number was negative." )]
            public bool isnegative { get; init; }

            [PublicAPI( "The multiplier for the selected order of magnitude." )]
            public long ordermultiplier { get; init; }

            [PublicAPI( "The significant whole number extracted for the selected order of magnitude." )]
            public int number { get; init; }

            [PublicAPI( "The next significant digit after number." )]
            public int nextdigit { get; init; }

            [PublicAPI( "The decomposition category used by the default Humanise script." )]
            public string format { get; init; }

            [PublicAPI( "The prepared quantity text used by the default Humanise script." )]
            public string quantity { get; init; }

            [PublicAPI( "The localized magnitude name for the selected order." )]
            public string magnitudename { get; init; }

            [PublicAPI( "The invariant English magnitude name for the selected order, for example thousand or million." )]
            public string invariantmagnitudename { get; init; }
        }

        public static HumanizedNumber DecomposeHumanizedNumber ( decimal? rawValue, bool? forceIntegerMantissa = null )
        {
            if (rawValue == null) {return null;}
            var value = (decimal)rawValue;
            if ( value == 0 )
            {
                var zeroResult = Properties.Phrases.zero;
                Logging.Debug( $"Converted raw value '{rawValue}' to '{zeroResult}'" );
                return new HumanizedNumber
                {
                    rawvalue = value,
                    absolutevalue = value,
                    ordermultiplier = 1,
                    format = "zero",
                    quantity = zeroResult
                };
            }

            var wantIntegerMantissa = forceIntegerMantissa ?? Properties.FormatOverrides.forceIntegerMantissa.Equals("true");

            var isNegative = value < 0;
            if (isNegative)
            {
                value = -value;
            }
            var absoluteValue = value;

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
                var smallResult = ( isNegative ? Properties.Phrases.minus + " " : "" ) +
                             ( Math.Round( value * 10 ) / (decimal)Math.Pow( 10, numzeros + 2 ) );
                Logging.Debug($"Converted raw value '{rawValue}' to '{smallResult}'");
                return new HumanizedNumber
                {
                    rawvalue = (decimal)rawValue,
                    absolutevalue = absoluteValue,
                    isnegative = isNegative,
                    ordermultiplier = 1,
                    number = (int)Math.Floor( value ),
                    format = "small",
                    quantity = smallResult
                };
            }

            var magnitude = Math.Log10((double) value);
            var orderMultiplier = (long) Math.Pow(10, Math.Floor(magnitude / 3) * 3);
            var (number, nextDigit) = Normalize(value, orderMultiplier);
            var shortDecimal = number + ( (decimal)nextDigit / 10 );
            string format;
            var quantity = number.ToString( CultureInfo.InvariantCulture );

            // See if we have a whole number that is fully described within the largest order
            if (number * orderMultiplier == Math.Abs(value))
            {
                // Some languages render these differently than others. "1000" in English is "one thousand" but in Italian is simply "mille".
                // Consequently, we leave the interpretation to the culture-specific voice.
                format = "verbatim";
                quantity = FormatVerbatim(number, isNegative, orderMultiplier);
            }
            else if (number < 100)
            {
                format = Get2SignificantDigitFormat( number, orderMultiplier, nextDigit, value, wantIntegerMantissa );
                if ( format == "short_decimal" )
                {
                    quantity = shortDecimal.ToString( CultureInfo.InvariantCulture );
                }
                else if ( format == "integer_mantissa" )
                {
                    quantity = ( ( number * 1000 ) + ( nextDigit * 100 ) ).ToString( CultureInfo.InvariantCulture );
                    orderMultiplier /= 1000;
                }
                else if ( format == "nearly" && nextDigit == 9 )
                {
                    quantity = ( number + 1 ).ToString( CultureInfo.InvariantCulture );
                }
            }
            else // Describe (less precisely) values for numbers where the largest order number exceeds one hundred
            {
                format = Get3SignificantDigitFormat( ref number, ref nextDigit );
                quantity = format == "nearly"
                    ? ( number + 1 ).ToString( CultureInfo.InvariantCulture )
                    : number.ToString( CultureInfo.InvariantCulture );
            }

            return new HumanizedNumber
            {
                rawvalue = (decimal)rawValue,
                absolutevalue = value,
                isnegative = isNegative,
                ordermultiplier = orderMultiplier,
                number = number,
                nextdigit = nextDigit,
                format = format,
                quantity = quantity,
                magnitudename = GetLocalizedMagnitudeName( orderMultiplier ),
                invariantmagnitudename = GetInvariantMagnitudeName( orderMultiplier )
            };
        }

        private static (int number, int nextDigit) Normalize(decimal inputValue, decimal orderMultiplierVal)
        {
            return (number: (int) (inputValue / orderMultiplierVal), nextDigit: (int) (inputValue % orderMultiplierVal / (orderMultiplierVal / 10)));
        }

        private static string Get2SignificantDigitFormat ( int number, long orderMultiplier, int nextDigit, decimal value, bool wantIntegerMantissa )
        {
            var shortDecimal = number + ( (decimal)nextDigit / 10 );
            if ( shortDecimal == Math.Round( value / orderMultiplier, 2 ) )
            {
                return wantIntegerMantissa && orderMultiplier >= 1000
                    ? "integer_mantissa"
                    : nextDigit == 0
                        ? "just_over"
                        : "short_decimal";
            }

            return nextDigit switch
            {
                1 => "just_over",
                2 => "over",
                3 => "well_over",
                4 => "nearly_half",
                5 => "around_half",
                6 or 7 => "over_half",
                8 => "well_over_half",
                9 => "nearly",
                _ => "just_over"
            };
        }

        private static string Get3SignificantDigitFormat ( ref int number, ref int nextDigit )
        {
            if ((number - ((int)((decimal)number / 100) * 100)) >= 20)
            {
                (number, nextDigit) = Normalize(number, 10);
                number *= 10;
            }

            return nextDigit switch
            {
                1 => "just_over",
                2 or 3 or 4 or 5 or 6 => "over",
                7 or 8 or 9 => "nearly",
                _ => "just_over"
            };
        }

        private static string GetLocalizedMagnitudeName ( long orderMultiplier )
        {
            var magnitudeFormat = (decimal)orderMultiplier switch
            {
                1E3M => Properties.Phrases.shortDecimalThousand,
                1E6M => Properties.Phrases.shortDecimalMillion,
                1E9M => Properties.Phrases.shortDecimalBillion,
                1E12M => Properties.Phrases.shortDecimalTrillion,
                1E15M => Properties.Phrases.shortDecimalQuadrillion,
                1E18M => Properties.Phrases.shortDecimalQuintillion,
                _ => string.Empty
            };
            return magnitudeFormat.Replace( "{0}", string.Empty ).Trim();
        }

        private static string GetInvariantMagnitudeName ( long orderMultiplier )
        {
            return orderMultiplier switch
            {
                1 => "",
                1_000 => "thousand",
                1_000_000 => "million",
                1_000_000_000 => "billion",
                1_000_000_000_000 => "trillion",
                1_000_000_000_000_000 => "quadrillion",
                1_000_000_000_000_000_000 => "quintillion",
                _ => ""
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

    }
}
