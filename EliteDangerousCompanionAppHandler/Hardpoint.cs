using Newtonsoft.Json.Linq;

namespace EliteDangerousCompanionAppService
{
    public class Hardpoint
    {
        public int Size { get; set; }
        public Module Module { get; set; }

        public static Hardpoint FromProfile(dynamic json)
        {
            Hardpoint Hardpoint = new Hardpoint();

            string name = json.Name;
            if (name.StartsWith("Huge"))
            {
                Hardpoint.Size = 4;
            }
            else if (name.StartsWith("Large"))
            {
                Hardpoint.Size = 3;
            }
            else if (name.StartsWith("Medium"))
            {
                Hardpoint.Size = 2;
            }
            else if (name.StartsWith("Small"))
            {
                Hardpoint.Size = 1;
            }
            else if (name.StartsWith("Tiny"))
            {
                Hardpoint.Size = 0;
            }

            if (json.Value is JObject)
            {
                JToken value;
                if (json.Value.TryGetValue("module", out value))
                {
                    Hardpoint.Module = Module.FromProfile(name, json.Value);
                }
            }

            return Hardpoint;
        }
    }
}
