using EddiConfigService;
using EddiConfigService.Configurations;
using EddiDataDefinitions;
using EddiSpeechService;
using EddiSpeechService.SpeechPreparation;
using EddiSpeechService.SpeechProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass, TestCategory("UnitTests")]
    public class SpeechProviderUnitTests : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        [TestMethod]
        public void SpeechProviderConfiguration_IsProviderAgnostic()
        {
            var providerSpecificMembers = typeof(WebSpeechProvider)
                .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Select(member => member.Name)
                .Where(name =>
                    name.Contains("Azure", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("ApiKey", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("Region", StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .ToList();

            Assert.IsEmpty(providerSpecificMembers);
        }

        [TestMethod]
        public void AzureSpeechProvider_CreateProfile_StoresAzureSettingsInGenericProviderSettings()
        {
            var provider = new AzureSpeechProvider();
            var profile = provider.CreateProfile();

            AzureSpeechProvider.SetApiKey(profile, "test-key");
            AzureSpeechProvider.SetRegion(profile, "uksouth");

            Assert.AreEqual(AzureSpeechProvider.ProviderTypeId, profile.ProviderType);
            Assert.AreEqual("Azure Speech Services", profile.DisplayName);
            Assert.AreEqual("test-key", profile.Settings["apiKey"]);
            Assert.AreEqual("uksouth", profile.Settings["region"]);
            Assert.IsTrue(provider.IsConfigured(profile));
        }

        [TestMethod]
        public void AzureSpeechProvider_Descriptor_ProvidesProviderHelpLinks()
        {
            var descriptor = new AzureSpeechProvider().Descriptor;

            Assert.AreEqual(
                "https://github.com/EDCD/EDDI/wiki/Azure-Speech-Services",
                descriptor.SetupUrl);
            Assert.AreEqual("https://portal.azure.com/", descriptor.AccountUrl);
        }

        [TestMethod]
        public void AzureSpeechProvider_GetSynthesizerCacheKey_IsStableWithoutExposingApiKey()
        {
            var profile = new WebSpeechProvider
            {
                Id = "azure-main",
                ProviderType = AzureSpeechProvider.ProviderTypeId,
                DisplayName = "Azure Speech Services"
            };
            AzureSpeechProvider.SetApiKey(profile, "test-secret-key");
            AzureSpeechProvider.SetRegion(profile, "uksouth");
            var voice = new VoiceDetails(
                "en-GB-SoniaNeural",
                "Female",
                CultureInfo.GetCultureInfo("en-GB"),
                AzureSpeechProvider.ProviderTypeId,
                providerProfileId: profile.Id,
                providerDisplayName: profile.DisplayName);

            var firstKey = AzureSpeechProvider.GetSynthesizerCacheKey(profile, voice);
            var secondKey = AzureSpeechProvider.GetSynthesizerCacheKey(profile, voice);

            Assert.AreEqual(firstKey, secondKey);
            Assert.IsFalse(firstKey.Contains("test-secret-key", StringComparison.InvariantCulture));
            Assert.IsTrue(firstKey.Contains("azure-main", StringComparison.InvariantCulture));
            Assert.IsTrue(firstKey.Contains("en-GB-SoniaNeural", StringComparison.InvariantCulture));
        }

        [TestMethod]
        public void SpeechManager_MigrateLegacyWebProviderConfigurations_MigratesLegacyAzureAdditionalData()
        {
            var configuration = ConfigService.FromJson<SpeechServiceConfiguration>(
                """
                {
                  "azureApiKey": "legacy-key",
                  "azureRegion": "uksouth"
                }
                """);
            var speechManager = new SpeechManager(new AudioManager(), [new AzureSpeechProvider()]);

            speechManager.MigrateLegacyWebProviderConfigurations(configuration);

            var profile = configuration.SpeechProviderConfigurations.Single();
            Assert.AreEqual(AzureSpeechProvider.ProviderTypeId, profile.ProviderType);
            Assert.AreEqual("legacy-key", AzureSpeechProvider.GetApiKey(profile));
            Assert.AreEqual("uksouth", AzureSpeechProvider.GetRegion(profile));
            Assert.IsFalse(configuration.HasAdditionalData("azureApiKey"));
            Assert.IsFalse(configuration.HasAdditionalData("azureRegion"));
        }

        [TestMethod]
        public void WebSpeechProviderFilters_MatchesLocalePrefixes()
        {
            var voice = new VoiceDetails(
                "en-GB-TestNeural",
                "Female",
                CultureInfo.GetCultureInfo("en-GB"),
                AzureSpeechProvider.ProviderTypeId,
                providerProfileId: "azure-main",
                providerDisplayName: "Azure Speech Services",
                supportedLocales: ["en-GB"]);

            Assert.IsTrue(WebSpeechProviderFilters.IsVoiceAllowed(voice, ["en"]));
            Assert.IsTrue(WebSpeechProviderFilters.IsVoiceAllowed(voice, ["en-GB"]));
            Assert.IsFalse(WebSpeechProviderFilters.IsVoiceAllowed(voice, ["fr"]));
        }

        [TestMethod]
        public void WebSpeechProviderFilters_AllowsMultilingualVoiceWhenRequestedLocaleCouldBeSupported()
        {
            var voice = new VoiceDetails(
                "en-US-AvaMultilingualNeural",
                "Female",
                CultureInfo.GetCultureInfo("en-US"),
                AzureSpeechProvider.ProviderTypeId,
                providerProfileId: "azure-main",
                providerDisplayName: "Azure Speech Services",
                isMultilingual: true,
                supportedLocales: ["en-US"]);

            Assert.IsTrue(WebSpeechProviderFilters.IsVoiceAllowed(voice, ["fr-FR"]));
        }

        [TestMethod]
        public void AzureSpeechProvider_PrepareAzureSsml_StripsUnsupportedLexiconTags()
        {
            var preparedSpeech = """
                <?xml version="1.0" encoding="UTF-8"?><speak version="1.0" xmlns="http://www.w3.org/2001/10/synthesis" xml:lang="en-GB"><lexicon uri="C:\Users\Test\AppData\Roaming\EDDI\lexicons\en.pls" type="application/pls+xml"/>Hello <phoneme alphabet="ipa" ph="ˈdezɦrə">Dezhra</phoneme>.</speak>
                """;
            var voice = new VoiceDetails(
                "en-GB-SoniaNeural",
                "Female",
                CultureInfo.GetCultureInfo("en-GB"),
                AzureSpeechProvider.ProviderTypeId,
                providerProfileId: "azure-test",
                providerDisplayName: "Azure Speech Services");
            var configuration = new SpeechServiceConfiguration { Volume = 80, Rate = 0 };
            var method = typeof(AzureSpeechProvider).GetMethod(
                "PrepareAzureSsml",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(method);
            var ssml = (string)method.Invoke(null, new object[] { preparedSpeech, voice, configuration });

            Assert.IsFalse(ssml.Contains("<lexicon", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsTrue(ssml.Contains("<voice name=\"en-GB-SoniaNeural\">", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsTrue(ssml.Contains("<phoneme alphabet=\"ipa\" ph=\"ˈdezɦrə\">Dezhra</phoneme>", StringComparison.InvariantCultureIgnoreCase));
        }

        [TestMethod]
        public async Task SpeechManager_LoadWebProviderVoicesAsync_LoadsAllEnabledProfiles()
        {
            var provider = new FakeWebSpeechProvider("FakeProvider");
            var configuration = new SpeechServiceConfiguration
            {
                SpeechProviderConfigurations =
                [
                    new WebSpeechProvider
                    {
                        Id = "profile-one",
                        ProviderType = "FakeProvider",
                        DisplayName = "Profile One",
                        Enabled = true
                    },
                    new WebSpeechProvider
                    {
                        Id = "profile-two",
                        ProviderType = "FakeProvider",
                        DisplayName = "Profile Two",
                        Enabled = true
                    }
                ]
            };
            var speechManager = new SpeechManager(new AudioManager(), [provider]);
            var voiceStore = new HashSet<VoiceDetails>();

            await speechManager.LoadWebProviderVoicesAsync(voiceStore, configuration, CancellationToken.None);

            Assert.HasCount(2, voiceStore);
            var expected = new[] { "FakeProvider:profile-one:profile-one-Voice", "FakeProvider:profile-two:profile-two-Voice" };
            CollectionAssert.AreEquivalent(
                expected,
                voiceStore.Select(v => v.voiceKey).ToArray());
        }

        [TestMethod]
        public async Task SpeechManager_LoadWebProviderVoicesAsync_SkipsDisabledProfiles()
        {
            var provider = new FakeWebSpeechProvider("FakeProvider");
            var configuration = new SpeechServiceConfiguration
            {
                SpeechProviderConfigurations =
                [
                    new WebSpeechProvider
                    {
                        Id = "enabled-profile",
                        ProviderType = "FakeProvider",
                        DisplayName = "Enabled Profile",
                        Enabled = true
                    },
                    new WebSpeechProvider
                    {
                        Id = "disabled-profile",
                        ProviderType = "FakeProvider",
                        DisplayName = "Disabled Profile",
                        Enabled = false
                    }
                ]
            };
            var speechManager = new SpeechManager(new AudioManager(), [provider]);
            var voiceStore = new HashSet<VoiceDetails>();

            await speechManager.LoadWebProviderVoicesAsync(voiceStore, configuration, CancellationToken.None);

            Assert.HasCount(1, voiceStore);
            Assert.AreEqual("FakeProvider:enabled-profile:enabled-profile-Voice", voiceStore.Single().voiceKey);
        }

        [TestMethod]
        public async Task SpeechManager_GetWebProviderSpeechStreamAsync_RoutesByProviderQualifiedVoiceKey()
        {
            var provider = new FakeWebSpeechProvider("FakeProvider");
            var profile = new WebSpeechProvider
            {
                Id = "route-profile",
                ProviderType = "FakeProvider",
                DisplayName = "Route Profile",
                Enabled = true
            };
            var configuration = new SpeechServiceConfiguration
            {
                SpeechProviderConfigurations = [profile]
            };
            var voice = new VoiceDetails(
                "route-profile-Voice",
                "Female",
                CultureInfo.GetCultureInfo("en-US"),
                "FakeProvider",
                providerProfileId: "route-profile",
                providerDisplayName: "Route Profile");
            var speechManager = new SpeechManager(new AudioManager(), [provider]);

            using var stream = await speechManager.GetWebProviderSpeechStreamAsync(
                voice,
                "hello",
                configuration,
                CancellationToken.None);

            Assert.IsNotNull(stream);
            Assert.AreEqual("route-profile", provider.LastSynthesisProfileId);
        }

        [TestMethod]
        public void SpeechFormatter_EnsureLexiconSchemasLoaded_IsIdempotent()
        {
            SpeechFormatter.EnsureLexiconSchemasLoaded(Assembly.GetAssembly(typeof(SpeechFormatter)));
            SpeechFormatter.EnsureLexiconSchemasLoaded(Assembly.GetAssembly(typeof(SpeechFormatter)));

            var lexiconPath = Path.Combine(Path.GetTempPath(), $"eddi-valid-{Guid.NewGuid():N}.pls");
            File.WriteAllText(
                lexiconPath,
                """
                <?xml version="1.0" encoding="UTF-8"?>
                <lexicon version="1.0"
                         xmlns="http://www.w3.org/2005/01/pronunciation-lexicon"
                         alphabet="ipa"
                         xml:lang="en">
                  <lexeme>
                    <grapheme>Dezhra</grapheme>
                    <phoneme>ˈdezɦrə</phoneme>
                  </lexeme>
                </lexicon>
                """);

            try
            {
                Assert.IsTrue(SpeechFormatter.IsValidPLS(lexiconPath));
            }
            finally
            {
                File.Delete(lexiconPath);
            }
        }

        [TestMethod]
        public void SpeechFormatter_TryQuarantineInvalidLexicon_DoesNotThrowWhenFileIsLocked()
        {
            var lexiconPath = Path.Combine(Path.GetTempPath(), $"eddi-locked-{Guid.NewGuid():N}.pls");
            File.WriteAllText(lexiconPath, "not xml");

            try
            {
                using var lockedFile = File.Open(lexiconPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                Assert.IsFalse(SpeechFormatter.TryQuarantineInvalidLexicon(new FileInfo(lexiconPath)));
                Assert.IsTrue(File.Exists(lexiconPath));
            }
            finally
            {
                File.Delete(lexiconPath);
            }
        }

        private sealed class FakeWebSpeechProvider(string providerType) : IWebSpeechProvider
        {
            public string ProviderType { get; } = providerType;
            public string DisplayName => "Fake Provider";
            public WebSpeechProviderDescriptor Descriptor => new(ProviderType, DisplayName, []);
            public string LastSynthesisProfileId { get; private set; }

            public WebSpeechProvider CreateProfile()
            {
                return new WebSpeechProvider
                {
                    Id = $"{ProviderType}-profile",
                    ProviderType = ProviderType,
                    DisplayName = DisplayName,
                    Enabled = true
                };
            }

            public void MigrateLegacyConfiguration(SpeechServiceConfiguration configuration)
            { }

            public bool IsConfigured(WebSpeechProvider profile) => true;

            public Task<IReadOnlyList<VoiceDetails>> GetVoicesAsync(
                WebSpeechProvider profile,
                CancellationToken ct)
            {
                IReadOnlyList<VoiceDetails> voices =
                [
                    new VoiceDetails(
                        $"{profile.Id}-Voice",
                        "Female",
                        CultureInfo.GetCultureInfo("en-US"),
                        ProviderType,
                        providerProfileId: profile.Id,
                        providerDisplayName: profile.DisplayName,
                        supportedLocales: ["en-US"])
                ];
                return Task.FromResult(voices);
            }

            public Task<Stream> SynthesizeAsync(
                WebSpeechProvider profile,
                VoiceDetails voice,
                string speech,
                SpeechServiceConfiguration configuration,
                CancellationToken ct)
            {
                LastSynthesisProfileId = profile.Id;
                return Task.FromResult<Stream>(new MemoryStream([1, 2, 3, 4]));
            }

            public Task ValidateAsync(WebSpeechProvider profile, CancellationToken ct)
            {
                return Task.CompletedTask;
            }
        }
    }
}
