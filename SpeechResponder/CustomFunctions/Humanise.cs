﻿using Cottle.Functions;
using EddiSpeechResponder.Service;
using EddiSpeechService;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class Humanise : ICustomFunction
    {
        public string name => "Humanise";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.Humanise;
        public Type ReturnType => typeof( string );
        public NativeFunction function => new NativeFunction((values) =>
        {
            return Translations.Humanize(values[0].AsNumber);
        }, 1);
    }
}
