using System;

namespace EddiSpeechService
{
    public partial class Translations
    {
        public static string Humanize(decimal? rawValue)
        {
            if (rawValue == null) {return null;}
            decimal value = (decimal) rawValue;
            if (value == 0) {return Properties.Phrases.zero;}

            bool isNegative = value < 0;
            if (isNegative)
            {
                value = -value;
            }

            if (value < 10)
            {
                // Work out how many 0s to begin with
                int numzeros = -1;
                while (value < 1)
                {
                    value *= 10;
                    numzeros++;
                }

                // Now round it to 2sf
                return (isNegative ? Properties.Phrases.minus + " " : "") +
                       (Math.Round(value * 10) / (decimal) Math.Pow(10, numzeros + 2));
            }

            double magnitude = Math.Log10((double) value);
            long orderMultiplier = (long) Math.Pow(10, Math.Floor(magnitude / 3) * 3);
            var (number, nextDigit) = Normalize(value, orderMultiplier);

            // See if we have a whole number that is fully described within the largest order
            if (number * orderMultiplier == Math.Abs(value))
            {
                // Some languages render these differently than others. "1000" in English is "one thousand" but in Italian is simply "mille".
                // Consequently, we leave the interpretation to the culture-specific voice.
                return FormatVerbatim(number, isNegative, orderMultiplier);
            }

            if (number < 100)
            {
                return FormatWith2SignificantDigits(number, isNegative, orderMultiplier, nextDigit, value);
            }
            else // Describe (less precisely) values for numbers where the largest order number exceeds one hundred
            {
                return FormatWith3SignificantDigits(number, isNegative, orderMultiplier, nextDigit, value);
            }
        }

        private static (int number, int nextDigit) Normalize(decimal inputValue, decimal orderMultiplierVal)
        {
            return (number: (int) (inputValue / orderMultiplierVal), nextDigit: (int) ((inputValue % orderMultiplierVal) / (orderMultiplierVal / 10)));
        }

        private static string FormatWith2SignificantDigits(int number, bool isNegative, long orderMultiplier, int nextDigit, decimal value)
        {
            // See if we have a number whose value can be expressed with a short decimal (i.e 1.3 million)
            var shortDecimal = (number + ((decimal) nextDigit / 10));
            if (shortDecimal == Math.Round(value / orderMultiplier, 2))
            {
                if (nextDigit == 0)
                {
                    return FormatVerbatim(number, isNegative, orderMultiplier);
                }

                return FormatAsShortDecimal(shortDecimal, isNegative, orderMultiplier);
            }

            // Describe values for numbers where the largest order number does not exceed one hundred
            switch (nextDigit)
            {
            case 1:
                return FormatAsJustOver(number, isNegative, orderMultiplier, value);
            case 2:
                return FormatAsOver(number, isNegative, orderMultiplier, value);
            case 3:
                return FormatAsWellOver(number, isNegative, orderMultiplier, value);
            case 4:
                return FormatAsNearlyOneAndAHalf(number, isNegative, orderMultiplier, value);
            case 5:
                return FormatAsAroundOneAndAHalf(number, isNegative, orderMultiplier, value);
            case 6:
            case 7:
                return FormatAsOverOneAndAHalf(number, isNegative, orderMultiplier, value);
            case 8:
                return FormatAsWellOverOneAndAHalf(number, isNegative, orderMultiplier, value);
            case 9:
                return FormatAsNearly(number + 1, isNegative, orderMultiplier, value);
            default:
                // `nextDigit` is zero. the figure we are saying is round enough already
                return FormatVerbatim(number, isNegative, orderMultiplier);
            }
        }

        private static string FormatWith3SignificantDigits(int number, bool isNegative, long orderMultiplier, int nextDigit,
            decimal value)
        {
            // Round largest order numbers in the hundreds to the nearest 10, except where the number after the hundreds place is 20 or less
            if (number - (int)((decimal)number / 100) * 100 >= 20)
            {
                (number, nextDigit) = Normalize(number, 10);
                number *= 10;
            }

            switch (nextDigit)
            {
            case 1:
                return FormatAsJustOver(number, isNegative, orderMultiplier, value);
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
                return FormatAsOver(number, isNegative, orderMultiplier, value);
            case 7:
            case 8:
            case 9:
                return FormatAsNearly(number + 1, isNegative, orderMultiplier, value);
            default:
                // `nextDigit` is zero. the figure we are saying is round enough already
                return FormatVerbatim(number, isNegative, orderMultiplier);
            }
        }

        private static string FormatVerbatim(int number, bool isNegative, long orderMultiplier)
        {
            return (isNegative ? Properties.Phrases.minus + " " : "") + (number * orderMultiplier);
        }

