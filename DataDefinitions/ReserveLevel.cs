using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiDataDefinitions
{
    public class ReserveLevel : ResourceBasedLocalizedEDName<ReserveLevel>
    {
        static ReserveLevel()
        {
            resourceManager = Properties.ReserveLevel.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new ReserveLevel(edname);

            var Depleted = new ReserveLevel("Depleted");
            var Minor = new ReserveLevel("Minor");
            var Common = new ReserveLevel("Common");
            var Major = new ReserveLevel("Major");
            var Pristine = new ReserveLevel("Pristine");
        }

        public static readonly ReserveLevel None = new ReserveLevel("None");

        // dummy used to ensure that the static constructor has run
        public ReserveLevel() : this("")
        { }

        private ReserveLevel(string edname) : base(edname, edname)
        { }
    }
}
