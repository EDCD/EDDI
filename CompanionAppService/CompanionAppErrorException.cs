using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiCompanionAppService
{
    /// <summary>Exceptions thrown due to API errors</summary>
    [Serializable]
    public class EliteDangerousCompanionAppErrorException : EliteDangerousCompanionAppException
    {
        public EliteDangerousCompanionAppErrorException() : base() { }

        public EliteDangerousCompanionAppErrorException(string message) : base(message) { }
    }
}