        private static string FormatAsShortDecimal(decimal shortDecimal, bool isNegative, long orderMultiplier)
        {
            switch ((decimal) orderMultiplier)
            {
            case 1:
                return (isNegative ? Properties.Phrases.minus + " " : "") + shortDecimal;
            case 1E3M:
                return (isNegative ? Properties.Phrases.minus + " " : "") +
                       string.Format(Properties.Phrases.shortDecimalThousand, shortDecimal);
            case 1E6M:
                return (isNegative ? Properties.Phrases.minus + " " : "") +
                       string.Format(Properties.Phrases.shortDecimalMillion, shortDecimal);
            case 1E9M:
                return (isNegative ? Properties.Phrases.minus + " " : "") +
                       string.Format(Properties.Phrases.shortDecimalBillion, shortDecimal);
            case 1E12M:
                return (isNegative ? Properties.Phrases.minus + " " : "") +
                       string.Format(Properties.Phrases.shortDecimalTrillion, shortDecimal);
            case 1E15M:
                return (isNegative ? Properties.Phrases.minus + " " : "") +
                       string.Format(Properties.Phrases.shortDecimalQuadrillion, shortDecimal);
            case 1E18M:
                return (isNegative ? Properties.Phrases.minus + " " : "") +
                       string.Format(Properties.Phrases.shortDecimalQuintillion, shortDecimal);
            default:
                return (isNegative ? Properties.Phrases.minus + " " : "") +
                       (shortDecimal * orderMultiplier);
            }
        }

        private static string FormatAsJustOver(int number, bool isNegative, long orderMultiplier, decimal value)
        {
            switch ((decimal) orderMultiplier)
            {
            case 1:
                return isNegative
                    ? string.Format(Properties.Phrases.justOverMinus, number)
                    : string.Format(Properties.Phrases.justOver, number);
            case 1E3M:
                return isNegative
                    ? string.Format(Properties.Phrases.justOverMinusThousand, number)
                    : string.Format(Properties.Phrases.justOverThousand, number);
            case 1E6M:
                return isNegative
                    ? string.Format(Properties.Phrases.justOverMinusMillion, number)
                    : string.Format(Properties.Phrases.justOverMillion, number);
            case 1E9M:
                return isNegative
                    ? string.Format(Properties.Phrases.justOverMinusBillion, number)
                    : string.Format(Properties.Phrases.justOverBillion, number);
            case 1E12M:
                return isNegative
                    ? string.Format(Properties.Phrases.justOverMinusTrillion, number)
                    : string.Format(Properties.Phrases.justOverTrillion, number);
            case 1E15M:
                return isNegative
                    ? string.Format(Properties.Phrases.justOverMinusQuadrillion, number)
                    : string.Format(Properties.Phrases.justOverQuadrillion, number);
            case 1E18M:
                return isNegative
                    ? string.Format(Properties.Phrases.justOverMinusQuintillion, number)
                    : string.Format(Properties.Phrases.justOverQuintillion, number);
            default:
                return $"{value}";
            }
        }

        private static string FormatAsOver(int number, bool isNegative, long orderMultiplier, decimal value)
        {
            switch ((decimal) orderMultiplier)
            {
            case 1:
                return isNegative
                    ? string.Format(Properties.Phrases.overMinus, number)
                    : string.Format(Properties.Phrases.over, number);
            case 1E3M:
                return isNegative
                    ? string.Format(Properties.Phrases.overMinusThousand, number)
                    : string.Format(Properties.Phrases.overThousand, number);
            case 1E6M:
                return isNegative
                    ? string.Format(Properties.Phrases.overMinusMillion, number)
                    : string.Format(Properties.Phrases.overMillion, number);
            case 1E9M:
                return isNegative
                    ? string.Format(Properties.Phrases.overMinusBillion, number)
                    : string.Format(Properties.Phrases.overBillion, number);
            case 1E12M:
                return isNegative
                    ? string.Format(Properties.Phrases.overMinusTrillion, number)
                    : string.Format(Properties.Phrases.overTrillion, number);
            case 1E15M:
                return isNegative
                    ? string.Format(Properties.Phrases.overMinusQuadrillion, number)
                    : string.Format(Properties.Phrases.overQuadrillion, number);
            case 1E18M:
                return isNegative
                    ? string.Format(Properties.Phrases.overMinusQuintillion, number)
                    : string.Format(Properties.Phrases.overQuintillion, number);
            default:
                return $"{value}";
            }
        }

