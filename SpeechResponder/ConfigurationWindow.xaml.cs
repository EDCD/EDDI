﻿using System;
using Eddi;
using EddiEvents;
using EddiJournalMonitor;
using EddiShipMonitor;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security.AccessControl;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using EddiSpeechResponder.Service;
using Utilities;

namespace EddiSpeechResponder
{
	/// <summary>
	/// Interaction logic for ConfigurationWindow.xaml
	/// </summary>
	public partial class ConfigurationWindow : UserControl, INotifyPropertyChanged
	{
		private ObservableCollection<Personality> personalities;
		public ObservableCollection<Personality> Personalities
		{
			get { return personalities; }
			set { personalities = value; OnPropertyChanged("Personalities"); }
		}
		private Personality personality;
		public Personality Personality
		{
			get { return personality; }
			set
			{
				personality = value;
				viewEditContent = value != null && value.IsEditable ? "Edit" : "View";
				OnPropertyChanged("Personality");
			}
		}
		public string viewEditContent = "View";

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public ConfigurationWindow()
		{
			InitializeComponent();
			DataContext = this;

			ObservableCollection<Personality> personalities = new ObservableCollection<Personality>
			{
				// Add our default personality
				Personality.Default()
			};
			// Add local personalities
			foreach (Personality personality in Personality.AllFromDirectory())
			{
				if (personality != null)
				{
					personalities.Add(personality);
				}
			}
			Personalities = personalities;

			SpeechResponderConfiguration configuration = SpeechResponderConfiguration.FromFile();
			subtitlesCheckbox.IsChecked = configuration.Subtitles;
			subtitlesOnlyCheckbox.IsChecked = configuration.SubtitlesOnly;

			foreach (Personality personality in Personalities)
			{
				if (personality.Name == configuration.Personality)
				{
					Personality = personality;
					break;
				}
			}

			Dispatcher.BeginInvoke(new Action(() =>
			{
				var recoveredScript = ScriptRecoveryService.GetRecoveredScript();
				if (recoveredScript != null)
				{
					var messageBoxResult = MessageBox.Show(Properties.SpeechResponder.messagebox_recoveredScript,
						Properties.SpeechResponder.messagebox_recoveredScript_title,
						MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes,
						MessageBoxOptions.DefaultDesktopOnly);
					if (messageBoxResult == MessageBoxResult.Yes)
					{
						OpenEditScriptWindow(recoveredScript);
					}
				}
			}), DispatcherPriority.ApplicationIdle);
		}


		private void eddiScriptsEnabledUpdated(object sender, RoutedEventArgs e)
		{
			if (sender is CheckBox checkbox)
			{
				if (checkbox.IsLoaded)
				{
					updateScriptsConfiguration();
				}
			}
		}

		private void eddiScriptsPriorityUpdated(object sender, SelectionChangedEventArgs e)
		{
			if (sender is ComboBox comboBox)
			{
				if (comboBox.IsLoaded && (comboBox.IsDropDownOpen || comboBox.IsKeyboardFocused))
				{
					updateScriptsConfiguration();
				}
			}
		}

		private void editScript(object sender, RoutedEventArgs e)
		{
			Script script = ((KeyValuePair<string, Script>)((Button)e.Source).DataContext).Value;
			OpenEditScriptWindow(script);
		}

		private void OpenEditScriptWindow(Script script)
		{
			EditScriptWindow editScriptWindow = new EditScriptWindow(Personality.Scripts, script.Name);
			EDDI.Instance.SpeechResponderModalWait = true;
			editScriptWindow.ShowDialog();
			EDDI.Instance.SpeechResponderModalWait = false;
			if ((bool)editScriptWindow.DialogResult)
			{
				updateScriptsConfiguration();
				scriptsData.Items.Refresh();
			}
		}

		private void viewScript(object sender, RoutedEventArgs e)
		{
			Script script = ((KeyValuePair<string, Script>)((Button)e.Source).DataContext).Value;
			ViewScriptWindow viewScriptWindow = new ViewScriptWindow(script);
			viewScriptWindow.Show();
		}

