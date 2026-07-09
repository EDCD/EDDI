using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Utilities;

namespace EddiSpeechService.SpeechPreparation
{
    public static class SpeechFormatter
    {
        private const string AmazonPollyProviderTypeId = "AmazonPolly";
        private const string LegacyAmazonPollyVoicePrefix = "Amazon Polly ";
        internal static readonly XmlSchemaSet lexiconSchemas = new();
        private static readonly object lexiconSchemaLock = new();
        private static readonly object lexiconQuarantineLock = new();
        private static bool lexiconSchemasLoaded;

        // Identify any statements that need to be separated into their own speech streams (e.g. audio or special voice effects)
        private static readonly string[] separatorsList =
        [
            @"(<audio.*?\/>)",
            @"(<transmit.*?>[\s\S]*?<\/transmit>)",
            @"(<voice.*?>[\s\S]*?<\/voice>)"
        ];

        /// <summary>
        /// Removes excess whitespace and SSML &lt;break/&gt; tags.
        /// </summary>
        /// <param name="s">The target string</param>
        /// <returns>The trimmed string</returns>
        public static string TrimSpeech(string s)
        {
            // Skip empty speech, speech containing nothing except one or more pauses / breaks,
            // and pauses appended to the end of speech with nothing following.
            s = s?.Trim();
            if (!string.IsNullOrEmpty(s))
            {
                s = GeneratedRegex.EndingPauseRegex().Replace(s, "");
                return s;
            }
            return string.Empty;
        }