        private static string FormatAsWellOver(int number, bool isNegative, long orderMultiplier, decimal value)
        {
            switch ((decimal)orderMultiplier)
            {
            case 1:
                return isNegative
                    ? string.Format(Properties.Phrases.wellOverMinus, number)
                    : string.Format(Properties.Phrases.wellOver, number);
            case 1E3M:
                return isNegative
                    ? string.Format(Properties.Phrases.wellOverMinusThousand, number)
                    : string.Format(Properties.Phrases.wellOverThousand, number);
            case 1E6M:
                return isNegative
                    ? string.Format(Properties.Phrases.wellOverMinusMillion, number)
                    : string.Format(Properties.Phrases.wellOverMillion, number);
            case 1E9M:
                return isNegative
                    ? string.Format(Properties.Phrases.wellOverMinusBillion, number)
                    : string.Format(Properties.Phrases.wellOverBillion, number);
            case 1E12M:
                return isNegative
                    ? string.Format(Properties.Phrases.wellOverMinusTrillion, number)
                    : string.Format(Properties.Phrases.wellOverTrillion, number);
            case 1E15M:
                return isNegative
                    ? string.Format(Properties.Phrases.wellOverMinusQuadrillion, number)
                    : string.Format(Properties.Phrases.wellOverQuadrillion, number);
            case 1E18M:
                return isNegative
                    ? string.Format(Properties.Phrases.wellOverMinusQuintillion, number)
                    : string.Format(Properties.Phrases.wellOverQuintillion, number);
            default:
                return $"{value}";
            }
        }

        private static string FormatAsNearlyOneAndAHalf(int number, bool isNegative, long orderMultiplier, decimal value)
        {
            switch ((decimal)orderMultiplier)
            {
            case 1:
                return isNegative
                    ? string.Format(Properties.Phrases.nearlyMinusAndAHalf, number)
                    : string.Format(Properties.Phrases.nearlyAndAHalf, number);
            case 1E3M:
                return isNegative
                    ? string.Format(Properties.Phrases.nearlyMinusThousandAndAHalf, number)
                    : string.Format(Properties.Phrases.nearlyThousandAndAHalf, number);
            case 1E6M:
                return isNegative
                    ? string.Format(Properties.Phrases.nearlyMinusMillionAndAHalf, number)
                    : string.Format(Properties.Phrases.nearlyMillionAndAHalf, number);
            case 1E9M:
                return isNegative
                    ? string.Format(Properties.Phrases.nearlyMinusBillionAndAHalf, number)
                    : string.Format(Properties.Phrases.nearlyBillionAndAHalf, number);
            case 1E12M:
                return isNegative
                    ? string.Format(Properties.Phrases.nearlyMinusTrillionAndAHalf, number)
                    : string.Format(Properties.Phrases.nearlyTrillionAndAHalf, number);
            case 1E15M:
                return isNegative
                    ? string.Format(Properties.Phrases.nearlyMinusQuadrillionAndAHalf, number)
                    : string.Format(Properties.Phrases.nearlyQuadrillionAndAHalf, number);
            case 1E18M:
                return isNegative
                    ? string.Format(Properties.Phrases.nearlyMinusQuintillionAndAHalf, number)
                    : string.Format(Properties.Phrases.nearlyQuintillionAndAHalf, number);
            default:
                return $"{value}";
            }
        }

        private static string FormatAsAroundOneAndAHalf(int number, bool isNegative, long orderMultiplier, decimal value)
        {
            switch ((decimal)orderMultiplier)
            {
                case 1:
                    return isNegative
                        ? string.Format(Properties.Phrases.aroundMinusAndAHalf, number)
                        : string.Format(Properties.Phrases.aroundAndAHalf, number);
                case 1E3M:
                    return isNegative
                        ? string.Format(Properties.Phrases.aroundMinusAndAHalfThousand, number)
                        : string.Format(Properties.Phrases.aroundAndAHalfThousand, number);
                case 1E6M:
                    return isNegative
                        ? string.Format(Properties.Phrases.aroundMinusAndAHalfMillion, number)
                        : string.Format(Properties.Phrases.aroundAndAHalfMillion, number);
                case 1E9M:
                    return isNegative
                        ? string.Format(Properties.Phrases.aroundMinusAndAHalfBillion, number)
                        : string.Format(Properties.Phrases.aroundAndAHalfBillion, number);
                case 1E12M:
                    return isNegative
                        ? string.Format(Properties.Phrases.aroundMinusAndAHalfTrillion, number)
                        : string.Format(Properties.Phrases.aroundAndAHalfTrillion, number);
                case 1E15M:
                    return isNegative
                        ? string.Format(Properties.Phrases.aroundMinusAndAHalfQuadrillion, number)
                        : string.Format(Properties.Phrases.aroundAndAHalfQuadrillion, number);
                case 1E18M:
                    return isNegative
                        ? string.Format(Properties.Phrases.aroundMinusAndAHalfQuintillion, number)
                        : string.Format(Properties.Phrases.aroundAndAHalfQuintillion, number);
                default:
                    return $"{value}";
            }
        }

