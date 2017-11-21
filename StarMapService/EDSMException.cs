using System;

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
