using Cottle.Functions;
using Cottle.Values;
using EddiCore;
using EddiShipMonitor;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class JumpDetails : ICustomFunction
    {
        public string name => "JumpDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => @"
This function will provide jump information based on your ship loadout and current fuel level, dependent on the following types:

* `next` range of next jump at current fuel mass and current laden mass
* `max` maximum jump range at minimum fuel mass and current laden mass
* `total` total range of multiple jumps from current fuel mass and current laden mass
* `full` total range of multiple jumps from maximum fuel mass and current laden mass

The returned `JumpDetail` object contains properties `distance` and `jumps`.

Common usage is to provide distance and number of jumps for a specific query:

    {set detail to JumpDetails(""total"")}
    Total jump range at current fuel levels is {round(detail.distance, 1)} light years with {detail.jumps} jumps until empty.
    {if detail.distance < destdistance:
	    Warning: Fuel levels insufficient to reach destination. Fuel scooping is required.
	}";
        public NativeFunction function => new NativeFunction((values) =>
        {
            string value = values[0].AsString;
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            var result = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor"))?.JumpDetails(value);
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
