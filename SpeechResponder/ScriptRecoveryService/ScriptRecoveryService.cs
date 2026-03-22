using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EddiSpeechResponder.ScriptRecoveryService
{
    public class ScriptRecoveryService ( EditScriptWindow scriptWindow )
    {
        static ScriptRecoveryService()
        {
            WorkingDirectory = Utilities.Constants.DATA_DIR;
        }

        private static readonly string WorkingDirectory;
        private static string _tempFileName => Path.Combine(WorkingDirectory, "editedScript.temp");
        private bool _scriptSaveCallGuard;
        private readonly object _lockRoot = new();
        private static CancellationTokenSource cancellationTS; // This must be static so that it is visible to child threads and tasks

        public static Script GetRecoveredScript()
        {
            var recoveredScriptPath = Path.Combine(WorkingDirectory, "editedScript.temp");

            if (!File.Exists(recoveredScriptPath))
            {
                return null;
            }

            var recoveringScript = File.ReadAllText(recoveredScriptPath);
            if (string.IsNullOrWhiteSpace(recoveringScript))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<Script>(recoveringScript);
        }

        /// <summary>
        ///        Will be called when ether the name of the script has changed or the script edit window was opened
        /// </summary>
        public void BeginScriptRecovery()
        {
            if (File.Exists(_tempFileName))
            {
                File.Delete(_tempFileName);
            }

            scriptWindow.revisedScript.PropertyChanged += _scriptWindow_PropertyChanged;
            cancellationTS = new CancellationTokenSource();
        }

        private void _scriptWindow_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EditScriptWindow.revisedScript.Value))
            {
                //the script value has changed. Begin the callguard and save the script value
                BeginScriptSave(scriptWindow);
            }
        }

        private void BeginScriptSave(EditScriptWindow window)
        {
            //this is guaranteed to run in the dispatcher so no worry about non locked accessing
            if (_scriptSaveCallGuard)
            {
                return;
            }

            _scriptSaveCallGuard = true;

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(3)).ConfigureAwait(false);
                    SaveRecoveryScript(window.revisedScript);
                }
                finally
                {
                    _scriptSaveCallGuard = false;
                }
            }, cancellationTS.Token);
        }

        /// <summary>
        ///        Should be called periodically and saves the script into the temp file
        /// </summary>
        public void SaveRecoveryScript(Script script)
        {
            lock (_lockRoot)
            {
                var serializeObject = JsonConvert.SerializeObject(script);
                File.WriteAllText(_tempFileName, serializeObject);
            }
        }

        /// <summary>
        ///        The script editor was closed and the temp file is no long needed
        /// </summary>
        public void StopScriptRecovery()
        {
            cancellationTS.Cancel();
            lock (_lockRoot)
            {
                if (File.Exists(_tempFileName))
                {
                    File.Delete(_tempFileName);
                }
            }

            scriptWindow.revisedScript.PropertyChanged -= _scriptWindow_PropertyChanged;
        }
    }
}
