using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System.Text.RegularExpressions;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class ShipDetails : ICustomFunction
    {
        public string name => "ShipDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.ShipDetails;
        public NativeFunction function => new NativeFunction((values) =>
        {
            // The inputs to this function might include phonetic SSML tags
            // (to improve phonetic pronunciations). We'll need to strip those.
            var tidiedModel = Regex.Replace(values[0].AsString, @"<phoneme.*?>", string.Empty);
            tidiedModel = Regex.Replace(tidiedModel, @"<\/phoneme>", string.Empty);
            tidiedModel = tidiedModel // Ship models with mark numbers need to be reverted to abbreviated forms
                .Replace(" Mark 1", " Mk. I")
                .Replace(" Mark 2", " Mk. II")
                .Replace(" Mark 3", " Mk. III")
                .Replace(" Mark 4", " Mk. IV")
                .Replace(" Mark 5", " Mk. V");
            tidiedModel = tidiedModel.Trim();

            var result = ShipDefinitions.FromModel(tidiedModel);
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
