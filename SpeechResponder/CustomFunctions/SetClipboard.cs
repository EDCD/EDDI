using Cottle;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Threading;
using System.Windows;
using Utilities;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class SetClipboard : ICustomFunction
    {
        public string name => "SetClipboard";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.SetClipboard;
        public Type ReturnType => typeof( string );
        public IFunction function => Function.CreatePure1( ( runtime, input ) =>
        {
            var text = input.AsString;
            if ( !string.IsNullOrEmpty( text ) )
            {
                var clipboardThread = new Thread( () =>
                {
                    try
                    {
                        Clipboard.SetData(DataFormats.Text, text);
                    }
                    catch ( Exception e )
                    {
                        Logging.Error( "Failed to set clipboard", e );
                    }
                });
                clipboardThread.SetApartmentState( ApartmentState.STA );
                clipboardThread.Start();
                clipboardThread.Join();
                return "";
            }
            else
            {
                return "The SetClipboard function is used improperly. Please review the documentation for correct usage.";
            }
        } );
    }
}