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
        private static I18NBundle langsBundle = I18NBundle.GetInstance();
        private static string langCode = GetSystemLangCode();

        public static string GetSystemLangCode()
        {
            return CultureInfo.CurrentUICulture.Name.Split('-')[0];
        }

        public static string GetCurrentLangCode()
        {
            return langCode;
        }

        public static string GetString(string code)
        {
            return GetString(code, GetCurrentLangCode());
        }

        public static string GetString(string code, string lang)
        {
            return langsBundle.GetString(code);
        }

        public static List<string> GetAvailableLang()
        {
            return langsBundle.GetAvailableLangs();
        }

        public static string GetLangPath()
        {
            string path =  Path.Combine(Environment.CurrentDirectory, @"lang\");
            return path;
        }
    }
}
