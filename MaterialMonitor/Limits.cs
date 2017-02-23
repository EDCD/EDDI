using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiMaterialMonitor
{
    /// <summary>
    /// Limits for a material
    /// </summary>
    public class Limits
    {
        public int current { get; set; }
        public int? minimum { get; set; }
        public int? desired { get; set; }
        public int? maximum { get; set; }

        public Limits(int current, int? minimum, int? desired, int? maximum)
        {
            this.current = current;
            this.minimum = minimum;
            this.desired = desired;
            this.maximum = maximum;
        }
    }
}
