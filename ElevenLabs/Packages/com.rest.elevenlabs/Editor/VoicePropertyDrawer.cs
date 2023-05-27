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
        private static readonly GUIContent voiceContent = new GUIContent("Voice");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            try
            {
                elevenLabsClient ??= new ElevenLabsClient();
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

            var voiceName = property.FindPropertyRelative("name");
            var id = property.FindPropertyRelative("id");

            if (voiceOptions.Length < 1)
            {
                FetchVoices();

                if (string.IsNullOrWhiteSpace(voiceName.stringValue))
                {
                    EditorGUI.HelpBox(position, "Fetching voices...", MessageType.Info);
                    return;
                }

                EditorGUI.LabelField(position, voiceContent, new GUIContent(voiceName.stringValue, id.stringValue));
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
                    if (voiceOptions[i].text.Contains(currentVoiceOption.Name))
                    {
                        voiceIndex = i;
                        break;
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            voiceIndex = EditorGUI.Popup(position, voiceContent, voiceIndex, voiceOptions);

            if (EditorGUI.EndChangeCheck())
            {
                currentVoiceOption = voices?.FirstOrDefault(voice => voiceOptions[voiceIndex].text.Contains($"{voice.Category}/{voice.Name}"));
                id.stringValue = currentVoiceOption!.Id;
                voiceName.stringValue = currentVoiceOption!.Name;
            }
        }

        private static ElevenLabsClient elevenLabsClient;

        private static bool isFetchingVoices;

        private static IReadOnlyList<Voice> voices = new List<Voice>();

        private static GUIContent[] voiceOptions = Array.Empty<GUIContent>();

        private static async void FetchVoices()
        {
            if (isFetchingVoices) { return; }
            isFetchingVoices = true;

            try
            {
                voices = await elevenLabsClient.VoicesEndpoint.GetAllVoicesAsync();
                voiceOptions = voices.OrderBy(voice => voice.Name).Select(voice => new GUIContent($"{voice.Category}/{voice.Name}")).ToArray();
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
