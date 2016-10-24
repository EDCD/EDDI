using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class Translation
    {
        public string from { get; private set; }
        public string to { get; private set; }

        public Translation(string from, string to)
        {
            this.from = from;
            this.to = to;
        }
    }
}
