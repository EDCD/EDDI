using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using Newtonsoft.Json;
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
        // JSON filename
        private static readonly string langsFile = "langs.json";

        static I18N()
        {
            Init();
        }

        // Set all values to their defaults
        private static void Init()
        {
            translations = new Dictionary<string, I18NString>();
            availableLangs = new List<string>();
            currentLang = defaultLang;
            UpdateResources();
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
                    i18ns.Add(pair.Name, pair.Value.ToString());
                }
                translations[translation.Name] = i18ns;
            }
        }

        // Reset all values to default by calling Init()
        public static void Reset()
        {
            Init();
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
            return availableLangs.Contains(lang);
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
        public static string GetString(string stringCode, string lang)
        {
            string s = "";
            try
            {
                s = IsAvailableLang(lang) ? translations[stringCode][lang] : translations[stringCode][defaultLang];
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine($"Unable to find string '{stringCode}' in lang '{lang}'");
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
