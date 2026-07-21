using EddiDataDefinitions;
using EddiEvents;
using EddiVoiceAttackResponder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace Tests
{
    [TestClass, TestCategory("UnitTests")]
    public class MetaVariablesTests : TestBase
    {
        private sealed class TestSpeech
        {
            [PublicAPI( "The speech text." )]
            public string Text { get; set; }

            [PublicAPI( "Speech priority." )]
            public int Priority { get; set; }
        }

        [PublicAPI]
        private sealed class TypeLevelPublicApiOnly
        {
            public string Hidden { get; set; }
        }

        private sealed class MissingDescription
        {
            [PublicAPI]
            public string Text { get; set; }
        }

        private sealed class ObsoleteVariable
        {
            [PublicAPI( "Old text." ), Obsolete( "Use NewText instead." )]
            public string OldText { get; set; }
        }

        private sealed class UnsupportedVariableType
        {
            [PublicAPI( "Unsupported tuple." )]
            public Tuple<string, string> TupleValue { get; set; }
        }

        private sealed class BodyTypeVariable
        {
            [PublicAPI( "The body type." )]
            public BodyType BodyType { get; set; }
        }

        private sealed class RuntimeVariableProvider
        {
            [PublicAPI( "A runtime test variable." )]
            public static RuntimeVariableDefinition TestRuntimeVariable => new( "runtime", typeof(string), () => "value" );
        }

        private sealed class RuntimeVariableProviderMissingDescription
        {
            [PublicAPI]
            public static RuntimeVariableDefinition TestRuntimeVariable => new( "runtime", typeof(string), () => "value" );
        }
        
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        [TestMethod]
        public void TestGalnetNewsPublishedEvent()
        {
            var entry = new KeyValuePair<string, Type>("Galnet news published", typeof(GalnetNewsPublishedEvent));
            var vars = new MetaVariables(entry.Value, null).Results;

            var cottleVars = vars.AsCottleVariables();
            Assert.HasCount( 7, cottleVars);
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"items"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"items[\<index\>].category"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"items[\<index\>].content"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"items[\<index\>].id"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"items[\<index\>].published"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"items[\<index\>].read"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"items[\<index\>].title"));
            Assert.IsNotNull(cottleVars.TrueForAll(v => v.value == null));

            var vaVars = VoiceAttackVariables.Convert(vars,"EDDI", entry.Key);
            Assert.HasCount( 7, vaVars );
            var category = vaVars.FirstOrDefault( k => k.key == @"EDDI galnet news published items \<index\> category" );
            Assert.IsNotNull( category );
            Assert.AreEqual(typeof(string), category.variableType);
            var content = vaVars.FirstOrDefault( k => k.key == @"EDDI galnet news published items \<index\> content" );
            Assert.IsNotNull( content );
            Assert.AreEqual(typeof(string), content.variableType);
            var id = vaVars.FirstOrDefault( k => k.key == @"EDDI galnet news published items \<index\> id" );
            Assert.IsNotNull( id );
            Assert.AreEqual(typeof(string), id.variableType);
            var publishedDate = vaVars.FirstOrDefault( k => k.key == @"EDDI galnet news published items \<index\> published" );
            Assert.IsNotNull( publishedDate );
            Assert.AreEqual(typeof(DateTime), publishedDate.variableType);
            var read = vaVars.FirstOrDefault( k => k.key == @"EDDI galnet news published items \<index\> read" );
            Assert.IsNotNull( read );
            Assert.AreEqual(typeof(bool), read.variableType);
            var title = vaVars.FirstOrDefault( k => k.key == @"EDDI galnet news published items \<index\> title" );
            Assert.IsNotNull( title );
            Assert.AreEqual(typeof(string), title.variableType);
            var items = vaVars.FirstOrDefault( k => k.key == @"EDDI galnet news published items" );
            Assert.IsNotNull( items );
            Assert.AreEqual(typeof( int ), items.variableType);
            Assert.IsTrue(vaVars.TrueForAll(v => v.value == null));
        }

        [TestMethod]
        public void TestSRVTurretDeployableEvent()
        {
            var entry = new KeyValuePair<string, Type>("SRV turret deployable", typeof(SRVTurretDeployableEvent));
            var vars = new MetaVariables(entry.Value, null).Results;

            var cottleVars = vars.AsCottleVariables();
            Assert.HasCount( 1, cottleVars );
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"deployable")?.key);
            Assert.IsTrue(cottleVars.TrueForAll(v => v.value == null));

            var vaVars = VoiceAttackVariables.Convert(vars,"EDDI", entry.Key);
            Assert.HasCount( 1, vaVars );
            var var = vaVars.FirstOrDefault( k => k.key == @"EDDI srv turret deployable" );
            Assert.IsNotNull(var);
            Assert.AreEqual(typeof(bool), var.variableType);
            Assert.IsTrue(vaVars.TrueForAll(v => v.value == null));
        }

        [TestMethod]
        public void TestExplorationDataSoldEvent()
        {
            var entry = new KeyValuePair<string, Type>("Exploration data sold", typeof(ExplorationDataSoldEvent));
            var vars = new MetaVariables(entry.Value, null).Results;

            var cottleVars = vars.AsCottleVariables();
            Assert.HasCount( 5, cottleVars );
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"systems"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"systems[\<index\>]"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "reward"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "bonus"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "total"));

            var vaVars = VoiceAttackVariables.Convert(vars,"EDDI", entry.Key);
            Assert.HasCount( 5, vaVars );
            var index = vaVars.FirstOrDefault( k => k.key == "EDDI exploration data sold systems \\<index\\>" );
            Assert.IsNotNull( index );
            Assert.AreEqual(typeof(string), index.variableType);
            var systems = vaVars.FirstOrDefault( k => k.key == "EDDI exploration data sold systems" );
            Assert.IsNotNull( systems );
            Assert.AreEqual(typeof( int ), systems.variableType);
            var reward = vaVars.FirstOrDefault( k => k.key == "EDDI exploration data sold reward" );
            Assert.IsNotNull( reward );
            Assert.AreEqual(typeof(decimal), reward.variableType);
            var bonus = vaVars.FirstOrDefault( k => k.key == "EDDI exploration data sold bonus" );
            Assert.IsNotNull( bonus );
            Assert.AreEqual(typeof(decimal), bonus.variableType);
            var total = vaVars.FirstOrDefault( k => k.key == "EDDI exploration data sold total" );
            Assert.IsNotNull( total );
            Assert.AreEqual(typeof(decimal), total.variableType);
        }

        [TestMethod]
        public void TestDiscoveryScanEvent()
        {
            var entry = new KeyValuePair<string, Type>("Discovery scan", typeof(DiscoveryScanEvent));
            var vars = new MetaVariables(entry.Value, null).Results;

            var cottleVars = vars.AsCottleVariables();
            Assert.HasCount( 2, cottleVars );
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "totalbodies"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "nonbodies"));
            Assert.IsNull(cottleVars.FirstOrDefault(k => k.key == "progress"));
            Assert.IsTrue(cottleVars.TrueForAll(v => v.value == null));

            var vaVars = VoiceAttackVariables.Convert(vars,"EDDI", entry.Key);
            Assert.HasCount( 2, vaVars );
            var totalbodies = vaVars.FirstOrDefault( k => k.key == "EDDI discovery scan totalbodies" );
            Assert.IsNotNull( totalbodies );
            Assert.AreEqual(typeof(int), totalbodies.variableType);
            var nonbodies = vaVars.FirstOrDefault( k => k.key == "EDDI discovery scan nonbodies" );
            Assert.IsNotNull( nonbodies );
            Assert.AreEqual(typeof(int), nonbodies.variableType);
            Assert.IsNull(vaVars.FirstOrDefault(k => k.key == "EDDI discovery scan progress")?.variableType);
        }

        [TestMethod]
        public void TestAsteroidProspectedEvent()
        {
            var entry = new KeyValuePair<string, Type>("Asteroid prospected", typeof(AsteroidProspectedEvent));
            var vars = new MetaVariables(entry.Value, null).Results;

            var cottleVars = vars.AsCottleVariables();
            Assert.HasCount( 6, cottleVars );
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "commodities"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "commodities[\\<index\\>].commodity"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "commodities[\\<index\\>].percentage"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "materialcontent"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "remaining"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "motherlode"));

            var vaVars = VoiceAttackVariables.Convert(vars,"EDDI", entry.Key);
            Assert.HasCount( 6, vaVars );
            var commodity = vaVars.FirstOrDefault( k => k.key == "EDDI asteroid prospected commodities \\<index\\> commodity" );
            Assert.IsNotNull( commodity );
            Assert.AreEqual(typeof(string), commodity.variableType);
            var percentage = vaVars.FirstOrDefault( k => k.key == "EDDI asteroid prospected commodities \\<index\\> percentage" );
            Assert.IsNotNull( percentage );
            Assert.AreEqual(typeof(decimal), percentage.variableType);
            var commodities = vaVars.FirstOrDefault( k => k.key == "EDDI asteroid prospected commodities" );
            Assert.IsNotNull( commodities );
            Assert.AreEqual(typeof(int), commodities.variableType);
            var materialcontent = vaVars.FirstOrDefault( k => k.key == "EDDI asteroid prospected materialcontent" );
            Assert.IsNotNull( materialcontent );
            Assert.AreEqual(typeof(string), materialcontent.variableType);
            var remaining = vaVars.FirstOrDefault( k => k.key == "EDDI asteroid prospected remaining" );
            Assert.IsNotNull( remaining );
            Assert.AreEqual(typeof(decimal), remaining.variableType);
            var motherlode = vaVars.FirstOrDefault( k => k.key == "EDDI asteroid prospected motherlode" );
            Assert.IsNotNull( motherlode );
            Assert.AreEqual(typeof(string), motherlode.variableType);
        }

        [TestMethod]
        public void TestCommodityEjectedEvent()
        {
            var entry = new KeyValuePair<string, Type>("Commodity ejected", typeof(CommodityEjectedEvent));
            var vars = new MetaVariables(entry.Value, null).Results;

            Assert.HasCount( 4, vars );
            Assert.IsNotNull(vars.FirstOrDefault(k => k.keysPath.Last() == "commodity")?.description);
            Assert.IsNotNull(vars.FirstOrDefault(k => k.keysPath.Last() == "amount")?.description);
            Assert.IsNotNull(vars.FirstOrDefault(k => k.keysPath.Last() == "missionid")?.description);
            Assert.IsNotNull(vars.FirstOrDefault(k => k.keysPath.Last() == "abandoned")?.description);

            var cottleVars = vars.AsCottleVariables();
            Assert.HasCount( 4, cottleVars );
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "commodity"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "amount"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "missionid"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "abandoned"));

            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "commodity")?.description);
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "amount")?.description);
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "missionid")?.description);
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "abandoned")?.description);

            var vaVars =VoiceAttackVariables.Convert(vars,"EDDI", entry.Key);
            Assert.HasCount( 4, vaVars );
            var commodity = vaVars.FirstOrDefault( k => k.key == "EDDI commodity ejected commodity" );
            Assert.IsNotNull( commodity );
            Assert.AreEqual(typeof(string), commodity.variableType);
            var amount = vaVars.FirstOrDefault( k => k.key == "EDDI commodity ejected amount" );
            Assert.IsNotNull( amount );
            Assert.AreEqual(typeof(int), amount.variableType);
            var missionid = vaVars.FirstOrDefault( k => k.key == "EDDI commodity ejected missionid" );
            Assert.IsNotNull( missionid );
            Assert.AreEqual(typeof(decimal), missionid.variableType);
            var abandoned = vaVars.FirstOrDefault( k => k.key == "EDDI commodity ejected abandoned" );
            Assert.IsNotNull( abandoned );
            Assert.AreEqual(typeof(bool), abandoned.variableType);

            var ejectedcommodity = vaVars.FirstOrDefault( k => k.key == "EDDI commodity ejected commodity" );
            Assert.IsNotNull( ejectedcommodity );
            Assert.AreEqual("The name of the commodity ejected", ejectedcommodity.description);
            var ejectedamount = vaVars.FirstOrDefault( k => k.key == "EDDI commodity ejected amount" );
            Assert.IsNotNull( ejectedamount );
            Assert.AreEqual("The amount of commodity ejected", ejectedamount.description);
            var ejectedmissionid = vaVars.FirstOrDefault( k => k.key == "EDDI commodity ejected missionid" );
            Assert.IsNotNull( ejectedmissionid );
            Assert.AreEqual("ID of the mission-related commodity, if applicable", ejectedmissionid.description);
            var ejectedabandoned = vaVars.FirstOrDefault( k => k.key == "EDDI commodity ejected abandoned" );
            Assert.IsNotNull( ejectedabandoned );
            Assert.AreEqual("True if the cargo has been abandoned", ejectedabandoned.description);
        }

        [ TestMethod ]
        public void TestRouteDetailsEvent ()
        {
            var entry = new KeyValuePair<string, Type>( "Route details", typeof(RouteDetailsEvent) );
            var vars = new MetaVariables( entry.Value, new RouteDetailsEvent(DateTime.MinValue, "set", "Shinrarta Dezhra", 3932277478106U, "Jameson Memorial", 128666762, new NavWaypointCollection(), 0, null ) ).Results;
            var vaVars = VoiceAttackVariables.Convert(vars,string.Empty, entry.Key );
            vaVars.ForEach( v => v.Set() ); // This test is primarily to check that no exceptions are thrown when setting variables.
        }

