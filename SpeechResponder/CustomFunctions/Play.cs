﻿using Cottle.Functions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class Play : ICustomFunction
    {
        public string name => "Play";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.Play;
        public Type ReturnType => typeof( string );
        public NativeFunction function => new NativeFunction((values) =>
        {
            // The file to play
            string fileName = values[0].AsString;

            // Whether the audio should be played asynchronously
            bool async = false;
            if (values.Count > 1)
            {
                async = values[1].AsBoolean;
            }

            // The volume override (where 100 is normal max volume)
            decimal? volumeOverride = null;
            if (values.Count > 2)
            {
                volumeOverride = values[2].AsNumber;
            }

            // Use a psuedo-SSML tag to pass the result to the Speech Service
            if (volumeOverride != null)
            {
                return $@"<audio src=""{fileName}"" async=""{async}"" volume=""{volumeOverride}"" />";
            }
            return $@"<audio src=""{fileName}"" async=""{async}"" />";
        }, 1, 3);
    }
}
