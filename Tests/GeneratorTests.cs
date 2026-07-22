using DocumentationGenerator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public class GeneratorTests
    {
        [TestMethod]
        public void RenderWikiEvents_IsDeterministicAndIncludesRepresentativeVariables ()
        {
            var firstRender = DocumentationGenerator.DocumentationGenerator.RenderWikiEventPages();
            var secondRender = DocumentationGenerator.DocumentationGenerator.RenderWikiEventPages();

            CollectionAssert.AreEqual( firstRender.Keys.ToList(), secondRender.Keys.ToList() );
            foreach ( var key in firstRender.Keys )
            {
                Assert.AreEqual( firstRender[ key ], secondRender[ key ] );
            }

            var commodityEjectedPage = firstRender[ @"Wiki\events\Commodity-ejected-event.md" ];
            Assert.Contains( "{event.commodity}" , commodityEjectedPage);
            Assert.Contains( "{TXT:EDDI commodity ejected commodity}" , commodityEjectedPage);
            Assert.Contains( "The name of the commodity ejected" , commodityEjectedPage);
        }

        [TestMethod]
        public void RenderWikiEventsList_IsDeterministicAndLinksEvents ()
        {
            var firstRender = DocumentationGenerator.DocumentationGenerator.RenderWikiEventsList();
            var secondRender = DocumentationGenerator.DocumentationGenerator.RenderWikiEventsList();

            Assert.AreEqual( firstRender, secondRender );
            Assert.Contains( "## [Commodity ejected](Commodity-ejected-event)" , firstRender);
        }

        [TestMethod]
        public void RenderEventVariableKeywords_IsDeterministicAndUsesDescriptorInventory ()
        {
            var firstRender = DocumentationGenerator.DocumentationGenerator.RenderEventVariableKeywords();
            var secondRender = DocumentationGenerator.DocumentationGenerator.RenderEventVariableKeywords();

            Assert.AreEqual( firstRender, secondRender );
            Assert.Contains( "<Word>commodity</Word>" , firstRender);
            Assert.Contains( "<Word>" + Utilities.MetaVariables.indexMarker + "</Word>" , firstRender);
        }

        [TestMethod]
        public void RenderVariablesPage_IsDeterministicAndIncludesStandardRuntimeAndMonitorVariables ()
        {
            var firstRender = DocumentationGenerator.DocumentationGenerator.RenderVariablesPage();
            var secondRender = DocumentationGenerator.DocumentationGenerator.RenderVariablesPage();

            Assert.AreEqual( firstRender, secondRender );
            Assert.Contains( "## Root Variables" , firstRender);
            Assert.DoesNotContain( "## Top-level variables", firstRender );
            Assert.DoesNotContain( "## Standard variables", firstRender );
            Assert.Contains( "A variable can be a simple value, such as `environment`, or an object, such as `cmdr`." , firstRender);
            Assert.Contains( "The object reference documents each object shape once and lists the roots that use it" , firstRender);
            Assert.Contains( "*environment*" , firstRender);
            Assert.Contains( "The commander's current environment." , firstRender);
            Assert.Contains( "Used by: `cmdr`" , firstRender);
            Assert.Contains( "Used by: `inventory[\\<index\\>]`" , firstRender);
            Assert.Contains( "### StarSystem" , firstRender);
            Assert.Contains( "`system`" , firstRender);
            Assert.Contains( "`lastsystem`" , firstRender);
            Assert.Contains( "`searchsystem`" , firstRender);
            Assert.Contains( "*powerplant* - details of the ship's powerplant (this is a Module object) See: `Module`." , firstRender);
            Assert.Contains( "Used by: `Compartment.module`, `Hardpoint.module`, `Ship.bulkheads`" , firstRender);
            Assert.DoesNotContain( "*powerplant.class*", firstRender );
            Assert.Contains( "*name*" , firstRender);
            Assert.Contains( "*gui_focus*" , firstRender);
        }

        [TestMethod]
        public void RenderVoiceAttackIntegrationPage_ReplacesVariableInventoryFromRuntimeCatalog ()
        {
            const string template = """
                                    # Using EDDI with VoiceAttack

                                    Keep this setup prose.

                                    {{VoiceAttackVariables}}

                                    ## Legacy Standard Variables

                                    ## Commander Variables

                                      * {TXT:Name}: the name of the commander

                                    # Running Commands on EDDI Events

                                    Keep this command prose.
                                    """;

            var firstRender = DocumentationGenerator.DocumentationGenerator.RenderVoiceAttackIntegrationPage( template );
            var secondRender = DocumentationGenerator.DocumentationGenerator.RenderVoiceAttackIntegrationPage( template );

            Assert.AreEqual( firstRender, secondRender );
            Assert.Contains( "Keep this setup prose.", firstRender );
            Assert.Contains( "Keep this command prose.", firstRender );
            Assert.DoesNotContain( "{{VoiceAttackVariables}}", firstRender );
            Assert.Contains( "{TXT:Environment}", firstRender );
            Assert.Contains( "The commander's current environment.", firstRender );
            Assert.Contains( "{BOOL:cAPI active}", firstRender );
            Assert.Contains( "## Legacy Standard Variables", firstRender );
            Assert.Contains( "{TXT:Name}: the name of the commander", firstRender );
        }

        [TestMethod]
        public void RenderVoiceAttackIntegrationPage_GeneratedVariableInventoryIsStableAndOrdered ()
        {
            var render = DocumentationGenerator.DocumentationGenerator.RenderVoiceAttackIntegrationPage();

            CollectionAssert.AreEqual(
                new List<string>
                {
                    "cAPI active",
                    "Destination system distance",
                    "EDDI version",
                    "Environment",
                    "horizons",
                    "icao active",
                    "ipa active",
                    "odyssey",
                    "Search system distance",
                    "Vehicle"
                },
                ExtractVoiceAttackVariableKeys(
                    render,
                    "## Generated Standard Variables",
                    "## Legacy Standard Variables" ) );

            Assert.IsTrue(
                render.IndexOf( "## Generated Standard Variables", StringComparison.Ordinal ) <
                render.IndexOf( "## Legacy Standard Variables", StringComparison.Ordinal ) );
            Assert.IsTrue(
                render.IndexOf( "## Legacy Standard Variables", StringComparison.Ordinal ) <
                render.IndexOf( "# Running Commands on EDDI Events", StringComparison.Ordinal ) );
        }

        [TestMethod]
        public void RenderVoiceAttackIntegrationPage_DoesNotDuplicateMigratedLegacyVariables ()
        {
            var render = DocumentationGenerator.DocumentationGenerator.RenderVoiceAttackIntegrationPage();
            var generatedKeys = ExtractVoiceAttackVariableKeys(
                render,
                "## Generated Standard Variables",
                "## Legacy Standard Variables" );
            var legacyKeys = ExtractVoiceAttackVariableKeys(
                render,
                "## Legacy Standard Variables",
                "# Running Commands on EDDI Events" );

            foreach ( var generatedKey in generatedKeys )
            {
                Assert.IsFalse(
                    legacyKeys.Contains( generatedKey, StringComparer.Ordinal ),
                    $"Generated VoiceAttack variable remains in the legacy section: {generatedKey}" );
            }

            Assert.Contains( "{TXT:Name}: the name of the commander", render );
            Assert.Contains( "{TXT:EDDI uri}", render );
            Assert.DoesNotContain( "  * {TXT:Environment}:", render );
            Assert.DoesNotContain( "  * {BOOL:cAPI active}:", render );
        }

        [TestMethod]
        public void RenderVoiceAttackIntegrationPage_RequiresVariablePlaceholder ()
        {
            const string template = """
                                    # Using EDDI with VoiceAttack

                                    # Running Commands on EDDI Events
                                    """;

            Assert.ThrowsExactly<InvalidOperationException>(
                () => DocumentationGenerator.DocumentationGenerator.RenderVoiceAttackIntegrationPage( template ) );
        }

        [TestMethod]
        public void RenderVoiceAttackIntegrationPage_LoadsDocumentationGeneratorTemplate ()
        {
            var render = DocumentationGenerator.DocumentationGenerator.RenderVoiceAttackIntegrationPage();

            Assert.Contains( "# Using EDDI with VoiceAttack", render );
            Assert.Contains( "{TXT:Environment}", render );
            Assert.Contains( "## Legacy Standard Variables", render );
            Assert.Contains( "## Commander Variables", render );
            Assert.Contains( "# Running Commands on EDDI Events", render );
        }

        [TestMethod]
        public void RenderFunctionsHelp_IsDeterministicAndIncludesFunctionIndex ()
        {
            var (Help, Functions) = DocumentationGenerator.DocumentationGenerator.RenderFunctionsHelp();
            var (Help2, Functions2) = DocumentationGenerator.DocumentationGenerator.RenderFunctionsHelp();

            Assert.AreEqual( Help, Help2 );
            Assert.AreEqual( Functions, Functions2 );
            Assert.Contains( "* " , Functions);
        }

        [TestMethod]
        public void WriteWikiOutput_RemovesObsoleteRootMarkdownOutputs ()
        {
            var outputDirectory = Path.Combine( Path.GetTempPath(), "eddi-docgen-test-" + Guid.NewGuid().ToString( "N" ) );
            Directory.CreateDirectory( outputDirectory );
            try
            {
                File.WriteAllText( Path.Combine( outputDirectory, "Variables.md" ), "stale variables" );
                File.WriteAllText( Path.Combine( outputDirectory, "Help.md" ), "stale help" );

                DocumentationGenerator.DocumentationGenerator.WriteWikiOutput( outputDirectory );

                Assert.IsFalse( File.Exists( Path.Combine( outputDirectory, "Variables.md" ) ) );
                Assert.IsFalse( File.Exists( Path.Combine( outputDirectory, "Help.md" ) ) );
                Assert.IsTrue( File.Exists( Path.Combine( outputDirectory, "Wiki", "Variables.md" ) ) );
                Assert.IsTrue( File.Exists( Path.Combine( outputDirectory, "Wiki", "Help.md" ) ) );
                Assert.IsTrue( File.Exists( Path.Combine( outputDirectory, "Wiki", "VoiceAttack-Integration.md" ) ) );
                Assert.IsTrue( File.Exists( Path.Combine( outputDirectory, "Wiki", "Functions.md" ) ) );
                Assert.IsTrue( Directory.EnumerateFiles( Path.Combine( outputDirectory, "Wiki", "events" ), "*.md" ).Any() );

                var variables = File.ReadAllText( Path.Combine( outputDirectory, "Wiki", "Variables.md" ) );
                var voiceAttackIntegration = File.ReadAllText( Path.Combine( outputDirectory, "Wiki", "VoiceAttack-Integration.md" ) );

                Assert.Contains( "## Root Variables", variables );
                Assert.DoesNotContain( "{{", variables );
                Assert.DoesNotContain( "{{", voiceAttackIntegration );
            }
            finally
            {
                if ( Directory.Exists( outputDirectory ) )
                {
                    Directory.Delete( outputDirectory, true );
                }
            }
        }

        private static List<string> ExtractVoiceAttackVariableKeys (
            string render,
            string startHeading,
            string endHeading )
        {
            var lines = render.Split( [ "\r\n", "\n" ], StringSplitOptions.None );
            var start = Array.FindIndex( lines, line => line.Trim() == startHeading );
            var end = Array.FindIndex( lines, start + 1, line => line.Trim() == endHeading );

            Assert.IsTrue( start >= 0, $"Could not find heading '{startHeading}'." );
            Assert.IsTrue( end > start, $"Could not find heading '{endHeading}' after '{startHeading}'." );

            return lines
                .Skip( start + 1 )
                .Take( end - start - 1 )
                .Select( line => Regex.Match( line, @"\{(?:TXT|INT|DEC|BOOL|DATE):(?<key>[^}\r\n]+)\}" ) )
                .Where( match => match.Success )
                .Select( match => match.Groups[ "key" ].Value )
                .ToList();
        }
    }
}
