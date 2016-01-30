using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousCompanionAppService
{
    /// <summary>Exceptions thrown due to API errors</summary>
    public class EliteDangerousCompanionAppErrorException : EliteDangerousCompanionAppException
    {
        public EliteDangerousCompanionAppErrorException() : base() { }

        public EliteDangerousCompanionAppErrorException(string message) : base(message) { }
    }
}
