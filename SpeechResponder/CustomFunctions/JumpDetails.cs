﻿using Cottle.Functions;
using Cottle.Values;
using EddiCore;
using EddiNavigationService;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class JumpDetails : ICustomFunction
    {
        public string name => "JumpDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.JumpDetails;
        public NativeFunction function => new NativeFunction((values) =>
        {
            string value = values[0].AsString;
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            var result = JumpCalcs.JumpDetails(value, EDDI.Instance.CurrentShip);
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
