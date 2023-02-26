// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Voices;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace ElevenLabs.Editor
{
    public sealed class ElevenLabsEditorWindow : EditorWindow
    {
        private static readonly GUIContent guiTitleContent = new GUIContent("Eleven Labs Dashboard");

        private static readonly GUIContent voiceModelContent = new GUIContent("Voice Model");

        private static readonly GUIContent stabilityContent = new GUIContent("Stability");

        private static readonly GUIContent moreVariableContent = new GUIContent("More Variable", "Increasing variability can make speech more expressive with output varying between re-generations. It can also lead to instabilities.");

        private static readonly GUIContent moreStableContent = new GUIContent("More Stable", "Increasing stability will make the voice more consistent between re-generations, but it can also make it sounds a bit monotone. On longer text fragments we recommend lowering this value.");

        private static readonly GUIContent clarityContent = new GUIContent("Clarity + Similarity Enhancement");

        private static readonly string[] tabTitles = { "Speech Synthesis", "Voice Lab", "History" };

        private static GUIStyle boldCenteredHeaderStyle;

        private static GUIStyle BoldCenteredHeaderStyle
        {
            get
            {
                if (boldCenteredHeaderStyle == null)
                {
                    var editorStyle = EditorGUIUtility.isProSkin ? EditorStyles.whiteLargeLabel : EditorStyles.largeLabel;

                    if (editorStyle != null)
                    {
                        boldCenteredHeaderStyle = new GUIStyle(editorStyle)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            fontSize = 18,
                            padding = new RectOffset(0, 0, -8, -8)
                        };
                    }
                }

                return boldCenteredHeaderStyle;
            }
        }

        private static GUIStyle rightMiddleAlignedLabel;

        private static GUIStyle RightMiddleAlignedLabel
            => rightMiddleAlignedLabel ??= new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleRight,
        };

        private static Vector2 scrollPosition = Vector2.zero;

        private static ElevenLabsClient api;

        private static bool hasFetchedVoices;

        private static bool isFetchingVoices;

        private static bool hasFetchedHistory;

        private static bool isFetchingHistory;

        private static IReadOnlyList<Voice> voices;

        private static GUIContent[] voiceOptions = Array.Empty<GUIContent>();

        private static Voice currentVoiceOption;

        private static VoiceSettings currentVoiceSettings;

        [SerializeField]
        private int tab;

        [SerializeField]
        private Vector2 voiceSettingsSliderValues = Vector2.zero;

        private float localLabelWidth = 100;

        [MenuItem("ElevenLabs/Dashboard")]
        private static void OpenWindow()
        {
            // Dock it next to the Scene View.
            var instance = GetWindow<ElevenLabsEditorWindow>(typeof(SceneView));
            instance.Show();
            instance.titleContent = guiTitleContent;
        }

        private void OnEnable()
        {
            titleContent = guiTitleContent;
            minSize = new Vector2(512, 256);
        }

        private void OnFocus()
        {
            api ??= new ElevenLabsClient();

            if (!hasFetchedVoices)
            {
                hasFetchedVoices = true;
                FetchVoices();
            }

            if (!hasFetchedHistory)
            {
                hasFetchedHistory = true;
                FetchHistory();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Eleven Labs Dashboard", BoldCenteredHeaderStyle);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (api == null ||
                string.IsNullOrWhiteSpace(api.ElevenLabsAuthentication.ApiKey))
            {
                EditorGUILayout.HelpBox($"No valid {nameof(ElevenLabsConfigurationSettings)} was found. This tool requires that you set your API key.", MessageType.Error);
                return;
            }

            tab = GUILayout.Toolbar(tab, tabTitles);

            EditorGUILayout.Space();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true));

            switch (tab)
            {
                case 0:
                    RenderSpeechSynthesis();
                    break;
                case 1:
                    RenderVoiceLab();
                    break;
                case 2:
                    RenderHistory();
                    break;
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private async void FetchVoices()
        {
            if (isFetchingVoices) { return; }
            isFetchingVoices = true;

            try
            {
                currentVoiceSettings = await api.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
                voiceSettingsSliderValues = new Vector2(currentVoiceSettings.Stability, currentVoiceSettings.SimilarityBoost);
                voices = await api.VoicesEndpoint.GetAllVoicesAsync();
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

        private void RenderSpeechSynthesis()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUI.enabled = !isFetchingVoices;

            if (GUILayout.Button("Refresh"))
            {
                EditorApplication.delayCall += FetchVoices;
            }

            EditorGUILayout.Space(10);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
            EditorGUI.indentLevel++;

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            var prevLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = EditorGUI.indentLevel;

            var voiceIndex = -1;

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

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            voiceIndex = EditorGUILayout.Popup(voiceModelContent, voiceIndex, voiceOptions);

            if (EditorGUI.EndChangeCheck())
            {
                currentVoiceOption = voices.FirstOrDefault(voice => voiceOptions[voiceIndex].text.Contains(voice.Name));
                EditorApplication.delayCall += () => GetVoiceSettings(currentVoiceOption);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical();
            localLabelWidth = EditorGUILayout.Slider(new GUIContent("Width"), localLabelWidth, 0f, 100f);
            GUI.enabled = currentVoiceSettings != null;
            EditorGUI.BeginChangeCheck();

            voiceSettingsSliderValues.x = EditorGUILayout.Slider(stabilityContent, voiceSettingsSliderValues.x, 0f, 1f);
            EditorGUILayout.BeginHorizontal();
                //EditorGUI.indentLevel++;
                //EditorGUILayout.Space(localLabelWidth);
                EditorGUILayout.LabelField(moreVariableContent, GUILayout.Width(localLabelWidth));
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(moreStableContent, RightMiddleAlignedLabel, GUILayout.Width(localLabelWidth));
                //EditorGUI.indentLevel--;
            EditorGUILayout.EndHorizontal();

            voiceSettingsSliderValues.y = EditorGUILayout.Slider(clarityContent, voiceSettingsSliderValues.y, 0f, 1f);
            EditorGUILayout.BeginHorizontal();
                //EditorGUILayout.Space(localLabelWidth);
                EditorGUILayout.LabelField(moreVariableContent, GUILayout.Width(localLabelWidth));
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(moreStableContent, RightMiddleAlignedLabel, GUILayout.Width(localLabelWidth));
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                currentVoiceSettings = new VoiceSettings(voiceSettingsSliderValues.x, voiceSettingsSliderValues.y);
            }

            EditorGUILayout.EndVertical();

            GUI.enabled = true;

            EditorGUIUtility.labelWidth = prevLabelWidth;
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }

        private async void GetVoiceSettings(Voice voice)
        {
            try
            {
                currentVoiceSettings = await api.VoicesEndpoint.GetVoiceSettingsAsync(voice);
                voiceSettingsSliderValues = new Vector2(currentVoiceSettings.Stability, currentVoiceSettings.SimilarityBoost);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void RenderVoiceLab()
        {
        }

        private static void FetchHistory()
        {
        }

        private static void RenderHistory()
        {
        }
    }
}
