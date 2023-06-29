// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Voices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using UnityEditor;
using UnityEngine;

namespace ElevenLabs.Editor
{
    [CustomPropertyDrawer(typeof(Voice))]
    public class VoicePropertyDrawer : PropertyDrawer
    {
        private static ElevenLabsClient elevenLabsClient;

        private static ElevenLabsClient ElevenLabsClient => elevenLabsClient ??= new ElevenLabsClient();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            try
            {
                if (!ElevenLabsClient.HasValidAuthentication)
                {
                    EditorGUI.LabelField(position, "Cannot fetch voices");
                    return;
                }
            }
            catch (AuthenticationException)
            {
                EditorGUI.HelpBox(position, "Check elevenlabs api key", MessageType.Error);

                return;
            }
            catch (Exception e)
            {
                Debug.LogError(e);

                return;
            }

            var id = property.FindPropertyRelative("id");
            var voiceName = property.FindPropertyRelative("name");

            if (voiceOptions.Length < 1)
            {
                FetchVoices();

                if (string.IsNullOrWhiteSpace(id.stringValue))
                {
                    EditorGUI.HelpBox(position, "Fetching voices...", MessageType.Info);
                    return;
                }

                EditorGUI.LabelField(position, label, new GUIContent(voiceName.stringValue, id.stringValue));
                return;
            }

            // voice dropdown
            var voiceIndex = -1;
            Voice currentVoiceOption = null;

            if (!string.IsNullOrWhiteSpace(id.stringValue))
            {
                currentVoiceOption = voices?.FirstOrDefault(voice => voice.Id == id.stringValue);
            }

            if (currentVoiceOption != null)
            {
                for (var i = 0; i < voiceOptions.Length; i++)
                {
                    if (voiceOptions[i].tooltip.Contains(currentVoiceOption.Id))
                    {
                        voiceIndex = i;
                        break;
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            voiceIndex = EditorGUI.Popup(position, label, voiceIndex, voiceOptions);

            if (EditorGUI.EndChangeCheck())
            {
                currentVoiceOption = voices?.FirstOrDefault(voice => voiceOptions[voiceIndex].text.Contains($"{voice.Category}/{voice.Name}"));
                id.stringValue = currentVoiceOption!.Id;
                voiceName.stringValue = currentVoiceOption!.Name;
            }
        }

        private static bool isFetchingVoices;

        public static bool IsFetchingVoices => isFetchingVoices;

        private static IReadOnlyList<Voice> voices = new List<Voice>();

        public static IReadOnlyList<Voice> Voices => voices;

        private static GUIContent[] voiceOptions = Array.Empty<GUIContent>();

        public static IReadOnlyList<GUIContent> VoiceOptions => voiceOptions;

        public static async void FetchVoices()
        {
            if (isFetchingVoices) { return; }
            isFetchingVoices = true;

            try
            {
                voices = await ElevenLabsClient.VoicesEndpoint.GetAllVoicesAsync();
                voiceOptions = voices.Select(voice => new GUIContent($"{voice.Category}/{voice.Name}", voice.Id)).ToArray();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                isFetchingVoices = false;
            }
        }
    }
}
