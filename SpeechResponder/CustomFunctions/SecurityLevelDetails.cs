﻿using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class SecurityLevelDetails : ICustomFunction
    {
        public string name => "SecurityLevelDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.SecurityLevelDetails;
        public NativeFunction function => new NativeFunction((values) =>
        {
            var result = SecurityLevel.FromName(values[0].AsString) ?? 
                                     SecurityLevel.FromEDName(values[0].AsString);
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
