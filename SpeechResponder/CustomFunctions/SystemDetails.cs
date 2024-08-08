using Cottle;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Reflection;
using Utilities;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class SystemDetails : ICustomFunction
    {
        public string name => "SystemDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.SystemDetails;
        public Type ReturnType => typeof( StarSystem );
        public IFunction function => Function.CreateNativeMinMax( ( runtime, values, writer ) =>
        {
            try
            {
                var result = values.Count == 0 
                    ? EDDI.Instance.CurrentStarSystem 
                    : StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[0].AsString, true);

                var distanceFromHome = result?.DistanceFromStarSystem(EDDI.Instance.HomeStarSystem);
                if (distanceFromHome != null)
                {
                    Logging.Debug("Distance from home is " + distanceFromHome);
                    result.distancefromhome = distanceFromHome;
                }

                return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
            }
            catch (Exception e)
            {
                return $"The SystemDetails function is used incorrectly. {e.Message}.";
            }
        }, 0, 1);
    }
}
