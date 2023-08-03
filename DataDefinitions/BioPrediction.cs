using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>Try to guess what biologicals are present on a body.</summary>
    public class BioPrediction
    {
        public BioPrediction ()
        { }

        /// <summary>Using body data iterate through biologicals and return candidates as a simple string list.</summary>
        public List<string> getBios( Body body )
        {
            List<string> bios = new List<string>();
            return bios;
        }
    }
}
