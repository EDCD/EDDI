using EddiSpeechResponder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Utilities;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public class SpeechResponderTests : TestBase
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
    }
}
