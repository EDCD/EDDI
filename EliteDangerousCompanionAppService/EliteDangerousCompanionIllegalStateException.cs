using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousCompanionAppService
{
    /// <summary>Exceptions thrown due to illegal service state</summary>
    public class EliteDangerousCompanionAppIllegalStateException : EliteDangerousCompanionAppException
    {
        public EliteDangerousCompanionAppIllegalStateException() : base() { }

        public EliteDangerousCompanionAppIllegalStateException(string message) : base(message) { }
    }
}
