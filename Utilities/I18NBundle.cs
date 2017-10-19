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
    public class I18NBundle
    {
        private const string defaultLang = "en";
        private string currentLang;
        private Dictionary<string, ResourceManager> rmDictionnary;
        private List<String> rmFiles;
        private static I18NBundle instance = new I18NBundle();
        private string langRoot;

        private I18NBundle()
        {
            langRoot = "Utilities.lang";
            currentLang = defaultLang;
            rmDictionnary = new Dictionary<string, ResourceManager>();
            rmFiles = new List<string>();
            UpdateResources();
            UpdateResourceManagers();
        }

        public static I18NBundle ResetInstance()
        {
            instance = new I18NBundle();
            return instance;
        }
        
        public string GetLang()
        {
            return currentLang;
        }

        public bool SetLang(string lang)
        {
            if (IsAvailableLang(lang))
            {
                currentLang = lang;
                return true;
            }
            return false;
        }

        public void FallbackLang()
        {
            currentLang = defaultLang;
        }

        public bool IsAvailableLang(string lang)
        {
            return rmFiles.Contains(lang);
        }

        public bool SetLangOrFallback(string lang)
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

        public void UpdateResources()
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

        public List<string> GetAvailableLangs()
        {
            return rmFiles;
        }

        public static I18NBundle GetInstance()
        {
            if (instance == null)
            {
                instance = new I18NBundle();
            }

            return instance;
        }

        private void AddResourceFile(string filename)
        {
            rmDictionnary.Add(filename, null);
            rmFiles.Add(filename);
        }

        public string GetString(string stringCode)
        {
            return GetString(stringCode, currentLang);
        }

        public string GetString(string stringCode, string lang)
        {
            ResourceManager rm = rmDictionnary[lang];
            if(rm == null)
            {
                rm = rmDictionnary[defaultLang];
            }
            return rm.GetString(stringCode);
        }

        private void UpdateResourceManagers()
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
