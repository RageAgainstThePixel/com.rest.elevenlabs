// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Utilities.Rest.Editor;

namespace ElevenLabs.Editor
{
    [CustomEditor(typeof(ElevenLabsConfiguration))]
    internal class ElevenLabsConfigurationInspector : BaseConfigurationInspector<ElevenLabsConfiguration>
    {
        private static bool triggerReload;

        private SerializedProperty apiKey;
        private SerializedProperty proxyDomain;
        private SerializedProperty globalVoice;

        #region Project Settings Window

        [SettingsProvider]
        private static SettingsProvider Preferences()
            => GetSettingsProvider(nameof(ElevenLabs), CheckReload);

        #endregion Project Settings Window

        #region Inspector Window

        private void OnEnable()
        {
            GetOrCreateInstance(target);

            try
            {
                apiKey = serializedObject.FindProperty(nameof(apiKey));
                proxyDomain = serializedObject.FindProperty(nameof(proxyDomain));
                globalVoice = serializedObject.FindProperty(nameof(globalVoice));
            }
            catch (Exception)
            {
                // Ignored
            }
        }

        private void OnDisable() => CheckReload();

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.Space();
            EditorGUI.indentLevel++;

            if (apiKey == null)
            {
                OnEnable();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(apiKey);
            EditorGUILayout.PropertyField(proxyDomain);

            if (EditorGUI.EndChangeCheck())
            {
                triggerReload = true;
            }

            if (VoicePropertyDrawer.VoiceOptions == null ||
                VoicePropertyDrawer.VoiceOptions.Count == 0 ||
                VoicePropertyDrawer.Voices == null ||
                VoicePropertyDrawer.Voices.Count == 0)
            {
                if (!VoicePropertyDrawer.IsFetchingVoices)
                {
                    VoicePropertyDrawer.FetchVoices();
                }
                else
                {
                    GUILayout.Label("Fetching voices...");
                }
            }
            else
            {
                if (!VoicePropertyDrawer.IsFetchingVoices)
                {
                    if (GUILayout.Button("Fetch Voices"))
                    {
                        VoicePropertyDrawer.FetchVoices();
                    }
                }
                else
                {
                    GUILayout.Label("Fetching voices...");
                }

                var globalVoiceId = globalVoice.FindPropertyRelative("id");
                var globalVoiceName = globalVoice.FindPropertyRelative("name");
                var cachedVoiceId = globalVoiceId.stringValue;
                var globalVoiceSelectionId = EditorPrefs.GetString($"AUDIO_CLIP_RECORDING_{Application.companyName}_{Application.productName}", cachedVoiceId);
                var selection = 0;

                foreach (var voice in VoicePropertyDrawer.Voices)
                {
                    if (voice.Id.Equals(globalVoiceSelectionId))
                    {
                        //Debug.Log($"{selection} => {voice.Id}::{voice.Name} || {globalVoiceSelectionId}");
                        break;
                    }

                    selection++;
                }

                EditorGUI.BeginChangeCheck();
                selection = EditorGUILayout.Popup(new GUIContent("Global Voice"), selection, VoicePropertyDrawer.VoiceOptions.ToArray());

                if (EditorGUI.EndChangeCheck())
                {
                    var selectedGlobalVoice = VoicePropertyDrawer.Voices[selection];
                    Assert.IsNotNull(selectedGlobalVoice);
                    EditorPrefs.SetString($"AUDIO_CLIP_RECORDING_{Application.companyName}_{Application.productName}", selectedGlobalVoice.Id);
                    //Debug.Log($"{selection} <= {selectedGlobalVoice.Id}::{selectedGlobalVoice.Name}");
                    globalVoiceId.stringValue = selectedGlobalVoice.Id;
                    globalVoiceName.stringValue = selectedGlobalVoice.Name;
                }
            }

            EditorGUI.indentLevel--;
            serializedObject.ApplyModifiedProperties();
        }

        #endregion Inspector Window

        private static void CheckReload()
        {
            if (triggerReload)
            {
                triggerReload = false;
                EditorUtility.RequestScriptReload();
            }
        }
    }
}