        internal static void PrepareSpeech(VoiceDetails voice, ref string speech, out bool useSSML)
        {
            var lexicons = GetLexicons(voice);
            var isAmazonPollyVoice = IsAmazonPollyVoice( voice );
            if (speech.Contains('<') || lexicons.Count > 0 )
            {
                // Keep XML version at 1.0. Version 1.1 is not recommended for general use. https://en.wikipedia.org/wiki/XML#Versions
                var xmlHeader = @"<?xml version=""1.0"" encoding=""UTF-8""?>";

                // SSML "speak" tag must use version 1.0. This synthesizer rejects version 1.1.
                var speakHeader = $@"<speak version=""1.0"" xmlns=""http://www.w3.org/2001/10/synthesis"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.w3.org/2001/10/synthesis http://www.w3.org/TR/speech-synthesis/synthesis.xsd"" xml:lang=""{voice.culturecode}"">";
                var speakFooter = @"</speak>";

                // Lexicons are applied as a child element to the `speak` element. For Amazon Polly voices, the lexicon must be managed via the AWS Management Console.
                var lexiconString = lexicons.Count > 0 && !isAmazonPollyVoice
                    ? lexicons.Aggregate(string.Empty, (current, lexiconFile) => current + $"<lexicon uri=\"{lexiconFile}\" type=\"application/pls+xml\"/>") 
                    : string.Empty;

                var speakBody = lexiconString + EscapeSSML(speech);

                // Put it all together
                speech = xmlHeader + speakHeader + speakBody + speakFooter;

                if (isAmazonPollyVoice)
                {
                    // Amazon Polly voices do not respect `SpeakSsml` (particularly for IPA), but they do handle SSML via the `Speak` method.
                    Logging.Debug("Working around Amazon Polly SSML support");
                    useSSML = false;
                }
                else if (voice.name.StartsWith("CereVoice "))
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

        private static bool IsAmazonPollyVoice ( VoiceDetails voice )
        {
            return string.Equals( voice?.synthType, AmazonPollyProviderTypeId, StringComparison.InvariantCultureIgnoreCase ) ||
                   ( voice?.name?.StartsWith( LegacyAmazonPollyVoicePrefix, StringComparison.InvariantCultureIgnoreCase ) ?? false );
        }

        public static string EscapeSSML(string text)
        {
            // Our input text might have SSML elements in it but the rest needs escaping
            var result = text;

            // We need to make sure file names for the play function include a "/" (e.g. C:/)
            result = GeneratedRegex.SrcFixRegex().Replace(result, "$1$2%SSS%$3");

            // Escape any double quotes or single quotes inside the elements
            result = GeneratedRegex.SsmlTagRegex().Replace( result, m => m.Value
                .Replace( "\"", "%ZZZ%" )
                .Replace( "\'", "%WWW%" ) );

            // Hide valid SSML tags
            result = GeneratedRegex.ValidTagRegex().Replace( result, "%XXX%$1%YYY%" );

            // Escape everything else
            result = SecurityElement.Escape( result );

            // Restore placeholders
            result = result
                .Replace( "%XXX%", "<" )
                .Replace( "%YYY%", ">" )
                .Replace( "%ZZZ%", "\"" )
                .Replace( "%WWW%", "'" )
                .Replace( "%SSS%", @"\" );

            return result;
        }

        public static List<string> SeparateSpeechSegments(string speech)
        {
            // Separate speech into statements that can be handled differently & sequentially by the speech service
            var statements = new List<string>();
            var separators = string.Join("|", separatorsList);

            var match = Regex.Match(speech, separators);
            if (match.Success)
            {
                var splitSpeech = new Regex(separators).Split(speech);
                foreach (var split in splitSpeech)
                {
                    if ( GeneratedRegex.NonWordRegex().Match(split).Success) // Trim out non-word statements; match only words
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

        public static string StripRadioTags(string statement)
        {
            statement = statement.Replace("<transmit>", "");
            statement = statement.Replace("</transmit>", "");
            return statement;
        }

        public static string StripSSML ( string speech )
        {
            speech = GeneratedRegex.SsmlTagRegex().Replace( speech, string.Empty );
            return speech;
        }

        public static void UnpackAudioTags(string inputStatement, out string fileName, out bool async, out decimal? volumeOverride)
        {
            fileName = string.Empty;
            async = false;
            volumeOverride = null;
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(inputStatement);
            if (xmlDoc.FirstChild?.Attributes != null)
            {
                foreach (XmlAttribute attribute in xmlDoc.FirstChild.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "src":
                        {
                            fileName = attribute.Value;
                            break;
                        }
                        case "async":
                        {
                            async = bool.Parse(attribute.Value);
                            break;
                        }
                        case "volume":
                        {
                            volumeOverride = decimal.Parse(attribute.Value);
                            break;
                        }
                    }
                }
            }
        }

        public static void UnpackVoiceTags(string inputStatement, out string voice, out string outputStatement)
        {
            voice = GeneratedRegex.VoiceNameRegex().Match( inputStatement ).Value;
            outputStatement = GeneratedRegex.VoiceSpeechExtractionRegex().Match( inputStatement ).Value;
        }

        public static string DisableIPA(string speech)
        {
            // User has disabled IPA so remove all IPA phoneme tags
            Logging.Debug("Phonetic speech is disabled, removing.");
            speech = GeneratedRegex.OpenPhonemeRegex().Replace( speech, string.Empty )
                .Replace( "</phoneme>", string.Empty );
            return speech;
        }

        #region Lexicons

        internal static void EnsureLexiconSchemasLoaded ( Assembly assembly = null )
        {
            if ( lexiconSchemasLoaded )
            {
                return;
            }

            lock ( lexiconSchemaLock )
            {
                if ( lexiconSchemasLoaded )
                {
                    return;
                }

                var targetAssembly = assembly ?? Assembly.GetExecutingAssembly();

                void FetchSchemasFromResource ( string resourceName )
                {
                    using var resourceStream = targetAssembly.GetManifestResourceStream( resourceName );
                    if ( resourceStream == null )
                    {
                        return;
                    }

                    try
                    {
                        var schema = XmlSchema.Read( resourceStream, null );
                        if ( schema != null )
                        {
                            lexiconSchemas.Add( schema );
                        }
                    }
                    catch ( Exception e )
                    {
                        Logging.Warn( "Failed to initialize lexicon schema validation", e );
                    }
                }

                try
                {
                    FetchSchemasFromResource( "EddiSpeechService.Properties.pls.xsd" );
                    FetchSchemasFromResource( "EddiSpeechService.Properties.xml.xsd" );
                    lexiconSchemas.Compile();
                    lexiconSchemasLoaded = true;
                }
                catch ( ArgumentException ae )
                {
                    Logging.Warn( "Unable to load lexicon validation schema.", ae );
                }
                catch ( XmlSchemaException xmle )
                {
                    Logging.Warn( $"Problem with lexicon validation schema at {xmle.SourceUri}", xmle );
                }
            }
        }

        private static HashSet<string> GetLexicons ( VoiceDetails voice )
        {
            EnsureLexiconSchemasLoaded();
            var result = new HashSet<string>();

            // When multiple lexicons are referenced, their precedence goes from lower to higher with document order (https://www.w3.org/TR/2004/REC-speech-synthesis-20040907/#S3.1.4) 

            // Add lexicons from our installation directory
            result.UnionWith( GetLexiconsFromDirectory( new FileInfo( System.Reflection.Assembly.GetExecutingAssembly().Location ).DirectoryName + @"\lexicons" ) );

            // Add lexicons from our user configuration (allowing these to overwrite any prior lexeme values)
            result.UnionWith( GetLexiconsFromDirectory( Constants.DATA_DIR + @"\lexicons" ) );

            return result;

            HashSet<string> GetLexiconsFromDirectory ( string directory, bool createIfMissing = false )
            {
                // When multiple lexicons are referenced, their precedence goes from lower to higher with document order.
                // Precedence means that a token is first looked up in the lexicon with highest precedence.
                // Only if not found in that lexicon, the next lexicon is searched and so on until a first match or until all lexicons have been used for lookup. (https://www.w3.org/TR/2004/REC-speech-synthesis-20040907/#S3.1.4).

                if ( string.IsNullOrEmpty( directory ) || string.IsNullOrEmpty( voice.culturecode ) )
                { return []; }
                var dir = new DirectoryInfo(directory);
                if ( dir.Exists )
                {
                    // Find two letter language code lexicons (these will have lower precedence than any full language code lexicons)
                    foreach ( var file in dir.GetFiles( "*.pls", SearchOption.AllDirectories )
                                 .Where( f => $"{f.Name.ToLowerInvariant()}" == $"{voice.cultureTwoLetterISOLanguageName.ToLowerInvariant()}.pls" ) )
                    {
                        CheckAndAdd( file );
                    }
                    // Find full language code lexicons
                    foreach ( var file in dir.GetFiles( "*.pls", SearchOption.AllDirectories )
                                 .Where( f => $"{f.Name.ToLowerInvariant()}" == $"{voice.cultureIetfLanguageTag.ToLowerInvariant()}.pls" ) )
                    {
                        CheckAndAdd( file );
                    }
                }
                else if ( createIfMissing )
                {
                    dir.Create();
                }
                return result;
            }

            void CheckAndAdd ( FileInfo file )
            {
                if ( IsValidPLS( file.FullName ) )
                {
                    result.Add( file.FullName );
                }
                else
                {
                    TryQuarantineInvalidLexicon( file );
                }
            }
        }
        
        /// <summary>
        /// Check whether the file is valid .pls (.pls is an xml-based format)
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        internal static bool IsValidPLS ( string filename )
        {
            EnsureLexiconSchemasLoaded();
            const string PlsNamespace = "http://www.w3.org/2005/01/pronunciation-lexicon";
            try
            {
                var settings = new XmlReaderSettings
                {
                    ValidationType = ValidationType.Schema,
                    Schemas = lexiconSchemas,
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null,
                    ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings
                };

                settings.ValidationEventHandler += ( _, e ) =>
                {
                    if ( e.Severity is not ( XmlSeverityType.Warning or XmlSeverityType.Error ) ) { return; }

                    var ex = e.Exception;
                    var location = $" at line {ex.LineNumber}, position {ex.LinePosition}";

                    throw new XmlSchemaValidationException(
                        $"Schema validation {e.Severity.ToString().ToLowerInvariant()}{location}: {e.Message}",
                        ex );
                };

                XDocument xml;
                using ( var reader = XmlReader.Create( filename, settings ) )
                {
                    xml = XDocument.Load( reader, LoadOptions.SetLineInfo );
                }
                XNamespace pls = PlsNamespace;
                foreach ( var phoneme in xml.Descendants( pls + "phoneme" ) )
                {
                    var value = phoneme.Value.Trim();
                    if ( IPA.IsValid( value ) ) { continue; }

                    IXmlLineInfo lineInfo = phoneme;
                    var location = lineInfo.HasLineInfo()
                        ? $" at line {lineInfo.LineNumber}, position {lineInfo.LinePosition}"
                        : string.Empty;

                    throw new ArgumentException(
                        $"Invalid phoneme found in lexicon file{location}: {value}" );
                }

                return true;
            }
            catch ( Exception ex )
            {
                Logging.Warn( $"Could not load lexicon file '{filename}', please review.", ex );
                return false;
            }
        }

        internal static bool TryQuarantineInvalidLexicon ( FileInfo file )
        {
            lock ( lexiconQuarantineLock )
            {
                try
                {
                    var destination = $"{file.FullName}.malformed";
                    if ( File.Exists( destination ) )
                    {
                        File.Delete( destination );
                    }

                    file.MoveTo( destination );
                    return true;
                }
                catch ( Exception ex ) when ( ex is IOException or UnauthorizedAccessException )
                {
                    Logging.Warn( $"Unable to quarantine invalid lexicon file '{file.FullName}'.", ex );
                    return false;
                }
            }
        }

        #endregion
    }
}
