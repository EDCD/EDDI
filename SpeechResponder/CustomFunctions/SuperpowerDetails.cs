﻿using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class SuperpowerDetails : ICustomFunction
    {
        public string name => "SuperpowerDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.SuperpowerDetails;
        public Type ReturnType => typeof( Superpower );
        public NativeFunction function => new NativeFunction((values) =>
        {
            var result = Superpower.FromNameOrEdName(values[0].AsString);
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
