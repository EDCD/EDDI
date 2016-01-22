using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousCompanionAppService
{
    public class Module
    {
        // Definition of the module
        public string name { get; set; }
        public int size { get; set; } // == class, but saves mucking about with reserved symbols
        public string grade { get; set; }

        // State of the module
        public bool enabled { get; set; }
        public int priority { get; set; }
        public decimal integrity { get; set; }
    }
}
