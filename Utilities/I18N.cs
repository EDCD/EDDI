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
    public class I18N
    {
        private const string defaultLang = "en";
        private static string currentLang;
        private static Dictionary<string, ResourceManager> rmDictionnary;
        private static List<String> rmFiles;
        private static string langRoot;

        static I18N()
        {
            Init();
        }

        private static void Init()
        {
            langRoot = "Utilities.lang";
            //currentLang = defaultLang;
            currentLang = GetSystemLangCode();
            rmDictionnary = new Dictionary<string, ResourceManager>();
            rmFiles = new List<string>();
            UpdateResources();
            UpdateResourceManagers();
        }

        public static void Reset()
        {
            Init();
        }

        public static string GetSystemLangCode()
        {
            return CultureInfo.CurrentUICulture.Name.Split('-')[0];
        }

        public static string GetLang()
        {
            return currentLang;
        }

        public static bool SetLang(string lang)
        {
            if (IsAvailableLang(lang))
            {
                currentLang = lang;
                return true;
            }
            return false;
        }

        public static void FallbackLang()
        {
            currentLang = defaultLang;
        }

        public static bool IsAvailableLang(string lang)
        {
            return rmFiles.Contains(lang);
        }

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

        public static List<string> GetAvailableLangs()
        {
            return rmFiles;
        }

        private static void AddResourceFile(string filename)
        {
            rmDictionnary.Add(filename, null);
            rmFiles.Add(filename);
        }

        public static string GetString(string stringCode)
        {
            return GetString(stringCode, currentLang);
        }

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

        public static string GetStringWithArgs(string stringCode, string[] args)
        {
            return GetStringWithArgs(stringCode, currentLang, args);
        }

        public static string GetStringWithArgs(string stringCode, string langCode, string[] args)
        {
            string s = GetString(stringCode, langCode);
            for(int i=0; i<args.Length; i++)
            {
                s = s.Replace("{"+i+"}", args[i]);
            }
            return s;
        }

        private static void UpdateResourceManagers()
        {
            if (rmDictionnary.Count == 0)
                throw new Exception("No resources added in the bundle");

            string basename = langRoot;
            foreach (string key in rmFiles)
            {//key is filename
                string type = basename + "." + key;
                //set a new ResourceManager to match current locale
                ResourceManager rm = rmDictionnary[key];
                if (rm != null)
                    rm.ReleaseAllResources();
                rmDictionnary[key] = null;
                rm = new ResourceManager(type, Assembly.GetExecutingAssembly());
                rmDictionnary[key] = rm;
            }
        }
    }
}