#pragma warning disable MSTEST0037 // The current Assert pattern is is best available for these tests.

        [TestMethod]
        public void MetaVariables_ReturnsElementMembersForRootListType ()
        {
            var results = new MetaVariables(
                typeof( List<TestSpeech> ),
                null ).Results;

            Assert.IsTrue( results.Any( v =>
                v.keysPath.SequenceEqual( [
                    MetaVariables.indexMarker,
                    nameof( TestSpeech.Text )
                ] ) ) );

            Assert.IsTrue( results.Any( v =>
                v.keysPath.SequenceEqual( [
                    MetaVariables.indexMarker,
                    nameof( TestSpeech.Text )
                ] ) ) );

            Assert.IsTrue( results.Any( v =>
                v.keysPath.SequenceEqual( [
                    MetaVariables.indexMarker,
                    nameof( TestSpeech.Priority )
                ] ) ) );
        }

        [TestMethod]
        public void MetaVariables_ReturnsElementMembersForRootArrayType ()
        {
            var results = new MetaVariables(
                typeof( TestSpeech[] ),
                null ).Results;

            Assert.IsTrue( results.Any( v =>
                v.keysPath.SequenceEqual( [
                    MetaVariables.indexMarker,
                    nameof( TestSpeech.Text )
                ] ) ) );
        }

        [TestMethod]
        public void MetaVariables_ReturnsElementMembersForRootEnumerableInterfaceType ()
        {
            var results = new MetaVariables(
                typeof( IEnumerable<TestSpeech> ),
                null ).Results;

            Assert.IsTrue( results.Any( v =>
                v.keysPath.SequenceEqual( [
                    MetaVariables.indexMarker,
                    nameof( TestSpeech.Text )
                ] ) ) );
        }

        [TestMethod]
        public void MetaVariables_ReturnsIndexedElementMembersForRootListInstance ()
        {
            var speech = new List<TestSpeech>
            {
                new() { Text = "one", Priority = 1 },
                new() { Text = "two", Priority = 2 }
            };

            var results = new MetaVariables(
                typeof( List<TestSpeech> ),
                speech ).Results;

            Assert.IsTrue( results.Any( v =>
                v.keysPath.SequenceEqual( [
                    "1",
                    nameof( TestSpeech.Text )
                ] ) &&
                Equals( v.value, "one" ) ) );

            Assert.IsTrue( results.Any( v =>
                v.keysPath.SequenceEqual( [
                    "2",
                    nameof( TestSpeech.Priority )
                ] ) &&
                Equals( v.value, 2 ) ) );
        }

        [TestMethod]
        public void MetaVariables_DescriptorsExposeRenderedPathsWithoutChangingCompatibilityResults ()
        {
            var entry = new KeyValuePair<string, Type>( "Commodity ejected", typeof( CommodityEjectedEvent ) );
            var metaVariables = new MetaVariables( entry.Value, null );

            var descriptor = metaVariables.Descriptors.First( d => d.KeysPath[  d.KeysPath.Count  -  1  ] == "commodity" );

            Assert.AreEqual( "commodity", descriptor.CottlePath );
            Assert.AreEqual( "EDDI commodity ejected commodity", descriptor.RenderVoiceAttackName( "EDDI", entry.Key ) );
            Assert.AreEqual( "TXT", descriptor.VoiceAttackTypeName );
            Assert.AreEqual( typeof( string ), descriptor.VariableType );
            Assert.AreEqual( "The name of the commodity ejected", descriptor.Description );
            CollectionAssert.AreEqual(
                descriptor.KeysPath.ToList(),
                metaVariables.Results.First( v => v.keysPath.Last() == "commodity" ).keysPath );
        }

        [TestMethod]
        public void CottleVariable_DoesNotMutateInputPath ()
        {
            var path = new List<string> { "items", MetaVariables.indexMarker, "name" };

            _ = new CottleVariable( path, "Item name.", null );

            CollectionAssert.AreEqual(
                new List<string> { "items", MetaVariables.indexMarker, "name" },
                path );
        }

        [TestMethod]
        public void MetaVariables_StrictDocumentation_FailsForMissingMemberDescriptions ()
        {
            Assert.ThrowsExactly<MetaVariableDiscoveryException>( () =>
                _ = new MetaVariables(
                    typeof( MissingDescription ),
                    null,
                    null,
                    MetaVariableDiscoveryOptions.StrictDocumentation ) );
        }

        [TestMethod]
        public void MetaVariables_StrictDocumentation_HonorsMissingDescriptionAllowlist ()
        {
            var options = new MetaVariableDiscoveryOptions
            {
                Strict = true,
                RequireDescriptions = true,
                MissingDescriptionAllowlist = new HashSet<string>
                {
                    $"{typeof( MissingDescription ).FullName}.{nameof( MissingDescription.Text )}"
                }
            };

            var metaVariables = new MetaVariables( typeof( MissingDescription ), null, null, options );

            Assert.HasCount( 1, metaVariables.Results );
        }

        [TestMethod]
        public void MetaVariables_TypeLevelPublicApi_IsNotVariableDocumentationInput ()
        {
            var metaVariables = new MetaVariables(
                typeof( TypeLevelPublicApiOnly ),
                null,
                null,
                MetaVariableDiscoveryOptions.StrictDocumentation );

            Assert.HasCount( 0, metaVariables.Results );
        }

        [TestMethod]
        public void MetaVariables_StrictDocumentation_FailsForUnsupportedTypes ()
        {
            Assert.ThrowsExactly<MetaVariableDiscoveryException>( () =>
                _ = new MetaVariables(
                    typeof( UnsupportedVariableType ),
                    new UnsupportedVariableType(),
                    null,
                    MetaVariableDiscoveryOptions.StrictDocumentation ) );
        }

        [TestMethod]
        public void MetaVariables_DescriptorsExposeObsoleteMetadata ()
        {
            var descriptor = new MetaVariables( typeof( ObsoleteVariable ), null )
                .Descriptors
                .Single();

            Assert.IsTrue( descriptor.IsObsolete );
            Assert.AreEqual( "Use NewText instead.", descriptor.ObsoleteMessage );
        }

        [TestMethod]
        public void MetaVariables_DescriptorsIncludeSmallLocalizedEdNameValueSets ()
        {
            var descriptor = new MetaVariables( typeof( BodyTypeVariable ), null )
                .Descriptors
                .Single( d => d.KeysPath.SequenceEqual( [ nameof( BodyTypeVariable.BodyType ) ] ) );

            Assert.IsTrue( descriptor.AllowedValues.Any( v => v.InvariantName == "Planet" ) );
            Assert.IsTrue( descriptor.AllowedValues.Any( v => v.InvariantName == "Star" ) );
            Assert.IsNull( descriptor.AllowedValuesOmittedReason );
        }

        [TestMethod]
        public void VariableDescriptor_OmitsLargeLocalizedEdNameValueSetsByPolicy ()
        {
            var descriptor = VariableDescriptor.Create(
                [ "commodity" ],
                typeof( CommodityDefinition ),
                "Commodity.",
                options: new MetaVariableDiscoveryOptions { MaxInlineAllowedValues = 2 } );

            Assert.HasCount( 0, descriptor.AllowedValues );
            Assert.IsTrue( descriptor.AllowedValuesOmittedReason?.Contains( "omitted" ) );
        }

        [TestMethod]
        public void MetaVariables_DescriptorsCanBeBuiltFromRuntimeVariableDeclarations ()
        {
            var declarations = RuntimeVariableDefinitionExtensions.DiscoverDeclarations(
                typeof( RuntimeVariableProvider ) );
            var metaVariables = new MetaVariables( declarations, options: MetaVariableDiscoveryOptions.StrictDocumentation );

            var descriptor = metaVariables.Descriptors.Single();
            Assert.AreEqual( "runtime", descriptor.CottlePath );
            Assert.AreEqual( typeof(string), descriptor.VariableType );
            Assert.AreEqual( "A runtime test variable.", descriptor.Description );
        }

        [TestMethod]
        public void MetaVariables_DescriptorsCanBeBuiltFromTopLevelRuntimeVariableCatalog ()
        {
            var declarations = RuntimeVariableDefinitionExtensions.DiscoverDeclarations(
                typeof( EddiCore.RuntimeVariables.RuntimeVariableCatalog ) );
            var metaVariables = new MetaVariables( declarations, options: MetaVariableDiscoveryOptions.StrictDocumentation );

            Assert.IsTrue( metaVariables.Descriptors.Any( d => d.CottlePath == "environment" ) );
            Assert.IsTrue( metaVariables.Descriptors.Any( d => d.CottlePath == "destinationdistance" ) );
            Assert.IsTrue( metaVariables.Descriptors.All( d => !string.IsNullOrWhiteSpace( d.Description ) ) );
        }

        [TestMethod]
        public void StandardVariableInventoryBuilder_BuildsMetaVariablesFromClrRoots ()
        {
            var roots = new[]
            {
                new EddiCore.RuntimeVariables.RuntimeVariableRoot( "speech", typeof(TestSpeech) )
            };

            var metaVariables = EddiCore.RuntimeVariables.StandardVariableInventoryBuilder.BuildStandardMetaVariables(
                roots,
                [],
                MetaVariableDiscoveryOptions.StrictDocumentation );

            Assert.IsTrue( metaVariables.Any( v => v.Descriptor.CottlePath == "environment" ) );
            Assert.IsTrue( metaVariables.Any( v => v.Descriptor.CottlePath == "system" ) );
            Assert.IsTrue( metaVariables.Any( v => v.Descriptor.CottlePath == "speech.Text" ) );
            Assert.IsTrue( metaVariables.Any( v => v.Descriptor.CottlePath == "speech.Priority" ) );
        }

        [TestMethod]
        public void MetaVariables_StrictDocumentation_FailsForRuntimeVariablesMissingDescriptions ()
        {
            var declarations = RuntimeVariableDefinitionExtensions.DiscoverDeclarations(
                typeof( RuntimeVariableProviderMissingDescription ) );

            Assert.ThrowsExactly<MetaVariableDiscoveryException>( () =>
                _ = new MetaVariables( declarations, options: MetaVariableDiscoveryOptions.StrictDocumentation ) );
        }

#pragma warning restore MSTEST0037

    }
}
