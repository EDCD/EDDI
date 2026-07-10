using Amazon.Polly.Model;
using AwsEngine = Amazon.Polly.Engine;
using AwsGender = Amazon.Polly.Gender;
using AwsLanguageCode = Amazon.Polly.LanguageCode;
using AwsVoiceId = Amazon.Polly.VoiceId;
using EddiConfigService;
using EddiConfigService.Configurations;
using EddiDataDefinitions;
using EddiSpeechService;
using EddiSpeechService.SpeechPreparation;
using EddiSpeechService.SpeechProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
        public void AmazonPollySpeechProvider_CreateProfile_StoresAmazonSettingsInGenericProviderSettings()
        {
            var provider = new AmazonPollySpeechProvider();
            var profile = provider.CreateProfile();

            AmazonPollySpeechProvider.SetAccessKeyId(profile, "test-access-key");
            AmazonPollySpeechProvider.SetSecretAccessKey(profile, "test-secret-key");
            AmazonPollySpeechProvider.SetRegion(profile, "us-east-1");

            Assert.AreEqual(AmazonPollySpeechProvider.ProviderTypeId, profile.ProviderType);
            Assert.AreEqual("Amazon Polly", profile.DisplayName);
            Assert.AreEqual("test-access-key", profile.Settings["accessKeyId"]);
            Assert.AreEqual("test-secret-key", profile.Settings["secretAccessKey"]);
            Assert.AreEqual("us-east-1", profile.Settings["region"]);
            Assert.IsTrue(provider.IsConfigured(profile));
        }

        [TestMethod]
        public void AmazonPollySpeechProvider_IsConfigured_RequiresRegionAndAccessKeyPair()
        {
            var provider = new AmazonPollySpeechProvider();
            var profile = provider.CreateProfile();

            Assert.IsFalse(provider.IsConfigured(profile));

            AmazonPollySpeechProvider.SetRegion(profile, "us-east-1");
            Assert.IsFalse(provider.IsConfigured(profile));

            AmazonPollySpeechProvider.SetAccessKeyId(profile, "test-access-key");
            Assert.IsFalse(provider.IsConfigured(profile));

            AmazonPollySpeechProvider.SetSecretAccessKey(profile, "test-secret-key");
            Assert.IsTrue(provider.IsConfigured(profile));
        }

        [TestMethod]
        public void AmazonPollySpeechProvider_Descriptor_ProvidesProviderHelpLinks()
        {
            var descriptor = new AmazonPollySpeechProvider().Descriptor;

            Assert.AreEqual(
                "https://github.com/EDCD/EDDI/wiki/Amazon-Polly",
                descriptor.SetupUrl);
            Assert.AreEqual("https://console.aws.amazon.com/", descriptor.AccountUrl);
            CollectionAssert.AreEqual(
                new[] { "region", "accessKeyId", "secretAccessKey" },
                descriptor.ProfileFields.Select(field => field.Key).ToArray());
        }

        [TestMethod]
        public void SpeechManager_DefaultWebProviderDescriptors_IncludesAzureAndAmazonPolly()
        {
            var speechManager = new SpeechManager(new AudioManager());

            CollectionAssert.AreEquivalent(
                new[] { AzureSpeechProvider.ProviderTypeId, AmazonPollySpeechProvider.ProviderTypeId },
                speechManager.WebProviderDescriptors.Select(descriptor => descriptor.ProviderType).ToArray());
        }

        [TestMethod]
        public async Task AmazonPollySpeechProvider_GetVoicesAsync_MapsVoicesAndAppliesLocaleFilters()
        {
            var client = new FakeAmazonPollyClient
            {
                Voices =
                [
                    new AmazonPollyVoice("Joanna", "Joanna", "Female", "en-US", AmazonPollySpeechProvider.NeuralEngine),
                    new AmazonPollyVoice("Lea", "Lea", "Female", "fr-FR", AmazonPollySpeechProvider.StandardEngine)
                ]
            };
            var provider = new AmazonPollySpeechProvider(_ => client);
            var profile = CreateConfiguredAmazonPollyProfile(localeFilters: ["en"]);

            var voices = await provider.GetVoicesAsync(profile, CancellationToken.None);

            Assert.HasCount(1, voices);
            var voice = voices.Single();
            Assert.AreEqual("Joanna (Neural)", voice.name);
            Assert.AreEqual("Female", voice.gender);
            Assert.AreEqual("en-US", voice.culturecode);
            Assert.AreEqual(AmazonPollySpeechProvider.ProviderTypeId, voice.synthType);
            Assert.AreEqual(profile.Id, voice.providerProfileId);
            Assert.AreEqual(profile.DisplayName, voice.providerDisplayName);
            Assert.AreEqual("AmazonPolly:amazon-polly-main:Joanna:neural", voice.voiceKey);
            Assert.AreEqual("English (United States) Joanna (Neural) [Amazon Polly]", voice.friendlyName);
            var expected = new[] { "en-US" };
            CollectionAssert.AreEqual(expected, voice.supportedLocales.ToArray());
        }

        [TestMethod]
        public void AmazonPollySdkClient_CreateVoiceVariants_ExpandsSupportedEngines()
        {
            var voice = new Voice
            {
                Id = AwsVoiceId.Joanna,
                Name = "Joanna",
                Gender = AwsGender.Female,
                LanguageCode = AwsLanguageCode.EnUS,
                SupportedEngines = [ AwsEngine.Standard, AwsEngine.Neural ]
            };

            var voices = AmazonPollySdkClient.CreateVoiceVariants( voice, AwsEngine.Standard ).ToList();

            Assert.HasCount(2, voices);
            CollectionAssert.AreEquivalent(
                new[] { AmazonPollySpeechProvider.StandardEngine, AmazonPollySpeechProvider.NeuralEngine },
                voices.Select( v => v.Engine ).ToArray());
            CollectionAssert.AreEquivalent(
                new[]
                {
                    "Joanna:standard",
                    "Joanna:neural"
                },
                voices.Select( v => $"{v.VoiceId}:{v.Engine}" ).ToArray());
        }

        [TestMethod]
        public async Task AmazonPollySdkClient_DescribeVoicesByEngineAsync_ReturnsStandardVoicesWhenNeuralLookupFails()
        {
            var voices = await AmazonPollySdkClient.DescribeVoicesByEngineAsync(
                [AwsEngine.Standard, AwsEngine.Neural],
                (engine, _, _) =>
                {
                    if (engine == AwsEngine.Neural)
                    {
                        throw new InvalidOperationException("Neural voices are unavailable in this test region.");
                    }

                    return Task.FromResult(new DescribeVoicesResponse
                    {
                        Voices =
                        [
                            new Voice
                            {
                                Id = AwsVoiceId.Salli,
                                Name = "Salli",
                                Gender = AwsGender.Female,
                                LanguageCode = AwsLanguageCode.EnUS,
                                SupportedEngines = [ AwsEngine.Standard ]
                            }
                        ]
                    });
                },
                "test-region",
                CancellationToken.None);

            Assert.HasCount(1, voices);
            var voice = voices.Single();
            Assert.AreEqual("Salli", voice.VoiceId);
            Assert.AreEqual(AmazonPollySpeechProvider.StandardEngine, voice.Engine);
        }

        [TestMethod]
        public async Task AmazonPollySpeechProvider_GetVoicesAsync_RepresentsBilingualVoicesAsSingleMultilingualVoice()
        {
            var client = new FakeAmazonPollyClient
            {
                Voices =
                [
                    new AmazonPollyVoice(
                        "Aditi",
                        "Aditi",
                        "Female",
                        "en-IN",
                        AmazonPollySpeechProvider.StandardEngine,
                        ["hi-IN"])
                ]
            };
            var provider = new AmazonPollySpeechProvider(_ => client);
            var profile = CreateConfiguredAmazonPollyProfile();

            var voices = await provider.GetVoicesAsync(profile, CancellationToken.None);

            Assert.HasCount(1, voices);
            var voice = voices.Single();
            Assert.AreEqual("Aditi (Standard)", voice.name);
            Assert.AreEqual("en-IN", voice.culturecode);
            Assert.IsTrue(voice.isMultilingual);
            Assert.AreEqual("AmazonPolly:amazon-polly-main:Aditi:standard", voice.voiceKey);
            CollectionAssert.AreEqual(new[] { "en-IN", "hi-IN" }, voice.supportedLocales.ToArray());
        }

        [TestMethod]
        public async Task AmazonPollySpeechProvider_GetVoicesAsync_FiltersBilingualVoiceByExplicitAdditionalLanguages()
        {
            var client = new FakeAmazonPollyClient
            {
                Voices =
                [
                    new AmazonPollyVoice(
                        "Aditi",
                        "Aditi",
                        "Female",
                        "en-IN",
                        AmazonPollySpeechProvider.StandardEngine,
                        ["hi-IN"])
                ]
            };
            var provider = new AmazonPollySpeechProvider(_ => client);
            var profile = CreateConfiguredAmazonPollyProfile(localeFilters: ["hi-IN"]);

            var voices = await provider.GetVoicesAsync(profile, CancellationToken.None);

            Assert.HasCount(1, voices);
        }

        [TestMethod]
        public async Task AmazonPollySpeechProvider_GetVoicesAsync_DoesNotMatchUnsupportedLanguageForBilingualVoice()
        {
            var client = new FakeAmazonPollyClient
            {
                Voices =
                [
                    new AmazonPollyVoice(
                        "Aditi",
                        "Aditi",
                        "Female",
                        "en-IN",
                        AmazonPollySpeechProvider.StandardEngine,
                        ["hi-IN"])
                ]
            };
            var provider = new AmazonPollySpeechProvider(_ => client);
            var profile = CreateConfiguredAmazonPollyProfile(localeFilters: ["fr-FR"]);

            var voices = await provider.GetVoicesAsync(profile, CancellationToken.None);

            Assert.HasCount(0, voices);
        }

        [TestMethod]
        public async Task AmazonPollySpeechProvider_SynthesizeAsync_RequestsMp3AndDecodesAudioAsWave()
        {
            var client = new FakeAmazonPollyClient
            {
                SynthesisStream = CreateMp3SineStream(24000, TimeSpan.FromSeconds(1))
            };
            var provider = new AmazonPollySpeechProvider(
                _ => client,
                tempoStretchFactor: 1.0);
            var profile = CreateConfiguredAmazonPollyProfile();
            var voice = new VoiceDetails(
                "Joanna (Neural)",
                "Female",
                CultureInfo.GetCultureInfo("en-US"),
                AmazonPollySpeechProvider.ProviderTypeId,
                providerProfileId: profile.Id,
                providerDisplayName: profile.DisplayName,
                supportedLocales: ["en-US"],
                providerVoiceId: "Joanna:neural");
            var configuration = new SpeechServiceConfiguration { Volume = 80, Rate = 0 };

            using var stream = await provider.SynthesizeAsync(
                profile,
                voice,
                "hello",
                configuration,
                CancellationToken.None);

            Assert.IsNotNull(client.LastSynthesisRequest);
            Assert.AreEqual("Joanna", client.LastSynthesisRequest.VoiceId);
            Assert.AreEqual(AmazonPollySpeechProvider.NeuralEngine, client.LastSynthesisRequest.Engine);
            Assert.Contains( "hello", client.LastSynthesisRequest.Text);
            Assert.AreEqual("ssml", client.LastSynthesisRequest.TextType);
            Assert.AreEqual(AmazonPollySpeechProvider.Mp3OutputFormat, client.LastSynthesisRequest.OutputFormat);
            Assert.AreEqual("24000", client.LastSynthesisRequest.SampleRate);
            Assert.AreEqual("en-US", client.LastSynthesisRequest.LanguageCode);

            using var reader = new WaveFileReader(stream);
            Assert.AreEqual(24000, reader.WaveFormat.SampleRate);
            Assert.AreEqual(16, reader.WaveFormat.BitsPerSample);
            Assert.AreEqual(1, reader.WaveFormat.Channels);
            Assert.IsTrue(
                reader.TotalTime > TimeSpan.FromMilliseconds(900) &&
                reader.TotalTime < TimeSpan.FromMilliseconds(1100),
                $"Expected approximately one second of Polly audio but got {reader.TotalTime.TotalMilliseconds} ms.");
        }

        [TestMethod]
        public async Task AmazonPollySpeechProvider_SynthesizeAsync_AppliesBaselineTempoStretchAfterDecode()
        {
            var client = new FakeAmazonPollyClient
            {
                SynthesisStream = new MemoryStream([1, 2, 3])
            };
            var provider = new AmazonPollySpeechProvider(
                _ => client,
                _ => CreateSineWaveStream(24000, TimeSpan.FromSeconds(1)),
                tempoStretchFactor: 2.0);
            var profile = CreateConfiguredAmazonPollyProfile();
            var voice = new VoiceDetails(
                "Amy (Standard)",
                "Female",
                CultureInfo.GetCultureInfo("en-GB"),
                AmazonPollySpeechProvider.ProviderTypeId,
                providerProfileId: profile.Id,
                providerDisplayName: profile.DisplayName,
                supportedLocales: ["en-GB"],
                providerVoiceId: "Amy:standard");

            using var stream = await provider.SynthesizeAsync(
                profile,
                voice,
                "hello",
                new SpeechServiceConfiguration { Volume = 80, Rate = 0 },
                CancellationToken.None);

            using var reader = new WaveFileReader(stream);
            Assert.AreEqual(24000, reader.WaveFormat.SampleRate);
            Assert.AreEqual(16, reader.WaveFormat.BitsPerSample);
            Assert.AreEqual(1, reader.WaveFormat.Channels);
            Assert.IsTrue(
                reader.TotalTime > TimeSpan.FromMilliseconds(1800) &&
                reader.TotalTime < TimeSpan.FromMilliseconds(2300),
                $"Expected approximately two seconds of tempo-stretched Polly audio but got {reader.TotalTime.TotalMilliseconds} ms.");
        }

        [TestMethod]
        public async Task AmazonPollySpeechProvider_SynthesizeAsync_AppliesConfiguredRateAndVolume()
        {
            var client = new FakeAmazonPollyClient
            {
                SynthesisStream = new MemoryStream([0, 0, 255, 127])
            };
            var provider = CreateAmazonPollyProvider(client);
            var profile = CreateConfiguredAmazonPollyProfile();
            var voice = new VoiceDetails(
                "Joanna (Neural)",
                "Female",
                CultureInfo.GetCultureInfo("en-US"),
                AmazonPollySpeechProvider.ProviderTypeId,
                providerProfileId: profile.Id,
                providerDisplayName: profile.DisplayName,
                supportedLocales: ["en-US"]);
            var configuration = new SpeechServiceConfiguration { Volume = 50, Rate = 5 };

            using var stream = await provider.SynthesizeAsync(
                profile,
                voice,
                "hello",
                configuration,
                CancellationToken.None);

            Assert.IsNotNull(stream);
            Assert.IsNotNull(client.LastSynthesisRequest);
            Assert.AreEqual( "ssml", client.LastSynthesisRequest.TextType);
            Assert.Contains( "<prosody volume=\"-6dB\" rate=\"125%\">", client.LastSynthesisRequest.Text);
            Assert.Contains( "hello", client.LastSynthesisRequest.Text);
        }

        [TestMethod]
        public async Task AmazonPollySpeechProvider_SynthesizeAsync_UsesNoOpPollyRateForNeutralRate()
        {
            var client = new FakeAmazonPollyClient
            {
                SynthesisStream = new MemoryStream([0, 0, 255, 127])
            };
            var provider = CreateAmazonPollyProvider(client);
            var profile = CreateConfiguredAmazonPollyProfile();
            var voice = new VoiceDetails(
                "Amy (Standard)",
                "Female",
                CultureInfo.GetCultureInfo("en-GB"),
                AmazonPollySpeechProvider.ProviderTypeId,
                providerProfileId: profile.Id,
                providerDisplayName: profile.DisplayName,
                supportedLocales: ["en-GB"]);
            var configuration = new SpeechServiceConfiguration { Volume = 85, Rate = 0 };

            using var stream = await provider.SynthesizeAsync(
                profile,
                voice,
                "hello",
                configuration,
                CancellationToken.None);

            Assert.IsNotNull(stream);
            Assert.IsNotNull(client.LastSynthesisRequest);
            Assert.Contains( "rate=\"100%\"", client.LastSynthesisRequest.Text);
        }

        [TestMethod]
        public async Task AmazonPollySpeechProvider_SynthesizeAsync_UsesGentleMaximumRate()
        {
            var client = new FakeAmazonPollyClient
            {
                SynthesisStream = new MemoryStream([0, 0, 255, 127])
            };
            var provider = CreateAmazonPollyProvider(client);
            var profile = CreateConfiguredAmazonPollyProfile();
            var voice = new VoiceDetails(
                "Joanna (Neural)",
                "Female",
                CultureInfo.GetCultureInfo("en-US"),
                AmazonPollySpeechProvider.ProviderTypeId,
                providerProfileId: profile.Id,
                providerDisplayName: profile.DisplayName,
                supportedLocales: ["en-US"]);
            var configuration = new SpeechServiceConfiguration { Volume = 80, Rate = 10 };

            using var stream = await provider.SynthesizeAsync(
                profile,
                voice,
                "hello",
                configuration,
                CancellationToken.None);

            Assert.IsNotNull(stream);
            Assert.IsNotNull(client.LastSynthesisRequest);
            Assert.Contains( "<prosody volume=\"-1.9dB\" rate=\"150%\">", client.LastSynthesisRequest.Text);
        }

        [TestMethod]
        public async Task AmazonPollySpeechProvider_SynthesizeAsync_UsesProfileLocaleFilterForBilingualVoiceLanguage()
        {
            var client = new FakeAmazonPollyClient
            {
                SynthesisStream = new MemoryStream([0, 0, 255, 127])
            };
            var provider = CreateAmazonPollyProvider(client);
            var profile = CreateConfiguredAmazonPollyProfile(localeFilters: ["hi-IN"]);
            var voice = new VoiceDetails(
                "Aditi (Standard)",
                "Female",
                CultureInfo.GetCultureInfo("en-IN"),
                AmazonPollySpeechProvider.ProviderTypeId,
                providerProfileId: profile.Id,
                providerDisplayName: profile.DisplayName,
                isMultilingual: true,
                supportedLocales: ["en-IN", "hi-IN"],
                providerVoiceId: "Aditi:standard");
            var configuration = new SpeechServiceConfiguration { Volume = 80, Rate = 0 };

            using var stream = await provider.SynthesizeAsync(
                profile,
                voice,
                "namaste",
                configuration,
                CancellationToken.None);

            Assert.IsNotNull(stream);
            Assert.IsNotNull(client.LastSynthesisRequest);
            Assert.AreEqual("hi-IN", client.LastSynthesisRequest.LanguageCode);
            Assert.Contains("xml:lang=\"hi-IN\"", client.LastSynthesisRequest.Text);
        }

        [TestMethod]
        public async Task AmazonPollySpeechProvider_SynthesizeAsync_PrefersFirstMatchingProfileLocaleFilter()
        {
            var client = new FakeAmazonPollyClient
            {
                SynthesisStream = new MemoryStream([0, 0, 255, 127])
            };
            var provider = CreateAmazonPollyProvider(client);
            var profile = CreateConfiguredAmazonPollyProfile(localeFilters: ["hi-IN", "en"]);
            var voice = new VoiceDetails(
                "Aditi (Standard)",
                "Female",
                CultureInfo.GetCultureInfo("en-IN"),
                AmazonPollySpeechProvider.ProviderTypeId,
                providerProfileId: profile.Id,
                providerDisplayName: profile.DisplayName,
                isMultilingual: true,
                supportedLocales: ["en-IN", "hi-IN"],
                providerVoiceId: "Aditi:standard");
            var configuration = new SpeechServiceConfiguration { Volume = 80, Rate = 0 };

            using var stream = await provider.SynthesizeAsync(
                profile,
                voice,
                "namaste",
                configuration,
                CancellationToken.None);

            Assert.IsNotNull(stream);
            Assert.IsNotNull(client.LastSynthesisRequest);
            Assert.AreEqual("hi-IN", client.LastSynthesisRequest.LanguageCode);
        }

        [TestMethod]
        public async Task AmazonPollySpeechProvider_SynthesizeAsync_UsesSilentVolumeAtZero()
        {
            var client = new FakeAmazonPollyClient
            {
                SynthesisStream = new MemoryStream([0, 0, 255, 127])
            };
            var provider = CreateAmazonPollyProvider(client);
            var profile = CreateConfiguredAmazonPollyProfile();
            var voice = new VoiceDetails(
                "Joanna (Neural)",
                "Female",
                CultureInfo.GetCultureInfo("en-US"),
                AmazonPollySpeechProvider.ProviderTypeId,
                providerProfileId: profile.Id,
                providerDisplayName: profile.DisplayName,
                supportedLocales: ["en-US"]);
            var configuration = new SpeechServiceConfiguration { Volume = 0, Rate = -10 };

            using var stream = await provider.SynthesizeAsync(
                profile,
                voice,
                "hello",
                configuration,
                CancellationToken.None);

            Assert.IsNotNull(stream);
            Assert.IsNotNull(client.LastSynthesisRequest);
            Assert.Contains( "<prosody volume=\"silent\" rate=\"50%\">", client.LastSynthesisRequest.Text);
        }

        [TestMethod]
        public void SpeechFx_AddEffectsToSource_PreservesWebProviderWaveDuration()
        {
            using var webProviderStream = CreateSilenceWaveStream(24000, TimeSpan.FromSeconds(1));

            var provider = SpeechFx.addEffectsToSource(
                webProviderStream,
                fxLevel: 0,
                distortionLevel: 0,
                echoDelay: 0,
                radio: true);
            var outputLength = ReadAllBytes(provider);
            var duration = TimeSpan.FromSeconds(
                outputLength.Length / (double)provider.WaveFormat.AverageBytesPerSecond);

            Assert.AreEqual(44100, provider.WaveFormat.SampleRate);
            Assert.AreEqual(1, provider.WaveFormat.Channels);
            Assert.AreEqual(16, provider.WaveFormat.BitsPerSample);
            Assert.IsTrue(
                duration > TimeSpan.FromMilliseconds(900) &&
                duration < TimeSpan.FromMilliseconds(1100),
                $"Expected approximately one second of audio after resampling but got {duration.TotalMilliseconds} ms.");
        }

        [TestMethod]
        public async Task AmazonPollySpeechProvider_ValidateAsync_ThrowsWhenNoVoicesReturned()
        {
            var provider = new AmazonPollySpeechProvider(_ => new FakeAmazonPollyClient());
            var profile = CreateConfiguredAmazonPollyProfile();

            await Assert.ThrowsExactlyAsync<InvalidOperationException>(
                () => provider.ValidateAsync(profile, CancellationToken.None));
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

        private static WebSpeechProvider CreateConfiguredAmazonPollyProfile(List<string> localeFilters = null)
        {
            var profile = new WebSpeechProvider
            {
                Id = "amazon-polly-main",
                ProviderType = AmazonPollySpeechProvider.ProviderTypeId,
                DisplayName = "Amazon Polly",
                LocaleFilters = localeFilters ?? []
            };
            AmazonPollySpeechProvider.SetAccessKeyId(profile, "test-access-key");
            AmazonPollySpeechProvider.SetSecretAccessKey(profile, "test-secret-key");
            AmazonPollySpeechProvider.SetRegion(profile, "us-east-1");
            return profile;
        }

        private static AmazonPollySpeechProvider CreateAmazonPollyProvider(FakeAmazonPollyClient client)
        {
            return new AmazonPollySpeechProvider(
                _ => client,
                _ => CreateSilenceWaveStream(24000, TimeSpan.FromSeconds(1)),
                tempoStretchFactor: 1.0);
        }

        private static MemoryStream CreateSilenceWaveStream(int sampleRate, TimeSpan duration)
        {
            const short channels = 1;
            const short bitsPerSample = 16;
            var bytes = new byte[(int)(sampleRate * duration.TotalSeconds) * channels * bitsPerSample / 8];
            return CreatePcmWaveStream(sampleRate, channels, bitsPerSample, bytes);
        }

        private static MemoryStream CreateSineWaveStream(int sampleRate, TimeSpan duration)
        {
            const short channels = 1;
            const short bitsPerSample = 16;
            var frames = (int)(sampleRate * duration.TotalSeconds);
            var bytes = new byte[frames * channels * bitsPerSample / 8];
            for (var frame = 0; frame < frames; frame++)
            {
                var sample = (short)(Math.Sin(2 * Math.PI * 440 * frame / sampleRate) * short.MaxValue * 0.25);
                BitConverter.GetBytes(sample).CopyTo(bytes, frame * sizeof(short));
            }

            return CreatePcmWaveStream(sampleRate, channels, bitsPerSample, bytes);
        }

        private static MemoryStream CreatePcmWaveStream(int sampleRate, short channels, short bitsPerSample, byte[] bytes)
        {
            var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                var byteRate = sampleRate * channels * bitsPerSample / 8;
                var blockAlign = (short)(channels * bitsPerSample / 8);
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + bytes.Length);
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);
                writer.Write((short)1);
                writer.Write(channels);
                writer.Write(sampleRate);
                writer.Write(byteRate);
                writer.Write(blockAlign);
                writer.Write(bitsPerSample);
                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(bytes.Length);
                writer.Write(bytes);
            }

            stream.Position = 0;
            return stream;
        }

        private static MemoryStream CreateMp3SineStream(int sampleRate, TimeSpan duration)
        {
            var file = Path.Combine(Path.GetTempPath(), $"eddi-polly-decode-{Guid.NewGuid():N}.mp3");
            try
            {
                using (var source = CreateSineWaveStream(sampleRate, duration))
                using (var reader = new WaveFileReader(source))
                {
                    MediaFoundationEncoder.EncodeToMp3(reader, file, 48000);
                }

                return new MemoryStream(File.ReadAllBytes(file));
            }
            finally
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }

        private static byte[] ReadAllBytes(IWaveProvider provider)
        {
            var buffer = new byte[provider.WaveFormat.AverageBytesPerSecond];
            using var stream = new MemoryStream();
            int read;
            while ((read = provider.Read(buffer, 0, buffer.Length)) > 0)
            {
                stream.Write(buffer, 0, read);
            }

            return stream.ToArray();
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

            Assert.IsFalse(WebSpeechProviderFilters.IsVoiceAllowed(voice, ["fr-FR"]));
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
        public void SpeechFormatter_PrepareSpeech_TreatsAmazonPollySynthTypeAsPollyVoice()
        {
            var voice = new VoiceDetails(
                "Joanna (Neural)",
                "Female",
                CultureInfo.GetCultureInfo("en-US"),
                AmazonPollySpeechProvider.ProviderTypeId,
                providerProfileId: "amazon-polly-main",
                providerDisplayName: "Amazon Polly",
                supportedLocales: ["en-US"]);
            var speech = "hello <break time=\"100ms\"/>";

            SpeechFormatter.PrepareSpeech(voice, ref speech, out var useSSML);

            Assert.IsFalse(useSSML);
            Assert.Contains("<speak", speech);
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

        private sealed class FakeAmazonPollyClient : IAmazonPollyClient
        {
            public IReadOnlyList<AmazonPollyVoice> Voices { get; init; } = [];
            public Stream SynthesisStream { get; init; } = new MemoryStream();
            public AmazonPollySynthesisRequest LastSynthesisRequest { get; private set; }

            public Task<IReadOnlyList<AmazonPollyVoice>> DescribeVoicesAsync(CancellationToken ct)
            {
                return Task.FromResult(Voices);
            }

            public Task<Stream> SynthesizeSpeechAsync(AmazonPollySynthesisRequest request, CancellationToken ct)
            {
                LastSynthesisRequest = request;
                return Task.FromResult(SynthesisStream);
            }

            public void Dispose()
            { }
        }
    }
}
