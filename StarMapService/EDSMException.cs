using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiStarMapService
{
    [Serializable]
    public class EDSMException : Exception
    {
        public EDSMException()
        {
        }

        public EDSMException(string message)
            : base(message)
        {
        }

        public EDSMException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
