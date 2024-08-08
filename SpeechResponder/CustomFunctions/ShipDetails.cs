using Cottle;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class ShipDetails : ICustomFunction
    {
        public string name => "ShipDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.ShipDetails;
        public Type ReturnType => typeof( Ship );
        public IFunction function => Function.CreateNative1( ( runtime, shipModel, writer ) =>
        {
            // The inputs to this function might include phonetic SSML tags
            // (to improve phonetic pronunciations). We'll need to strip those.
            var tidiedModel = Regex.Replace(shipModel.AsString, @"<phoneme.*?>", string.Empty);
            tidiedModel = Regex.Replace(tidiedModel, @"<\/phoneme>", string.Empty);
            tidiedModel = tidiedModel // Ship models with mark numbers need to be reverted to abbreviated forms
                .Replace(" Mark 1", " Mk. I")
                .Replace(" Mark 2", " Mk. II")
                .Replace(" Mark 3", " Mk. III")
                .Replace(" Mark 4", " Mk. IV")
                .Replace(" Mark 5", " Mk. V");
            tidiedModel = tidiedModel.Trim();

            var result = ShipDefinitions.FromModel(tidiedModel);
            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        });
    }
}