        private static string FormatAsOverOneAndAHalf(int number, bool isNegative, long orderMultiplier, decimal value)
        {
            switch ((decimal)orderMultiplier)
            {
                case 1:
                    return isNegative
                        ? string.Format(Properties.Phrases.overMinusAndAHalf, number)
                        : string.Format(Properties.Phrases.overAndAHalf, number);
                case 1E3M:
                    return isNegative
                        ? string.Format(Properties.Phrases.overMinusAndAHalfThousand, number)
                        : string.Format(Properties.Phrases.overAndAHalfThousand, number);
                case 1E6M:
                    return isNegative
                        ? string.Format(Properties.Phrases.overMinusAndAHalfMillion, number)
                        : string.Format(Properties.Phrases.overAndAHalfMillion, number);
                case 1E9M:
                    return isNegative
                        ? string.Format(Properties.Phrases.overMinusAndAHalfBillion, number)
                        : string.Format(Properties.Phrases.overAndAHalfBillion, number);
                case 1E12M:
                    return isNegative
                        ? string.Format(Properties.Phrases.overMinusAndAHalfTrillion, number)
                        : string.Format(Properties.Phrases.overAndAHalfTrillion, number);
                case 1E15M:
                    return isNegative
                        ? string.Format(Properties.Phrases.overMinusAndAHalfQuadrillion, number)
                        : string.Format(Properties.Phrases.overAndAHalfQuadrillion, number);
                case 1E18M:
                    return isNegative
                        ? string.Format(Properties.Phrases.overMinusAndAHalfQuintillion, number)
                        : string.Format(Properties.Phrases.overAndAHalfQuintillion, number);
                default:
                    return $"{value}";
            }
        }

        private static string FormatAsWellOverOneAndAHalf(int number, bool isNegative, long orderMultiplier, decimal value)
        {
            switch ((decimal)orderMultiplier)
            {
            case 1:
                return isNegative
                    ? string.Format(Properties.Phrases.wellOverMinusAndAHalf, number)
                    : string.Format(Properties.Phrases.wellOverAndAHalf, number);
            case 1E3M:
                return isNegative
                    ? string.Format(Properties.Phrases.wellOverMinusAndAHalfThousand, number)
                    : string.Format(Properties.Phrases.wellOverAndAHalfThousand, number);
            case 1E6M:
                return isNegative
                    ? string.Format(Properties.Phrases.wellOverMinusAndAHalfMillion, number)
                    : string.Format(Properties.Phrases.wellOverAndAHalfMillion, number);
            case 1E9M:
                return isNegative
                    ? string.Format(Properties.Phrases.wellOverMinusAndAHalfBillion, number)
                    : string.Format(Properties.Phrases.wellOverAndAHalfBillion, number);
            case 1E12M:
                return isNegative
                    ? string.Format(Properties.Phrases.wellOverMinusAndAHalfTrillion, number)
                    : string.Format(Properties.Phrases.wellOverAndAHalfTrillion, number);
            case 1E15M:
                return isNegative
                    ? string.Format(Properties.Phrases.wellOverMinusAndAHalfQuadrillion, number)
                    : string.Format(Properties.Phrases.wellOverAndAHalfQuadrillion, number);
            case 1E18M:
                return isNegative
                    ? string.Format(Properties.Phrases.wellOverMinusAndAHalfQuintillion, number)
                    : string.Format(Properties.Phrases.wellOverAndAHalfQuintillion, number);
            default:
                return $"{value}";
            }
        }

        private static string FormatAsNearly(int number, bool isNegative, long orderMultiplier, decimal value)
        {
            switch ((decimal) orderMultiplier)
            {
            case 1:
                return isNegative
                    ? string.Format(Properties.Phrases.nearlyMinus, number)
                    : string.Format(Properties.Phrases.nearly, number);
            case 1E3M:
                return isNegative
                    ? string.Format(Properties.Phrases.nearlyMinusThousand, number)
                    : string.Format(Properties.Phrases.nearlyThousand, number);
            case 1E6M:
                return isNegative
                    ? string.Format(Properties.Phrases.nearlyMinusMillion, number)
                    : string.Format(Properties.Phrases.nearlyMillion, number);
            case 1E9M:
                return isNegative
                    ? string.Format(Properties.Phrases.nearlyMinusBillion, number)
                    : string.Format(Properties.Phrases.nearlyBillion, number);
            case 1E12M:
                return isNegative
                    ? string.Format(Properties.Phrases.nearlyMinusTrillion, number)
                    : string.Format(Properties.Phrases.nearlyTrillion, number);
            case 1E15M:
                return isNegative
                    ? string.Format(Properties.Phrases.nearlyMinusQuadrillion, number)
                    : string.Format(Properties.Phrases.nearlyQuadrillion, number);
            case 1E18M:
                return isNegative
                    ? string.Format(Properties.Phrases.nearlyMinusQuintillion, number)
                    : string.Format(Properties.Phrases.nearlyQuintillion, number);
            default:
                return $"{value}";
            }
        }
    }
}