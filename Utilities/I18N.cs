using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.IO;

namespace Utilities
{
    public class I18N
    {
        private static ResourceManager i18nManager = new ResourceManager("", Assembly.GetExecutingAssembly());
        private static string langCode = getSystemLangCode();

        public static string getSystemLangCode()
        {
            return CultureInfo.CurrentUICulture.Name.Split('-')[0];
        }

        public static string getCurrentLangCode()
        {
            return langCode;
        }

        public static string getString(string code)
        {
            return getString(code, getCurrentLangCode());
        }

        public static string getString(string code, string lang)
        {
            return i18nManager.GetString(code, CultureInfo.CreateSpecificCulture(lang));
        }

        public static List<string> getAvailableLang()
        {
            //TODO get resources correctly
            List<string> files = Directory.GetFiles(getLangPath(), "*.resx").ToList<string>();
            List<string> langs = new List<string>();
            string suffix = ".resx";
            foreach(string file in files)
            {
                if (file.EndsWith(".resx"))
                {
                    string lang = file.Substring(0, file.Length - suffix.Length);
                    langs.Add(lang);
                }
            }
            return langs;
        }

        public static string getLangPath()
        {
            string path =  Path.Combine(Environment.CurrentDirectory, @"lang\");
            return path;
        }
    }
}
