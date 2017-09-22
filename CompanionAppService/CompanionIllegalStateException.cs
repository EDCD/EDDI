using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiCompanionAppService
{
    /// <summary>Exceptions thrown due to illegal service state</summary>
    [Serializable]
    public class EliteDangerousCompanionAppIllegalStateException : EliteDangerousCompanionAppException
    {
        public EliteDangerousCompanionAppIllegalStateException() : base() { }

        public EliteDangerousCompanionAppIllegalStateException(string message) : base(message) { }
    }
}
