// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.TextToSpeech;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ElevenLabs.Tests
{
    internal class Test_Fixture_04_TextToSpeechEndpoint : AbstractTestFixture
    {
        [Test]
        public async Task Test_01_TextToSpeech()
        {
            Assert.NotNull(ElevenLabsClient.TextToSpeechEndpoint);
            var voice = (await ElevenLabsClient.VoicesEndpoint.GetAllVoicesAsync()).FirstOrDefault();
            Assert.NotNull(voice);
            var request = new TextToSpeechRequest(voice, "The quick brown fox jumps over the lazy dog.");
            var voiceClip = await ElevenLabsClient.TextToSpeechEndpoint.TextToSpeechAsync(request);
            Assert.NotNull(voiceClip);
            Assert.NotNull(voiceClip.AudioClip);
            Debug.Log(voiceClip.Id);
        }

        [Test]
        public async Task Test_02_StreamTextToSpeech()
        {
            Assert.NotNull(ElevenLabsClient.TextToSpeechEndpoint);
            var voice = (await ElevenLabsClient.VoicesEndpoint.GetAllVoicesAsync()).FirstOrDefault();
            Assert.NotNull(voice);
            var partialClips = new Queue<AudioClip>();
            var request = new TextToSpeechRequest(voice, "The quick brown fox jumps over the lazy dog.");
            var voiceClip = await ElevenLabsClient.TextToSpeechEndpoint.TextToSpeechAsync(request, voiceClip => partialClips.Enqueue(voiceClip));
            Assert.NotNull(partialClips);
            Assert.IsNotEmpty(partialClips);
            Assert.NotNull(voiceClip);
            Assert.IsNotNull(voiceClip.AudioClip);
            Debug.Log(voiceClip.Id);
        }

        [Test]
        public async Task Test_03_TextToSpeech_Transcription()
        {
            Assert.NotNull(ElevenLabsClient.TextToSpeechEndpoint);
            var voice = (await ElevenLabsClient.VoicesEndpoint.GetAllVoicesAsync()).FirstOrDefault();
            Assert.NotNull(voice);
            var request = new TextToSpeechRequest(voice, "The quick brown fox jumps over the lazy dog.", withTimestamps: true);
            var voiceClip = await ElevenLabsClient.TextToSpeechEndpoint.TextToSpeechAsync(request);
            Assert.NotNull(voiceClip);
            Assert.NotNull(voiceClip.AudioClip);
            Debug.Log(voiceClip.Id);
            Assert.NotNull(voiceClip.TimestampedTranscriptCharacters);
            Assert.IsNotEmpty(voiceClip.TimestampedTranscriptCharacters);
            Debug.Log("| Character | Start Time | End Time |");
            Debug.Log("| --------- | ---------- | -------- |");
            foreach (var character in voiceClip.TimestampedTranscriptCharacters)
            {
                Debug.Log($"| {character.Character} | {character.StartTime} | {character.EndTime} |");
            }
        }

        [Test]
        public async Task Test_04_StreamTextToSpeech_Transcription()
        {
            Assert.NotNull(ElevenLabsClient.TextToSpeechEndpoint);
            var voice = Voices.Voice.Adam;
            Assert.NotNull(voice);
            voice.Settings ??= await ElevenLabsClient.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
            var partialClips = new Queue<AudioClip>();
            var characters = new Queue<TimestampedTranscriptCharacter>();
            Debug.Log("| Character | Start Time | End Time |");
            Debug.Log("| --------- | ---------- | -------- |");
            var request = new TextToSpeechRequest(voice, "The quick brown fox jumps over the lazy dog.", withTimestamps: true);
            var voiceClip = await ElevenLabsClient.TextToSpeechEndpoint.TextToSpeechAsync(request, voiceClip =>
            {
                partialClips.Enqueue(voiceClip.AudioClip);
                foreach (var character in voiceClip.TimestampedTranscriptCharacters)
                {
                    characters.Enqueue(character);
                    Debug.Log($"| {character.Character} | {character.StartTime} | {character.EndTime} |");
                }
            });
            Assert.NotNull(partialClips);
            Assert.NotNull(partialClips);
            Assert.IsNotEmpty(partialClips);
            Assert.NotNull(voiceClip);
            Assert.IsNotNull(voiceClip.AudioClip);
            Debug.Log(voiceClip.Id);
            Assert.AreEqual(characters.ToArray(), voiceClip.TimestampedTranscriptCharacters);
        }

        [Test]
        public async Task Test_05_LanguageEnforced_TextToSpeech()
        {
            Assert.NotNull(ElevenLabsClient.TextToSpeechEndpoint);
            var voice = Voices.Voice.Adam;
            Assert.NotNull(voice);
            var defaultVoiceSettings = await ElevenLabsClient.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
            var request = new TextToSpeechRequest(
                voice: voice,
                text: "Příliš žluťoučký kůň úpěl ďábelské ódy",
                voiceSettings: defaultVoiceSettings,
                model: Models.Model.TurboV2_5,
                outputFormat: OutputFormat.MP3_44100_192,
                cacheFormat: CacheFormat.None,
                languageCode: "cs");
            var voiceClip = await ElevenLabsClient.TextToSpeechEndpoint.TextToSpeechAsync(request);
            Assert.NotNull(voiceClip);
            Assert.NotNull(voiceClip.AudioClip);
            Debug.Log(voiceClip.Id);
        }
    }
}
