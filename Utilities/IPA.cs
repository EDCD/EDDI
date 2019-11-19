using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace Utilities
{
    public static class IPA
    {
        // IPA symbols sourced from https://www.internationalphoneticassociation.org/content/ipa-chart (IPA 2005 edition)
        // and from https://www.internationalphoneticassociation.org/sites/default/files/phonsymbol.pdf

        // By Unicode hex code and symbol
        private static readonly Dictionary<string, string> validIPA = new Dictionary<string, string>()
        {
            { "0061", "a" },
            { "0062", "b" },
            { "0063", "c" },
            { "0064", "d" },
            { "0065", "e" },
            { "0066", "f" },
            { "0067", "g" }, // This is an ordinary "g", which the IPA has ruled is acceptable in place of a vd velar plosive "ɡ" (code code 0261)
            { "0068", "h" },
            { "0069", "i" },
            { "006A", "j" },
            { "006B", "k" },
            { "006C", "l" },
            { "006D", "m" },
            { "006E", "n" },
            { "006F", "o" },
            { "0070", "p" },
            { "0071", "q" },
            { "0072", "r" },
            { "0073", "s" },
            { "0074", "t" },
            { "0075", "u" },
            { "0076", "v" },
            { "0077", "w" },
            { "0078", "x" },
            { "0079", "y" },
            { "007A", "z" },
            { "00E6", "æ" },
            { "00E7", "ç" },
            { "00F0", "ð" },
            { "00F8", "ø" },
            { "0127", "ħ" },
            { "014B", "ŋ" },
            { "0153", "œ" },
            { "01C0", "ǀ" },
            { "01C1", "ǁ" },
            { "01C2", "ǂ" },
            { "01C3", "ǃ" },
            { "0250", "ɐ" },
            { "0251", "ɑ" },
            { "0252", "ɒ" },
            { "0253", "ɓ" },
            { "0254", "ɔ" },
            { "0255", "ɕ" },
            { "0256", "ɖ" },
            { "0257", "ɗ" },
            { "0258", "ɘ" },
            { "0259", "ə" },
            { "025A", "ɚ" },
            { "025B", "ɛ" },
            { "025C", "ɜ" },
            { "025D", "ɝ" },
            { "025E", "ɞ" },
            { "025F", "ɟ" },
            { "0260", "ɠ" },
            { "0261", "ɡ" },
            { "0262", "ɢ" },
            { "0263", "ɣ" },
            { "0264", "ɤ" },
            { "0265", "ɥ" },
            { "0266", "ɦ" },
            { "0267", "ɧ" },
            { "0268", "ɨ" },
            { "026A", "ɪ" },
            { "026B", "ɫ" },
            { "026C", "ɬ" },
            { "026D", "ɭ" },
            { "026E", "ɮ" },
            { "026F", "ɯ" },
            { "0270", "ɰ" },
            { "0271", "ɱ" },
            { "0272", "ɲ" },
            { "0273", "ɳ" },
            { "0274", "ɴ" },
            { "0275", "ɵ" },
            { "0276", "ɶ" },
            { "0278", "ɸ" },
            { "0279", "ɹ" },
            { "027A", "ɺ" },
            { "027B", "ɻ" },
            { "027D", "ɽ" },
            { "027E", "ɾ" },
            { "0280", "ʀ" },
            { "0281", "ʁ" },
            { "0282", "ʂ" },
            { "0283", "ʃ" },
            { "0284", "ʄ" },
            { "0288", "ʈ" },
            { "0289", "ʉ" },
            { "028A", "ʊ" },
            { "028B", "ʋ" },
            { "028C", "ʌ" },
            { "028D", "ʍ" },
            { "028E", "ʎ" },
            { "028F", "ʏ" },
            { "0290", "ʐ" },
            { "0291", "ʑ" },
            { "0292", "ʒ" },
            { "0294", "ʔ" },
            { "0295", "ʕ" },
            { "0298", "ʘ" },
            { "0299", "ʙ" },
            { "029B", "ʛ" },
            { "029C", "ʜ" },
            { "029D", "ʝ" },
            { "029F", "ʟ" },
            { "02A1", "ʡ" },
            { "02A2", "ʢ" },
            { "02A4", "ʤ" },
            { "02A7", "ʧ" },
            { "02B0", "ʰ" },
            { "02B1", "ʱ" },
            { "02B2", "ʲ" },
            { "02B4", "ʴ" },
            { "02B7", "ʷ" },
            { "02BC", "ʼ" },
            { "02C8", "ˈ" },
            { "02CC", "ˌ" },
            { "02D0", "ː" },
            { "02D1", "ˑ" },
            { "02DE", "˞" },
            { "02E0", "ˠ" },
            { "02E4", "ˤ" },
            { "0300", "è" },
            { "0301", "é" },
            { "0303", "ẽ" },
            { "0304", "ē" },
            { "0306", "ĕ" },
            { "0308", "ë" },
            { "030A", "ŋ̊" },
            { "030B", "e̋" },
            { "030F", "ȅ" },
            { "0318", "e̘" },
            { "0319", "e̙" },
            { "031A", "t̚" },
            { "031C", "ɔ̜" },
            { "031D", "e̝ ɹ̝" },
            { "031E", "e̞ β̞" },
            { "031F", "u̟" },
            { "0320", "e̠" },
            { "0324", "b̤ a̤" },
            { "0325", "n̥ d̥" },
            { "0329", "m̩ n̩ l̩" },
            { "032A", "t̪ d̪" },
            { "032C", "s̬ t̬" },
            { "032F", "e̯" },
            { "0330", "b̰ a̰" },
            { "0334", "l̴ n̴" },
            { "0339", "ɔ̹" },
            { "033A", "t̺ d̺" },
            { "033B", "t̻ d̻" },
            { "033C", "t̼ d̼" },
            { "033D", "e̽" },
            { "035C", "x͜x" },
            { "0361", "x͡x" },
            { "03B2", "β" },
            { "03B8", "θ" },
            { "03C7", "χ" },
            { "2191", "↑" },
            { "2192", "→" },
            { "2193", "↓" },
            { "2197", "↗" },
            { "2198", "↘" },
            { "2C71", "ⱱ" }
        };

        public static bool IsValid(char value)
        {
            int unicodeDecimalCode = Convert.ToUInt16(value);
            string unicodeHexCode = unicodeDecimalCode.ToString("X4");
            return validIPA.ContainsKey(unicodeHexCode);
        }

        public static bool IsValid(string value)
        {
            foreach (var ch in value)
            {
                if (!IsValid(ch))
                {
                    return false;
                }
            }
            return true;
        }

        public static string[] InvalidChars(string value)
        {
            List<string> invalidChars = new List<string>();
            foreach (var ch in value)
            {
                if (!IsValid(ch))
                {
                    invalidChars.Add(string.IsNullOrEmpty(ch.ToString()) ? "(space)" : ch.ToString());
                }
            }
            return invalidChars.ToArray();
        }
    }

    public class ValidIPARule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value != null)
            {
                string[] invalidChars = IPA.InvalidChars(value.ToString());
                if (invalidChars.Any())
                {
                    string invalid = "";
                    foreach (string str in invalidChars)
                    {
                        invalid = invalid + (string.IsNullOrEmpty(invalid) ? "" : ", ") + (str == " " ? "(space)" : str);
                    }
                    string errMsg = @"Contains invalid characters: """ + invalid + @""". Please copy and paste characters directly from a valid source.";
                    Logging.Debug(errMsg + " Discarding last input.");
                    return new ValidationResult(false, errMsg);
                }

            }
            return ValidationResult.ValidResult;
        }
    }
}
