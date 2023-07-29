using Cottle.Functions;
using Cottle.Values;
using EddiBgsService;
using EddiCore;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;

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

        public NativeFunction function => new NativeFunction((values) =>
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
            return new ReflectionValue(result ?? new object());
        }, 1, 2);
    }
}
