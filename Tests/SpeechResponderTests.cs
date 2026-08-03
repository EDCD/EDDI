using EddiSpeechResponder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utilities;
using Utilities.MetaVariables;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public partial class SpeechResponderTests : TestBase
    {
        [TestInitialize]
        public void Start ()
        {
            MakeSafe();
        }

        [TestMethod]
        [DataRow( "{set wordCount to wordCount + speech.", "speech" )]
        [DataRow( "{event.body.", "event.body" )]
        [DataRow( "{!event.body.", "event.body" )]
        [DataRow( "{GetPendingSpeech().", "GetPendingSpeech()" )]
        [DataRow( "{set value to GetPendingSpeech().", "GetPendingSpeech()" )]
        [DataRow( "{set value to speechQueue.", "speechQueue" )]
        [DataRow( "{set value to speechQueue.item.", "speechQueue.item" )]
        public void GetTextCompletionLookupItem_ReturnsExpectedLookupItem (
            string lineText,
            string expected )
        {
            var actual = TextCompletion.GetLookupItem( lineText );

            Assert.AreEqual( expected, actual );
        }

        [TestMethod]
        public void GetTextCompletionLookupItem_NormalizesNumericIndexer ()
        {
            var expected = $"bodies.{MetaVariables.indexMarker}";

            var actual = TextCompletion.GetLookupItem( "{bodies[0]." );

            Assert.AreEqual( expected, actual );
        }

        [TestMethod]
        public void GetTextCompletionLookupItem_NormalizesStringIndexer ()
        {
            var expected = $"mymap.{MetaVariables.indexMarker}";

            var actual = TextCompletion.GetLookupItem( "{mymap[\"f1\"]." );

            Assert.AreEqual( expected, actual );
        }

        [TestMethod]
        public void GetTextCompletionLookupItem_NormalizesSingleQuotedStringIndexer ()
        {
            var expected = $"mymap.{MetaVariables.indexMarker}";

            var actual = TextCompletion.GetLookupItem( "{mymap['f1']." );

            Assert.AreEqual( expected, actual );
        }

        [TestMethod]
        public void GetTextCompletionLookupItem_NormalizesVariableIndexer ()
        {
            var expected = $"mymap.{MetaVariables.indexMarker}";

            var actual = TextCompletion.GetLookupItem( "{mymap[key]." );

            Assert.AreEqual( expected, actual );
        }

        [TestMethod]
        public void GetTextCompletionLookupItem_NormalizesDottedVariableIndexer ()
        {
            var expected = $"mymap.{MetaVariables.indexMarker}";

            var actual = TextCompletion.GetLookupItem( "{mymap[event.name]." );

            Assert.AreEqual( expected, actual );
        }

        [TestMethod]
        public void GetTextCompletionLookupItem_NormalizesNestedPathAfterStringIndexer ()
        {
            var expected = $"mymap.{MetaVariables.indexMarker}.name";

            var actual = TextCompletion.GetLookupItem( "{mymap[\"f1\"].name." );

            Assert.AreEqual( expected, actual );
        }

        [TestMethod]
        public void GetTextCompletionLookupItem_ReturnsEmptyStringWhenNoCompletionContextExists ()
        {
            var actual = TextCompletion.GetLookupItem( "{set wordCount to wordCount + speech" );

            Assert.AreEqual( string.Empty, actual );
        }

        [TestMethod]
        public void ScriptPersonalityIsCustom_NotifiesResetOrDeleteEnabled ()
        {
            var script = new Script( "Test script", null, false, "Custom script", 3, "Default script" );
            var propertyNames = new List<string>();
            script.PropertyChanged += ( _, args ) => propertyNames.Add( args.PropertyName );

            Assert.IsFalse( script.IsResetOrDeleteEnabled );

            script.PersonalityIsCustom = true;

            Assert.IsTrue( script.IsResetOrDeleteEnabled );
            CollectionAssert.Contains( propertyNames, nameof( Script.PersonalityIsCustom ) );
            CollectionAssert.Contains( propertyNames, nameof( Script.IsResetOrDeleteEnabled ) );
        }

        [TestMethod]
        public void PersonalityToJson_WritesLegacyScriptShapeForRollbackCompatibility ()
        {
            var script = Personality.Default().Scripts[ "AFMU repairs" ].Copy();
            script.Enabled = false;
            script.Value = "Custom AFMU repairs";
            var personality = new Personality( "Custom", "Custom description", new Dictionary<string, Script>
            {
                [ script.Name ] = script
            } );

            var json = personality.ToJson();
            var file = JObject.Parse( json );
            var serializedScript = (JObject)file[ "scripts" ]?[ "AFMU repairs" ];

            Assert.IsNull( file[ "version" ] );
            Assert.IsFalse( serializedScript?.Value<bool>( "enabled" ) );
            Assert.AreEqual( "AFMU repairs", serializedScript?.Value<string>( "name" ) );
            Assert.IsTrue( serializedScript?.Value<bool>( "responder" ) );
            Assert.AreEqual( "Custom AFMU repairs", serializedScript?.Value<string>( "script" ) );
            Assert.IsNotNull( serializedScript?[ "defaultValue" ] );
        }

        [TestMethod]
        public void DefaultPersonality_IncludesEditableApproximateHelperScript ()
        {
            var script = Personality.Default().Scripts[ "Approximate" ];

            Assert.IsFalse( script.Responder );
            Assert.IsNull( script.Priority );
            Assert.IsTrue( script.Enabled );
            Assert.IsTrue( script.Default );
            Assert.IsTrue( script.IsResettable );
            Assert.IsFalse( string.IsNullOrWhiteSpace( script.Value ) );
            Assert.AreEqual( script.Value, script.defaultValue );
        }

        [TestMethod]
        public void ShippedDefaultPersonalities_IncludeEditableApproximateScaffold ()
        {
            var dirInfo = new DirectoryInfo( AppContext.BaseDirectory );
            foreach ( var fileInfo in dirInfo.GetFiles()
                         .Where( f => f.Name.StartsWith( "eddi" ) && f.Extension == ".json" ) )
            {
                var personality = Personality.FromFile( fileInfo.FullName, true );
                Assert.IsTrue(
                    personality.Scripts.TryGetValue( "Approximate", out var script ),
                    $"{fileInfo.Name} is missing the Approximate helper script." );

                Assert.IsFalse( script.Responder, $"{fileInfo.Name} Approximate should be a non-responder helper." );
                Assert.IsNull( script.Priority, $"{fileInfo.Name} Approximate should not have a responder priority." );
                Assert.IsTrue( script.Enabled, $"{fileInfo.Name} Approximate should be enabled." );
                Assert.IsTrue( script.IsResettable, $"{fileInfo.Name} Approximate should be resettable." );
                Assert.AreEqual( script.Value, script.defaultValue, $"{fileInfo.Name} Approximate should be resettable to its shipped script." );
                Assert.Contains( "NumberDetails(args.number)", script.Value, $"{fileInfo.Name} Approximate should decompose args.number." );
                Assert.Contains( "humanise.magnitudename", script.Value, $"{fileInfo.Name} Approximate should expose magnitude handling." );
                Assert.Contains( "humanise.isnegative", script.Value, $"{fileInfo.Name} Approximate should expose sign handling." );
                Assert.Contains( "humanise.format = \"short_decimal\"", script.Value, $"{fileInfo.Name} Approximate should expose decimal formatting." );
                Assert.Contains( "humanise.format = \"just_over\"", script.Value, $"{fileInfo.Name} Approximate should expose approximation formatting." );
                Assert.Contains( "humanise.format = \"around_half\"", script.Value, $"{fileInfo.Name} Approximate should expose half-step formatting." );
                Assert.Contains( "humanise.format = \"well_over_half\"", script.Value, $"{fileInfo.Name} Approximate should expose rounded half-step handling." );
                Assert.DoesNotContain( "fallback", script.Value, $"{fileInfo.Name} Approximate should not use legacy fallback output." );
            }
        }

        [TestMethod]
        public void ShippedDefaultPersonalities_DoNotUseDeprecatedFunctionNames ()
        {
            var deprecatedFunctionPattern = GeneratedRegex.DeprecatedCottleFunctionsRegex();
            var dirInfo = new DirectoryInfo( AppContext.BaseDirectory );

            foreach ( var fileInfo in dirInfo.GetFiles()
                         .Where( f => f.Name.StartsWith( "eddi" ) && f.Extension == ".json" ) )
            {
                var personality = Personality.FromFile( fileInfo.FullName, true );
                foreach ( var script in personality.Scripts.Values )
                {
                    Assert.IsFalse(
                        deprecatedFunctionPattern.IsMatch( script.Value ?? string.Empty ),
                        $"{fileInfo.Name} script '{script.Name}' should use preferred Cottle function aliases." );
                }
            }
        }

        [TestMethod]
        public void PersonalityFromFile_PreservesCustomizedObsoleteScriptAsRecoveryScript ()
        {
            var obsoleteScript = new Script(
                "Vehicle destroyed",
                "Old vehicle destroyed description",
                true,
                "Custom vehicle destroyed script",
                4,
                "Default vehicle destroyed script" )
            {
                Enabled = false,
                includes = ".Runtime"
            };

            var personality = LoadCustomPersonality( obsoleteScript );

            Assert.IsFalse( personality.Scripts.ContainsKey( "Vehicle destroyed" ) );
            Assert.IsTrue( personality.Scripts.TryGetValue( "(Obsolete) Vehicle destroyed", out var recoveredScript ) );
            Assert.AreEqual( "(Obsolete) Vehicle destroyed", recoveredScript.Name );
            Assert.AreEqual( "Old vehicle destroyed description", recoveredScript.Description );
            Assert.AreEqual( "Custom vehicle destroyed script", recoveredScript.Value );
            Assert.IsFalse( recoveredScript.Enabled );
            Assert.IsFalse( recoveredScript.Responder );
            Assert.IsNull( recoveredScript.defaultValue );
            Assert.AreEqual( ".Runtime", recoveredScript.includes );
        }

        [TestMethod]
        public void PersonalityFromFile_RemovesDefaultObsoleteScript ()
        {
            var obsoleteScript = new Script(
                "Vehicle destroyed",
                "Old vehicle destroyed description",
                true,
                "Default vehicle destroyed script",
                3,
                "Default vehicle destroyed script" );

            var personality = LoadCustomPersonality( obsoleteScript );

            Assert.IsFalse( personality.Scripts.ContainsKey( "Vehicle destroyed" ) );
            Assert.IsFalse( personality.Scripts.ContainsKey( "(Obsolete) Vehicle destroyed" ) );
        }

        [TestMethod]
        public void PersonalityFromFile_UsesExistingObsoleteRecoveryScriptWhenPresent ()
        {
            var obsoleteScript = new Script(
                "Vehicle destroyed",
                "Old vehicle destroyed description",
                true,
                "Custom vehicle destroyed script",
                4,
                "Default vehicle destroyed script" );
            var existingRecoveryScript = new Script(
                "(Obsolete) Vehicle destroyed",
                "Existing recovery description",
                false,
                "Existing recovery script",
                null );

            var personality = LoadCustomPersonality( obsoleteScript, existingRecoveryScript );

            Assert.IsFalse( personality.Scripts.ContainsKey( "Vehicle destroyed" ) );
            Assert.IsTrue( personality.Scripts.TryGetValue( "(Obsolete) Vehicle destroyed", out var recoveredScript ) );
            Assert.AreEqual( "Existing recovery description", recoveredScript.Description );
            Assert.AreEqual( "Existing recovery script", recoveredScript.Value );
            Assert.IsFalse( recoveredScript.Responder );
            Assert.IsNull( recoveredScript.defaultValue );
        }

        [TestMethod]
        public void PersonalityFromFile_DoesNotRenameDefaultLocalizedHelperScriptAsObsolete ()
        {
            var localizedHelperScript = new Script(
                ".Preferencias",
                "Localized helper",
                false,
                "Default localized helper",
                null,
                "Default localized helper" );

            var personality = LoadCustomPersonality( localizedHelperScript );

            Assert.IsFalse( personality.Scripts.ContainsKey( ".Preferencias" ) );
            Assert.IsFalse( personality.Scripts.ContainsKey( "(Obsolete) .Preferencias" ) );
        }

        [TestMethod]
        public void SplitLookupPath_SplitsSimplePath ()
        {
            var actual = TextCompletion.SplitLookupPath( "event.body.name" );

            CollectionAssert.AreEqual(
                new List<string> { "event", "body", "name" },
                actual );
        }

        [TestMethod]
        public void SplitLookupPath_NormalizesIndexer ()
        {
            var actual = TextCompletion.SplitLookupPath( "bodies[5].name" );

            CollectionAssert.AreEqual(
                new List<string> { "bodies", MetaVariables.indexMarker, "name" },
                actual );
        }

        [TestMethod]
        public void BuildCompletionAliases_ResolvesSimpleSetAlias ()
        {
            const string priorText = "{set speechQueue to GetPendingSpeech()}";

            var aliases = TextCompletion.BuildCompletionAliases( priorText );

            Assert.IsTrue( aliases.ContainsKey( "speechQueue" ) );
            Assert.AreEqual( "GetPendingSpeech()", aliases[ "speechQueue" ].Expression );
            Assert.IsFalse( aliases[ "speechQueue" ].IsEnumerationKey );
        }

        [TestMethod]
        public void BuildCompletionAliases_ResolvesForValueAlias ()
        {
            const string priorText = "{for speech in speechQueue:";

            var aliases = TextCompletion.BuildCompletionAliases( priorText );

            Assert.IsTrue( aliases.ContainsKey( "speech" ) );
            Assert.AreEqual(
                $"speechQueue.{MetaVariables.indexMarker}",
                aliases[ "speech" ].Expression );
            Assert.IsFalse( aliases[ "speech" ].IsEnumerationKey );
        }

        [TestMethod]
        public void BuildCompletionAliases_ResolvesForKeyAndValueAliases ()
        {
            const string priorText = "{for index, speech in speechQueue:";

            var aliases = TextCompletion.BuildCompletionAliases( priorText );

            Assert.IsTrue( aliases.ContainsKey( "index" ) );
            Assert.AreEqual( string.Empty, aliases[ "index" ].Expression );
            Assert.IsTrue( aliases[ "index" ].IsEnumerationKey );

            Assert.IsTrue( aliases.ContainsKey( "speech" ) );
            Assert.AreEqual(
                $"speechQueue.{MetaVariables.indexMarker}",
                aliases[ "speech" ].Expression );
            Assert.IsFalse( aliases[ "speech" ].IsEnumerationKey );
        }

        [TestMethod]
        public void ResolveLookupKeys_ResolvesSimpleSetAlias ()
        {
            const string priorText = "{set speechQueue to GetPendingSpeech()}";

            var actual = TextCompletion.ResolveLookupKeys( "speechQueue", priorText );

            CollectionAssert.AreEqual(
                new List<string> { "GetPendingSpeech()" },
                actual );
        }

        [TestMethod]
        public void ResolveLookupKeys_ResolvesForValueAlias ()
        {
            const string priorText = "{for speech in speechQueue:";

            var actual = TextCompletion.ResolveLookupKeys( "speech", priorText );

            CollectionAssert.AreEqual(
                new List<string> { "speechQueue", MetaVariables.indexMarker },
                actual );
        }

        [TestMethod]
        public void ResolveLookupKeys_ResolvesForValueAliasAndPreservesNestedMemberPath ()
        {
            const string priorText = "{for speech in speechQueue:";

            var actual = TextCompletion.ResolveLookupKeys( "speech.event", priorText );

            CollectionAssert.AreEqual(
                new List<string> { "speechQueue", MetaVariables.indexMarker, "event" },
                actual );
        }

        [TestMethod]
        public void ResolveLookupKeys_ResolvesSetAliasThenForAlias ()
        {
            var priorText = """
                {set speechQueue to GetPendingSpeech()}
                {for speech in speechQueue:
                """;

            var actual = TextCompletion.ResolveLookupKeys( "speech", priorText );

            CollectionAssert.AreEqual(
                new List<string> { "GetPendingSpeech()", MetaVariables.indexMarker },
                actual );
        }

        [TestMethod]
        public void ResolveLookupKeys_ResolvesSetAliasThenForAliasAndPreservesNestedMemberPath ()
        {
            var priorText = """
                {set speechQueue to GetPendingSpeech()}
                {for speech in speechQueue:
                """;

            var actual = TextCompletion.ResolveLookupKeys( "speech.name", priorText );

            CollectionAssert.AreEqual(
                new List<string> { "GetPendingSpeech()", MetaVariables.indexMarker, "name" },
                actual );
        }

        [TestMethod]
        public void ResolveLookupKeys_EnumerationKeyAliasReturnsEmptyList ()
        {
            const string priorText = "{for index, speech in speechQueue:";

            var actual = TextCompletion.ResolveLookupKeys( "index", priorText );

            Assert.HasCount( 0, actual );
        }

        [TestMethod]
        public void ResolveLookupKeys_LaterAliasOverridesEarlierAlias ()
        {
            var priorText = """
                {set speechQueue to OldSpeechQueue()}
                {set speechQueue to GetPendingSpeech()}
                """;

            var actual = TextCompletion.ResolveLookupKeys( "speechQueue", priorText );

            CollectionAssert.AreEqual(
                new List<string> { "GetPendingSpeech()" },
                actual );
        }

        [TestMethod]
        public void ResolveLookupKeys_FailsClosedOnAliasLoop ()
        {
            var priorText = """
                {set a to b}
                {set b to a}
                """;

            var actual = TextCompletion.ResolveLookupKeys( "a", priorText );

            Assert.HasCount( 0, actual );
        }

        [TestMethod]
        public void ResolveLookupKeys_LeavesUnknownLookupUnchanged ()
        {
            const string priorText = "{set speechQueue to GetPendingSpeech()}";

            var actual = TextCompletion.ResolveLookupKeys( "event.body", priorText );

            CollectionAssert.AreEqual(
                new List<string> { "event", "body" },
                actual );
        }

        [TestMethod]
        public void ResolveLookupKeys_HandlesScreenshotCase ()
        {
            var priorText = """
                {set speechQueue to GetPendingSpeech()}
                {set wordCount to 0}
                {for speech in speechQueue:
                    {set wordCount to wordCount + speech.
                """;

            var lookupItem = TextCompletion.GetLookupItem(
                "{set wordCount to wordCount + speech." );

            var actual = TextCompletion.ResolveLookupKeys( lookupItem, priorText );

            CollectionAssert.AreEqual(
                new List<string> { "GetPendingSpeech()", MetaVariables.indexMarker },
                actual );
        }

        [TestMethod]
        public void ResolveLookupKeys_HandlesCottleForKeyValueSyntax ()
        {
            var priorText = """
                {set speechQueue to GetPendingSpeech()}
                {for index, speech in speechQueue:
                    {speech.
                """;

            var actual = TextCompletion.ResolveLookupKeys( "speech", priorText );

            CollectionAssert.AreEqual(
                new List<string> { "GetPendingSpeech()", MetaVariables.indexMarker },
                actual );
        }

        private static Personality LoadCustomPersonality ( params Script[] scripts )
        {
            Personality.ResetDefault();
            var personality = new Personality(
                $"Custom {Guid.NewGuid():N}",
                "Custom description",
                new Dictionary<string, Script>() );
            foreach ( var script in scripts )
            {
                personality.Scripts[ script.Name ] = script;
            }

            var filePath = Path.Combine( Path.GetTempPath(), $"{Guid.NewGuid():N}.json" );
            try
            {
                File.WriteAllText( filePath, personality.ToJson() );
                return Personality.FromFile( filePath );
            }
            finally
            {
                Personality.ResetDefault();
                if ( File.Exists( filePath ) )
                {
                    File.Delete( filePath );
                }
            }
        }
    }
}
