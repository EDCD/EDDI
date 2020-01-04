using Newtonsoft.Json;

namespace Utilities
{
    public static class CustomExtensionMethods
    {
        public static bool DeepEquals(this object obj, object another)
        {
            if (ReferenceEquals(obj, another)) { return true; }
            if ((obj == null) || (another == null)) { return false; }
            if (obj.GetType() != another.GetType()) { return false; }

            var objJson = JsonConvert.SerializeObject(obj);
            var anotherJson = JsonConvert.SerializeObject(another);

            return objJson == anotherJson;
        }
    }
}
