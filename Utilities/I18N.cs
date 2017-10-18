using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Resources;
using System.Reflection;

namespace Utilities
{
    class I18N
    {
        private static ResourceManager i18nManager = new ResourceManager("", Assembly.GetExecutingAssembly());
    }
}
