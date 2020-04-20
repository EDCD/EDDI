using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EddiSpeechResponder.Service
{
    public class ScriptRecoveryService
    {
        static ScriptRecoveryService()
        {
            WorkingDirectory = Utilities.Constants.DATA_DIR;
        }

        public ScriptRecoveryService(EditScriptWindow scriptWindow)
        {
            _scriptWindow = scriptWindow;
            _lockRoot = new object();
        }

        private static readonly string WorkingDirectory;
        private string _tempFileName;
        private EditScriptWindow _scriptWindow;
        private bool _scriptSaveCallGuard;
        private readonly object _lockRoot;

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
        ///		Will be called when ether the name of the script has changed or the script edit window was opened
        /// </summary>
        public void BeginScriptRecovery()
        {
            _tempFileName = Path.Combine(WorkingDirectory, "editedScript.temp");

            if (File.Exists(_tempFileName))
            {
                File.Delete(_tempFileName);
            }

            _scriptWindow.PropertyChanged += _scriptWindow_PropertyChanged;
        }

        private void _scriptWindow_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EditScriptWindow.ScriptValue))
            {
                //the script value has changed. Begin the callguard and save the script value
                BeginScriptSave(_scriptWindow);
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
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    SaveRecoveryScript(window.ScriptValue, 
                        window.ScriptName, 
                        window.ScriptDescription, 
                        window.Responder, 
                        window.Priority, 
                        window.ScriptDefaultValue);
                }
                finally
                {
                    _scriptSaveCallGuard = false;
                }
            });
        }

        /// <summary>
        ///		Should be called periodically and saves the script into the temp file
        /// </summary>
        public void SaveRecoveryScript(string scriptValue,
            string scriptName,
            string scriptDescription,
            bool isResponder, 
            int priority, 
            string defaultScript)
        {
            lock (_lockRoot)
            {
                var script = new Script(scriptName, scriptDescription, isResponder, scriptValue, priority, defaultScript);
                var serializeObject = JsonConvert.SerializeObject(script);
                File.WriteAllText(_tempFileName, serializeObject);
            }
        }

        /// <summary>
        ///		The script editor was closed and the temp file is no long needed
        /// </summary>
        public void StopScriptRecovery()
        {
            if (File.Exists(_tempFileName))
            {
                File.Delete(_tempFileName);
            }
            _scriptWindow.PropertyChanged -= _scriptWindow_PropertyChanged;
        }
    }
}
