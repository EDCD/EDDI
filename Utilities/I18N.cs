using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Resources;

namespace Utilities
{
    // Simple internationalization helper class
    public class I18N
    {
        // default langCode if getString() fail with currentLang
        private const string defaultLang = "en";
        // current langCode
        private static string currentLang;
        // contains a ResourceManager for each resources files in Utilities.lang
        // key is the langCode, value is the ResourceManager
        private static Dictionary<string, ResourceManager> rmDictionnary;
        // contains each resource file's paths
        private static List<String> rmFiles;
        // lang directory basename to reach each resources in UpdateResourceManagers()
        private static readonly string langRoot = "Utilities.lang";

        static I18N()
        {
            Init();
        }

        // Set all values to their defaults
        private static void Init()
        {
            rmDictionnary = new Dictionary<string, ResourceManager>();
            rmFiles = new List<string>();
            UpdateResources();
            UpdateResourceManagers();
            currentLang = defaultLang;
        }

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

        // Returns true if lang is a file in the files list obtained with UpdateResources()
        public static bool IsAvailableLang(string lang)
        {
            return rmFiles.Contains(lang);
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

        // Call AddResourceFile() with all available files in Utilities.lang
        public static void UpdateResources()
        {
            List<Type> types = Assembly
                        .GetExecutingAssembly()
                        .GetTypes()
                        .Where(t => t.Namespace.StartsWith(langRoot))
                        .ToList<Type>();
            foreach(Type t in types)
            {
                AddResourceFile(t.Name);
            }
        }

        // returns files list as it contains every usable langCode
        public static List<string> GetAvailableLangs()
        {
            return rmFiles;
        }

        // Add given filename to the files list and init an entry in the ResourceManagers dictionnary with null value
        private static void AddResourceFile(string filename)
        {
            rmDictionnary.Add(filename, null);
            rmFiles.Add(filename);
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
            if (IsAvailableLang(lang))
            {
                return rmDictionnary[lang].GetString(stringCode);
            }
            else
            {
                return rmDictionnary[defaultLang].GetString(stringCode);
            }
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

        // Instanciate every usable ResourceManagers with their corresponding files in Utilities.lang
        private static void UpdateResourceManagers()
        {
            if (rmDictionnary.Count == 0)
                throw new Exception("No resources added in the bundle");

            string basename = langRoot;
            foreach (string key in rmFiles)//key is resource filename
            {
                string type = basename + "." + key; // string like Utilities.lang.en

                ResourceManager rm = rmDictionnary[key];
                if (rm != null) // release resources if already exists
                {
                    rm.ReleaseAllResources();
                }
                rmDictionnary[key] = null; // just to be sure
                rm = new ResourceManager(type, Assembly.GetExecutingAssembly());
                rmDictionnary[key] = rm;
            }
        }
    }
}
