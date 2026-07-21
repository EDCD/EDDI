using DocumentationGenerator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

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

                                    # Running Commands on EDDI Events

                                    Keep this command prose.
                                    """;
            const string legacyVariablesTemplate = """
                                                   ## Commander Variables

                                                     * {TXT:Name}: the name of the commander
                                                   """;

            var firstRender = DocumentationGenerator.DocumentationGenerator.RenderVoiceAttackIntegrationPage( template, legacyVariablesTemplate );
            var secondRender = DocumentationGenerator.DocumentationGenerator.RenderVoiceAttackIntegrationPage( template, legacyVariablesTemplate );

            Assert.AreEqual( firstRender, secondRender );
            Assert.Contains( "Keep this setup prose.", firstRender );
            Assert.Contains( "Keep this command prose.", firstRender );
            Assert.DoesNotContain( "{{VoiceAttackVariables}}", firstRender );
            Assert.Contains( "{TXT:Environment}", firstRender );
            Assert.Contains( "The commander's current environment.", firstRender );
            Assert.Contains( "{BOOL:cAPI active}", firstRender );
            Assert.Contains( "## Legacy Standard Variables", firstRender );
            Assert.Contains( "{TXT:Name}: the name of the commander", firstRender );
            Assert.Contains( "## Event Variables", firstRender );
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
    }
}
