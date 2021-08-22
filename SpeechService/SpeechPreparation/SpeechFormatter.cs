using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using Utilities;

namespace EddiSpeechService.SpeechPreparation
{
    public static class SpeechFormatter
    {

        // Identify any statements that need to be separated into their own speech streams (e.g. audio or special voice effects)
        private static readonly string[] separatorsList =
        {
            @"(<audio.*?>)",
            @"(<transmit.*?>[\s\S]*?<\/transmit>)",
            @"(<voice.*?>[\s\S]*?<\/voice>)",
        };

        internal static void PrepareSpeech(VoiceDetails voice, ref string speech, out bool useSSML)
        {
            var lexicons = voice.GetLexicons();
            if (speech.Contains("<") || lexicons.Any())
            {
                // Keep XML version at 1.0. Version 1.1 is not recommended for general use. https://en.wikipedia.org/wiki/XML#Versions
                var xmlHeader = @"<?xml version=""1.0"" encoding=""UTF-8""?>";

                // SSML "speak" tag must use version 1.0. This synthesizer rejects version 1.1.
                var speakHeader = $@"<speak version=""1.0"" xmlns=""http://www.w3.org/2001/10/synthesis"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.w3.org/2001/10/synthesis http://www.w3.org/TR/speech-synthesis/synthesis.xsd"" xml:lang=""{voice.culturecode}"">";
                var speakFooter = @"</speak>";

                // Lexicons are applied as a child element to the `speak` element
                var lexiconString = lexicons.Aggregate(string.Empty, (current, lexiconFile) => current + $"<lexicon uri=\"{lexiconFile}\" type=\"application/pls+xml\"/>");

                var speakBody = lexiconString + EscapeSSML(speech);

                // Put it all together
                speech = xmlHeader + speakHeader + speakBody + speakFooter;

                if (voice.name.StartsWith("CereVoice "))
                {
                    // Cereproc voices do not respect `SpeakSsml` (particularly for IPA), but they do handle SSML via the `Speak` method.
                    Logging.Debug("Working around CereVoice SSML support");
                    useSSML = false;
                }
                else
                {
                    useSSML = true;
                }
            }
            else
            {
                useSSML = false;
            }
        }

        public static string EscapeSSML(string text)
        {
            // Our input text might have SSML elements in it but the rest needs escaping
            string result = text;

            // We need to make sure file names for the play function include a "/" (e.g. C:/)
            result = Regex.Replace(result, "(<.+?src=\")(.:)(.*?" + @"\/>)", "$1" + "$2%SSS%" + "$3");

            // Our valid SSML elements are audio, break, emphasis, play, phoneme, & prosody so encode these differently for now
            // Also escape any double quotes or single quotes inside the elements
            result = Regex.Replace(result, "(<[^>]*)\"", "$1%ZZZ%");
            result = Regex.Replace(result, "(<[^>]*)\"", "$1%ZZZ%");
            result = Regex.Replace(result, "(<[^>]*)\"", "$1%ZZZ%");
            result = Regex.Replace(result, "(<[^>]*)\"", "$1%ZZZ%");
            result = Regex.Replace(result, "(<[^>]*)\'", "$1%WWW%");
            result = Regex.Replace(result, "(<[^>]*)\'", "$1%WWW%");
            result = Regex.Replace(result, "(<[^>]*)\'", "$1%WWW%");
            result = Regex.Replace(result, "(<[^>]*)\'", "$1%WWW%");
            result = Regex.Replace(result, "<(audio.*?)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(break.*?)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(play.*?)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(phoneme.*?)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(/phoneme)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(prosody.*?)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(/prosody)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(emphasis.*?)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(/emphasis)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(transmit.*?)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(/transmit)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(voice.*?)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(/voice)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(say-as.*?)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(/say-as)>", "%XXX%$1%YYY%");

            // Cereproc uses some additional custom SSML tags (documented in https://www.cereproc.com/files/CereVoiceCloudGuide.pdf)
            result = Regex.Replace(result, "<(usel.*?)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(/usel)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(spurt.*?)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(/spurt)>", "%XXX%$1%YYY%");

            // Now escape anything that is still present
            result = SecurityElement.Escape(result) ?? "";

            // Put back the characters we hid
            result = Regex.Replace(result, "%XXX%", "<");
            result = Regex.Replace(result, "%YYY%", ">");
            result = Regex.Replace(result, "%ZZZ%", "\"");
            result = Regex.Replace(result, "%WWW%", "\'");
            result = Regex.Replace(result, "%SSS%", @"\");
            return result;
        }

        public static List<string> SeparateSpeechStatements(string speech)
        {
            // Separate speech into statements that can be handled differently & sequentially by the speech service
            var statements = new List<string>();
            var separators = string.Join("|", separatorsList);

            Match match = Regex.Match(speech, separators);
            if (match.Success)
            {
                string[] splitSpeech = new Regex(separators).Split(speech);
                foreach (string split in splitSpeech)
                {
                    if (Regex.Match(split, @"\S").Success) // Trim out non-word statements; match only words
                    {
                        statements.Add(split);
                    }
                }
            }
            else
            {
                statements.Add(speech);
            }
            return statements;
        }

        public static string FormatAudioTags(string statement)
        {
            statement = Regex.Replace(statement, "^.*<audio", "<audio");
            statement = Regex.Replace(statement, ">.*$", ">");
            return statement;
        }

        public static string StripRadioTags(string statement)
        {
            statement = statement.Replace("<transmit>", "");
            statement = statement.Replace("</transmit>", "");
            return statement;
        }

        public static void UnpackVoiceTags(string inputStatement, out string voice, out string outputStatement)
        {
            var match = Regex.Match(inputStatement, @"(?<=<voice name="")(.+)(?="" )(?>.)*(?<=>)(.+)(?=<\/voice>)");
            if (match.Groups.Count >= 2)
            {
                voice = match.Groups[1].ToString();
                outputStatement = match.Groups[2].ToString();
            }
            else
            {
                voice = null;
                outputStatement = null;
            }
        }

        public static string DisableIPA(string speech)
        {
            // User has disabled IPA so remove all IPA phoneme tags
            Logging.Debug("Phonetic speech is disabled, removing.");
            speech = Regex.Replace(speech, @"<phoneme.*?>", string.Empty);
            speech = Regex.Replace(speech, @"<\/phoneme>", string.Empty);
            return speech;
        }
    }
}
