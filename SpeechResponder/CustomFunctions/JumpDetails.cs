﻿using Cottle.Functions;
using Cottle.Values;
using EddiCore;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class JumpDetails : ICustomFunction
    {
        public string name => "JumpDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.JumpDetails;
        public Type ReturnType => typeof( JumpDetail );
        public NativeFunction function => new NativeFunction((values) =>
        {
            string value = values[0].AsString;
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            var result = EDDI.Instance.CurrentShip?.JumpDetails(value);
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
