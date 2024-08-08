using Cottle;
using EddiBgsService;
using EddiCore;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Reflection;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class FactionDetails : ICustomFunction
    {
        public string name => "FactionDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.FactionDetails;
        public Type ReturnType => typeof( Faction );

        private static readonly BgsService bgsService = new BgsService();

        public IFunction function => Function.CreateNativeMinMax( ( runtime, values, writer ) =>
        {
            Faction result;
            if (values.Count == 0)
            {
                result = EDDI.Instance.CurrentStarSystem?.Faction;
            }
            else if (values.Count == 1)
            {
                result = bgsService.GetFactionByName(values[0].AsString);
            }
            else
            {
                result = bgsService.GetFactionByName(values[0].AsString, values[1].AsString);
            }
            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        }, 1, 2);
    }
}