		private void testScript(object sender, RoutedEventArgs e)
		{
			Script script = ((KeyValuePair<string, Script>)((Button)e.Source).DataContext).Value;
			SpeechResponder responder = new SpeechResponder();
			responder.Start();
			// See if we have a sample
			List<Event> sampleEvents;
			object sample = Events.SampleByName(script.Name);
			if (sample == null)
			{
				sampleEvents = new List<Event>();
			}
			else if (sample is string)
			{
				// It's as tring so a journal entry.  Parse it
				sampleEvents = JournalMonitor.ParseJournalEntry((string)sample);
			}
			else if (sample is Event)
			{
				// It's a direct event
				sampleEvents = new List<Event>() { (Event)sample };
			}
			else
			{
				Logging.Warn("Unknown sample type " + sample.GetType());
				sampleEvents = new List<Event>();
			}

			ScriptResolver scriptResolver = new ScriptResolver(Personality.Scripts);
			if (sampleEvents.Count == 0)
			{
				sampleEvents.Add(null);
			}
			foreach (Event sampleEvent in sampleEvents)
			{
				responder.Say(scriptResolver, ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor"))?.GetCurrentShip(), script.Name, sampleEvent, scriptResolver.priority(script.Name));
			}
		}

		private void resetOrDeleteScript(object sender, RoutedEventArgs e)
		{
			Script script = ((KeyValuePair<string, Script>)((Button)e.Source).DataContext).Value;
			if (script != null)
			{
				if (script.IsResettable)
				{
					resetScript(sender, e);
				}
				else
				{
					deleteScript(sender, e);
				}
			}
		}

		private void deleteScript(object sender, RoutedEventArgs e)
		{
			EDDI.Instance.SpeechResponderModalWait = true;
			Script script = ((KeyValuePair<string, Script>)((Button)e.Source).DataContext).Value;
			string messageBoxText = string.Format(Properties.SpeechResponder.delete_script_message, script.Name);
			string caption = Properties.SpeechResponder.delete_script_caption;
			MessageBoxResult result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
			switch (result)
			{
				case MessageBoxResult.Yes:
					// Remove the script from the list
					Personality.Scripts.Remove(script.Name);
					updateScriptsConfiguration();
					// We updated a property of the personality but not the personality itself so need to manually update items
					scriptsData.Items.Refresh();
					break;
			}
			EDDI.Instance.SpeechResponderModalWait = false;
		}
		private void resetScript(object sender, RoutedEventArgs e)
		{
			Script script = ((KeyValuePair<string, Script>)((Button)e.Source).DataContext).Value;
			// Resetting the script resets it to its value in the default personality
			if (Personality.Scripts.ContainsKey(script.Name))
			{
				string messageBoxText = string.Format(Properties.SpeechResponder.reset_script_message, script.Name);
				string caption = Properties.SpeechResponder.reset_script_button;
				MessageBoxResult result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
				switch (result)
				{
					case MessageBoxResult.Yes:
						script.Value = script.defaultValue;
						Personality.Scripts[script.Name] = script;
						updateScriptsConfiguration();
						scriptsData.Items.Refresh();
						break;
				}
			}
		}

		private void updateScriptsConfiguration()
		{
			if (Personality != null)
			{
				Personality.ToFile();
				EDDI.Instance.Reload("Speech responder");
			}
		}

		private void personalityChanged(object sender, SelectionChangedEventArgs e)
		{
			if (Personality != null)
			{
				SpeechResponderConfiguration configuration = SpeechResponderConfiguration.FromFile();
				configuration.Personality = Personality.Name;
				configuration.ToFile();
				EDDI.Instance.Reload("Speech responder");
			}
		}

		private void newScriptClicked(object sender, RoutedEventArgs e)
		{
			string baseName = "New function";
			string scriptName = baseName;
			int i = 2;
			while (Personality.Scripts.ContainsKey(scriptName))
			{
				scriptName = baseName + " " + i++;
			}
			Script script = new Script(scriptName, null, false, null);
			Personality.Scripts.Add(script.Name, script);

			// Now fire up an edit
			EDDI.Instance.SpeechResponderModalWait = true;
			EditScriptWindow editScriptWindow = new EditScriptWindow(Personality.Scripts, script.Name);
			if (editScriptWindow.ShowDialog() == true)
			{
				Personality.ToFile();
				EDDI.Instance.Reload("Speech responder");
			}
			else
			{
				Personality.Scripts.Remove(script.Name);
			}
			scriptsData.Items.Refresh();
			EDDI.Instance.SpeechResponderModalWait = false;
		}

		private void copyPersonalityClicked(object sender, RoutedEventArgs e)
		{
			EDDI.Instance.SpeechResponderModalWait = true;
			CopyPersonalityWindow window = new CopyPersonalityWindow(Personality)
			{
				Owner = Window.GetWindow(this)
			};
			if (window.ShowDialog() == true)
			{
				string PersonalityName = window.PersonalityName?.Trim();
				string PersonalityDescription = window.PersonalityDescription?.Trim();
				Personality newPersonality = Personality.Copy(PersonalityName, PersonalityDescription);
				Personalities.Add(newPersonality);
				Personality = newPersonality;
			}
			EDDI.Instance.SpeechResponderModalWait = false;
		}

		private void deletePersonalityClicked(object sender, RoutedEventArgs e)
		{
			EDDI.Instance.SpeechResponderModalWait = true;
			string messageBoxText = string.Format(Properties.SpeechResponder.delete_personality_message, Personality.Name);
			string caption = Properties.SpeechResponder.delete_personality_caption;
			MessageBoxResult result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
			switch (result)
			{
				case MessageBoxResult.Yes:
					// Remove the personality from the list and the local filesystem
					Personality oldPersonality = Personality;
					Personalities.Remove(oldPersonality);
					Personality = Personalities[0];
					oldPersonality.RemoveFile();
					break;
			}
			EDDI.Instance.SpeechResponderModalWait = false;
		}

		private void subtitlesEnabled(object sender, RoutedEventArgs e)
		{
			if (sender is CheckBox checkBox)
			{
				if (checkBox.IsLoaded)
				{
					SpeechResponderConfiguration configuration = SpeechResponderConfiguration.FromFile();
					configuration.Subtitles = true;
					configuration.ToFile();
					EDDI.Instance.Reload("Speech responder");
				}
			}
		}

		private void subtitlesDisabled(object sender, RoutedEventArgs e)
		{
			if (sender is CheckBox checkBox)
			{
				if (checkBox.IsLoaded)
				{
					SpeechResponderConfiguration configuration = SpeechResponderConfiguration.FromFile();
					configuration.Subtitles = false;
					configuration.ToFile();
					EDDI.Instance.Reload("Speech responder");
				}
			}
		}

		private void subtitlesOnlyEnabled(object sender, RoutedEventArgs e)
		{
			if (sender is CheckBox checkBox)
			{
				if (checkBox.IsLoaded)
				{
					SpeechResponderConfiguration configuration = SpeechResponderConfiguration.FromFile();
					configuration.SubtitlesOnly = true;
					configuration.ToFile();
					EDDI.Instance.Reload("Speech responder");
				}
			}
		}

		private void subtitlesOnlyDisabled(object sender, RoutedEventArgs e)
		{
			if (sender is CheckBox checkBox)
			{
				if (checkBox.IsLoaded)
				{
					SpeechResponderConfiguration configuration = SpeechResponderConfiguration.FromFile();
					configuration.SubtitlesOnly = false;
					configuration.ToFile();
					EDDI.Instance.Reload("Speech responder");
				}
			}
		}

		private void SpeechResponderHelp_Click(object sender, RoutedEventArgs e)
		{
			MarkdownWindow speechResponderHelpWindow = new MarkdownWindow("speechResponderHelp.md");
			speechResponderHelpWindow.Show();
		}
	}

	public class BooleanAndConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			foreach (object value in values)
			{
				if ((value is bool) && (bool)value == false)
				{
					return false;
				}
			}
			return true;
		}
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException("BooleanAndConverter is a OneWay converter.");
		}
	}
}
