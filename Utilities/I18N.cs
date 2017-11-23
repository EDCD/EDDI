using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Utilities
{
    // Simple internationalization helper class
    public class I18N
    {
        // default langCode, used if getString() fail with currentLang
        private const string defaultLang = "en";
        // current langCode
        private static string currentLang;
        // contains a I18NString for each internationalized string in langs.json
        // key is the langCode, value is internationalized string
        // to access a string use translations[stringCode][langCode]
        private static Dictionary<string, I18NString> translations;
        // contains each langs defined in langs.json
        private static List<String> availableLangs;
        // Can't use Utilities.Constants there because of cyclic dependecies
        private static string dataDir = Environment.GetEnvironmentVariable("AppData") + @"\EDDI\";
        // JSON filename, copy langs.json to EDDI DATA_DIR in order to use unit tests
        public static readonly string langsFile;

        static I18N()
        {
            if (File.Exists("langs.json"))
                langsFile = "langs.json";
            else if (File.Exists(dataDir + "langs.json"))
                langsFile = dataDir + "langs.json";
            else
                throw new Exception("Utilities.I18N : Unable to reach 'langs.json' file");
            Init();
        }

        // Set all values to their defaults
        private static void Init()
        {
            //Logging.Info($"I18N reading '{langsFile}'");
            translations = new Dictionary<string, I18NString>();
            availableLangs = new List<string>();
            currentLang = GetSystemLangCode();
            UpdateResources();
        }

        // Reset all values to default by calling Init()
        public static void Reset()
        {
            Init();
        }

        public static List<string> GetKeys()
        {
            return translations.Keys.ToList<string>();
        }

        // Read the .json file and store all resources
        public static void UpdateResources()
        {
            JObject json = JObject.Parse(File.ReadAllText(langsFile));
            availableLangs = json["langs"].ToObject<List<string>>();
            foreach(JProperty translation in json["translations"])
            {
                I18NString i18ns = new I18NString();
                //Console.WriteLine(translation.Name+":");
                foreach(JProperty pair in json["translations"][translation.Name])
                {
                    //Console.WriteLine("\t"+pair);
                    //Console.WriteLine($"\t'{pair.Name}': '{pair.Value.ToString()}'");
                    string value = pair.Value.Type == JTokenType.Null ? null : pair.Value.ToObject<string>();
                    if(pair.Name == "test_null") Console.WriteLine(pair.Value);
                    i18ns.Add(pair.Name, value);
                }
                translations[translation.Name] = i18ns;
            }
        }

        // returns the OS langCode
        // for example returns "en" if CultureInfo.CurrentUICulture.Name equals "en-US" or "en-UK" etc...
        public static string GetSystemLangCode()
        {
            return CultureInfo.CurrentUICulture.Name.Split('-')[0];
        }

        // Returns the current langCode
        public static string GetLang()
        {
            return currentLang;
        }

        // Set lang with given langCode if available
        public static bool SetLang(string lang)
        {
            if (IsAvailableLang(lang))
            {
                currentLang = lang;
                return true;
            }
            return false;
        }

        // Set the lang to default
        public static void FallbackLang()
        {
            currentLang = defaultLang;
        }

        // Returns true if lang is defined in langs.json
        public static bool IsAvailableLang(string lang)
        {
            return lang == null ? false : availableLangs.Contains(lang);
        }

        // Set lang with given langCode if available, call FallbackLang() otherwise
        public static bool SetLangOrFallback(string lang)
        {
            if (SetLang(lang))
            {
                return true;
            }
            else
            {
                FallbackLang();
                return false;
            }
        }

        // returns files list as it contains every usable langCode
        public static List<string> GetAvailableLangs()
        {
            return availableLangs;
        }

        // Returns the string value corresponding to the given key
        // Uses the current lang
        public static string GetString(string stringCode)
        {
            return GetString(stringCode, currentLang);
        }

        // Returns the string value corresponding to the given key and lang
        // Tries to return the string with defaultLang if it's not defined in given lang
        // Return null if it fails
        // or if KeyNotFoundException was thrown
        public static string GetString(string stringCode, string lang)
        {
            if (stringCode == null)
                return null;
            string s = null;
            // Console.WriteLine($"searching for string '{stringCode}' in lang '{lang}' in file '{langsFile}'");
            try
            {
                if (translations.ContainsKey(stringCode)) // if stringCode does not exists then return ""
                {
                    if (IsAvailableLang(lang))
                    {
                        // if lang is undefined for this langCode then throw KeyNotFoundException
                        // if result is null then returns the same string with defaultLang
                        s = translations[stringCode][lang];
                        // if(stringCode == "pres_par0") Console.WriteLine("===================\n"+s+"\n===================");
                        if (s == null) // 
                        {
                            Console.WriteLine($"string {stringCode} is not defined in lang '{lang}', returning with default lang '{defaultLang}'");
                            s = translations[stringCode][defaultLang]; // if also null returns ""
                        }
                    }
                    else // lang is not available, meaning it is not in 'langs' property in langs.json
                    {
                        Console.WriteLine($"lang '{lang}' is not available in {langsFile}, returning with default lang '{defaultLang}'");
                        // if null then return "", if defaultLang is undefined for this langCode then throw KeyNotFoundException
                        s = translations[stringCode][defaultLang];
                    }
                }
                else
                {
                    Console.WriteLine($"string '{stringCode}' does not exists in {langsFile}");
                }
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine($"Unable to find string '{stringCode}' in lang '{lang}' in '{langsFile}'");
                s = null; // Just to be sure of what is returned in this case
            }
            return s;
        }

        // Returns the string value corresponding to the given key
        // Uses the current lang
        // Replace each occurences, if they exists, of "{x}" patterns in the string value, where x is an integer,
        // by its corresponding value in args param (args[x])
        public static string GetStringWithArgs(string stringCode, string[] args)
        {
            return GetStringWithArgs(stringCode, currentLang, args);
        }

        // Returns the string value corresponding to the given key and lang
        // Replace each occurences, if they exists, of "{x}" patterns in the string value, where x is an integer,
        // by its corresponding value in args param (args[x])
        public static string GetStringWithArgs(string stringCode, string langCode, string[] args)
        {
            string s = GetString(stringCode, langCode);
            for(int i=0; i<args.Length; i++)
            {
                s = s.Replace("{"+i+"}", args[i]);
            }
            return s;
        }
    }
}
