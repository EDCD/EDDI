using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiCompanionAppService
{
    /// <summary>Exceptions thrown due to authentication errors</summary>
    [Serializable]
    public class EliteDangerousCompanionAppAuthenticationException : EliteDangerousCompanionAppException
    {
        public EliteDangerousCompanionAppAuthenticationException() : base() { }

        public EliteDangerousCompanionAppAuthenticationException(string message) : base(message) { }
    }
}
