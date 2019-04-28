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

            None = new ReserveLevel("None");
            var DepletedResources = new ReserveLevel("DepletedResources");
            var LowResources = new ReserveLevel("LowResources");
            var CommonResources = new ReserveLevel("CommonResources");
            var MajorResources = new ReserveLevel("MajorResources");
            var PristineResources = new ReserveLevel("PristineResources");
        }

        public static readonly ReserveLevel None;

        // dummy used to ensure that the static constructor has run
        public ReserveLevel() : this("")
        { }

        private ReserveLevel(string edname) : base(edname, edname)
        { }
    }
}
