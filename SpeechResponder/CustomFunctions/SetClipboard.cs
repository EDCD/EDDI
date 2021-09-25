using Cottle.Functions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System.Threading;
using System.Windows;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class SetClipboard : ICustomFunction
    {
        public string name => "SetClipboard";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.SetClipboard;

        public NativeFunction function => new NativeFunction((values) =>
        {
            var text = values[0].AsString;
            if (!string.IsNullOrEmpty(text))
            {
                var clipboardThread = new Thread(() => { Clipboard.SetData(DataFormats.Text, text); });
                clipboardThread.SetApartmentState(ApartmentState.STA);
                clipboardThread.Start();
                clipboardThread.Join();
                return "";
            }
            else
            {
                return
                    "The SetClipboard function is used improperly. Please review the documentation for correct usage.";
            }
        }, 1);
    }
}